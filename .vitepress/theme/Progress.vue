<script setup lang="ts">
// Learning progress of the reader, kept in this browser only (localStorage).
import { useData, useRoute } from 'vitepress'
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue'

const KEY = 'generated-docs:learned'
// The id has no language segment, so the ru and en versions share one mark.
const MATERIAL = /\/(?:ru|en)\/(tracks\/[^/]+\/(?:modules|libraries|tools|recipes|projects)\/[^/]+)\//
const TRACK_OVERVIEW = /\/(?:ru|en)\/tracks\/[^/]+\/overview(?:\.html)?$/

const route = useRoute()
const { lang } = useData()
const learned = ref(new Set<string>())
const mounted = ref(false)
const total = ref(0)
const done = ref(0)

const materialId = computed(() => route.path.match(MATERIAL)?.[1])
const isTrack = computed(() => TRACK_OVERVIEW.test(route.path))
const ru = computed(() => lang.value.startsWith('ru'))

// Storage may be unavailable (private window, blocked site data): the page must still work.
function load() {
  try {
    const value = JSON.parse(localStorage.getItem(KEY) ?? '[]')
    learned.value = new Set(Array.isArray(value) ? value : [])
  } catch {
    learned.value = new Set()
  }
}

function toggle(event: Event) {
  const id = materialId.value
  if (!id) return
  load() // another tab may have changed the list
  const next = new Set(learned.value)
  if ((event.target as HTMLInputElement).checked) next.add(id)
  else next.delete(id)
  learned.value = next
  try {
    localStorage.setItem(KEY, JSON.stringify([...next]))
  } catch {
    // the mark lives until the page is reloaded
  }
}

// Marks learned materials in the roadmap of a track overview.
function markRoadmap() {
  const ids = new Set<string>()
  const finished = new Set<string>()
  if (isTrack.value) {
    // Links to materials of other tracks do not belong to this roadmap.
    const own = `${route.path.match(/\/(tracks\/[^/]+)\/overview/)?.[1]}/`
    for (const link of document.querySelectorAll<HTMLAnchorElement>('.vp-doc a')) {
      const id = new URL(link.href).pathname.match(MATERIAL)?.[1]
      // Planned materials are not counted: there is nothing to learn yet.
      if (!id?.startsWith(own) || link.classList.contains('link-planned')) continue
      ids.add(id)
      const isDone = learned.value.has(id)
      link.classList.toggle('link-learned', isDone)
      if (isDone) finished.add(id)
    }
  }
  total.value = ids.size
  done.value = finished.size
}

async function refresh() {
  await nextTick()
  // The new page content is rendered after the route changes.
  requestAnimationFrame(markRoadmap)
}

function onStorage(event: StorageEvent) {
  if (event.key === KEY) load()
}

onMounted(() => {
  load()
  mounted.value = true
  window.addEventListener('storage', onStorage)
  refresh()
})
onUnmounted(() => window.removeEventListener('storage', onStorage))
watch([() => route.data, learned], refresh)
</script>

<template>
  <div v-if="mounted && materialId" class="progress-box">
    <label>
      <input type="checkbox" :checked="learned.has(materialId)" @change="toggle" />
      {{ ru ? 'Изучено' : 'Learned' }}
    </label>
    <span class="progress-hint">{{ ru ? 'сохраняется в этом браузере' : 'saved in this browser' }}</span>
  </div>
  <div v-else-if="mounted && isTrack && total" class="progress-box">
    <span>{{ ru ? 'Изучено' : 'Learned' }}: {{ done }} / {{ total }}</span>
    <progress :value="done" :max="total" />
    <span class="progress-hint">{{ ru ? 'сохраняется в этом браузере' : 'saved in this browser' }}</span>
  </div>
</template>

<style scoped>
.progress-box {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px 16px;
  margin-bottom: 20px;
  padding: 8px 12px;
  border: 1px solid var(--vp-c-divider);
  border-radius: 8px;
  font-size: 14px;
}
.progress-box label {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
}
.progress-hint {
  color: var(--vp-c-text-3);
  font-size: 12px;
}
</style>
