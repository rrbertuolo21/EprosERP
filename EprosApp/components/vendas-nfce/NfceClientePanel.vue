<script setup lang="ts">
/**
 * NfceClientePanel — dados do consumidor (destinatário) da NFC-e.
 *
 * Porta o comportamento de `components/pos/cliente.vue` do legado: CPF/CNPJ do consumidor
 * (opcional na NFC-e), opção de identificar o destinatário e busca de cliente cadastrado.
 * O documento é validado via useDocumento; consumidor não identificado é permitido.
 */
import { computed } from 'vue'
import TextField from '~/components/shared/fields/TextField.vue'
import { useMask } from '~/composables/useMask'
import { useDocumento } from '~/composables/useDocumento'
import type { NfceDestinatario } from './types'

const props = defineProps<{
  modelValue: NfceDestinatario
  somenteLeitura?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: NfceDestinatario]
}>()

const { maskCpfCnpj, somenteDigitos } = useMask()
const { validarCpfCnpj } = useDocumento()

function atualizar(parcial: Partial<NfceDestinatario>) {
  emit('update:modelValue', { ...props.modelValue, ...parcial })
}

const documentoMascarado = computed(() => maskCpfCnpj(props.modelValue.documentoConsumidor || ''))

/** Mensagem de validação do documento do consumidor (vazio é permitido na NFC-e). */
const erroDocumento = computed(() => {
  const doc = somenteDigitos(props.modelValue.documentoConsumidor)
  if (!doc) return ''
  if (doc.length !== 11 && doc.length !== 14) return 'Informe um CPF (11) ou CNPJ (14) completo.'
  if (!validarCpfCnpj(doc)) return 'CPF/CNPJ inválido.'
  return ''
})

function aoDigitarDocumento(valor: string) {
  atualizar({ documentoConsumidor: somenteDigitos(valor) })
}
</script>

<template>
  <div class="cliente-panel glass-panel">
    <div class="panel-header">
      <span class="panel-titulo">Consumidor</span>
      <span class="panel-hint">Opcional — deixe em branco para consumidor não identificado.</span>
    </div>

    <div class="form-grid">
      <div class="field col-6">
        <label class="field-label">CPF/CNPJ do consumidor</label>
        <input
          class="input"
          :class="{ 'is-invalid': !!erroDocumento }"
          :value="documentoMascarado"
          placeholder="000.000.000-00"
          :disabled="somenteLeitura"
          inputmode="numeric"
          @input="aoDigitarDocumento(($event.target as HTMLInputElement).value)"
        />
        <span v-if="erroDocumento" class="field-error">{{ erroDocumento }}</span>
      </div>

      <TextField
        :model-value="modelValue.descricao"
        label="Nome do consumidor"
        class="col-6"
        :disabled="somenteLeitura"
        @update:model-value="atualizar({ descricao: $event })"
      />

      <label class="filter-checkbox col-12">
        <input
          type="checkbox"
          :checked="modelValue.enviarDestinatarioNaNfce"
          :disabled="somenteLeitura"
          @change="atualizar({ enviarDestinatarioNaNfce: ($event.target as HTMLInputElement).checked })"
        />
        <span>Identificar destinatário na NFC-e</span>
      </label>
    </div>
  </div>
</template>

<style scoped>
.cliente-panel { padding: 12px; }
.panel-header { display: flex; flex-direction: column; gap: 2px; margin-bottom: 10px; }
.panel-titulo { font-weight: 600; font-size: 14px; }
.panel-hint { font-size: 12px; color: var(--text-muted); }
.form-grid { display: grid; grid-template-columns: repeat(12, 1fr); gap: 12px 16px; }
.col-6 { grid-column: span 6; }
.col-12 { grid-column: span 12; }
@media (max-width: 720px) {
  .col-6 { grid-column: span 12; }
}
</style>
