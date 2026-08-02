<script setup lang="ts">
/**
 * Orçamento de projeto — criação e detalhe com marcos e progresso.
 * POST /projetos/orcamento · GET /projetos/orcamento/{id}.
 */
import { ref, computed, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import { rotuloStatusWorkflow, fmtMoeda, fmtData, BILLING_TYPE_OPCOES, MARCO_STATUS_OPCOES } from '~/components/projetos-shared/statusWorkflow'

definePageMeta({ layout: 'default' })

interface Marco {
  id: string; titulo?: string | null; custo?: number | null; dataInicio?: string | null
  dataFim?: string | null; progresso?: number | null; status?: number | null
}
interface Orcamento {
  id: string; projetoId?: string | null; budget?: number | null; billingType?: number | null
  billingRate?: number | null; estimatedHours?: number | null; costsEstimate?: number | null
  status?: number | null; versao?: number | null; marcos?: Marco[]
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const item = ref<Orcamento | null>(null)

const form = reactive({
  projetoId: (route.query.projetoId as string) || '',
  budget: 0, billingType: 0 as number | null, billingRate: 0, estimatedHours: 0, costsEstimate: 0
})
const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.projetoId) erros.projetoId = 'Projeto é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const r = await useApi(`/projetos/orcamento/${idParam}`)
    item.value = extrairDados<Orcamento>(r) ?? null
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { carregando.value = false }
}
async function salvar() {
  if (!validar()) { toast.error('Formulário possui erros de validação.'); return }
  salvando.value = true
  try {
    await useApi('/projetos/orcamento', { method: 'POST', body: { ...form } })
    toast.success('Orçamento criado com sucesso!')
    router.push('/erp/projetos/orcamento')
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { salvando.value = false }
}
function voltar() { router.push('/erp/projetos/orcamento') }

const acaoSalvando = ref(false)
async function acaoSimples(acao: 'submeter' | 'aprovar') {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/orcamento/${idParam}/${acao}`, { method: 'POST' })
    toast.success('Ação executada.'); await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

/* marco */
const marcoDialog = ref(false)
const marcoForm = reactive({ titulo: '', custo: 0, dataInicio: null as string | null, dataFim: null as string | null, resumo: '' })
async function adicionarMarco() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/orcamento/${idParam}/marcos`, { method: 'POST', body: { ...marcoForm } })
    toast.success('Marco adicionado.'); marcoDialog.value = false; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

/* progresso marco */
const progDialog = ref(false)
const progForm = reactive({ marcoId: '', progresso: 0, status: 0 as number | null })
function abrirProgresso(m: Marco) {
  progForm.marcoId = m.id; progForm.progresso = Number(m.progresso ?? 0); progForm.status = m.status ?? 0
  progDialog.value = true
}
async function salvarProgresso() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/orcamento/${idParam}/marcos/${progForm.marcoId}/progresso`, {
      method: 'POST', body: { progresso: progForm.progresso, status: progForm.status }
    })
    toast.success('Progresso atualizado.'); progDialog.value = false; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

function rotuloBilling(v: unknown): string {
  const f = BILLING_TYPE_OPCOES.find((o) => String(o.value) === String(v))
  return f ? f.label : '—'
}
onMounted(carregar)
</script>

<template>
  <div>
    <template v-if="!isEdit">
      <PageToolbar title="Novo orçamento">
        <template #actions>
          <button type="button" class="btn btn-secondary" @click="voltar">Cancelar</button>
          <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
            <span v-if="salvando" class="spinner"></span><span v-else>Salvar</span>
          </button>
        </template>
      </PageToolbar>
      <div class="glass-panel form-panel">
        <div class="form-grid">
          <TextField v-model="form.projetoId" label="Projeto (ID)" required :error="erros.projetoId" hint="UUID do projeto" />
          <MoneyInput v-model="form.budget" label="Orçamento (budget)" />
          <SelectField v-model="form.billingType" label="Tipo de cobrança" :options="BILLING_TYPE_OPCOES" :clearable="false" />
          <MoneyInput v-model="form.billingRate" label="Taxa (billing rate)" />
          <QuantityInput v-model="form.estimatedHours" label="Horas estimadas" :decimais="2" />
          <MoneyInput v-model="form.costsEstimate" label="Custos estimados" />
        </div>
      </div>
    </template>

    <template v-else>
      <PageToolbar title="Orçamento" :subtitle="rotuloStatusWorkflow(item?.status)" :loading="carregando">
        <template #actions>
          <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
        </template>
      </PageToolbar>

      <div v-if="item" class="glass-panel form-panel">
        <div class="detail-grid">
          <div><span class="dl">Projeto (ID)</span><span class="dv">{{ item.projetoId || '—' }}</span></div>
          <div><span class="dl">Orçamento</span><span class="dv">{{ fmtMoeda(item.budget) }}</span></div>
          <div><span class="dl">Cobrança</span><span class="dv">{{ rotuloBilling(item.billingType) }}</span></div>
          <div><span class="dl">Taxa</span><span class="dv">{{ item.billingRate == null ? '—' : fmtMoeda(item.billingRate) }}</span></div>
          <div><span class="dl">Horas estimadas</span><span class="dv">{{ item.estimatedHours == null ? '—' : Number(item.estimatedHours).toLocaleString('pt-BR') }}</span></div>
          <div><span class="dl">Custos estimados</span><span class="dv">{{ item.costsEstimate == null ? '—' : fmtMoeda(item.costsEstimate) }}</span></div>
        </div>
      </div>

      <div class="glass-panel form-panel mt-2">
        <div class="section-head"><h3>Ações</h3></div>
        <div class="btn-row">
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoSimples('submeter')">Submeter</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoSimples('aprovar')">Aprovar</button>
        </div>
      </div>

      <div class="glass-panel form-panel mt-2">
        <div class="section-head">
          <h3>Marcos orçamentários</h3>
          <button type="button" class="btn btn-primary btn-sm" @click="marcoDialog = true">+ Marco</button>
        </div>
        <table class="admin-table">
          <thead><tr><th>Título</th><th>Início</th><th>Fim</th><th class="td-right">Custo</th><th class="td-right">Progresso</th><th class="td-actions">Ações</th></tr></thead>
          <tbody>
            <tr v-if="!item?.marcos?.length"><td colspan="6"><div class="table-empty">Nenhum marco.</div></td></tr>
            <tr v-for="m in item?.marcos ?? []" :key="m.id">
              <td>{{ m.titulo }}</td>
              <td>{{ fmtData(m.dataInicio) }}</td>
              <td>{{ fmtData(m.dataFim) }}</td>
              <td class="td-right">{{ fmtMoeda(m.custo) }}</td>
              <td class="td-right">{{ Number(m.progresso ?? 0) }}%</td>
              <td class="td-actions"><button type="button" class="btn btn-ghost btn-sm" @click="abrirProgresso(m)">Progresso</button></td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <AppDialog v-model="marcoDialog" title="Adicionar marco" width="560px">
      <div class="form-grid">
        <TextField v-model="marcoForm.titulo" label="Título" required />
        <MoneyInput v-model="marcoForm.custo" label="Custo" />
        <DateTimeField v-model="marcoForm.dataInicio" label="Início" mode="datetime" required />
        <DateTimeField v-model="marcoForm.dataFim" label="Fim" mode="datetime" required />
        <TextField v-model="marcoForm.resumo" label="Resumo" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="marcoDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="adicionarMarco">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Adicionar</span>
        </button>
      </template>
    </AppDialog>

    <AppDialog v-model="progDialog" title="Atualizar progresso do marco" width="480px">
      <div class="form-grid">
        <QuantityInput v-model="progForm.progresso" label="Progresso (%)" :decimais="0" :min="0" />
        <SelectField v-model="progForm.status" label="Status" :options="MARCO_STATUS_OPCOES" :clearable="false" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="progDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="salvarProgresso">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Salvar</span>
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
