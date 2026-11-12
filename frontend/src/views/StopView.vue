<script setup lang="ts">
import { onMounted, onUnmounted, ref, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { api } from '../api/client'
import type { StopBoard } from '../api/types'
import FavoriteButton from '../components/FavoriteButton.vue'
import LineBadge from '../components/LineBadge.vue'
import MapPanel from '../components/MapPanel.vue'
import StatusMessage from '../components/StatusMessage.vue'
import TimeDelay from '../components/TimeDelay.vue'
import { useAsync } from '../composables/useAsync'
import { ageLabel, confidenceLabel, untilLabel } from '../lib/format'

const props = defineProps<{ id: string }>()
const { data, error, loading, notFound, run } = useAsync(() => api.stop(props.id))
const board = ref<StopBoard | null>(null)
const boardError = ref<string | null>(null)
const now = ref(new Date())
let timer: ReturnType<typeof setInterval> | undefined

async function refreshBoard() {
  try {
    board.value = await api.stopBoard(props.id)
    boardError.value = null
  } catch {
    boardError.value = 'Horaires indisponibles pour le moment.'
  }
  now.value = new Date()
}

async function load() {
  await run()
  await refreshBoard()
}
watch(() => props.id, load)
onMounted(() => {
  void load()
  timer = setInterval(refreshBoard, 30000)
})
onUnmounted(() => clearInterval(timer))
</script>

<template>
  <section class="space-y-4">
    <StatusMessage :loading="loading" :error="notFound ? 'Cet arrêt n’existe pas ou plus.' : error" />
    <template v-if="data">
      <div class="flex flex-wrap items-center gap-3">
        <h1 class="text-2xl font-bold">{{ data.stop.name }}</h1>
        <FavoriteButton kind="Stop" :target-id="data.stop.id" />
      </div>
      <div class="flex flex-wrap gap-1"><LineBadge v-for="l in data.stop.lines" :key="l.id" :line="l" /></div>

      <div class="grid gap-4 lg:grid-cols-[1.4fr_1fr]">
        <div class="card">
          <h2 class="mb-1 font-semibold">Prochains passages</h2>
          <p class="muted mb-2">
            Heure prévue et heure estimée par Ametis quand le bus est suivi en temps réel.
            <template v-if="board?.feedAgeSeconds !== null && board?.feedAgeSeconds !== undefined"> Temps réel mis à jour {{ ageLabel(board.feedAgeSeconds) }}.</template>
          </p>
          <StatusMessage :loading="!board && !boardError" :error="boardError" :empty="!!board && !board.upcoming.length" empty-text="Aucun passage prévu dans les 3 prochaines heures." />
          <ul class="divide-y divide-slate-100 dark:divide-slate-800">
            <li v-for="(e, i) in board?.upcoming ?? []" :key="`${e.tripId ?? ''}-${e.scheduledTime}-${i}`">
              <component :is="e.tripId ? RouterLink : 'div'" :to="e.tripId ? { name: 'trip', params: { tripId: e.tripId } } : undefined"
                         class="flex items-center gap-3 py-2" :class="{ 'hover:bg-slate-50 dark:hover:bg-slate-800/50': e.tripId }">
                <LineBadge :line="e.line" />
                <span class="min-w-0 flex-1">
                  <span class="block truncate">{{ e.destination ?? '—' }}</span>
                  <span class="text-xs text-slate-500">{{ untilLabel(e.expectedTime ?? e.scheduledTime, now) }}<template v-if="e.isRealtime && e.confidence !== 'High'"> · rattachement {{ confidenceLabel[e.confidence] }}</template></span>
                </span>
                <TimeDelay :scheduled="e.scheduledTime" :expected="e.expectedTime" :delay="e.delaySeconds" :stale="e.isStale" />
              </component>
            </li>
          </ul>
        </div>

        <div class="card">
          <h2 class="mb-1 font-semibold">Derniers passages</h2>
          <p class="muted mb-2">Heures de passage publiées par Ametis (30 dernières minutes).</p>
          <StatusMessage :empty="!!board && !board.recent.length" empty-text="Aucun passage suivi en temps réel récemment." />
          <ul class="divide-y divide-slate-100 dark:divide-slate-800">
            <li v-for="(e, i) in board?.recent ?? []" :key="`r-${e.tripId}-${i}`">
              <RouterLink :to="{ name: 'trip', params: { tripId: e.tripId! } }" class="flex items-center gap-3 py-2 hover:bg-slate-50 dark:hover:bg-slate-800/50">
                <LineBadge :line="e.line" />
                <span class="min-w-0 flex-1 truncate">{{ e.destination ?? '—' }}</span>
                <TimeDelay :scheduled="e.scheduledTime" :expected="e.expectedTime" :delay="e.delaySeconds" passed />
              </RouterLink>
            </li>
          </ul>
        </div>
      </div>

      <MapPanel :stops="[data.stop, ...data.quays]" :center="[data.stop.longitude, data.stop.latitude]" :zoom="16" height="300px" />
    </template>
  </section>
</template>
