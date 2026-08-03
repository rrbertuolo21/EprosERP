<script setup lang="ts">
/**
 * PdvItens — lista (comanda) dos itens da venda no caixa.
 *
 * Porta `components/pos/list.vue`: exibe código, descrição, quantidade, preço unitário,
 * desconto e total por item, com ações de editar/remover. Reconstruído com a tabela
 * `.admin-table` do design system (sem Vuetify). A remoção pede confirmação via ConfirmDialog.
 */
import { computed, ref } from 'vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import { useHelper } from '~/composables/useHelper'
import type { ItemPdv } from './tipos'

const props = defineProps<{
  itens: ItemPdv[]
}>()

const emit = defineEmits<{
  'editar-item': [index: number]
  'remover-item': [index: number]
}>()

const { formatarNumero, formatarMoeda } = useHelper()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const vazio = computed(() => props.itens.length === 0)

function totalItem(item: ItemPdv): number {
  const base = item.quantidadeComercial * (item.valorUnitarioComercial ?? 0)
  return Math.round((base - item.valorDesconto + Number.EPSILON) * 100) / 100
}

async function remover(index: number) {
  const ok = await confirmRef.value?.open(
    'Remover item',
    'Deseja remover este item da venda?',
    { danger: true, textoConfirmar: 'Remover' }
  )
  if (ok) emit('remover-item', index)
}
</script>

<template>
  <div class="pdv-itens glass-panel">
    <div class="itens-wrap">
      <table class="admin-table">
        <thead>
          <tr>
            <th style="width: 40px;">#</th>
            <th>Produto</th>
            <th class="td-right">Qtd.</th>
            <th class="td-right">Preço Unit. / Desc.</th>
            <th class="td-right">Total</th>
            <th class="td-actions">Ações</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="vazio">
            <td colspan="6">
              <div class="itens-vazio">Nenhum item adicionado.</div>
            </td>
          </tr>
          <tr v-for="(item, index) in itens" v-else :key="index">
            <td>{{ index + 1 }}</td>
            <td>
              <div class="item-prod">
                <span class="item-cod">{{ item.produto?.codigo }}</span>
                <span class="item-desc">{{ item.produto?.descricao }}</span>
              </div>
            </td>
            <td class="td-right">
              {{ Number.isInteger(item.quantidadeComercial) ? item.quantidadeComercial : formatarNumero(item.quantidadeComercial, 3, 3) }}
            </td>
            <td class="td-right">
              <div>{{ formatarMoeda(item.valorUnitarioComercial, false) }}</div>
              <div v-if="item.valorDesconto" class="item-desconto">- {{ formatarMoeda(item.valorDesconto, false) }}</div>
            </td>
            <td class="td-right item-total">{{ formatarMoeda(totalItem(item), false) }}</td>
            <td class="td-actions">
              <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click="emit('editar-item', index)">✎</button>
              <button type="button" class="btn btn-ghost btn-sm" title="Remover" @click="remover(index)">🗑</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.pdv-itens { padding: 8px 12px; height: 100%; display: flex; flex-direction: column; }
.itens-wrap { overflow: auto; flex: 1; min-height: 0; }
.itens-vazio { text-align: center; padding: 24px 0; color: var(--text-muted); font-size: 13px; }
.item-prod { display: flex; flex-direction: column; gap: 2px; }
.item-cod { font-weight: 600; font-size: 12px; }
.item-desc { font-size: 12px; color: var(--text-secondary); }
.item-desconto { font-size: 11px; color: var(--danger); }
.item-total { font-weight: 700; }
</style>
