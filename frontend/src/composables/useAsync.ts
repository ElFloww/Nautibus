import { ref, type Ref } from 'vue'
import { ApiError } from '../api/client'

/** Chargement asynchrone avec état d'attente et message d'erreur lisible. */
export function useAsync<T>(p_Load: () => Promise<T>) {
  const data = ref<T | null>(null) as Ref<T | null>
  const error = ref<string | null>(null)
  const loading = ref(false)
  const notFound = ref(false)

  async function run() {
    loading.value = true
    error.value = null
    notFound.value = false
    try {
      data.value = await p_Load()
    } catch (p_E) {
      notFound.value = p_E instanceof ApiError && p_E.status === 404
      error.value = p_E instanceof ApiError ? p_E.message : 'Service injoignable. Vérifiez votre connexion.'
    } finally {
      loading.value = false
    }
  }

  return { data, error, loading, notFound, run }
}
