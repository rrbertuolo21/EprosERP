<script setup lang="ts">
/**
 * EntradaProdutosCard — lista de itens da NF-e de entrada.
 *
 * Porta o `NfeProdutosCard` do legado para o contexto de compras. Renderiza a tabela de produtos
 * (código/SKU, descrição, NCM, CFOP, CST, unidade, quantidade, unitário, desconto, total, %ICMS, %IPI)
 * com ações de editar/remover e o botão de adicionar. Não faz IO — apenas emite eventos.
 */
import { computed } from 'vue'
import { useHelper } from '~/composables/useHelper'
import type { EntradaItem } from './tipos'

const props = defineProps<{
  itens: EntradaItem[]
  /** Desabilita a inclusão quando não há fornecedor. */
  bloqueado?: boolean
  /** Somente leitura (nota já lançada/autorizada). */
  readonly?: boolean
  erro?: string
}>()

const emit = defineEmits<{
  adicionar: []
  editar: [item: EntradaItem]
  remover: [uid: string]
}>()

const { formatarMoeda, formatarNumero, formatarPorcentagem } = useHelper()

const totalItens = computed(() => props.itens.length)
const totalQuantidade = computed(() => props.itens.reduce((a, i) => a + (i.quantidade || 0), 0))
const totalGeral = computed(() => props.itens.reduce((a, i) => a + (i.total || 0), 0))
</script>

<template>
  <section class="glass-panel nfe-card">
    <header class="nfe-card-header">
      <h2 class="nfe-card-title">Produtos</h2>
      <button
        v-if="!readonly"
        type="button"
        class="btn btn-primary btn-sm"
        :disabled="bloqueado"
        :title="bloqueado ? 'Informe o fornecedor para adicionar produtos' : ''"
        @click="emit('adicionar')"
      >
        + Adicionar produto <span class="atalho">F2</span>
      </button>
    </header>

    <span v-if="erro" class="field-error erro-topo">{{ erro }}</span>

    <div class="table-wrap">
      <table class="admin-table nfe-itens">
        <thead>
          <tr>
            <th>Código</th>
            <th>Descrição</th>
            <th>NCM</th>
            <th>CFOP</th>
            <th>CST/CSOSN</th>
            <th>UN</th>
            <th class="td-right">Qtde</th>
            <th class="td-right">Unitário</th>
            <th class="td-right">Desconto</th>
            <th class="td-right">Total</th>
            <th class="td-right">%ICMS</th>
            <th class="td-right">%IPI</th>
            <th v-if="!readonly" class="td-actions">Ações</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="itens.length === 0">
            <td :colspan="readonly ? 12 : 13" class="table-empty">
              Nenhum produto adicionado à nota.
            </td>
          </tr>
          <tr v-for="i in itens" :key="i._uid">
            <td>{{ i.codigoProduto }}</td>
            <td class="td-nome">{{ i.nomeProduto }}</td>
            <td>{{ i.ncm }}</td>
            <td>{{ i.cfop ?? '' }}</td>
            <td>{{ i.csosnCst }}</td>
            <td>{{ i.unidade }}</td>
            <td class="td-right">{{ formatarNumero(i.quantidade, 0, 3) }}</td>
            <td class="td-right">{{ formatarMoeda(i.valorUnitario) }}</td>
            <td class="td-right">{{ formatarMoeda(i.descontoValor) }}</td>
            <td class="td-right td-total">{{ formatarMoeda(i.total) }}</td>
            <td class="td-right">{{ formatarPorcentagem(i.aliquotaIcms) }}</td>
            <td class="td-right">{{ formatarPorcentagem(i.aliquotaIpi) }}</td>
            <td v-if="!readonly" class="td-actions">
              <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click="emit('editar', i)">✎</button>
              <button type="button" class="btn btn-ghost btn-sm" title="Remover" @click="emit('remover', i._uid)">🗑</button>
            </td>
          </tr>
        </tbody>
        <tfoot v-if="itens.length">
          <tr class="nfe-itens-total">
            <td :colspan="6">Itens: {{ totalItens }}</td>
            <td class="td-right">{{ formatarNumero(totalQuantidade, 0, 3) }}</td>
            <td :colspan="2"></td>
            <td class="td-right td-total">{{ formatarMoeda(totalGeral) }}</td>
            <td :colspan="readonly ? 2 : 3"></td>
          </tr>
        </tfoot>
      </table>
    </div>
  </section>
</template>

<style scoped>
.nfe-card { padding: 18px 20px; margin-bottom: 16px; }
.nfe-card-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; }
.nfe-card-title { font-size: 14px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--text-secondary); }
.atalho { font-size: 10px; opacity: 0.6; margin-left: 4px; }
.erro-topo { display: block; margin-bottom: 8px; }
.nfe-itens { font-size: 12.5px; }
.td-nome { max-width: 260px; }
.td-total { font-weight: 600; color: var(--primary); }
.nfe-itens-total td { font-weight: 700; border-top: 1px solid var(--border-color); }
</style>
