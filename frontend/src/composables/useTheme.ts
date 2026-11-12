import { ref } from 'vue'

const dark = ref(document.documentElement.classList.contains('dark'))

/** Thème clair/sombre, mémorisé pour ce navigateur. */
export function useTheme() {
  function toggle() {
    dark.value = !dark.value
    document.documentElement.classList.toggle('dark', dark.value)
    try { localStorage.setItem('theme', dark.value ? 'dark' : 'light') } catch { /* sans conséquence */ }
  }
  return { dark, toggle }
}
