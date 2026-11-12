<script setup lang="ts">
import { RouterLink } from 'vue-router'
import type { VehicleView } from '../api/types'
import LineBadge from './LineBadge.vue'
import { ageLabel, confidenceLabel, delayClass, delayLabel } from '../lib/format'

withDefaults(defineProps<{ vehicle: VehicleView; showTripLink?: boolean }>(), { showTripLink: true })
</script>

<template>
  <div class="flex flex-wrap items-center gap-3">
    <LineBadge :line="vehicle.line" large />
    <div class="min-w-0 flex-1">
      <p class="font-semibold">{{ vehicle.destination ? `Vers ${vehicle.destination}` : 'Destination inconnue' }}</p>
      <p class="text-sm">
        <span v-if="vehicle.delaySeconds !== null" class="font-semibold" :class="delayClass(vehicle.delaySeconds)">{{ delayLabel(vehicle.delaySeconds) }}</span>
        <span v-else class="muted">avance/retard inconnu</span>
        <template v-if="vehicle.nextStopName"> · prochain arrêt : {{ vehicle.nextStopName }}</template>
      </p>
      <p class="muted">Bus {{ vehicle.label ?? vehicle.vehicleId }} · position {{ ageLabel(vehicle.ageSeconds) }}<template v-if="vehicle.speedKmh !== null"> · {{ Math.round(vehicle.speedKmh) }} km/h</template><template v-if="vehicle.lineConfidence !== 'High'"> · ligne {{ confidenceLabel[vehicle.lineConfidence] }}</template></p>
      <p v-if="vehicle.isOffRoute" class="text-sm font-semibold text-red-600 dark:text-red-400">Hors itinéraire habituel (≈ {{ Math.round(vehicle.offRouteMeters ?? 0) }} m) : déviation probable, estimation.</p>
      <p v-if="vehicle.isStale" class="text-sm text-amber-700 dark:text-amber-300">Position ancienne : le bus a pu avancer.</p>
      <RouterLink v-if="showTripLink && vehicle.tripId" :to="{ name: 'trip', params: { tripId: vehicle.tripId } }" class="mt-1 inline-block text-sm font-semibold text-brand underline dark:text-emerald-300">
        Voir le trajet : arrêts desservis et à venir
      </RouterLink>
    </div>
  </div>
</template>
