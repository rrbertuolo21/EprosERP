<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute } from '#app'
import AppSidebarItem from './AppSidebarItem.vue'
import AppIcon from './AppIcon.vue'
import type { MenuGroup } from './menu'

/**
 * AppSidebarGroup — grupo expansível do menu lateral (um módulo do ERP).
 *
 * Contrato:
 *   props: { group: MenuGroup; collapsed?: boolean }
 * Abre automaticamente quando alguma rota filha está ativa.
 */
const props = defineProps<{
  group: MenuGroup
  collapsed?: boolean
}>()

const route = useRoute()

const temFilhoAtivo = computed(() =>
  props.group.itens.some((i) => route.path === i.to || route.path.startsWith(i.to + '/'))
)

const aberto = ref<boolean>(temFilhoAtivo.value)

function alternar() {
  aberto.value = !aberto.value
}
</script>

<template>
  <div class="sidebar-group">
    <button type="button" class="sidebar-group-header" :class="{ active: temFilhoAtivo }" @click="alternar" :title="collapsed ? group.label : undefined">
      <span v-if="group.icon" class="sidebar-group-icon">
        <AppIcon :name="group.icon" :size="18" />
      </span>
      <span v-if="!collapsed" class="sidebar-group-label">{{ group.label }}</span>
      <span v-if="!collapsed" class="sidebar-group-chevron" :class="{ open: aberto }">
        <AppIcon name="chevron-right" :size="14" />
      </span>
    </button>

    <div v-show="aberto && !collapsed" class="sidebar-group-items">
      <AppSidebarItem
        v-for="item in group.itens"
        :key="item.to"
        :label="item.label"
        :to="item.to"
        :icon="item.icon"
      />
    </div>
  </div>
</template>

<style scoped>
.sidebar-group { margin-bottom: 2px; }
.sidebar-group-header {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 9px 12px;
  border: none;
  background: none;
  color: var(--text-secondary);
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  border-radius: 8px;
  transition: all 0.15s ease;
  font-family: inherit;
}
.sidebar-group-header:hover { color: var(--text-primary); background: var(--hover-bg); }
.sidebar-group-header.active { color: var(--text-primary); }
.sidebar-group-icon { width: 18px; display: inline-flex; align-items: center; justify-content: center; flex-shrink: 0; }
.sidebar-group-label { flex: 1; text-align: left; white-space: nowrap; }
.sidebar-group-chevron { display: inline-flex; align-items: center; justify-content: center; transition: transform 0.2s ease; }
.sidebar-group-chevron.open { transform: rotate(90deg); }
.sidebar-group-items { padding-left: 14px; margin: 2px 0 6px; display: flex; flex-direction: column; gap: 2px; }
</style>
