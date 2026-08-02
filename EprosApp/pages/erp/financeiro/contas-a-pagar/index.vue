<script setup lang="ts">
/**
 * Listagem de Contas a Pagar (financeiro/contas-a-pagar).
 * Porta o comportamento do módulo Financeiro/CP do legado (tela stub em
 * `financeiro/contas-a-pagar/index.vue`, sem implementação real) usando como referência de
 * domínio o par completo Contas a Receber (`composables/financeiro/useContasAReceber*` +
 * `types/financeiro/recebimento.ts`), espelhando os mesmos campos para o lado "a pagar":
 * busca/filtros server-side, tabela paginada, cards de totais (vencendo hoje/vencido/a vencer),
 * baixa rápida de parcela (registrar pagamento) e exclusão.
 */
import { computed, onMounted, reactive, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import BaixaContaPagarDialog from '~/components/financeiro-pagar/BaixaContaPagarDialog.vue'
import {
  SITUACAO_CONTAS_A_PAGAR,
  situacaoContasAPagarInfo,
  type ContasAPagar,
  type ContasAPagarFiltros,
  type ContasAPagarTotais
} from '~/components/financeiro-pagar/types'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

const toast = useToast()
const { formatarData, formatarMoeda } = useHelper()

const lista = useApiList<ContasAPagar, ContasAPagarFiltros>('/financeiro/contas-pagar', {
  filtrosIniciais: {
    documento: '',
    nomeFornecedor: '',
    situacao: '',
    dataVencimento: ''
  },
  tamanhoPaginaInicial: 25
})

const filtrosForm = ref<Record<string, unknown>>({
  documento: '',
  nomeFornecedor: '',
  situacao: '',
  dataVencimento: ''
})

const opcoesSituacao = [
  { label: 'Aberta', value: SITUACAO_CONTAS_A_PAGAR.ABERTA },
  { label: 'Paga', value: SITUACAO_CONTAS_A_PAGAR.PAGO },
  { label: 'Paga Parcial', value: SITUACAO_CONTAS_A_PAGAR.PAGO_PARCIAL },
  { label: 'Cancelada', value: SITUACAO_CONTAS_A_PAGAR.CANCELADO }
]

const camposFiltro: FilterField[] = [
  { key: 'nomeFornecedor', label: 'Fornecedor', type: 'text', placeholder: 'Nome do fornecedor...', grow: true },
  { key: 'documento', label: 'Documento', type: 'text', placeholder: 'Nº do documento' },
  { key: 'dataVencimento', label: 'Vencimento', type: 'date' },
  { key: 'situacao', label: 'Situação', type: 'select', options: opcoesSituacao }
]

const colunas: DataTableColumn<ContasAPagar>[] = [
  { key: 'documento', label: 'Documento', sortable: true },
  { key: 'nomeFornecedor', label: 'Fornecedor', sortable: true },
  {
    key: 'dataVencimento',
    label: 'Vencimento',
    sortable: true,
    formatter: (v) => formatarData(v as string)
  },
  {
    key: 'valorTitulo',
    label: 'Valor Título',
    sortable: true,
    align: 'right',
    formatter: (v) => formatarMoeda(v as number)
  },
  {
    key: 'valorTotalPago',
    label: 'Valor Pago',
    sortable: false,
    align: 'right',
    formatter: (v) => formatarMoeda(v as number)
  },
  { key: 'situacao', label: 'Situação', sortable: true, align: 'center' }
]

let debounceTimer: ReturnType<typeof setTimeout> | undefined

function aoMudarFiltros(valores: Record<string, unknown>) {
  filtrosForm.value = valores
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => aplicarFiltros(valores), 500)
}

function aoBuscar(valores: Record<string, unknown>) {
  if (debounceTimer) clearTimeout(debounceTimer)
  aplicarFiltros(valores)
}

function aplicarFiltros(valores: Record<string, unknown>) {
  void lista.aplicarFiltros({
    documento: (valores.documento as string) || '',
    nomeFornecedor: (valores.nomeFornecedor as string) || '',
    situacao: (valores.situacao as string) || '',
    dataVencimento: (valores.dataVencimento as string) || ''
  } as Partial<ContasAPagarFiltros>)
}

function aoLimpar() {
  filtrosForm.value = { documento: '', nomeFornecedor: '', situacao: '', dataVencimento: '' }
  void lista.limpar()
}

function criarNovo() {
  navigateTo('/erp/financeiro/contas-a-pagar/novo')
}

function editar(item: ContasAPagar) {
  navigateTo(`/erp/financeiro/contas-a-pagar/${item.id}`)
}

function situacaoDe(item: ContasAPagar) {
  return situacaoContasAPagarInfo(item.situacao)
}

// --- Totais (cards de resumo, espelha /contas-a-receber/totais do legado)
const totais = reactive<ContasAPagarTotais>({
  valorVencendoHoje: 0,
  valorVencido: 0,
  valorAVencer: 0
})
const carregandoTotais = ref(false)

async function carregarTotais() {
  carregandoTotais.value = true
  try {
    const resposta = await useApi('/financeiro/contas-pagar/totais')
    const dados = extrairDados<ContasAPagarTotais>(resposta)
    if (dados) {
      totais.valorVencendoHoje = dados.valorVencendoHoje ?? 0
      totais.valorVencido = dados.valorVencido ?? 0
      totais.valorAVencer = dados.valorAVencer ?? 0
    }
  } catch (e) {
    // Cards de totais são complementares — não bloqueiam a listagem em caso de erro.
    console.error('[financeiro/contas-a-pagar] erro ao carregar totais', e)
  } finally {
    carregandoTotais.value = false
  }
}

// --- Exclusão
const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<ContasAPagar | null>(null)

function pedirExclusao(item: ContasAPagar) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi(`/financeiro/contas-pagar/{id}`, { method: 'DELETE', params: { id: itemParaExcluir.value.id } })
    toast.success('Conta a pagar excluída com sucesso')
    excluirVisivel.value = false
    itemParaExcluir.value = null
    await lista.buscar()
    await carregarTotais()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

const tituloExclusao = computed(() => itemParaExcluir.value?.documento ?? '')

// --- Baixa rápida de parcela (registrar pagamento)
const baixaVisivel = ref(false)
const itemParaBaixa = ref<ContasAPagar | null>(null)

function abrirBaixa(item: ContasAPagar) {
  itemParaBaixa.value = item
  baixaVisivel.value = true
}

async function aoConfirmarBaixa() {
  baixaVisivel.value = false
  itemParaBaixa.value = null
  await lista.buscar()
  await carregarTotais()
}

onMounted(() => {
  void lista.buscar()
  void carregarTotais()
})
</script>

<template>
  <div>
    <PageToolbar title="Contas a Pagar" subtitle="Títulos e obrigações a pagar" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="criarNovo">+ Nova Conta a Pagar</button>
      </template>
    </PageToolbar>

    <div class="cards-resumo">
      <div class="glass-panel card-resumo">
        <span class="card-resumo-label">Vencendo Hoje</span>
        <span class="card-resumo-valor" :class="{ loading: carregandoTotais }">
          {{ formatarMoeda(totais.valorVencendoHoje) }}
        </span>
      </div>
      <div class="glass-panel card-resumo">
        <span class="card-resumo-label">Vencido</span>
        <span class="card-resumo-valor valor-negativo" :class="{ loading: carregandoTotais }">
          {{ formatarMoeda(totais.valorVencido) }}
        </span>
      </div>
      <div class="glass-panel card-resumo">
        <span class="card-resumo-label">A Vencer</span>
        <span class="card-resumo-valor" :class="{ loading: carregandoTotais }">
          {{ formatarMoeda(totais.valorAVencer) }}
        </span>
      </div>
    </div>

    <FilterBar
      :fields="camposFiltro"
      :model-value="filtrosForm"
      :loading="lista.carregando.value"
      @update:model-value="aoMudarFiltros"
      @search="aoBuscar"
      @clear="aoLimpar"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhuma conta a pagar encontrada"
      @update:page="(p) => lista.irParaPagina(p)"
      @update:page-size="(ps) => lista.buscar({ tamanhoPagina: ps, pagina: 1 })"
      @update:sort="(s) => lista.buscar({ ordenacao: s })"
      @row-click="editar"
    >
      <template #cell-situacao="{ row }">
        <span class="badge" :class="`badge-${situacaoDe(row).classe}`">{{ situacaoDe(row).texto }}</span>
      </template>

      <template #actions="{ row }">
        <button
          type="button"
          class="btn btn-ghost btn-sm"
          title="Registrar pagamento"
          :disabled="row.situacao === SITUACAO_CONTAS_A_PAGAR.PAGO || row.situacao === SITUACAO_CONTAS_A_PAGAR.CANCELADO"
          @click.stop="abrirBaixa(row)"
        >
          💲
        </button>
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editar(row)">✎</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Excluir" @click.stop="pedirExclusao(row)">🗑</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="tituloExclusao"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />

    <BaixaContaPagarDialog
      v-model="baixaVisivel"
      :conta="itemParaBaixa"
      @confirmado="aoConfirmarBaixa"
    />
  </div>
</template>

<style scoped>
.cards-resumo {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 12px;
  margin-bottom: 16px;
}
.card-resumo {
  padding: 16px 18px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.card-resumo-label {
  font-size: 12px;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.4px;
}
.card-resumo-valor {
  font-size: 22px;
  font-weight: 700;
  color: var(--text-primary);
}
.card-resumo-valor.valor-negativo {
  color: var(--danger);
}
.card-resumo-valor.loading {
  opacity: 0.5;
}
</style>
