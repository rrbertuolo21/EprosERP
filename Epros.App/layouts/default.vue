<script setup lang="ts">
/**
 * Layout padrão do ERP: AppHeader + AppSidebar + conteúdo, sobre o fundo glass do app.vue.
 * Usado pelas páginas sob `pages/erp/*` via `definePageMeta({ layout: 'default' })`.
 */
import AppHeader from '~/components/AppHeader.vue'
import AppSidebar from '~/components/AppSidebar.vue'
import ToastHost from '~/components/shared/ToastHost.vue'
</script>

<template>
  <div class="erp-shell">
    <AppSidebar />
    <div class="erp-main">
      <AppHeader />
      <main class="erp-content">
        <slot />
      </main>
    </div>
    <ToastHost />
  </div>
</template>
