<script setup lang="ts">
/**
 * Formulário de Conta a Pagar (criar/editar) — financeiro/contas-a-pagar/[id].vue.
 * A tela legada (`financeiro/contas-a-pagar/[id].vue`) é um stub sem implementação real;
 * o comportamento foi portado a partir do par completo Contas a Receber do legado
 * (`useContasAReceberFetchId`/`Post`/`Put`, `types/financeiro/recebimento.ts`), espelhando
 * os mesmos campos para o lado "a pagar" (fornecedor no lugar de cliente).
 *
 * Rota `novo` cria; qualquer outro valor numérico edita o registro existente.
 */
import { computed, onMounted, ref } from 'vue'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import type { SelectOption } from '~/composables/useEnum'
import { situacaoContasAPagarInfo, type ContasAPagar } from '~/components/financeiro-pagar/types'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

interface Fornecedor {
  id: number
  nome: string
  razaoSocial?: string
}

interface NaturezaFinanceira {
  id: number
  descricao: string
}

interface ContasAPagarForm {
  id?: number
  pessoaId: number | null
  planoDeContasFinanceiroItemId: number | null
  documento: string
  detalhamento: string
  dataEmissao: string
  dataVencimento: string
  valorTitulo: number
  numeroParcela: number
}

const route = useRoute()
const toast = useToast()
const { formatarMoeda, paraIsoData } = useHelper()

const idParam = computed(() => {
  const p = route.params.id
  return Array.isArray(p) ? p[0] : p
})
const isEdit = computed(() => !!idParam.value && idParam.value !== 'novo')
const tituloPagina = computed(() => (isEdit.value ? 'Editar Conta a Pagar' : 'Nova Conta a Pagar'))

const form = ref<ContasAPagarForm>({
  pessoaId: null,
  planoDeContasFinanceiroItemId: null,
  documento: '',
  detalhamento: '',
  dataEmissao: new Date().toISOString().slice(0, 10),
  dataVencimento: '',
  valorTitulo: 0,
  numeroParcela: 1
})

const registroCarregado = ref<ContasAPagar | null>(null)
const erros = ref<Record<string, string>>({})

const carregando = ref(false)
const salvando = ref(false)

const fornecedores = ref<Fornecedor[]>([])
const carregandoFornecedores = ref(false)
const opcoesFornecedores = computed<SelectOption[]>(() =>
  fornecedores.value.map((f) => ({ label: f.razaoSocial ?? f.nome, value: f.id }))
)

const naturezas = ref<NaturezaFinanceira[]>([])
const carregandoNaturezas = ref(false)
const opcoesNaturezas = computed<SelectOption[]>(() =>
  naturezas.value.map((n) => ({ label: n.descricao, value: n.id }))
)

const situacaoAtual = computed(() =>
  registroCarregado.value ? situacaoContasAPagarInfo(registroCarregado.value.situacao) : null
)

const valorPago = computed(() => registroCarregado.value?.valorTotalPago ?? 0)
const valorRestante = computed(() =>
  registroCarregado.value ? Math.max(0, registroCarregado.value.valorTitulo - valorPago.value) : 0
)

async function carregarFornecedores() {
  carregandoFornecedores.value = true
  try {
    // Fornecedores são pessoas cadastradas em cadastros/pessoas (mesmo cadastro de clientes).
    const resposta = await useApi('/cadastros/pessoas', { query: { tamanhoPagina: 200 } })
    fornecedores.value = extrairLista<Fornecedor>(resposta) ?? []
  } catch (e) {
    console.error('[contas-a-pagar/[id]] erro ao carregar fornecedores', e)
  } finally {
    carregandoFornecedores.value = false
  }
}

async function carregarNaturezas() {
  carregandoNaturezas.value = true
  try {
    const resposta = await useApi('/configuracao-codigo-naturezas-financeiras', { query: { tamanhoPagina: 200 } })
    naturezas.value = extrairLista<NaturezaFinanceira>(resposta) ?? []
  } catch (e) {
    console.error('[contas-a-pagar/[id]] erro ao carregar naturezas financeiras', e)
  } finally {
    carregandoNaturezas.value = false
  }
}

async function carregarConta() {
  if (!isEdit.value || !idParam.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/financeiro/contas-pagar/{id}`, { params: { id: idParam.value } })
    const dados = extrairDados<ContasAPagar>(resposta)
    if (dados) {
      registroCarregado.value = dados
      form.value = {
        id: dados.id,
        pessoaId: dados.pessoaId,
        planoDeContasFinanceiroItemId: dados.planoDeContasFinanceiroItemId,
        documento: dados.documento,
        detalhamento: dados.detalhamento,
        dataEmissao: dados.dataEmissao?.slice(0, 10) ?? '',
        dataVencimento: dados.dataVencimento?.slice(0, 10) ?? '',
        valorTitulo: dados.valorTitulo,
        numeroParcela: dados.numeroParcela
      }
    }
  } catch (e) {
    toast.error('Não foi possível carregar a conta a pagar')
    console.error('[contas-a-pagar/[id]] erro ao carregar conta', e)
    await navigateTo('/erp/financeiro/contas-a-pagar')
  } finally {
    carregando.value = false
  }
}

function validar(): boolean {
  const novosErros: Record<string, string> = {}
  if (!form.value.pessoaId) novosErros.pessoaId = 'Fornecedor é obrigatório'
  if (!form.value.documento?.trim()) novosErros.documento = 'Documento é obrigatório'
  if (!form.value.dataVencimento) novosErros.dataVencimento = 'Vencimento é obrigatório'
  if (!form.value.valorTitulo || form.value.valorTitulo <= 0) novosErros.valorTitulo = 'Valor deve ser maior que zero'
  erros.value = novosErros
  return Object.keys(novosErros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação')
    return
  }

  salvando.value = true
  try {
    const payload = {
      ...form.value,
      dataEmissao: paraIsoData(new Date(form.value.dataEmissao)),
      dataVencimento: paraIsoData(new Date(form.value.dataVencimento))
    }

    if (isEdit.value && idParam.value) {
      await useApi(`/financeiro/contas-pagar/{id}`, {
        method: 'PUT',
        params: { id: idParam.value },
        body: { ...payload, id: Number(idParam.value) }
      })
      toast.success('Conta a pagar atualizada com sucesso!')
    } else {
      await useApi('/financeiro/contas-pagar', { method: 'POST', body: payload })
      toast.success('Conta a pagar criada com sucesso!')
    }
    await navigateTo('/erp/financeiro/contas-a-pagar')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  navigateTo('/erp/financeiro/contas-a-pagar')
}

onMounted(async () => {
  await Promise.all([carregarFornecedores(), carregarNaturezas()])
  if (isEdit.value) {
    await carregarConta()
  }
})
</script>

<template>
  <div>
    <PageToolbar :title="tituloPagina" :loading="carregando || salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div v-if="isEdit && situacaoAtual" class="glass-panel resumo-panel">
      <div class="resumo-item">
        <span class="resumo-label">Situação</span>
        <span class="badge" :class="`badge-${situacaoAtual.classe}`">{{ situacaoAtual.texto }}</span>
      </div>
      <div class="resumo-item">
        <span class="resumo-label">Valor Pago</span>
        <span class="resumo-valor">{{ formatarMoeda(valorPago) }}</span>
      </div>
      <div class="resumo-item">
        <span class="resumo-label">Valor Restante</span>
        <span class="resumo-valor" :class="{ 'valor-negativo': valorRestante > 0 }">{{ formatarMoeda(valorRestante) }}</span>
      </div>
    </div>

    <div class="glass-panel form-panel">
      <div class="form-grid">
        <SelectField
          v-model="form.pessoaId"
          label="Fornecedor"
          required
          :options="opcoesFornecedores"
          :disabled="carregandoFornecedores"
          :error="erros.pessoaId"
          class="col-6"
        />
        <SelectField
          v-model="form.planoDeContasFinanceiroItemId"
          label="Natureza Financeira"
          :options="opcoesNaturezas"
          :disabled="carregandoNaturezas"
          class="col-6"
        />

        <TextField
          v-model="form.documento"
          label="Documento"
          required
          :error="erros.documento"
          class="col-4"
        />
        <TextField
          v-model.number="form.numeroParcela"
          type="number"
          label="Nº Parcela"
          class="col-2"
        />
        <MoneyInput
          v-model="form.valorTitulo"
          label="Valor do Título"
          required
          :error="erros.valorTitulo"
          class="col-3"
        />

        <DateTimeField v-model="form.dataEmissao" label="Data de Emissão" class="col-3" />
        <DateTimeField
          v-model="form.dataVencimento"
          label="Data de Vencimento"
          required
          :error="erros.dataVencimento"
          class="col-3"
        />

        <div class="field col-12">
          <label class="field-label">Detalhamento</label>
          <textarea v-model="form.detalhamento" class="input textarea" rows="3"></textarea>
        </div>
      </div>
    </div>

    <div v-if="isEdit && registroCarregado && registroCarregado.contasAPagarItens.length" class="glass-panel form-panel">
      <h3 class="secao-titulo">Pagamentos Registrados</h3>
      <table class="admin-table">
        <thead>
          <tr>
            <th>Data Pagamento</th>
            <th>Desconto</th>
            <th>Juros</th>
            <th>Multa</th>
            <th class="td-right">Valor Pago</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in registroCarregado.contasAPagarItens" :key="item.id">
            <td>{{ item.dataPagamento ? new Date(item.dataPagamento).toLocaleDateString('pt-BR') : '-' }}</td>
            <td>{{ formatarMoeda(item.valorDesconto) }}</td>
            <td>{{ formatarMoeda(item.valorJuros) }}</td>
            <td>{{ formatarMoeda(item.valorMulta) }}</td>
            <td class="td-right">{{ formatarMoeda(item.valorPago) }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.resumo-panel {
  display: flex;
  gap: 24px;
  padding: 14px 18px;
  margin-bottom: 16px;
}
.resumo-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.resumo-label {
  font-size: 11px;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.4px;
}
.resumo-valor {
  font-size: 16px;
  font-weight: 700;
}
.resumo-valor.valor-negativo {
  color: var(--danger);
}
.form-panel {
  padding: 16px;
  margin-bottom: 16px;
}
.secao-titulo {
  font-size: 14px;
  color: var(--text-primary);
  margin: 0 0 12px;
}
.form-grid {
  display: grid;
  grid-template-columns: repeat(12, 1fr);
  gap: 12px 16px;
}
.col-2 { grid-column: span 2; }
.col-3 { grid-column: span 3; }
.col-4 { grid-column: span 4; }
.col-6 { grid-column: span 6; }
.col-12 { grid-column: span 12; }
.textarea {
  width: 100%;
  resize: vertical;
  font-family: inherit;
}
@media (max-width: 900px) {
  .col-2, .col-3, .col-4, .col-6 { grid-column: span 12; }
}
</style>
