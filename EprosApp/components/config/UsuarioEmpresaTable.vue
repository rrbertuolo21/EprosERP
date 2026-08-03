<script setup lang="ts">
/**
 * UsuarioEmpresaTable — tabela local (não paginada) dos vínculos empresa/perfil do usuário.
 * Específico da fatia Configurações; não é um componente compartilhado.
 *
 * Contrato:
 *   props:
 *     items: UsuarioEmpresaVinculo[]
 *     nomeEmpresa: (empresaId: number) => string
 *     nomePerfil: (perfilId: string | null) => string
 *   emits:
 *     edit: [index: number]
 *     remove: [index: number]
 */
export interface UsuarioEmpresaVinculo {
  empresaId: number | null
  perfilUsuarioId: string | null
  isAdmin: boolean
}

defineProps<{
  items: UsuarioEmpresaVinculo[]
  nomeEmpresa: (empresaId: number | null) => string
  nomePerfil: (perfilId: string | null) => string
}>()

const emit = defineEmits<{
  edit: [index: number]
  remove: [index: number]
}>()
</script>

<template>
  <div class="table-wrap">
    <table class="admin-table">
      <thead>
        <tr>
          <th>Empresa</th>
          <th>Perfil</th>
          <th class="td-center">Admin</th>
          <th class="td-actions">Ações</th>
        </tr>
      </thead>
      <tbody>
        <tr v-if="items.length === 0">
          <td colspan="4">
            <div class="table-empty">Nenhuma empresa vinculada.</div>
          </td>
        </tr>
        <tr v-for="(item, index) in items" v-else :key="index">
          <td>{{ nomeEmpresa(item.empresaId) }}</td>
          <td>{{ item.isAdmin ? '—' : nomePerfil(item.perfilUsuarioId) }}</td>
          <td class="td-center">{{ item.isAdmin ? 'Sim' : 'Não' }}</td>
          <td class="td-actions">
            <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click="emit('edit', index)">✏️</button>
            <button type="button" class="btn btn-ghost btn-sm" title="Remover" @click="emit('remove', index)">🗑️</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.td-center { text-align: center; }
</style>
