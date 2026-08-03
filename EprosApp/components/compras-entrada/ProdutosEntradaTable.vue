<script setup lang="ts">
/**
 * ProdutosEntradaTable — grade de itens da nota de entrada.
 *
 * Substitui o `NfeProdutosCard` do legado por uma tabela simples no design system do novo app.
 * Exibe os itens já incluídos e emite ações de adicionar/editar/remover; os totais são
 * calculados pela tela e recebidos por props.
 */
import { useHelper } from '~/composables/useHelper'
import type { ItemEntrada } from './tipos'

defineProps<{
  itens: ItemEntrada[]
  /** Bloqueia edição (ex.: nota já lançada/cancelada). */
  somenteLeitura?: boolean
}>()

const emit = defineEmits<{
  adicionar: []
  editar: [index: number]
  remover: [index: number]
}>()

const { formatarMoeda, formatarNumero } = useHelper()
</script>

<template>
  <div class="produtos-entrada glass-panel">
    <div class="pe-header">
      <h3 class="pe-title">Produtos</h3>
      <button
        v-if="!somenteLeitura"
        type="button"
        class="btn btn-primary btn-sm"
        @click="emit('adicionar')"
      >
        + Adicionar produto
      </button>
    </div>

    <div class="table-wrap">
      <table class="admin-table">
        <thead>
          <tr>
            <th style="width: 110px">Código</th>
            <th>Descrição</th>
            <th style="width: 90px">NCM</th>
            <th style="width: 70px">UN</th>
            <th class="td-right" style="width: 90px">Qtde</th>
            <th class="td-right" style="width: 110px">Unitário</th>
            <th class="td-right" style="width: 100px">Desconto</th>
            <th class="td-right" style="width: 120px">Total</th>
            <th v-if="!somenteLeitura" class="td-actions">Ações</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="itens.length === 0">
            <td :colspan="somenteLeitura ? 8 : 9">
              <div class="table-empty">Nenhum item adicionado à nota.</div>
            </td>
          </tr>
          <tr v-for="(item, index) in itens" v-else :key="`${item.sku}-${index}`">
            <td>{{ item.sku }}</td>
            <td>{{ item.nomeProduto }}</td>
            <td>{{ item.ncm || '-' }}</td>
            <td>{{ item.unidade || '-' }}</td>
            <td class="td-right">{{ formatarNumero(item.quantidade, 0, 3) }}</td>
            <td class="td-right">{{ formatarMoeda(item.precoUnitario) }}</td>
            <td class="td-right">{{ formatarMoeda(item.valorDesconto ?? 0) }}</td>
            <td class="td-right">{{ formatarMoeda(item.totalItem) }}</td>
            <td v-if="!somenteLeitura" class="td-actions" @click.stop>
              <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click="emit('editar', index)">✎</button>
              <button type="button" class="btn btn-ghost btn-sm" title="Remover" @click="emit('remover', index)">🗑</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.produtos-entrada {
  padding: 12px 16px 8px;
}
.pe-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}
.pe-title {
  font-size: 15px;
  font-weight: 600;
  margin: 0;
}
.td-right {
  text-align: right;
  font-variant-numeric: tabular-nums;
}
</style>
