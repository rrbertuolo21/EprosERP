<script setup lang="ts">
/**
 * Formulário de Conta a Receber (novo/edição) — Financeiro / Contas a Receber.
 *
 * Porta o comportamento de `financeiro/contas-a-receber/[id].vue` do legado:
 * lançamento único ou parcelado (com geração automática de parcelas), ajustes de
 * valores (desconto/multa/juros/acréscimo), plano de contas, cliente, justificativa
 * de cancelamento e histórico de pagamentos já recebidos.
 */
import { ref, computed, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import { useEnum, type SelectOption } from '~/composables/useEnum'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import BaixaPagamentoDialog from '~/components/financeiro-receber/BaixaPagamentoDialog.vue'
import ClienteAutocomplete from '~/components/financeiro-receber/ClienteAutocomplete.vue'
import type { ContasAReceber, ContasAReceberItem, PlanoDeContasItem } from '~/components/financeiro-receber/types'

definePageMeta({ layout: 'default' })

type TipoOcorrencia = 'unica' | 'parcelada'
type IntervaloParcela = 'semanal' | 'quinzenal' | 'mensal' | 'bimestral' | 'trimestral' | 'semestral' | 'anual'

interface ParcelaGerada {
  numeroParcela: number
  dataVencimento: string
  valorTitulo: number
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { formatarMoeda, formatarData, paraData, paraIsoData } = useHelper()
const { carregarOpcoes } = useEnum()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')
const contaId = computed(() => (isEdit.value ? Number(idParam) : undefined))

const carregando = ref(false)
const salvando = ref(false)
const excluindo = ref(false)
const excluirVisivel = ref(false)
const showBaixaDialog = ref(false)

const tipoOcorrencia = ref<TipoOcorrencia>('unica')
const nomeCliente = ref('')
const opcoesSituacao = ref<SelectOption[]>([])
const opcoesPlanoContas = ref<SelectOption[]>([])
const carregandoPlanoContas = ref(false)

const intervaloOptions: { label: string; value: IntervaloParcela }[] = [
  { label: 'Semanal', value: 'semanal' },
  { label: 'Quinzenal', value: 'quinzenal' },
  { label: 'Mensal', value: 'mensal' },
  { label: 'Bimestral', value: 'bimestral' },
  { label: 'Trimestral', value: 'trimestral' },
  { label: 'Semestral', value: 'semestral' },
  { label: 'Anual', value: 'anual' }
]

const parcelamento = reactive<{
  intervalo: IntervaloParcela
  numero: number | null
  total: number | null
  primeiraData: string | null
  itens: ParcelaGerada[]
}>({
  intervalo: 'mensal',
  numero: null,
  total: null,
  primeiraData: null,
  itens: []
})

const parcelasColunas: DataTableColumn<ParcelaGerada>[] = [
  { key: 'numeroParcela', label: 'Parcela', align: 'center', width: '90px' },
  { key: 'dataVencimento', label: 'Vencimento', align: 'center', width: '140px' },
  { key: 'valorTitulo', label: 'Valor', align: 'right', width: '160px' }
]

const historicoColunas: DataTableColumn<ContasAReceberItem>[] = [
  { key: 'dataRecebimento', label: 'Data Pagamento', align: 'center' },
  { key: 'formaPagamento', label: 'Forma Recebimento' },
  { key: 'valorDesconto', label: 'Desconto', align: 'right' },
  { key: 'valorJuros', label: 'Juros', align: 'right' },
  { key: 'valorMulta', label: 'Multa', align: 'right' },
  { key: 'valorAcrescimo', label: 'Acréscimo', align: 'right' },
  { key: 'valorParcela', label: 'Valor Pago', align: 'right' },
  { key: 'valorPago', label: 'Valor Recebido', align: 'right' }
]

const form = reactive<ContasAReceber>({
  documento: '',
  numeroParcela: 1,
  situacao: 1,
  dataEmissao: paraIsoData(new Date()),
  dataVencimento: null,
  dataBaixa: null,
  detalhamento: null,
  pessoaId: null,
  planoDeContasFinanceiroItemId: null,
  valorTitulo: 0,
  valorInicialDesconto: 0,
  valorInicialMulta: 0,
  valorInicialJuros: 0,
  valorInicialAcrescimo: 0,
  valorTotalDesconto: 0,
  valorTotalMulta: 0,
  valorTotalJuros: 0,
  valorTotalAcrescimo: 0,
  valorTituloLiquido: 0,
  valorTotalRecebido: 0,
  justificativaCancelamento: null,
  contasAReceberItens: []
})

const erros = reactive<Record<string, string>>({})

const isCancelado = computed(() => form.situacao === 4)

const origemDescricao = computed(() => form.fatoGeradorFinanceiro?.descricao ?? '')

const totalAReceber = computed(() => {
  return form.valorTitulo + form.valorInicialAcrescimo + form.valorInicialJuros + form.valorInicialMulta - form.valorInicialDesconto
})

const valorPago = computed(() => {
  return form.contasAReceberItens?.reduce((acc, item) => acc + (item.valorPago ?? 0), 0) ?? 0
})

const troco = computed(() => valorPago.value - totalAReceber.value)

const totalParcelas = computed(() => parcelamento.itens.reduce((acc, item) => acc + item.valorTitulo, 0))

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()
  if (!form.documento) erros.documento = 'Número do documento é obrigatório.'
  if (!form.situacao) erros.situacao = 'Situação é obrigatória.'
  if (!form.dataEmissao) erros.dataEmissao = 'Data de emissão é obrigatória.'
  if (tipoOcorrencia.value === 'unica' && !form.dataVencimento) erros.dataVencimento = 'Data de vencimento é obrigatória.'
  if (!form.pessoaId) erros.pessoaId = 'Cliente é obrigatório.'
  if (!form.planoDeContasFinanceiroItemId) erros.planoDeContasFinanceiroItemId = 'Plano de contas é obrigatório.'
  if (isCancelado.value && !form.justificativaCancelamento) erros.justificativaCancelamento = 'Informe a justificativa do cancelamento.'
  return Object.keys(erros).length === 0
}

function proximaData(data: Date, intervalo: IntervaloParcela): Date {
  const d = new Date(data)
  switch (intervalo) {
    case 'semanal':
      d.setDate(d.getDate() + 7)
      break
    case 'quinzenal':
      d.setDate(d.getDate() + 14)
      break
    case 'mensal':
      d.setMonth(d.getMonth() + 1)
      break
    case 'bimestral':
      d.setMonth(d.getMonth() + 2)
      break
    case 'trimestral':
      d.setMonth(d.getMonth() + 3)
      break
    case 'semestral':
      d.setMonth(d.getMonth() + 6)
      break
    case 'anual':
      d.setFullYear(d.getFullYear() + 1)
      break
  }
  return d
}

function gerarParcelas() {
  if (!parcelamento.numero) {
    toast.error('Informe o número de parcelas.')
    return
  }
  if (!parcelamento.total) {
    toast.error('Informe o valor total.')
    return
  }
  if (!parcelamento.primeiraData) {
    toast.error('Informe a data da primeira parcela.')
    return
  }

  const itens: ParcelaGerada[] = []
  let saldo = parcelamento.total
  let data = paraData(parcelamento.primeiraData) ?? new Date()

  for (let i = 0; i < parcelamento.numero; i++) {
    const valorParcela = Math.ceil((saldo / (parcelamento.numero - i)) * 100) / 100
    saldo -= valorParcela
    itens.push({
      numeroParcela: i + 1,
      dataVencimento: paraIsoData(data) ?? '',
      valorTitulo: valorParcela
    })
    data = proximaData(data, parcelamento.intervalo)
  }

  parcelamento.itens = itens
}

async function carregarPlanoContas() {
  carregandoPlanoContas.value = true
  try {
    const resposta = await useApi('/configuracao-codigo-naturezas-financeiras', { query: { tamanhoPagina: 1000 } })
    const lista = extrairLista<PlanoDeContasItem>(resposta) ?? []
    opcoesPlanoContas.value = lista.map((c) => ({ label: c.descricaoFormatada ?? c.descricao ?? String(c.id), value: c.id }))
  } catch (e) {
    console.error('[contas-a-receber/[id]] plano-contas', e)
    opcoesPlanoContas.value = []
  } finally {
    carregandoPlanoContas.value = false
  }
}

async function carregarConta() {
  if (!isEdit.value || !contaId.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/financeiro/contas-receber/${contaId.value}`)
    const dados = extrairDados<Partial<ContasAReceber>>(resposta)
    if (dados) {
      Object.assign(form, dados)
      if (!form.contasAReceberItens) form.contasAReceberItens = []
      nomeCliente.value = dados.nomePessoa ?? ''
    }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function salvarUnica() {
  if (isEdit.value) {
    await useApi(`/financeiro/contas-receber/${contaId.value}`, { method: 'PUT', body: form })
  } else {
    await useApi('/financeiro/contas-receber', { method: 'POST', body: form })
  }
}

async function salvarParcelado() {
  if (parcelamento.itens.length === 0) {
    toast.error('Gere as parcelas antes de salvar.')
    throw new Error('sem-parcelas')
  }
  const requisicoes = parcelamento.itens.map((parcela) =>
    useApi('/financeiro/contas-receber', {
      method: 'POST',
      body: {
        ...form,
        dataVencimento: parcela.dataVencimento,
        valorTitulo: parcela.valorTitulo,
        numeroParcela: parcela.numeroParcela
      }
    })
  )
  await Promise.all(requisicoes)
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }

  salvando.value = true
  try {
    if (tipoOcorrencia.value === 'parcelada' && !isEdit.value) {
      await salvarParcelado()
      toast.success('Parcelas salvas com sucesso!')
    } else {
      await salvarUnica()
      toast.success('Conta a receber salva com sucesso!')
    }
    router.push('/erp/financeiro/contas-a-receber')
  } catch (e) {
    if ((e as Error)?.message !== 'sem-parcelas') {
      toast.error(obterMensagemErro(e))
    }
  } finally {
    salvando.value = false
  }
}

async function excluir() {
  if (!contaId.value) return
  excluindo.value = true
  try {
    await useApi(`/financeiro/contas-receber/${contaId.value}`, { method: 'DELETE' })
    toast.success('Conta a receber excluída com sucesso!')
    router.push('/erp/financeiro/contas-a-receber')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
    excluirVisivel.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/contas-a-receber')
}

function aoSalvarBaixa() {
  void carregarConta()
}

onMounted(async () => {
  opcoesSituacao.value = await carregarOpcoes('financeiros-enums/situacao-titulo')
  await carregarPlanoContas()
  await carregarConta()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar Conta a Receber' : 'Nova Conta a Receber'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
        <button v-if="isEdit" type="button" class="btn btn-danger" :disabled="excluindo" @click="excluirVisivel = true">Excluir</button>
        <button v-if="isEdit" type="button" class="btn btn-secondary" @click="showBaixaDialog = true">Baixa</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="form-layout">
      <div class="form-coluna-principal">
        <div v-if="form.fatoGeradorFinanceiro" class="alerta-origem">
          <strong>{{ origemDescricao }}:</strong> {{ form.fatoGeradorFinanceiro.descricao }}
        </div>

        <div v-if="!isEdit" class="ocorrencia-row">
          <span class="ocorrencia-label">Ocorrência:</span>
          <label class="radio-option">
            <input v-model="tipoOcorrencia" type="radio" value="unica" />
            <span>Única</span>
          </label>
          <label class="radio-option">
            <input v-model="tipoOcorrencia" type="radio" value="parcelada" />
            <span>Parcelada</span>
          </label>
        </div>

        <div class="glass-panel form-panel">
          <h3 class="panel-titulo">Informações Básicas</h3>
          <div class="form-grid">
            <TextField v-model="form.documento" label="Número do Documento" required :error="erros.documento" />
            <TextField
              v-model="form.numeroParcela"
              label="Número da Parcela"
              type="number"
              :disabled="tipoOcorrencia === 'parcelada' && !isEdit"
            />
            <SelectField v-model="form.situacao" label="Situação" required :options="opcoesSituacao" :error="erros.situacao" />
            <DateTimeField v-model="form.dataEmissao" label="Data de Emissão" required :error="erros.dataEmissao" />
            <DateTimeField
              v-model="form.dataVencimento"
              label="Data de Vencimento"
              :required="tipoOcorrencia === 'unica'"
              :disabled="tipoOcorrencia === 'parcelada' && !isEdit"
              :error="erros.dataVencimento"
            />
            <DateTimeField v-model="form.dataBaixa" label="Data de Baixa" :disabled="tipoOcorrencia === 'parcelada' && !isEdit" />
            <div class="field-col-full">
              <TextField v-model="form.detalhamento" label="Descrição" />
            </div>
            <div class="field-col-full">
              <ClienteAutocomplete
                v-model="form.pessoaId"
                v-model:nome-selecionado="nomeCliente"
                :error="erros.pessoaId"
              />
            </div>
            <div class="field-col-full">
              <SelectField
                v-model="form.planoDeContasFinanceiroItemId"
                label="Plano de Contas"
                required
                :options="opcoesPlanoContas"
                :disabled="carregandoPlanoContas"
                :error="erros.planoDeContasFinanceiroItemId"
              />
            </div>
          </div>
        </div>

        <div v-if="tipoOcorrencia === 'parcelada' && !isEdit" class="glass-panel form-panel">
          <h3 class="panel-titulo">Parcelas</h3>
          <div class="parcelas-grid">
            <SelectField
              v-model="parcelamento.intervalo"
              label="Intervalo de Parcelas"
              :options="intervaloOptions"
              :clearable="false"
            />
            <TextField v-model="parcelamento.numero" label="Número de Parcelas" type="number" />
            <MoneyInput v-model="parcelamento.total" label="Valor Total" />
            <DateTimeField v-model="parcelamento.primeiraData" label="Vencimento da 1ª Parcela" />
            <button type="button" class="btn btn-primary parcelas-gerar" @click="gerarParcelas">Gerar Parcelas</button>
          </div>

          <div v-if="parcelamento.itens.length" class="parcelas-tabela">
            <DataTable
              :items="parcelamento.itens"
              :columns="parcelasColunas"
              :total="parcelamento.itens.length"
              :page="1"
              :page-size="parcelamento.itens.length"
            >
              <template #cell-dataVencimento="{ row }">
                <DateTimeField v-model="row.dataVencimento" />
              </template>
              <template #cell-valorTitulo="{ row }">
                <MoneyInput v-model="row.valorTitulo" />
              </template>
            </DataTable>
            <div class="parcelas-total">
              <span>Total:</span>
              <strong>{{ formatarMoeda(totalParcelas) }}</strong>
            </div>
          </div>
        </div>

        <div v-if="isCancelado" class="glass-panel form-panel">
          <h3 class="panel-titulo">Justificativa de Cancelamento</h3>
          <div class="field">
            <textarea
              v-model="form.justificativaCancelamento"
              class="textarea"
              maxlength="500"
              placeholder="Descreva o motivo do cancelamento..."
            ></textarea>
            <span v-if="erros.justificativaCancelamento" class="field-error">{{ erros.justificativaCancelamento }}</span>
          </div>
        </div>
      </div>

      <div v-if="tipoOcorrencia === 'unica'" class="form-coluna-lateral">
        <div class="glass-panel form-panel">
          <h3 class="panel-titulo">Valores e Ajustes</h3>
          <div class="valores-grid">
            <MoneyInput v-model="form.valorTitulo" label="Valor do Título" />
            <MoneyInput v-model="form.valorInicialDesconto" label="Desconto" />
            <MoneyInput v-model="form.valorInicialMulta" label="Multa" />
            <MoneyInput v-model="form.valorInicialJuros" label="Juros" />
            <MoneyInput v-model="form.valorInicialAcrescimo" label="Acréscimos" />
            <hr class="divisor" />
            <MoneyInput :model-value="totalAReceber" label="Valor a Receber" readonly class="valor-destaque valor-receber" />
            <MoneyInput :model-value="valorPago" label="Valor Pago" readonly class="valor-destaque valor-pago" />
            <MoneyInput :model-value="troco" label="Troco" readonly class="valor-destaque valor-troco" />
          </div>
        </div>
      </div>
    </div>

    <div v-if="form.contasAReceberItens?.length" class="glass-panel form-panel historico-panel">
      <h3 class="panel-titulo">Histórico de Pagamentos</h3>
      <DataTable
        :items="form.contasAReceberItens"
        :columns="historicoColunas"
        :total="form.contasAReceberItens.length"
        :page="1"
        :page-size="form.contasAReceberItens.length"
        empty-text="Nenhum pagamento realizado"
      >
        <template #cell-dataRecebimento="{ value }">
          {{ formatarData(value as string) }}
        </template>
        <template #cell-valorDesconto="{ value }">{{ formatarMoeda((value as number) ?? 0) }}</template>
        <template #cell-valorJuros="{ value }">{{ formatarMoeda((value as number) ?? 0) }}</template>
        <template #cell-valorMulta="{ value }">{{ formatarMoeda((value as number) ?? 0) }}</template>
        <template #cell-valorAcrescimo="{ value }">{{ formatarMoeda((value as number) ?? 0) }}</template>
        <template #cell-valorParcela="{ value }">{{ formatarMoeda((value as number) ?? 0) }}</template>
        <template #cell-valorPago="{ value }">{{ formatarMoeda((value as number) ?? 0) }}</template>
      </DataTable>
    </div>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="form.documento"
      :loading="excluindo"
      @confirm="excluir"
    />

    <BaixaPagamentoDialog v-model="showBaixaDialog" :item-id="contaId" @saved="aoSalvarBaixa" />
  </div>
</template>

<style scoped>
.form-layout {
  display: flex;
  gap: 20px;
  align-items: flex-start;
  flex-wrap: wrap;
}
.form-coluna-principal { flex: 1 1 560px; min-width: 0; display: flex; flex-direction: column; gap: 16px; }
.form-coluna-lateral { flex: 0 1 360px; min-width: 320px; }
.form-panel { padding: 18px 20px; }
.panel-titulo { font-size: 15px; font-weight: 700; margin-bottom: 14px; }
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
}
.field-col-full { grid-column: 1 / -1; }
.alerta-origem {
  padding: 10px 14px;
  border-left: 3px solid var(--text-muted);
  background: rgba(255, 255, 255, 0.03);
  border-radius: 6px;
  font-size: 13px;
  color: var(--text-secondary);
}
.ocorrencia-row { display: flex; align-items: center; gap: 16px; padding: 4px 4px 0; }
.ocorrencia-label { font-weight: 700; font-size: 14px; }
.radio-option { display: flex; align-items: center; gap: 6px; font-size: 13px; cursor: pointer; }
.radio-option input { accent-color: var(--primary); width: 16px; height: 16px; }
.parcelas-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 14px;
  align-items: end;
}
.parcelas-gerar { height: 38px; }
.parcelas-tabela { margin-top: 16px; }
.parcelas-total {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  padding: 12px;
  font-size: 15px;
}
.valores-grid { display: flex; flex-direction: column; gap: 12px; }
.divisor { border: none; border-top: 1px solid var(--border-color); margin: 4px 0; }
.valor-destaque :deep(input) { font-weight: 700; }
.valor-receber :deep(input) { color: var(--success, #22c55e); }
.valor-pago :deep(input) { color: var(--info, #3b82f6); }
.valor-troco :deep(input) { color: var(--warning, #f59e0b); }
.historico-panel { margin-top: 20px; }
.textarea {
  width: 100%;
  min-height: 100px;
  border-radius: 8px;
  border: 1px solid var(--border-color);
  background: rgba(255, 255, 255, 0.02);
  color: var(--text-primary);
  padding: 10px 12px;
  font-size: 13.5px;
  font-family: inherit;
  resize: vertical;
}
</style>
