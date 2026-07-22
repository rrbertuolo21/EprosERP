<script setup lang="ts">
/**
 * Listagem de Contas a Receber — Financeiro / Contas a Receber.
 *
 * Porta o comportamento de `financeiro/contas-a-receber/index.vue` do legado:
 * cards de resumo (vencendo hoje / vencidos / a vencer) com toggle de filtro,
 * busca com filtros avançados, tabela paginada server-side, baixa de pagamento,
 * editar e excluir.
 */
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi, extrairDados } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import { useEnum, type SelectOption } from '~/composables/useEnum'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import BaixaPagamentoDialog from '~/components/financeiro-receber/BaixaPagamentoDialog.vue'
import { situacaoInfo, type ContasAReceber } from '~/components/financeiro-receber/types'

definePageMeta({ layout: 'default' })

interface ContasAReceberFiltros {
  dataVencimento?: string
  documento?: string
  natureza?: string
  nomeCliente?: string
  origem?: string
  situacao?: string
  valorTitulo?: number
  statusVencimento?: string
}

const router = useRouter()
const toast = useToast()
const { formatarData, formatarMoeda } = useHelper()
const { carregarOpcoes } = useEnum()

const lista = useApiList<ContasAReceber, ContasAReceberFiltros>('/financeiro/contas-receber', {
  filtrosIniciais: {},
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<ContasAReceber>[] = [
  { key: 'dataEmissao', label: 'Emissão', sortable: true, align: 'center', width: '110px' },
  { key: 'dataVencimento', label: 'Vencimento', sortable: true, align: 'center', width: '110px' },
  { key: 'nomePessoa', label: 'Cliente', sortable: true },
  { key: 'documento', label: 'Documento', sortable: true },
  { key: 'detalhamento', label: 'Descrição', sortable: false },
  { key: 'valorTitulo', label: 'Valor', sortable: true, align: 'right', width: '130px' },
  { key: 'situacao', label: 'Situação', sortable: true, align: 'center', width: '120px' }
]

const opcoesSituacao = ref<SelectOption[]>([])
const opcoesOrigem = ref<SelectOption[]>([])

const camposFiltro = computed<FilterField[]>(() => [
  { key: 'nomeCliente', label: 'Cliente', type: 'text', placeholder: 'Nome do cliente...', grow: true },
  { key: 'documento', label: 'Documento', type: 'text', placeholder: 'Número do documento...' },
  { key: 'natureza', label: 'Natureza', type: 'text', placeholder: 'Plano de contas...' },
  { key: 'situacao', label: 'Situação', type: 'select', options: opcoesSituacao.value },
  { key: 'origem', label: 'Origem', type: 'select', options: opcoesOrigem.value },
  { key: 'dataVencimento', label: 'Data de Vencimento', type: 'date' }
])

const totais = ref<{ valorVencendoHoje: number; valorVencido: number; valorAVencer: number }>({
  valorVencendoHoje: 0,
  valorVencido: 0,
  valorAVencer: 0
})
const carregandoTotais = ref(false)

const statusCards = computed(() => [
  {
    id: 'VencendoHoje',
    titulo: 'Vencendo Hoje',
    cor: 'warning',
    valor: totais.value.valorVencendoHoje
  },
  {
    id: 'Vencidos',
    titulo: 'Vencidos',
    cor: 'error',
    valor: totais.value.valorVencido
  },
  {
    id: 'AVencer',
    titulo: 'A Vencer',
    cor: 'success',
    valor: totais.value.valorAVencer
  }
])

async function carregarTotais() {
  carregandoTotais.value = true
  try {
    const resposta = await useApi('/financeiro/contas-receber/totais')
    const dados = extrairDados<{ valorVencendoHoje: number; valorVencido: number; valorAVencer: number }>(resposta)
    if (dados) totais.value = dados
  } catch (e) {
    console.error('[contas-a-receber/index] totais', e)
  } finally {
    carregandoTotais.value = false
  }
}

function toggleStatusVencimento(id: string) {
  const atual = lista.filtros.value.statusVencimento
  lista.aplicarFiltros({ statusVencimento: atual === id ? undefined : id })
}

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<ContasAReceber | null>(null)

const showBaixaDialog = ref(false)
const baixaItemId = ref<number | undefined>()

function novaConta() {
  router.push('/erp/financeiro/contas-a-receber/novo')
}

function editarConta(item: ContasAReceber) {
  if (!item.id) return
  router.push(`/erp/financeiro/contas-a-receber/${item.id}`)
}

function abrirBaixa(item: ContasAReceber) {
  baixaItemId.value = item.id
  showBaixaDialog.value = true
}

function pedirExclusao(item: ContasAReceber) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value?.id) return
  excluindo.value = true
  try {
    await useApi(`/financeiro/contas-receber/${itemParaExcluir.value.id}`, { method: 'DELETE' })
    toast.success('Conta a receber excluída com sucesso!')
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

function aoSalvarBaixa() {
  void lista.buscar()
  void carregarTotais()
}

onMounted(async () => {
  opcoesSituacao.value = await carregarOpcoes('financeiros-enums/situacao-titulo')
  opcoesOrigem.value = await carregarOpcoes('financeiros-enums/origem')
  await lista.buscar()
  await carregarTotais()
})
</script>

<template>
  <div>
    <PageToolbar title="Contas a Receber" subtitle="Gestão de títulos e recebimentos" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novaConta">+ Nova Conta</button>
      </template>
    </PageToolbar>

    <div class="status-cards">
      <button
        v-for="card in statusCards"
        :key="card.id"
        type="button"
        class="status-card"
        :class="[`status-card-${card.cor}`, { inativo: lista.filtros.value.statusVencimento && lista.filtros.value.statusVencimento !== card.id }]"
        @click="toggleStatusVencimento(card.id)"
      >
        <span class="status-card-titulo">{{ card.titulo }}</span>
        <span v-if="carregandoTotais" class="status-card-skeleton"></span>
        <span v-else class="status-card-valor">{{ formatarMoeda(card.valor) }}</span>
      </button>
    </div>

    <FilterBar
      :fields="camposFiltro"
      :model-value="lista.filtros.value"
      :loading="lista.carregando.value"
      @update:model-value="(v) => (lista.filtros.value = v as typeof lista.filtros.value)"
      @search="lista.aplicarFiltros($event as Partial<typeof lista.filtros.value>)"
      @clear="lista.limpar()"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhuma conta a receber encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="editarConta"
    >
      <template #cell-dataEmissao="{ value }">
        {{ formatarData(value as string) }}
      </template>
      <template #cell-dataVencimento="{ value }">
        {{ formatarData(value as string) }}
      </template>
      <template #cell-documento="{ row, value }">
        {{ value }}
        <span v-if="row.numeroParcela && row.numeroParcela > 1"> / {{ row.numeroParcela }}</span>
      </template>
      <template #cell-valorTitulo="{ value }">
        {{ formatarMoeda(value as number) }}
      </template>
      <template #cell-situacao="{ row }">
        <span class="badge" :class="`badge-${situacaoInfo(row.situacao).cor === 'error' ? 'danger' : situacaoInfo(row.situacao).cor}`">
          {{ situacaoInfo(row.situacao).texto }}
        </span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Baixa" @click.stop="abrirBaixa(row)">Baixa</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editarConta(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Excluir" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.documento || undefined"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />

    <BaixaPagamentoDialog v-model="showBaixaDialog" :item-id="baixaItemId" @saved="aoSalvarBaixa" />
  </div>
</template>

<style scoped>
.status-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 12px;
  margin: 16px 0;
}
.status-card {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 14px 16px;
  border-radius: 10px;
  border: 1px solid var(--border-color);
  background: rgba(255, 255, 255, 0.03);
  cursor: pointer;
  text-align: left;
  transition: opacity 0.15s ease, border-color 0.15s ease;
}
.status-card.inativo { opacity: 0.45; }
.status-card-titulo { font-size: 12px; color: var(--text-muted); }
.status-card-valor { font-size: 20px; font-weight: 700; }
.status-card-skeleton {
  display: block;
  width: 120px;
  height: 22px;
  border-radius: 4px;
  background: rgba(255, 255, 255, 0.08);
}
.status-card-warning { border-color: rgba(245, 158, 11, 0.4); }
.status-card-error { border-color: rgba(239, 68, 68, 0.4); }
.status-card-success { border-color: rgba(34, 197, 94, 0.4); }
</style>
