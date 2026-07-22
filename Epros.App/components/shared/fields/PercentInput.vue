<script setup lang="ts">
/**
 * PercentInput — campo de porcentagem (0–100, `decimais` casas, default 2). Emite número.
 * O valor é em pontos percentuais (ex.: 12.5 => "12,50 %").
 *
 * Contrato:
 *   props:
 *     modelValue: number | null   (v-model)
 *     label?: string
 *     required?, disabled?, readonly?: boolean
 *     error?, hint?: string
 *     decimais?: number   (default 2)
 *     max?: number        (teto; default 100)
 *   emits:
 *     'update:modelValue': [value: number | null]
 */
import { ref, watch } from 'vue'

const props = withDefaults(
  defineProps<{
    modelValue?: number | null
    label?: string
    required?: boolean
    disabled?: boolean
    readonly?: boolean
    error?: string
    hint?: string
    decimais?: number
    max?: number
  }>(),
  { decimais: 2, max: 100 }
)

const emit = defineEmits<{
  'update:modelValue': [value: number | null]
}>()

const idCampo = `pi-${Math.random().toString(36).slice(2, 9)}`
const texto = ref(formatar(props.modelValue))

watch(
  () => props.modelValue,
  (v) => {
    const atual = paraNumero(texto.value)
    if (v !== atual) texto.value = formatar(v)
  }
)

function formatar(v: number | null | undefined): string {
  if (v == null) return ''
  return new Intl.NumberFormat('pt-BR', {
    minimumFractionDigits: 0,
    maximumFractionDigits: props.decimais
  }).format(v)
}

function paraNumero(t: string): number {
  const limpo = t.replace(/\./g, '').replace(',', '.').replace(/[^\d.]/g, '')
  const n = Number(limpo)
  return isNaN(n) ? 0 : n
}

function aoDigitar(ev: Event) {
  texto.value = (ev.target as HTMLInputElement).value
  let n = paraNumero(texto.value)
  if (n < 0) n = 0
  if (props.max != null && n > props.max) n = props.max
  emit('update:modelValue', n)
}
</script>

<template>
  <div class="field">
    <label v-if="label" :for="idCampo" class="field-label">
      {{ label }}<span v-if="required" class="required">*</span>
    </label>
    <div class="input-affix has-suffix">
      <input
        :id="idCampo"
        class="input input-text-right"
        :class="{ 'is-invalid': !!error }"
        inputmode="decimal"
        :value="texto"
        :disabled="disabled"
        :readonly="readonly"
        :required="required"
        @input="aoDigitar"
      />
      <span class="suffix">%</span>
    </div>
    <span v-if="error" class="field-error">{{ error }}</span>
    <span v-else-if="hint" class="field-hint">{{ hint }}</span>
  </div>
</template>
