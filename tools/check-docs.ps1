<#
.SYNOPSIS
  Checks documentation invariants of the knowledge base.

.DESCRIPTION
  - every *.ru.md has a *.en.md pair and vice versa;
  - a ru/en pair has the same structure (heading counts per level, code blocks, <details> blocks);
  - documents inside tracks/ have front matter whose `lang` matches the file suffix;
  - relative links point to existing files. Roadmaps (track overviews, root READMEs, PROGRESS.md)
    may link to materials that are not created yet; such links are reported as "planned";
  - the `levels` list in a material's overview matches the level files and code/ folders on disk.

.EXAMPLE
  powershell -NoProfile -ExecutionPolicy Bypass -File tools/check-docs.ps1
.EXAMPLE
  powershell -NoProfile -ExecutionPolicy Bypass -File tools/check-docs.ps1 tracks/python/modules/14-asyncio
#>
param([string]$Path)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$scope = if ($Path) { (Resolve-Path $Path).Path } else { $repoRoot }

$problems = New-Object System.Collections.Generic.List[string]
$planned = New-Object System.Collections.Generic.List[string]

function Get-RelPath([string]$full) {
    if ($full.StartsWith($repoRoot)) {
        $rel = $full.Substring($repoRoot.Length).TrimStart('\', '/')
        if ($rel) { return $rel } else { return '.' }
    }
    return $full
}

# Splits a markdown file into all lines and prose lines (outside fenced code blocks).
function Read-Doc([string]$file) {
    $lines = @(Get-Content -LiteralPath $file -Encoding UTF8)
    $prose = New-Object System.Collections.Generic.List[string]
    $inFence = $false
    $fences = 0
    foreach ($line in $lines) {
        if ($line -match '^\s*(```|~~~)') { $inFence = -not $inFence; $fences++; continue }
        if (-not $inFence) { $prose.Add($line) }
    }
    return @{ Lines = $lines; Prose = $prose; CodeBlocks = [int][math]::Floor($fences / 2) }
}

# A short fingerprint of the document structure, used to compare ru/en versions.
function Get-Shape($doc) {
    $levels = @{}
    $details = 0
    $inFrontMatter = ($doc.Lines.Count -gt 0 -and $doc.Lines[0] -eq '---')
    $first = $true
    foreach ($line in $doc.Prose) {
        if ($inFrontMatter) {
            if (-not $first -and $line -eq '---') { $inFrontMatter = $false }
            $first = $false
            continue
        }
        if ($line -match '^(#{1,6})\s') {
            $key = 'h' + $Matches[1].Length
            $levels[$key] = 1 + [int]$levels[$key]
        }
        if ($line -match '<details') { $details++ }
    }
    $parts = @($levels.Keys | Sort-Object | ForEach-Object { "$_=$($levels[$_])" })
    $parts += "code=$($doc.CodeBlocks)"
    $parts += "details=$details"
    return ($parts -join ' ')
}

$files = @(Get-ChildItem -LiteralPath $scope -Recurse -File -Filter *.md | Where-Object {
    $rel = Get-RelPath $_.FullName
    $rel -notmatch '^(\.claude|_templates|\.git)[\\/]' -and
    $_.FullName -notmatch '[\\/](node_modules|bin|obj|\.venv)[\\/]'
})

foreach ($f in $files) {
    $rel = Get-RelPath $f.FullName
    $doc = Read-Doc $f.FullName

    if ($f.Name -match '^(?<base>.+)\.(?<lang>ru|en)\.md$') {
        $base = $Matches['base']
        $lang = $Matches['lang']
        $otherLang = if ($lang -eq 'ru') { 'en' } else { 'ru' }
        $pairPath = Join-Path $f.DirectoryName "$base.$otherLang.md"

        if (-not (Test-Path -LiteralPath $pairPath)) {
            $problems.Add("$rel : missing translation $base.$otherLang.md")
        }
        elseif ($lang -eq 'ru') {
            $shapeRu = Get-Shape $doc
            $shapeEn = Get-Shape (Read-Doc $pairPath)
            if ($shapeRu -ne $shapeEn) {
                $problems.Add("$rel : structure differs from $base.en.md (ru: $shapeRu | en: $shapeEn)")
            }
        }

        if ($rel -match '^tracks[\\/]') {
            if ($doc.Lines.Count -eq 0 -or $doc.Lines[0] -ne '---') {
                $problems.Add("$rel : no front matter")
            }
            elseif (-not ($doc.Lines | Where-Object { $_ -match "^lang:\s*$lang\b" })) {
                $problems.Add("$rel : front matter 'lang' is not '$lang'")
            }
        }
    }

    $isRoadmap = ($rel -match '^tracks[\\/][^\\/]+[\\/]overview\.(ru|en)\.md$') -or
                 ($rel -match '^(README(\.(ru|en))?|PROGRESS)\.md$')

    foreach ($line in $doc.Prose) {
        $withoutInlineCode = $line -replace '`[^`]*`', ''
        foreach ($m in [regex]::Matches($withoutInlineCode, '\]\(([^)\s]+)\)')) {
            $target = $m.Groups[1].Value
            if ($target -match '^(https?:|mailto:|#)') { continue }
            $target = ($target -split '#')[0]
            if (-not $target) { continue }
            $resolved = Join-Path $f.DirectoryName ([uri]::UnescapeDataString($target))
            if (-not (Test-Path -LiteralPath $resolved)) {
                if ($isRoadmap) { $planned.Add("$rel -> $target") }
                else { $problems.Add("$rel : broken link $target") }
            }
        }
    }
}

# Detail levels: the `levels` list in a material's overview must match what exists on disk.
$levelFiles = @{
    1 = @('basics.ru.md')
    2 = @('advanced.ru.md')
    3 = @('expert.ru.md')
    4 = @('quiz.ru.md', 'exercises.ru.md')
    5 = @('code\examples')
    6 = @('code\tests')
}
$materials = 0
foreach ($f in $files) {
    $rel = Get-RelPath $f.FullName
    if ($rel -notmatch '^tracks[\\/][^\\/]+[\\/](modules|libraries|tools|recipes)[\\/][^\\/]+[\\/]overview\.ru\.md$') { continue }
    $materials++
    $line = Get-Content -LiteralPath $f.FullName -Encoding UTF8 | Where-Object { $_ -match '^levels:\s*\[' } | Select-Object -First 1
    if (-not $line) {
        $problems.Add("$rel : no 'levels' in front matter")
        continue
    }
    $declared = @([regex]::Matches($line, '\d') | ForEach-Object { [int]$_.Value })
    foreach ($level in 1..6) {
        $present = $true
        foreach ($name in $levelFiles[$level]) {
            if (-not (Test-Path -LiteralPath (Join-Path $f.DirectoryName $name))) { $present = $false }
        }
        $isDeclared = $declared -contains $level
        if ($isDeclared -and -not $present) {
            $problems.Add("$rel : level $level is declared, but $($levelFiles[$level] -join ' + ') is missing")
        }
        if ($present -and -not $isDeclared) {
            $problems.Add("$rel : level $level exists on disk but is not listed in 'levels'")
        }
    }
    if (($declared -contains 6) -and -not ($declared -contains 5)) {
        $problems.Add("$rel : level 6 requires level 5")
    }
}

Write-Host "Checked $($files.Count) markdown files and $materials materials in $(Get-RelPath $scope)"
if ($planned.Count -gt 0) {
    Write-Host "Planned: $($planned.Count) roadmap links to materials that are not created yet"
}
if ($problems.Count -gt 0) {
    Write-Host "Problems: $($problems.Count)" -ForegroundColor Red
    foreach ($p in $problems) { Write-Host "  $p" }
    exit 1
}
Write-Host 'OK' -ForegroundColor Green
