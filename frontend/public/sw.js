// Service worker : affichage des notifications push (infos trafic) et ouverture de la page au clic.
self.addEventListener('push', (event) => {
  let data = {}
  try { data = event.data ? event.data.json() : {} } catch { data = { body: event.data ? event.data.text() : '' } }
  event.waitUntil(self.registration.showNotification(data.title || 'Nautibus', {
    body: data.body || '',
    tag: data.tag,
    icon: '/icon.svg',
    badge: '/icon.svg',
    data: { url: data.url || '/' }
  }))
})

self.addEventListener('notificationclick', (event) => {
  event.notification.close()
  const url = (event.notification.data && event.notification.data.url) || '/'
  event.waitUntil(self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then((list) => {
    for (const client of list) {
      if ('focus' in client) {
        client.navigate(url)
        return client.focus()
      }
    }
    return self.clients.openWindow(url)
  }))
})
