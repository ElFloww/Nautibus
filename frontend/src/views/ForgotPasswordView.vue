<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink } from 'vue-router'
import { api } from '../api/client'

const email = ref('')
const sent = ref(false)
const busy = ref(false)
const error = ref<string | null>(null)

async function submit() {
  busy.value = true
  error.value = null
  try {
    await api.forgotPassword(email.value)
    sent.value = true
  } catch {
    error.value = 'Envoi impossible pour le moment. Réessayez plus tard.'
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <section class="mx-auto max-w-sm space-y-4">
    <h1 class="text-2xl font-bold">Mot de passe oublié</h1>
    <p v-if="sent" class="card" role="status">
      Si un compte confirmé existe pour cette adresse, un e-mail contenant un lien de réinitialisation vient d’être envoyé.
    </p>
    <form v-else class="card space-y-3" @submit.prevent="submit">
      <label class="block space-y-1"><span class="text-sm font-medium">E-mail du compte</span>
        <input v-model="email" class="input" type="email" autocomplete="email" required />
      </label>
      <p v-if="error" class="text-sm text-red-600" role="alert">{{ error }}</p>
      <button class="btn-primary w-full justify-center" :disabled="busy">{{ busy ? 'Envoi…' : 'Recevoir un lien' }}</button>
    </form>
    <p class="muted"><RouterLink to="/connexion" class="underline">Retour à la connexion</RouterLink></p>
  </section>
</template>
