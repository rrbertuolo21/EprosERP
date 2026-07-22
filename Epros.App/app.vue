<script setup lang="ts">
/**
 * Shell raiz do app. O fundo decorativo (bg-grid + glow-orb) é uma camada ÚNICA
 * por rota: aqui, para o shell autenticado (layout default/pos); nas rotas guest
 * (login/cadastro/recuperar-senha), o layout `guest` é quem desenha o próprio
 * fundo, para não competir visualmente com o painel de marca do login.
 */
import { computed } from 'vue'
import { useRoute } from '#app'

const route = useRoute()
const isGuestRoute = computed(() => route.meta.layout === 'guest')
</script>

<template>
  <div class="app-container">
    <!-- Efeitos de Grid e Glow globais ao fundo — só fora das rotas guest -->
    <template v-if="!isGuestRoute">
      <div class="bg-grid"></div>
      <div class="glow-orb glow-1"></div>
      <div class="glow-orb glow-2"></div>
    </template>

    <!-- Conteúdo Dinâmico das Rotas Nuxt -->
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </div>
</template>

<style>
/* Reset básico de layout global */
.app-container {
  position: relative;
  width: 100vw;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
}
</style>
