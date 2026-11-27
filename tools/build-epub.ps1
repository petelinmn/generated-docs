<#
.SYNOPSIS
  Builds an EPUB book from one track of the knowledge base.

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File tools/build-epub.ps1 -Track csharp -Lang ru

.EXAMPLE
  tools/build-epub.ps1 -Track react -Lang en -Levels 1,2

.NOTES
  Needs pandoc 3.x. Levels: 1 basics, 2 advanced, 3 expert, 4 quiz and exercises.
  The overview of every material is always included.
  Keep this file in UTF-8 with BOM: Windows PowerShell 5.1 needs it for Cyrillic.
#>
param(
    [Parameter(Mandatory = $true)][string]$Track,
    [ValidateSet('ru', 'en')][string]$Lang = 'ru',
    [string[]]$Levels = @('1', '2', '3', '4'),
    [string]$Out
)

$ErrorActionPreference = 'Stop'
$utf8 = New-Object System.Text.UTF8Encoding($false)
$root = Split-Path -Parent $PSScriptRoot
$trackDir = Join-Path $root "tracks\$Track"
if (-not (Test-Path $trackDir)) { throw "Track not found: $Track" }
if (-not (Get-Command pandoc -ErrorAction SilentlyContinue)) {
    throw 'pandoc is not installed: winget install --id JohnMacFarlane.Pandoc'
}

# "-File script.ps1 -Levels 1,2" arrives as the single string "1,2"
$levelList = @($Levels | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() })
if ($levelList | Where-Object { $_ -notmatch '^[1-4]$' }) { throw "Levels must be numbers from 1 to 4, got: $($Levels -join ',')" }

$text = @{
    ru = @{ part = 'Часть'; intro = 'О треке'; libraries = 'Библиотеки'; tools = 'Инструменты'; recipes = 'Рецепты'; projects = 'Проекты'
            answers = 'Ответы и решения'; answer = 'Открыть'; back = 'Назад к вопросу'; levelsTable = @('Уровни материала'); nav = 'Уровни' }
    en = @{ part = 'Part'; intro = 'About the track'; libraries = 'Libraries'; tools = 'Tools'; recipes = 'Recipes'; projects = 'Projects'
            answers = 'Answers and solutions'; answer = 'Open'; back = 'Back to the question'; levelsTable = 'Levels of this material', 'Material levels'; nav = 'Levels' }
}[$Lang]

$docs = @('overview')
if ($levelList -contains 1) { $docs += 'basics' }
if ($levelList -contains 2) { $docs += 'advanced' }
if ($levelList -contains 3) { $docs += 'expert' }
if ($levelList -contains 4) { $docs += 'quiz', 'exercises' }

function Read-Text([string]$path) { [IO.File]::ReadAllText($path, $utf8) -replace "`r`n", "`n" }

function Get-Field([string]$content, [string]$name) {
    if ($content -match "(?s)^---\n(.*?)\n---") {
        if ($matches[1] -match "(?m)^${name}:\s*`"?(.*?)`"?\s*$") { return $matches[1] }
    }
    return $null
}

# Anchor the way GitHub builds it from a heading
function Get-Slug([string]$heading) {
    $h = $heading -replace '\[([^\]]*)\]\([^)]*\)', '$1'
    $h = $h.ToLowerInvariant() -replace '[^\p{L}\p{Nd} _-]', ''
    return $h.Trim() -replace ' ', '-'
}

# ---------- 1. what goes into the book, in roadmap order ----------
$trackOverview = Join-Path $trackDir "overview.$Lang.md"
$trackText = Read-Text $trackOverview
$found = New-Object System.Collections.Generic.List[string]
foreach ($m in [regex]::Matches($trackText, '\]\((?:\./)?((?:modules|libraries|tools|recipes|projects)/[^/)#]+)')) {
    $found.Add($m.Groups[1].Value)
}
# materials that exist but are not linked from the track overview
foreach ($kind in 'modules', 'libraries', 'tools', 'recipes', 'projects') {
    $dir = Join-Path $trackDir $kind
    if (Test-Path $dir) { Get-ChildItem $dir -Directory | Sort-Object Name | ForEach-Object { $found.Add("$kind/$($_.Name)") } }
}

$materials = @()
$seen = @{}
foreach ($rel in $found) {
    if ($seen.ContainsKey($rel)) { continue }
    $seen[$rel] = $true
    $dir = Join-Path $trackDir ($rel -replace '/', '\')
    $overview = Join-Path $dir "overview.$Lang.md"
    if (-not (Test-Path $overview)) { continue }   # planned, not written yet
    $content = Read-Text $overview
    $kind = $rel.Split('/')[0]
    $part = Get-Field $content 'part'
    if ($kind -eq 'modules') { $group = "$($text.part) $part"; $order = [int]("0$part") }
    else { $group = $text[$kind]; $order = @{ libraries = 10; tools = 11; recipes = 12; projects = 13 }[$kind] }
    $materials += [pscustomobject]@{
        Dir = $dir; Id = ($rel -replace '/', '-'); Title = (Get-Field $content 'title'); Group = $group
        Order = $order; Index = $materials.Count
    }
}
if ($materials.Count -eq 0) { throw "No materials in track $Track" }
$materials = $materials | Sort-Object Order, Index

# ---------- 2. files and their anchors ----------
$files = @()
$files += [pscustomobject]@{ Path = $trackOverview; Id = 'intro'; Material = $null; Doc = 'intro' }
foreach ($mat in $materials) {
    foreach ($doc in $docs) {
        $path = Join-Path $mat.Dir "$doc.$Lang.md"
        if (Test-Path $path) { $files += [pscustomobject]@{ Path = $path; Id = "$($mat.Id)-$doc"; Material = $mat; Doc = $doc } }
    }
}
$included = @{}
foreach ($f in $files) { $included[[IO.Path]::GetFullPath($f.Path).ToLowerInvariant()] = $f.Id }

$anchors = @{}   # "id--slug" that really exist in the book
foreach ($f in $files) {
    $fence = $null; $count = @{}
    foreach ($line in (Read-Text $f.Path) -split "`n") {
        if ($line -match '^\s*(`{3,}|~{3,})') {
            if (-not $fence) { $fence = $matches[1] } elseif ($line.Trim().StartsWith($fence)) { $fence = $null }
            continue
        }
        if (-not $fence -and $line -match '^(#{2,6})\s+(.*?)\s*$') {
            $slug = Get-Slug $matches[2]
            if ($count.ContainsKey($slug)) { $count[$slug]++; $slug = "$slug-$($count[$slug])" } else { $count[$slug] = 0 }
            $anchors["$($f.Id)--$slug"] = $true
        }
    }
}

# ---------- 3. rewrite links ----------
$linkRx = [regex]'(?<!!)\[((?:\\.|`[^`]*`|\[[^\]]*\]|[^\]`\\])*)\]\(((?:[^()\s]|\([^()]*\))+)\)'

function Convert-Links([string]$line, $file) {
    return $linkRx.Replace($line, {
        param($m)
        # a "link" inside inline code is code, for example `x[i](y)`
        $before = $line.Substring(0, $m.Index)
        if (([regex]::Matches($before, '`').Count % 2) -eq 1) { return $m.Value }
        $label = $m.Groups[1].Value
        $url = $m.Groups[2].Value
        if ($url -match '^[a-z]+:') { return $m.Value }
        $target, $hash = $url -split '#', 2
        if ($target) {
            $full = [IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $file.Path) ($target -replace '/', '\'))).ToLowerInvariant()
            $id = $included[$full]
        } else { $id = $file.Id }
        if (-not $id) { return $label }          # not part of the book: keep the text only
        if ($hash) {
            $decoded = [uri]::UnescapeDataString($hash).ToLowerInvariant()
            if ($anchors.ContainsKey("$id--$decoded")) { return "[$label](#$id--$decoded)" }
        }
        return "[$label](#$id)"
    })
}

# ---------- 4. assemble ----------
$book = New-Object System.Text.StringBuilder
$answers = New-Object System.Text.StringBuilder
$script:answerNo = 0
$mermaid = 0

function Add-Line($builder, [string]$line) { [void]$builder.Append($line).Append("`n") }

function Add-Document($file, [int]$shift) {
    $content = (Read-Text $file.Path) -replace '(?s)^---\n.*?\n---\n', ''
    $fence = $null; $count = @{}
    $inDetails = $false; $skipSection = $false; $titleDone = $false
    $heading = ''
    $question = ''   # number of the last bold "**3. ..." interview question
    $target = $book

    foreach ($raw in $content -split "`n") {
        $line = $raw
        if ($line -match '^\s*(`{3,}|~{3,})(.*)$') {
            if (-not $fence) {
                $fence = $matches[1]
                if ($matches[2].Trim() -eq 'mermaid') { $script:mermaidCount++ }
            } elseif ($line.Trim().StartsWith($fence)) { $fence = $null }
            if (-not $skipSection) { Add-Line $target $line }
            continue
        }
        if ($fence) { if (-not $skipSection) { Add-Line $target $line }; continue }

        # spoilers move to the appendix, so that answers are not visible next to the question
        if ($line -match '^\s*<details>\s*(?:<summary>(.*?)</summary>)?\s*$') {
            $script:answerNo++
            $n = $script:answerNo
            $label = ($matches[1] -replace '</?code>', '`')
            if (-not $label) { $label = $text.answer }
            Add-Line $book "[$label $([char]0x2192)](#ans-$n){#q-$n}"
            $where = if ($heading) { $heading } else { $file.Id }
            Add-Line $answers ''
            if ($question) { $where = "$where, $question" }
            Add-Line $answers "#### $where. $label {#ans-$n}"
            Add-Line $answers ''
            $inDetails = $true; $target = $answers
            continue
        }
        if ($inDetails -and $line -match '^\s*</details>\s*$') {
            Add-Line $answers ''
            Add-Line $answers "[$([char]0x2190) $($text.back)](#q-$($script:answerNo))"
            $inDetails = $false; $target = $book
            continue
        }

        if ($line -match '^(#{1,6})\s+(.*?)\s*$') {
            $depth = $matches[1].Length
            $name = $matches[2]
            if ($depth -eq 1 -and -not $titleDone) {
                $titleDone = $true
                $skipSection = $false
                continue                                   # replaced by the book heading
            }
            $skipSection = ($file.Doc -eq 'overview' -and $text.levelsTable -contains $name)
            if ($skipSection) { continue }
            $slug = Get-Slug $name
            if ($count.ContainsKey($slug)) { $count[$slug]++; $slug = "$slug-$($count[$slug])" } else { $count[$slug] = 0 }
            $heading = $name -replace '\s*\{.*\}$', ''
            $question = ''
            $level = [Math]::Min(6, $depth + $shift)
            Add-Line $target ('#' * $level + ' ' + (Convert-Links $name $file) + " {#$($file.Id)--$slug}")
            continue
        }
        if ($skipSection) { continue }
        if (-not $inDetails -and $line -match '^\*\*(\d+)\.\s') { $question = $matches[1] }
        # language switch and level navigation make no sense inside a book
        if ($line.StartsWith([char]::ConvertFromUtf32(0x1F310)) -or $line.StartsWith("**$($text.nav):**")) { continue }
        Add-Line $target (Convert-Links $line $file)
    }
}

$script:mermaidCount = 0
$docNames = @{
    ru = @{ overview = 'Обзор'; basics = 'Базовая теория'; advanced = 'Продвинутая теория'; expert = 'Экспертная теория'; quiz = 'Квиз'; exercises = 'Упражнения' }
    en = @{ overview = 'Overview'; basics = 'Basics'; advanced = 'Advanced'; expert = 'Expert'; quiz = 'Quiz'; exercises = 'Exercises' }
}[$Lang]

Add-Line $book "# $($text.intro) {#intro}"
Add-Line $book ''
Add-Document $files[0] 0

$group = $null
$material = $null
$groupNo = 0
foreach ($f in $files | Select-Object -Skip 1) {
    if ($f.Material.Group -ne $group) {
        $group = $f.Material.Group
        $groupNo++
        Add-Line $book ''
        Add-Line $book "# $group {#group-$groupNo}"
    }
    if ($f.Material.Id -ne $material) {
        $material = $f.Material.Id
        Add-Line $book ''
        Add-Line $book "## $($f.Material.Title) {#$material}"
        Add-Line $answers ''
        Add-Line $answers "<!-- $material -->"
        Add-Line $answers "## $($f.Material.Title) {#answers-$material}"
        $materialStart = $script:answerNo
    }
    Add-Line $book ''
    Add-Line $book "### $($docNames[$f.Doc]) {#$($f.Id)}"
    Add-Line $book ''
    Add-Document $f 2
}

# drop appendix headings of materials without any spoiler
$appendix = $answers.ToString()
$appendix = [regex]::Replace($appendix, '<!-- [^>]+ -->\n## [^\n]*\{#answers-[^\n]*\n(?=\s*(?:<!-- |$))', '')

$markdown = $book.ToString()
if ($appendix.Trim()) { $markdown += "`n# $($text.answers) {#answers}`n" + $appendix }

if (-not $Out) { $Out = Join-Path $root "dist\epub\$Track.$Lang.epub" }
$Out = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Out)
New-Item -ItemType Directory -Force (Split-Path -Parent $Out) | Out-Null

$work = Join-Path ([IO.Path]::GetTempPath()) ("epub-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $work | Out-Null
try {
    $mdPath = Join-Path $work 'book.md'
    $cssPath = Join-Path $work 'book.css'
    [IO.File]::WriteAllText($mdPath, $markdown, $utf8)
    [IO.File]::WriteAllText($cssPath, @'
body { line-height: 1.45; }
pre { white-space: pre-wrap; word-wrap: break-word; font-size: 0.8em; background: #f4f4f4; padding: 0.5em; }
code { font-family: monospace; }
table { border-collapse: collapse; font-size: 0.85em; width: 100%; }
th, td { border: 1px solid #999; padding: 0.2em 0.4em; vertical-align: top; }
blockquote { border-left: 3px solid #999; margin-left: 0; padding-left: 0.8em; }
'@, $utf8)

    $title = Get-Field $trackText 'title'
    if (-not $title) { $title = $Track }
    & pandoc $mdPath -f 'gfm+attributes' -t epub3 -o $Out --toc --toc-depth=3 --split-level=3 `
        --css $cssPath --metadata "title=$title" --metadata "lang=$Lang"
    if ($LASTEXITCODE -ne 0) { throw "pandoc failed with exit code $LASTEXITCODE" }
}
finally { Remove-Item -Recurse -Force $work }

$size = [Math]::Round((Get-Item $Out).Length / 1KB)
Write-Host "Materials: $($materials.Count), documents: $($files.Count), spoilers moved: $($script:answerNo)"
if ($script:mermaidCount) { Write-Host "Mermaid diagrams left as source text: $($script:mermaidCount)" }
Write-Host "Written: $Out ($size KB)"
