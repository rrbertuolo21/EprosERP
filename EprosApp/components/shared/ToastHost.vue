<script setup lang="ts">
/**
 * ToastHost — renderizador global dos toasts. Colocar UMA vez por layout.
 * Lê o estado de `useToast()`; as telas apenas disparam `toast.success(...)` etc.
 * Sem props/emits.
 */
import { useToast, type ToastType } from '~/composables/useToast'

const { toasts, remove } = useToast()

const icone: Record<ToastType, string> = {
  success: '✓',
  error: '✕',
  warning: '⚠',
  info: 'ℹ'
}
</script>

<template>
  <div class="toast-host">
    <div
      v-for="t in toasts"
      :key="t.id"
      class="toast"
      :class="`toast-${t.type}`"
      role="alert"
    >
      <span class="toast-icon">{{ icone[t.type] }}</span>
      <span class="toast-msg">{{ t.message }}</span>
      <button type="button" class="toast-close" aria-label="Fechar" @click="remove(t.id)">×</button>
    </div>
  </div>
</template>

<style scoped>
.toast-icon { font-weight: 700; }
.toast-msg { flex: 1; }
</style>
