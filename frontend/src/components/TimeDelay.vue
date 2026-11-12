<script setup lang="ts">
import { computed } from 'vue'
import { clock, delayClass, delayLabel } from '../lib/format'

/**
 * Heure prévue et heure effective. Écart d'au moins une minute : l'heure prévue est barrée et l'écart affiché en couleur.
 * Sans heure effective (pas de temps réel) : horaire théorique, signalé comme tel.
 */
const props = defineProps<{ scheduled: string; expected?: string | null; delay?: number | null; passed?: boolean; stale?: boolean }>()
const realtime = computed(() => !!props.expected)
const shifted = computed(() => realtime.value && Math.abs(props.delay ?? 0) >= 60)
</script>

<template>
  <span class="inline-flex flex-col items-end leading-tight tabular-nums" :class="{ 'opacity-60': stale }">
    <span class="flex items-baseline gap-1.5">
      <span v-if="shifted" class="text-xs text-slate-500 line-through" :title="`Prévu à ${clock(scheduled)}`">{{ clock(scheduled) }}</span>
      <strong :title="realtime ? (passed ? 'Heure de passage publiée par Ametis' : 'Heure estimée par Ametis') : 'Horaire théorique'">{{ clock(expected ?? scheduled) }}</strong>
    </span>
    <span v-if="realtime" class="text-xs font-semibold" :class="delayClass(delay ?? null)">{{ delayLabel(delay ?? null) }}</span>
    <span v-else class="text-xs text-slate-500">théorique</span>
  </span>
</template>
