<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useFavoritesStore } from '../stores/favorites'

const auth = useAuthStore()
const favorites = useFavoritesStore()
const router = useRouter()
const confirming = ref(false)
const error = ref<string | null>(null)

onMounted(() => favorites.refresh().catch(() => undefined))

async function logout() {
  await auth.logout().catch(() => undefined)
  favorites.clear()
  await router.replace('/')
}

async function remove() {
  try {
    await auth.deleteAccount()
    favorites.clear()
    await router.replace('/')
  } catch {
    error.value = 'La suppression a échoué. Réessayez.'
  }
}
</script>

<template>
  <section v-if="auth.user" class="mx-auto max-w-lg space-y-4">
    <h1 class="text-2xl font-bold">Mon compte</h1>
    <div class="card space-y-1">
      <p><span class="muted">E-mail :</span> {{ auth.user.email }} <span v-if="!auth.user.emailConfirmed" class="text-xs text-amber-700 dark:text-amber-300">(non confirmée)</span></p>
      <p><span class="muted">Favoris :</span> {{ favorites.items.length }}</p>
      <button class="btn-ghost mt-2 border border-slate-200 dark:border-slate-700" @click="logout">Se déconnecter</button>
    </div>
    <div class="card space-y-2 border-red-200 dark:border-red-900">
      <h2 class="font-semibold">Supprimer mon compte</h2>
      <p class="muted">Le compte et tous vos favoris sont effacés définitivement.</p>
      <button v-if="!confirming" class="btn border border-red-300 text-red-700 dark:text-red-300" @click="confirming = true">Supprimer mon compte…</button>
      <div v-else class="flex flex-wrap gap-2">
        <button class="btn bg-red-600 text-white" @click="remove">Oui, supprimer définitivement</button>
        <button class="btn-ghost" @click="confirming = false">Annuler</button>
      </div>
      <p v-if="error" class="text-sm text-red-600" role="alert">{{ error }}</p>
    </div>
  </section>
</template>
