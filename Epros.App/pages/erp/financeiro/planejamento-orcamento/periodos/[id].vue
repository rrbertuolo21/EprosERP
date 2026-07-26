<script setup lang="ts">
/**
 * Período Orçamentário — novo (POST) e detalhe com budgets.
 * POST /planejamento-orcamento/periodos · GET/POST /{id}/budgets ·
 * POST /budgets/{budgetId}/aprovar · POST /budgets/{budgetId}/alocacoes.
 * Lacuna: POST /linhas/{linhaId}/realizado (linha de budget) não tem tela — requer o id da
 * linha, não exposto pelos GET do digest.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'

definePageMeta({ layout: 'default' })

interface Budget {
  id: string
  tipo?: string | null
  valor?: number | null
  statusDescricao?: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { formatarMoeda } = useHelper()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const form = reactive<{ dataInicio: string | null; dataFim: string | null }>({ dataInicio: null, dataFim: null })
const erros = reactive<Record<string, string>>({})

const budgets = ref<Budget[]>([])
const novoBudget = reactive<{ tipo: string | null; valor: number | null }>({ tipo: null, valor: null })
const salvandoBudget = ref(false)

// alocacao
const alocarVisivel = ref(false)
const budgetAlvo = ref<Budget | null>(null)
const alocacao = reactive<{ contaId: string | null; valorAlocado: number | null; autoAprovar: boolean }>({ contaId: null, valorAlocado: null, autoAprovar: false })
const salvandoAlocacao = ref(false)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.dataInicio) erros.dataInicio = 'Data de início é obrigatória.'
  if (!form.dataFim) erros.dataFim = 'Data de fim é obrigatória.'
  return Object.keys(erros).length === 0
}

async function carregarBudgets() {
  if (!isEdit.value) return
  try {
    const resposta = await useApi('/planejamento-orcamento/periodos/{id}/budgets', { params: { id: idParam } })
    budgets.value = extrairDados<Budget[]>(resposta) ?? []
  } catch (e) {
    console.error('[periodos/[id]] budgets', e)
  }
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/planejamento-orcamento/periodos', { method: 'POST', body: { dataInicio: form.dataInicio, dataFim: form.dataFim } })
    toast.success('Período criado com sucesso!')
    router.push('/erp/financeiro/planejamento-orcamento/periodos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function adicionarBudget() {
  if (novoBudget.valor == null) {
    toast.error('Informe o valor do budget.')
    return
  }
  salvandoBudget.value = true
  try {
    await useApi('/planejamento-orcamento/periodos/{id}/budgets', {
      method: 'POST',
      params: { id: idParam },
      body: { periodoId: idParam, tipo: novoBudget.tipo, valor: novoBudget.valor }
    })
    toast.success('Budget adicionado.')
    novoBudget.tipo = null
    novoBudget.valor = null
    await carregarBudgets()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoBudget.value = false
  }
}

async function aprovarBudget(b: Budget) {
  try {
    await useApi('/planejamento-orcamento/budgets/{budgetId}/aprovar', { method: 'POST', params: { budgetId: b.id } })
    toast.success('Budget aprovado.')
    await carregarBudgets()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

function abrirAlocar(b: Budget) {
  budgetAlvo.value = b
  alocacao.contaId = null
  alocacao.valorAlocado = null
  alocacao.autoAprovar = false
  alocarVisivel.value = true
}

async function confirmarAlocacao() {
  if (!budgetAlvo.value || !alocacao.contaId) {
    toast.error('Informe a conta da alocação.')
    return
  }
  salvandoAlocacao.value = true
  try {
    await useApi('/planejamento-orcamento/budgets/{budgetId}/alocacoes', {
      method: 'POST',
      params: { budgetId: budgetAlvo.value.id },
      body: { budgetId: budgetAlvo.value.id, contaId: alocacao.contaId, valorAlocado: alocacao.valorAlocado, autoAprovar: alocacao.autoAprovar }
    })
    toast.success('Alocação registrada.')
    alocarVisivel.value = false
    await carregarBudgets()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoAlocacao.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/planejamento-orcamento/periodos')
}

onMounted(carregarBudgets)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Período orçamentário' : 'Novo período'" :loading="carregando || salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <button v-if="!isEdit" type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div v-if="!isEdit" class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <DateTimeField v-model="form.dataInicio" label="Data de início" mode="datetime" required :error="erros.dataInicio" />
          <DateTimeField v-model="form.dataFim" label="Data de fim" mode="datetime" required :error="erros.dataFim" />
        </div>
      </form>
    </div>

    <div v-if="isEdit" class="glass-panel form-panel">
      <h3 class="secao-titulo">Budgets do período</h3>
      <div class="form-grid nova-linha">
        <TextField v-model="novoBudget.tipo" label="Tipo" maxlength="40" />
        <MoneyInput v-model="novoBudget.valor" label="Valor" />
        <div class="acao-linha">
          <button type="button" class="btn btn-secondary" :disabled="salvandoBudget" @click="adicionarBudget">+ Adicionar budget</button>
        </div>
      </div>
      <table class="admin-table mt">
        <thead><tr><th>Tipo</th><th class="td-right">Valor</th><th class="td-center">Status</th><th class="td-actions">Ações</th></tr></thead>
        <tbody>
          <tr v-if="budgets.length === 0"><td colspan="4"><div class="table-empty">Nenhum budget.</div></td></tr>
          <tr v-for="b in budgets" v-else :key="b.id">
            <td>{{ b.tipo }}</td>
            <td class="td-right">{{ b.valor != null ? formatarMoeda(b.valor) : '' }}</td>
            <td class="td-center">{{ b.statusDescricao }}</td>
            <td class="td-actions">
              <button type="button" class="btn btn-ghost btn-sm" title="Alocar" @click="abrirAlocar(b)">Alocar</button>
              <button type="button" class="btn btn-ghost btn-sm" title="Aprovar" @click="aprovarBudget(b)">Aprovar</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <AppDialog v-model="alocarVisivel" title="Alocar budget" width="440px">
      <div class="form-grid-modal">
        <!-- TODO: contaId é UUID (conta contábil) sem endpoint de listagem no digest. -->
        <TextField v-model="alocacao.contaId" label="ID da conta" hint="UUID" />
        <MoneyInput v-model="alocacao.valorAlocado" label="Valor alocado" />
        <label class="field toggle-row">
          <span class="field-label">Auto-aprovar</span>
          <input v-model="alocacao.autoAprovar" type="checkbox" />
        </label>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="alocarVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoAlocacao" @click="confirmarAlocacao">
          <span v-if="salvandoAlocacao" class="spinner"></span>
          <span v-else>Alocar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.form-grid-modal { display: grid; grid-template-columns: 1fr; gap: 14px; }
.nova-linha { align-items: end; }
.acao-linha { display: flex; align-items: flex-end; }
.secao-titulo { font-size: 15px; margin-bottom: 14px; }
.toggle-row { display: flex; align-items: center; gap: 10px; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
.mt { margin-top: 18px; width: 100%; }
</style>
