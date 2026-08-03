<script setup lang="ts">
/**
 * Portfólio — criação (novo) e detalhe (existente) com ações de workflow.
 * POST /projetos/portfolio · GET /projetos/portfolio/{id}.
 * Sem PUT — registro existente abre em modo detalhe; alterações via ações (diálogos).
 */
import { ref, computed, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import { rotuloStatusWorkflow, fmtMoeda } from '~/components/projetos-shared/statusWorkflow'

definePageMeta({ layout: 'default' })

interface PortfolioItem {
  id: string; sequencia?: number; tipoItem?: string | null; titulo?: string | null
  valorEstimado?: number | null; score?: number | null
}
interface Portfolio {
  id: string; codigo?: string | null; descricao?: string | null; status?: number | null
  responsavelId?: string | null; tipoPortfolio?: string | null; justificativa?: string | null
  scoreTotal?: number | null; versao?: number | null; itens?: PortfolioItem[]
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const item = ref<Portfolio | null>(null)

const form = reactive({ codigo: '', descricao: '', responsavelId: '', tipoPortfolio: '', justificativa: '' })
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.responsavelId) erros.responsavelId = 'Responsável é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const r = await useApi(`/projetos/portfolio/${idParam}`)
    item.value = extrairDados<Portfolio>(r) ?? null
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { carregando.value = false }
}

async function salvar() {
  if (!validar()) { toast.error('Formulário possui erros de validação.'); return }
  salvando.value = true
  try {
    await useApi('/projetos/portfolio', { method: 'POST', body: { ...form } })
    toast.success('Portfólio criado com sucesso!')
    router.push('/erp/projetos/portfolio')
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { salvando.value = false }
}
function voltar() { router.push('/erp/projetos/portfolio') }

/* --- ações de workflow que pedem apenas usuarioId --- */
const acaoUsuario = ref<null | 'submeter' | 'aprovar' | 'suspender' | 'retomar' | 'reativar' | 'inativar' | 'encerrar'>(null)
const usuarioId = ref('')
const acaoSalvando = ref(false)
const rotuloAcao: Record<string, string> = {
  submeter: 'Submeter', aprovar: 'Aprovar', suspender: 'Suspender', retomar: 'Retomar',
  reativar: 'Reativar', inativar: 'Inativar', encerrar: 'Encerrar'
}
async function confirmarAcaoUsuario() {
  if (!acaoUsuario.value) return
  acaoSalvando.value = true
  try {
    const body = acaoUsuario.value === 'submeter' ? { usuarioId: usuarioId.value } : { usuarioId: usuarioId.value }
    await useApi(`/projetos/portfolio/${idParam}/${acaoUsuario.value}`, { method: 'POST', body })
    toast.success('Ação executada com sucesso.')
    acaoUsuario.value = null; usuarioId.value = ''
    await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

/* --- rejeitar --- */
const rejDialog = ref(false)
const rejForm = reactive({ motivo: '', usuarioId: '' })
async function rejeitar() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/portfolio/${idParam}/rejeitar`, { method: 'POST', body: { ...rejForm } })
    toast.success('Portfólio rejeitado.'); rejDialog.value = false; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

/* --- priorizar --- */
const prioDialog = ref(false)
const prioForm = reactive({ scoreTotal: 0, justificativa: '', usuarioId: '' })
async function priorizar() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/portfolio/${idParam}/priorizar`, { method: 'POST', body: { ...prioForm } })
    toast.success('Priorização registrada.'); prioDialog.value = false; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

/* --- adicionar item --- */
const itemDialog = ref(false)
const itemForm = reactive({
  sequencia: 1, tipoItem: '', projetoId: '', programaId: '', titulo: '',
  valorEstimado: 0, esforcoEstimado: 0, capacidadeRequerida: 0, npv: 0, payback: 0,
  alinhamentoEstrategico: 0, risco: 0, score: 0, justificativaPrioridade: '', observacao: ''
})
async function adicionarItem() {
  acaoSalvando.value = true
  try {
    const body = {
      sequencia: itemForm.sequencia, tipoItem: itemForm.tipoItem || null,
      projetoId: itemForm.projetoId || null, programaId: itemForm.programaId || null,
      titulo: itemForm.titulo || null, valorEstimado: itemForm.valorEstimado,
      esforcoEstimado: itemForm.esforcoEstimado, capacidadeRequerida: itemForm.capacidadeRequerida,
      npv: itemForm.npv, payback: itemForm.payback, alinhamentoEstrategico: itemForm.alinhamentoEstrategico,
      risco: itemForm.risco, score: itemForm.score, justificativaPrioridade: itemForm.justificativaPrioridade || null,
      observacao: itemForm.observacao || null
    }
    await useApi(`/projetos/portfolio/${idParam}/itens`, { method: 'POST', body })
    toast.success('Item adicionado.'); itemDialog.value = false; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

/* --- anexo --- */
const anexoDialog = ref(false)
const anexoForm = reactive({ itemId: '', arquivoId: '', tipoAnexo: '' })
async function adicionarAnexo() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/portfolio/${idParam}/anexos`, {
      method: 'POST',
      body: { itemId: anexoForm.itemId || null, arquivoId: anexoForm.arquivoId, tipoAnexo: anexoForm.tipoAnexo || null }
    })
    toast.success('Anexo adicionado.'); anexoDialog.value = false
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

onMounted(carregar)
</script>

<template>
  <div>
    <!-- NOVO -->
    <template v-if="!isEdit">
      <PageToolbar title="Novo portfólio">
        <template #actions>
          <button type="button" class="btn btn-secondary" @click="voltar">Cancelar</button>
          <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
            <span v-if="salvando" class="spinner"></span><span v-else>Salvar</span>
          </button>
        </template>
      </PageToolbar>
      <div class="glass-panel form-panel">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" maxlength="40" />
          <TextField v-model="form.tipoPortfolio" label="Tipo de portfólio" />
          <TextField v-model="form.responsavelId" label="Responsável (ID)" required :error="erros.responsavelId" hint="UUID do responsável" />
          <TextField v-model="form.descricao" label="Descrição" maxlength="500" />
          <TextField v-model="form.justificativa" label="Justificativa" maxlength="500" />
        </div>
      </div>
    </template>

    <!-- DETALHE -->
    <template v-else>
      <PageToolbar :title="item?.codigo || 'Portfólio'" :subtitle="rotuloStatusWorkflow(item?.status)" :loading="carregando">
        <template #actions>
          <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
        </template>
      </PageToolbar>

      <div v-if="item" class="glass-panel form-panel">
        <div class="detail-grid">
          <div><span class="dl">Descrição</span><span class="dv">{{ item.descricao || '—' }}</span></div>
          <div><span class="dl">Tipo</span><span class="dv">{{ item.tipoPortfolio || '—' }}</span></div>
          <div><span class="dl">Responsável (ID)</span><span class="dv">{{ item.responsavelId || '—' }}</span></div>
          <div><span class="dl">Score total</span><span class="dv">{{ item.scoreTotal == null ? '—' : Number(item.scoreTotal).toLocaleString('pt-BR') }}</span></div>
          <div><span class="dl">Versão</span><span class="dv">{{ item.versao ?? '—' }}</span></div>
        </div>
      </div>

      <div class="glass-panel form-panel mt-2">
        <div class="section-head"><h3>Ações</h3></div>
        <div class="btn-row">
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoUsuario = 'submeter'">Submeter</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoUsuario = 'aprovar'">Aprovar</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="rejDialog = true">Rejeitar</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="prioDialog = true">Priorizar</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoUsuario = 'suspender'">Suspender</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoUsuario = 'retomar'">Retomar</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoUsuario = 'reativar'">Reativar</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoUsuario = 'inativar'">Inativar</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoUsuario = 'encerrar'">Encerrar</button>
        </div>
      </div>

      <div class="glass-panel form-panel mt-2">
        <div class="section-head">
          <h3>Itens do portfólio</h3>
          <div class="btn-row">
            <button type="button" class="btn btn-ghost btn-sm" @click="anexoDialog = true">+ Anexo</button>
            <button type="button" class="btn btn-primary btn-sm" @click="itemDialog = true">+ Item</button>
          </div>
        </div>
        <table class="admin-table">
          <thead><tr><th>Seq.</th><th>Título</th><th>Tipo</th><th class="td-right">Valor estimado</th><th class="td-right">Score</th></tr></thead>
          <tbody>
            <tr v-if="!item?.itens?.length"><td colspan="5"><div class="table-empty">Nenhum item.</div></td></tr>
            <tr v-for="it in item?.itens ?? []" :key="it.id">
              <td>{{ it.sequencia }}</td>
              <td>{{ it.titulo || '—' }}</td>
              <td>{{ it.tipoItem || '—' }}</td>
              <td class="td-right">{{ fmtMoeda(it.valorEstimado) }}</td>
              <td class="td-right">{{ it.score == null ? '—' : Number(it.score).toLocaleString('pt-BR') }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <!-- Ação simples (usuarioId) -->
    <AppDialog :model-value="acaoUsuario !== null" :title="acaoUsuario ? rotuloAcao[acaoUsuario] : ''" width="420px" @update:model-value="acaoUsuario = null">
      <TextField v-model="usuarioId" label="Usuário (ID)" required hint="UUID do usuário que executa a ação" />
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="acaoUsuario = null">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="confirmarAcaoUsuario">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Confirmar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Rejeitar -->
    <AppDialog v-model="rejDialog" title="Rejeitar portfólio" width="480px">
      <div class="form-grid">
        <TextField v-model="rejForm.usuarioId" label="Usuário (ID)" required hint="UUID" />
        <TextField v-model="rejForm.motivo" label="Motivo" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="rejDialog = false">Cancelar</button>
        <button type="button" class="btn btn-danger" :disabled="acaoSalvando" @click="rejeitar">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Rejeitar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Priorizar -->
    <AppDialog v-model="prioDialog" title="Priorizar portfólio" width="480px">
      <div class="form-grid">
        <QuantityInput v-model="prioForm.scoreTotal" label="Score total" :decimais="2" />
        <TextField v-model="prioForm.usuarioId" label="Usuário (ID)" required hint="UUID" />
        <TextField v-model="prioForm.justificativa" label="Justificativa" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="prioDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="priorizar">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Priorizar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Item -->
    <AppDialog v-model="itemDialog" title="Adicionar item de portfólio" width="720px">
      <div class="form-grid">
        <QuantityInput v-model="itemForm.sequencia" label="Sequência" :decimais="0" />
        <TextField v-model="itemForm.tipoItem" label="Tipo de item" />
        <TextField v-model="itemForm.titulo" label="Título" />
        <TextField v-model="itemForm.projetoId" label="Projeto (ID)" hint="UUID (opcional)" />
        <TextField v-model="itemForm.programaId" label="Programa (ID)" hint="UUID (opcional)" />
        <MoneyInput v-model="itemForm.valorEstimado" label="Valor estimado" />
        <QuantityInput v-model="itemForm.esforcoEstimado" label="Esforço estimado" :decimais="2" />
        <QuantityInput v-model="itemForm.capacidadeRequerida" label="Capacidade requerida" :decimais="2" />
        <QuantityInput v-model="itemForm.npv" label="NPV" :decimais="2" />
        <QuantityInput v-model="itemForm.payback" label="Payback" :decimais="2" />
        <QuantityInput v-model="itemForm.alinhamentoEstrategico" label="Alinhamento estratégico" :decimais="2" />
        <QuantityInput v-model="itemForm.risco" label="Risco" :decimais="2" />
        <QuantityInput v-model="itemForm.score" label="Score" :decimais="2" />
        <TextField v-model="itemForm.justificativaPrioridade" label="Justificativa da prioridade" />
        <TextField v-model="itemForm.observacao" label="Observação" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="itemDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="adicionarItem">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Adicionar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Anexo -->
    <AppDialog v-model="anexoDialog" title="Adicionar anexo" width="480px">
      <div class="form-grid">
        <TextField v-model="anexoForm.arquivoId" label="Arquivo (ID)" required hint="UUID" />
        <TextField v-model="anexoForm.itemId" label="Item (ID)" hint="UUID (opcional)" />
        <TextField v-model="anexoForm.tipoAnexo" label="Tipo de anexo" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="anexoDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="adicionarAnexo">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Adicionar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.detail-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; }
.detail-grid .dl { display: block; font-size: 12px; color: var(--text-secondary); }
.detail-grid .dv { display: block; font-size: 14px; color: var(--text-primary); font-weight: 600; }
.section-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
.section-head h3 { font-size: 15px; }
.btn-row { display: flex; gap: 8px; flex-wrap: wrap; }
.mt-2 { margin-top: 16px; }
</style>
