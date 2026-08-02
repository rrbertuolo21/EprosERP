<script setup lang="ts">
/**
 * Rastreabilidade de Lote e Serialização (erp/estoque/rastreabilidade).
 * Duas abas — Lotes e Números de série (RastreabilidadeLotesController):
 *   Lotes:   GET /estoque-rastreabilidade/lotes?produtoId&status   · POST /lotes (criar)
 *   Seriais: GET /estoque-rastreabilidade/seriais?produtoId&status · POST /seriais (registrar)
 * Ações de lote (bloquear/desbloquear/recall/genealogia) vivem no detalhe [id].vue.
 */
import { onMounted, reactive, ref, watch } from 'vue'
import { useApiList } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import { useEstoqueEnums, classeBadge } from '~/composables/useEstoqueEnums'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

const toast = useToast()
const { formatarData, formatarNumero } = useHelper()
const { statusLote, statusSerial, origemLote } = useEstoqueEnums()

type Aba = 'lotes' | 'seriais'
const aba = ref<Aba>('lotes')

// --------- Lotes ---------
interface LoteListagem {
  id: string
  produtoId: string
  codigoLote: string
  dataValidade: string | null
  status: number
  quantidadeDisponivel: number
  quantidadeBloqueada: number
  quantidadeConsumida: number
}
interface LoteFiltros extends Record<string, unknown> { produtoId?: string; status?: number | null }

const listaLotes = useApiList<LoteListagem, LoteFiltros>('/estoque-rastreabilidade/lotes', { tamanhoPaginaInicial: 25 })

const camposLote: FilterField[] = [
  { key: 'produtoId', label: 'Produto (ID)', type: 'text', placeholder: 'GUID do produto', grow: true },
  { key: 'status', label: 'Status', type: 'select', options: statusLote.opcoes }
]
const colunasLote: DataTableColumn<LoteListagem>[] = [
  { key: 'codigoLote', label: 'Lote', sortable: false },
  { key: 'produtoId', label: 'Produto (ID)' },
  { key: 'dataValidade', label: 'Validade', width: '120px', formatter: (v) => formatarData(v as string) || '-' },
  { key: 'quantidadeDisponivel', label: 'Disponível', align: 'right', formatter: (v) => formatarNumero(v as number, 0, 4) },
  { key: 'quantidadeBloqueada', label: 'Bloqueada', align: 'right', formatter: (v) => formatarNumero(v as number, 0, 4) },
  { key: 'quantidadeConsumida', label: 'Consumida', align: 'right', formatter: (v) => formatarNumero(v as number, 0, 4) },
  { key: 'status', label: 'Status', align: 'center', width: '120px' }
]
function normalizarLote(v: Record<string, unknown>): Partial<LoteFiltros> {
  return { produtoId: (v.produtoId as string) || undefined, status: v.status === '' || v.status == null ? undefined : Number(v.status) }
}
function abrirLote(item: LoteListagem) { navigateTo(`/erp/estoque/rastreabilidade/${item.id}`) }

// --------- Seriais ---------
interface SerialListagem { id: string; produtoId: string; numero: string; status: number; loteId: string | null }
interface SerialFiltros extends Record<string, unknown> { produtoId?: string; status?: number | null }

const listaSeriais = useApiList<SerialListagem, SerialFiltros>('/estoque-rastreabilidade/seriais', { tamanhoPaginaInicial: 25 })
const camposSerial: FilterField[] = [
  { key: 'produtoId', label: 'Produto (ID)', type: 'text', placeholder: 'GUID do produto', grow: true },
  { key: 'status', label: 'Status', type: 'select', options: statusSerial.opcoes }
]
const colunasSerial: DataTableColumn<SerialListagem>[] = [
  { key: 'numero', label: 'Número de série' },
  { key: 'produtoId', label: 'Produto (ID)' },
  { key: 'loteId', label: 'Lote (ID)', formatter: (v) => (v as string) || '-' },
  { key: 'status', label: 'Status', align: 'center', width: '120px' }
]
function normalizarSerial(v: Record<string, unknown>): Partial<SerialFiltros> {
  return { produtoId: (v.produtoId as string) || undefined, status: v.status === '' || v.status == null ? undefined : Number(v.status) }
}

watch(aba, (a) => {
  if (a === 'lotes' && listaLotes.itens.value.length === 0) void listaLotes.buscar()
  if (a === 'seriais' && listaSeriais.itens.value.length === 0) void listaSeriais.buscar()
})

// --------- Criar lote ---------
const loteDialog = ref(false)
const salvandoLote = ref(false)
const formLote = reactive({ empresaId: '', produtoId: '', codigoLote: '', quantidadeRecebida: null as number | null, origem: 0 as number, localId: '', dataFabricacao: null as string | null, dataValidade: null as string | null, observacao: '' })
const errosLote = reactive<Record<string, string>>({})
function abrirCriarLote() {
  Object.assign(formLote, { empresaId: '', produtoId: '', codigoLote: '', quantidadeRecebida: null, origem: 0, localId: '', dataFabricacao: null, dataValidade: null, observacao: '' })
  for (const k of Object.keys(errosLote)) delete errosLote[k]
  loteDialog.value = true
}
async function salvarLote() {
  for (const k of Object.keys(errosLote)) delete errosLote[k]
  if (!formLote.empresaId.trim()) errosLote.empresaId = 'Empresa é obrigatória.'
  if (!formLote.produtoId.trim()) errosLote.produtoId = 'Produto é obrigatório.'
  if (!formLote.codigoLote.trim()) errosLote.codigoLote = 'Código do lote é obrigatório.'
  if (Object.keys(errosLote).length > 0) return
  salvandoLote.value = true
  try {
    await useApi('/estoque-rastreabilidade/lotes', {
      method: 'POST',
      body: {
        empresaId: formLote.empresaId.trim(),
        produtoId: formLote.produtoId.trim(),
        codigoLote: formLote.codigoLote.trim(),
        quantidadeRecebida: Number(formLote.quantidadeRecebida ?? 0),
        origem: formLote.origem,
        localId: formLote.localId.trim() || null,
        dataFabricacao: formLote.dataFabricacao || null,
        dataValidade: formLote.dataValidade || null,
        observacao: formLote.observacao.trim() || null
      }
    })
    toast.success('Lote criado com sucesso!')
    loteDialog.value = false
    await listaLotes.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoLote.value = false
  }
}

// --------- Registrar serial ---------
const serialDialog = ref(false)
const salvandoSerial = ref(false)
const formSerial = reactive({ produtoId: '', numero: '', loteId: '' })
const errosSerial = reactive<Record<string, string>>({})
function abrirRegistrarSerial() {
  Object.assign(formSerial, { produtoId: '', numero: '', loteId: '' })
  for (const k of Object.keys(errosSerial)) delete errosSerial[k]
  serialDialog.value = true
}
async function salvarSerial() {
  for (const k of Object.keys(errosSerial)) delete errosSerial[k]
  if (!formSerial.produtoId.trim()) errosSerial.produtoId = 'Produto é obrigatório.'
  if (!formSerial.numero.trim()) errosSerial.numero = 'Número de série é obrigatório.'
  if (Object.keys(errosSerial).length > 0) return
  salvandoSerial.value = true
  try {
    await useApi('/estoque-rastreabilidade/seriais', {
      method: 'POST',
      body: { produtoId: formSerial.produtoId.trim(), numero: formSerial.numero.trim(), loteId: formSerial.loteId.trim() || null }
    })
    toast.success('Número de série registrado!')
    serialDialog.value = false
    await listaSeriais.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoSerial.value = false
  }
}

onMounted(() => void listaLotes.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Rastreabilidade" subtitle="Lotes, números de série, bloqueio, recall e genealogia" :loading="listaLotes.carregando.value || listaSeriais.carregando.value">
      <template #actions>
        <button v-if="aba === 'lotes'" type="button" class="btn btn-primary" @click="abrirCriarLote">+ Novo lote</button>
        <button v-else type="button" class="btn btn-primary" @click="abrirRegistrarSerial">+ Registrar série</button>
      </template>
    </PageToolbar>

    <div class="tabs">
      <button type="button" class="tab" :class="{ ativo: aba === 'lotes' }" @click="aba = 'lotes'">Lotes</button>
      <button type="button" class="tab" :class="{ ativo: aba === 'seriais' }" @click="aba = 'seriais'">Números de série</button>
    </div>

    <!-- Lotes -->
    <template v-if="aba === 'lotes'">
      <FilterBar
        :fields="camposLote"
        :model-value="listaLotes.filtros.value"
        :loading="listaLotes.carregando.value"
        @update:model-value="(v) => (listaLotes.filtros.value = v as typeof listaLotes.filtros.value)"
        @search="(v) => listaLotes.aplicarFiltros(normalizarLote(v as Record<string, unknown>))"
        @clear="listaLotes.limpar()"
      />
      <DataTable
        :items="listaLotes.itens.value"
        :columns="colunasLote"
        :total="listaLotes.total.value"
        :page="listaLotes.pagina.value"
        :page-size="listaLotes.tamanhoPagina.value"
        :loading="listaLotes.carregando.value"
        row-key="id"
        empty-text="Nenhum lote encontrado"
        @update:page="(p) => listaLotes.irParaPagina(p)"
        @update:page-size="(ps) => listaLotes.buscar({ tamanhoPagina: ps, pagina: 1 })"
        @row-click="abrirLote"
      >
        <template #cell-status="{ value }">
          <span class="badge" :class="classeBadge(statusLote.cor(value as number))">{{ statusLote.label(value as number) }}</span>
        </template>
        <template #actions="{ row }">
          <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrirLote(row)">Detalhes</button>
        </template>
      </DataTable>
    </template>

    <!-- Seriais -->
    <template v-else>
      <FilterBar
        :fields="camposSerial"
        :model-value="listaSeriais.filtros.value"
        :loading="listaSeriais.carregando.value"
        @update:model-value="(v) => (listaSeriais.filtros.value = v as typeof listaSeriais.filtros.value)"
        @search="(v) => listaSeriais.aplicarFiltros(normalizarSerial(v as Record<string, unknown>))"
        @clear="listaSeriais.limpar()"
      />
      <DataTable
        :items="listaSeriais.itens.value"
        :columns="colunasSerial"
        :total="listaSeriais.total.value"
        :page="listaSeriais.pagina.value"
        :page-size="listaSeriais.tamanhoPagina.value"
        :loading="listaSeriais.carregando.value"
        row-key="id"
        empty-text="Nenhum número de série encontrado"
        @update:page="(p) => listaSeriais.irParaPagina(p)"
        @update:page-size="(ps) => listaSeriais.buscar({ tamanhoPagina: ps, pagina: 1 })"
      >
        <template #cell-status="{ value }">
          <span class="badge" :class="classeBadge(statusSerial.cor(value as number))">{{ statusSerial.label(value as number) }}</span>
        </template>
      </DataTable>
    </template>

    <!-- Dialog criar lote -->
    <AppDialog v-model="loteDialog" title="Novo lote rastreável" width="560px">
      <div class="form-grid">
        <TextField v-model="formLote.empresaId" label="Empresa (ID)" required :error="errosLote.empresaId" />
        <TextField v-model="formLote.produtoId" label="Produto (ID)" required :error="errosLote.produtoId" />
        <TextField v-model="formLote.codigoLote" label="Código do lote" required :error="errosLote.codigoLote" />
        <QuantityInput v-model="formLote.quantidadeRecebida" label="Quantidade recebida" :decimais="4" />
        <SelectField v-model="formLote.origem" label="Origem" :options="origemLote.opcoes" :clearable="false" />
        <TextField v-model="formLote.localId" label="Local (ID)" />
        <DateTimeField v-model="formLote.dataFabricacao" label="Data de fabricação" />
        <DateTimeField v-model="formLote.dataValidade" label="Data de validade" />
      </div>
      <TextField v-model="formLote.observacao" label="Observação" />
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoLote" @click="loteDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoLote" @click="salvarLote">
          <span v-if="salvandoLote" class="spinner"></span><span v-else>Criar lote</span>
        </button>
      </template>
    </AppDialog>

    <!-- Dialog registrar serial -->
    <AppDialog v-model="serialDialog" title="Registrar número de série" width="480px">
      <div class="form-grid">
        <TextField v-model="formSerial.produtoId" label="Produto (ID)" required :error="errosSerial.produtoId" />
        <TextField v-model="formSerial.numero" label="Número de série" required :error="errosSerial.numero" />
        <TextField v-model="formSerial.loteId" label="Lote (ID)" hint="Opcional" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoSerial" @click="serialDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoSerial" @click="salvarSerial">
          <span v-if="salvandoSerial" class="spinner"></span><span v-else>Registrar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.tabs { display: flex; gap: 4px; margin: 8px 0 12px; }
.tab { padding: 8px 16px; border: 1px solid var(--border-color); background: transparent; color: var(--text-secondary); border-radius: 8px; cursor: pointer; font-size: 13px; }
.tab.ativo { background: var(--primary); color: #fff; border-color: var(--primary); }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; margin-bottom: 12px; }
</style>
