<script setup lang="ts">
defineProps<{ loading?: boolean; error?: string | null; empty?: boolean; emptyText?: string }>()
</script>

<template>
  <p v-if="loading" class="muted" role="status">Chargement…</p>
  <p v-else-if="error" class="rounded-lg bg-red-50 p-3 text-sm text-red-700 dark:bg-red-950 dark:text-red-200" role="alert">{{ error }}</p>
  <p v-else-if="empty" class="muted">{{ emptyText ?? 'Aucun résultat.' }}</p>
</template>
