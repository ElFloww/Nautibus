import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from './stores/auth'

export const router = createRouter({
  history: createWebHistory(),
  scrollBehavior: () => ({ top: 0 }),
  routes: [
    { path: '/', name: 'home', component: () => import('./views/HomeView.vue') },
    { path: '/lignes', name: 'lines', component: () => import('./views/LinesView.vue') },
    { path: '/lignes/:id', name: 'line', component: () => import('./views/LineView.vue'), props: true },
    { path: '/arrets/:id', name: 'stop', component: () => import('./views/StopView.vue'), props: true },
    { path: '/infos-trafic', name: 'events', component: () => import('./views/EventsView.vue') },
    { path: '/courses/:tripId', name: 'trip', component: () => import('./views/TripView.vue'), props: true },
    { path: '/carte', name: 'map', component: () => import('./views/MapView.vue') },
    { path: '/connexion', name: 'login', component: () => import('./views/LoginView.vue') },
    { path: '/inscription', name: 'register', component: () => import('./views/RegisterView.vue') },
    { path: '/confirmation-email', name: 'confirm-email', component: () => import('./views/ConfirmEmailView.vue') },
    { path: '/mot-de-passe-oublie', name: 'forgot-password', component: () => import('./views/ForgotPasswordView.vue') },
    { path: '/mot-de-passe/nouveau', name: 'reset-password', component: () => import('./views/ResetPasswordView.vue') },
    { path: '/compte', name: 'account', component: () => import('./views/AccountView.vue'), meta: { requiresAuth: true } },
    { path: '/donnees', name: 'data', component: () => import('./views/DataView.vue') },
    { path: '/:pathMatch(.*)*', name: 'notfound', component: () => import('./views/NotFoundView.vue') }
  ]
})

router.beforeEach(async (p_To) => {
  if (!p_To.meta.requiresAuth) return true
  const v_Auth = useAuthStore()
  await v_Auth.load()
  return v_Auth.user ? true : { name: 'login', query: { retour: p_To.fullPath } }
})
