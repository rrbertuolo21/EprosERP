<script setup lang="ts">
/**
 * Importar XML (fatia 13) — `erp/integracao/importar-xml`.
 *
 * Porta o comportamento da tela legada `integracao/importar-xml.vue`: envio de um arquivo XML
 * (ou .ZIP com múltiplos XMLs) para processamento e listagem dos documentos importados, com
 * atualização automática (polling) enquanto houver itens em processamento.
 *
 * IO exclusivamente pelo cliente compartilhado `useApi`. A listagem consome `fiscal/documentos`
 * (documentos fiscais já processados, endpoint real).
 *
 * `POST compras/importar-xml` (multipart/form-data, campo "arquivo") importa a NF-e de entrada
 * e persiste o agregado de compra completo (`ComprasController.ImportarXml`).
 *
 * Endpoints: compras/importar-xml (real), fiscal/documentos (real).
 */
import { computed, onMounted, onBeforeUnmount, ref } from 'vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

const toast = useToast()
const { formatarDataHora, formatarMoeda } = useHelper()

/** Linha da tabela de documentos importados (retorno de fiscal/documentos). */
interface DocumentoFiscal extends Record<string, unknown> {
  id: string
  modelo?: string
  status?: string
  chaveAcesso?: string
  protocolo?: string
  urlDanfe?: string | null
  urlXml?: string | null
  total?: number
  destinatarioNome?: string
  destinatarioCnpjCpf?: string
  dataEmissao?: string
}

/** Resposta de `fiscal/documentos` (envelope próprio: Total/Pagina/Itens, PascalCase). */
interface RespostaDocumentos {
  Total?: number
  total?: number
  Itens?: DocumentoFiscal[]
  itens?: DocumentoFiscal[]
}

// --- Upload ---
const arquivo = ref<File | null>(null)
const enviando = ref(false)
const inputRef = ref<HTMLInputElement | null>(null)

function aoSelecionarArquivo(ev: Event) {
  const arquivos = (ev.target as HTMLInputElement).files
  arquivo.value = arquivos && arquivos.length ? arquivos[0] : null
}

async function enviar() {
  if (!arquivo.value) {
    toast.error('Selecione um arquivo XML ou .ZIP')
    return
  }
  enviando.value = true
  try {
    const formData = new FormData()
    formData.append('arquivo', arquivo.value)
    await useApi('/compras/importar-xml', { method: 'POST', body: formData as unknown as Record<string, unknown> })
    toast.success('XML enviado para processamento')
    arquivo.value = null
    if (inputRef.value) inputRef.value.value = ''
    await buscar()
    iniciarPolling()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    enviando.value = false
  }
}

// --- Listagem (paginação local; fiscal/documentos usa envelope Total/Itens próprio) ---
const itens = ref<DocumentoFiscal[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(20)
const carregando = ref(false)

const colunas: DataTableColumn<DocumentoFiscal>[] = [
  { key: 'chaveAcesso', label: 'Chave' },
  { key: 'modelo', label: 'Modelo' },
  { key: 'destinatarioNome', label: 'Parceiro' },
  { key: 'total', label: 'Valor', align: 'right', formatter: (v) => formatarMoeda(v as number) },
  { key: 'status', label: 'Status' },
  { key: 'dataEmissao', label: 'Emissão', formatter: (v) => (v ? formatarDataHora(v as string) : '') }
]

/** Há documentos ainda em processamento (status pendente/processando)? */
const temProcessando = computed(() =>
  itens.value.some((d) => {
    const s = (d.status ?? '').toLowerCase()
    return s.includes('process') || s.includes('pend') || s.includes('aguard')
  })
)

async function buscar() {
  carregando.value = true
  try {
    const resp = await useApi<RespostaDocumentos>('/fiscal/documentos', {
      query: { pagina: pagina.value, tamanhoPagina: tamanhoPagina.value }
    })
    itens.value = resp?.Itens ?? resp?.itens ?? []
    total.value = resp?.Total ?? resp?.total ?? itens.value.length
  } catch (e) {
    console.error('[importar-xml] falha ao listar documentos', e)
    itens.value = []
    total.value = 0
  } finally {
    carregando.value = false
  }
}

function statusClasse(status?: string): string {
  const s = (status ?? '').toLowerCase()
  if (s.includes('autoriz') || s.includes('conclu')) return 'chip-success'
  if (s.includes('erro') || s.includes('rejeit') || s.includes('falha')) return 'chip-danger'
  if (s.includes('process') || s.includes('pend') || s.includes('aguard')) return 'chip-warning'
  return ''
}

async function onPagina(p: number) {
  pagina.value = p
  await buscar()
}

async function onTamanho(t: number) {
  tamanhoPagina.value = t
  pagina.value = 1
  await buscar()
}

// --- Polling enquanto houver itens em processamento ---
let timer: ReturnType<typeof setInterval> | null = null

function iniciarPolling() {
  pararPolling()
  timer = setInterval(() => {
    if (!temProcessando.value) {
      pararPolling()
      return
    }
    void buscar()
  }, 10000)
}

function pararPolling() {
  if (timer) {
    clearInterval(timer)
    timer = null
  }
}

onMounted(async () => {
  await buscar()
  if (temProcessando.value) iniciarPolling()
})

onBeforeUnmount(pararPolling)
</script>

<template>
  <div class="importar-page">
    <PageToolbar title="Importar XML" subtitle="Integração · Importação de XML" :loading="carregando" />

    <!-- Área de upload -->
    <section class="glass-panel upload-card">
      <div class="upload-row">
        <div class="field upload-field">
          <label class="field-label">Arquivo XML da NF-e de entrada</label>
          <input
            ref="inputRef"
            type="file"
            class="input"
            accept="application/xml,text/xml,.xml"
            :disabled="enviando"
            @change="aoSelecionarArquivo"
          />
          <span v-if="arquivo" class="upload-nome">{{ arquivo.name }}</span>
        </div>
        <button
          type="button"
          class="btn btn-primary"
          :disabled="enviando || !arquivo"
          @click="enviar"
        >
          <span v-if="enviando" class="spinner"></span>
          <span v-else>Importar XML</span>
        </button>
      </div>
    </section>

    <!-- Documentos importados -->
    <DataTable
      :items="itens"
      :columns="colunas"
      :total="total"
      :page="pagina"
      :page-size="tamanhoPagina"
      :loading="carregando"
      row-key="id"
      empty-text="Nenhum documento importado."
      @update:page="onPagina"
      @update:page-size="onTamanho"
    >
      <template #cell-status="{ row }">
        <span class="chip" :class="statusClasse((row as DocumentoFiscal).status)">
          {{ (row as DocumentoFiscal).status ?? '—' }}
        </span>
      </template>
      <template #cell-chaveAcesso="{ row }">
        <span class="chave-cel">{{ (row as DocumentoFiscal).chaveAcesso ?? '—' }}</span>
      </template>
    </DataTable>
  </div>
</template>

<style scoped>
.importar-page { padding-bottom: 40px; }
.upload-card { padding: 18px 20px; margin-bottom: 16px; }
.upload-row { display: flex; align-items: flex-end; gap: 14px; flex-wrap: wrap; }
.upload-field { flex: 1; min-width: 260px; }
.upload-nome { font-size: 12px; color: var(--text-muted); margin-top: 4px; }
.chave-cel { font-family: monospace; font-size: 12px; word-break: break-all; }
</style>
