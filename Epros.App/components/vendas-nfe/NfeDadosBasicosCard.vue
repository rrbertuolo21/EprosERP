<script setup lang="ts">
/**
 * NfeDadosBasicosCard — dados de cabeçalho da NF-e (porta NfeDadosBasicosCard do legado).
 *
 * Campos: natureza da operação, tipo de operação fiscal (CFOP padrão), datas de emissão e
 * saída. Recebe o formulário reativo por v-model e as listas de apoio (tipos de operação).
 */
import { computed } from 'vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import type { SelectOption } from '~/composables/useEnum'
import type { NfeForm } from './nfeTypes'

const props = defineProps<{
  modelValue: NfeForm
  tiposOperacaoOpcoes: SelectOption[]
  erros?: Record<string, string>
}>()

const emit = defineEmits<{
  'update:modelValue': [value: NfeForm]
}>()

const form = computed(() => props.modelValue)

function definir<K extends keyof NfeForm>(chave: K, valor: NfeForm[K]) {
  emit('update:modelValue', { ...props.modelValue, [chave]: valor })
}

/** Datas ISO no formato aceito pelo input datetime-local (YYYY-MM-DDTHH:mm). */
function paraDatetimeLocal(iso: string): string {
  if (!iso) return ''
  return iso.length >= 16 ? iso.slice(0, 16) : iso
}
</script>

<template>
  <section class="glass-panel nfe-card">
    <header class="nfe-card-header">
      <h2 class="nfe-card-title">Dados da Nota</h2>
    </header>

    <div class="form-grid nfe-grid">
      <div class="col-natureza">
        <TextField
          :model-value="form.naturezaOperacao"
          label="Natureza da Operação"
          placeholder="Ex.: Venda de mercadoria"
          required
          :error="erros?.naturezaOperacao"
          @update:model-value="definir('naturezaOperacao', $event)"
        />
      </div>

      <SelectField
        :model-value="form.tipoOperacaoFiscalId"
        label="Tipo de Operação Fiscal"
        :options="tiposOperacaoOpcoes"
        @update:model-value="definir('tipoOperacaoFiscalId', $event as number | null)"
      />

      <DateTimeField
        :model-value="paraDatetimeLocal(form.dataEmissao)"
        label="Data de Emissão"
        mode="datetime"
        required
        :error="erros?.dataEmissao"
        @update:model-value="definir('dataEmissao', ($event ?? '') as string)"
      />

      <DateTimeField
        :model-value="paraDatetimeLocal(form.dataHoraSaida)"
        label="Data/Hora de Saída"
        mode="datetime"
        @update:model-value="definir('dataHoraSaida', ($event ?? '') as string)"
      />
    </div>
  </section>
</template>

<style scoped>
.nfe-card { padding: 18px 20px; margin-bottom: 16px; }
.nfe-card-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; }
.nfe-card-title { font-size: 14px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--text-secondary); }
.nfe-grid { grid-template-columns: repeat(4, 1fr); gap: 14px; }
.col-natureza { grid-column: span 2; }
@media (max-width: 900px) {
  .nfe-grid { grid-template-columns: 1fr 1fr; }
  .col-natureza { grid-column: span 2; }
}
</style>
