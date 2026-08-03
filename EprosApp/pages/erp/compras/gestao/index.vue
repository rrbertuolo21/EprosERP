<script setup lang="ts">
/**
 * Gestão de Compras — hub do fluxo requisição → cotação → pedido (erp/compras/gestao).
 *
 * Tela com abas, camada de apresentação sobre os controllers de Sourcing:
 *   - Requisições (`/api/v1/requisicoes-compra`, GET) — demandas de compra;
 *   - Cotações (`/api/v1/cotacoes-compra`, GET) — atalho para o mapa comparativo (Sourcing);
 *   - Pedidos (`/api/v1/pedidos-compra`, GET) — pedidos de compra emitidos.
 *
 * Criação de requisição/pedido depende de "tipos" (TipoRequisicao/TipoPedido) que não têm
 * endpoint de LISTAGEM (apenas POST de cadastro), portanto a criação inline fica pendente e é
 * anotada; cotação é criada pela tela de Sourcing. Este hub consolida a visão em abas.
 *
 * Endpoints: requisicoes-compra (GET), cotacoes-compra (GET), pedidos-compra (GET).
 */
import { onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

const toast = useToast()
const { formatarData } = useHelper()

type Aba = 'requisicoes' | 'cotacoes' | 'pedidos'
const abaAtiva = ref<Aba>('requisicoes')
const carregado = ref<Record<Aba, boolean>>({ requisicoes: false, cotacoes: false, pedidos: false })

// --- Requisições ---
interface Requisicao { id: string; tipoRequisicaoId: string | null; colaboradorId: string | null; dataRequisicao: string | null; criadoEm: string | null }
const requisicoes = ref<Requisicao[]>([])
const totalReq = ref(0)
const carregandoReq = ref(false)
const colunasReq: DataTableColumn<Requisicao>[] = [
  { key: 'id', label: 'Requisição', formatter: (v) => `#${String(v).slice(0, 8)}` },
  { key: 'colaboradorId', label: 'Colaborador', formatter: (v) => (v ? String(v).slice(0, 8) : '-') },
  { key: 'dataRequisicao', label: 'Data', width: '130px', formatter: (v) => formatarData(v as string | null) },
  { key: 'criadoEm', label: 'Criada em', width: '130px', formatter: (v) => formatarData(v as string | null) }
]

// --- Cotações ---
interface Cotacao { id: string; descricao: string | null; situacao: string | null; dataCotacao: string | null }
const cotacoes = ref<Cotacao[]>([])
const totalCot = ref(0)
const carregandoCot = ref(false)
const colunasCot: DataTableColumn<Cotacao>[] = [
  { key: 'descricao', label: 'Descrição', formatter: (v) => (v as string | null) ?? '-' },
  { key: 'situacao', label: 'Situação', align: 'center', width: '150px', formatter: (v) => (v as string | null) ?? '-' },
  { key: 'dataCotacao', label: 'Data', width: '130px', formatter: (v) => formatarData(v as string | null) }
]

// --- Pedidos ---
interface Pedido { id: string; fornecedorId: string | null; cotacaoId: string | null; dataPedido: string | null; dataPrevistaEntrega: string | null }
const pedidos = ref<Pedido[]>([])
const totalPed = ref(0)
const carregandoPed = ref(false)
const colunasPed: DataTableColumn<Pedido>[] = [
  { key: 'id', label: 'Pedido', formatter: (v) => `#${String(v).slice(0, 8)}` },
  { key: 'fornecedorId', label: 'Fornecedor', formatter: (v) => (v ? `Forn. ${String(v).slice(0, 8)}` : '-') },
  { key: 'cotacaoId', label: 'Cotação', formatter: (v) => (v ? `#${String(v).slice(0, 8)}` : '-') },
  { key: 'dataPedido', label: 'Data', width: '130px', formatter: (v) => formatarData(v as string | null) },
  { key: 'dataPrevistaEntrega', label: 'Prev. entrega', width: '140px', formatter: (v) => formatarData(v as string | null) }
]

async function carregarRequisicoes() {
  carregandoReq.value = true
  try {
    const resposta = await useApi('/requisicoes-compra', { query: { pagina: 1, tamanhoPagina: 50 } })
    const dados = extrairDados<{ total: number; itens: Requisicao[] }>(resposta)
    requisicoes.value = dados?.itens ?? []
    totalReq.value = dados?.total ?? requisicoes.value.length
    carregado.value.requisicoes = true
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregandoReq.value = false
  }
}
async function carregarCotacoes() {
  carregandoCot.value = true
  try {
    const resposta = await useApi('/cotacoes-compra', { query: { pagina: 1, tamanhoPagina: 50 } })
    const dados = extrairDados<{ total: number; itens: Cotacao[] }>(resposta)
    cotacoes.value = dados?.itens ?? []
    totalCot.value = dados?.total ?? cotacoes.value.length
    carregado.value.cotacoes = true
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregandoCot.value = false
  }
}
async function carregarPedidos() {
  carregandoPed.value = true
  try {
    const resposta = await useApi('/pedidos-compra', { query: { pagina: 1, tamanhoPagina: 50 } })
    const dados = extrairDados<{ total: number; itens: Pedido[] }>(resposta)
    pedidos.value = dados?.itens ?? []
    totalPed.value = dados?.total ?? pedidos.value.length
    carregado.value.pedidos = true
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregandoPed.value = false
  }
}

function trocarAba(aba: Aba) {
  abaAtiva.value = aba
  if (aba === 'requisicoes' && !carregado.value.requisicoes) void carregarRequisicoes()
  if (aba === 'cotacoes' && !carregado.value.cotacoes) void carregarCotacoes()
  if (aba === 'pedidos' && !carregado.value.pedidos) void carregarPedidos()
}

function abrirCotacao(c: Cotacao) {
  navigateTo(`/erp/compras/sourcing/${c.id}`)
}

onMounted(() => void carregarRequisicoes())
</script>

<template>
  <div>
    <PageToolbar title="Gestão de Compras" subtitle="Fluxo requisição → cotação → pedido" :loading="carregandoReq || carregandoCot || carregandoPed">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="navigateTo('/erp/compras/sourcing')">Ir ao Sourcing</button>
      </template>
    </PageToolbar>

    <div class="tabs">
      <button type="button" class="tab" :class="{ ativa: abaAtiva === 'requisicoes' }" @click="trocarAba('requisicoes')">Requisições</button>
      <button type="button" class="tab" :class="{ ativa: abaAtiva === 'cotacoes' }" @click="trocarAba('cotacoes')">Cotações</button>
      <button type="button" class="tab" :class="{ ativa: abaAtiva === 'pedidos' }" @click="trocarAba('pedidos')">Pedidos</button>
    </div>

    <div v-show="abaAtiva === 'requisicoes'">
      <DataTable :items="requisicoes" :columns="colunasReq" :total="totalReq" :page="1" :page-size="50" :loading="carregandoReq" empty-text="Nenhuma requisição de compra" />
    </div>

    <div v-show="abaAtiva === 'cotacoes'">
      <DataTable :items="cotacoes" :columns="colunasCot" :total="totalCot" :page="1" :page-size="50" :loading="carregandoCot" empty-text="Nenhuma cotação" @row-click="abrirCotacao">
        <template #actions="{ row }">
          <button type="button" class="btn btn-ghost btn-sm" title="Mapa comparativo" @click.stop="abrirCotacao(row)">📊</button>
        </template>
      </DataTable>
    </div>

    <div v-show="abaAtiva === 'pedidos'">
      <DataTable :items="pedidos" :columns="colunasPed" :total="totalPed" :page="1" :page-size="50" :loading="carregandoPed" empty-text="Nenhum pedido de compra" />
    </div>
  </div>
</template>

<style scoped>
.tabs { display: flex; gap: 4px; margin-bottom: 16px; border-bottom: 1px solid var(--border-color, rgba(255, 255, 255, 0.1)); }
.tab { background: none; border: none; padding: 10px 16px; cursor: pointer; color: var(--text-secondary); font-size: 14px; font-weight: 600; border-bottom: 2px solid transparent; margin-bottom: -1px; }
.tab.ativa { color: var(--primary); border-bottom-color: var(--primary); }
</style>
