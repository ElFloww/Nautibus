<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import type { NetworkEventView } from '../api/types'
import LineBadge from './LineBadge.vue'
import { confidenceLabel } from '../lib/format'

const props = defineProps<{ event: NetworkEventView; compact?: boolean }>()
const date = new Intl.DateTimeFormat('fr-FR', { weekday: 'short', day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit', timeZone: 'Europe/Paris' })
const official = computed(() => props.event.kind === 'Official')
const ended = computed(() => props.event.closedUtc ?? (props.event.endUtc && !props.event.isActive ? props.event.endUtc : null))
</script>

<template>
  <article class="card space-y-2" :class="event.isActive ? (official ? 'border-l-4 border-l-amber-500' : 'border-l-4 border-l-red-500') : 'opacity-75'">
    <div class="flex flex-wrap items-center gap-2">
      <span class="rounded px-1.5 py-0.5 text-xs font-semibold"
            :class="official ? 'bg-amber-100 text-amber-900 dark:bg-amber-900 dark:text-amber-100' : 'bg-red-100 text-red-900 dark:bg-red-950 dark:text-red-200'">
        {{ official ? 'Info Ametis' : `Détecté · estimation ${confidenceLabel[event.confidence]}` }}
      </span>
      <span v-if="event.isActive" class="text-xs font-semibold text-emerald-700 dark:text-emerald-300">● en cours</span>
      <RouterLink v-for="l in event.lines" :key="l.id" :to="{ name: 'line', params: { id: l.id } }"><LineBadge :line="l" /></RouterLink>
    </div>
    <h3 class="font-semibold">{{ event.title }}</h3>
    <p v-if="event.description && !compact" class="text-sm whitespace-pre-line">{{ event.description }}</p>
    <p class="muted">
      Depuis {{ date.format(new Date(event.startUtc)) }}
      <template v-if="ended"> · terminé {{ date.format(new Date(ended)) }}</template>
      <template v-else-if="event.endUtc"> · fin prévue {{ date.format(new Date(event.endUtc)) }}</template>
    </p>
    <a v-if="event.url && !compact" :href="event.url" class="text-sm underline" target="_blank" rel="noopener">En savoir plus</a>
  </article>
</template>
