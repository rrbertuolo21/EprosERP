<script setup lang="ts">
/**
 * Layout de PDV/caixa (sem sidebar): barra superior enxuta + área de trabalho ampla.
 * Usado pela tela de PDV (NFC-e em modo caixa) via `definePageMeta({ layout: 'pos' })`.
 */
import { computed, onMounted, ref } from 'vue'
import { useRouter } from '#app'
import ToastHost from '~/components/shared/ToastHost.vue'
import { useAuth, type UsuarioSessao } from '~/composables/useAuth'
import { useTenant } from '~/composables/useTenant'

const router = useRouter()
const { logout } = useAuth()
const { empresaAtiva } = useTenant()

const user = ref<UsuarioSessao | null>(null)
const nomeEmpresa = computed(() => empresaAtiva.value?.nome ?? user.value?.tenantName ?? '')

onMounted(() => {
  const raw = localStorage.getItem('epros_user')
  if (raw) {
    try {
      user.value = JSON.parse(raw) as UsuarioSessao
    } catch {
      user.value = null
    }
  }
})
</script>

<template>
  <div class="pos-shell">
    <header class="pos-bar glass-panel">
      <div class="pos-bar-left">
        <button type="button" class="btn btn-ghost btn-sm" @click="router.back()">← Voltar</button>
        <span class="pos-logo">▲ PDV</span>
      </div>
      <div class="pos-bar-center">
        <span class="pos-empresa">{{ nomeEmpresa }}</span>
      </div>
      <div class="pos-bar-right">
        <span class="user-email">{{ user?.email }}</span>
        <button type="button" class="btn btn-secondary btn-sm" @click="logout">Sair</button>
      </div>
    </header>

    <main class="pos-content">
      <slot />
    </main>

    <ToastHost />
  </div>
</template>

<style scoped>
.pos-shell { display: flex; flex-direction: column; min-height: 100vh; width: 100%; }
.pos-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 12px 20px;
  margin: 16px 20px;
  border-radius: 12px;
}
.pos-bar-left,
.pos-bar-right { display: flex; align-items: center; gap: 12px; }
.pos-logo { font-weight: 700; font-size: 16px; }
.pos-empresa { font-size: 14px; font-weight: 600; color: var(--text-secondary); }
.pos-content { flex: 1; padding: 0 20px 20px; }
.user-email { font-size: 12px; color: var(--text-secondary); }
</style>
