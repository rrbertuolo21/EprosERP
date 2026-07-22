<script setup lang="ts">
/**
 * SelectField — campo de seleção (dropdown) padronizado.
 *
 * Contrato:
 *   props:
 *     modelValue: string | number | null    (v-model)
 *     options: { label: string; value: string | number }[]
 *     label?: string
 *     placeholder?: string   (opção vazia inicial; default 'Selecione...')
 *     required?: boolean
 *     disabled?: boolean
 *     error?: string
 *     hint?: string
 *     clearable?: boolean    (mostra opção vazia)  default true
 *   emits:
 *     'update:modelValue': [value: string | number | null]
 *     change: [value: string | number | null]
 *
 * Alimente `options` com `useEnum().paraOpcoes(...)` para selects de domínio.
 */
import { computed } from 'vue'
import type { SelectOption } from '~/composables/useEnum'

const props = withDefaults(
  defineProps<{
    modelValue?: string | number | null
    options: SelectOption[]
    label?: string
    placeholder?: string
    required?: boolean
    disabled?: boolean
    error?: string
    hint?: string
    clearable?: boolean
  }>(),
  { placeholder: 'Selecione...', clearable: true }
)

const emit = defineEmits<{
  'update:modelValue': [value: string | number | null]
  change: [value: string | number | null]
}>()

const idCampo = `sf-${Math.random().toString(36).slice(2, 9)}`

const valor = computed({
  get: () => (props.modelValue ?? '') as string | number,
  set: (v: string) => {
    // Preserva o tipo original da option (number vs string).
    const encontrada = props.options.find((o) => String(o.value) === String(v))
    const resolvido = v === '' ? null : encontrada ? encontrada.value : v
    emit('update:modelValue', resolvido)
    emit('change', resolvido)
  }
})
</script>

<template>
  <div class="field">
    <label v-if="label" :for="idCampo" class="field-label">
      {{ label }}<span v-if="required" class="required">*</span>
    </label>
    <select
      :id="idCampo"
      v-model="valor"
      class="select"
      :class="{ 'is-invalid': !!error }"
      :disabled="disabled"
      :required="required"
    >
      <option v-if="clearable" value="">{{ placeholder }}</option>
      <option v-for="opt in options" :key="String(opt.value)" :value="opt.value">
        {{ opt.label }}
      </option>
    </select>
    <span v-if="error" class="field-error">{{ error }}</span>
    <span v-else-if="hint" class="field-hint">{{ hint }}</span>
  </div>
</template>
