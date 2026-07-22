<script setup lang="ts">
import { ref } from 'vue'
import AppSidebarGroup from './AppSidebarGroup.vue'
import { erpMenu, type MenuGroup } from './menu'

/**
 * AppSidebar — navegação lateral do ERP, agrupada por módulo.
 *
 * Contrato:
 *   props: { menu?: MenuGroup[] }  // default: erpMenu (definido em components/menu.ts)
 * Botão de recolher (rail) controla o modo compacto (só ícones).
 */
withDefaults(
  defineProps<{
    menu?: MenuGroup[]
  }>(),
  { menu: () => erpMenu }
)

const colapsada = ref(false)
</script>

<template>
  <aside class="app-sidebar glass-panel" :class="{ collapsed: colapsada }">
    <div class="sidebar-top">
      <NuxtLink to="/erp/acesso-rapido" class="sidebar-logo">
        <span class="logo-sym">▲</span>
        <span v-if="!colapsada" class="logo-txt">Epros ERP</span>
      </NuxtLink>
      <button type="button" class="sidebar-toggle btn-ghost" @click="colapsada = !colapsada" :title="colapsada ? 'Expandir' : 'Recolher'">
        {{ colapsada ? '»' : '«' }}
      </button>
    </div>

    <nav class="sidebar-nav">
      <AppSidebarGroup
        v-for="grupo in menu"
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
</style>
