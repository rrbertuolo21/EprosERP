<script setup lang="ts">
/**
 * ItemLista — lista dos itens já adicionados à NF-e simplificada.
 *
 * Porta o comportamento do `components/pos/list.vue` do legado, sem Vuetify.
 * Cada linha permite editar (devolve o item ao formulário) ou remover.
 *
 * Contrato:
 *   props:
 *     itens: ItemVenda[]
 *   emits:
 *     'editar': [indice: number]
 *     'remover': [indice: number]
 */
import { computed } from 'vue'
import { useHelper } from '~/composables/useHelper'
import type { ItemVenda } from './tipos'

const props = defineProps<{
  itens: ItemVenda[]
}>()

defineEmits<{
  editar: [indice: number]
  remover: [indice: number]
}>()

const { formatarMoeda, formatarNumero } = useHelper()

function totalLinha(item: ItemVenda): number {
  const base = item.quantidadeComercial * item.valorUnitarioComercial - item.valorDesconto
  return base > 0 ? Math.round((base + Number.EPSILON) * 100) / 100 : 0
}

const totalGeral = computed(() =>
  props.itens.reduce((acc, item) => acc + totalLinha(item), 0)
)
</script>

<template>
  <div class="item-lista glass-panel">
    <div class="il-header">
      <span class="il-title">Itens da nota</span>
      <span class="il-count">{{ itens.length }} item(ns)</span>
    </div>

    <div v-if="!itens.length" class="il-vazio">Nenhum item adicionado.</div>

    <div v-else class="il-tabela-wrap">
      <table class="admin-table">
        <thead>
          <tr>
            <th>Produto</th>
            <th class="td-right">Qtd.</th>
            <th class="td-right">Vlr. Unit.</th>
            <th class="td-right">Desc.</th>
            <th class="td-right">Total</th>
            <th class="td-actions">Ações</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(item, i) in itens" :key="`${item.produtoId}-${i}`">
            <td>
              <span class="il-prod-cod" v-if="item.produto?.codigo">{{ item.produto.codigo }} · </span>
              {{ item.descricao }}
            </td>
            <td class="td-right">{{ formatarNumero(item.quantidadeComercial) }}</td>
            <td class="td-right">{{ formatarMoeda(item.valorUnitarioComercial) }}</td>
            <td class="td-right">{{ formatarMoeda(item.valorDesconto) }}</td>
            <td class="td-right">{{ formatarMoeda(totalLinha(item)) }}</td>
            <td class="td-actions">
              <button type="button" class="btn btn-ghost btn-sm" @click="$emit('editar', i)">Editar</button>
              <button type="button" class="btn btn-ghost btn-sm" @click="$emit('remover', i)">Remover</button>
            </td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td colspan="4" class="td-right"><strong>Total dos itens</strong></td>
            <td class="td-right"><strong>{{ formatarMoeda(totalGeral) }}</strong></td>
            <td></td>
          </tr>
        </tfoot>
      </table>
    </div>
  </div>
</template>

<style scoped>
.item-lista { padding: 14px; display: flex; flex-direction: column; gap: 12px; }
.il-header { display: flex; align-items: center; justify-content: space-between; }
.il-title { font-weight: 600; font-size: 14px; }
.il-count { font-size: 12px; color: var(--text-muted); }
.il-vazio { color: var(--text-muted); font-size: 13px; padding: 8px 0; }
.il-tabela-wrap { overflow-x: auto; }
.il-prod-cod { color: var(--text-muted); }
.admin-table tfoot td { padding-top: 10px; border-top: 1px solid var(--border-color); }
</style>
