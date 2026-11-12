<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { api, ApiError } from '../api/client'
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()
const router = useRouter()
const email = ref('')
const password = ref('')
const errors = ref<string[]>([])
const busy = ref(false)
const mustConfirm = ref(false)

async function submit() {
  busy.value = true
  errors.value = []
  try {
    await api.register(email.value, password.value)
  } catch (p_E) {
    errors.value = p_E instanceof ApiError ? p_E.messages : ['Service injoignable.']
    busy.value = false
    return
  }
  try {
    await auth.login(email.value, password.value)
    await router.replace('/')
  } catch (p_E) {
    mustConfirm.value = p_E instanceof ApiError && p_E.message === 'NotAllowed'
    if (mustConfirm.value) return
    errors.value = p_E instanceof ApiError ? p_E.messages : ['Service injoignable.']
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <section class="mx-auto max-w-sm space-y-4">
    <h1 class="text-2xl font-bold">Créer un compte</h1>
    <p v-if="mustConfirm" class="card" role="status">
      Compte créé. Un e-mail de confirmation a été envoyé à {{ email }} : ouvrez le lien qu’il contient, puis connectez-vous.
    </p>
    <form v-else class="card space-y-3" @submit.prevent="submit">
      <label class="block space-y-1"><span class="text-sm font-medium">E-mail</span>
        <input v-model="email" class="input" type="email" autocomplete="email" required />
      </label>
      <label class="block space-y-1"><span class="text-sm font-medium">Mot de passe</span>
        <input v-model="password" class="input" type="password" autocomplete="new-password" required minlength="6" aria-describedby="pwd-help" />
        <span id="pwd-help" class="muted block">Au moins 6 caractères, avec majuscule, minuscule, chiffre et caractère spécial.</span>
      </label>
      <ul v-if="errors.length" class="list-disc pl-5 text-sm text-red-600" role="alert"><li v-for="e in errors" :key="e">{{ e }}</li></ul>
      <button class="btn-primary w-full justify-center" :disabled="busy">{{ busy ? 'Création…' : 'Créer mon compte' }}</button>
    </form>
    <p class="muted">Vos favoris sont liés à votre compte. Vous pouvez supprimer votre compte et toutes ses données à tout moment.</p>
    <p class="muted">Déjà inscrit ? <RouterLink to="/connexion" class="underline">Se connecter</RouterLink></p>
  </section>
</template>
