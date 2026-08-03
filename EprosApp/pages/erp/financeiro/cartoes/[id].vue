<script setup lang="ts">
/**
 * Cartão de Crédito — novo/edição + gestão de faturas.
 * POST /cartoes-credito · GET/PUT /cartoes-credito/{id} ·
 * GET/POST /cartoes-credito/{id}/faturas · POST /cartoes-credito/faturas/{faturaId}/baixar.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import type { SelectOption } from '~/composables/useEnum'
import { OPCOES_BANDEIRA_CARTAO, carregarOpcoesDe } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface CartaoForm {
  id?: string
  contaBancariaId: string | null
  apelido: string | null
  titular: string | null
  bandeiraCartao: number | null
  observacao: string | null
}
interface Fatura {
  id: string
  dataLancamento?: string | null
  dataVencimento?: string | null
  valor?: number | null
  pago?: boolean | null
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
const opcoesConta = ref<SelectOption[]>([])
const form = reactive<CartaoForm>({
  contaBancariaId: null,
  apelido: null,
  titular: null,
  bandeiraCartao: -1,
  observacao: null
})
const erros = reactive<Record<string, string>>({})

// --- Faturas
const faturas = ref<Fatura[]>([])
const carregandoFaturas = ref(false)
const novaFatura = reactive<{ dataLancamento: string | null; dataVencimento: string | null; valor: number | null; pago: boolean }>({
  dataLancamento: null,
  dataVencimento: null,
  valor: null,
  pago: false
})
const salvandoFatura = ref(false)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.contaBancariaId) erros.contaBancariaId = 'Conta bancária é obrigatória.'
  if (form.bandeiraCartao == null) erros.bandeiraCartao = 'Bandeira é obrigatória.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi('/cartoes-credito/{id}', { params: { id: idParam } })
    const dados = extrairDados<CartaoForm>(resposta)
    if (dados) Object.assign(form, dados)
    await carregarFaturas()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function carregarFaturas() {
  if (!isEdit.value) return
  carregandoFaturas.value = true
  try {
    const resposta = await useApi('/cartoes-credito/{id}/faturas', { params: { id: idParam } })
    faturas.value = extrairDados<Fatura[]>(resposta) ?? []
  } catch (e) {
    console.error('[cartoes/[id]] faturas', e)
    faturas.value = []
  } finally {
    carregandoFaturas.value = false
  }
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    if (isEdit.value) {
      await useApi('/cartoes-credito/{id}', { method: 'PUT', params: { id: idParam }, body: { id: idParam, ...form } })
    } else {
      await useApi('/cartoes-credito', { method: 'POST', body: { ...form } })
    }
    toast.success('Registro salvo com sucesso!')
    router.push('/erp/financeiro/cartoes')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function adicionarFatura() {
  if (!novaFatura.dataLancamento || !novaFatura.dataVencimento || novaFatura.valor == null) {
    toast.error('Preencha lançamento, vencimento e valor da fatura.')
    return
  }
  salvandoFatura.value = true
  try {
    await useApi('/cartoes-credito/{id}/faturas', { method: 'POST', params: { id: idParam }, body: { ...novaFatura } })
    toast.success('Fatura adicionada.')
    novaFatura.dataLancamento = null
    novaFatura.dataVencimento = null
    novaFatura.valor = null
    novaFatura.pago = false
    await carregarFaturas()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoFatura.value = false
  }
}

async function baixarFatura(f: Fatura) {
  const ok = await confirmRef.value!.open('Baixar fatura', 'Confirma a baixa desta fatura como paga?')
  if (!ok) return
  try {
    await useApi('/cartoes-credito/faturas/{faturaId}/baixar', { method: 'POST', params: { faturaId: f.id } })
    toast.success('Fatura baixada.')
    await carregarFaturas()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

function cancelar() {
  router.push('/erp/financeiro/cartoes')
}

onMounted(async () => {
  opcoesConta.value = await carregarOpcoesDe('/contas-bancarias', ['apelido', 'titular', 'conta'])
  await carregar()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar cartão' : 'Novo cartão'" :loading="carregando">
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
          <SelectField v-model="form.contaBancariaId" label="Conta bancária" required :options="opcoesConta" :error="erros.contaBancariaId" />
          <TextField v-model="form.apelido" label="Apelido" maxlength="60" />
          <TextField v-model="form.titular" label="Titular" maxlength="80" />
          <SelectField v-model="form.bandeiraCartao" label="Bandeira" required :options="OPCOES_BANDEIRA_CARTAO" :clearable="false" :error="erros.bandeiraCartao" />
          <TextField v-model="form.observacao" label="Observação" maxlength="200" />
        </div>
      </form>
    </div>

    <div v-if="isEdit" class="glass-panel form-panel">
      <h3 class="secao-titulo">Faturas</h3>
      <div class="form-grid nova-fatura">
        <DateTimeField v-model="novaFatura.dataLancamento" label="Lançamento" />
        <DateTimeField v-model="novaFatura.dataVencimento" label="Vencimento" />
        <MoneyInput v-model="novaFatura.valor" label="Valor" />
        <label class="field toggle-row">
          <span class="field-label">{{ novaFatura.pago ? 'Paga' : 'Em aberto' }}</span>
          <input v-model="novaFatura.pago" type="checkbox" />
        </label>
        <div class="acao-fatura">
          <button type="button" class="btn btn-secondary" :disabled="salvandoFatura" @click="adicionarFatura">+ Adicionar fatura</button>
        </div>
      </div>

      <table class="admin-table mt">
        <thead>
          <tr>
            <th>Lançamento</th><th>Vencimento</th><th class="td-right">Valor</th><th class="td-center">Situação</th><th class="td-actions">Ações</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="carregandoFaturas"><td colspan="5" class="table-loading"><span class="spinner"></span> Carregando...</td></tr>
          <tr v-else-if="faturas.length === 0"><td colspan="5"><div class="table-empty">Nenhuma fatura.</div></td></tr>
          <tr v-for="f in faturas" v-else :key="f.id">
            <td>{{ f.dataLancamento ? formatarData(f.dataLancamento) : '' }}</td>
            <td>{{ f.dataVencimento ? formatarData(f.dataVencimento) : '' }}</td>
            <td class="td-right">{{ formatarMoeda(f.valor as number) }}</td>
            <td class="td-center">
              <span class="badge" :class="f.pago ? 'badge-success' : 'badge-danger'">{{ f.pago ? 'Paga' : 'Em aberto' }}</span>
            </td>
            <td class="td-actions">
              <button type="button" class="btn btn-ghost btn-sm" :disabled="!!f.pago" title="Baixar" @click="baixarFatura(f)">Baixar</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.secao-titulo { font-size: 15px; margin-bottom: 14px; }
.nova-fatura { align-items: end; }
.acao-fatura { display: flex; align-items: flex-end; }
.toggle-row { display: flex; align-items: center; gap: 10px; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
.mt { margin-top: 18px; width: 100%; }
</style>
