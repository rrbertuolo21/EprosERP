<template>
  <button
    type="button"
    class="theme-toggle"
    :aria-label="'Alternar tema'"
    :title="tema === 'dark' ? 'Mudar para tema claro' : 'Mudar para tema escuro'"
    @click="alternarTema"
  >
    <!-- No escuro mostramos o SOL (indica "ir para o claro"); no claro, a LUA. -->
    <svg
      v-if="tema === 'dark'"
      class="theme-toggle-icon"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
    >
      <circle cx="12" cy="12" r="4" />
      <path d="M12 2v2" />
      <path d="M12 20v2" />
      <path d="m4.93 4.93 1.41 1.41" />
      <path d="m17.66 17.66 1.41 1.41" />
      <path d="M2 12h2" />
      <path d="M20 12h2" />
      <path d="m6.34 17.66-1.41 1.41" />
      <path d="m19.07 4.93-1.41 1.41" />
    </svg>
    <svg
      v-else
      class="theme-toggle-icon"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
    >
      <path d="M12 3a6 6 0 0 0 9 9 9 9 0 1 1-9-9Z" />
    </svg>
  </button>
</template>

<script setup lang="ts">
import { useTheme } from '~/composables/useTheme'

const { tema, alternarTema } = useTheme()
</script>

<style scoped>
.theme-toggle {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 38px;
  height: 38px;
  border-radius: 10px;
  border: 1px solid var(--border-color);
  background: var(--surface-raised);
  color: var(--text-secondary);
  cursor: pointer;
  transition: all 0.2s ease;
  flex-shrink: 0;
}

.theme-toggle:hover {
  color: var(--text-primary);
  background: var(--surface-raised-hover);
  border-color: var(--border-strong);
}

.theme-toggle:focus-visible {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 3px var(--primary-glow);
}

.theme-toggle-icon {
  width: 18px;
  height: 18px;
}
</style>
