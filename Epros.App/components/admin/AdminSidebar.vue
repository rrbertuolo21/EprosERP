<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute } from '#app'
import AppSidebarGroup from '~/components/AppSidebarGroup.vue'
import AppIcon from '~/components/AppIcon.vue'
import type { MenuGroup } from '~/components/menu'

/**
 * AdminSidebar — navegação lateral da área de administração SaaS (Landlord).
 *
 * Reaproveita o design glass do ERP (`AppSidebarGroup`/`AppSidebarItem`).
 * O Dashboard é um item de topo (fora de grupo, com destaque exato); os demais
 * itens ficam agrupados conforme a seção (A) do recon Landlord. Cada item aponta
 * para uma rota sob `/plataforma/admin/*`.
 */
const route = useRoute()
const colapsada = ref(false)

const dashboardAtivo = computed(() => route.path === '/plataforma/admin')

const adminMenu: MenuGroup[] = [
  {
    label: 'Cadastros',
    icon: 'building',
    itens: [
      { label: 'Empresas', to: '/plataforma/admin/empresas' },
      { label: 'Clientes', to: '/plataforma/admin/clientes' },
      { label: 'Revendas', to: '/plataforma/admin/revendas' },
      { label: 'Vendedores', to: '/plataforma/admin/vendedores' },
      { label: 'Módulos', to: '/plataforma/admin/modulos' }
    ]
  },
  {
    label: 'Comercial',
    icon: 'cash',
    itens: [
      { label: 'Planos', to: '/plataforma/admin/planos' },
      { label: 'Grupos de Planos', to: '/plataforma/admin/planos-grupos' }
    ]
  },
  {
    label: 'Faturamento',
    icon: 'receipt',
    itens: [
      { label: 'Faturas', to: '/plataforma/admin/faturas' },
      { label: 'Assinaturas', to: '/plataforma/admin/assinaturas' }
    ]
  },
  {
    label: 'Operação',
    icon: 'settings',
    itens: [
      { label: 'Equipe', to: '/plataforma/admin/equipe' },
      { label: 'Configurações', to: '/plataforma/admin/configuracoes' },
      { label: 'Execuções', to: '/plataforma/admin/execucoes' },
      { label: 'Mensagens', to: '/plataforma/admin/mensagens' },
      { label: 'Tarefas', to: '/plataforma/admin/tarefas' }
    ]
  },
  {
    label: 'Sistema',
    icon: 'lock',
    itens: [
      { label: 'Desenvolvedor', to: '/plataforma/admin/desenvolvedor' },
      { label: 'Sobre', to: '/plataforma/admin/sobre' }
    ]
  }
]
</script>

<template>
  <aside class="app-sidebar glass-panel" :class="{ collapsed: colapsada }">
    <div class="sidebar-top">
      <NuxtLink to="/plataforma/admin" class="sidebar-logo">
        <span class="logo-sym">▲</span>
        <span v-if="!colapsada" class="logo-txt">Epros Admin</span>
      </NuxtLink>
      <button
        type="button"
        class="sidebar-toggle btn-ghost"
        @click="colapsada = !colapsada"
        :title="colapsada ? 'Expandir' : 'Recolher'"
      >
        {{ colapsada ? '»' : '«' }}
      </button>
    </div>

    <nav class="sidebar-nav">
      <!-- Dashboard (item de topo, fora de grupo) -->
      <NuxtLink
        to="/plataforma/admin"
        class="sidebar-item dash-item"
        :class="{ 'sidebar-item-active': dashboardAtivo }"
        :title="colapsada ? 'Dashboard' : undefined"
      >
        <span class="sidebar-item-icon"><AppIcon name="home" :size="18" /></span>
        <span v-if="!colapsada" class="sidebar-item-label">Dashboard</span>
      </NuxtLink>

      <AppSidebarGroup
        v-for="grupo in adminMenu"
        :key="grupo.label"
        :group="grupo"
        :collapsed="colapsada"
      />
    </nav>
  </aside>
</template>

<style scoped>
.app-sidebar {
  width: 250px;
  flex-shrink: 0;
  margin: 20px 0 20px 20px;
  padding: 16px 12px;
  border-radius: 14px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  position: sticky;
  top: 20px;
  height: calc(100vh - 40px);
  transition: width 0.2s ease;
}
.app-sidebar.collapsed { width: 72px; }
.sidebar-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding: 0 4px 8px;
  border-bottom: 1px solid var(--border-color);
}
.sidebar-logo { display: flex; align-items: center; gap: 8px; text-decoration: none; color: var(--text-primary); }
.logo-sym {
  font-size: 20px;
  background: linear-gradient(135deg, var(--primary), var(--accent-purple));
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}
.logo-txt { font-size: 16px; font-weight: 700; letter-spacing: -0.3px; }
.sidebar-toggle { font-size: 14px; }
.sidebar-nav {
  display: flex;
  flex-direction: column;
  gap: 2px;
  overflow-y: auto;
  flex: 1;
  padding-right: 2px;
}
.sidebar-nav::-webkit-scrollbar { width: 6px; }
.sidebar-nav::-webkit-scrollbar-thumb { background: var(--border-strong); border-radius: 3px; }

/* Item Dashboard de topo (mesmo visual dos itens de folha) */
.sidebar-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 9px 12px;
  border-radius: 8px;
  color: var(--text-secondary);
  font-size: 13px;
  font-weight: 600;
  text-decoration: none;
  transition: all 0.15s ease;
}
.sidebar-item:hover { color: var(--text-primary); background: var(--hover-bg); }
.sidebar-item-active {
  color: var(--primary);
  background: var(--primary-glow);
  border-left: 3px solid var(--primary);
  padding-left: 9px;
}
.sidebar-item-icon { width: 18px; display: inline-flex; align-items: center; justify-content: center; flex-shrink: 0; }
.sidebar-item-label { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.dash-item { margin-bottom: 4px; }
</style>
