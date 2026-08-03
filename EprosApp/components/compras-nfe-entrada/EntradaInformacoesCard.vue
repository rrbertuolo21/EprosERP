<script setup lang="ts">
/**
 * EntradaInformacoesCard — informações complementares / adicionais ao fisco.
 *
 * Porta o `NfeInformacoesComplementaresCard` do legado. Em modo devolução/retorno mostra também o
 * botão para abrir o diálogo de NF-e referenciadas (obrigatório na devolução).
 */
import type { EntradaForm } from './tipos'

const props = defineProps<{
  modelValue: EntradaForm
  readonly?: boolean
  /** Mostra o acesso às NF-e referenciadas (usado na devolução/retorno). */
  mostrarReferenciadas?: boolean
  erroReferenciadas?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: EntradaForm]
  'abrir-referenciadas': []
}>()

function set<K extends keyof EntradaForm>(chave: K, valor: EntradaForm[K]) {
  props.modelValue[chave] = valor
  emit('update:modelValue', props.modelValue)
}
</script>

<template>
  <section class="glass-panel nfe-card">
    <header class="nfe-card-header">
      <h2 class="nfe-card-title">Informações complementares</h2>
      <button
        v-if="mostrarReferenciadas"
        type="button"
        class="btn btn-ghost btn-sm"
        @click="emit('abrir-referenciadas')"
      >
        NF-e referenciadas
        <span v-if="modelValue.chavesReferenciadas.length" class="chip chip-info">
          {{ modelValue.chavesReferenciadas.length }}
        </span>
      </button>
    </header>

    <span v-if="erroReferenciadas" class="field-error erro-topo">{{ erroReferenciadas }}</span>

    <div class="form-grid">
      <div class="field">
        <label class="field-label">Informações complementares</label>
        <textarea
          class="input"
          rows="3"
          :value="modelValue.informacoesComplementares"
          :disabled="readonly"
          placeholder="Informações complementares de interesse do contribuinte"
          @input="set('informacoesComplementares', ($event.target as HTMLTextAreaElement).value)"
        ></textarea>
      </div>
      <div class="field">
        <label class="field-label">Informações adicionais ao fisco</label>
        <textarea
          class="input"
          rows="3"
          :value="modelValue.informacoesAdicionaisFisco"
          :disabled="readonly"
          placeholder="Informações adicionais de interesse do fisco"
          @input="set('informacoesAdicionaisFisco', ($event.target as HTMLTextAreaElement).value)"
        ></textarea>
      </div>
    </div>
  </section>
</template>

<style scoped>
.nfe-card { padding: 18px 20px; margin-bottom: 16px; }
.nfe-card-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; }
.nfe-card-title { font-size: 14px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--text-secondary); }
.erro-topo { display: block; margin-bottom: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 12px 16px; }
textarea.input { resize: vertical; min-height: 72px; font-family: inherit; }
@media (max-width: 760px) {
  .form-grid { grid-template-columns: 1fr; }
}
</style>
