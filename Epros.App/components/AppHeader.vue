<template>
  <header class="app-header glass-panel">
    <div class="header-left">
      <AppLogo :size="24" />

      <span class="divider">|</span>
      
      <div class="tenant-box" v-if="user">
        <span class="tenant-name">{{ user.tenantName }}</span>
        <span v-if="user.planName" :class="['plan-badge', user.planName.toLowerCase().replace(/ /g, '-')]">
          {{ user.planName }}
        </span>
      </div>
    </div>

    <nav class="header-nav" v-if="user">
      <template v-if="user.status === 'Admin'">
        <NuxtLink to="/plataforma/admin" class="nav-item" active-class="nav-active">
          Painel Landlord
        </NuxtLink>
      </template>
      <template v-else>
        <!-- Permite alternar para o ERP apenas se estiver adimplente -->
        <NuxtLink v-if="user.status !== 'Atrasado'" to="/dashboard" class="nav-item" active-class="nav-active">
          Workspace ERP
        </NuxtLink>
        <NuxtLink to="/area-cliente/minhas-faturas" class="nav-item" active-class="nav-active">
          Minhas Faturas
        </NuxtLink>
        <NuxtLink to="/area-cliente/planos" class="nav-item" active-class="nav-active">
          Planos & Upgrade
        </NuxtLink>
      </template>
    </nav>

    <div class="header-right">
      <ThemeToggle />

      <template v-if="user">
        <div class="status-warning-badge" v-if="user.status === 'Atrasado'">
          <span class="warning-dot"></span>
          Inadimplência
        </div>

        <!-- Seletor de empresa ativa (multiempresa) — só aparece se houver empresas -->
        <EmpresaSelector v-if="temEmpresas" />

        <div class="user-menu" ref="userMenuRef">
          <button type="button" class="user-chip" @click="menuAberto = !menuAberto">
            <span class="user-avatar"><AppIcon name="user" :size="14" /></span>
            <span class="user-email">{{ user.email }}</span>
            <AppIcon name="chevron-right" :size="12" class="chevron" :class="{ open: menuAberto }" />
          </button>
          <div v-if="menuAberto" class="user-dropdown">
            <NuxtLink to="/area-cliente/planos" class="user-dropdown-item" @click="menuAberto = false">
              <AppIcon name="building" :size="15" /> Trocar Empresa
            </NuxtLink>
            <NuxtLink to="/area-cliente/minhas-faturas" class="user-dropdown-item" @click="menuAberto = false">
              <AppIcon name="file-invoice" :size="15" /> Minhas Faturas
            </NuxtLink>
            <button type="button" class="user-dropdown-item user-dropdown-danger" @click="handleLogout">
              <AppIcon name="logout" :size="15" /> Sair
            </button>
          </div>
        </div>
      </template>
    </div>
  </header>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import EmpresaSelector from './shared/EmpresaSelector.vue'
import ThemeToggle from './ThemeToggle.vue'
import AppLogo from './AppLogo.vue'
import AppIcon from './AppIcon.vue'
import { useAuth, type UsuarioSessao } from '../composables/useAuth'

const { logout } = useAuth()
const user = ref<UsuarioSessao | null>(null)
const menuAberto = ref(false)
const userMenuRef = ref<HTMLElement | null>(null)

const temEmpresas = computed(() => (user.value?.empresas?.length ?? 0) > 0)

function aoClicarFora(ev: MouseEvent) {
  if (userMenuRef.value && !userMenuRef.value.contains(ev.target as Node)) {
    menuAberto.value = false
  }
}

onMounted(() => {
  const storedUser = localStorage.getItem('epros_user')
  if (storedUser) {
    try {
      user.value = JSON.parse(storedUser) as UsuarioSessao
    } catch {
      user.value = null
    }
  }
  document.addEventListener('click', aoClicarFora)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', aoClicarFora)
})

const handleLogout = () => {
  menuAberto.value = false
  logout()
}
</script>

<style scoped>
.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 14px 28px;
  margin: 20px;
  border-radius: 14px;
  z-index: 100;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.divider {
  color: var(--text-muted);
  font-size: 14px;
}

.tenant-box {
  display: flex;
  align-items: center;
  gap: 10px;
}

.tenant-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
}

.plan-badge {
  font-size: 10px;
  font-weight: 700;
  padding: 2px 8px;
  border-radius: 4px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  border: 1px solid var(--border-color);
}

.plano-silver {
  background: rgba(161, 161, 170, 0.15);
  color: #e4e4e7;
  border-color: rgba(161, 161, 170, 0.3);
}

.plano-gold {
  background: rgba(234, 179, 8, 0.12);
  color: #fef08a;
  border-color: rgba(234, 179, 8, 0.3);
}

.plano-platinum {
  background: rgba(139, 92, 246, 0.15);
  color: #c084fc;
  border-color: rgba(139, 92, 246, 0.3);
}

.plano-bronze {
  background: rgba(239, 68, 68, 0.1);
  color: #fca5a5;
  border-color: rgba(239, 68, 68, 0.3);
}

.plano-dono-da-plataforma {
  background: rgba(139, 92, 246, 0.2);
  color: #d8b4fe;
  border-color: rgba(139, 92, 246, 0.4);
}

.header-nav {
  display: flex;
  gap: 8px;
}

.nav-item {
  font-size: 13px;
  font-weight: 500;
  color: var(--text-secondary);
  padding: 8px 16px;
  border-radius: 8px;
  transition: all 0.2s ease;
  text-decoration: none;
}

.nav-item:hover {
  color: var(--text-primary);
  background: var(--hover-bg);
}

.nav-active {
  color: var(--primary);
  background: var(--primary-glow);
  border: 1px solid var(--border-color);
}

.header-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.user-menu { position: relative; }

.user-chip {
  display: flex;
  align-items: center;
  gap: 8px;
  background: var(--surface-raised);
  border: 1px solid var(--border-color);
  border-radius: 20px;
  padding: 5px 12px 5px 6px;
  cursor: pointer;
  font-family: inherit;
  transition: background 0.15s ease, border-color 0.15s ease;
}
.user-chip:hover { background: var(--surface-raised-hover); border-color: var(--border-strong); }

.user-avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background: var(--primary-glow);
  color: var(--primary);
  flex-shrink: 0;
}

.user-email {
  font-size: 12px;
  color: var(--text-secondary);
}

.chevron { color: var(--text-muted); transition: transform 0.15s ease; transform: rotate(90deg); }
.chevron.open { transform: rotate(-90deg); }

.user-dropdown {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  min-width: 200px;
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: 10px;
  box-shadow: var(--glass-shadow);
  padding: 6px;
  display: flex;
  flex-direction: column;
  gap: 2px;
  z-index: 50;
}

.user-dropdown-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 9px 10px;
  border-radius: 6px;
  font-size: 13px;
  color: var(--text-primary);
  text-decoration: none;
  background: none;
  border: none;
  cursor: pointer;
  font-family: inherit;
  text-align: left;
  width: 100%;
}
.user-dropdown-item:hover { background: var(--hover-bg); }
.user-dropdown-danger { color: var(--danger); }

.status-warning-badge {
  display: flex;
  align-items: center;
  gap: 6px;
  background: rgba(239, 68, 68, 0.12);
  color: var(--danger);
  border: 1px solid rgba(239, 68, 68, 0.3);
  font-size: 11px;
  font-weight: 600;
  padding: 4px 10px;
  border-radius: 6px;
}

.warning-dot {
  width: 6px;
  height: 6px;
  background-color: var(--danger);
  border-radius: 50%;
  animation: pulse-danger 1.5s infinite alternate;
}

@keyframes pulse-danger {
  0% { transform: scale(1); opacity: 0.5; }
  100% { transform: scale(1.3); opacity: 1; box-shadow: 0 0 8px var(--danger); }
}
</style>
