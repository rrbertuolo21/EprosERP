<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import AppSidebarGroup from './AppSidebarGroup.vue'
import { erpMenu, type MenuGroup } from './menu'
import { useMenu } from '../composables/useMenu'

/**
 * AppSidebar — navegação lateral do ERP, agrupada por módulo.
 *
 * 1.10 (PERMISSOES_DE_MENU): passa a consumir `GET /api/v1/menu` (via useMenu) e renderizar SÓ os
 * itens visíveis, projetados das capacidades efetivas do RBAC (LC-3/§8.2/REG-002). Enquanto o menu
 * do servidor não chega (ou vem vazio — catálogo ainda sem capacidades semeadas), cai para a árvore
 * estática `erpMenu` como fallback. A prop `menu`, quando passada, tem prioridade (uso em testes/telas).
 */
const props = withDefaults(
  defineProps<{
    menu?: MenuGroup[]
  }>(),
  { menu: undefined }
)

const colapsada = ref(false)
const { grupos, carregar } = useMenu()

onMounted(() => {
  // Best-effort: se falhar, `grupos` fica vazio e o fallback estático assume.
  carregar()
})

// Precedência: prop explícita > menu do servidor (por capacidade) > árvore estática (fallback).
const menuEfetivo = computed<MenuGroup[]>(() => {
  if (props.menu && props.menu.length > 0) return props.menu
  if (grupos.value.length > 0) return grupos.value
  return erpMenu
})
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
        v-for="grupo in menuEfetivo"
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
