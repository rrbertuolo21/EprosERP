<script setup lang="ts">
/**
 * QuantityInput — campo de quantidade (até `decimais` casas, default 3). Emite número.
 *
 * Contrato:
 *   props:
 *     modelValue: number | null    (v-model)
 *     label?: string
 *     required?, disabled?, readonly?: boolean
 *     error?, hint?: string
 *     decimais?: number   (casas decimais; default 3)
 *     min?: number        (default 0)
 *     suffix?: string     (ex.: unidade 'UN', 'KG')
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
    min?: number
    suffix?: string
  }>(),
  { decimais: 3, min: 0 }
)

const emit = defineEmits<{
  'update:modelValue': [value: number | null]
}>()

const idCampo = `qi-${Math.random().toString(36).slice(2, 9)}`

// Mantém o texto local para permitir digitação de vírgula/ponto sem "brigar" com o número.
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
  if (props.min != null && n < props.min) n = props.min
  emit('update:modelValue', n)
}
</script>

<template>
  <div class="field">
    <label v-if="label" :for="idCampo" class="field-label">
      {{ label }}<span v-if="required" class="required">*</span>
    </label>
    <div class="input-affix" :class="{ 'has-suffix': !!suffix }">
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
      <span v-if="suffix" class="suffix">{{ suffix }}</span>
    </div>
    <span v-if="error" class="field-error">{{ error }}</span>
    <span v-else-if="hint" class="field-hint">{{ hint }}</span>
  </div>
</template>
