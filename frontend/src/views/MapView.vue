<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { api } from '../api/client'
import type { LineShape, LineSummary, StopSummary, TripDetail, VehicleView } from '../api/types'
import LineBadge from '../components/LineBadge.vue'
import MapPanel from '../components/MapPanel.vue'
import VehicleCard from '../components/VehicleCard.vue'
import { ageLabel, lineColors } from '../lib/format'
import { useRealtimeStore } from '../stores/realtime'

const router = useRouter()
const realtime = useRealtimeStore()
const stops = ref<StopSummary[]>([])
const lines = ref<LineSummary[]>([])
const selectedLine = ref<LineSummary | null>(null)
const shapes = ref<LineShape[]>([])
const vehicle = ref<VehicleView | null>(null)
const trip = ref<TripDetail | null>(null)
const tripError = ref<string | null>(null)
let tripTimer: ReturnType<typeof setInterval> | undefined

const liveVehicle = computed(() => vehicle.value ? realtime.vehicles.find((p_V) => p_V.vehicleId === vehicle.value!.vehicleId) ?? vehicle.value : null)
const tripOverlay = computed(() => trip.value
  ? { key: trip.value.tripId, color: lineColors(trip.value.line).background, stops: trip.value.stops, trace: trip.value.trace }
  : null)
const next = computed(() => trip.value?.stops.find((p_S) => p_S.status === 'Next') ?? null)
const passedCount = computed(() => trip.value?.stops.filter((p_S) => p_S.status === 'Passed').length ?? 0)

async function selectLine(p_Line: LineSummary | null) {
  selectedLine.value = p_Line
  shapes.value = p_Line ? await api.lineShapes(p_Line.id).catch(() => []) : []
}

async function loadTrip() {
  const v_TripId = liveVehicle.value?.tripId
  if (!v_TripId) {
    trip.value = null
    tripError.value = 'Ce bus n’est rattaché à aucune course : trajet indisponible.'
    return
  }
  try {
    trip.value = await api.trip(v_TripId)
    tripError.value = null
  } catch {
    trip.value = null
    tripError.value = 'Trajet indisponible pour ce bus.'
  }
}

async function selectVehicle(p_Vehicle: VehicleView) {
  vehicle.value = p_Vehicle
  trip.value = null
  clearInterval(tripTimer)
  await loadTrip()
  tripTimer = setInterval(loadTrip, 30000)
}

function closeVehicle() {
  clearInterval(tripTimer)
  vehicle.value = null
  trip.value = null
  tripError.value = null
}

onMounted(async () => {
  void realtime.start()
  ;[stops.value, lines.value] = await Promise.all([api.stops({ limit: 2000 }).catch(() => []), api.lines().catch(() => [])])
})
onUnmounted(() => {
  clearInterval(tripTimer)
  void realtime.stop()
})
</script>

<template>
  <section class="space-y-3">
    <div class="flex flex-wrap items-center gap-2">
      <h1 class="mr-2 text-2xl font-bold">Carte du réseau</h1>
      <span class="muted" role="status">
        <template v-if="realtime.status && !realtime.status.isStale">{{ realtime.status.vehicleCount }} bus en direct<template v-if="realtime.vehicles.some((p_V) => p_V.isOffRoute)"> · {{ realtime.vehicles.filter((p_V) => p_V.isOffRoute).length }} hors itinéraire</template> · {{ ageLabel(realtime.status.feedAgeSeconds) }}</template>
        <template v-else>Temps réel indisponible : positions non affichées ou anciennes.</template>
        <span v-if="!realtime.connected"> · mises à jour en pause</span>
      </span>
    </div>
    <div class="flex gap-1 overflow-x-auto pb-1" aria-label="Afficher le tracé d’une ligne">
      <button class="btn-ghost shrink-0 border border-slate-200 dark:border-slate-700" :aria-pressed="!selectedLine" @click="selectLine(null)">Aucun tracé</button>
      <button v-for="l in lines" :key="l.id" class="shrink-0 rounded-md p-0.5" :class="{ 'ring-2 ring-brand': selectedLine?.id === l.id }" :aria-pressed="selectedLine?.id === l.id" @click="selectLine(l)">
        <LineBadge :line="l" />
      </button>
    </div>
    <MapPanel :stops="trip ? [] : stops" :vehicles="realtime.vehicles" :shapes="shapes" :shape-color="lineColors(selectedLine).background" height="68vh"
              :trip="tripOverlay" :focus-vehicle-id="liveVehicle?.vehicleId ?? null"
              @select-stop="(id) => router.push({ name: 'stop', params: { id } })" @select-vehicle="selectVehicle" />
    <div v-if="liveVehicle" class="card space-y-2" role="status">
      <div class="flex items-start gap-3">
        <VehicleCard :vehicle="liveVehicle" :show-trip-link="false" class="flex-1" />
        <button class="btn-ghost" @click="closeVehicle">Fermer</button>
      </div>
      <p v-if="trip" class="muted">
        Trajet affiché sur la carte : {{ passedCount }}/{{ trip.stops.length }} arrêts desservis<template v-if="next"> · prochain : {{ next.stopName }}</template>.
        Heures publiées par Ametis : réelles pour les arrêts desservis, estimées pour les suivants.
      </p>
      <p v-else-if="tripError" class="muted">{{ tripError }}</p>
    </div>
  </section>
</template>
