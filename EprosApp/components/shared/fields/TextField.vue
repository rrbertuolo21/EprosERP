<script setup lang="ts">
/**
 * TextField — campo de texto padrão.
 *
 * Contrato:
 *   props:
 *     modelValue: string | number | null   (v-model)
 *     label?: string
 *     placeholder?: string
 *     type?: 'text' | 'email' | 'password' | 'number' | 'tel'  (default 'text')
 *     required?: boolean
 *     disabled?: boolean
 *     readonly?: boolean
 *     error?: string        (mensagem de erro; aplica estilo inválido)
 *     hint?: string
 *     prefix?: string       (afixo à esquerda)
 *     suffix?: string       (afixo à direita)
 *     maxlength?: number | string   (repassado ao atributo HTML; aceita literal estático)
 *   emits:
 *     'update:modelValue': [value: string]
 *     blur / focus: [ev: FocusEvent]
 */
import { computed } from 'vue'

const props = withDefaults(
  defineProps<{
    modelValue?: string | number | null
    label?: string
    placeholder?: string
    type?: 'text' | 'email' | 'password' | 'number' | 'tel'
    required?: boolean
    disabled?: boolean
    readonly?: boolean
    error?: string
    hint?: string
    prefix?: string
    suffix?: string
    maxlength?: number | string
  }>(),
  { type: 'text' }
)

const emit = defineEmits<{
  'update:modelValue': [value: string]
  blur: [ev: FocusEvent]
  focus: [ev: FocusEvent]
}>()

const valor = computed({
  get: () => (props.modelValue ?? '') as string | number,
  set: (v: string) => emit('update:modelValue', v)
})

const idCampo = `tf-${Math.random().toString(36).slice(2, 9)}`
</script>

<template>
  <div class="field">
    <label v-if="label" :for="idCampo" class="field-label">
      {{ label }}<span v-if="required" class="required">*</span>
    </label>
    <div class="input-affix" :class="{ 'has-prefix': !!prefix, 'has-suffix': !!suffix }">
      <span v-if="prefix" class="prefix">{{ prefix }}</span>
      <input
        :id="idCampo"
        v-model="valor"
        class="input"
        :class="{ 'is-invalid': !!error }"
        :type="type"
        :placeholder="placeholder"
        :disabled="disabled"
        :readonly="readonly"
        :required="required"
        :maxlength="maxlength"
        @blur="emit('blur', $event)"
        @focus="emit('focus', $event)"
      />
      <span v-if="suffix" class="suffix">{{ suffix }}</span>
    </div>
    <span v-if="error" class="field-error">{{ error }}</span>
    <span v-else-if="hint" class="field-hint">{{ hint }}</span>
  </div>
</template>
