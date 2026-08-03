<script setup lang="ts">
/**
 * Acesso Restrito — tela exibida quando o usuário autenticado não tem permissão
 * para acessar a rota solicitada.
 *
 * Porta o comportamento de `acesso-restrito.vue` do legado: mensagem de bloqueio +
 * ação de logout via `useAuth().logout()`.
 */
import { useAuth } from '~/composables/useAuth'

definePageMeta({ layout: 'guest' })

const { logout } = useAuth()
</script>

<template>
  <div class="acesso-restrito-wrapper">
    <div class="acesso-restrito-card glass-panel">
      <span class="icone">🔒</span>
      <h1 class="titulo">Acesso restrito</h1>
      <p class="descricao">Entre em contato com o administrador do sistema.</p>
      <button type="button" class="btn btn-primary" @click="logout">
        Sair
      </button>
    </div>
  </div>
</template>

<style scoped>
.acesso-restrito-wrapper {
  width: 100%;
  min-height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 16px;
}
.acesso-restrito-card {
  max-width: 460px;
  width: 100%;
  padding: 40px 32px;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 16px;
}
.icone {
  font-size: 48px;
  line-height: 1;
  opacity: 0.7;
}
.titulo {
  font-size: 18px;
  font-weight: 700;
  color: var(--text-primary);
}
.descricao {
  font-size: 13px;
  color: var(--text-secondary);
  margin-bottom: 8px;
}
</style>
