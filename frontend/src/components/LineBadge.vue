<script setup lang="ts">
import { computed } from 'vue'
import type { LineSummary } from '../api/types'
import { lineColors } from '../lib/format'

const props = defineProps<{ line: LineSummary | null; large?: boolean }>()
const style = computed(() => lineColors(props.line))
</script>

<template>
  <span
    class="inline-flex min-w-8 items-center justify-center rounded-md px-1.5 font-bold leading-none"
    :class="large ? 'h-10 min-w-10 text-lg' : 'h-6 text-xs'"
    :style="style"
    :title="line?.longName ?? undefined"
  >{{ line?.shortName ?? '?' }}</span>
</template>
