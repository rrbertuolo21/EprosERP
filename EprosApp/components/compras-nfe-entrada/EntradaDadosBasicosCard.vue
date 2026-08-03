<script setup lang="ts">
/**
 * EntradaDadosBasicosCard — dados básicos da NF-e de entrada.
 *
 * Porta o `NfeDadosBasicosCard` do legado para o contexto de compras: número/série/chave da nota
 * do fornecedor, natureza da operação, datas (emissão/entrada) e modalidade de frete.
 *
 * v-model recebe o objeto `EntradaForm` (mutado diretamente por ser reactive na página/composable).
 */
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import { MODALIDADES_FRETE, type EntradaForm } from './tipos'

const props = defineProps<{
  modelValue: EntradaForm
  erros?: Record<string, string>
  readonly?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: EntradaForm]
}>()

/** Atualiza um campo do formulário mantendo a reatividade da página. */
function set<K extends keyof EntradaForm>(chave: K, valor: EntradaForm[K]) {
  props.modelValue[chave] = valor
  emit('update:modelValue', props.modelValue)
}
</script>

<template>
  <section class="glass-panel nfe-card">
    <header class="nfe-card-header">
      <h2 class="nfe-card-title">Dados da nota</h2>
    </header>

    <div class="form-grid">
      <TextField
        :model-value="modelValue.numero"
        label="Número da nota"
        placeholder="Nº da NF-e do fornecedor"
        required
        :disabled="readonly"
        :error="erros?.numero"
        @update:model-value="set('numero', $event)"
      />
      <TextField
        :model-value="modelValue.serie"
        label="Série"
        placeholder="Série"
        :disabled="readonly"
        @update:model-value="set('serie', $event)"
      />
      <TextField
        class="col-span-2"
        :model-value="modelValue.chave"
        label="Chave de acesso (44 dígitos)"
        placeholder="Chave de acesso da NF-e"
        required
        :disabled="readonly"
        :maxlength="54"
        :error="erros?.chave"
        @update:model-value="set('chave', $event)"
      />
      <TextField
        class="col-span-2"
        :model-value="modelValue.naturezaOperacao"
        label="Natureza da operação"
        placeholder="Ex.: Compra para comercialização"
        :disabled="readonly"
        @update:model-value="set('naturezaOperacao', $event)"
      />
      <DateTimeField
        :model-value="modelValue.dataEmissao"
        label="Data de emissão"
        mode="datetime"
        required
        :disabled="readonly"
        :error="erros?.dataEmissao"
        @update:model-value="set('dataEmissao', $event ?? '')"
      />
      <DateTimeField
        :model-value="modelValue.dataEntrada"
        label="Data de entrada"
        mode="datetime"
        :disabled="readonly"
        @update:model-value="set('dataEntrada', $event ?? '')"
      />
      <SelectField
        class="col-span-2"
        :model-value="modelValue.modalidadeFrete"
        label="Modalidade do frete"
        :options="MODALIDADES_FRETE"
        :clearable="false"
        :disabled="readonly"
        @update:model-value="set('modalidadeFrete', Number($event ?? 9))"
      />
    </div>
  </section>
</template>

<style scoped>
.nfe-card { padding: 18px 20px; margin-bottom: 16px; }
.nfe-card-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; }
.nfe-card-title { font-size: 14px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--text-secondary); }
.form-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px 14px; }
.col-span-2 { grid-column: span 2; }
@media (max-width: 900px) {
  .form-grid { grid-template-columns: repeat(2, 1fr); }
  .col-span-2 { grid-column: span 2; }
}
</style>
