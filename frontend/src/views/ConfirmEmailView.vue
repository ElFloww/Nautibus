<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { api } from '../api/client'

const route = useRoute()
const state = ref<'pending' | 'ok' | 'error'>('pending')

onMounted(async () => {
  const { userId, code } = route.query
  if (typeof userId !== 'string' || typeof code !== 'string') { state.value = 'error'; return }
  try {
    await api.confirmEmail(userId, code)
    state.value = 'ok'
  } catch {
    state.value = 'error'
  }
})
</script>

<template>
  <section class="mx-auto max-w-sm space-y-4" role="status">
    <h1 class="text-2xl font-bold">Confirmation de l’adresse</h1>
    <p v-if="state === 'pending'" class="muted">Vérification en cours…</p>
    <template v-else-if="state === 'ok'">
      <p>Votre adresse est confirmée. Vous pouvez vous connecter.</p>
      <RouterLink to="/connexion" class="btn-primary">Se connecter</RouterLink>
    </template>
    <template v-else>
      <p class="text-red-600">Ce lien est invalide ou a expiré.</p>
      <p class="muted">Depuis la page de connexion, vous pouvez demander un nouvel e-mail de confirmation.</p>
      <RouterLink to="/connexion" class="btn-ghost border border-slate-200 dark:border-slate-700">Page de connexion</RouterLink>
    </template>
  </section>
</template>
