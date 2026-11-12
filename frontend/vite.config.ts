import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'

// En développement, l'API est servie par le même origine via le proxy : les cookies SameSite=Strict fonctionnent.
const api = process.env.API_URL ?? 'https://localhost:7124'

export default defineConfig({
  plugins: [vue(), tailwindcss()],
  server: {
    port: 5173,
    proxy: {
      '/api': { target: api, secure: false, changeOrigin: true },
      '/hubs': { target: api, secure: false, changeOrigin: true, ws: true },
      '/health': { target: api, secure: false, changeOrigin: true }
    }
  },
  test: { environment: 'node' }
})
