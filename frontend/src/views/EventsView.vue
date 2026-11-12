<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ApiError } from '../api/client'
import EventCard from '../components/EventCard.vue'
import { currentSubscription, pushSupported, savedAllLines, subscribePush, unsubscribePush } from '../lib/push'
import { useAuthStore } from '../stores/auth'
import { useRealtimeStore } from '../stores/realtime'

const realtime = useRealtimeStore()
const auth = useAuthStore()
const supported = pushSupported()
const subscribed = ref(false)
const allLines = ref(savedAllLines())
const busy = ref(false)
const message = ref<string | null>(null)

const past = computed(() => realtime.events.filter((p_E) => !p_E.isActive))

onMounted(async () => {
  void realtime.start()
  subscribed.value = (await currentSubscription()) !== null
})
onUnmounted(() => realtime.stop())

async function toggle() {
  busy.value = true
  message.value = null
  try {
    if (subscribed.value) {
      await unsubscribePush()
      subscribed.value = false
      message.value = 'Notifications désactivées sur cet appareil.'
    } else {
      await subscribePush(allLines.value)
      subscribed.value = true
      message.value = 'Notifications activées sur cet appareil.'
    }
  } catch (p_E) {
    message.value = p_E instanceof ApiError && p_E.status === 503 ? 'Les notifications ne sont pas encore configurées sur ce serveur.'
      : p_E instanceof Error ? p_E.message : 'Activation impossible.'
  } finally {
    busy.value = false
  }
}

async function changeScope() {
  if (subscribed.value) await subscribePush(allLines.value).catch(() => { message.value = 'Mise à jour impossible.' })
}
</script>

<template>
  <section class="space-y-5">
    <div>
      <h1 class="text-2xl font-bold">Infos trafic</h1>
      <p class="muted">Perturbations publiées par Ametis et déviations détectées à partir des positions des bus. Mise à jour en direct.</p>
    </div>

    <div class="card space-y-3">
      <h2 class="font-semibold">Être alerté</h2>
      <p v-if="!supported" class="muted">Ce navigateur ne gère pas les notifications. Sur iPhone, ajoutez d’abord le site à l’écran d’accueil (Partager › Sur l’écran d’accueil), puis ouvrez-le depuis l’icône.</p>
      <p v-else-if="!auth.user" class="muted"><RouterLink to="/connexion" class="underline">Connectez-vous</RouterLink> pour recevoir les infos trafic en notification, même site fermé.</p>
      <template v-else>
        <fieldset class="flex flex-wrap gap-4 text-sm">
          <legend class="sr-only">Lignes concernées</legend>
          <label class="flex items-center gap-2"><input v-model="allLines" type="radio" :value="true" @change="changeScope" /> Toutes les lignes</label>
          <label class="flex items-center gap-2"><input v-model="allLines" type="radio" :value="false" @change="changeScope" /> Mes lignes favorites</label>
        </fieldset>
        <button class="btn" :class="subscribed ? 'btn-ghost border border-slate-200 dark:border-slate-700' : 'btn-primary'" :disabled="busy" @click="toggle">
          {{ subscribed ? 'Désactiver les notifications' : 'Activer les notifications' }}
        </button>
        <p v-if="message" class="text-sm" role="status">{{ message }}</p>
      </template>
    </div>

    <div class="space-y-3">
      <h2 class="font-semibold">En cours ({{ realtime.activeEvents.length }})</h2>
      <p v-if="!realtime.activeEvents.length" class="muted">Aucune perturbation en cours. Ametis ne publie pas toujours ses perturbations dans son flux ouvert : en cas de doute, consultez aussi ametis.fr.</p>
      <EventCard v-for="e in realtime.activeEvents" :key="e.id" :event="e" />
    </div>

    <div v-if="past.length" class="space-y-3">
      <h2 class="font-semibold">7 derniers jours</h2>
      <EventCard v-for="e in past" :key="e.id" :event="e" compact />
    </div>
  </section>
</template>
