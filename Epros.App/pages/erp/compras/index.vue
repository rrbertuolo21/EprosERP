<script setup lang="ts">
/**
 * Lista de Compras (erp/compras).
 *
 * Porta o comportamento de `compras/lista-compras.vue` do legado, adaptado ao design system
 * do novo app e ao contrato real do backend (`ComprasController` / `ListarComprasQuery`):
 *   - busca única por texto (`localizar`): casa fornecedor (nome/CNPJ), número da nota e chave;
 *   - listagem paginada server-side;
 *   - por linha: editar (abre a entrada), cancelar (POST {id}/cancelar com motivo).
 *
 * O envelope da listagem deste endpoint é CommandResult com `dados = { Total, Pagina, Itens }`
 * (não usa `totalRegistros`), por isso a paginação é controlada localmente com `useApi`
 * em vez de `useApiList`.
 *
 * Endpoints: compras (GET, {id}/cancelar).
 */
import { computed, onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import { useMask } from '~/composables/useMask'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

/** Item de compra retornado pela listagem (projeção de `ListarComprasQuery`). */
interface CompraListagem {
  id: string
  fornecedorCnpj: string | null
  fornecedorNome: string | null
  numeroNota: string | null
  chaveAcesso: string | null
  valorTotal: number | null
  dataEmissao: string | null
  status: number | string | null
  criadoEm: string | null
}

/** Resposta de dados da listagem: `dados` = { Total, Pagina, Itens }. */
interface ComprasListagemDados {
  total: number
  pagina: number
  itens: CompraListagem[]
}

const toast = useToast()
const { formatarMoeda, formatarData } = useHelper()
const { maskCpfCnpj } = useMask()

// --- Estado da listagem (paginação local) ---
const itens = ref<CompraListagem[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(20)
const carregando = ref(false)

// --- Filtro (busca única por texto) ---
const filtrosForm = ref<Record<string, unknown>>({ localizar: '' })
const localizar = ref('')

const camposFiltro = computed<FilterField[]>(() => [
  {
    key: 'localizar',
    label: 'Buscar',
    type: 'text',
    placeholder: 'Fornecedor, número da nota ou chave de acesso',
    grow: true
  }
])

/** Rótulos de status da compra (espelha os status internos do documento de entrada). */
const STATUS_LABEL: Record<string, string> = {
  '0': 'Recebido',
  '1': 'Em processamento',
  '2': 'Autorizado',
  '3': 'Rejeitado',
  '4': 'Cancelado'
}

function rotuloStatus(valor: number | string | null): string {
  if (valor == null) return '-'
  return STATUS_LABEL[String(valor)] ?? String(valor)
}

function classeStatus(valor: number | string | null): string {
  switch (String(valor)) {
    case '2':
      return 'st-ok'
    case '3':
      return 'st-erro'
    case '4':
      return 'st-cancelado'
    default:
      return 'st-pendente'
  }
}

const colunas: DataTableColumn<CompraListagem>[] = [
  {
    key: 'dataEmissao',
    label: 'Data',
    width: '110px',
    formatter: (v) => formatarData(v as string | null)
  },
  { key: 'numeroNota', label: 'Número', width: '110px' },
  {
    key: 'fornecedorNome',
    label: 'Fornecedor',
    formatter: (v) => (v as string | null) ?? '-'
  },
  { key: 'chaveAcesso', label: 'Chave', formatter: (v) => (v as string | null) ?? '-' },
  { key: 'status', label: 'Status', align: 'center', width: '150px' },
  {
    key: 'valorTotal',
    label: 'Valor',
    align: 'right',
    width: '130px',
    formatter: (v) => formatarMoeda(v as number | null)
  }
]

async function buscar(): Promise<void> {
  carregando.value = true
  try {
    const resposta = await useApi('/compras', {
      query: {
        localizar: localizar.value || undefined,
        pagina: pagina.value,
        tamanhoPagina: tamanhoPagina.value
      }
    })
    const dados = extrairDados<ComprasListagemDados>(resposta)
    itens.value = dados?.itens ?? []
    total.value = dados?.total ?? itens.value.length
  } catch (e) {
    itens.value = []
    total.value = 0
    toast.error(obterMensagemErro(e))
    console.error('[compras:index]', e)
  } finally {
    carregando.value = false
  }
}

let debounceTimer: ReturnType<typeof setTimeout> | undefined

function aoMudarFiltros(valores: Record<string, unknown>) {
  filtrosForm.value = valores
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    localizar.value = (valores.localizar as string) || ''
    pagina.value = 1
    void buscar()
  }, 500)
}

function aoBuscar(valores: Record<string, unknown>) {
  if (debounceTimer) clearTimeout(debounceTimer)
  localizar.value = (valores.localizar as string) || ''
  pagina.value = 1
  void buscar()
}

function aoLimpar() {
  filtrosForm.value = { localizar: '' }
  localizar.value = ''
  pagina.value = 1
  void buscar()
}

function irParaPagina(p: number) {
  pagina.value = p
  void buscar()
}

function mudarTamanho(ps: number) {
  tamanhoPagina.value = ps
  pagina.value = 1
  void buscar()
}

function criarNovo() {
  navigateTo('/erp/compras/entrada-mercadorias/novo')
}

function editar(item: CompraListagem) {
  navigateTo(`/erp/compras/entrada-mercadorias/${item.id}`)
}

// --- Cancelamento de compra ---
const cancelarVisivel = ref(false)
const cancelando = ref(false)
const compraParaCancelar = ref<CompraListagem | null>(null)
const motivoCancelamento = ref('')

function pedirCancelamento(item: CompraListagem) {
  compraParaCancelar.value = item
  motivoCancelamento.value = ''
  cancelarVisivel.value = true
}

function fecharCancelamento() {
  cancelarVisivel.value = false
  compraParaCancelar.value = null
  motivoCancelamento.value = ''
}

async function confirmarCancelamento() {
  if (!compraParaCancelar.value || motivoCancelamento.value.trim().length < 3) return
  cancelando.value = true
  try {
    await useApi('/compras/{id}/cancelar', {
      method: 'POST',
      params: { id: compraParaCancelar.value.id },
      body: { motivo: motivoCancelamento.value.trim() }
    })
    toast.success('Compra cancelada com sucesso')
    fecharCancelamento()
    await buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    cancelando.value = false
  }
}

const podeCancelar = (item: CompraListagem): boolean => String(item.status) !== '4'

const documentoFornecedor = (item: CompraListagem): string =>
  item.fornecedorCnpj ? maskCpfCnpj(item.fornecedorCnpj) : '-'

onMounted(() => {
  void buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Compras" subtitle="Notas de entrada de mercadorias" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="criarNovo">+ Nova Compra</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="filtrosForm"
      :loading="carregando"
      @update:model-value="aoMudarFiltros"
      @search="aoBuscar"
      @clear="aoLimpar"
    />

    <DataTable
      :items="itens"
      :columns="colunas"
      :total="total"
      :page="pagina"
      :page-size="tamanhoPagina"
      :loading="carregando"
      empty-text="Nenhuma nota de entrada encontrada"
      @update:page="irParaPagina"
      @update:page-size="mudarTamanho"
      @row-click="editar"
    >
      <template #cell-fornecedorNome="{ row }">
        <div class="fornecedor-cell">
          <span class="fornecedor-nome">{{ row.fornecedorNome || '-' }}</span>
          <span class="fornecedor-doc">{{ documentoFornecedor(row) }}</span>
        </div>
      </template>

      <template #cell-status="{ row }">
        <span class="badge" :class="classeStatus(row.status)">{{ rotuloStatus(row.status) }}</span>
      </template>

      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editar(row)">✎</button>
        <button
          v-if="podeCancelar(row)"
          type="button"
          class="btn btn-ghost btn-sm"
          title="Cancelar"
          @click.stop="pedirCancelamento(row)"
        >
          ⊘
        </button>
      </template>
    </DataTable>

    <AppDialog v-model="cancelarVisivel" title="Cancelar Compra" width="480px" persistent>
      <p class="cancel-msg">
        Informe o motivo do cancelamento da nota
        <strong v-if="compraParaCancelar?.numeroNota">nº {{ compraParaCancelar.numeroNota }}</strong>.
        Esta ação estornará a movimentação de estoque vinculada.
      </p>
      <div class="form-grid">
        <div class="col-12">
          <TextField
            v-model="motivoCancelamento"
            label="Motivo do cancelamento"
            required
            placeholder="Descreva o motivo (mínimo 3 caracteres)"
            :maxlength="255"
          />
        </div>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="cancelando" @click="fecharCancelamento">
          Voltar
        </button>
        <button
          type="button"
          class="btn btn-danger"
          :disabled="motivoCancelamento.trim().length < 3 || cancelando"
          @click="confirmarCancelamento"
        >
          <span v-if="cancelando" class="spinner"></span>
          <span v-else>Cancelar Compra</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.fornecedor-cell {
  display: flex;
  flex-direction: column;
  line-height: 1.3;
}
.fornecedor-nome {
  font-weight: 600;
}
.fornecedor-doc {
  font-size: 11px;
  color: var(--text-secondary);
}
.cancel-msg {
  color: var(--text-secondary);
  font-size: 14px;
  line-height: 1.5;
  margin-bottom: 16px;
}
/* Chips de status locais (o main.css só define badges financeiros). */
.badge.st-ok {
  background: rgba(16, 185, 129, 0.1);
  border: 1px solid rgba(16, 185, 129, 0.3);
  color: var(--success);
}
.badge.st-erro {
  background: rgba(239, 68, 68, 0.1);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: var(--danger);
}
.badge.st-pendente {
  background: rgba(245, 158, 11, 0.1);
  border: 1px solid rgba(245, 158, 11, 0.3);
  color: var(--warning);
}
.badge.st-cancelado {
  background: rgba(113, 113, 122, 0.1);
  border: 1px solid rgba(113, 113, 122, 0.3);
  color: #a1a1aa;
}
</style>
