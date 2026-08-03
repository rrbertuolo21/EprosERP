<script setup lang="ts">
/**
 * Projeto — criação (novo) e detalhe (existente).
 *
 * Contrato: POST /projetos (criar) · GET /projetos/{id} (detalhe).
 * Não há PUT no agregado raiz — por isso o registro existente abre em modo detalhe
 * (somente leitura) com ações via diálogos: WBS, progresso de WBS, alocação de recurso
 * e as ações de Definição (membros, clientes, atividades, arquivos, duplicar).
 */
import { ref, computed, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'

definePageMeta({ layout: 'default' })

interface WbsItem {
  id: string
  nome?: string | null
  descricao?: string | null
  dataInicio?: string | null
  dataTermino?: string | null
  pesoPonderado?: number | null
  percentualConclusao?: number | null
}
interface Alocacao {
  id: string
  colaboradorId?: string | null
  funcao?: string | null
  custoHora?: number | null
  horasPlanejadas?: number | null
}
interface Projeto {
  id: string
  nome?: string | null
  descricao?: string | null
  clienteId?: string | null
  dataInicio?: string | null
  dataTermino?: string | null
  orcamentoTotal?: number | null
  custoAcumulado?: number | null
  percentualConclusao?: number | null
  status?: string | null
  itensWbs?: WbsItem[]
  alocacoes?: Alocacao[]
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const projeto = ref<Projeto | null>(null)

const form = reactive({
  nome: '' as string,
  descricao: '' as string,
  clienteId: '' as string,
  dataInicio: null as string | null,
  dataTermino: null as string | null,
  orcamentoTotal: 0 as number
})

const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.nome) erros.nome = 'Nome é obrigatório.'
  if (!form.clienteId) erros.clienteId = 'Cliente é obrigatório.'
  if (!form.dataInicio) erros.dataInicio = 'Data de início é obrigatória.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/projetos/${idParam}`)
    projeto.value = extrairDados<Projeto>(resposta) ?? null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/projetos', { method: 'POST', body: { ...form } })
    toast.success('Projeto criado com sucesso!')
    router.push('/erp/projetos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/projetos')
}

/* ---------- Diálogo: adicionar item WBS ---------- */
const wbsDialog = ref(false)
const wbsSalvando = ref(false)
const wbsForm = reactive({ nome: '', descricao: '', dataInicio: null as string | null, dataTermino: null as string | null, pesoPonderado: 0 })
function abrirWbs() {
  wbsForm.nome = ''; wbsForm.descricao = ''; wbsForm.dataInicio = null; wbsForm.dataTermino = null; wbsForm.pesoPonderado = 0
  wbsDialog.value = true
}
async function salvarWbs() {
  wbsSalvando.value = true
  try {
    await useApi(`/projetos/${idParam}/wbs`, { method: 'POST', body: { ...wbsForm } })
    toast.success('Item WBS adicionado.')
    wbsDialog.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    wbsSalvando.value = false
  }
}

/* ---------- Diálogo: progresso de item WBS ---------- */
const progDialog = ref(false)
const progSalvando = ref(false)
const progForm = reactive({ wbsItemId: '', percentualConclusao: 0 })
function abrirProgresso(item: WbsItem) {
  progForm.wbsItemId = item.id
  progForm.percentualConclusao = Number(item.percentualConclusao ?? 0)
  progDialog.value = true
}
async function salvarProgresso() {
  progSalvando.value = true
  try {
    await useApi(`/projetos/${idParam}/wbs/${progForm.wbsItemId}/progresso`, {
      method: 'POST',
      body: { percentualConclusao: progForm.percentualConclusao }
    })
    toast.success('Progresso atualizado.')
    progDialog.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    progSalvando.value = false
  }
}

/* ---------- Diálogo: alocar recurso ---------- */
const alocDialog = ref(false)
const alocSalvando = ref(false)
const alocForm = reactive({ colaboradorId: '', funcao: '', custoHora: 0, horasPlanejadas: 0 })
function abrirAloc() {
  alocForm.colaboradorId = ''; alocForm.funcao = ''; alocForm.custoHora = 0; alocForm.horasPlanejadas = 0
  alocDialog.value = true
}
async function salvarAloc() {
  alocSalvando.value = true
  try {
    await useApi(`/projetos/${idParam}/alocacoes`, { method: 'POST', body: { ...alocForm } })
    toast.success('Recurso alocado.')
    alocDialog.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    alocSalvando.value = false
  }
}

/* ---------- Definição: membros / clientes / atividades / arquivos / duplicar ---------- */
const defDialog = ref<'membro' | 'cliente' | 'atividade' | 'arquivo' | null>(null)
const defSalvando = ref(false)
const membroForm = reactive({ usuarioId: '', papel: '' })
const clienteForm = reactive({ clienteId: '' })
const atividadeForm = reactive({ usuarioId: '', tipoUsuario: '', tipoAtividade: '', observacao: '' })
const arquivoForm = reactive({ nomeArquivo: '', caminhoArquivo: '', arquivoId: '' })

async function salvarDefinicao() {
  defSalvando.value = true
  try {
    if (defDialog.value === 'membro') {
      await useApi(`/projetos/definicao/${idParam}/membros`, { method: 'POST', body: { ...membroForm } })
    } else if (defDialog.value === 'cliente') {
      await useApi(`/projetos/definicao/${idParam}/clientes`, { method: 'POST', body: { ...clienteForm } })
    } else if (defDialog.value === 'atividade') {
      await useApi(`/projetos/definicao/${idParam}/atividades`, { method: 'POST', body: { ...atividadeForm } })
    } else if (defDialog.value === 'arquivo') {
      await useApi(`/projetos/definicao/${idParam}/arquivos`, {
        method: 'POST',
        body: { nomeArquivo: arquivoForm.nomeArquivo, caminhoArquivo: arquivoForm.caminhoArquivo, arquivoId: arquivoForm.arquivoId || null }
      })
    }
    toast.success('Registro da definição salvo.')
    defDialog.value = null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    defSalvando.value = false
  }
}

async function duplicarProjeto() {
  try {
    await useApi(`/projetos/definicao/${idParam}/duplicar`, { method: 'POST' })
    toast.success('Projeto duplicado a partir da definição.')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

function fmtData(v: unknown): string {
  if (!v) return '—'
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? '—' : d.toLocaleDateString('pt-BR')
}
function fmtMoeda(v: unknown): string {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(Number(v ?? 0))
}

onMounted(carregar)
</script>

<template>
  <div>
    <!-- ===================== NOVO ===================== -->
    <template v-if="!isEdit">
      <PageToolbar title="Novo projeto">
        <template #actions>
          <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
          <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
            <span v-if="salvando" class="spinner"></span>
            <span v-else>Salvar</span>
          </button>
        </template>
      </PageToolbar>

      <div class="glass-panel form-panel">
        <form class="vertical-form" @submit.prevent="salvar">
          <div class="form-grid">
            <TextField v-model="form.nome" label="Nome" required :error="erros.nome" maxlength="150" />
            <TextField
              v-model="form.clienteId"
              label="Cliente (ID)"
              required
              :error="erros.clienteId"
              hint="Identificador (UUID) do cliente"
              placeholder="00000000-0000-0000-0000-000000000000"
            />
            <!-- TODO: substituir por SelectField quando houver endpoint de clientes que exponha UUID -->
            <DateTimeField v-model="form.dataInicio" label="Data de início" mode="datetime" required :error="erros.dataInicio" />
            <DateTimeField v-model="form.dataTermino" label="Data de término" mode="datetime" />
            <MoneyInput v-model="form.orcamentoTotal" label="Orçamento total" />
          </div>
          <div class="form-grid mt-2">
            <TextField v-model="form.descricao" label="Descrição" maxlength="500" />
          </div>
        </form>
      </div>
    </template>

    <!-- ===================== DETALHE ===================== -->
    <template v-else>
      <PageToolbar :title="projeto?.nome || 'Projeto'" :subtitle="projeto?.status || ''" :loading="carregando">
        <template #actions>
          <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
          <button type="button" class="btn btn-ghost" @click="duplicarProjeto">Duplicar</button>
        </template>
      </PageToolbar>

      <div v-if="projeto" class="glass-panel form-panel">
        <div class="detail-grid">
          <div><span class="dl">Cliente (ID)</span><span class="dv">{{ projeto.clienteId || '—' }}</span></div>
          <div><span class="dl">Início</span><span class="dv">{{ fmtData(projeto.dataInicio) }}</span></div>
          <div><span class="dl">Término</span><span class="dv">{{ fmtData(projeto.dataTermino) }}</span></div>
          <div><span class="dl">Orçamento</span><span class="dv">{{ fmtMoeda(projeto.orcamentoTotal) }}</span></div>
          <div><span class="dl">Custo acumulado</span><span class="dv">{{ fmtMoeda(projeto.custoAcumulado) }}</span></div>
          <div><span class="dl">Conclusão</span><span class="dv">{{ Number(projeto.percentualConclusao ?? 0).toFixed(0) }}%</span></div>
        </div>
        <p v-if="projeto.descricao" class="descricao">{{ projeto.descricao }}</p>
      </div>

      <!-- WBS -->
      <div class="glass-panel form-panel mt-2">
        <div class="section-head">
          <h3>Estrutura analítica (WBS)</h3>
          <button type="button" class="btn btn-primary btn-sm" @click="abrirWbs">+ Item WBS</button>
        </div>
        <table class="admin-table">
          <thead><tr><th>Nome</th><th>Início</th><th>Término</th><th class="td-right">Peso</th><th class="td-right">Conclusão</th><th class="td-actions">Ações</th></tr></thead>
          <tbody>
            <tr v-if="!projeto?.itensWbs?.length"><td colspan="6"><div class="table-empty">Nenhum item de WBS.</div></td></tr>
            <tr v-for="w in projeto?.itensWbs ?? []" :key="w.id">
              <td>{{ w.nome }}</td>
              <td>{{ fmtData(w.dataInicio) }}</td>
              <td>{{ fmtData(w.dataTermino) }}</td>
              <td class="td-right">{{ Number(w.pesoPonderado ?? 0).toLocaleString('pt-BR') }}</td>
              <td class="td-right">{{ Number(w.percentualConclusao ?? 0).toFixed(0) }}%</td>
              <td class="td-actions"><button type="button" class="btn btn-ghost btn-sm" @click="abrirProgresso(w)">Progresso</button></td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Alocações -->
      <div class="glass-panel form-panel mt-2">
        <div class="section-head">
          <h3>Alocações de recurso</h3>
          <button type="button" class="btn btn-primary btn-sm" @click="abrirAloc">+ Alocar recurso</button>
        </div>
        <table class="admin-table">
          <thead><tr><th>Colaborador (ID)</th><th>Função</th><th class="td-right">Custo/hora</th><th class="td-right">Horas planejadas</th></tr></thead>
          <tbody>
            <tr v-if="!projeto?.alocacoes?.length"><td colspan="4"><div class="table-empty">Nenhuma alocação.</div></td></tr>
            <tr v-for="a in projeto?.alocacoes ?? []" :key="a.id">
              <td>{{ a.colaboradorId }}</td>
              <td>{{ a.funcao || '—' }}</td>
              <td class="td-right">{{ fmtMoeda(a.custoHora) }}</td>
              <td class="td-right">{{ Number(a.horasPlanejadas ?? 0).toLocaleString('pt-BR') }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Definição -->
      <div class="glass-panel form-panel mt-2">
        <div class="section-head"><h3>Definição do projeto</h3></div>
        <div class="btn-row">
          <button type="button" class="btn btn-ghost btn-sm" @click="defDialog = 'membro'">+ Membro</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="defDialog = 'cliente'">+ Cliente</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="defDialog = 'atividade'">+ Atividade</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="defDialog = 'arquivo'">+ Arquivo</button>
        </div>
      </div>
    </template>

    <!-- Diálogo WBS -->
    <AppDialog v-model="wbsDialog" title="Adicionar item WBS" width="560px">
      <div class="form-grid">
        <TextField v-model="wbsForm.nome" label="Nome" required />
        <TextField v-model="wbsForm.descricao" label="Descrição" />
        <DateTimeField v-model="wbsForm.dataInicio" label="Início" mode="datetime" required />
        <DateTimeField v-model="wbsForm.dataTermino" label="Término" mode="datetime" required />
        <QuantityInput v-model="wbsForm.pesoPonderado" label="Peso ponderado" :decimais="2" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="wbsDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="wbsSalvando" @click="salvarWbs">
          <span v-if="wbsSalvando" class="spinner"></span><span v-else>Adicionar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Diálogo progresso WBS -->
    <AppDialog v-model="progDialog" title="Atualizar progresso" width="420px">
      <PercentInput v-model="progForm.percentualConclusao" label="Percentual de conclusão" />
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="progDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="progSalvando" @click="salvarProgresso">
          <span v-if="progSalvando" class="spinner"></span><span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Diálogo alocação -->
    <AppDialog v-model="alocDialog" title="Alocar recurso" width="560px">
      <div class="form-grid">
        <TextField v-model="alocForm.colaboradorId" label="Colaborador (ID)" required hint="UUID do colaborador" />
        <TextField v-model="alocForm.funcao" label="Função" />
        <MoneyInput v-model="alocForm.custoHora" label="Custo/hora" />
        <QuantityInput v-model="alocForm.horasPlanejadas" label="Horas planejadas" :decimais="2" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="alocDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="alocSalvando" @click="salvarAloc">
          <span v-if="alocSalvando" class="spinner"></span><span v-else>Alocar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Diálogo definição -->
    <AppDialog :model-value="defDialog !== null" title="Definição do projeto" width="560px" @update:model-value="defDialog = null">
      <div v-if="defDialog === 'membro'" class="form-grid">
        <TextField v-model="membroForm.usuarioId" label="Usuário (ID)" required hint="UUID do usuário" />
        <TextField v-model="membroForm.papel" label="Papel" />
      </div>
      <div v-else-if="defDialog === 'cliente'" class="form-grid">
        <TextField v-model="clienteForm.clienteId" label="Cliente (ID)" required hint="UUID do cliente" />
      </div>
      <div v-else-if="defDialog === 'atividade'" class="form-grid">
        <TextField v-model="atividadeForm.usuarioId" label="Usuário (ID)" hint="UUID do usuário" />
        <TextField v-model="atividadeForm.tipoUsuario" label="Tipo de usuário" />
        <TextField v-model="atividadeForm.tipoAtividade" label="Tipo de atividade" />
        <TextField v-model="atividadeForm.observacao" label="Observação" />
      </div>
      <div v-else-if="defDialog === 'arquivo'" class="form-grid">
        <TextField v-model="arquivoForm.nomeArquivo" label="Nome do arquivo" />
        <TextField v-model="arquivoForm.caminhoArquivo" label="Caminho do arquivo" />
        <TextField v-model="arquivoForm.arquivoId" label="Arquivo (ID)" hint="UUID do arquivo (opcional)" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="defDialog = null">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="defSalvando" @click="salvarDefinicao">
          <span v-if="defSalvando" class="spinner"></span><span v-else>Salvar</span>
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
.descricao { margin-top: 16px; color: var(--text-secondary); font-size: 14px; }
.section-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
.section-head h3 { font-size: 15px; }
.btn-row { display: flex; gap: 8px; flex-wrap: wrap; }
.mt-2 { margin-top: 16px; }
</style>
