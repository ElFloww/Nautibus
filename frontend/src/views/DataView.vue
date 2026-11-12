<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { api } from '../api/client'
import type { DataHealth, FeedVersionInfo, RealtimeStatus } from '../api/types'
import { ageLabel } from '../lib/format'

const health = ref<DataHealth | null>(null)
const feeds = ref<FeedVersionInfo[]>([])
const status = ref<RealtimeStatus | null>(null)
const error = ref<string | null>(null)
const statusText = { Healthy: 'Données à jour', Degraded: 'Données partiellement dégradées', Unhealthy: 'Données indisponibles' }
const sourceText = { Network: 'Réseau (GTFS complet)', Realtime: 'Référence temps réel' }
const importText = { Running: 'en cours', Succeeded: 'réussi', Failed: 'échec', Skipped: 'inchangé' }
const date = new Intl.DateTimeFormat('fr-FR', { dateStyle: 'short', timeStyle: 'short', timeZone: 'Europe/Paris' })

onMounted(async () => {
  try {
    ;[health.value, feeds.value, status.value] = await Promise.all([api.dataHealth(), api.feeds(), api.realtimeStatus()])
  } catch {
    error.value = 'État des données indisponible.'
  }
})
</script>

<template>
  <section class="space-y-4">
    <h1 class="text-2xl font-bold">État des données</h1>
    <p v-if="error" class="text-red-600" role="alert">{{ error }}</p>
    <div v-if="health" class="card">
      <p class="font-semibold">{{ statusText[health.status] }}</p>
      <ul class="muted list-disc pl-5"><li v-for="m in health.messages" :key="m">{{ m }}</li></ul>
    </div>
    <div v-if="status" class="card">
      <h2 class="font-semibold">Temps réel</h2>
      <p class="muted">Dernier flux {{ ageLabel(status.feedAgeSeconds) }} · {{ status.vehicleCount }} véhicules · {{ status.alertCount }} perturbation(s) publiée(s)</p>
    </div>
    <div class="card overflow-x-auto">
      <h2 class="mb-2 font-semibold">Derniers imports</h2>
      <table class="w-full text-left text-sm">
        <thead class="muted"><tr><th class="py-1 pr-3">Source</th><th class="pr-3">Début</th><th class="pr-3">Résultat</th><th class="pr-3">Validité</th><th>Contenu</th></tr></thead>
        <tbody>
          <tr v-for="f in feeds" :key="f.id" class="border-t border-slate-100 dark:border-slate-800">
            <td class="py-1 pr-3">{{ sourceText[f.source] }}<span v-if="f.isActive" class="ml-1 text-xs text-emerald-600">active</span></td>
            <td class="pr-3">{{ date.format(new Date(f.startedAtUtc)) }}</td>
            <td class="pr-3" :class="{ 'text-red-600': f.status === 'Failed' }" :title="f.errorMessage ?? undefined">{{ importText[f.status] }}</td>
            <td class="pr-3">{{ f.validFrom ?? '—' }} → {{ f.validTo ?? '—' }}</td>
            <td>{{ f.routeCount }} lignes, {{ f.stopCount }} arrêts, {{ f.rejectedCount }} rejet(s)</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>
