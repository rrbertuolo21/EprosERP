<script setup lang="ts">
/**
 * Logotipo do EprosERP — logomark geométrico + wordmark.
 *
 * Substitui o glifo placeholder "▲" usado no login, cadastro e header do shell.
 * Usa exclusivamente as CSS vars de tema (--primary, --accent-purple, --text-primary),
 * portanto funciona sem ajustes nos temas claro e escuro.
 *
 * Props:
 * - `size`: altura do logomark em px (wordmark escala junto via font-size relativo).
 * - `wordmark`: exibe ou não o texto "Epros"/"EprosERP" ao lado do símbolo.
 * - `full`: quando true, exibe "EprosERP"; caso contrário, apenas "Epros".
 * - `monochrome`: quando true, usa `currentColor` no lugar do gradiente (para uso
 *   sobre fundos coloridos, ex.: painel de marca do login).
 */
interface Props {
  size?: number
  wordmark?: boolean
  full?: boolean
  monochrome?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  size: 32,
  wordmark: true,
  full: false,
  monochrome: false
})
</script>

<template>
  <div class="app-logo" :class="{ 'app-logo-mono': props.monochrome }">
    <svg
      class="app-logo-mark"
      :width="props.size"
      :height="props.size"
      viewBox="0 0 32 32"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      aria-hidden="true"
    >
      <defs>
        <linearGradient id="epros-logo-grad" x1="2" y1="2" x2="30" y2="30" gradientUnits="userSpaceOnUse">
          <stop offset="0" stop-color="var(--primary)" />
          <stop offset="1" stop-color="var(--accent-purple)" />
        </linearGradient>
      </defs>
      <rect x="1" y="1" width="30" height="30" rx="9" :fill="props.monochrome ? 'currentColor' : 'url(#epros-logo-grad)'" fill-opacity="0.12" />
      <path
        d="M16 6 L25 11 V21 L16 26 L7 21 V11 Z"
        :stroke="props.monochrome ? 'currentColor' : 'url(#epros-logo-grad)'"
        stroke-width="2"
        stroke-linejoin="round"
        fill="none"
      />
      <path
        d="M16 12 L20.5 14.5 V19.5 L16 22 L11.5 19.5 V14.5 Z"
        :fill="props.monochrome ? 'currentColor' : 'url(#epros-logo-grad)'"
      />
    </svg>

    <span v-if="props.wordmark" class="app-logo-word">
      Epros<span v-if="props.full" class="app-logo-word-suffix">ERP</span>
    </span>
  </div>
</template>

<style scoped>
.app-logo {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  line-height: 1;
}

.app-logo-mark {
  flex-shrink: 0;
  display: block;
}

.app-logo-word {
  font-size: 1.375rem;
  font-weight: 700;
  letter-spacing: -0.02em;
  color: var(--text-primary);
  white-space: nowrap;
}

.app-logo-word-suffix {
  color: var(--text-secondary);
  font-weight: 600;
}

.app-logo-mono .app-logo-word {
  color: currentColor;
}

.app-logo-mono .app-logo-word-suffix {
  color: currentColor;
  opacity: 0.75;
}
</style>
