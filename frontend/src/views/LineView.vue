<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { api } from '../api/client'
import type { LineShape } from '../api/types'
import LineBadge from '../components/LineBadge.vue'
import FavoriteButton from '../components/FavoriteButton.vue'
import MapPanel from '../components/MapPanel.vue'
import StatusMessage from '../components/StatusMessage.vue'
import EventCard from '../components/EventCard.vue'
import VehicleCard from '../components/VehicleCard.vue'
import { useAsync } from '../composables/useAsync'
import { lineColors } from '../lib/format'
import { useRealtimeStore } from '../stores/realtime'

const props = defineProps<{ id: string }>()
const router = useRouter()
const realtime = useRealtimeStore()
const { data, error, loading, notFound, run } = useAsync(() => api.line(props.id))
const shapes = ref<LineShape[]>([])
const direction = ref(0)

const current = computed(() => data.value?.directions[direction.value])
const lineVehicles = computed(() => realtime.vehicles.filter((p_V) => p_V.line?.id === props.id))
// Bus du sens affiché ; sens inconnu : montrés dans les deux pour ne pas les masquer.
const directionVehicles = computed(() => (data.value?.directions.length ?? 0) > 1 && current.value
  ? lineVehicles.value.filter((p_V) => p_V.directionId === null || p_V.directionId === current.value!.directionId)
  : lineVehicles.value)
const lineEvents = computed(() => realtime.activeEvents.filter((p_E) => p_E.lines.some((p_L) => p_L.id === props.id)))

async function load() {
  direction.value = 0
  await run()
  shapes.value = await api.lineShapes(props.id).catch(() => [])
}
watch(() => props.id, load)
onMounted(() => { void load(); void realtime.start() })
onUnmounted(() => realtime.stop())
</script>

<template>
  <section class="space-y-4">
    <StatusMessage :loading="loading" :error="notFound ? 'Cette ligne n’existe pas ou plus.' : error" />
    <template v-if="data">
      <div class="flex flex-wrap items-center gap-3">
        <LineBadge :line="data.line" large />
        <h1 class="text-xl font-bold">{{ data.line.longName }}</h1>
        <FavoriteButton kind="Line" :target-id="data.line.id" />
      </div>

      <EventCard v-for="e in lineEvents" :key="e.id" :event="e" />

      <div v-if="data.directions.length > 1" class="flex flex-wrap gap-2" role="tablist" aria-label="Direction">
        <button v-for="(d, i) in data.directions" :key="d.directionId" role="tab" :aria-selected="direction === i" class="btn border"
                :class="direction === i ? 'border-brand bg-brand-soft text-brand dark:bg-slate-800 dark:text-emerald-300' : 'border-slate-200 dark:border-slate-700'"
                @click="direction = i">→ {{ d.destination }}</button>
      </div>

      <div class="grid gap-4 lg:grid-cols-[1fr_1.3fr]">
        <ol v-if="current" class="card relative space-y-0" aria-label="Arrêts desservis">
          <li v-for="(s, i) in current.stops" :key="`${s.id}-${i}`" class="relative flex items-center gap-3 py-1.5">
            <span class="z-10 h-3 w-3 shrink-0 rounded-full border-2 bg-white dark:bg-slate-900" :style="{ borderColor: lineColors(data.line).background }" aria-hidden="true" />
            <RouterLink :to="{ name: 'stop', params: { id: s.id } }" class="hover:underline">{{ s.name }}</RouterLink>
          </li>
        </ol>
        <div class="space-y-2">
          <MapPanel :shapes="shapes" :shape-color="lineColors(data.line).background" :stops="current?.stops ?? []" :vehicles="directionVehicles"
                    height="480px" @select-stop="(id) => router.push({ name: 'stop', params: { id } })" />
          <div class="card space-y-3">
            <h2 class="font-semibold">Bus en circulation<template v-if="current && data.directions.length > 1"> vers {{ current.destination }}</template> ({{ directionVehicles.length }})</h2>
            <p v-if="!directionVehicles.length" class="muted">Aucun bus visible en direct dans ce sens actuellement.</p>
            <VehicleCard v-for="v in directionVehicles" :key="v.vehicleId" :vehicle="v" class="border-t border-slate-100 pt-3 first-of-type:border-0 first-of-type:pt-0 dark:border-slate-800" />
          </div>
        </div>
      </div>
    </template>
  </section>
</template>
