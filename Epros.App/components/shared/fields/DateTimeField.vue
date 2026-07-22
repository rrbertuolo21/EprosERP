<script setup lang="ts">
/**
 * DateTimeField — campo de data (e opcionalmente hora) usando inputs nativos.
 *
 * Contrato:
 *   props:
 *     modelValue: string | null   (v-model — ISO: 'YYYY-MM-DD' ou 'YYYY-MM-DDTHH:mm')
 *     label?: string
 *     mode?: 'date' | 'datetime'  (default 'date')
 *     required?, disabled?, readonly?: boolean
 *     error?, hint?: string
 *   emits:
 *     'update:modelValue': [value: string | null]
 *
 * Mantém o valor como string ISO para envio direto à API.
 */
import { computed } from 'vue'

const props = withDefaults(
  defineProps<{
    modelValue?: string | null
    label?: string
    mode?: 'date' | 'datetime'
    required?: boolean
    disabled?: boolean
    readonly?: boolean
    error?: string
    hint?: string
  }>(),
  { mode: 'date' }
)

const emit = defineEmits<{
  'update:modelValue': [value: string | null]
}>()

const idCampo = `dt-${Math.random().toString(36).slice(2, 9)}`

const tipo = computed(() => (props.mode === 'datetime' ? 'datetime-local' : 'date'))

const valor = computed({
  get: () => props.modelValue ?? '',
  set: (v: string) => emit('update:modelValue', v || null)
})
</script>

<template>
  <div class="field">
    <label v-if="label" :for="idCampo" class="field-label">
      {{ label }}<span v-if="required" class="required">*</span>
    </label>
    <input
      :id="idCampo"
      v-model="valor"
      class="input"
      :class="{ 'is-invalid': !!error }"
      :type="tipo"
      :disabled="disabled"
      :readonly="readonly"
      :required="required"
    />
    <span v-if="error" class="field-error">{{ error }}</span>
    <span v-else-if="hint" class="field-hint">{{ hint }}</span>
  </div>
</template>
