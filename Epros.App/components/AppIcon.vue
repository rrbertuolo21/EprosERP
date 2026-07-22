<script setup lang="ts">
/**
 * Conjunto pequeno de ícones SVG inline (stroke, estilo Lucide/Feather) usados
 * na grade de "Acesso rápido" e em outros pontos do shell. Substitui o uso de
 * emojis do sistema operacional por SVGs consistentes entre plataformas e
 * que herdam `currentColor` (funcionam em ambos os temas).
 */
export type AppIconName =
  | 'invoice'
  | 'receipt'
  | 'cart'
  | 'document'
  | 'box'
  | 'users'
  | 'truck'
  | 'wallet-in'
  | 'wallet-out'
  | 'building'
  | 'chart'
  | 'pos'
  | 'address-book'
  | 'file-invoice'
  | 'shopping-cart'
  | 'package'
  | 'cash'
  | 'report'
  | 'settings'
  | 'lock'
  | 'user'
  | 'chevron-right'
  | 'plus'
  | 'edit'
  | 'trash'
  | 'search'
  | 'filter'
  | 'download'
  | 'upload'
  | 'eye'
  | 'check'
  | 'x'
  | 'logout'
  | 'menu'
  | 'home'

interface Props {
  name: AppIconName
  size?: number
}

const props = withDefaults(defineProps<Props>(), { size: 24 })
</script>

<template>
  <svg
    :width="props.size"
    :height="props.size"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    stroke-width="1.8"
    stroke-linecap="round"
    stroke-linejoin="round"
    aria-hidden="true"
    class="app-icon"
  >
    <template v-if="name === 'invoice'">
      <path d="M7 3h10a1 1 0 0 1 1 1v16l-2.5-1.5L13 20l-2.5-1.5L8 20l-2.5-1.5L3 20V4a1 1 0 0 1 1-1h1" />
      <path d="M8 8h8M8 12h8M8 16h4" />
    </template>

    <template v-else-if="name === 'receipt'">
      <path d="M6 3h12v18l-3-2-3 2-3-2-3 2Z" />
      <path d="M9 8h6M9 12h6" />
    </template>

    <template v-else-if="name === 'cart'">
      <circle cx="9" cy="20" r="1.4" />
      <circle cx="18" cy="20" r="1.4" />
      <path d="M3 4h2l2.4 12.2a2 2 0 0 0 2 1.6h8.4a2 2 0 0 0 2-1.6L21 8H6.2" />
    </template>

    <template v-else-if="name === 'document'">
      <path d="M7 3h7l4 4v14H7Z" />
      <path d="M14 3v4h4" />
      <path d="M9.5 12.5h5M9.5 16h5" />
    </template>

    <template v-else-if="name === 'box'">
      <path d="M3.5 8 12 4l8.5 4L12 12 3.5 8Z" />
      <path d="M3.5 8v8.5L12 20l8.5-3.5V8" />
      <path d="M12 12v8" />
    </template>

    <template v-else-if="name === 'users'">
      <circle cx="9" cy="8" r="3" />
      <path d="M3.5 19c.6-3 2.8-5 5.5-5s4.9 2 5.5 5" />
      <circle cx="17" cy="8" r="2.4" />
      <path d="M15.5 14.2c2.2.4 3.9 2.1 4.5 4.8" />
    </template>

    <template v-else-if="name === 'truck'">
      <rect x="1.5" y="7" width="12" height="9" rx="1" />
      <path d="M13.5 10h4l3 3v3h-7Z" />
      <circle cx="6" cy="18" r="1.6" />
      <circle cx="16.5" cy="18" r="1.6" />
    </template>

    <template v-else-if="name === 'wallet-in'">
      <rect x="3" y="6" width="18" height="13" rx="2" />
      <path d="M3 10h18" />
      <path d="M8 15h2" />
      <path d="M16 3v4M14 5.5 16 3l2 2.5" />
    </template>

    <template v-else-if="name === 'wallet-out'">
      <rect x="3" y="6" width="18" height="13" rx="2" />
      <path d="M3 10h18" />
      <path d="M8 15h2" />
      <path d="M16 7V3M14 4.5 16 7l2-2.5" />
    </template>

    <template v-else-if="name === 'building'">
      <rect x="4" y="3" width="10" height="18" rx="1" />
      <path d="M14 8h6v13h-6" />
      <path d="M7 7h1M10 7h1M7 11h1M10 11h1M7 15h1M10 15h1" />
    </template>

    <template v-else-if="name === 'chart'">
      <path d="M4 20V10M10 20V4M16 20v-7M22 20H2" />
    </template>

    <template v-else-if="name === 'pos'">
      <rect x="4" y="4" width="16" height="11" rx="2" />
      <path d="M8 20h8M9 17v3M15 17v3" />
      <path d="M7.5 8h9" />
    </template>

    <template v-else-if="name === 'address-book'">
      <rect x="5" y="3" width="14" height="18" rx="2" />
      <circle cx="12" cy="10" r="2.2" />
      <path d="M8.5 16c.6-1.7 1.9-2.5 3.5-2.5s2.9.8 3.5 2.5" />
      <path d="M3 8h2M3 12h2M3 16h2" />
    </template>

    <template v-else-if="name === 'file-invoice'">
      <path d="M7 3h8l4 4v14H7Z" />
      <path d="M15 3v4h4" />
      <path d="M9.5 12h5M9.5 15.5h5M9.5 19h3" />
    </template>

    <template v-else-if="name === 'shopping-cart'">
      <circle cx="9" cy="20" r="1.4" />
      <circle cx="18" cy="20" r="1.4" />
      <path d="M3 4h2l2.4 12.2a2 2 0 0 0 2 1.6h8.4a2 2 0 0 0 2-1.6L21 8H6.2" />
    </template>

    <template v-else-if="name === 'package'">
      <path d="M3.5 8 12 4l8.5 4L12 12 3.5 8Z" />
      <path d="M3.5 8v8.5L12 20l8.5-3.5V8" />
      <path d="M12 12v8" />
    </template>

    <template v-else-if="name === 'cash'">
      <rect x="2.5" y="6.5" width="19" height="11" rx="2" />
      <circle cx="12" cy="12" r="2.6" />
      <path d="M6 8v0M18 16v0" />
    </template>

    <template v-else-if="name === 'report'">
      <rect x="4" y="3" width="16" height="18" rx="1.5" />
      <path d="M8 12h3v5H8zM13 8h3v9h-3z" />
    </template>

    <template v-else-if="name === 'settings'">
      <circle cx="12" cy="12" r="3" />
      <path d="M19.4 13.5a1.7 1.7 0 0 0 .3 1.9l.1.1a2 2 0 1 1-2.9 2.9l-.1-.1a1.7 1.7 0 0 0-1.9-.3 1.7 1.7 0 0 0-1 1.6V20a2 2 0 1 1-4 0v-.2a1.7 1.7 0 0 0-1.1-1.6 1.7 1.7 0 0 0-1.9.3l-.1.1a2 2 0 1 1-2.9-2.9l.1-.1a1.7 1.7 0 0 0 .3-1.9 1.7 1.7 0 0 0-1.6-1H4a2 2 0 1 1 0-4h.2a1.7 1.7 0 0 0 1.6-1.1 1.7 1.7 0 0 0-.3-1.9l-.1-.1a2 2 0 1 1 2.9-2.9l.1.1a1.7 1.7 0 0 0 1.9.3H10.5a1.7 1.7 0 0 0 1-1.6V4a2 2 0 1 1 4 0v.2a1.7 1.7 0 0 0 1 1.6 1.7 1.7 0 0 0 1.9-.3l.1-.1a2 2 0 1 1 2.9 2.9l-.1.1a1.7 1.7 0 0 0-.3 1.9V10.5a1.7 1.7 0 0 0 1.6 1H20a2 2 0 1 1 0 4h-.2a1.7 1.7 0 0 0-1.6 1Z" />
    </template>

    <template v-else-if="name === 'lock'">
      <rect x="5" y="11" width="14" height="9" rx="2" />
      <path d="M8 11V7a4 4 0 0 1 8 0v4" />
    </template>

    <template v-else-if="name === 'user'">
      <circle cx="12" cy="8" r="3.5" />
      <path d="M4.5 20c.9-4 3.6-6.5 7.5-6.5s6.6 2.5 7.5 6.5" />
    </template>

    <template v-else-if="name === 'chevron-right'">
      <path d="M9 6l6 6-6 6" />
    </template>

    <template v-else-if="name === 'plus'">
      <path d="M12 5v14M5 12h14" />
    </template>

    <template v-else-if="name === 'edit'">
      <path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4Z" />
    </template>

    <template v-else-if="name === 'trash'">
      <path d="M4 7h16" />
      <path d="M9 7V4h6v3" />
      <path d="M6 7l1 13h10l1-13" />
      <path d="M10 11v6M14 11v6" />
    </template>

    <template v-else-if="name === 'search'">
      <circle cx="10.5" cy="10.5" r="6.5" />
      <path d="M20 20l-4.35-4.35" />
    </template>

    <template v-else-if="name === 'filter'">
      <path d="M4 5h16l-6 7.5V19l-4 2v-8.5Z" />
    </template>

    <template v-else-if="name === 'download'">
      <path d="M12 4v11" />
      <path d="M7.5 11 12 15.5 16.5 11" />
      <path d="M4 19h16" />
    </template>

    <template v-else-if="name === 'upload'">
      <path d="M12 15V4" />
      <path d="M7.5 8 12 3.5 16.5 8" />
      <path d="M4 19h16" />
    </template>

    <template v-else-if="name === 'eye'">
      <path d="M2.5 12S6 5.5 12 5.5 21.5 12 21.5 12 18 18.5 12 18.5 2.5 12 2.5 12Z" />
      <circle cx="12" cy="12" r="2.8" />
    </template>

    <template v-else-if="name === 'check'">
      <path d="M4.5 12.5 9.5 17.5 19.5 7" />
    </template>

    <template v-else-if="name === 'x'">
      <path d="M6 6l12 12M18 6 6 18" />
    </template>

    <template v-else-if="name === 'logout'">
      <path d="M9 4H6a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h3" />
      <path d="M15 16l4-4-4-4" />
      <path d="M19 12H9" />
    </template>

    <template v-else-if="name === 'menu'">
      <path d="M4 6h16M4 12h16M4 18h16" />
    </template>

    <template v-else-if="name === 'home'">
      <path d="M4 11.5 12 4l8 7.5" />
      <path d="M6 10v9h12v-9" />
      <path d="M10 19v-5h4v5" />
    </template>
  </svg>
</template>

<style scoped>
.app-icon {
  display: block;
  flex-shrink: 0;
}
</style>
