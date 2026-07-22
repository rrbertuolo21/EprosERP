<script setup lang="ts">
/**
 * Layout de visitante (não autenticado): login, cadastro, recuperar senha.
 * Sem header/sidebar — apenas centraliza o conteúdo sobre o fundo glass.
 */
import ToastHost from '~/components/shared/ToastHost.vue'
</script>

<template>
  <div class="guest-shell">
    <!-- Camada única de fundo decorativo para todas as rotas guest (login, cadastro,
         recuperar-senha). O app.vue não desenha bg-grid/glow-orb nessas rotas para
         evitar duplicidade de camadas. -->
    <div class="bg-grid"></div>
    <div class="glow-orb glow-1"></div>
    <div class="glow-orb glow-2"></div>

    <slot />
    <ToastHost />
  </div>
</template>

<style scoped>
.guest-shell {
  position: relative;
  min-height: 100vh;
  width: 100%;
  display: flex;
  flex-direction: column;
}
</style>
