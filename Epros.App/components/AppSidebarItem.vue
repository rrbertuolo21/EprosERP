<script setup lang="ts">
/**
 * AppSidebarItem — item de folha (link) do menu lateral.
 *
 * Contrato:
 *   props: { label: string; to: string; icon?: string; collapsed?: boolean }
 * Destaca-se automaticamente quando a rota atual bate com `to` (via NuxtLink active-class).
 */
defineProps<{
  label: string
  to: string
  icon?: string
  collapsed?: boolean
}>()
</script>

<template>
  <NuxtLink :to="to" class="sidebar-item" active-class="sidebar-item-active" :title="collapsed ? label : undefined">
    <span v-if="icon" class="sidebar-item-icon">{{ icon }}</span>
    <span v-if="!collapsed" class="sidebar-item-label">{{ label }}</span>
  </NuxtLink>
</template>

<style scoped>
.sidebar-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 12px;
  border-radius: 8px;
  color: var(--text-secondary);
  font-size: 13px;
  text-decoration: none;
  transition: all 0.15s ease;
}
.sidebar-item:hover {
  color: var(--text-primary);
  background: var(--hover-bg);
}
.sidebar-item-active {
  color: var(--primary);
  font-weight: 600;
  background: var(--primary-glow);
  border: 1px solid transparent;
  border-left: 3px solid var(--primary);
  padding-left: 9px;
}
.sidebar-item-icon { width: 18px; text-align: center; flex-shrink: 0; }
.sidebar-item-label { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
</style>
