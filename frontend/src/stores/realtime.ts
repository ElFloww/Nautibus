import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr'
import { api } from '../api/client'
import type { NetworkEventView, RealtimeStatus, VehicleView } from '../api/types'

/** Véhicules et infos trafic en direct : chargement initial par l'API, puis mises à jour poussées par SignalR. */
export const useRealtimeStore = defineStore('realtime', () => {
  const vehicles = ref<VehicleView[]>([])
  const events = ref<NetworkEventView[]>([])
  const status = ref<RealtimeStatus | null>(null)
  const connected = ref(false)
  let connection: HubConnection | null = null
  let users = 0

  const activeEvents = computed(() => events.value.filter((p_E) => p_E.isActive))

  async function refreshStatus() {
    try { status.value = await api.realtimeStatus() } catch { status.value = null }
  }

  async function start() {
    users++
    if (connection) return
    connection = new HubConnectionBuilder().withUrl('/hubs/vehicles').withAutomaticReconnect().configureLogging(LogLevel.Warning).build()
    connection.on('vehicles', (p_List: VehicleView[]) => {
      vehicles.value = p_List
      void refreshStatus()
    })
    connection.on('events', (p_List: NetworkEventView[]) => { events.value = p_List })
    connection.onreconnecting(() => { connected.value = false })
    connection.onreconnected(() => { connected.value = true })
    connection.onclose(() => { connected.value = false })
    ;[vehicles.value, events.value] = await Promise.all([api.vehicles().catch(() => []), api.events().catch(() => [])])
    await refreshStatus()
    try {
      await connection.start()
      connected.value = true
    } catch {
      connected.value = false
    }
  }

  async function stop() {
    users = Math.max(0, users - 1)
    if (users > 0 || !connection) return
    const v_Connection = connection
    connection = null
    connected.value = false
    await v_Connection.stop()
  }

  return { vehicles, events, activeEvents, status, connected, start, stop, refreshStatus }
})
