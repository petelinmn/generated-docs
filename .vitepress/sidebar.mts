import fs from 'node:fs'
import path from 'node:path'

export type Lang = 'ru' | 'en'

interface Material {
  dir: string // path relative to the repo root, e.g. tracks/kubernetes/modules/04-pods
  kind: string // modules | libraries | recipes | tools | projects
  slug: string
  title: string
  part?: number
  files: { doc: string; title: string }[]
}

const root = path.resolve(import.meta.dirname, '..')

const labels = {
  ru: {
    overview: 'Обзор',
    basics: '1. Базовая теория',
    advanced: '2. Продвинутая теория',
    expert: '3. Экспертная теория',
    quiz: '4. Квиз',
    exercises: '4. Упражнения',
    kinds: {
      modules: 'Модули',
      libraries: 'Библиотеки',
      tools: 'Инструменты',
      recipes: 'Рецепты',
      projects: 'Проекты',
    },
    parts: ['Часть 1. Основы', 'Часть 2. Продвинутые темы', 'Часть 3. Темы для опытных'],
    track: 'Обзор трека',
  },
  en: {
    overview: 'Overview',
    basics: '1. Basics',
    advanced: '2. Advanced',
    expert: '3. Expert',
    quiz: '4. Quiz',
    exercises: '4. Exercises',
    kinds: {
      modules: 'Modules',
      libraries: 'Libraries',
      tools: 'Tools',
      recipes: 'Recipes',
      projects: 'Projects',
    },
    parts: ['Part 1. Fundamentals', 'Part 2. Advanced topics', 'Part 3. Expert topics'],
    track: 'Track overview',
  },
} as const

const docOrder = ['overview', 'basics', 'advanced', 'expert', 'quiz', 'exercises']
const kindOrder = ['modules', 'libraries', 'tools', 'recipes', 'projects']

function frontMatter(file: string): Record<string, string> {
  const text = fs.readFileSync(file, 'utf8')
  const m = text.match(/^---\r?\n([\s\S]*?)\r?\n---/)
  const out: Record<string, string> = {}
  if (!m) return out
  for (const line of m[1].split(/\r?\n/)) {
    const kv = line.match(/^([A-Za-z_]+):\s*(.*?)\s*(#.*)?$/)
    if (kv) out[kv[1]] = kv[2].replace(/^"(.*)"$/, '$1')
  }
  return out
}

function listMaterials(track: string, lang: Lang): Material[] {
  const result: Material[] = []
  for (const kind of kindOrder) {
    const kindDir = path.join(root, 'tracks', track, kind)
    if (!fs.existsSync(kindDir)) continue
    for (const slug of fs.readdirSync(kindDir).sort()) {
      const dir = path.join(kindDir, slug)
      const overview = path.join(dir, `overview.${lang}.md`)
      if (!fs.statSync(dir).isDirectory() || !fs.existsSync(overview)) continue
      const fm = frontMatter(overview)
      const files = docOrder
        .filter((d) => fs.existsSync(path.join(dir, `${d}.${lang}.md`)))
        .map((d) => ({ doc: d, title: labels[lang][d as keyof (typeof labels)['ru']] as string }))
      result.push({
        dir: `tracks/${track}/${kind}/${slug}`,
        kind,
        slug,
        title: fm.title || slug,
        part: fm.part ? Number(fm.part) : undefined,
        files,
      })
    }
  }
  return result
}

function materialItem(m: Material, lang: Lang, prefix: string) {
  const number = m.slug.match(/^(\d+[a-z]?)-/)?.[1]
  return {
    text: number ? `${number} · ${m.title}` : m.title,
    collapsed: true,
    items: m.files.map((f) => ({
      text: f.title,
      link: `${prefix}/${m.dir}/${f.doc}`,
    })),
  }
}

export function trackSidebar(track: string, lang: Lang) {
  const prefix = `/${lang}`
  const l = labels[lang]
  const materials = listMaterials(track, lang)
  const items: any[] = [{ text: l.track, link: `${prefix}/tracks/${track}/overview` }]

  for (const kind of kindOrder) {
    const ofKind = materials.filter((m) => m.kind === kind)
    if (!ofKind.length) continue
    if (kind === 'modules' && ofKind.some((m) => m.part)) {
      for (const part of [1, 2, 3]) {
        const inPart = ofKind.filter((m) => (m.part ?? 1) === part)
        if (!inPart.length) continue
        items.push({
          text: l.parts[part - 1],
          collapsed: part !== 1,
          items: inPart.map((m) => materialItem(m, lang, prefix)),
        })
      }
    } else {
      items.push({
        text: l.kinds[kind as keyof typeof l.kinds],
        collapsed: true,
        items: ofKind.map((m) => materialItem(m, lang, prefix)),
      })
    }
  }
  return items
}

export function listTracks(): string[] {
  const dir = path.join(root, 'tracks')
  return fs
    .readdirSync(dir)
    .filter((t) => fs.existsSync(path.join(dir, t, 'overview.ru.md')))
    .sort()
}

export function trackTitle(track: string, lang: Lang): string {
  const file = path.join(root, 'tracks', track, `overview.${lang}.md`)
  return frontMatter(file).title || track
}
