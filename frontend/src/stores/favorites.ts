import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { api } from '../api/client'
import type { Favorite, FavoriteKind } from '../api/types'

/** Favoris de l'utilisateur connecté, conservés aussi localement pour l'affichage hors connexion réseau. */
export const useFavoritesStore = defineStore('favorites', () => {
  const items = ref<Favorite[]>(readCache())

  const has = computed(() => (p_Kind: FavoriteKind, p_Id: string) => items.value.some((p_F) => p_F.kind === p_Kind && p_F.targetId === p_Id))

  async function refresh() {
    items.value = await api.favorites()
    writeCache(items.value)
  }

  async function toggle(p_Kind: FavoriteKind, p_Id: string) {
    if (has.value(p_Kind, p_Id)) await api.removeFavorite(p_Kind, p_Id)
    else await api.addFavorite(p_Kind, p_Id)
    await refresh()
  }

  function clear() {
    items.value = []
    writeCache([])
  }

  return { items, has, refresh, toggle, clear }
})

const cacheKey = 'favorites'
function readCache(): Favorite[] {
  try { return JSON.parse(localStorage.getItem(cacheKey) ?? '[]') } catch { return [] }
}
function writeCache(p_Items: Favorite[]) {
  try { localStorage.setItem(cacheKey, JSON.stringify(p_Items)) } catch { /* stockage indisponible : sans conséquence */ }
}
