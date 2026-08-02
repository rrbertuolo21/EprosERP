<script setup lang="ts">
/**
 * Devolução de Compra — lista (erp/compras/devolucao).
 *
 * Camada de apresentação sobre `DevolucoesCompraController` (`/api/v1/compras-devolucoes`):
 *   - listagem paginada com filtro por status;
 *   - confirmar / cancelar / excluir devolução;
 *   - iniciar nova devolução a partir de uma compra de origem (busca em /compras).
 *
 * Endpoints: compras-devolucoes (GET, {id}/confirmar, {id}/cancelar, DELETE {id}); compras (GET).
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
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

interface Devolucao {
  id: string
  numero: string | null
  compraOrigemId: string | null
  fornecedorId: string | null
  dataDevolucao: string | null
  tipo: number | string | null
  status: number | string | null
  cfop: string | null
  total: number | null
  criadoEm: string | null
  confirmadaEm: string | null
}

const STATUS_OPTIONS = [
  { label: 'Rascunho', value: '0' },
  { label: 'Confirmada', value: '1' },
  { label: 'Cancelada', value: '2' }
]
const STATUS_TEXTO: Record<string, { texto: string; classe: string }> = {
  '0': { texto: 'Rascunho', classe: 'pendente' },
  '1': { texto: 'Confirmada', classe: 'ok' },
  '2': { texto: 'Cancelada', classe: 'cancelado' }
}
const TIPO_TEXTO: Record<string, string> = { '0': 'Total', '1': 'Parcial' }

const toast = useToast()
const { formatarData, formatarMoeda } = useHelper()

const itens = ref<Devolucao[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(20)
const carregando = ref(false)
const filtroStatus = ref('')

const filtrosForm = ref<Record<string, unknown>>({ status: '' })
const camposFiltro: FilterField[] = [
  { key: 'status', label: 'Status', type: 'select', options: STATUS_OPTIONS, grow: true }
]

const colunas: DataTableColumn<Devolucao>[] = [
  { key: 'numero', label: 'Número', width: '130px', formatter: (v) => (v as string | null) ?? '-' },
  { key: 'fornecedorId', label: 'Fornecedor', formatter: (v) => (v ? `Forn. ${String(v).slice(0, 8)}` : '-') },
  { key: 'tipo', label: 'Tipo', align: 'center', width: '100px', formatter: (v) => TIPO_TEXTO[String(v)] ?? '-' },
  { key: 'dataDevolucao', label: 'Data', width: '120px', formatter: (v) => formatarData(v as string | null) },
  { key: 'total', label: 'Total', align: 'right', width: '130px', formatter: (v) => formatarMoeda(v as number | null) },
  { key: 'status', label: 'Status', align: 'center', width: '130px' }
]

async function buscar(): Promise<void> {
  carregando.value = true
  try {
    const query: Record<string, unknown> = { pagina: pagina.value, tamanhoPagina: tamanhoPagina.value }
    if (filtroStatus.value !== '') query.status = Number(filtroStatus.value)
    const resposta = await useApi('/compras-devolucoes', { query })
    const dados = extrairDados<{ total: number; itens: Devolucao[] }>(resposta)
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
  deb = setTimeout(() => { filtroStatus.value = (v.status as string) ?? ''; pagina.value = 1; void buscar() }, 300)
}
function aoBuscar(v: Record<string, unknown>) {
  if (deb) clearTimeout(deb)
  filtroStatus.value = (v.status as string) ?? ''
  pagina.value = 1
  void buscar()
}
function aoLimpar() {
  filtrosForm.value = { status: '' }
  filtroStatus.value = ''
  pagina.value = 1
  void buscar()
}

function abrir(item: Devolucao) {
  navigateTo(`/erp/compras/devolucao/${item.id}`)
}
function ehRascunho(d: Devolucao): boolean {
  return String(d.status) === '0'
}

const acaoEmCurso = ref(false)
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

async function confirmar(d: Devolucao) {
  const ok = await confirmRef.value?.open('Confirmar devolução', `Confirmar a devolução ${d.numero ?? ''}? A movimentação fiscal/estoque será efetivada.`, { textoConfirmar: 'Confirmar', textoCancelar: 'Voltar' })
  if (!ok) return
  acaoEmCurso.value = true
  try {
    await useApi('/compras-devolucoes/{id}/confirmar', { method: 'POST', params: { id: d.id } })
    toast.success('Devolução confirmada')
    await buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmCurso.value = false
  }
}
async function cancelar(d: Devolucao) {
  const ok = await confirmRef.value?.open('Cancelar devolução', `Cancelar a devolução ${d.numero ?? ''}?`, { danger: true, textoConfirmar: 'Cancelar', textoCancelar: 'Voltar' })
  if (!ok) return
  acaoEmCurso.value = true
  try {
    await useApi('/compras-devolucoes/{id}/cancelar', { method: 'POST', params: { id: d.id } })
    toast.success('Devolução cancelada')
    await buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmCurso.value = false
  }
}

const excluirVisivel = ref(false)
const excluindo = ref(false)
const paraExcluir = ref<Devolucao | null>(null)
function pedirExclusao(d: Devolucao) {
  paraExcluir.value = d
  excluirVisivel.value = true
}
async function confirmarExclusao() {
  if (!paraExcluir.value) return
  excluindo.value = true
  try {
    await useApi('/compras-devolucoes/{id}', { method: 'DELETE', params: { id: paraExcluir.value.id } })
    toast.success('Devolução excluída')
    excluirVisivel.value = false
    await buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

// --- Nova devolução: escolher compra de origem ---
const origemVisivel = ref(false)
const termo = ref('')
const buscandoCompra = ref(false)
const resultados = ref<{ id: string; numeroNota: string | null; fornecedorNome: string | null }[]>([])
let debC: ReturnType<typeof setTimeout> | undefined
function abrirNova() {
  termo.value = ''
  resultados.value = []
  origemVisivel.value = true
}
function aoDigitar(v: string) {
  termo.value = v
  if (debC) clearTimeout(debC)
  if (!v || v.trim().length < 2) { resultados.value = []; return }
  debC = setTimeout(() => void buscarCompra(v.trim()), 400)
}
async function buscarCompra(t: string) {
  buscandoCompra.value = true
  try {
    const resposta = await useApi('/compras', { query: { localizar: t, pagina: 1, tamanhoPagina: 20 } })
    const dados = extrairDados<{ itens: typeof resultados.value }>(resposta)
    resultados.value = dados?.itens ?? []
  } catch (e) {
    resultados.value = []
    console.error('[devolucao] busca compra', e)
  } finally {
    buscandoCompra.value = false
  }
}
function escolherOrigem(c: { id: string }) {
  origemVisivel.value = false
  navigateTo(`/erp/compras/devolucao/novo?origem=${c.id}`)
}

onMounted(() => void buscar())
</script>

<template>
  <div>
    <PageToolbar title="Devolução de Compra" subtitle="Devolução de mercadoria ao fornecedor (total/parcial)" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirNova">+ Nova Devolução</button>
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
      empty-text="Nenhuma devolução de compra"
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
        <template v-if="ehRascunho(row)">
          <button type="button" class="btn btn-ghost btn-sm" title="Confirmar" :disabled="acaoEmCurso" @click.stop="confirmar(row)">✔</button>
          <button type="button" class="btn btn-ghost btn-sm" title="Cancelar" :disabled="acaoEmCurso" @click.stop="cancelar(row)">⊘</button>
          <button type="button" class="btn btn-ghost btn-sm" title="Excluir" @click.stop="pedirExclusao(row)">🗑</button>
        </template>
      </template>
    </DataTable>

    <AppDialog v-model="origemVisivel" title="Nova Devolução — escolher compra de origem" width="640px">
      <div class="field">
        <label class="field-label">Compra de origem</label>
        <input class="input" type="text" placeholder="Fornecedor, número da nota ou chave (mín. 2 caracteres)" :value="termo" @input="aoDigitar(($event.target as HTMLInputElement).value)" />
      </div>
      <div v-if="buscandoCompra" class="aviso">Buscando...</div>
      <ul v-else-if="resultados.length" class="lista">
        <li v-for="c in resultados" :key="c.id" class="item" @click="escolherOrigem(c)">
          <span class="item-nome">{{ c.fornecedorNome || 'Fornecedor não identificado' }}</span>
          <span class="item-meta">Nota {{ c.numeroNota || '-' }}</span>
        </li>
      </ul>
      <p v-else-if="termo.length >= 2" class="aviso">Nenhuma compra encontrada.</p>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="origemVisivel = false">Fechar</button>
      </template>
    </AppDialog>

    <DeleteAlert v-model="excluirVisivel" :item-label="paraExcluir?.numero ?? ''" :loading="excluindo" @confirm="confirmarExclusao" />
    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.aviso { color: var(--text-secondary); font-size: 13px; margin: 8px 0; }
.lista { list-style: none; margin: 8px 0 0; padding: 0; max-height: 320px; overflow-y: auto; }
.item { display: flex; align-items: center; justify-content: space-between; padding: 10px 12px; border-radius: 8px; cursor: pointer; }
.item:hover { background: rgba(255, 255, 255, 0.06); }
.item-nome { font-weight: 600; font-size: 14px; }
.item-meta { font-size: 12px; color: var(--text-secondary); }
.badge.st-ok { background: rgba(16, 185, 129, 0.1); border: 1px solid rgba(16, 185, 129, 0.3); color: var(--success); }
.badge.st-pendente { background: rgba(245, 158, 11, 0.1); border: 1px solid rgba(245, 158, 11, 0.3); color: var(--warning); }
.badge.st-cancelado { background: rgba(113, 113, 122, 0.1); border: 1px solid rgba(113, 113, 122, 0.3); color: #a1a1aa; }
</style>
