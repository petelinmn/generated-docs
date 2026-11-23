import fs from 'node:fs'
import path from 'node:path'
import { defineConfig } from 'vitepress'
import { withMermaid } from 'vitepress-plugin-mermaid'
import { listTracks, trackSidebar, trackTitle, type Lang } from './sidebar.mts'

const tracks = listTracks()
const base = process.env.DOCS_BASE ?? '/generated-docs/'

// `ru/tracks/x/name.md` (rewritten page path) -> `tracks/x/name.ru.md` (file in the repo)
function originalSource(rel: string): string | null {
  let m = rel.match(/^(ru|en)\/index\.md$/)
  if (m) return `README.${m[1]}.md`
  m = rel.match(/^(ru|en)\/(tracks\/.*)\.md$/)
  if (m) return `${m[2]}.${m[1]}.md`
  return rel === 'index.md' ? 'README.md' : rel === 'progress.md' ? 'PROGRESS.md' : null
}

const repoUrl = 'https://github.com/petelinmn/generated-docs'
const repoRoot = path.resolve(import.meta.dirname, '..')

// Relative markdown link (resolved against the repo directory of the current file) -> site URL.
// VitePress adds `base` to absolute links itself. Anything that is not a published page
// (CLAUDE.md, code/, src/, tools/) points to GitHub.
function siteUrl(href: string, dir: string): string | null {
  const m = href.match(/^([^#?]*)(#.*)?$/)
  if (!m || !m[1] || /^[a-z][a-z0-9+.-]*:/i.test(m[1]) || m[1].startsWith('/')) return null
  const resolved = path.posix.normalize(path.posix.join(dir, m[1])).replace(/\/$/, '')
  const hash = m[2] ?? ''
  if (resolved === 'PROGRESS.md') return `/progress${hash}`
  if (resolved === 'README.md') return `/${hash}`
  const lang = resolved.match(/^README\.(ru|en)\.md$/)
  if (lang) return `/${lang[1]}/${hash}`
  const doc = resolved.match(/^(tracks\/.*)\.(ru|en)\.md$/)
  if (doc && !/\/(notes|review)\.(ru|en)\.md$/.test(resolved)) return `/${doc[2]}/${doc[1]}${hash}`
  const full = path.join(repoRoot, resolved)
  if (fs.existsSync(full)) {
    return `${repoUrl}/${fs.statSync(full).isDirectory() ? 'tree' : 'blob'}/main/${resolved}`
  }
  return null
}

// A link to a markdown file that does not exist yet (a planned material in a roadmap, README or PROGRESS).
// Such links are rendered red on the site (see theme/custom.css).
function isPlanned(href: string, dir: string): boolean {
  const m = href.match(/^([^#?]*)/)
  if (!m || !m[1] || /^[a-z][a-z0-9+.-]*:/i.test(m[1]) || m[1].startsWith('/')) return false
  const resolved = path.posix.normalize(path.posix.join(dir, m[1]))
  return /\.md$/.test(resolved) && !resolved.startsWith('..') && !fs.existsSync(path.join(repoRoot, resolved))
}

const escapeHtml = (s: string) => s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')

// Markdown is not parsed inside a raw HTML line like `<details><summary>…</summary>`, so
// `Foo<T>` in backticks would reach Vue as an unclosed <T> tag. Turn code spans into <code>.
function summaryCodeSpans() {
  return {
    name: 'summary-code-spans',
    enforce: 'pre' as const,
    transform(code: string, id: string) {
      if (!id.endsWith('.md')) return null
      let changed = false
      const out = code.replace(/^.*<summary>.*<\/summary>.*$/gm, (line) => {
        const fixed = line.replace(/`([^`]+)`/g, (_m, c) => `<code v-pre>${escapeHtml(c)}</code>`)
        if (fixed !== line) changed = true
        return fixed
      })
      return changed ? { code: out, map: null } : null
    },
  }
}

function localeThemeConfig(lang: Lang) {
  const sidebar: Record<string, any> = {}
  for (const t of tracks) sidebar[`/${lang}/tracks/${t}/`] = trackSidebar(t, lang)
  return {
    nav: [
      { text: lang === 'ru' ? 'Треки' : 'Tracks', items: tracks.map((t) => ({ text: trackTitle(t, lang), link: `/${lang}/tracks/${t}/overview` })) },
      { text: lang === 'ru' ? 'Прогресс' : 'Progress', link: '/progress' },
    ],
    sidebar,
    outline: { level: [2, 3], label: lang === 'ru' ? 'На этой странице' : 'On this page' },
    docFooter: lang === 'ru' ? { prev: 'Назад', next: 'Дальше' } : { prev: 'Previous', next: 'Next' },
    darkModeSwitchLabel: lang === 'ru' ? 'Тема' : 'Theme',
    sidebarMenuLabel: lang === 'ru' ? 'Меню' : 'Menu',
    returnToTopLabel: lang === 'ru' ? 'Наверх' : 'Back to top',
    langMenuLabel: lang === 'ru' ? 'Язык' : 'Language',
  }
}

// Files `name.ru.md` / `name.en.md` live side by side in the repo; map them to /ru/ and /en/ trees.
const rewrites: Record<string, string> = {
  'README.ru.md': 'ru/index.md',
  'README.en.md': 'en/index.md',
  'README.md': 'index.md',
  'PROGRESS.md': 'progress.md',
  'tracks/:path*/:name.ru.md': 'ru/tracks/:path*/:name.md',
  'tracks/:path*/:name.en.md': 'en/tracks/:path*/:name.md',
}

export default withMermaid(
  defineConfig({
    title: 'Generated Docs',
    description: 'Knowledge base for learning technologies / База знаний для изучения технологий',
    base,
    cleanUrls: true,
    lastUpdated: false,
    // Roadmaps intentionally link to materials that are not created yet; code/ and src/ are folders.
    // Real broken links are caught by tools/check-docs.ps1.
    ignoreDeadLinks: true,
    srcExclude: [
      '.claude/**',
      '_templates/**',
      'tools/**',
      'node_modules/**',
      'CLAUDE.md',
      'PLAN.md',
      '**/notes.md',
      '**/review.md',
      'tracks/**/code/**',
      'tracks/**/projects/*/src/**',
    ],
    rewrites,
    vite: { plugins: [summaryCodeSpans()] },
    markdown: {
      // Same heading ids as on GitHub, so `file.md#anchor` links work both there and on the site.
      anchor: {
        slugify: (s: string) =>
          s
            .trim()
            .toLowerCase()
            .replace(/[^\p{L}\p{N}\p{M} _-]/gu, '')
            .replace(/ /g, '-'),
      },
      config(md) {
        // Links between `name.ru.md` / `name.en.md` files must point to the /ru/ and /en/ site trees.
        md.core.ruler.push('lang-links', (state) => {
          const rel: string = (state.env as any)?.relativePath ?? ''
          const source = originalSource(rel)
          if (!source) return
          const dir = path.posix.dirname(source)
          for (const block of state.tokens) {
            for (const tok of block.children ?? []) {
              if (tok.type !== 'link_open') continue
              const href = tok.attrGet('href')
              if (href && isPlanned(href, dir)) {
                tok.attrJoin('class', 'link-planned')
                tok.attrSet('title', rel.startsWith('ru/') ? 'Материал ещё не создан' : 'Not created yet')
              }
              const target = href && siteUrl(href, dir)
              if (target) tok.attrSet('href', target)
            }
          }
        })
        // `{{ … }}` in inline code (Go templates, C# interpolation) must not be treated as a Vue expression.
        const inline = md.renderer.rules.code_inline!
        md.renderer.rules.code_inline = (tokens, idx, options, env, self) =>
          inline(tokens, idx, options, env, self).replace(/^<code/, '<code v-pre')
      },
    },
    locales: {
      ru: { label: 'Русский', lang: 'ru', link: '/ru/', themeConfig: localeThemeConfig('ru') },
      en: { label: 'English', lang: 'en', link: '/en/', themeConfig: localeThemeConfig('en') },
    },
    themeConfig: {
      search: {
        provider: 'local',
        options: {
          locales: {
            ru: {
              translations: {
                button: { buttonText: 'Поиск', buttonAriaLabel: 'Поиск' },
                modal: {
                  noResultsText: 'Ничего не найдено',
                  resetButtonTitle: 'Сбросить',
                  footer: { selectText: 'выбрать', navigateText: 'перейти', closeText: 'закрыть' },
                },
              },
            },
          },
        },
      },
      socialLinks: [{ icon: 'github', link: 'https://github.com/petelinmn/generated-docs' }],
    },
    mermaid: {},
    mermaidPlugin: { class: 'mermaid' },
  }),
)
