<script setup lang="ts">
import { onMounted } from 'vue'
import { RouterLink, RouterView } from 'vue-router'
import { useAuthStore } from './stores/auth'
import { useFavoritesStore } from './stores/favorites'
import { useRealtimeStore } from './stores/realtime'
import { useTheme } from './composables/useTheme'

const auth = useAuthStore()
const favorites = useFavoritesStore()
const realtime = useRealtimeStore()
const { dark, toggle } = useTheme()

onMounted(async () => {
  void realtime.start()
  await auth.load()
  if (auth.user) await favorites.refresh().catch(() => undefined)
  else favorites.clear()
})

const links = [
  { to: '/', label: 'Accueil', desktopOnly: true },
  { to: '/lignes', label: 'Lignes' },
  { to: '/carte', label: 'Carte' },
  { to: '/infos-trafic', label: 'Infos trafic' }
]
</script>

<template>
  <div class="flex min-h-dvh flex-col">
    <a href="#contenu" class="sr-only focus:not-sr-only focus:absolute focus:p-2">Aller au contenu</a>
    <header class="sticky top-0 z-20 border-b border-slate-200 bg-white/90 backdrop-blur dark:border-slate-800 dark:bg-slate-950/90">
      <nav class="mx-auto flex max-w-5xl items-center gap-0.5 px-2 py-2 sm:gap-1 sm:px-4" aria-label="Navigation principale">
        <RouterLink to="/" class="mr-1 flex items-center gap-2 sm:mr-3 font-semibold text-brand dark:text-emerald-300">
          <span aria-hidden="true" class="grid h-7 w-7 place-items-center rounded-lg bg-brand text-xs font-bold text-white">N</span>
          <span class="hidden sm:inline">Nautibus</span>
        </RouterLink>
        <RouterLink v-for="l in links" :key="l.to" :to="l.to" class="btn-ghost px-2 sm:px-3" :class="{ 'max-sm:hidden': l.desktopOnly }" active-class="bg-brand-soft text-brand dark:bg-slate-800 dark:text-emerald-300">
          <span :class="{ 'max-sm:hidden': l.to === '/infos-trafic' }">{{ l.label }}</span><span v-if="l.to === '/infos-trafic'" class="sm:hidden" aria-label="Infos trafic">⚠</span>
          <span v-if="l.to === '/infos-trafic' && realtime.activeEvents.length" class="rounded-full bg-red-600 px-1.5 text-xs text-white" :aria-label="`${realtime.activeEvents.length} en cours`">{{ realtime.activeEvents.length }}</span>
        </RouterLink>
        <span class="flex-1" />
        <button class="btn-ghost" :aria-label="dark ? 'Passer en thème clair' : 'Passer en thème sombre'" @click="toggle">
          <span aria-hidden="true">{{ dark ? '☀' : '☾' }}</span>
        </button>
        <RouterLink v-if="auth.user" to="/compte" class="btn-ghost">Mon compte</RouterLink>
        <RouterLink v-else to="/connexion" class="btn-primary">Connexion</RouterLink>
      </nav>
    </header>

    <main id="contenu" class="mx-auto w-full max-w-5xl flex-1 px-4 py-6">
      <RouterView />
    </main>

    <footer class="border-t border-slate-200 px-4 py-4 text-center text-xs text-slate-500 dark:border-slate-800 dark:text-slate-400">
      Nautibus, application indépendante, non affiliée à Ametis ni à Keolis. Données : Ametis / Amiens Métropole, Licence Ouverte 2.0, via
      <a class="underline" href="https://transport.data.gouv.fr/datasets/ametis" rel="noopener" target="_blank">transport.data.gouv.fr</a>.
      <RouterLink to="/donnees" class="underline">État des données</RouterLink>
    </footer>
  </div>
</template>
