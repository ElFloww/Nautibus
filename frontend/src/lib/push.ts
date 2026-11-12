import { api } from '../api/client'

/** Notifications push : navigateur compatible (sur iPhone, seulement si le site est installé sur l'écran d'accueil). */
export function pushSupported(): boolean {
  return 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window
}

export async function registerServiceWorker(): Promise<void> {
  if ('serviceWorker' in navigator) await navigator.serviceWorker.register('/sw.js').catch(() => undefined)
}

export async function currentSubscription(): Promise<PushSubscription | null> {
  if (!pushSupported()) return null
  const v_Registration = await navigator.serviceWorker.getRegistration()
  return (await v_Registration?.pushManager.getSubscription()) ?? null
}

function keyToBytes(p_Key: string): Uint8Array<ArrayBuffer> {
  const v_Base64 = (p_Key + '='.repeat((4 - (p_Key.length % 4)) % 4)).replace(/-/g, '+').replace(/_/g, '/')
  const v_Raw = atob(v_Base64)
  const v_Bytes = new Uint8Array(new ArrayBuffer(v_Raw.length))
  for (let v_I = 0; v_I < v_Raw.length; v_I++) v_Bytes[v_I] = v_Raw.charCodeAt(v_I)
  return v_Bytes
}

/** Abonne ce navigateur ; allLines = toutes les lignes, sinon seulement les lignes favorites. */
export async function subscribePush(p_AllLines: boolean): Promise<void> {
  const v_Key = await api.pushKey()
  if ((await Notification.requestPermission()) !== 'granted') throw new Error('Notifications refusées dans le navigateur.')
  await registerServiceWorker()
  const v_Registration = await navigator.serviceWorker.ready
  const v_Subscription = (await v_Registration.pushManager.getSubscription())
    ?? (await v_Registration.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: keyToBytes(v_Key) }))
  const v_Json = v_Subscription.toJSON()
  await api.subscribePush({ endpoint: v_Subscription.endpoint, p256dh: v_Json.keys?.p256dh ?? '', auth: v_Json.keys?.auth ?? '', allLines: p_AllLines })
  try { localStorage.setItem('pushAllLines', String(p_AllLines)) } catch { /* sans conséquence */ }
}

export async function unsubscribePush(): Promise<void> {
  const v_Subscription = await currentSubscription()
  if (!v_Subscription) return
  await api.unsubscribePush(v_Subscription.endpoint).catch(() => undefined)
  await v_Subscription.unsubscribe()
}

export function savedAllLines(): boolean {
  try { return localStorage.getItem('pushAllLines') !== 'false' } catch { return true }
}
