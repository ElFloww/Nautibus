<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { api, ApiError } from '../api/client'
import { useAuthStore } from '../stores/auth'
import { useFavoritesStore } from '../stores/favorites'

const auth = useAuthStore()
const favorites = useFavoritesStore()
const router = useRouter()
const route = useRoute()
const email = ref('')
const password = ref('')
const error = ref<string | null>(null)
const busy = ref(false)
const unconfirmed = ref(false)
const resent = ref(false)

async function submit() {
  busy.value = true
  error.value = null
  try {
    await auth.login(email.value, password.value)
    await favorites.refresh().catch(() => undefined)
    const v_Back = typeof route.query.retour === 'string' && route.query.retour.startsWith('/') ? route.query.retour : '/'
    await router.replace(v_Back)
  } catch (p_E) {
    unconfirmed.value = p_E instanceof ApiError && p_E.message === 'NotAllowed'
    error.value = !(p_E instanceof ApiError) ? 'Service injoignable.'
      : unconfirmed.value ? 'Adresse e-mail non confirmée : ouvrez le lien reçu par e-mail.'
      : p_E.message === 'LockedOut' ? 'Compte temporairement bloqué après plusieurs échecs. Réessayez plus tard.'
      : p_E.status === 401 ? 'E-mail ou mot de passe incorrect.' : p_E.message
  } finally {
    busy.value = false
  }
}

async function resend() {
  await api.resendConfirmation(email.value).catch(() => undefined)
  resent.value = true
}
</script>

<template>
  <section class="mx-auto max-w-sm space-y-4">
    <h1 class="text-2xl font-bold">Connexion</h1>
    <form class="card space-y-3" @submit.prevent="submit">
      <label class="block space-y-1"><span class="text-sm font-medium">E-mail</span>
        <input v-model="email" class="input" type="email" autocomplete="email" required />
      </label>
      <label class="block space-y-1"><span class="text-sm font-medium">Mot de passe</span>
        <input v-model="password" class="input" type="password" autocomplete="current-password" required />
      </label>
      <p v-if="error" class="text-sm text-red-600" role="alert">{{ error }}</p>
      <p v-if="unconfirmed" class="text-sm">
        <button v-if="!resent" type="button" class="underline" @click="resend">Renvoyer l’e-mail de confirmation</button>
        <span v-else role="status">E-mail renvoyé si le compte existe.</span>
      </p>
      <button class="btn-primary w-full justify-center" :disabled="busy">{{ busy ? 'Connexion…' : 'Se connecter' }}</button>
    </form>
    <p class="muted"><RouterLink to="/mot-de-passe-oublie" class="underline">Mot de passe oublié ?</RouterLink></p>
    <p class="muted">Pas encore de compte ? <RouterLink to="/inscription" class="underline">Créer un compte</RouterLink></p>
  </section>
</template>
