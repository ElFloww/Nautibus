<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { api, ApiError } from '../api/client'

const route = useRoute()
const email = ref('')
const code = ref(typeof route.query.code === 'string' ? route.query.code : '')
const password = ref('')
const done = ref(false)
const busy = ref(false)
const errors = ref<string[]>([])

async function submit() {
  busy.value = true
  errors.value = []
  try {
    await api.resetPassword(email.value, code.value.trim(), password.value)
    done.value = true
  } catch (p_E) {
    errors.value = p_E instanceof ApiError && p_E.status === 400 && p_E.messages.length ? p_E.messages : ['Réinitialisation impossible : code invalide ou expiré.']
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <section class="mx-auto max-w-sm space-y-4">
    <h1 class="text-2xl font-bold">Nouveau mot de passe</h1>
    <div v-if="done" class="card space-y-3" role="status">
      <p>Votre mot de passe a été modifié.</p>
      <RouterLink to="/connexion" class="btn-primary">Se connecter</RouterLink>
    </div>
    <form v-else class="card space-y-3" @submit.prevent="submit">
      <label class="block space-y-1"><span class="text-sm font-medium">E-mail du compte</span>
        <input v-model="email" class="input" type="email" autocomplete="email" required />
      </label>
      <label class="block space-y-1"><span class="text-sm font-medium">Code reçu par e-mail</span>
        <input v-model="code" class="input font-mono" required autocomplete="one-time-code" />
      </label>
      <label class="block space-y-1"><span class="text-sm font-medium">Nouveau mot de passe</span>
        <input v-model="password" class="input" type="password" autocomplete="new-password" required minlength="6" />
      </label>
      <ul v-if="errors.length" class="list-disc pl-5 text-sm text-red-600" role="alert"><li v-for="e in errors" :key="e">{{ e }}</li></ul>
      <button class="btn-primary w-full justify-center" :disabled="busy">{{ busy ? 'Enregistrement…' : 'Changer le mot de passe' }}</button>
    </form>
  </section>
</template>
