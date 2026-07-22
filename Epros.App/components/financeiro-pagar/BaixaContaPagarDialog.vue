<script setup lang="ts">
/**
 * BaixaContaPagarDialog — modal de baixa rápida (registrar pagamento) de uma conta a pagar,
 * acionado a partir da listagem. Envia o pagamento para `financeiro/contas-pagar/{id}/baixar`.
 *
 * Contrato:
 *   props:
 *     modelValue: boolean          (v-model — visível?)
 *     conta: ContasAPagar | null   (título selecionado na listagem)
 *   emits:
 *     'update:modelValue': [value: boolean]
 *     confirmado: []               (baixa registrada com sucesso — a tela chamadora recarrega a lista)
 */
import { computed, ref, watch } from 'vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import AppDialog from '~/components/shared/AppDialog.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import type { SelectOption } from '~/composables/useEnum'
import type { ContasAPagar } from './types'

const props = defineProps<{
  modelValue: boolean
  conta: ContasAPagar | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirmado: []
}>()

const toast = useToast()
const { paraIsoData } = useHelper()

const salvando = ref(false)

const form = ref({
  dataPagamento: new Date().toISOString().slice(0, 10),
  valorPago: 0,
  valorDesconto: 0,
  valorJuros: 0,
  valorMulta: 0,
  contaBancariaId: null as number | null,
  tipoPagamento: null as number | null
})

const opcoesTipoPagamento: SelectOption[] = [
  { label: 'Dinheiro', value: 1 },
  { label: 'PIX', value: 2 },
  { label: 'Cartão de Débito', value: 3 },
  { label: 'Cartão de Crédito', value: 4 },
  { label: 'Boleto', value: 5 },
  { label: 'Transferência', value: 6 }
]

const contasBancarias = ref<{ id: number; descricao: string }[]>([])
const opcoesContasBancarias = computed<SelectOption[]>(() =>
  contasBancarias.value.map((c) => ({ label: c.descricao, value: c.id }))
)

async function carregarContasBancarias() {
  try {
    const resposta = await useApi<{ dados?: { id: number; descricao: string }[] }>('/contas-bancarias', {
      query: { tamanhoPagina: 200 }
    })
    contasBancarias.value = resposta?.dados ?? []
  } catch (e) {
    console.error('[BaixaContaPagarDialog] erro ao carregar contas bancárias', e)
  }
}

const valorRestante = computed(() => {
  if (!props.conta) return 0
  return Math.max(0, props.conta.valorTitulo - (props.conta.valorTotalPago ?? 0))
})

watch(
  () => props.modelValue,
  (aberto) => {
    if (aberto && props.conta) {
      form.value = {
        dataPagamento: new Date().toISOString().slice(0, 10),
        valorPago: valorRestante.value,
        valorDesconto: 0,
        valorJuros: 0,
        valorMulta: 0,
        contaBancariaId: null,
        tipoPagamento: null
      }
      void carregarContasBancarias()
    }
  }
)

function fechar() {
  emit('update:modelValue', false)
}

async function confirmar() {
  if (!props.conta) return
  if (form.value.valorPago <= 0) {
    toast.error('Informe o valor pago')
    return
  }
  salvando.value = true
  try {
    await useApi(`/financeiro/contas-pagar/{id}/baixar`, {
      method: 'POST',
      params: { id: props.conta.id },
      body: {
        dataPagamento: paraIsoData(new Date(form.value.dataPagamento)),
        valorPago: form.value.valorPago,
        valorDesconto: form.value.valorDesconto,
        valorJuros: form.value.valorJuros,
        valorMulta: form.value.valorMulta,
        contaBancariaId: form.value.contaBancariaId,
        tipoPagamento: form.value.tipoPagamento
      }
    })
    toast.success('Pagamento registrado com sucesso')
    emit('confirmado')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
</script>

<template>
  <AppDialog
    :model-value="modelValue"
    title="Registrar Pagamento"
    width="480px"
    persistent
    @update:model-value="fechar"
  >
    <div v-if="conta" class="baixa-form">
      <p class="baixa-info">
        <strong>{{ conta.nomeFornecedor }}</strong> — Doc. {{ conta.documento }}
      </p>
      <p class="baixa-info baixa-restante">Valor restante: {{ valorRestante.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) }}</p>

      <div class="form-grid">
        <DateTimeField v-model="form.dataPagamento" label="Data do Pagamento" required class="col-6" />
        <MoneyInput v-model="form.valorPago" label="Valor Pago" required class="col-6" />
        <MoneyInput v-model="form.valorDesconto" label="Desconto" class="col-4" />
        <MoneyInput v-model="form.valorJuros" label="Juros" class="col-4" />
        <MoneyInput v-model="form.valorMulta" label="Multa" class="col-4" />
        <SelectField
          v-model="form.tipoPagamento"
          label="Forma de Pagamento"
          :options="opcoesTipoPagamento"
          class="col-6"
        />
        <SelectField
          v-model="form.contaBancariaId"
          label="Conta Bancária"
          :options="opcoesContasBancarias"
          class="col-6"
        />
      </div>
    </div>

    <template #footer>
      <button type="button" class="btn btn-secondary" :disabled="salvando" @click="fechar">Cancelar</button>
      <button type="button" class="btn btn-primary" :disabled="salvando" @click="confirmar">
        <span v-if="salvando" class="spinner"></span>
        <span v-else>Confirmar Pagamento</span>
      </button>
    </template>
  </AppDialog>
</template>

<style scoped>
.baixa-form {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.baixa-info {
  color: var(--text-secondary);
  font-size: 13px;
  margin: 0;
}
.baixa-restante {
  color: var(--text-primary);
  font-weight: 600;
}
.form-grid {
  display: grid;
  grid-template-columns: repeat(12, 1fr);
  gap: 12px 16px;
}
.col-4 { grid-column: span 4; }
.col-6 { grid-column: span 6; }
@media (max-width: 480px) {
  .col-4, .col-6 { grid-column: span 12; }
}
</style>
