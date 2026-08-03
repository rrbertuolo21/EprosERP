<script setup lang="ts">
/**
 * PermissaoMenuTree — árvore recursiva de permissões de menu (ver/editar/excluir).
 * Específico da fatia Configurações; substitui o VTreeview do legado.
 *
 * Contrato:
 *   props: { items: MenuPermissaoItem[]; nivel?: number }
 *   Os itens são mutados diretamente (v-model implícito via referência reativa) —
 *   o componente pai passa o array reativo e lê o resultado ao salvar.
 */
export interface MenuPermissaoItem {
  id: number
  titulo: string
  ver: boolean
  editar: boolean
  excluir: boolean
  filhos?: MenuPermissaoItem[]
}

withDefaults(
  defineProps<{
    items: MenuPermissaoItem[]
    nivel?: number
  }>(),
  { nivel: 0 }
)
</script>

<template>
  <ul class="menu-tree" :class="{ raiz: nivel === 0 }">
    <li v-for="item in items" :key="item.id" class="menu-node">
      <div class="menu-row">
        <label class="menu-check">
          <input v-model="item.ver" type="checkbox" />
          <span>{{ item.titulo }}</span>
        </label>
        <div v-if="!item.filhos || item.filhos.length === 0" class="menu-perms">
          <label class="menu-check small">
            <input v-model="item.editar" type="checkbox" />
            <span>Editar</span>
          </label>
          <label class="menu-check small">
            <input v-model="item.excluir" type="checkbox" />
            <span>Excluir</span>
          </label>
        </div>
      </div>
      <PermissaoMenuTree v-if="item.filhos && item.filhos.length > 0" :items="item.filhos" :nivel="nivel + 1" />
    </li>
  </ul>
</template>

<style scoped>
.menu-tree { list-style: none; margin: 0; padding-left: 0; }
.menu-tree:not(.raiz) { padding-left: 22px; border-left: 1px solid var(--border-color); margin-top: 4px; }
.menu-node { padding: 4px 0; }
.menu-row { display: flex; align-items: center; justify-content: space-between; gap: 12px; flex-wrap: wrap; }
.menu-check { display: flex; align-items: center; gap: 6px; font-size: 13px; color: var(--text-primary); cursor: pointer; }
.menu-check.small { font-size: 12px; color: var(--text-secondary); }
.menu-check input { accent-color: var(--primary); width: 15px; height: 15px; cursor: pointer; }
.menu-perms { display: flex; gap: 14px; }
</style>
