<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { GeolocateControl, LngLatBounds, Map as MapLibreMap, NavigationControl, setWorkerUrl, type GeoJSONSource } from 'maplibre-gl'
import workerUrl from 'maplibre-gl/dist/maplibre-gl-worker.mjs?url'
import type { Feature, FeatureCollection } from 'geojson'
import type { LineShape, StopSummary, TripStopView, VehicleView } from '../api/types'
import { delayHex, lineColors, tripStopLabel } from '../lib/format'
import { useTheme } from '../composables/useTheme'

const props = defineProps<{
  stops?: StopSummary[]
  shapes?: LineShape[]
  shapeColor?: string
  vehicles?: VehicleView[]
  center?: [number, number]
  zoom?: number
  height?: string
  /** Course affichée sur la carte : arrêts avec horaires et trace déjà parcourue. key change = nouveau cadrage. */
  trip?: { key: string; color: string; stops: TripStopView[]; trace: [number, number][] } | null
  /** Bus mis en avant : les autres sont estompés. */
  focusVehicleId?: string | null
}>()
const emit = defineEmits<{ selectStop: [id: string]; selectVehicle: [vehicle: VehicleView] }>()

// URL du worker résolue par Vite (dev et build).
setWorkerUrl(workerUrl)

const container = ref<HTMLDivElement>()
const { dark } = useTheme()
let map: MapLibreMap | null = null
let ready = false

// Fonds OpenFreeMap : gratuits, sans clé ni inscription, données OpenStreetMap (attribution incluse dans le style).
function styleUrl(): string {
  return `https://tiles.openfreemap.org/styles/${dark.value ? 'dark' : 'liberty'}`
}

function collection(p_Features: Feature[]): FeatureCollection {
  return { type: 'FeatureCollection', features: p_Features }
}

function stopsData() {
  return collection((props.stops ?? []).map((p_S) => ({
    type: 'Feature', geometry: { type: 'Point', coordinates: [p_S.longitude, p_S.latitude] }, properties: { id: p_S.id, name: p_S.name }
  })))
}

function shapesData() {
  return collection((props.shapes ?? []).map((p_S) => ({
    type: 'Feature', geometry: { type: 'LineString', coordinates: p_S.coordinates }, properties: { id: p_S.shapeId }
  })))
}

function vehiclesData() {
  return collection((props.vehicles ?? []).map((p_V) => ({
    type: 'Feature',
    geometry: { type: 'Point', coordinates: [p_V.longitude, p_V.latitude] },
    properties: {
      id: p_V.vehicleId,
      label: p_V.line?.shortName ?? '?',
      color: lineColors(p_V.line).background,
      text: lineColors(p_V.line).color,
      stale: p_V.isStale,
      dim: !!props.focusVehicleId && p_V.vehicleId !== props.focusVehicleId,
      offRoute: p_V.isOffRoute,
      // Sans cap connu, pas de flèche : on n'invente pas de sens de circulation.
      icon: p_V.bearing === null ? 'bus-dot' : 'bus-arrow',
      bearing: p_V.bearing ?? 0
    }
  })))
}

function tripTraceData() {
  const v_Trace = props.trip?.trace ?? []
  return collection(v_Trace.length > 1 ? [{ type: 'Feature', geometry: { type: 'LineString', coordinates: v_Trace }, properties: {} }] : [])
}

function tripStopsData() {
  return collection((props.trip?.stops ?? []).filter((p_S) => p_S.latitude !== null && p_S.longitude !== null).map((p_S) => ({
    type: 'Feature',
    geometry: { type: 'Point', coordinates: [p_S.longitude!, p_S.latitude!] },
    properties: {
      status: p_S.status,
      label: `${p_S.status === 'Next' ? '▶ ' : ''}${p_S.stopName}\n${tripStopLabel(p_S.scheduledTime, p_S.expectedTime, p_S.delaySeconds, p_S.status === 'Passed')}`,
      delayColor: delayHex(p_S.delaySeconds, dark.value),
      // Priorité d'affichage des étiquettes : prochain arrêt, puis arrêts à venir, puis desservis.
      rank: p_S.status === 'Next' ? 0 : p_S.status === 'Upcoming' ? 1 + p_S.order : 1000 - p_S.order
    }
  })))
}

/** Itinéraires de déviation : positions successives hors tracé des bus concernés. */
function detoursData() {
  return collection((props.vehicles ?? []).filter((p_V) => p_V.isOffRoute && p_V.offRouteTrack.length > 1).map((p_V) => ({
    type: 'Feature', geometry: { type: 'LineString', coordinates: p_V.offRouteTrack }, properties: { id: p_V.vehicleId }
  })))
}

/** Pictogrammes en champ de distance (SDF), colorés à l'affichage : rond seul, ou rond avec pointe dans le sens de marche. */
function busIcon(p_WithArrow: boolean): ImageData {
  const v_Size = 64
  const v_Canvas = document.createElement('canvas')
  v_Canvas.width = v_Canvas.height = v_Size
  const v_Ctx = v_Canvas.getContext('2d')!
  v_Ctx.fillStyle = '#000'
  v_Ctx.beginPath()
  v_Ctx.arc(32, 34, 20, 0, Math.PI * 2)
  v_Ctx.fill()
  if (p_WithArrow) {
    v_Ctx.beginPath()
    v_Ctx.moveTo(32, 1)
    v_Ctx.lineTo(19, 20)
    v_Ctx.lineTo(45, 20)
    v_Ctx.closePath()
    v_Ctx.fill()
  }
  return v_Ctx.getImageData(0, 0, v_Size, v_Size)
}

function addLayers() {
  if (!map) return
  for (const [v_Name, v_Arrow] of [['bus-arrow', true], ['bus-dot', false]] as const)
    if (!map.hasImage(v_Name)) map.addImage(v_Name, busIcon(v_Arrow), { sdf: true, pixelRatio: 2 })

  map.addSource('shapes', { type: 'geojson', data: shapesData() })
  map.addLayer({ id: 'shapes', type: 'line', source: 'shapes', paint: { 'line-color': props.shapeColor ?? '#00594f', 'line-width': 4, 'line-opacity': 0.85 }, layout: { 'line-cap': 'round', 'line-join': 'round' } })
  map.addSource('detours', { type: 'geojson', data: detoursData() })
  map.addLayer({ id: 'detours', type: 'line', source: 'detours', paint: { 'line-color': '#dc2626', 'line-width': 4, 'line-dasharray': [1.5, 1] }, layout: { 'line-cap': 'round', 'line-join': 'round' } })
  const v_TripColor = props.trip?.color ?? '#00594f'
  map.addSource('trip-trace', { type: 'geojson', data: tripTraceData() })
  map.addLayer({ id: 'trip-trace', type: 'line', source: 'trip-trace', paint: { 'line-color': v_TripColor, 'line-width': 6, 'line-opacity': 0.9 }, layout: { 'line-cap': 'round', 'line-join': 'round' } })
  map.addSource('stops', { type: 'geojson', data: stopsData() })
  map.addLayer({ id: 'stops', type: 'circle', source: 'stops', paint: { 'circle-radius': ['interpolate', ['linear'], ['zoom'], 11, 2, 16, 6], 'circle-color': '#ffffff', 'circle-stroke-color': '#00594f', 'circle-stroke-width': 2 } })
  map.addSource('trip-stops', { type: 'geojson', data: tripStopsData() })
  map.addLayer({
    id: 'trip-stops', type: 'circle', source: 'trip-stops',
    paint: {
      'circle-radius': ['match', ['get', 'status'], 'Next', 9, 6],
      'circle-color': ['match', ['get', 'status'], 'Passed', v_TripColor, '#ffffff'],
      'circle-stroke-color': ['match', ['get', 'status'], 'Next', '#16a34a', v_TripColor],
      'circle-stroke-width': ['match', ['get', 'status'], 'Next', 4, 2.5]
    }
  })
  map.addLayer({
    id: 'trip-labels', type: 'symbol', source: 'trip-stops',
    layout: {
      'text-field': ['get', 'label'], 'text-font': ['Noto Sans Bold'], 'text-size': 11, 'text-anchor': 'left', 'text-offset': [0.9, 0],
      'text-justify': 'left', 'symbol-sort-key': ['get', 'rank'], 'text-optional': true
    },
    paint: {
      'text-color': ['get', 'delayColor'],
      'text-halo-color': dark.value ? '#0f172a' : '#ffffff', 'text-halo-width': 2,
      'text-opacity': ['match', ['get', 'status'], 'Passed', 0.75, 1]
    }
  })
  map.addSource('vehicles', { type: 'geojson', data: vehiclesData() })
  map.addLayer({
    id: 'vehicles', type: 'symbol', source: 'vehicles',
    layout: {
      'icon-image': ['get', 'icon'], 'icon-rotate': ['get', 'bearing'], 'icon-rotation-alignment': 'map',
      'icon-allow-overlap': true, 'icon-ignore-placement': true, 'icon-size': 1.1
    },
    paint: {
      'icon-color': ['get', 'color'],
      'icon-opacity': ['case', ['get', 'dim'], 0.25, ['get', 'stale'], 0.35, 1],
      'icon-halo-color': ['case', ['get', 'offRoute'], '#dc2626', '#ffffff'],
      'icon-halo-width': ['case', ['get', 'offRoute'], 4, 1.5]
    }
  })
  map.addLayer({ id: 'vehicle-labels', type: 'symbol', source: 'vehicles', layout: { 'text-field': ['get', 'label'], 'text-size': 10, 'text-font': ['Noto Sans Bold'], 'text-allow-overlap': true, 'text-ignore-placement': true }, paint: { 'text-color': ['get', 'text'], 'text-opacity': ['case', ['get', 'dim'], 0, 1] } })
  ready = true
}

onMounted(() => {
  const v_Map = new MapLibreMap({
    container: container.value!,
    style: styleUrl(),
    center: props.center ?? [2.2957, 49.8942],
    zoom: props.zoom ?? 12.5,
    attributionControl: { compact: true }
  })
  map = v_Map
  v_Map.addControl(new NavigationControl({ showCompass: false }), 'top-right')
  v_Map.addControl(new GeolocateControl({ positionOptions: { enableHighAccuracy: true } }), 'top-right')
  v_Map.on('load', addLayers)
  // Un seul gestionnaire : un bus posé sur un arrêt est prioritaire sur l'arrêt.
  v_Map.on('click', (p_E) => {
    if (!ready) return
    const v_Bus = v_Map.queryRenderedFeatures(p_E.point, { layers: ['vehicles'] })[0]
    if (v_Bus) {
      const v_Vehicle = props.vehicles?.find((p_V) => p_V.vehicleId === v_Bus.properties?.id)
      if (v_Vehicle) emit('selectVehicle', v_Vehicle)
      return
    }
    const v_Stop = v_Map.queryRenderedFeatures(p_E.point, { layers: ['stops'] })[0]
    if (v_Stop?.properties?.id) emit('selectStop', String(v_Stop.properties.id))
  })
  for (const v_Layer of ['stops', 'vehicles']) {
    v_Map.on('mouseenter', v_Layer, () => { v_Map.getCanvas().style.cursor = 'pointer' })
    v_Map.on('mouseleave', v_Layer, () => { v_Map.getCanvas().style.cursor = '' })
  }
})

onBeforeUnmount(() => map?.remove())

function setData(p_Source: string, p_Data: FeatureCollection) {
  if (ready) (map?.getSource(p_Source) as GeoJSONSource | undefined)?.setData(p_Data)
}

watch(() => props.stops, () => setData('stops', stopsData()))
watch(() => props.vehicles, () => {
  setData('vehicles', vehiclesData())
  setData('detours', detoursData())
})
watch(() => props.shapes, () => {
  setData('shapes', shapesData())
  const v_Points = (props.shapes ?? []).flatMap((p_S) => p_S.coordinates)
  if (map && v_Points.length) {
    const v_Bounds = v_Points.reduce((p_B, p_P) => p_B.extend(p_P), new LngLatBounds(v_Points[0], v_Points[0]))
    map.fitBounds(v_Bounds, { padding: 40, maxZoom: 15 })
  }
})
let m_TripKey: string | null = null
watch(() => props.trip, (p_Trip) => {
  setData('trip-trace', tripTraceData())
  setData('trip-stops', tripStopsData())
  if (ready && p_Trip) {
    map?.setPaintProperty('trip-trace', 'line-color', p_Trip.color)
    map?.setPaintProperty('trip-stops', 'circle-stroke-color', ['match', ['get', 'status'], 'Next', '#16a34a', p_Trip.color])
    map?.setPaintProperty('trip-stops', 'circle-color', ['match', ['get', 'status'], 'Passed', p_Trip.color, '#ffffff'])
  }
  // Cadrage seulement quand une autre course est choisie, pas à chaque rafraîchissement.
  const v_Points = (p_Trip?.stops ?? []).filter((p_S) => p_S.latitude !== null).map((p_S) => [p_S.longitude!, p_S.latitude!] as [number, number])
  if (map && p_Trip && p_Trip.key !== m_TripKey && v_Points.length) {
    const v_Bounds = v_Points.reduce((p_B, p_P) => p_B.extend(p_P), new LngLatBounds(v_Points[0], v_Points[0]))
    map.fitBounds(v_Bounds, { padding: { top: 40, bottom: 40, left: 40, right: 200 }, maxZoom: 15 })
  }
  m_TripKey = p_Trip?.key ?? null
})
watch(() => props.focusVehicleId, () => setData('vehicles', vehiclesData()))
watch(() => props.shapeColor, (p_C) => { if (ready && p_C) map?.setPaintProperty('shapes', 'line-color', p_C) })
watch(() => props.center, (p_C) => { if (p_C) map?.easeTo({ center: p_C, zoom: Math.max(map.getZoom(), 15) }) })
watch(dark, () => {
  if (!map) return
  ready = false
  map.setStyle(styleUrl())
  map.once('style.load', addLayers)
})
</script>

<template>
  <div class="space-y-1">
    <div ref="container" class="w-full overflow-hidden rounded-xl border border-slate-200 dark:border-slate-800" :style="{ height: height ?? '420px' }" role="region" aria-label="Carte" />
    <p v-if="vehicles?.length" class="muted flex flex-wrap gap-x-4 text-xs" aria-hidden="true">
      <span>▲ pointe = sens de circulation</span>
      <span>pâle = position ancienne</span>
      <span><span class="text-red-600">◯ rouge</span> = hors itinéraire (déviation probable)</span>
      <span v-if="trip">● plein = arrêt desservi · ◎ vert = prochain arrêt · ○ = à venir · heures : effective (prévue, écart)</span>
    </p>
  </div>
</template>
