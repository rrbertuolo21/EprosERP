<script setup lang="ts">
/**
 * MoneyInput — campo monetário (BRL). Emite SEMPRE um número (não string formatada).
 *
 * Contrato:
 *   props:
 *     modelValue: number | null       (v-model — valor numérico, ex.: 1234.56)
 *     label?: string
 *     required?: boolean
 *     disabled?: boolean
 *     readonly?: boolean
 *     error?: string
 *     hint?: string
 *     min?: number   (default 0)
 *     symbol?: boolean  (mostra prefixo R$; default true)
 *   emits:
 *     'update:modelValue': [value: number | null]
 *
 * Digitação estilo "calculadora": os dígitos preenchem os centavos da direita para a esquerda.
 */
import { computed } from 'vue'

const props = withDefaults(
  defineProps<{
    modelValue?: number | null
    label?: string
    required?: boolean
    disabled?: boolean
    readonly?: boolean
    error?: string
    hint?: string
    min?: number
    symbol?: boolean
  }>(),
  { min: 0, symbol: true }
)

const emit = defineEmits<{
  'update:modelValue': [value: number | null]
}>()

const idCampo = `mi-${Math.random().toString(36).slice(2, 9)}`

const exibicao = computed(() => {
  const v = props.modelValue ?? 0
  return new Intl.NumberFormat('pt-BR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(v)
})

function aoDigitar(ev: Event) {
  const bruto = (ev.target as HTMLInputElement).value
  const digitos = bruto.replace(/\D/g, '')
  let numero = digitos ? Number(digitos) / 100 : 0
  if (props.min != null && numero < props.min) numero = props.min
  emit('update:modelValue', numero)
}
</script>

<template>
  <div class="field">
    <label v-if="label" :for="idCampo" class="field-label">
      {{ label }}<span v-if="required" class="required">*</span>
    </label>
    <div class="input-affix" :class="{ 'has-prefix': symbol }">
      <span v-if="symbol" class="prefix">R$</span>
      <input
        :id="idCampo"
        class="input input-text-right"
        :class="{ 'is-invalid': !!error }"
        inputmode="decimal"
        :value="exibicao"
        :disabled="disabled"
        :readonly="readonly"
        :required="required"
        @input="aoDigitar"
      />
    </div>
    <span v-if="error" class="field-error">{{ error }}</span>
    <span v-else-if="hint" class="field-hint">{{ hint }}</span>
  </div>
</template>
