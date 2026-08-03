<script setup lang="ts">
/**
 * Subcontratação — ordens de beneficiamento em terceiro (erp/compras/subcontratacao).
 *
 * Camada de apresentação sobre `SubcontratacoesController` (`/api/v1/estoque-subcontratacoes`):
 *   - listagem paginada (filtro por fornecedor) com status da ordem;
 *   - criação de ordem (cabeçalho);
 *   - abrir detalhe (envio/retorno).
 *
 * Endpoints: estoque-subcontratacoes (GET, POST).
 */
import { onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

interface SubOrdem {
  id: string
  numeroOrdem: string | null
  fornecedorId: string | null
  ordemProducaoId: string | null
  status: number | string | null
  dataEmissao: string | null
  dataPrevistaRetorno: string | null
}

const STATUS_TEXTO: Record<string, { texto: string; classe: string }> = {
  '0': { texto: 'Aberta', classe: 'pendente' },
  '1': { texto: 'Em Processo', classe: 'pendente' },
  '2': { texto: 'Retornada', classe: 'ok' },
  '3': { texto: 'Concluída', classe: 'ok' },
  '4': { texto: 'Cancelada', classe: 'cancelado' }
}

const toast = useToast()
const { formatarData } = useHelper()

const itens = ref<SubOrdem[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(20)
const carregando = ref(false)
const filtroFornecedor = ref('')

const filtrosForm = ref<Record<string, unknown>>({ fornecedorId: '' })
const camposFiltro: FilterField[] = [
  { key: 'fornecedorId', label: 'Fornecedor (ID)', type: 'text', placeholder: 'ID do fornecedor', grow: true }
]

const colunas: DataTableColumn<SubOrdem>[] = [
  { key: 'numeroOrdem', label: 'Ordem', width: '150px', formatter: (v) => (v as string | null) ?? '-' },
  { key: 'fornecedorId', label: 'Fornecedor', formatter: (v) => (v ? `Forn. ${String(v).slice(0, 8)}` : '-') },
  { key: 'status', label: 'Status', align: 'center', width: '140px' },
  { key: 'dataEmissao', label: 'Emissão', width: '120px', formatter: (v) => formatarData(v as string | null) },
  { key: 'dataPrevistaRetorno', label: 'Prev. retorno', width: '130px', formatter: (v) => formatarData(v as string | null) }
]

async function buscar(): Promise<void> {
  carregando.value = true
  try {
    const query: Record<string, unknown> = { pagina: pagina.value, tamanhoPagina: tamanhoPagina.value }
    if (filtroFornecedor.value.trim()) query.fornecedorId = filtroFornecedor.value.trim()
    const resposta = await useApi('/estoque-subcontratacoes', { query })
    const dados = extrairDados<{ total: number; itens: SubOrdem[] }>(resposta)
    itens.value = dados?.itens ?? []
    total.value = dados?.total ?? itens.value.length
  } catch (e) {
    itens.value = []
    total.value = 0
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

let deb: ReturnType<typeof setTimeout> | undefined
function aoMudar(v: Record<string, unknown>) {
  filtrosForm.value = v
  if (deb) clearTimeout(deb)
  deb = setTimeout(() => { filtroFornecedor.value = (v.fornecedorId as string) || ''; pagina.value = 1; void buscar() }, 400)
}
function aoBuscar(v: Record<string, unknown>) {
  if (deb) clearTimeout(deb)
  filtroFornecedor.value = (v.fornecedorId as string) || ''
  pagina.value = 1
  void buscar()
}
function aoLimpar() {
  filtrosForm.value = { fornecedorId: '' }
  filtroFornecedor.value = ''
  pagina.value = 1
  void buscar()
}

function abrir(item: SubOrdem) {
  navigateTo(`/erp/compras/subcontratacao/${item.id}`)
}

// Nova ordem
const novaVisivel = ref(false)
const salvando = ref(false)
const nova = ref({ fornecedorId: '', numeroOrdem: '', dataEmissao: hojeIso(), dataPrevistaRetorno: '', observacao: '' })
function hojeIso(): string {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}
function abrirNova() {
  nova.value = { fornecedorId: '', numeroOrdem: '', dataEmissao: hojeIso(), dataPrevistaRetorno: '', observacao: '' }
  novaVisivel.value = true
}
async function salvarNova() {
  if (!nova.value.fornecedorId.trim()) {
    toast.error('Informe o fornecedor (ID)')
    return
  }
  salvando.value = true
  try {
    await useApi('/estoque-subcontratacoes', {
      method: 'POST',
      body: {
        fornecedorId: nova.value.fornecedorId.trim(),
        numeroOrdem: nova.value.numeroOrdem.trim() || null,
        dataEmissao: nova.value.dataEmissao || null,
        dataPrevistaRetorno: nova.value.dataPrevistaRetorno || null,
        observacao: nova.value.observacao.trim() || null,
        itens: []
      }
    })
    toast.success('Ordem de subcontratação criada')
    novaVisivel.value = false
    await buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

onMounted(() => void buscar())
</script>

<template>
  <div>
    <PageToolbar title="Subcontratação" subtitle="Ordens de beneficiamento em terceiro (envio/retorno)" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirNova">+ Nova Ordem</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="filtrosForm"
      :loading="carregando"
      @update:model-value="aoMudar"
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
      empty-text="Nenhuma ordem de subcontratação"
      @update:page="(p) => { pagina = p; buscar() }"
      @update:page-size="(ps) => { tamanhoPagina = ps; pagina = 1; buscar() }"
      @row-click="abrir"
    >
      <template #cell-status="{ row }">
        <span class="badge" :class="`st-${(STATUS_TEXTO[String(row.status)] || {}).classe || 'pendente'}`">
          {{ (STATUS_TEXTO[String(row.status)] || {}).texto || row.status }}
        </span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Abrir" @click.stop="abrir(row)">➔</button>
      </template>
    </DataTable>

    <AppDialog v-model="novaVisivel" title="Nova Ordem de Subcontratação" width="560px" persistent>
      <div class="form-grid">
        <div class="col-8"><TextField v-model="nova.fornecedorId" label="Fornecedor (ID)" required /></div>
        <div class="col-4"><TextField v-model="nova.numeroOrdem" label="Nº da ordem" /></div>
        <div class="col-6"><DateTimeField v-model="nova.dataEmissao" label="Data de emissão" /></div>
        <div class="col-6"><DateTimeField v-model="nova.dataPrevistaRetorno" label="Previsão de retorno" /></div>
        <div class="col-12"><TextField v-model="nova.observacao" label="Observação" /></div>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="novaVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvarNova">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Criar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.badge.st-ok { background: rgba(16, 185, 129, 0.1); border: 1px solid rgba(16, 185, 129, 0.3); color: var(--success); }
.badge.st-pendente { background: rgba(245, 158, 11, 0.1); border: 1px solid rgba(245, 158, 11, 0.3); color: var(--warning); }
.badge.st-cancelado { background: rgba(113, 113, 122, 0.1); border: 1px solid rgba(113, 113, 122, 0.3); color: #a1a1aa; }
</style>
