<script setup lang="ts">
/**
 * Alçadas & Aprovações de Compra (erp/compras/aprovacoes).
 *
 * Camada de apresentação com duas abas:
 *   - Fila de Aprovação (`ComprasAprovacoesController` / `/api/v1/compras-aprovacoes`):
 *     lista pedidos multi-nível e permite aprovar/reprovar/cancelar o nível atual;
 *   - Regras de Alçada (`ComprasAlcadasController` / `/api/v1/compras-alcadas`):
 *     CRUD das faixas de valor por nível/comprador/categoria/aprovador.
 *
 * Endpoints: compras-aprovacoes (GET, {id}/aprovar, {id}/reprovar, {id}/cancelar),
 *   compras-alcadas (GET, POST, PUT {id}, DELETE {id}).
 */
import { onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ middleware: 'auth', layout: 'default' })

const toast = useToast()
const { formatarMoeda, formatarData } = useHelper()

const abaAtiva = ref<'fila' | 'regras'>('fila')

// ===================== FILA DE APROVAÇÃO =====================
interface PedidoAprovacao {
  id: string
  origemTipo: number | string | null
  origemId: string | null
  valorTotal: number | null
  compradorId: string | null
  categoriaCompra: string | null
  status: number | string | null
  nivelAtual: number | null
  quantidadeNiveis: number | null
  criadoEm: string | null
  decididoEm: string | null
}

const STATUS_OPTIONS: SelectOption[] = [
  { label: 'Pendente', value: 1 },
  { label: 'Aprovado', value: 2 },
  { label: 'Reprovado', value: 3 },
  { label: 'Cancelado', value: 4 }
]
const ORIGEM_OPTIONS: SelectOption[] = [
  { label: 'Pedido de Compra', value: 1 },
  { label: 'Compra', value: 2 },
  { label: 'Contrato de Compra', value: 3 }
]
const STATUS_TEXTO: Record<string, { texto: string; classe: string }> = {
  '1': { texto: 'Pendente', classe: 'pendente' },
  '2': { texto: 'Aprovado', classe: 'ok' },
  '3': { texto: 'Reprovado', classe: 'erro' },
  '4': { texto: 'Cancelado', classe: 'cancelado' }
}
const ORIGEM_TEXTO: Record<string, string> = { '1': 'Pedido de Compra', '2': 'Compra', '3': 'Contrato' }

const pedidos = ref<PedidoAprovacao[]>([])
const totalPedidos = ref(0)
const paginaPedidos = ref(1)
const tamanhoPedidos = ref(20)
const carregandoFila = ref(false)
const filtroStatus = ref<string>('')

const filaFiltrosForm = ref<Record<string, unknown>>({ status: '' })
const filaCampos: FilterField[] = [
  { key: 'status', label: 'Status', type: 'select', options: STATUS_OPTIONS, grow: true }
]

const colunasFila: DataTableColumn<PedidoAprovacao>[] = [
  { key: 'origemTipo', label: 'Origem', width: '150px', formatter: (v) => ORIGEM_TEXTO[String(v)] ?? '-' },
  { key: 'valorTotal', label: 'Valor', align: 'right', width: '150px', formatter: (v) => formatarMoeda(v as number | null) },
  { key: 'categoriaCompra', label: 'Categoria', formatter: (v) => (v as string | null) ?? '-' },
  { key: 'nivelAtual', label: 'Nível', align: 'center', width: '110px' },
  { key: 'status', label: 'Status', align: 'center', width: '130px' },
  { key: 'criadoEm', label: 'Criado', width: '120px', formatter: (v) => formatarData(v as string | null) }
]

async function buscarFila(): Promise<void> {
  carregandoFila.value = true
  try {
    const query: Record<string, unknown> = { pagina: paginaPedidos.value, tamanhoPagina: tamanhoPedidos.value }
    if (filtroStatus.value !== '') query.status = Number(filtroStatus.value)
    const resposta = await useApi('/compras-aprovacoes', { query })
    const dados = extrairDados<{ total: number; pagina: number; itens: PedidoAprovacao[] }>(resposta)
    pedidos.value = dados?.itens ?? []
    totalPedidos.value = dados?.total ?? pedidos.value.length
  } catch (e) {
    pedidos.value = []
    totalPedidos.value = 0
    toast.error(obterMensagemErro(e))
  } finally {
    carregandoFila.value = false
  }
}

let debFila: ReturnType<typeof setTimeout> | undefined
function filaMudou(v: Record<string, unknown>) {
  filaFiltrosForm.value = v
  if (debFila) clearTimeout(debFila)
  debFila = setTimeout(() => { filtroStatus.value = (v.status as string) ?? ''; paginaPedidos.value = 1; void buscarFila() }, 300)
}
function filaBuscar(v: Record<string, unknown>) {
  if (debFila) clearTimeout(debFila)
  filtroStatus.value = (v.status as string) ?? ''
  paginaPedidos.value = 1
  void buscarFila()
}
function filaLimpar() {
  filaFiltrosForm.value = { status: '' }
  filtroStatus.value = ''
  paginaPedidos.value = 1
  void buscarFila()
}

function ehPendente(p: PedidoAprovacao): boolean {
  return String(p.status) === '1'
}

const acaoEmCurso = ref(false)
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

// Diálogo de aprovar/reprovar com justificativa
const decisaoVisivel = ref(false)
const decisaoTipo = ref<'aprovar' | 'reprovar'>('aprovar')
const decisaoPedido = ref<PedidoAprovacao | null>(null)
const decisaoJustificativa = ref('')

function abrirDecisao(p: PedidoAprovacao, tipo: 'aprovar' | 'reprovar') {
  decisaoPedido.value = p
  decisaoTipo.value = tipo
  decisaoJustificativa.value = ''
  decisaoVisivel.value = true
}

async function confirmarDecisao() {
  if (!decisaoPedido.value) return
  acaoEmCurso.value = true
  try {
    const rota = decisaoTipo.value === 'aprovar' ? '/compras-aprovacoes/{id}/aprovar' : '/compras-aprovacoes/{id}/reprovar'
    await useApi(rota, {
      method: 'POST',
      params: { id: decisaoPedido.value.id },
      body: { justificativa: decisaoJustificativa.value.trim() || null }
    })
    toast.success(decisaoTipo.value === 'aprovar' ? 'Nível aprovado' : 'Pedido reprovado')
    decisaoVisivel.value = false
    await buscarFila()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmCurso.value = false
  }
}

async function cancelarPedido(p: PedidoAprovacao) {
  const ok = await confirmRef.value?.open('Cancelar aprovação', 'Deseja cancelar este pedido de aprovação?', {
    danger: true, textoConfirmar: 'Cancelar pedido', textoCancelar: 'Voltar'
  })
  if (!ok) return
  acaoEmCurso.value = true
  try {
    await useApi('/compras-aprovacoes/{id}/cancelar', { method: 'POST', params: { id: p.id } })
    toast.success('Pedido cancelado')
    await buscarFila()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmCurso.value = false
  }
}

// ===================== REGRAS DE ALÇADA =====================
interface AlcadaRegra {
  id: string
  nivel: number | null
  valorMinimo: number | null
  valorMaximo: number | null
  compradorId: string | null
  categoriaCompra: string | null
  aprovadorId: string | null
  papelAprovador: string | null
  ativo: boolean
}

const regras = ref<AlcadaRegra[]>([])
const carregandoRegras = ref(false)
const regrasCarregadas = ref(false)

const colunasRegras: DataTableColumn<AlcadaRegra>[] = [
  { key: 'nivel', label: 'Nível', align: 'center', width: '90px' },
  { key: 'valorMinimo', label: 'Valor mín.', align: 'right', formatter: (v) => formatarMoeda(v as number | null) },
  { key: 'valorMaximo', label: 'Valor máx.', align: 'right', formatter: (v) => (v == null ? 'Sem teto' : formatarMoeda(v as number)) },
  { key: 'categoriaCompra', label: 'Categoria', formatter: (v) => (v as string | null) ?? '-' },
  { key: 'papelAprovador', label: 'Papel aprovador', formatter: (v) => (v as string | null) ?? '-' },
  { key: 'ativo', label: 'Ativo', align: 'center', width: '90px' }
]

async function buscarRegras(): Promise<void> {
  carregandoRegras.value = true
  try {
    const resposta = await useApi('/compras-alcadas', { query: { pagina: 1, tamanhoPagina: 100 } })
    const dados = extrairDados<{ total: number; itens: AlcadaRegra[] }>(resposta)
    regras.value = dados?.itens ?? []
    regrasCarregadas.value = true
  } catch (e) {
    regras.value = []
    toast.error(obterMensagemErro(e))
  } finally {
    carregandoRegras.value = false
  }
}

const regraVisivel = ref(false)
const salvandoRegra = ref(false)
const editandoRegraId = ref<string | null>(null)
const regraForm = ref<Omit<AlcadaRegra, 'id'>>({
  nivel: 1, valorMinimo: 0, valorMaximo: null, compradorId: '', categoriaCompra: '', aprovadorId: '', papelAprovador: '', ativo: true
})
const ATIVO_OPTIONS: SelectOption[] = [{ label: 'Ativo', value: 1 }, { label: 'Inativo', value: 0 }]

function abrirNovaRegra() {
  editandoRegraId.value = null
  regraForm.value = { nivel: 1, valorMinimo: 0, valorMaximo: null, compradorId: '', categoriaCompra: '', aprovadorId: '', papelAprovador: '', ativo: true }
  regraVisivel.value = true
}
function editarRegra(r: AlcadaRegra) {
  editandoRegraId.value = r.id
  regraForm.value = {
    nivel: r.nivel, valorMinimo: r.valorMinimo, valorMaximo: r.valorMaximo,
    compradorId: r.compradorId ?? '', categoriaCompra: r.categoriaCompra ?? '',
    aprovadorId: r.aprovadorId ?? '', papelAprovador: r.papelAprovador ?? '', ativo: r.ativo
  }
  regraVisivel.value = true
}

async function salvarRegra() {
  salvandoRegra.value = true
  try {
    const body = {
      nivel: Number(regraForm.value.nivel ?? 1),
      valorMinimo: regraForm.value.valorMinimo ?? 0,
      valorMaximo: regraForm.value.valorMaximo,
      compradorId: regraForm.value.compradorId || null,
      categoriaCompra: regraForm.value.categoriaCompra || null,
      aprovadorId: regraForm.value.aprovadorId || null,
      papelAprovador: regraForm.value.papelAprovador || null,
      ativo: regraForm.value.ativo
    }
    if (editandoRegraId.value) {
      await useApi('/compras-alcadas/{id}', { method: 'PUT', params: { id: editandoRegraId.value }, body: { id: editandoRegraId.value, ...body } })
      toast.success('Regra atualizada')
    } else {
      await useApi('/compras-alcadas', { method: 'POST', body })
      toast.success('Regra criada')
    }
    regraVisivel.value = false
    await buscarRegras()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoRegra.value = false
  }
}

const excluirVisivel = ref(false)
const excluindo = ref(false)
const regraExcluir = ref<AlcadaRegra | null>(null)
function pedirExclusaoRegra(r: AlcadaRegra) {
  regraExcluir.value = r
  excluirVisivel.value = true
}
async function confirmarExclusaoRegra() {
  if (!regraExcluir.value) return
  excluindo.value = true
  try {
    await useApi('/compras-alcadas/{id}', { method: 'DELETE', params: { id: regraExcluir.value.id } })
    toast.success('Regra excluída')
    excluirVisivel.value = false
    await buscarRegras()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

function trocarAba(aba: typeof abaAtiva.value) {
  abaAtiva.value = aba
  if (aba === 'regras' && !regrasCarregadas.value) void buscarRegras()
}

onMounted(() => void buscarFila())
</script>

<template>
  <div>
    <PageToolbar title="Alçadas & Aprovações" subtitle="Fila de aprovação multi-nível e regras de alçada" :loading="carregandoFila || carregandoRegras">
      <template #actions>
        <button v-if="abaAtiva === 'regras'" type="button" class="btn btn-primary" @click="abrirNovaRegra">+ Nova Regra</button>
      </template>
    </PageToolbar>

    <div class="tabs">
      <button type="button" class="tab" :class="{ ativa: abaAtiva === 'fila' }" @click="trocarAba('fila')">Fila de Aprovação</button>
      <button type="button" class="tab" :class="{ ativa: abaAtiva === 'regras' }" @click="trocarAba('regras')">Regras de Alçada</button>
    </div>

    <!-- Aba Fila -->
    <div v-show="abaAtiva === 'fila'">
      <FilterBar
        :fields="filaCampos"
        :model-value="filaFiltrosForm"
        :loading="carregandoFila"
        @update:model-value="filaMudou"
        @search="filaBuscar"
        @clear="filaLimpar"
      />
      <DataTable
        :items="pedidos"
        :columns="colunasFila"
        :total="totalPedidos"
        :page="paginaPedidos"
        :page-size="tamanhoPedidos"
        :loading="carregandoFila"
        empty-text="Nenhum pedido de aprovação"
        @update:page="(p) => { paginaPedidos = p; buscarFila() }"
        @update:page-size="(ps) => { tamanhoPedidos = ps; paginaPedidos = 1; buscarFila() }"
      >
        <template #cell-nivelAtual="{ row }">
          {{ row.nivelAtual ?? '-' }} / {{ row.quantidadeNiveis ?? '-' }}
        </template>
        <template #cell-status="{ row }">
          <span class="badge" :class="`st-${(STATUS_TEXTO[String(row.status)] || {}).classe || 'pendente'}`">
            {{ (STATUS_TEXTO[String(row.status)] || {}).texto || row.status }}
          </span>
        </template>
        <template #actions="{ row }">
          <template v-if="ehPendente(row)">
            <button type="button" class="btn btn-ghost btn-sm" title="Aprovar" :disabled="acaoEmCurso" @click.stop="abrirDecisao(row, 'aprovar')">✔</button>
            <button type="button" class="btn btn-ghost btn-sm" title="Reprovar" :disabled="acaoEmCurso" @click.stop="abrirDecisao(row, 'reprovar')">✘</button>
            <button type="button" class="btn btn-ghost btn-sm" title="Cancelar" :disabled="acaoEmCurso" @click.stop="cancelarPedido(row)">⊘</button>
          </template>
          <span v-else class="text-muted">—</span>
        </template>
      </DataTable>
    </div>

    <!-- Aba Regras -->
    <div v-show="abaAtiva === 'regras'">
      <DataTable
        :items="regras"
        :columns="colunasRegras"
        :total="regras.length"
        :page="1"
        :page-size="100"
        :loading="carregandoRegras"
        empty-text="Nenhuma regra de alçada cadastrada"
      >
        <template #cell-ativo="{ row }">
          <span class="badge" :class="row.ativo ? 'st-ok' : 'st-cancelado'">{{ row.ativo ? 'Sim' : 'Não' }}</span>
        </template>
        <template #actions="{ row }">
          <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editarRegra(row)">✎</button>
          <button type="button" class="btn btn-ghost btn-sm" title="Excluir" @click.stop="pedirExclusaoRegra(row)">🗑</button>
        </template>
      </DataTable>
    </div>

    <!-- Diálogo de decisão -->
    <AppDialog v-model="decisaoVisivel" :title="decisaoTipo === 'aprovar' ? 'Aprovar nível' : 'Reprovar pedido'" width="480px" persistent>
      <p class="msg">
        Valor: <strong>{{ formatarMoeda(decisaoPedido?.valorTotal ?? null) }}</strong> ·
        Nível {{ decisaoPedido?.nivelAtual ?? '-' }} de {{ decisaoPedido?.quantidadeNiveis ?? '-' }}.
      </p>
      <div class="form-grid">
        <div class="col-12"><TextField v-model="decisaoJustificativa" label="Justificativa (opcional)" /></div>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="acaoEmCurso" @click="decisaoVisivel = false">Voltar</button>
        <button type="button" :class="decisaoTipo === 'aprovar' ? 'btn btn-primary' : 'btn btn-danger'" :disabled="acaoEmCurso" @click="confirmarDecisao">
          <span v-if="acaoEmCurso" class="spinner"></span>
          <span v-else>{{ decisaoTipo === 'aprovar' ? 'Aprovar' : 'Reprovar' }}</span>
        </button>
      </template>
    </AppDialog>

    <!-- Diálogo de regra -->
    <AppDialog v-model="regraVisivel" :title="editandoRegraId ? 'Editar Regra de Alçada' : 'Nova Regra de Alçada'" width="560px" persistent>
      <div class="form-grid">
        <div class="col-4"><TextField v-model="regraForm.nivel" label="Nível" /></div>
        <div class="col-4"><MoneyInput v-model="regraForm.valorMinimo" label="Valor mínimo" /></div>
        <div class="col-4"><MoneyInput v-model="regraForm.valorMaximo" label="Valor máximo" /></div>
        <div class="col-6"><TextField v-model="regraForm.categoriaCompra" label="Categoria (opcional)" /></div>
        <div class="col-6"><TextField v-model="regraForm.papelAprovador" label="Papel do aprovador (opcional)" /></div>
        <div class="col-6"><TextField v-model="regraForm.compradorId" label="Comprador (ID, opcional)" /></div>
        <div class="col-6"><TextField v-model="regraForm.aprovadorId" label="Aprovador (ID, opcional)" /></div>
        <div class="col-6">
          <SelectField
            :model-value="regraForm.ativo ? 1 : 0"
            :options="ATIVO_OPTIONS"
            label="Situação"
            :clearable="false"
            @update:model-value="(v) => (regraForm.ativo = Number(v) === 1)"
          />
        </div>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoRegra" @click="regraVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoRegra" @click="salvarRegra">
          <span v-if="salvandoRegra" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>

    <DeleteAlert v-model="excluirVisivel" :item-label="`nível ${regraExcluir?.nivel ?? ''}`" :loading="excluindo" @confirm="confirmarExclusaoRegra" />
    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.tabs { display: flex; gap: 4px; margin-bottom: 16px; border-bottom: 1px solid var(--border-color, rgba(255, 255, 255, 0.1)); }
.tab { background: none; border: none; padding: 10px 16px; cursor: pointer; color: var(--text-secondary); font-size: 14px; font-weight: 600; border-bottom: 2px solid transparent; margin-bottom: -1px; }
.tab.ativa { color: var(--primary); border-bottom-color: var(--primary); }
.msg { color: var(--text-secondary); font-size: 14px; margin-bottom: 12px; }
.text-muted { color: var(--text-secondary); }
.badge.st-ok { background: rgba(16, 185, 129, 0.1); border: 1px solid rgba(16, 185, 129, 0.3); color: var(--success); }
.badge.st-erro { background: rgba(239, 68, 68, 0.1); border: 1px solid rgba(239, 68, 68, 0.3); color: var(--danger); }
.badge.st-pendente { background: rgba(245, 158, 11, 0.1); border: 1px solid rgba(245, 158, 11, 0.3); color: var(--warning); }
.badge.st-cancelado { background: rgba(113, 113, 122, 0.1); border: 1px solid rgba(113, 113, 122, 0.3); color: #a1a1aa; }
</style>
