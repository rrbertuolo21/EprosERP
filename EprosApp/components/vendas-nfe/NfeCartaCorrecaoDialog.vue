<script setup lang="ts">
/**
 * NfeCartaCorrecaoDialog — registro de Carta de Correção Eletrônica (CC-e)
 * (porta CartaCorrecaoDialog do legado, sem Vuetify).
 *
 * Exibe número/série/chave da nota (readonly) e coleta a justificativa. Sanitiza o texto
 * (linha única, sem quebras nem caracteres de controle) antes de emitir `registrar`.
 * A chamada à API fica na página (`vendas-fiscal/{id}/nfe/carta-correcao`).
 */
import { ref, computed, watch } from 'vue'
import AppDialog from '~/components/shared/AppDialog.vue'

const props = defineProps<{
  modelValue: boolean
  numero?: string | null
  serie?: string | null
  chave?: string | null
  loading?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  registrar: [texto: string]
}>()

const justificativa = ref('')

watch(
  () => props.modelValue,
  (aberto) => {
    if (!aberto) justificativa.value = ''
  }
)

// Regex de caracteres de controle (U+0000–U+001F e U+007F) montada por código para não
// inserir bytes de controle literais no fonte. A SEFAZ rejeita esses caracteres na CC-e.
const CTRL = new RegExp('[\\u0000-\\u001F\\u007F]+', 'g')

/** Normaliza o texto para uma linha contínua, sem quebras nem caracteres de controle. */
function sanitizar(texto: string): string {
  return texto
    .replace(/[\r\n\t]+/g, ' ')
    .replace(CTRL, '')
    .replace(/\s{2,}/g, ' ')
    .trim()
}

const textoLimpo = computed(() => sanitizar(justificativa.value))
const podeRegistrar = computed(() => textoLimpo.value.length >= 15)

function registrar() {
  if (!podeRegistrar.value) return
  emit('registrar', textoLimpo.value)
}
</script>

<template>
  <AppDialog
    :model-value="modelValue"
    title="Carta de Correção"
    width="820px"
    persistent
    @update:model-value="emit('update:modelValue', $event)"
  >
    <div class="form-grid cce-cab">
      <div class="cce-campo">
        <label class="field-label">Número</label>
        <input class="input" :value="numero ?? ''" readonly />
      </div>
      <div class="cce-campo">
        <label class="field-label">Série</label>
        <input class="input" :value="serie ?? ''" readonly />
      </div>
      <div class="cce-campo cce-chave">
        <label class="field-label">Chave de Acesso</label>
        <input class="input" :value="chave ?? ''" readonly />
      </div>
    </div>

    <div class="cce-just">
      <label class="field-label">Justificativa da Correção</label>
      <textarea
        v-model="justificativa"
        class="input cce-textarea"
        rows="4"
        maxlength="975"
        :disabled="loading"
        placeholder="Descreva a justificativa da correção (mínimo 15 caracteres)..."
        @keydown.enter.prevent
      ></textarea>
      <span class="field-hint">
        Linha única, sem Enter. Ao registrar, o texto é ajustado (trim e remoção de caracteres de
        controle) para coincidir com o envio à SEFAZ. {{ textoLimpo.length }}/975
      </span>
    </div>

    <template #footer>
      <button type="button" class="btn btn-secondary" :disabled="loading" @click="emit('update:modelValue', false)">
        Fechar
      </button>
      <button type="button" class="btn btn-primary" :disabled="!podeRegistrar || loading" @click="registrar">
        <span v-if="loading" class="spinner"></span>
        <span v-else>Registrar Evento</span>
      </button>
    </template>
  </AppDialog>
</template>

<style scoped>
.cce-cab { grid-template-columns: 1fr 1fr 3fr; gap: 12px; margin-bottom: 16px; }
.cce-chave { grid-column: span 1; }
.cce-just { margin-top: 4px; }
.cce-textarea { min-height: 96px; resize: vertical; font-family: inherit; line-height: 1.5; }
@media (max-width: 700px) { .cce-cab { grid-template-columns: 1fr 1fr; } .cce-chave { grid-column: span 2; } }
</style>
