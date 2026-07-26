<script setup lang="ts">
/**
 * Contrato Financeiro — novo (POST) e detalhe (GET /{id}) com ações e sub-recursos.
 * POST /contratos-financeiros · GET /{id} ·
 * POST /{id}/suspender | /reativar | /cancelar | /reajustar ·
 * GET/POST /{id}/faturas · GET /{id}/reajustes · POST /faturas/{faturaId}/cancelar.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import type { SelectOption } from '~/composables/useEnum'
import { carregarOpcoesDe } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface ContratoDetalhe {
  id?: string
  planoId: string | null
  pessoaId: string | null
  dataInicio: string | null
  statusDescricao?: string | null
  valorAtual?: number | null
}
interface Fatura {
  id: string
  competencia?: string | null
  valor?: number | null
  statusDescricao?: string | null
}
interface Reajuste {
  id: string
  dataReajuste?: string | null
  valorNovo?: number | null
  motivo?: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { formatarData, formatarMoeda } = useHelper()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const aba = ref<'dados' | 'faturas' | 'reajustes'>('dados')
const opcoesPlano = ref<SelectOption[]>([])
const opcoesPessoa = ref<SelectOption[]>([])

const form = reactive<ContratoDetalhe>({ planoId: null, pessoaId: null, dataInicio: null })
const erros = reactive<Record<string, string>>({})

const faturas = ref<Fatura[]>([])
const reajustes = ref<Reajuste[]>([])

// gerar fatura
const novaFatura = reactive<{ competencia: string | null; valor: number | null }>({ competencia: null, valor: null })
const salvandoFatura = ref(false)

// reajustar
const reajusteVisivel = ref(false)
const reajuste = reactive<{ dataReajuste: string | null; valorNovo: number | null; motivo: string | null }>({ dataReajuste: null, valorNovo: null, motivo: null })
const salvandoReajuste = ref(false)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.planoId) erros.planoId = 'Plano é obrigatório.'
  if (!form.pessoaId) erros.pessoaId = 'Pessoa é obrigatória.'
  if (!form.dataInicio) erros.dataInicio = 'Data de início é obrigatória.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi('/contratos-financeiros/{id}', { params: { id: idParam } })
    const dados = extrairDados<ContratoDetalhe>(resposta)
    if (dados) Object.assign(form, dados)
    await Promise.all([carregarFaturas(), carregarReajustes()])
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function carregarFaturas() {
  try {
    const resposta = await useApi('/contratos-financeiros/{id}/faturas', { params: { id: idParam } })
    faturas.value = extrairDados<Fatura[]>(resposta) ?? []
  } catch (e) {
    console.error('[contratos/[id]] faturas', e)
  }
}
async function carregarReajustes() {
  try {
    const resposta = await useApi('/contratos-financeiros/{id}/reajustes', { params: { id: idParam } })
    reajustes.value = extrairDados<Reajuste[]>(resposta) ?? []
  } catch (e) {
    console.error('[contratos/[id]] reajustes', e)
  }
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/contratos-financeiros', {
      method: 'POST',
      body: { planoId: form.planoId, pessoaId: form.pessoaId, dataInicio: form.dataInicio }
    })
    toast.success('Contrato criado com sucesso!')
    router.push('/erp/financeiro/contratos-financeiros/contratos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function acaoSimples(nome: 'suspender' | 'reativar', titulo: string) {
  const ok = await confirmRef.value!.open(titulo, 'Confirma esta operação no contrato?')
  if (!ok) return
  try {
    await useApi(`/contratos-financeiros/{id}/${nome}`, { method: 'POST', params: { id: idParam } })
    toast.success('Operação concluída.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

async function cancelarContrato() {
  const ok = await confirmRef.value!.open('Cancelar contrato', 'Confirma o cancelamento do contrato?', { danger: true })
  if (!ok) return
  try {
    await useApi('/contratos-financeiros/{id}/cancelar', { method: 'POST', params: { id: idParam }, body: { id: idParam } })
    toast.success('Contrato cancelado.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

async function gerarFatura() {
  if (novaFatura.competencia == null && novaFatura.valor == null) {
    toast.error('Informe a competência e/ou o valor da fatura.')
    return
  }
  salvandoFatura.value = true
  try {
    await useApi('/contratos-financeiros/{id}/faturas', {
      method: 'POST',
      params: { id: idParam },
      body: { contratoId: idParam, competencia: novaFatura.competencia, valor: novaFatura.valor }
    })
    toast.success('Fatura gerada.')
    novaFatura.competencia = null
    novaFatura.valor = null
    await carregarFaturas()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoFatura.value = false
  }
}

async function cancelarFatura(f: Fatura) {
  const ok = await confirmRef.value!.open('Cancelar fatura', 'Confirma o cancelamento desta fatura?', { danger: true })
  if (!ok) return
  try {
    await useApi('/contratos-financeiros/faturas/{faturaId}/cancelar', { method: 'POST', params: { faturaId: f.id } })
    toast.success('Fatura cancelada.')
    await carregarFaturas()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

async function confirmarReajuste() {
  salvandoReajuste.value = true
  try {
    await useApi('/contratos-financeiros/{id}/reajustar', {
      method: 'POST',
      params: { id: idParam },
      body: { id: idParam, dataReajuste: reajuste.dataReajuste, valorNovo: reajuste.valorNovo, motivo: reajuste.motivo }
    })
    toast.success('Contrato reajustado.')
    reajusteVisivel.value = false
    reajuste.dataReajuste = null
    reajuste.valorNovo = null
    reajuste.motivo = null
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoReajuste.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/contratos-financeiros/contratos')
}

onMounted(async () => {
  const [planos, pessoas] = await Promise.all([
    carregarOpcoesDe('/contratos-financeiros/planos', ['descricao']),
    carregarOpcoesDe('/cadastros/pessoas', ['nome', 'razaoSocial', 'nomeFantasia'])
  ])
  opcoesPlano.value = planos
  opcoesPessoa.value = pessoas
  await carregar()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Contrato financeiro' : 'Novo contrato'" :loading="carregando || salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <template v-if="isEdit">
          <button type="button" class="btn btn-secondary" @click="reajusteVisivel = true">Reajustar</button>
          <button type="button" class="btn btn-secondary" @click="acaoSimples('suspender', 'Suspender contrato')">Suspender</button>
          <button type="button" class="btn btn-secondary" @click="acaoSimples('reativar', 'Reativar contrato')">Reativar</button>
          <button type="button" class="btn btn-danger" @click="cancelarContrato">Cancelar contrato</button>
        </template>
        <button v-else type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <nav v-if="isEdit" class="form-tabs">
        <button type="button" class="tab-link" :class="{ active: aba === 'dados' }" @click="aba = 'dados'">Dados</button>
        <button type="button" class="tab-link" :class="{ active: aba === 'faturas' }" @click="aba = 'faturas'">Faturas</button>
        <button type="button" class="tab-link" :class="{ active: aba === 'reajustes' }" @click="aba = 'reajustes'">Reajustes</button>
      </nav>

      <div v-show="aba === 'dados'" class="mt">
        <div v-if="isEdit && form.statusDescricao" class="status-linha">
          Status: <strong>{{ form.statusDescricao }}</strong>
          <span v-if="form.valorAtual != null"> · Valor atual: <strong>{{ formatarMoeda(form.valorAtual) }}</strong></span>
        </div>
        <div class="form-grid">
          <SelectField v-model="form.planoId" label="Plano" required :options="opcoesPlano" :disabled="isEdit" :error="erros.planoId" />
          <SelectField v-model="form.pessoaId" label="Pessoa" required :options="opcoesPessoa" :disabled="isEdit" :error="erros.pessoaId" />
          <DateTimeField v-model="form.dataInicio" label="Data de início" mode="datetime" required :disabled="isEdit" :error="erros.dataInicio" />
        </div>
      </div>

      <div v-show="aba === 'faturas'" class="mt">
        <div class="form-grid nova-linha">
          <TextField v-model="novaFatura.competencia" label="Competência" placeholder="Ex.: 2026-07" maxlength="20" />
          <MoneyInput v-model="novaFatura.valor" label="Valor (opcional)" />
          <div class="acao-linha">
            <button type="button" class="btn btn-secondary" :disabled="salvandoFatura" @click="gerarFatura">Gerar fatura</button>
          </div>
        </div>
        <table class="admin-table mt">
          <thead><tr><th>Competência</th><th class="td-right">Valor</th><th class="td-center">Status</th><th class="td-actions">Ações</th></tr></thead>
          <tbody>
            <tr v-if="faturas.length === 0"><td colspan="4"><div class="table-empty">Nenhuma fatura.</div></td></tr>
            <tr v-for="f in faturas" v-else :key="f.id">
              <td>{{ f.competencia }}</td>
              <td class="td-right">{{ f.valor != null ? formatarMoeda(f.valor) : '' }}</td>
              <td class="td-center">{{ f.statusDescricao }}</td>
              <td class="td-actions">
                <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Cancelar" @click="cancelarFatura(f)">Cancelar</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-show="aba === 'reajustes'" class="mt">
        <table class="admin-table">
          <thead><tr><th>Data</th><th class="td-right">Valor Novo</th><th>Motivo</th></tr></thead>
          <tbody>
            <tr v-if="reajustes.length === 0"><td colspan="3"><div class="table-empty">Nenhum reajuste.</div></td></tr>
            <tr v-for="r in reajustes" v-else :key="r.id">
              <td>{{ r.dataReajuste ? formatarData(r.dataReajuste) : '' }}</td>
              <td class="td-right">{{ r.valorNovo != null ? formatarMoeda(r.valorNovo) : '' }}</td>
              <td>{{ r.motivo }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <AppDialog v-model="reajusteVisivel" title="Reajustar contrato" width="480px">
      <div class="form-grid-modal">
        <DateTimeField v-model="reajuste.dataReajuste" label="Data do reajuste" mode="datetime" />
        <MoneyInput v-model="reajuste.valorNovo" label="Novo valor" />
        <TextField v-model="reajuste.motivo" label="Motivo" maxlength="200" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="reajusteVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoReajuste" @click="confirmarReajuste">
          <span v-if="salvandoReajuste" class="spinner"></span>
          <span v-else>Confirmar</span>
        </button>
      </template>
    </AppDialog>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-tabs { display: flex; gap: 8px; border-bottom: 1px solid var(--border-color); padding-bottom: 12px; }
.tab-link { background: none; border: none; color: var(--text-secondary); padding: 8px 16px; font-size: 13.5px; font-weight: 600; cursor: pointer; border-radius: 6px; }
.tab-link.active { background: rgba(255,255,255,0.03); color: var(--text-primary); border: 1px solid rgba(99,102,241,0.2); }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.form-grid-modal { display: grid; grid-template-columns: 1fr; gap: 14px; }
.nova-linha { align-items: end; }
.acao-linha { display: flex; align-items: flex-end; }
.status-linha { margin-bottom: 14px; color: var(--text-secondary); font-size: 13px; }
.mt { margin-top: 18px; }
</style>
