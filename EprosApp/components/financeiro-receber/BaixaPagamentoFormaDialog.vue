<script setup lang="ts">
/**
 * BaixaPagamentoFormaDialog — inclui/edita uma forma de recebimento (item) da baixa de pagamento.
 *
 * Porta o comportamento de `components/financeiro/BaixaPagamentoFormaDialog.vue` do legado:
 * informa valor a receber, desconto, juros/multa/acréscimo e calcula o valor pago.
 *
 * Contrato:
 *   props: modelValue (v-model), item?: ContasAReceberItem
 *   emits: 'update:modelValue', save: [item: ContasAReceberItem]
 */
import { reactive, computed, watch } from 'vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import type { SelectOption } from '~/composables/useEnum'
import type { ContasAReceberItem } from './types'

const props = defineProps<{
  modelValue: boolean
  item?: ContasAReceberItem
  opcoesTipoPagamento?: SelectOption[]
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  save: [item: ContasAReceberItem]
}>()

const form = reactive<ContasAReceberItem>({
  dataRecebimento: null,
  tipoPagamento: null,
  valorDesconto: 0,
  valorJuros: 0,
  valorMulta: 0,
  valorAcrescimo: 0,
  valorParcela: 0,
  valorPago: 0
})

watch(
  () => [props.modelValue, props.item] as const,
  ([aberto, item]) => {
    if (aberto) {
      Object.assign(form, {
        dataRecebimento: null,
        tipoPagamento: null,
        valorDesconto: 0,
        valorJuros: 0,
        valorMulta: 0,
        valorAcrescimo: 0,
        valorParcela: 0,
        valorPago: 0,
        ...item
      })
    }
  },
  { immediate: true }
)

const valorLiquido = computed(() => {
  return (form.valorPago ?? 0) - (form.valorDesconto ?? 0) + (form.valorJuros ?? 0) + (form.valorMulta ?? 0) + (form.valorAcrescimo ?? 0)
})

watch(valorLiquido, (v) => {
  form.valorParcela = v
})

function fechar() {
  emit('update:modelValue', false)
}

function salvar() {
  form.valorParcela = valorLiquido.value
  emit('save', { ...form })
  fechar()
}
</script>

<template>
  <AppDialog :model-value="modelValue" title="Forma de Recebimento" width="520px" @update:model-value="emit('update:modelValue', $event)">
    <div class="forma-grid">
      <DateTimeField v-model="form.dataRecebimento" label="Data do Recebimento" required />
      <SelectField v-model="form.tipoPagamento" label="Forma de Recebimento" :options="opcoesTipoPagamento ?? []" required />
      <MoneyInput v-model="form.valorPago" label="Valor Recebido" required />
      <MoneyInput v-model="form.valorDesconto" label="Desconto" />
      <MoneyInput v-model="form.valorJuros" label="Juros" />
      <MoneyInput v-model="form.valorMulta" label="Multa" />
      <MoneyInput v-model="form.valorAcrescimo" label="Acréscimo" />
      <MoneyInput :model-value="valorLiquido" label="Valor Líquido" readonly />
    </div>
    <template #footer>
      <button type="button" class="btn btn-secondary" @click="fechar">Cancelar</button>
      <button type="button" class="btn btn-primary" @click="salvar">Salvar</button>
    </template>
  </AppDialog>
</template>

<style scoped>
.forma-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 14px;
}
</style>
