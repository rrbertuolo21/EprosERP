<script setup lang="ts">
/**
 * Detalhe do Grupo de Consolidação (novo/edição) — Consolidação.
 *
 * Contrato:
 *   POST /consolidacao/grupos                          (codigo, nome, descricao)
 *   GET  /consolidacao/grupos/{id}                     (inclui Empresas)
 *   PUT  /consolidacao/grupos/{id}
 *   POST /consolidacao/grupos/{id}/empresas            (adiciona empresa ao grupo)
 *   DELETE /consolidacao/empresas/{vinculoId}          (remove vínculo)
 *   GET  /consolidacao/grupos/{id}/balancetes          + POST /consolidacao/balancetes + .../{id}/publicar
 *   GET  /consolidacao/grupos/{id}/demonstrativos      + POST /consolidacao/demonstrativos + .../{id}/publicar
 *   GET  /consolidacao/grupos/{id}/eliminacoes         + POST /consolidacao/eliminacoes
 *
 * Em modo `novo`, só o cabeçalho do grupo é editável; os sub-recursos aparecem após salvar.
 */
import { computed, reactive, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import type { SelectOption } from '~/composables/useEnum'
import { estadoConsolidacaoLabel, estadoConsolidacaoClasse } from '~/components/contabilidade-contas/enums'

definePageMeta({ layout: 'default' })

interface GrupoEmpresa {
  id: string
  empresaId: string
  dataInicio?: string | null
  dataFim?: string | null
}
interface GrupoForm {
  id?: string
  codigo: string
  nome: string
  descricao: string
  situacao?: number
  empresas?: GrupoEmpresa[]
}
interface EmpresaOpcao { id: string; razaoSocial?: string; nomeFantasia?: string }
interface Balancete { id: string; periodo?: string | null; estado: number; totalDebito?: number; totalCredito?: number; saldoFinal?: number }
interface Demonstrativo { id: string; periodo?: string | null; estado: number; dataPublicacao?: string | null; totalAgregado?: number }
interface Eliminacao { id: string; periodo?: string | null; empresaOrigemId?: string | null; empresaDestinoId?: string | null; valor?: number; descricao?: string | null }

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { formatarMoeda, formatarData } = useHelper()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const abaAtiva = ref<'empresas' | 'balancetes' | 'demonstrativos' | 'eliminacoes'>('empresas')

const form = reactive<GrupoForm>({
  id: isEdit.value ? idParam : undefined,
  codigo: '',
  nome: '',
  descricao: '',
  situacao: 0,
  empresas: []
})
const erros = reactive<Record<string, string>>({})

const empresas = ref<EmpresaOpcao[]>([])
const balancetes = ref<Balancete[]>([])
const demonstrativos = ref<Demonstrativo[]>([])
const eliminacoes = ref<Eliminacao[]>([])

const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const opcoesEmpresa = computed<SelectOption[]>(() =>
  empresas.value.map((e) => ({ label: e.nomeFantasia || e.razaoSocial || e.id, value: e.id }))
)

function nomeEmpresa(id?: string | null): string {
  if (!id) return '—'
  const e = empresas.value.find((x) => x.id === id)
  return e ? (e.nomeFantasia || e.razaoSocial || id) : id
}

/* ----- Grupo ----- */
function validarGrupo(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.nome?.trim()) erros.nome = 'Nome é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregarEmpresasDisponiveis() {
  try {
    const resposta = await useApi('/cadastros/empresas', { query: { tamanhoPagina: 200 } })
    const dados = extrairDados<{ itens?: EmpresaOpcao[] } | EmpresaOpcao[]>(resposta)
    empresas.value = Array.isArray(dados) ? dados : dados?.itens ?? []
  } catch (e) {
    console.error('[consolidacao/[id]] empresas', e)
  }
}

async function carregarGrupo() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/consolidacao/grupos/{id}`, { params: { id: idParam } })
    const dados = extrairDados<GrupoForm>(resposta)
    if (dados) {
      form.codigo = dados.codigo ?? ''
      form.nome = dados.nome ?? ''
      form.descricao = dados.descricao ?? ''
      form.situacao = dados.situacao ?? 0
      form.empresas = dados.empresas ?? []
    }
    await Promise.all([carregarBalancetes(), carregarDemonstrativos(), carregarEliminacoes()])
  } catch (e) {
    toast.error(obterMensagemErro(e))
    await router.push('/erp/contabilidade/consolidacao')
  } finally {
    carregando.value = false
  }
}

async function salvarGrupo() {
  if (!validarGrupo()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    if (isEdit.value) {
      await useApi(`/consolidacao/grupos/{id}`, {
        method: 'PUT',
        params: { id: idParam },
        body: { id: idParam, codigo: form.codigo || null, nome: form.nome || null, descricao: form.descricao || null }
      })
      toast.success('Grupo atualizado com sucesso!')
      await carregarGrupo()
    } else {
      const resposta = await useApi('/consolidacao/grupos', {
        method: 'POST',
        body: { codigo: form.codigo || null, nome: form.nome || null, descricao: form.descricao || null }
      })
      const criado = extrairDados<{ id?: string }>(resposta)
      toast.success('Grupo criado com sucesso!')
      if (criado?.id) await router.push(`/erp/contabilidade/consolidacao/${criado.id}`)
      else await router.push('/erp/contabilidade/consolidacao')
    }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/contabilidade/consolidacao')
}

/* ----- Empresas do grupo ----- */
const empresaDialogVisivel = ref(false)
const salvandoEmpresa = ref(false)
const novaEmpresa = reactive<{ empresaId: string | null; dataInicio: string | null; dataFim: string | null }>({
  empresaId: null, dataInicio: null, dataFim: null
})

function abrirEmpresaDialog() {
  novaEmpresa.empresaId = null
  novaEmpresa.dataInicio = null
  novaEmpresa.dataFim = null
  empresaDialogVisivel.value = true
}

async function adicionarEmpresa() {
  if (!novaEmpresa.empresaId) {
    toast.error('Selecione uma empresa.')
    return
  }
  salvandoEmpresa.value = true
  try {
    await useApi(`/consolidacao/grupos/{id}/empresas`, {
      method: 'POST',
      params: { id: idParam },
      body: {
        grupoConsolidacaoId: idParam,
        empresaId: novaEmpresa.empresaId,
        dataInicio: novaEmpresa.dataInicio,
        dataFim: novaEmpresa.dataFim
      }
    })
    toast.success('Empresa vinculada ao grupo.')
    empresaDialogVisivel.value = false
    await carregarGrupo()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoEmpresa.value = false
  }
}

async function removerEmpresa(vinculo: GrupoEmpresa) {
  const ok = await confirmRef.value!.open('Remover empresa', `Remover ${nomeEmpresa(vinculo.empresaId)} do grupo?`, { danger: true, textoConfirmar: 'Remover' })
  if (!ok) return
  try {
    await useApi(`/consolidacao/empresas/{vinculoId}`, { method: 'DELETE', params: { vinculoId: vinculo.id } })
    toast.success('Empresa removida do grupo.')
    await carregarGrupo()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

/* ----- Balancetes ----- */
async function carregarBalancetes() {
  try {
    const resposta = await useApi(`/consolidacao/grupos/{id}/balancetes`, { params: { id: idParam } })
    balancetes.value = extrairLista<Balancete>(resposta) ?? []
  } catch (e) {
    console.error('[consolidacao/[id]] balancetes', e)
  }
}

interface BalanceteLinha { codigoConta: string; nomeConta: string; saldoAnterior: number; debito: number; credito: number; saldoFinal: number }
const balanceteDialogVisivel = ref(false)
const salvandoBalancete = ref(false)
const novoBalancete = reactive<{ periodo: string; linhas: BalanceteLinha[] }>({ periodo: '', linhas: [] })

function abrirBalanceteDialog() {
  novoBalancete.periodo = ''
  novoBalancete.linhas = []
  balanceteDialogVisivel.value = true
}
function addLinhaBalancete() {
  novoBalancete.linhas.push({ codigoConta: '', nomeConta: '', saldoAnterior: 0, debito: 0, credito: 0, saldoFinal: 0 })
}
function delLinhaBalancete(i: number) { novoBalancete.linhas.splice(i, 1) }

async function salvarBalancete() {
  if (!novoBalancete.periodo.trim()) {
    toast.error('Período é obrigatório.')
    return
  }
  salvandoBalancete.value = true
  try {
    await useApi('/consolidacao/balancetes', {
      method: 'POST',
      body: {
        grupoConsolidacaoId: idParam,
        periodo: novoBalancete.periodo,
        linhas: novoBalancete.linhas.map((l) => ({
          codigoConta: l.codigoConta || null,
          nomeConta: l.nomeConta || null,
          saldoAnterior: l.saldoAnterior || 0,
          debito: l.debito || 0,
          credito: l.credito || 0,
          saldoFinal: l.saldoFinal || 0
        }))
      }
    })
    toast.success('Balancete consolidado criado.')
    balanceteDialogVisivel.value = false
    await carregarBalancetes()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoBalancete.value = false
  }
}

async function publicarBalancete(item: Balancete) {
  const ok = await confirmRef.value!.open('Publicar balancete', `Publicar o balancete do período ${item.periodo}? Após publicado, torna-se oficial.`)
  if (!ok) return
  try {
    await useApi(`/consolidacao/balancetes/{id}/publicar`, { method: 'POST', params: { id: item.id } })
    toast.success('Balancete publicado.')
    await carregarBalancetes()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

/* ----- Demonstrativos ----- */
async function carregarDemonstrativos() {
  try {
    const resposta = await useApi(`/consolidacao/grupos/{id}/demonstrativos`, { params: { id: idParam } })
    demonstrativos.value = extrairLista<Demonstrativo>(resposta) ?? []
  } catch (e) {
    console.error('[consolidacao/[id]] demonstrativos', e)
  }
}

interface DemoLinha { ordem: number; codigoLinha: string; descricaoLinha: string; valor: number; tipoLinha: string }
const demoDialogVisivel = ref(false)
const salvandoDemo = ref(false)
const novoDemo = reactive<{ periodo: string; linhas: DemoLinha[] }>({ periodo: '', linhas: [] })

function abrirDemoDialog() {
  novoDemo.periodo = ''
  novoDemo.linhas = []
  demoDialogVisivel.value = true
}
function addLinhaDemo() {
  novoDemo.linhas.push({ ordem: novoDemo.linhas.length + 1, codigoLinha: '', descricaoLinha: '', valor: 0, tipoLinha: '' })
}
function delLinhaDemo(i: number) { novoDemo.linhas.splice(i, 1) }

async function salvarDemo() {
  if (!novoDemo.periodo.trim()) {
    toast.error('Período é obrigatório.')
    return
  }
  salvandoDemo.value = true
  try {
    await useApi('/consolidacao/demonstrativos', {
      method: 'POST',
      body: {
        grupoConsolidacaoId: idParam,
        periodo: novoDemo.periodo,
        linhas: novoDemo.linhas.map((l) => ({
          ordem: l.ordem,
          codigoLinha: l.codigoLinha || null,
          descricaoLinha: l.descricaoLinha || null,
          valor: l.valor || 0,
          tipoLinha: l.tipoLinha || null
        }))
      }
    })
    toast.success('Demonstrativo criado.')
    demoDialogVisivel.value = false
    await carregarDemonstrativos()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoDemo.value = false
  }
}

async function publicarDemo(item: Demonstrativo) {
  const ok = await confirmRef.value!.open('Publicar demonstrativo', `Publicar o demonstrativo do período ${item.periodo}?`)
  if (!ok) return
  try {
    await useApi(`/consolidacao/demonstrativos/{id}/publicar`, {
      method: 'POST',
      params: { id: item.id },
      body: { id: item.id, usuarioPublicacaoId: null }
    })
    toast.success('Demonstrativo publicado.')
    await carregarDemonstrativos()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

/* ----- Eliminações ----- */
async function carregarEliminacoes() {
  try {
    const resposta = await useApi(`/consolidacao/grupos/{id}/eliminacoes`, { params: { id: idParam } })
    eliminacoes.value = extrairLista<Eliminacao>(resposta) ?? []
  } catch (e) {
    console.error('[consolidacao/[id]] eliminacoes', e)
  }
}

const eliminacaoDialogVisivel = ref(false)
const salvandoEliminacao = ref(false)
const novaEliminacao = reactive<{ periodo: string; empresaOrigemId: string | null; empresaDestinoId: string | null; valor: number; descricao: string }>({
  periodo: '', empresaOrigemId: null, empresaDestinoId: null, valor: 0, descricao: ''
})

function abrirEliminacaoDialog() {
  Object.assign(novaEliminacao, { periodo: '', empresaOrigemId: null, empresaDestinoId: null, valor: 0, descricao: '' })
  eliminacaoDialogVisivel.value = true
}

async function salvarEliminacao() {
  if (!novaEliminacao.periodo.trim()) {
    toast.error('Período é obrigatório.')
    return
  }
  salvandoEliminacao.value = true
  try {
    await useApi('/consolidacao/eliminacoes', {
      method: 'POST',
      body: {
        grupoConsolidacaoId: idParam,
        periodo: novaEliminacao.periodo,
        empresaOrigemId: novaEliminacao.empresaOrigemId,
        empresaDestinoId: novaEliminacao.empresaDestinoId,
        valor: novaEliminacao.valor || null,
        descricao: novaEliminacao.descricao || null
      }
    })
    toast.success('Eliminação intercompany registrada.')
    eliminacaoDialogVisivel.value = false
    await carregarEliminacoes()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoEliminacao.value = false
  }
}

onMounted(async () => {
  await carregarEmpresasDisponiveis()
  await carregarGrupo()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Grupo de consolidação' : 'Novo grupo de consolidação'" :loading="carregando || salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="cancelar">Voltar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvarGrupo">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div v-if="isEdit" class="grupo-situacao">
        <span class="resumo-label">Situação</span>
        <span class="badge" :class="`badge-${estadoConsolidacaoClasse(form.situacao)}`">{{ estadoConsolidacaoLabel(form.situacao) }}</span>
      </div>
      <div class="form-grid">
        <TextField v-model="form.codigo" label="Código" maxlength="30" />
        <TextField v-model="form.nome" label="Nome" required maxlength="120" :error="erros.nome" />
        <TextField v-model="form.descricao" label="Descrição" maxlength="200" />
      </div>
    </div>

    <div v-if="isEdit" class="glass-panel form-panel">
      <nav class="form-tabs">
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'empresas' }" @click="abaAtiva = 'empresas'">Empresas</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'balancetes' }" @click="abaAtiva = 'balancetes'">Balancetes</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'demonstrativos' }" @click="abaAtiva = 'demonstrativos'">Demonstrativos</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'eliminacoes' }" @click="abaAtiva = 'eliminacoes'">Eliminações</button>
      </nav>

      <!-- Empresas -->
      <section v-show="abaAtiva === 'empresas'" class="tab-content">
        <div class="tab-actions">
          <button type="button" class="btn btn-secondary btn-sm" @click="abrirEmpresaDialog">+ Vincular empresa</button>
        </div>
        <table class="admin-table">
          <thead><tr><th>Empresa</th><th>Início</th><th>Fim</th><th class="td-actions">Ações</th></tr></thead>
          <tbody>
            <tr v-if="!form.empresas?.length"><td colspan="4"><div class="table-empty">Nenhuma empresa vinculada.</div></td></tr>
            <tr v-for="v in form.empresas" :key="v.id">
              <td>{{ nomeEmpresa(v.empresaId) }}</td>
              <td>{{ v.dataInicio ? formatarData(v.dataInicio) : '—' }}</td>
              <td>{{ v.dataFim ? formatarData(v.dataFim) : '—' }}</td>
              <td class="td-actions"><button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="removerEmpresa(v)">Remover</button></td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- Balancetes -->
      <section v-show="abaAtiva === 'balancetes'" class="tab-content">
        <div class="tab-actions">
          <button type="button" class="btn btn-secondary btn-sm" @click="abrirBalanceteDialog">+ Novo balancete</button>
        </div>
        <table class="admin-table">
          <thead><tr><th>Período</th><th>Estado</th><th class="td-right">Débito</th><th class="td-right">Crédito</th><th class="td-right">Saldo Final</th><th class="td-actions">Ações</th></tr></thead>
          <tbody>
            <tr v-if="!balancetes.length"><td colspan="6"><div class="table-empty">Nenhum balancete consolidado.</div></td></tr>
            <tr v-for="b in balancetes" :key="b.id">
              <td>{{ b.periodo }}</td>
              <td><span class="badge" :class="`badge-${estadoConsolidacaoClasse(b.estado)}`">{{ estadoConsolidacaoLabel(b.estado) }}</span></td>
              <td class="td-right">{{ formatarMoeda(b.totalDebito ?? 0) }}</td>
              <td class="td-right">{{ formatarMoeda(b.totalCredito ?? 0) }}</td>
              <td class="td-right">{{ formatarMoeda(b.saldoFinal ?? 0) }}</td>
              <td class="td-actions">
                <button v-if="b.estado === 0" type="button" class="btn btn-ghost btn-sm" @click="publicarBalancete(b)">Publicar</button>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- Demonstrativos -->
      <section v-show="abaAtiva === 'demonstrativos'" class="tab-content">
        <div class="tab-actions">
          <button type="button" class="btn btn-secondary btn-sm" @click="abrirDemoDialog">+ Novo demonstrativo</button>
        </div>
        <table class="admin-table">
          <thead><tr><th>Período</th><th>Estado</th><th class="td-right">Total Agregado</th><th>Publicação</th><th class="td-actions">Ações</th></tr></thead>
          <tbody>
            <tr v-if="!demonstrativos.length"><td colspan="5"><div class="table-empty">Nenhum demonstrativo.</div></td></tr>
            <tr v-for="d in demonstrativos" :key="d.id">
              <td>{{ d.periodo }}</td>
              <td><span class="badge" :class="`badge-${estadoConsolidacaoClasse(d.estado)}`">{{ estadoConsolidacaoLabel(d.estado) }}</span></td>
              <td class="td-right">{{ formatarMoeda(d.totalAgregado ?? 0) }}</td>
              <td>{{ d.dataPublicacao ? formatarData(d.dataPublicacao) : '—' }}</td>
              <td class="td-actions">
                <button v-if="d.estado === 0" type="button" class="btn btn-ghost btn-sm" @click="publicarDemo(d)">Publicar</button>
              </td>
            </tr>
          </tbody>
        </table>
      </section>

      <!-- Eliminações -->
      <section v-show="abaAtiva === 'eliminacoes'" class="tab-content">
        <div class="tab-actions">
          <button type="button" class="btn btn-secondary btn-sm" @click="abrirEliminacaoDialog">+ Nova eliminação</button>
        </div>
        <table class="admin-table">
          <thead><tr><th>Período</th><th>Origem</th><th>Destino</th><th class="td-right">Valor</th><th>Descrição</th></tr></thead>
          <tbody>
            <tr v-if="!eliminacoes.length"><td colspan="5"><div class="table-empty">Nenhuma eliminação intercompany.</div></td></tr>
            <tr v-for="e in eliminacoes" :key="e.id">
              <td>{{ e.periodo }}</td>
              <td>{{ nomeEmpresa(e.empresaOrigemId) }}</td>
              <td>{{ nomeEmpresa(e.empresaDestinoId) }}</td>
              <td class="td-right">{{ formatarMoeda(e.valor ?? 0) }}</td>
              <td>{{ e.descricao || '—' }}</td>
            </tr>
          </tbody>
        </table>
      </section>
    </div>

    <p v-else class="hint-novo">Salve o grupo para gerir empresas, balancetes, demonstrativos e eliminações.</p>

    <!-- Dialog: vincular empresa -->
    <AppDialog v-model="empresaDialogVisivel" title="Vincular empresa ao grupo" width="480px">
      <div class="dialog-form">
        <SelectField v-model="novaEmpresa.empresaId" label="Empresa" required :options="opcoesEmpresa" />
        <DateTimeField v-model="novaEmpresa.dataInicio" label="Data de Início" />
        <DateTimeField v-model="novaEmpresa.dataFim" label="Data de Fim" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoEmpresa" @click="empresaDialogVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoEmpresa" @click="adicionarEmpresa">
          <span v-if="salvandoEmpresa" class="spinner"></span>
          <span v-else>Vincular</span>
        </button>
      </template>
    </AppDialog>

    <!-- Dialog: novo balancete -->
    <AppDialog v-model="balanceteDialogVisivel" title="Novo balancete consolidado" width="820px">
      <div class="dialog-form">
        <TextField v-model="novoBalancete.periodo" label="Período" required placeholder="Ex.: 2026-01 ou 2026" maxlength="20" />
        <div class="linhas-header">
          <span class="field-label">Linhas do balancete</span>
          <button type="button" class="btn btn-secondary btn-sm" @click="addLinhaBalancete">+ Linha</button>
        </div>
        <div v-for="(l, i) in novoBalancete.linhas" :key="i" class="linha-bal">
          <TextField v-model="l.codigoConta" label="Código" />
          <TextField v-model="l.nomeConta" label="Conta" />
          <MoneyInput v-model="l.debito" label="Débito" />
          <MoneyInput v-model="l.credito" label="Crédito" />
          <MoneyInput v-model="l.saldoFinal" label="Saldo Final" />
          <button type="button" class="btn btn-ghost btn-sm btn-danger-action linha-del" @click="delLinhaBalancete(i)">×</button>
        </div>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoBalancete" @click="balanceteDialogVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoBalancete" @click="salvarBalancete">
          <span v-if="salvandoBalancete" class="spinner"></span>
          <span v-else>Criar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Dialog: novo demonstrativo -->
    <AppDialog v-model="demoDialogVisivel" title="Novo demonstrativo" width="820px">
      <div class="dialog-form">
        <TextField v-model="novoDemo.periodo" label="Período" required placeholder="Ex.: 2026-01 ou 2026" maxlength="20" />
        <div class="linhas-header">
          <span class="field-label">Linhas do demonstrativo</span>
          <button type="button" class="btn btn-secondary btn-sm" @click="addLinhaDemo">+ Linha</button>
        </div>
        <div v-for="(l, i) in novoDemo.linhas" :key="i" class="linha-demo">
          <TextField v-model.number="l.ordem" type="number" label="Ordem" />
          <TextField v-model="l.codigoLinha" label="Código" />
          <TextField v-model="l.descricaoLinha" label="Descrição" />
          <MoneyInput v-model="l.valor" label="Valor" />
          <TextField v-model="l.tipoLinha" label="Tipo" />
          <button type="button" class="btn btn-ghost btn-sm btn-danger-action linha-del" @click="delLinhaDemo(i)">×</button>
        </div>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoDemo" @click="demoDialogVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoDemo" @click="salvarDemo">
          <span v-if="salvandoDemo" class="spinner"></span>
          <span v-else>Criar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Dialog: nova eliminação -->
    <AppDialog v-model="eliminacaoDialogVisivel" title="Nova eliminação intercompany" width="520px">
      <div class="dialog-form">
        <TextField v-model="novaEliminacao.periodo" label="Período" required placeholder="Ex.: 2026-01" maxlength="20" />
        <SelectField v-model="novaEliminacao.empresaOrigemId" label="Empresa de Origem" :options="opcoesEmpresa" />
        <SelectField v-model="novaEliminacao.empresaDestinoId" label="Empresa de Destino" :options="opcoesEmpresa" />
        <MoneyInput v-model="novaEliminacao.valor" label="Valor" />
        <TextField v-model="novaEliminacao.descricao" label="Descrição" maxlength="200" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoEliminacao" @click="eliminacaoDialogVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoEliminacao" @click="salvarEliminacao">
          <span v-if="salvandoEliminacao" class="spinner"></span>
          <span v-else>Registrar</span>
        </button>
      </template>
    </AppDialog>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-bottom: 16px; }
.grupo-situacao { display: flex; flex-direction: column; gap: 4px; margin-bottom: 16px; }
.resumo-label { font-size: 11px; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.4px; }
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
}
.form-tabs { display: flex; gap: 8px; border-bottom: 1px solid var(--border-color); padding-bottom: 12px; margin-bottom: 16px; }
.tab-link {
  background: none; border: none; color: var(--text-secondary);
  padding: 8px 16px; font-size: 13.5px; font-weight: 600; cursor: pointer;
  border-radius: 6px; transition: all 0.2s ease;
}
.tab-link:hover, .tab-link.active { background: rgba(255, 255, 255, 0.03); color: var(--text-primary); }
.tab-link.active { box-shadow: 0 0 10px rgba(99, 102, 241, 0.15); border: 1px solid rgba(99, 102, 241, 0.2); }
.tab-actions { display: flex; justify-content: flex-end; margin-bottom: 10px; }
.dialog-form { display: flex; flex-direction: column; gap: 14px; }
.linhas-header { display: flex; align-items: center; justify-content: space-between; margin-top: 4px; }
.linha-bal { display: grid; grid-template-columns: 1fr 1.5fr 1fr 1fr 1fr auto; gap: 8px; align-items: end; }
.linha-demo { display: grid; grid-template-columns: 0.7fr 1fr 1.5fr 1fr 1fr auto; gap: 8px; align-items: end; }
.linha-del { margin-bottom: 4px; }
.hint-novo { color: var(--text-secondary); font-size: 13px; padding: 8px 4px; }
</style>
