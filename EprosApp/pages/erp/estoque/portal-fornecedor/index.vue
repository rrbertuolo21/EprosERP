<script setup lang="ts">
/**
 * Portal do Fornecedor — visão interna (erp/estoque/portal-fornecedor).
 * Superfície B2B (PortalFornecedorController). Quatro abas:
 *   Convites:   GET /convites?status · POST /convites (convidar) · POST /convites/{id}/ativar (ativar acesso)
 *   Cotações:   GET /cotacoes?fornecedorId&status · POST /cotacoes/publicar (publicar cotação a um fornecedor)
 *   Pré-avisos: GET /pre-avisos?fornecedorId (ASN de embarque enviados pelo fornecedor)
 *   Documentos: GET /documentos?fornecedorId (documentos enviados pelo fornecedor)
 *
 * Cotações/Pré-avisos/Documentos exigem informar o fornecedor (isolamento por fornecedor — PFO-002).
 */
import { onMounted, reactive, ref, watch } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import { useEstoqueEnums, classeBadge } from '~/composables/useEstoqueEnums'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

const toast = useToast()
const { formatarData, formatarDataHora } = useHelper()
const { statusConvite, statusCotacao } = useEstoqueEnums()

type Aba = 'convites' | 'cotacoes' | 'preavisos' | 'documentos'
const aba = ref<Aba>('convites')
const processando = ref(false)

// --------- Convites ---------
interface Convite { id: string; fornecedorId: string; emailConvite: string; status: number; dataEnvio: string | null; dataExpiracao: string | null }
interface ConviteFiltros extends Record<string, unknown> { status?: number | null }
const listaConvites = useApiList<Convite, ConviteFiltros>('/estoque-portal-fornecedor/convites', { tamanhoPaginaInicial: 25 })
const camposConvite: FilterField[] = [{ key: 'status', label: 'Status', type: 'select', options: statusConvite.opcoes, grow: true }]
const colunasConvite: DataTableColumn<Convite>[] = [
  { key: 'emailConvite', label: 'E-mail do convite' },
  { key: 'fornecedorId', label: 'Fornecedor (ID)' },
  { key: 'dataEnvio', label: 'Enviado', width: '150px', formatter: (v) => formatarDataHora(v as string) || '-' },
  { key: 'dataExpiracao', label: 'Expira', width: '130px', formatter: (v) => formatarData(v as string) || '-' },
  { key: 'status', label: 'Status', align: 'center', width: '120px' }
]
function normalizarConvite(v: Record<string, unknown>): Partial<ConviteFiltros> {
  return { status: v.status === '' || v.status == null ? undefined : Number(v.status) }
}

// --------- Cotações / Pré-avisos / Documentos (exigem fornecedorId) ---------
const fornecedorId = ref('')

interface Cotacao { id: string; cotacaoOrigemId: string; status: number; prazoResposta: string | null; criadoEm: string }
const listaCotacoes = useApiList<Cotacao>('/estoque-portal-fornecedor/cotacoes', { tamanhoPaginaInicial: 25 })
const colunasCotacao: DataTableColumn<Cotacao>[] = [
  { key: 'cotacaoOrigemId', label: 'Cotação de origem (ID)' },
  { key: 'prazoResposta', label: 'Prazo resposta', width: '150px', formatter: (v) => formatarData(v as string) || '-' },
  { key: 'criadoEm', label: 'Criada em', width: '150px', formatter: (v) => formatarDataHora(v as string) },
  { key: 'status', label: 'Status', align: 'center', width: '120px' }
]

interface PreAviso { id: string; pedidoCompraId: string; status: number; dataPrevistaEntrega: string | null; criadoEm: string }
const listaPreAvisos = useApiList<PreAviso>('/estoque-portal-fornecedor/pre-avisos', { tamanhoPaginaInicial: 25 })
const colunasPreAviso: DataTableColumn<PreAviso>[] = [
  { key: 'pedidoCompraId', label: 'Pedido de compra (ID)' },
  { key: 'dataPrevistaEntrega', label: 'Entrega prevista', width: '150px', formatter: (v) => formatarData(v as string) || '-' },
  { key: 'criadoEm', label: 'Criado em', width: '150px', formatter: (v) => formatarDataHora(v as string) },
  { key: 'status', label: 'Status', align: 'center', width: '110px' }
]

interface Documento { id: string; referenciaTipo: number; referenciaId: string; tipoDocumento: string | null; arquivoId: string; status: number; enviadoEm: string | null }
const listaDocumentos = useApiList<Documento>('/estoque-portal-fornecedor/documentos', { tamanhoPaginaInicial: 25 })
const colunasDocumento: DataTableColumn<Documento>[] = [
  { key: 'tipoDocumento', label: 'Tipo', formatter: (v) => (v as string) || '-' },
  { key: 'referenciaId', label: 'Referência (ID)' },
  { key: 'arquivoId', label: 'Arquivo (ID)' },
  { key: 'enviadoEm', label: 'Enviado', width: '150px', formatter: (v) => formatarDataHora(v as string) || '-' },
  { key: 'status', label: 'Status', align: 'center', width: '110px' }
]

function buscarPorFornecedor() {
  if (!fornecedorId.value.trim()) {
    toast.error('Informe o ID do fornecedor.')
    return
  }
  const fid = fornecedorId.value.trim()
  if (aba.value === 'cotacoes') void listaCotacoes.aplicarFiltros({ fornecedorId: fid } as never)
  else if (aba.value === 'preavisos') void listaPreAvisos.aplicarFiltros({ fornecedorId: fid } as never)
  else if (aba.value === 'documentos') void listaDocumentos.aplicarFiltros({ fornecedorId: fid } as never)
}

// --------- Convidar (dialog) ---------
const convidarDialog = ref(false)
const formConvite = reactive({ fornecedorId: '', emailConvite: '', dataExpiracao: null as string | null })
const errosConvite = reactive<Record<string, string>>({})
function abrirConvidar() {
  Object.assign(formConvite, { fornecedorId: '', emailConvite: '', dataExpiracao: null })
  for (const k of Object.keys(errosConvite)) delete errosConvite[k]
  convidarDialog.value = true
}
async function salvarConvite() {
  for (const k of Object.keys(errosConvite)) delete errosConvite[k]
  if (!formConvite.fornecedorId.trim()) errosConvite.fornecedorId = 'Fornecedor é obrigatório.'
  if (!formConvite.emailConvite.trim()) errosConvite.emailConvite = 'E-mail é obrigatório.'
  if (Object.keys(errosConvite).length > 0) return
  processando.value = true
  try {
    await useApi('/estoque-portal-fornecedor/convites', {
      method: 'POST',
      body: { fornecedorId: formConvite.fornecedorId.trim(), emailConvite: formConvite.emailConvite.trim(), dataExpiracao: formConvite.dataExpiracao || null }
    })
    toast.success('Convite criado.')
    convidarDialog.value = false
    await listaConvites.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}

// --------- Ativar acesso (dialog) ---------
const ativarDialog = ref(false)
const conviteSelecionado = ref<Convite | null>(null)
const usuarioIdAtivacao = ref('')
function abrirAtivar(c: Convite) { conviteSelecionado.value = c; usuarioIdAtivacao.value = ''; ativarDialog.value = true }
async function confirmarAtivacao() {
  if (!conviteSelecionado.value || !usuarioIdAtivacao.value.trim()) {
    toast.error('Informe o ID do usuário.')
    return
  }
  processando.value = true
  try {
    await useApi('/estoque-portal-fornecedor/convites/{id}/ativar', {
      method: 'POST',
      params: { id: conviteSelecionado.value.id },
      body: { conviteId: conviteSelecionado.value.id, usuarioId: usuarioIdAtivacao.value.trim() }
    })
    toast.success('Acesso do fornecedor ativado.')
    ativarDialog.value = false
    await listaConvites.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}

// --------- Publicar cotação (dialog) ---------
const publicarDialog = ref(false)
const formPublicar = reactive({ cotacaoOrigemId: '', fornecedorId: '', prazoResposta: null as string | null })
const errosPublicar = reactive<Record<string, string>>({})
function abrirPublicar() {
  Object.assign(formPublicar, { cotacaoOrigemId: '', fornecedorId: fornecedorId.value.trim(), prazoResposta: null })
  for (const k of Object.keys(errosPublicar)) delete errosPublicar[k]
  publicarDialog.value = true
}
async function salvarPublicacao() {
  for (const k of Object.keys(errosPublicar)) delete errosPublicar[k]
  if (!formPublicar.cotacaoOrigemId.trim()) errosPublicar.cotacaoOrigemId = 'Cotação de origem é obrigatória.'
  if (!formPublicar.fornecedorId.trim()) errosPublicar.fornecedorId = 'Fornecedor é obrigatório.'
  if (Object.keys(errosPublicar).length > 0) return
  processando.value = true
  try {
    await useApi('/estoque-portal-fornecedor/cotacoes/publicar', {
      method: 'POST',
      body: { cotacaoOrigemId: formPublicar.cotacaoOrigemId.trim(), fornecedorId: formPublicar.fornecedorId.trim(), prazoResposta: formPublicar.prazoResposta || null }
    })
    toast.success('Cotação publicada ao fornecedor.')
    publicarDialog.value = false
    if (fornecedorId.value.trim()) buscarPorFornecedor()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}

watch(aba, (a) => {
  if (a === 'convites' && listaConvites.itens.value.length === 0) void listaConvites.buscar()
})

onMounted(() => void listaConvites.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Portal do Fornecedor" subtitle="Convites, cotações, pré-avisos (ASN) e documentos do fornecedor" :loading="processando">
      <template #actions>
        <button v-if="aba === 'convites'" type="button" class="btn btn-primary" @click="abrirConvidar">+ Convidar fornecedor</button>
        <button v-if="aba === 'cotacoes'" type="button" class="btn btn-primary" @click="abrirPublicar">+ Publicar cotação</button>
      </template>
    </PageToolbar>

    <div class="tabs">
      <button type="button" class="tab" :class="{ ativo: aba === 'convites' }" @click="aba = 'convites'">Convites</button>
      <button type="button" class="tab" :class="{ ativo: aba === 'cotacoes' }" @click="aba = 'cotacoes'">Cotações</button>
      <button type="button" class="tab" :class="{ ativo: aba === 'preavisos' }" @click="aba = 'preavisos'">Pré-avisos (ASN)</button>
      <button type="button" class="tab" :class="{ ativo: aba === 'documentos' }" @click="aba = 'documentos'">Documentos</button>
    </div>

    <!-- Convites -->
    <template v-if="aba === 'convites'">
      <FilterBar
        :fields="camposConvite"
        :model-value="listaConvites.filtros.value"
        :loading="listaConvites.carregando.value"
        @update:model-value="(v) => (listaConvites.filtros.value = v as typeof listaConvites.filtros.value)"
        @search="(v) => listaConvites.aplicarFiltros(normalizarConvite(v as Record<string, unknown>))"
        @clear="listaConvites.limpar()"
      />
      <DataTable
        :items="listaConvites.itens.value"
        :columns="colunasConvite"
        :total="listaConvites.total.value"
        :page="listaConvites.pagina.value"
        :page-size="listaConvites.tamanhoPagina.value"
        :loading="listaConvites.carregando.value"
        row-key="id"
        empty-text="Nenhum convite encontrado"
        @update:page="(p) => listaConvites.irParaPagina(p)"
        @update:page-size="(ps) => listaConvites.buscar({ tamanhoPagina: ps, pagina: 1 })"
      >
        <template #cell-status="{ value }">
          <span class="badge" :class="classeBadge(statusConvite.cor(value as number))">{{ statusConvite.label(value as number) }}</span>
        </template>
        <template #actions="{ row }">
          <button v-if="row.status === 1" type="button" class="btn btn-ghost btn-sm" @click.stop="abrirAtivar(row)">Ativar acesso</button>
        </template>
      </DataTable>
    </template>

    <!-- Abas por fornecedor -->
    <template v-else>
      <div class="fornecedor-bar glass-panel">
        <TextField v-model="fornecedorId" label="Fornecedor (ID)" placeholder="GUID do fornecedor" />
        <button type="button" class="btn btn-primary btn-sm" @click="buscarPorFornecedor">Buscar</button>
      </div>

      <DataTable
        v-if="aba === 'cotacoes'"
        :items="listaCotacoes.itens.value"
        :columns="colunasCotacao"
        :total="listaCotacoes.total.value"
        :page="listaCotacoes.pagina.value"
        :page-size="listaCotacoes.tamanhoPagina.value"
        :loading="listaCotacoes.carregando.value"
        row-key="id"
        empty-text="Informe um fornecedor e busque as cotações"
        @update:page="(p) => listaCotacoes.irParaPagina(p)"
        @update:page-size="(ps) => listaCotacoes.buscar({ tamanhoPagina: ps, pagina: 1 })"
      >
        <template #cell-status="{ value }">
          <span class="badge" :class="classeBadge(statusCotacao.cor(value as number))">{{ statusCotacao.label(value as number) }}</span>
        </template>
      </DataTable>

      <DataTable
        v-else-if="aba === 'preavisos'"
        :items="listaPreAvisos.itens.value"
        :columns="colunasPreAviso"
        :total="listaPreAvisos.total.value"
        :page="listaPreAvisos.pagina.value"
        :page-size="listaPreAvisos.tamanhoPagina.value"
        :loading="listaPreAvisos.carregando.value"
        row-key="id"
        empty-text="Informe um fornecedor e busque os pré-avisos"
        @update:page="(p) => listaPreAvisos.irParaPagina(p)"
        @update:page-size="(ps) => listaPreAvisos.buscar({ tamanhoPagina: ps, pagina: 1 })"
      />

      <DataTable
        v-else
        :items="listaDocumentos.itens.value"
        :columns="colunasDocumento"
        :total="listaDocumentos.total.value"
        :page="listaDocumentos.pagina.value"
        :page-size="listaDocumentos.tamanhoPagina.value"
        :loading="listaDocumentos.carregando.value"
        row-key="id"
        empty-text="Informe um fornecedor e busque os documentos"
        @update:page="(p) => listaDocumentos.irParaPagina(p)"
        @update:page-size="(ps) => listaDocumentos.buscar({ tamanhoPagina: ps, pagina: 1 })"
      />
    </template>

    <!-- Dialog convidar -->
    <AppDialog v-model="convidarDialog" title="Convidar fornecedor" width="480px">
      <div class="form-grid">
        <TextField v-model="formConvite.fornecedorId" label="Fornecedor (ID)" required :error="errosConvite.fornecedorId" />
        <TextField v-model="formConvite.emailConvite" label="E-mail do convite" type="email" required :error="errosConvite.emailConvite" />
        <DateTimeField v-model="formConvite.dataExpiracao" label="Data de expiração" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="processando" @click="convidarDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="processando" @click="salvarConvite">
          <span v-if="processando" class="spinner"></span><span v-else>Enviar convite</span>
        </button>
      </template>
    </AppDialog>

    <!-- Dialog ativar -->
    <AppDialog v-model="ativarDialog" title="Ativar acesso do fornecedor" width="440px">
      <p v-if="conviteSelecionado" class="dialog-sub">Convite: <strong>{{ conviteSelecionado.emailConvite }}</strong></p>
      <TextField v-model="usuarioIdAtivacao" label="Usuário (ID)" required />
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="processando" @click="ativarDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="processando" @click="confirmarAtivacao">
          <span v-if="processando" class="spinner"></span><span v-else>Ativar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Dialog publicar cotação -->
    <AppDialog v-model="publicarDialog" title="Publicar cotação ao fornecedor" width="480px">
      <div class="form-grid">
        <TextField v-model="formPublicar.cotacaoOrigemId" label="Cotação de origem (ID)" required :error="errosPublicar.cotacaoOrigemId" />
        <TextField v-model="formPublicar.fornecedorId" label="Fornecedor (ID)" required :error="errosPublicar.fornecedorId" />
        <DateTimeField v-model="formPublicar.prazoResposta" label="Prazo de resposta" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="processando" @click="publicarDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="processando" @click="salvarPublicacao">
          <span v-if="processando" class="spinner"></span><span v-else>Publicar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.tabs { display: flex; gap: 4px; margin: 8px 0 12px; flex-wrap: wrap; }
.tab { padding: 8px 16px; border: 1px solid var(--border-color); background: transparent; color: var(--text-secondary); border-radius: 8px; cursor: pointer; font-size: 13px; }
.tab.ativo { background: var(--primary); color: #fff; border-color: var(--primary); }
.fornecedor-bar { display: flex; align-items: flex-end; gap: 12px; padding: 12px 16px; margin-bottom: 12px; }
.fornecedor-bar .field { flex: 1; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; margin-bottom: 12px; }
.dialog-sub { font-size: 13px; color: var(--text-secondary); margin-bottom: 12px; }
.badge-success { background: rgba(16, 185, 129, 0.1); color: var(--success); border: 1px solid rgba(16, 185, 129, 0.25); }
.badge-danger { background: rgba(239, 68, 68, 0.1); color: var(--danger); border: 1px solid rgba(239, 68, 68, 0.25); }
.badge-warning { background: rgba(245, 158, 11, 0.12); color: var(--warning); border: 1px solid rgba(245, 158, 11, 0.25); }
.badge-info { background: rgba(59, 130, 246, 0.1); color: #3b82f6; border: 1px solid rgba(59, 130, 246, 0.25); }
.badge-muted { background: rgba(120, 120, 130, 0.1); color: var(--text-muted); border: 1px solid var(--border-color); }
</style>
