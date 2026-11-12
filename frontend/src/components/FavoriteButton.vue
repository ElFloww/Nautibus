<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import type { FavoriteKind } from '../api/types'
import { useAuthStore } from '../stores/auth'
import { useFavoritesStore } from '../stores/favorites'

const props = defineProps<{ kind: FavoriteKind; targetId: string }>()
const auth = useAuthStore()
const favorites = useFavoritesStore()
const router = useRouter()
const busy = ref(false)
const error = ref<string | null>(null)
const active = computed(() => favorites.has(props.kind, props.targetId))

async function onClick() {
  if (!auth.user) {
    await router.push({ name: 'login', query: { retour: router.currentRoute.value.fullPath } })
    return
  }
  busy.value = true
  error.value = null
  try {
    await favorites.toggle(props.kind, props.targetId)
  } catch {
    error.value = 'Impossible de modifier le favori'
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <span class="inline-flex items-center gap-2">
    <button class="btn-ghost border border-slate-200 dark:border-slate-700" :aria-pressed="active" :disabled="busy" @click="onClick">
      <span aria-hidden="true" :class="active ? 'text-amber-500' : ''">{{ active ? '★' : '☆' }}</span>
      {{ active ? 'Favori' : 'Ajouter aux favoris' }}
    </button>
    <span v-if="error" class="text-sm text-red-600" role="alert">{{ error }}</span>
  </span>
</template>
