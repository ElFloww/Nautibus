<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { api } from '../api/client'
import LineBadge from '../components/LineBadge.vue'
import StatusMessage from '../components/StatusMessage.vue'
import { useAsync } from '../composables/useAsync'

const { data, error, loading, run } = useAsync(() => api.lines())
const filter = ref('')
const shown = computed(() => {
  const v_F = filter.value.trim().toLowerCase()
  return (data.value ?? []).filter((p_L) => !v_F || `${p_L.shortName} ${p_L.longName}`.toLowerCase().includes(v_F))
})
onMounted(run)
</script>

<template>
  <section class="space-y-4">
    <h1 class="text-2xl font-bold">Lignes</h1>
    <label class="block max-w-sm">
      <span class="sr-only">Filtrer les lignes</span>
      <input v-model="filter" class="input" type="search" placeholder="Filtrer (numéro ou nom)" />
    </label>
    <StatusMessage :loading="loading" :error="error" :empty="!loading && !error && !shown.length" empty-text="Aucune ligne." />
    <ul class="grid gap-2 sm:grid-cols-2">
      <li v-for="l in shown" :key="l.id">
        <RouterLink :to="{ name: 'line', params: { id: l.id } }" class="card flex items-center gap-3 hover:border-brand">
          <LineBadge :line="l" large />
          <span>{{ l.longName }}</span>
        </RouterLink>
      </li>
    </ul>
  </section>
</template>
