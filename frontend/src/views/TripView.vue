<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { api } from '../api/client'
import type { LineShape } from '../api/types'
import LineBadge from '../components/LineBadge.vue'
import MapPanel from '../components/MapPanel.vue'
import StatusMessage from '../components/StatusMessage.vue'
import TimeDelay from '../components/TimeDelay.vue'
import VehicleCard from '../components/VehicleCard.vue'
import { useAsync } from '../composables/useAsync'
import { ageLabel, confidenceLabel, lineColors, untilLabel } from '../lib/format'
import { useRealtimeStore } from '../stores/realtime'

const props = defineProps<{ tripId: string }>()
const realtime = useRealtimeStore()
const { data, error, loading, notFound, run } = useAsync(() => api.trip(props.tripId))
const now = ref(new Date())
let timer: ReturnType<typeof setInterval> | undefined

const vehicle = computed(() => realtime.vehicles.find((p_V) => p_V.tripId === props.tripId) ?? null)
const trace = computed<LineShape[]>(() => (data.value?.trace.length ?? 0) > 1 ? [{ shapeId: 'trace', coordinates: data.value!.trace as [number, number][] }] : [])
const passedCount = computed(() => data.value?.stops.filter((p_S) => p_S.status === 'Passed').length ?? 0)
const next = computed(() => data.value?.stops.find((p_S) => p_S.status === 'Next') ?? null)

async function refresh(p_Scroll = false) {
  await run()
  now.value = new Date()
  if (p_Scroll) {
    await nextTick()
    document.getElementById('arret-suivant')?.scrollIntoView({ block: 'center' })
  }
}

watch(() => props.tripId, () => refresh(true))
onMounted(() => {
  void realtime.start()
  void refresh(true)
  timer = setInterval(() => refresh(false), 30000)
})
onUnmounted(() => {
  clearInterval(timer)
  void realtime.stop()
})
</script>

<template>
  <section class="space-y-4">
    <StatusMessage :loading="loading && !data" :error="notFound ? 'Course introuvable : elle n’est pas (ou plus) suivie en temps réel.' : error" />
    <template v-if="data">
      <div class="flex flex-wrap items-center gap-3">
        <RouterLink v-if="data.line" :to="{ name: 'line', params: { id: data.line.id } }"><LineBadge :line="data.line" large /></RouterLink>
        <LineBadge v-else :line="null" large />
        <div>
          <h1 class="text-xl font-bold">Vers {{ data.destination ?? 'destination inconnue' }}</h1>
          <p class="muted">
            Bus {{ data.vehicleId ?? '—' }} · {{ passedCount }}/{{ data.stops.length }} arrêts desservis
            <template v-if="data.lineConfidence !== 'High'"> · ligne {{ confidenceLabel[data.lineConfidence] }}</template>
          </p>
        </div>
      </div>

      <p v-if="data.isStale" class="rounded bg-amber-50 p-2 text-sm text-amber-800 dark:bg-amber-950 dark:text-amber-200" role="status">
        Le temps réel n’est plus à jour (dernier flux {{ ageLabel(data.feedAgeSeconds) }}) : les heures affichées peuvent avoir changé.
      </p>

      <div v-if="vehicle" class="card"><VehicleCard :vehicle="vehicle" :show-trip-link="false" /></div>

      <div class="grid gap-4 lg:grid-cols-[1fr_1.2fr]">
        <ol class="card" aria-label="Arrêts de la course">
          <li v-for="s in data.stops" :id="s.status === 'Next' ? 'arret-suivant' : undefined" :key="`${s.order}-${s.scheduledTime}`"
              class="relative flex items-center gap-3 py-2 pl-6" :class="{ 'text-slate-500': s.status === 'Passed' }">
            <span class="absolute top-0 bottom-0 left-[9px] w-0.5" :style="{ background: s.status === 'Passed' ? lineColors(data.line).background : '#cbd5e1' }" aria-hidden="true" />
            <span class="absolute left-1 z-10 h-3.5 w-3.5 rounded-full border-2"
                  :class="s.status === 'Next' ? 'animate-pulse' : ''"
                  :style="{ borderColor: lineColors(data.line).background, background: s.status === 'Upcoming' ? 'white' : lineColors(data.line).background }" aria-hidden="true" />
            <span class="min-w-0 flex-1">
              <RouterLink v-if="s.networkStopId" :to="{ name: 'stop', params: { id: s.networkStopId } }" class="block truncate hover:underline" :class="{ 'font-semibold': s.status === 'Next' }">{{ s.stopName }}</RouterLink>
              <span v-else class="block truncate">{{ s.stopName }}</span>
              <span class="text-xs" :class="s.status === 'Next' ? 'font-semibold text-brand dark:text-emerald-300' : 'text-slate-500'">
                {{ s.status === 'Passed' ? 'desservi' : s.status === 'Next' ? `prochain arrêt · ${untilLabel(s.expectedTime, now)}` : untilLabel(s.expectedTime, now) }}
              </span>
            </span>
            <TimeDelay :scheduled="s.scheduledTime" :expected="s.expectedTime" :delay="s.delaySeconds" :passed="s.status === 'Passed'" />
          </li>
        </ol>
        <div class="space-y-2">
          <MapPanel :shapes="trace" :shape-color="lineColors(data.line).background" :vehicles="vehicle ? [vehicle] : []" height="460px" />
          <p class="muted">Tracé : positions enregistrées du bus sur cette course<template v-if="next"> · prochain arrêt {{ next.stopName }}</template>.</p>
        </div>
      </div>
    </template>
  </section>
</template>
