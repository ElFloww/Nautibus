import { createApp } from 'vue'
import { createPinia } from 'pinia'
import 'maplibre-gl/dist/maplibre-gl.css'
import './style.css'
import App from './App.vue'
import { router } from './router'
import { registerServiceWorker } from './lib/push'

createApp(App).use(createPinia()).use(router).mount('#app')
void registerServiceWorker()
