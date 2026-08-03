<script setup lang="ts">
/**
 * XML Contador — Fiscal / XML Contador.
 *
 * Porta o comportamento de `fiscal/xml-contador/index.vue` do legado: busca de documentos
 * fiscais emitidos por mês/ano de referência e download em lote (ZIP) dos XMLs — com opção de
 * filtrar por CNPJ/CPF do destinatário e incluir os PDFs (DANFE) no pacote.
 *
 * Endpoint de listagem: `fiscal/documentos` (filtrado por mês/ano de emissão).
 * Endpoint de download em lote: NÃO EXISTE ainda em `fiscal/documentos` — o legado usa rotas
 * dedicadas `baixa-documentos-dfe/obter-por-referencia/{mes}/{ano}/{pagina}/{tamanho}` e
 * `baixa-documentos-dfe/download-documentos-contador` (POST, retorna blob ZIP) que não constam
 * no MAPA_FRONTEND para esta fatia. Ver nota ao final do arquivo (FALTA_BACKEND).
 */
import { ref, computed, onMounted } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface DocumentoFiscal {
  id: string
  crt?: number
  documentoEmitente?: string
  documentoDestinatario?: string
  uf?: string
  chave?: string
  protocolo?: string
  serie?: number
  numero?: number
  status?: string
  dataEmissao?: string
}

const toast = useToast()
const { formatarDataHora } = useHelper()

const meses: SelectOption[] = [
  { label: 'Janeiro', value: '01' },
  { label: 'Fevereiro', value: '02' },
  { label: 'Março', value: '03' },
  { label: 'Abril', value: '04' },
  { label: 'Maio', value: '05' },
  { label: 'Junho', value: '06' },
  { label: 'Julho', value: '07' },
  { label: 'Agosto', value: '08' },
  { label: 'Setembro', value: '09' },
  { label: 'Outubro', value: '10' },
  { label: 'Novembro', value: '11' },
  { label: 'Dezembro', value: '12' }
]

const anoAtual = new Date().getFullYear()
const anos: SelectOption[] = Array.from({ length: 10 }, (_, i) => {
  const ano = anoAtual - i
  return { label: String(ano), value: String(ano) }
})

// Mês/ano de referência: default = mês anterior ao atual, espelhando o legado.
const dataReferencia = new Date()
dataReferencia.setDate(1)
dataReferencia.setMonth(dataReferencia.getMonth() - 1)
const mes = ref(String(dataReferencia.getMonth() + 1).padStart(2, '0'))
const ano = ref(String(dataReferencia.getFullYear()))

const carregando = ref(false)
const documentos = ref<DocumentoFiscal[]>([])
const totalDocumentos = ref(0)

const colunas: DataTableColumn<DocumentoFiscal>[] = [
  { key: 'documentoEmitente', label: 'Documento Emitente', width: '140px' },
  { key: 'documentoDestinatario', label: 'Documento Destinatário', width: '140px' },
  { key: 'uf', label: 'UF', width: '70px' },
  { key: 'serie', label: 'Série', width: '70px' },
  { key: 'numero', label: 'Número', width: '90px' },
  { key: 'status', label: 'Status', width: '120px' },
  { key: 'dataEmissao', label: 'Emissão', formatter: (v) => formatarDataHora(v as string) }
]

async function buscar() {
  carregando.value = true
  try {
    const resposta = await useApi('/fiscal/documentos', {
      query: { mes: Number(mes.value), ano: Number(ano.value), tamanhoPagina: 200 }
    })
    documentos.value = extrairDados<DocumentoFiscal[]>(resposta) ?? []
    totalDocumentos.value = documentos.value.length
  } catch (e) {
    toast.error(obterMensagemErro(e))
    documentos.value = []
    totalDocumentos.value = 0
  } finally {
    carregando.value = false
  }
}

// ---- Modal de download em lote ----
const modalDownloadAberto = ref(false)
const informarDestinatario = ref(false)
const destinatario = ref('')
const incluirPdfs = ref(false)
const baixando = ref(false)

function abrirModalDownload() {
  modalDownloadAberto.value = true
}

const opcoesInformarDestinatario: SelectOption[] = [
  { label: 'Sim', value: 'sim' },
  { label: 'Não', value: 'nao' }
]
const informarDestinatarioSelect = computed({
  get: () => (informarDestinatario.value ? 'sim' : 'nao'),
  set: (v: string | number | null) => {
    informarDestinatario.value = v === 'sim'
  }
})

async function confirmarDownload() {
  baixando.value = true
  try {
    const resposta = await useApi<Blob>('/fiscal/documentos/download-contador', {
      method: 'POST',
      body: {
        mes: Number(mes.value),
        ano: Number(ano.value),
        documentoDestinatario: informarDestinatario.value ? destinatario.value : '',
        incluiPdfs: incluirPdfs.value
      },
      responseType: 'blob'
    })

    if (resposta instanceof Blob) {
      const url = URL.createObjectURL(resposta)
      const a = document.createElement('a')
      a.href = url
      a.download = `XMLS-${incluirPdfs.value ? 'com-pdfs' : 'sem-pdfs'}-${mes.value}-${ano.value}.zip`
      document.body.appendChild(a)
      a.click()
      a.remove()
      URL.revokeObjectURL(url)
      toast.success('Download iniciado.')
      modalDownloadAberto.value = false
    } else {
      toast.error('Resposta inválida do servidor para o download.')
    }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    baixando.value = false
  }
}

onMounted(() => {
  void buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Baixar XMLs para o Contador" subtitle="Documentos fiscais emitidos no período, para envio ao contador" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="abrirModalDownload">Baixar XMLs</button>
      </template>
    </PageToolbar>

    <div class="glass-panel filtro-periodo">
      <SelectField v-model="mes" label="Mês" :options="meses" :clearable="false" />
      <SelectField v-model="ano" label="Ano" :options="anos" :clearable="false" />
      <button type="button" class="btn btn-primary" :disabled="carregando" @click="buscar">
        <span v-if="carregando" class="spinner"></span>
        <span v-else>Pesquisar</span>
      </button>
    </div>

    <DataTable
      :items="documentos"
      :columns="colunas"
      :total="totalDocumentos"
      :page="1"
      :page-size="200"
      :loading="carregando"
      empty-text="Nenhum documento fiscal encontrado para o período selecionado."
    />

    <AppDialog v-model="modalDownloadAberto" title="Baixar XMLs" width="480px">
      <div class="vertical-form">
        <SelectField v-model="informarDestinatarioSelect" label="Deseja informar o destinatário?" :options="opcoesInformarDestinatario" :clearable="false" />
        <TextField v-if="informarDestinatario" v-model="destinatario" label="Documento do Destinatário" placeholder="CPF ou CNPJ" />
        <label class="field-checkbox">
          <input v-model="incluirPdfs" type="checkbox" /> Incluir PDFs (DANFE)
        </label>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="baixando" @click="modalDownloadAberto = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="baixando" @click="confirmarDownload">
          <span v-if="baixando" class="spinner"></span>
          <span v-else>Baixar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.filtro-periodo {
  display: flex;
  align-items: flex-end;
  gap: 16px;
  padding: 16px;
  margin: 8px 0 16px;
}
.filtro-periodo > :first-child, .filtro-periodo > :nth-child(2) { width: 160px; }
.field-checkbox { display: flex; align-items: center; gap: 8px; font-size: 13.5px; color: var(--text-secondary); }
.field-checkbox input { width: 16px; height: 16px; accent-color: var(--primary); }
</style>
