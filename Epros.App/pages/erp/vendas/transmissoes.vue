<script setup lang="ts">
/**
 * Monitor de Transmissões SEFAZ (fatia 11) — `erp/vendas/transmissoes`.
 *
 * Porta a tela legada `vendas/transmissoes.vue` para o design novo: listagem paginada das
 * vendas fiscais (NF-e/NFC-e) por período/status/chave, com ações por linha — visualizar a
 * DANFE, gerar PDF, editar rascunho/rejeitada, cancelar documento autorizado, iniciar
 * devolução/retorno, baixar carta de correção — e exportação da planilha do período.
 *
 * Listagem via `useApiList` (dentro de `useTransmissoes`) + `DataTable`. DANFE via `DanfeViewer`.
 * Endpoints: `vendas-fiscal` (lista/cancelamento), `fiscal/documentos` (DANFE/PDF/CCe),
 * `.../nfe/referenciadas` (devolução).
 */
import { computed, onMounted, ref } from 'vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DanfeViewer from '~/components/shared/DanfeViewer.vue'
import CancelamentoDialog from '~/components/vendas-transmissoes/CancelamentoDialog.vue'
import {
  useTransmissoes,
  EXPORTAR_PLANILHA_EM_IMPLEMENTACAO
} from '~/components/vendas-transmissoes/useTransmissoes'
import { useHelper } from '~/composables/useHelper'
import { useEnum } from '~/composables/useEnum'
import { useToast } from '~/composables/useToast'
import {
  MODELOS_FISCAIS,
  NOME_MODELO,
  StatusDfe,
  documentoVigente,
  rotuloStatus,
  classeChipStatus,
  type TransmissaoItem
} from '~/components/vendas-transmissoes/transmissoesTypes'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

const router = useRouter()
const toast = useToast()
const { formatarDataHora, formatarMoeda } = useHelper()
const { carregarOpcoes } = useEnum()

const {
  itens,
  total,
  pagina,
  tamanhoPagina,
  ordenacao,
  filtros,
  carregando,
  baixando,
  cancelando,
  buscar,
  aplicar,
  limpar,
  baixarDanfe,
  gerarPdf,
  enviarCartaCorrecao,
  cancelar,
  exportarPlanilha
} = useTransmissoes()

// --- Filtros ---
const statusOpcoes = ref<{ label: string; value: number | string }[]>([])

const campos = computed<FilterField[]>(() => [
  { key: 'dataVendaInicial', label: 'Data inicial', type: 'date' },
  { key: 'dataVendaFinal', label: 'Data final', type: 'date' },
  { key: 'status', label: 'Status', type: 'select', options: statusOpcoes.value },
  { key: 'modeloFiscal', label: 'Modelo', type: 'select', options: MODELOS_FISCAIS },
  { key: 'chave', label: 'Chave', type: 'text', placeholder: 'Chave de acesso', grow: true },
  { key: 'nrNf', label: 'Nº da NF', type: 'text' },
  { key: 'cfop', label: 'CFOP', type: 'text' }
])

function aoBuscar(valores: Record<string, unknown>) {
  void aplicar(valores)
}

function aoLimpar() {
  void limpar()
}

// --- Colunas da tabela ---
const colunas: DataTableColumn<TransmissaoItem>[] = [
  { key: 'dataEmissao', label: 'Emissão' },
  { key: 'numero', label: 'Número', align: 'right' },
  { key: 'destinatario', label: 'Destinatário' },
  { key: 'valor', label: 'Valor', align: 'right' },
  { key: 'modeloFiscal', label: 'Modelo', align: 'center' },
  { key: 'chave', label: 'Chave', align: 'center' },
  { key: 'status', label: 'Status', align: 'center' }
]

function dataEmissao(item: TransmissaoItem): string {
  const doc = documentoVigente(item)
  return doc?.dataHoraEmissao ? formatarDataHora(doc.dataHoraEmissao) : '—'
}

function numeroDoc(item: TransmissaoItem): string {
  const doc = documentoVigente(item)
  return doc?.numero ? String(doc.numero) : '—'
}

function chaveDoc(item: TransmissaoItem): string {
  const doc = documentoVigente(item)
  return doc?.chave ?? '—'
}

function statusInterno(item: TransmissaoItem): number | null {
  const doc = documentoVigente(item)
  return doc?.statusInterno ?? item.status ?? null
}

function statusSefaz(item: TransmissaoItem): number | null {
  const doc = documentoVigente(item)
  return doc?.statusSefaz ?? null
}

function ehNfe(item: TransmissaoItem): boolean {
  return !!item.nfe
}

// --- DANFE viewer ---
const danfeVisivel = ref(false)
const danfeSrc = ref<Blob | string | null>(null)
const danfeTitulo = ref('DANFE')

async function visualizar(item: TransmissaoItem) {
  const blob = await baixarDanfe(item)
  if (blob) {
    danfeSrc.value = blob
    danfeTitulo.value = `DANFE ${numeroDoc(item)}`
    danfeVisivel.value = true
  }
}

/** Gera/baixa o PDF (mesmo endpoint do DANFE) e exibe no visualizador. */
async function gerarPdfViewer(item: TransmissaoItem) {
  const blob = await gerarPdf(item)
  if (blob) {
    danfeSrc.value = blob
    danfeTitulo.value = `PDF ${numeroDoc(item)}`
    danfeVisivel.value = true
  }
}

/**
 * Envia a Carta de Correção Eletrônica (CC-e) para a NF-e. Solicita o texto da correção
 * (mín. 15 caracteres exigidos pelo backend) e dispara a emissão.
 */
async function enviarCce(item: TransmissaoItem) {
  const texto = window.prompt('Texto da carta de correção (mínimo 15 caracteres):')
  if (texto === null) return
  if (texto.trim().length < 15) {
    toast.warning('O texto da correção deve ter no mínimo 15 caracteres.')
    return
  }
  const ok = await enviarCartaCorrecao(item, texto.trim())
  if (ok) void buscar()
}

function baixarViewer() {
  if (!(danfeSrc.value instanceof Blob)) return
  const url = URL.createObjectURL(danfeSrc.value)
  const a = document.createElement('a')
  a.href = url
  a.download = `${danfeTitulo.value.replace(/\s+/g, '-').toLowerCase()}.pdf`
  a.click()
  URL.revokeObjectURL(url)
}

function imprimirViewer() {
  if (!(danfeSrc.value instanceof Blob)) return
  const url = URL.createObjectURL(danfeSrc.value)
  window.open(url, '_blank')?.focus()
}

// --- Ações de linha ---
function editar(item: TransmissaoItem) {
  if (ehNfe(item)) router.push(`/erp/vendas/emissao/nfe/${item.id}`)
  else router.push(`/erp/vendas/emissao/nfce/${item.id}`)
}

function devolver(item: TransmissaoItem) {
  const chave = item.nfe?.chave ?? ''
  router.push({
    path: '/erp/vendas/emissao/devolucao-retorno/nfe',
    query: { origemId: String(item.id), chaveOrigem: chave }
  })
}

// --- Cancelamento ---
const cancelamentoVisivel = ref(false)
const itemCancelamento = ref<TransmissaoItem | null>(null)

function abrirCancelamento(item: TransmissaoItem) {
  itemCancelamento.value = item
  cancelamentoVisivel.value = true
}

async function confirmarCancelamento(justificativa: string) {
  if (!itemCancelamento.value) return
  const ok = await cancelar(itemCancelamento.value, justificativa)
  if (ok) {
    cancelamentoVisivel.value = false
    itemCancelamento.value = null
  }
}

// --- Exportação ---
async function exportar() {
  const blob = await exportarPlanilha()
  if (!blob) return
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `vendas-${new Date().toISOString().slice(0, 10)}.xlsx`
  a.click()
  URL.revokeObjectURL(url)
  toast.success('Planilha exportada')
}

// --- Paginação/ordenação ---
function aoMudarPagina(p: number) {
  void buscar({ pagina: p })
}
function aoMudarTamanho(t: number) {
  void buscar({ tamanhoPagina: t, pagina: 1 })
}
function aoOrdenar(sort: { key: string; order: 'asc' | 'desc' } | null) {
  void buscar({ ordenacao: sort, pagina: 1 })
}

onMounted(async () => {
  // Enum de status de venda/DFe para o filtro (com fallback silencioso).
  try {
    statusOpcoes.value = await carregarOpcoes('/vendas-enums/venda-status-dfe')
  } catch {
    statusOpcoes.value = []
  }
  await buscar()
})
</script>

<template>
  <div class="transmissoes-page">
    <PageToolbar title="Monitor de Transmissões" subtitle="Vendas · SEFAZ" :loading="carregando">
      <template #actions>
        <button
          type="button"
          class="btn btn-secondary"
          :disabled="baixando || EXPORTAR_PLANILHA_EM_IMPLEMENTACAO"
          :title="EXPORTAR_PLANILHA_EM_IMPLEMENTACAO ? 'Em implementação' : undefined"
          @click="exportar"
        >
          Exportar planilha
        </button>
        <button type="button" class="btn btn-primary" @click="router.push('/erp/vendas/emissao/nfe')">
          Nova NF-e
        </button>
      </template>
    </PageToolbar>

    <FilterBar
      v-model="filtros"
      :fields="campos"
      :loading="carregando"
      @search="aoBuscar"
      @clear="aoLimpar"
    />

    <DataTable
      :items="itens"
      :columns="colunas"
      :total="total"
      :page="pagina"
      :page-size="tamanhoPagina"
      :sort="ordenacao"
      :loading="carregando"
      row-key="id"
      empty-text="Nenhuma transmissão encontrada."
      @update:page="aoMudarPagina"
      @update:page-size="aoMudarTamanho"
      @update:sort="aoOrdenar"
    >
      <template #cell-dataEmissao="{ row }">
        <span class="nowrap">{{ dataEmissao(row as TransmissaoItem) }}</span>
      </template>
      <template #cell-numero="{ row }">
        {{ numeroDoc(row as TransmissaoItem) }}
      </template>
      <template #cell-destinatario="{ row }">
        {{ (row as TransmissaoItem).destinatario?.razaoSocial ?? '—' }}
      </template>
      <template #cell-valor="{ row }">
        <span class="nowrap">{{ formatarMoeda((row as TransmissaoItem).total?.valorNotaFiscal ?? 0) }}</span>
      </template>
      <template #cell-modeloFiscal="{ row }">
        {{ NOME_MODELO[(row as TransmissaoItem).modeloFiscal] ?? (row as TransmissaoItem).modeloFiscal }}
      </template>
      <template #cell-chave="{ row }">
        <span class="chave">{{ chaveDoc(row as TransmissaoItem) }}</span>
      </template>
      <template #cell-status="{ row }">
        <span :class="classeChipStatus(row as TransmissaoItem)">{{ rotuloStatus(row as TransmissaoItem) }}</span>
      </template>

      <template #actions="{ row }">
        <div class="acoes">
          <!-- Rejeitado: ver erro (via tooltip do motivo) -->
          <span
            v-if="statusSefaz(row as TransmissaoItem) === StatusDfe.Rejeitado"
            class="acao-erro"
            :title="documentoVigente(row as TransmissaoItem)?.motivoRejeicaoSefaz ?? 'Rejeitado pela SEFAZ'"
          >
            ⚠
          </span>

          <!-- Editar rascunho/rejeitado/recebido -->
          <button
            v-if="statusInterno(row as TransmissaoItem) === StatusDfe.Rejeitado
              || statusInterno(row as TransmissaoItem) === StatusDfe.Recebido"
            type="button"
            class="btn btn-ghost btn-sm"
            title="Editar"
            @click="editar(row as TransmissaoItem)"
          >
            ✎
          </button>

          <!-- Visualizar/imprimir DANFE (autorizado ou cancelado) -->
          <button
            v-if="statusInterno(row as TransmissaoItem) === StatusDfe.Autorizado
              || statusInterno(row as TransmissaoItem) === StatusDfe.Cancelado"
            type="button"
            class="btn btn-ghost btn-sm"
            title="Visualizar DANFE"
            :disabled="baixando"
            @click="visualizar(row as TransmissaoItem)"
          >
            🖨
          </button>

          <!-- Gerar PDF (autorizado com chave) -->
          <button
            v-if="statusInterno(row as TransmissaoItem) === StatusDfe.Autorizado && documentoVigente(row as TransmissaoItem)?.chave"
            type="button"
            class="btn btn-ghost btn-sm"
            title="Gerar PDF"
            :disabled="baixando"
            @click="gerarPdfViewer(row as TransmissaoItem)"
          >
            📄
          </button>

          <!-- Cancelar (autorizado) -->
          <button
            v-if="statusInterno(row as TransmissaoItem) === StatusDfe.Autorizado"
            type="button"
            class="btn btn-ghost btn-sm acao-danger"
            title="Cancelar"
            @click="abrirCancelamento(row as TransmissaoItem)"
          >
            ✕
          </button>

          <!-- Devolução/Retorno (NF-e autorizada) -->
          <button
            v-if="ehNfe(row as TransmissaoItem) && statusInterno(row as TransmissaoItem) === StatusDfe.Autorizado"
            type="button"
            class="btn btn-ghost btn-sm"
            title="Devolução / Retorno"
            @click="devolver(row as TransmissaoItem)"
          >
            ↩
          </button>

          <!-- Enviar carta de correção (NF-e autorizada) -->
          <button
            v-if="ehNfe(row as TransmissaoItem) && statusInterno(row as TransmissaoItem) === StatusDfe.Autorizado && (row as TransmissaoItem).nfe?.chave"
            type="button"
            class="btn btn-ghost btn-sm"
            title="Enviar carta de correção"
            :disabled="baixando"
            @click="enviarCce(row as TransmissaoItem)"
          >
            ✉
          </button>
        </div>
      </template>
    </DataTable>

    <!-- DANFE / CCe -->
    <AppDialog v-model="danfeVisivel" :title="danfeTitulo" width="90%">
      <DanfeViewer
        :src="danfeSrc"
        :title="danfeTitulo"
        @download="baixarViewer"
        @print="imprimirViewer"
      />
    </AppDialog>

    <!-- Cancelamento -->
    <CancelamentoDialog
      v-model="cancelamentoVisivel"
      :documento="itemCancelamento ? numeroDoc(itemCancelamento) : null"
      :loading="cancelando"
      @confirmar="confirmarCancelamento"
    />
  </div>
</template>

<style scoped>
.transmissoes-page { padding-bottom: 40px; }
.nowrap { white-space: nowrap; }
.chave { font-family: monospace; font-size: 11px; word-break: break-all; }
.acoes { display: flex; gap: 4px; justify-content: center; align-items: center; }
.acao-erro { color: var(--warning, #f59e0b); font-size: 15px; cursor: help; }
.acao-danger { color: var(--danger); }
</style>
