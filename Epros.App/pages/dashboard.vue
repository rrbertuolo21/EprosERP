<script setup lang="ts">
/**
 * Dashboard legado (redirect).
 *
 * A tela antiga (~7k linhas, com dados mockados em várias abas: Estoque, Vendas, PDV etc.)
 * foi substituída pela dashboard real do ERP em `/erp/acesso-rapido`, que consome dados
 * de verdade via `useApi`/`useApiList`. Esta rota é mantida apenas como redirecionamento
 * para não quebrar links/bookmarks antigos que apontam para `/dashboard`.
 *
 * O conteúdo original (mocks) foi preservado em `dashboard.vue.bak` neste mesmo diretório
 * — não foi apagado, apenas desativado como rota (Nuxt só roteia arquivos `.vue`).
 */
definePageMeta({
  middleware: 'auth'
})

navigateTo('/erp/acesso-rapido', { replace: true })
</script>

<template>
  <div />
</template>
