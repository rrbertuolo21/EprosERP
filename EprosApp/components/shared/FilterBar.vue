<script setup lang="ts" generic="F extends object">
/**
 * FilterBar — barra de filtros das listagens.
 *
 * Contrato:
 *   props:
 *     fields: FilterField[]                         (descrição declarativa dos filtros)
 *     modelValue: F                                 (v-model — objeto de filtros tipado)
 *     loading?: boolean
 *   emits:
 *     'update:modelValue': [value: F]
 *     search: [value: F]                            (clicou em Buscar / Enter)
 *     clear: []                                     (clicou em Limpar)
 *
 * FilterField:
 *   { key, label, type: 'text'|'select'|'date'|'boolean', options?, placeholder?, grow? }
 *
 * Genérico em F para aceitar filtros tipados por interface (ex.: ContadorFiltros)
 * sem exigir index signature. Integra com useApiList: no `search`, a tela chama
 * `lista.aplicarFiltros(valores)`.
 */
import { computed } from 'vue'
import type { SelectOption } from '~/composables/useEnum'

export interface FilterField {
  key: string
  label: string
  type: 'text' | 'select' | 'date' | 'boolean'
  options?: SelectOption[]
  placeholder?: string
  /** Ocupa mais espaço (campo de busca principal). */
  grow?: boolean
}

const props = defineProps<{
  fields: FilterField[]
  modelValue: F
  loading?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: F]
  search: [value: F]
  clear: []
}>()

/** Acesso indexado interno: F é um objeto de filtros; tratamos como record. */
const valores = computed(() => props.modelValue as Record<string, unknown>)

function atualizar(key: string, valor: unknown) {
  emit('update:modelValue', { ...props.modelValue, [key]: valor } as F)
}

function buscar() {
  emit('search', props.modelValue)
}

function limpar() {
  emit('clear')
}
</script>

<template>
  <div class="filter-bar glass-panel">
    <div
      v-for="campo in fields"
      :key="campo.key"
      class="filter-item"
      :class="{ grow: campo.grow }"
    >
      <label class="field-label">{{ campo.label }}</label>

      <input
        v-if="campo.type === 'text'"
        class="input"
        :placeholder="campo.placeholder"
        :value="(valores[campo.key] as string) ?? ''"
        @input="atualizar(campo.key, ($event.target as HTMLInputElement).value)"
        @keyup.enter="buscar"
      />

      <select
        v-else-if="campo.type === 'select'"
        class="select"
        :value="(valores[campo.key] as string) ?? ''"
        @change="atualizar(campo.key, ($event.target as HTMLSelectElement).value || null)"
      >
        <option value="">Todos</option>
        <option v-for="opt in campo.options ?? []" :key="String(opt.value)" :value="opt.value">
          {{ opt.label }}
        </option>
      </select>

      <input
        v-else-if="campo.type === 'date'"
        type="date"
        class="input"
        :value="(valores[campo.key] as string) ?? ''"
        @input="atualizar(campo.key, ($event.target as HTMLInputElement).value || null)"
      />

      <label v-else-if="campo.type === 'boolean'" class="filter-checkbox">
        <input
          type="checkbox"
          :checked="!!valores[campo.key]"
          @change="atualizar(campo.key, ($event.target as HTMLInputElement).checked)"
        />
        <span>{{ campo.placeholder ?? 'Sim' }}</span>
      </label>
    </div>

    <div class="filter-actions">
      <button type="button" class="btn btn-primary btn-sm" :disabled="loading" @click="buscar">Buscar</button>
      <button type="button" class="btn btn-secondary btn-sm" :disabled="loading" @click="limpar">Limpar</button>
    </div>
  </div>
</template>

<style scoped>
.filter-checkbox { display: flex; align-items: center; gap: 8px; font-size: 13px; color: var(--text-secondary); padding: 8px 0; }
.filter-checkbox input { accent-color: var(--primary); width: 16px; height: 16px; }
</style>
