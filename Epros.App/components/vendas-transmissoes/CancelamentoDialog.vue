<script setup lang="ts">
/**
 * CancelamentoDialog — captura a justificativa de cancelamento de um documento fiscal
 * (porta CancelSaleDialog do legado, sem Vuetify).
 *
 * Contrato:
 *   props:
 *     modelValue: boolean            (v-model — visível)
 *     documento?: string | null      (número/identificação exibida no título)
 *     loading?: boolean
 *   emits:
 *     'update:modelValue': [value: boolean]
 *     confirmar: [justificativa: string]
 *
 * A SEFAZ exige justificativa de no mínimo 15 caracteres para cancelamento.
 */
import { ref, watch, computed } from 'vue'
import AppDialog from '~/components/shared/AppDialog.vue'

const JUSTIFICATIVA_MIN = 15

const props = withDefaults(
  defineProps<{
    modelValue: boolean
    documento?: string | null
    loading?: boolean
  }>(),
  { documento: null, loading: false }
)

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirmar: [justificativa: string]
}>()

const justificativa = ref('')

const valida = computed(() => justificativa.value.trim().length >= JUSTIFICATIVA_MIN)

watch(
  () => props.modelValue,
  (aberto) => {
    if (aberto) justificativa.value = ''
  }
)

function confirmar() {
  if (!valida.value) return
  emit('confirmar', justificativa.value.trim())
}
</script>

<template>
  <AppDialog
    :model-value="modelValue"
    title="Cancelar documento fiscal"
    width="520px"
    persistent
    @update:model-value="emit('update:modelValue', $event)"
  >
    <p v-if="documento" class="cancel-info">
      Documento: <strong>{{ documento }}</strong>
    </p>
    <div class="field">
      <label class="field-label">
        Justificativa (mínimo {{ JUSTIFICATIVA_MIN }} caracteres)<span class="required">*</span>
      </label>
      <textarea
        v-model="justificativa"
        class="input"
        rows="3"
        maxlength="255"
        placeholder="Descreva o motivo do cancelamento"
      ></textarea>
      <span class="field-hint">{{ justificativa.trim().length }} / 255</span>
    </div>

    <template #footer>
      <button type="button" class="btn btn-secondary" :disabled="loading" @click="emit('update:modelValue', false)">
        Voltar
      </button>
      <button type="button" class="btn btn-danger" :disabled="!valida || loading" @click="confirmar">
        <span v-if="loading" class="spinner"></span>
        <span v-else>Confirmar cancelamento</span>
      </button>
    </template>
  </AppDialog>
</template>

<style scoped>
.cancel-info { color: var(--text-secondary); font-size: 13px; margin-bottom: 12px; }
textarea.input { resize: vertical; min-height: 72px; font-family: inherit; }
</style>
