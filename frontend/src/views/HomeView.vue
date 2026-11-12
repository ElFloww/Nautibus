<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { api } from '../api/client'
import type { LineSummary, StopSummary } from '../api/types'
import EventCard from '../components/EventCard.vue'
import { useRealtimeStore } from '../stores/realtime'
import LineBadge from '../components/LineBadge.vue'
import StatusMessage from '../components/StatusMessage.vue'
import { useAuthStore } from '../stores/auth'
import { useFavoritesStore } from '../stores/favorites'

const auth = useAuthStore()
const favorites = useFavoritesStore()
const query = ref('')
const lines = ref<LineSummary[]>([])
const stops = ref<StopSummary[]>([])
const nearby = ref<StopSummary[]>([])
const realtime = useRealtimeStore()
const allLines = ref<LineSummary[]>([])
const searching = ref(false)
const error = ref<string | null>(null)
const locating = ref(false)
const favoriteStops = ref<Record<string, string>>({})

let timer: ReturnType<typeof setTimeout> | undefined
watch(query, (p_Q) => {
  clearTimeout(timer)
  if (p_Q.trim().length < 2) { lines.value = []; stops.value = []; return }
  timer = setTimeout(() => search(p_Q.trim()), 250)
})

async function search(p_Q: string) {
  searching.value = true
  error.value = null
  try {
    ;[lines.value, stops.value] = await Promise.all([api.lines(p_Q), api.stops({ q: p_Q, limit: 15 })])
  } catch {
    error.value = 'Recherche impossible pour le moment.'
  } finally {
    searching.value = false
  }
}

function locate() {
  if (!navigator.geolocation) { error.value = 'Géolocalisation non disponible sur cet appareil.'; return }
  locating.value = true
  // La position reste dans le navigateur : elle n'est envoyée que pour cette recherche et n'est pas conservée.
  navigator.geolocation.getCurrentPosition(async (p_Pos) => {
    try {
      nearby.value = await api.stops({ lat: p_Pos.coords.latitude, lon: p_Pos.coords.longitude, radius: 600, limit: 8 })
      if (!nearby.value.length) error.value = 'Aucun arrêt à moins de 600 m.'
    } catch {
      error.value = 'Recherche des arrêts proches impossible.'
    } finally {
      locating.value = false
    }
  }, () => { locating.value = false; error.value = 'Position refusée ou indisponible.' }, { enableHighAccuracy: true, timeout: 10000 })
}

const favoriteLines = computed(() => allLines.value.filter((p_L) => favorites.has('Line', p_L.id)))
const favoriteStopIds = computed(() => favorites.items.filter((p_F) => p_F.kind === 'Stop').map((p_F) => p_F.targetId))

watch(favoriteStopIds, async (p_Ids) => {
  for (const v_Id of p_Ids.filter((p_I) => !favoriteStops.value[p_I])) {
    try { favoriteStops.value[v_Id] = (await api.stop(v_Id)).stop.name } catch { favoriteStops.value[v_Id] = v_Id }
  }
}, { immediate: true })

onMounted(async () => {
  allLines.value = await api.lines().catch(() => [])
})
</script>

<template>
  <section class="space-y-6">
    <div>
      <h1 class="text-2xl font-bold">Où allez-vous ?</h1>
      <p class="muted">Nautibus, le tour d’Amiens en temps réel : horaires, bus en direct et retards sur le réseau Ametis.</p>
    </div>

    <div v-if="realtime.activeEvents.length" class="space-y-2">
      <EventCard v-for="e in realtime.activeEvents.slice(0, 3)" :key="e.id" :event="e" compact />
      <RouterLink to="/infos-trafic" class="text-sm underline">Toutes les infos trafic ({{ realtime.activeEvents.length }})</RouterLink>
    </div>

    <div class="flex flex-col gap-2 sm:flex-row">
      <label class="flex-1">
        <span class="sr-only">Rechercher une ligne ou un arrêt</span>
        <input v-model="query" class="input text-base" type="search" placeholder="Ligne ou arrêt (ex. N1, Gare du Nord)" autocomplete="off" />
      </label>
      <button class="btn-primary justify-center" :disabled="locating" @click="locate">{{ locating ? 'Localisation…' : 'Arrêts autour de moi' }}</button>
    </div>
    <StatusMessage :loading="searching" :error="error" />

    <div v-if="lines.length || stops.length" class="grid gap-4 sm:grid-cols-2">
      <div v-if="lines.length" class="card">
        <h2 class="mb-2 font-semibold">Lignes</h2>
        <ul class="space-y-1">
          <li v-for="l in lines" :key="l.id">
            <RouterLink :to="{ name: 'line', params: { id: l.id } }" class="flex items-center gap-2 rounded p-1 hover:bg-slate-100 dark:hover:bg-slate-800">
              <LineBadge :line="l" /> <span class="truncate">{{ l.longName }}</span>
            </RouterLink>
          </li>
        </ul>
      </div>
      <div v-if="stops.length" class="card">
        <h2 class="mb-2 font-semibold">Arrêts</h2>
        <ul class="space-y-1">
          <li v-for="s in stops" :key="s.id">
            <RouterLink :to="{ name: 'stop', params: { id: s.id } }" class="flex flex-wrap items-center gap-1 rounded p-1 hover:bg-slate-100 dark:hover:bg-slate-800">
              <span class="mr-1">{{ s.name }}</span> <LineBadge v-for="l in s.lines" :key="l.id" :line="l" />
            </RouterLink>
          </li>
        </ul>
      </div>
    </div>

    <div v-if="nearby.length" class="card">
      <h2 class="mb-2 font-semibold">Autour de moi</h2>
      <ul class="space-y-1">
        <li v-for="s in nearby" :key="s.id">
          <RouterLink :to="{ name: 'stop', params: { id: s.id } }" class="flex flex-wrap items-center gap-1 rounded p-1 hover:bg-slate-100 dark:hover:bg-slate-800">
            <span class="mr-1">{{ s.name }}</span> <LineBadge v-for="l in s.lines" :key="l.id" :line="l" />
          </RouterLink>
        </li>
      </ul>
    </div>

    <div class="card">
      <h2 class="mb-2 font-semibold">Mes favoris</h2>
      <p v-if="!auth.user" class="muted"><RouterLink to="/connexion" class="underline">Connectez-vous</RouterLink> pour retrouver vos arrêts et lignes ici.</p>
      <p v-else-if="!favorites.items.length" class="muted">Ajoutez un arrêt ou une ligne avec l’étoile ☆.</p>
      <ul v-else class="flex flex-wrap gap-2">
        <li v-for="l in favoriteLines" :key="l.id"><RouterLink :to="{ name: 'line', params: { id: l.id } }"><LineBadge :line="l" large /></RouterLink></li>
        <li v-for="id in favoriteStopIds" :key="id">
          <RouterLink :to="{ name: 'stop', params: { id } }" class="btn-ghost border border-slate-200 dark:border-slate-700">{{ favoriteStops[id] ?? '…' }}</RouterLink>
        </li>
      </ul>
    </div>
  </section>
</template>
