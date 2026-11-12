import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api, ApiError } from '../api/client'
import type { AccountInfo } from '../api/types'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<AccountInfo | null>(null)
  let loading: Promise<void> | null = null

  /** Charge le compte une seule fois (cookie de session déjà présent ou non). */
  function load(p_Force = false): Promise<void> {
    if (!loading || p_Force) {
      loading = api.me().then((p_U) => { user.value = p_U }).catch((p_E) => {
        if (!(p_E instanceof ApiError) || p_E.status !== 401) console.error(p_E)
        user.value = null
      })
    }
    return loading
  }

  async function login(p_Email: string, p_Password: string) {
    await api.login(p_Email, p_Password)
    await load(true)
  }

  async function logout() {
    await api.logout()
    user.value = null
  }

  async function deleteAccount() {
    await api.deleteAccount()
    user.value = null
  }

  return { user, load, login, logout, deleteAccount }
})
