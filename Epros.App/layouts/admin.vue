<script setup lang="ts">
/**
 * Layout da área de Administração SaaS (Landlord).
 *
 * Shell com barra lateral à esquerda (AdminSidebar) + topbar próprio + conteúdo.
 * Usado pelas páginas sob `pages/plataforma/admin/*` via `definePageMeta({ layout: 'admin' })`.
 *
 * Guard: exige `epros_token` e `epros_user.status === 'Admin'` no localStorage; caso
 * contrário redireciona para `/` (login).
 */
import { ref, onMounted } from 'vue'
import { navigateTo } from '#app'
import AdminSidebar from '~/components/admin/AdminSidebar.vue'
import ThemeToggle from '~/components/ThemeToggle.vue'
import ToastHost from '~/components/shared/ToastHost.vue'

const email = ref<string>('')

function sair() {
  localStorage.removeItem('epros_token')
  localStorage.removeItem('epros_user')
  localStorage.removeItem('epros_empresa')
  navigateTo('/')
}

onMounted(() => {
  const token = localStorage.getItem('epros_token')
  const rawUser = localStorage.getItem('epros_user')
  let user: { email?: string; status?: string } | null = null
  if (rawUser) {
    try {
      user = JSON.parse(rawUser)
    } catch {
      user = null
    }
  }

  if (!token || !user || user.status !== 'Admin') {
    navigateTo('/')
    return
  }

  email.value = user.email ?? ''
})
</script>

<template>
  <div class="erp-shell">
    <AdminSidebar />
    <div class="erp-main">
      <header class="admin-topbar glass-panel">
        <div class="topbar-left">
          <span class="topbar-title">Administrador</span>
          <span class="topbar-sub">Dono da Plataforma</span>
        </div>
        <div class="topbar-right">
          <ThemeToggle />
          <span v-if="email" class="topbar-email">{{ email }}</span>
          <button type="button" class="btn btn-secondary btn-sair" @click="sair">Sair</button>
        </div>
      </header>
      <main class="erp-content">
        <slot />
      </main>
    </div>
    <ToastHost />
  </div>
</template>

<style scoped>
.admin-topbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 14px 28px;
  margin: 20px;
  border-radius: 14px;
  z-index: 100;
}
.topbar-left { display: flex; flex-direction: column; gap: 1px; }
.topbar-title { font-size: 15px; font-weight: 700; color: var(--text-primary); }
.topbar-sub { font-size: 11.5px; color: var(--text-secondary); }
.topbar-right { display: flex; align-items: center; gap: 14px; }
.topbar-email { font-size: 12.5px; color: var(--text-secondary); }
.btn-sair { padding: 6px 16px; font-size: 12.5px; }
</style>
