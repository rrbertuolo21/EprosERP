<script setup lang="ts">
/**
 * NfceItensTable — lista dos itens adicionados à NFC-e.
 *
 * Porta o comportamento de `components/pos/list.vue` do legado (lista de itens com
 * total por linha e ações de editar/remover), usando a tabela do design system.
 */
import { computed } from 'vue'
import { useHelper } from '~/composables/useHelper'
import type { NfceItem } from './types'

const props = defineProps<{
  itens: NfceItem[]
  /** Bloqueia edição/remoção (ex.: documento já transmitido). */
  somenteLeitura?: boolean
}>()

const emit = defineEmits<{
  editar: [index: number]
  remover: [index: number]
}>()

const { formatarMoeda, formatarNumero } = useHelper()

const round2 = (v: number) => Math.round((v + Number.EPSILON) * 100) / 100

function totalLinha(item: NfceItem): number {
  return round2(item.quantidadeComercial * item.valorUnitarioComercial - item.valorDesconto)
}

const totalItens = computed(() =>
  props.itens.reduce((acc, item) => acc + totalLinha(item), 0)
)
</script>

<template>
  <div class="itens-table glass-panel">
    <div class="itens-header">
      <span class="itens-titulo">Itens da venda</span>
      <span class="itens-contador">{{ itens.length }} item(ns)</span>
    </div>

    <div class="table-wrap">
      <table class="admin-table">
        <thead>
          <tr>
            <th style="width: 40px">#</th>
            <th>Produto</th>
            <th class="td-right" style="width: 90px">Qtd.</th>
            <th class="td-right" style="width: 120px">Vlr. Unit.</th>
            <th class="td-right" style="width: 110px">Desc.</th>
            <th class="td-right" style="width: 130px">Total</th>
            <th v-if="!somenteLeitura" style="width: 90px">Ações</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="itens.length === 0">
            <td :colspan="somenteLeitura ? 6 : 7">
              <div class="table-empty">Nenhum item adicionado.</div>
            </td>
          </tr>
          <tr v-for="(item, index) in itens" :key="`${item.produtoId}-${index}`">
            <td>{{ index + 1 }}</td>
            <td>
              <div class="produto-cel">
                <span class="produto-desc">{{ item.descricao }}</span>
                <span v-if="item.unidade" class="produto-un">{{ item.unidade }}</span>
              </div>
            </td>
            <td class="td-right">{{ formatarNumero(item.quantidadeComercial, 0, 3) }}</td>
            <td class="td-right">{{ formatarMoeda(item.valorUnitarioComercial) }}</td>
            <td class="td-right">{{ formatarMoeda(item.valorDesconto) }}</td>
            <td class="td-right total-cel">{{ formatarMoeda(totalLinha(item)) }}</td>
            <td v-if="!somenteLeitura" class="td-actions" @click.stop>
              <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click="emit('editar', index)">
                Editar
              </button>
              <button type="button" class="btn btn-ghost btn-sm btn-danger-ghost" title="Remover" @click="emit('remover', index)">
                Remover
              </button>
            </td>
          </tr>
        </tbody>
        <tfoot v-if="itens.length">
          <tr>
            <td :colspan="5" class="td-right foot-label">Total dos itens</td>
            <td class="td-right foot-total">{{ formatarMoeda(totalItens) }}</td>
            <td v-if="!somenteLeitura"></td>
          </tr>
        </tfoot>
      </table>
    </div>
  </div>
</template>

<style scoped>
.itens-table { padding: 12px; }
.itens-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}
.itens-titulo { font-weight: 600; font-size: 14px; }
.itens-contador { font-size: 12px; color: var(--text-muted); }
.produto-cel { display: flex; flex-direction: column; gap: 2px; }
.produto-desc { font-weight: 500; }
.produto-un { font-size: 11px; color: var(--text-muted); }
.total-cel { font-weight: 600; }
.foot-label { color: var(--text-muted); font-size: 12px; }
.foot-total { font-weight: 700; color: var(--primary); }
.td-actions { display: flex; gap: 4px; }
.btn-danger-ghost { color: var(--danger); }
</style>
