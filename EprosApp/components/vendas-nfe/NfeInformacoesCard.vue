<script setup lang="ts">
/**
 * NfeInformacoesCard — informações complementares e adicionais de interesse do fisco,
 * mais os atalhos para referenciar notas (porta NfeInformacoesComplementaresCard do legado).
 *
 * As áreas de texto entram no v-model do formulário. Os botões apenas emitem eventos que a
 * página trata (abrir diálogo de notas referenciadas).
 */
import { computed } from 'vue'
import type { NfeForm } from './nfeTypes'

const props = defineProps<{
  modelValue: NfeForm
  readonly?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: NfeForm]
  'abrir-referenciadas': []
}>()

const form = computed(() => props.modelValue)

function definir<K extends keyof NfeForm>(chave: K, valor: NfeForm[K]) {
  emit('update:modelValue', { ...props.modelValue, [chave]: valor })
}
</script>

<template>
  <section class="glass-panel nfe-card">
    <header class="nfe-card-header">
      <h2 class="nfe-card-title">Informações Complementares</h2>
      <button type="button" class="btn btn-secondary btn-sm" @click="emit('abrir-referenciadas')">
        Notas referenciadas
        <span v-if="form.chavesReferenciadas.length" class="badge-ref">{{ form.chavesReferenciadas.length }}</span>
      </button>
    </header>

    <div class="info-campo">
      <label class="field-label">Informações complementares (contribuinte)</label>
      <textarea
        class="input info-textarea"
        rows="3"
        :readonly="readonly"
        :value="form.informacoesComplementares"
        placeholder="Observações que aparecem na DANFE..."
        @input="definir('informacoesComplementares', ($event.target as HTMLTextAreaElement).value)"
      ></textarea>
    </div>

    <div class="info-campo">
      <label class="field-label">Informações adicionais de interesse do fisco</label>
      <textarea
        class="input info-textarea"
        rows="2"
        :readonly="readonly"
        :value="form.informacoesAdicionaisFisco"
        @input="definir('informacoesAdicionaisFisco', ($event.target as HTMLTextAreaElement).value)"
      ></textarea>
    </div>
  </section>
</template>

<style scoped>
.nfe-card { padding: 18px 20px; margin-bottom: 16px; }
.nfe-card-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; }
.nfe-card-title { font-size: 14px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--text-secondary); }
.info-campo { margin-bottom: 14px; }
.info-textarea { min-height: 60px; resize: vertical; font-family: inherit; line-height: 1.5; }
.badge-ref {
  display: inline-flex; align-items: center; justify-content: center;
  min-width: 18px; height: 18px; padding: 0 5px; margin-left: 6px;
  border-radius: 9px; background: var(--primary); color: #fff; font-size: 11px; font-weight: 700;
}
</style>
