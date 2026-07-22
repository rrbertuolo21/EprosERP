<script setup lang="ts">
/**
 * EmpresaSelector — seletor da empresa ativa (multiempresa), para o header/telas.
 *
 * Sem props obrigatórias. Lê e escreve a empresa ativa via `useTenant`.
 * Emite `change` com o id da empresa selecionada (para telas que queiram reagir).
 *
 * Contrato:
 *   props: { compact?: boolean }
 *   emits: { change: [empresaId: number] }
 */
import { computed } from 'vue'
import { useTenant } from '~/composables/useTenant'
import AppIcon from '~/components/AppIcon.vue'

withDefaults(defineProps<{ compact?: boolean }>(), { compact: false })

const emit = defineEmits<{ change: [empresaId: number] }>()

const { empresas, empresaAtiva, selecionarEmpresaPorId } = useTenant()

const valor = computed({
  get: () => empresaAtiva.value?.id ?? '',
  set: (v: string | number) => {
    const id = Number(v)
    selecionarEmpresaPorId(id)
    emit('change', id)
  }
})
</script>

<template>
  <div v-if="empresas.length > 0" class="empresa-selector" :class="{ compact }">
    <span v-if="!compact" class="es-icon"><AppIcon name="building" :size="14" /></span>
    <select v-model="valor" class="select es-select" aria-label="Empresa ativa">
      <option v-for="e in empresas" :key="e.id" :value="e.id">{{ e.nome }}</option>
    </select>
  </div>
</template>

<style scoped>
.empresa-selector { display: flex; align-items: center; gap: 6px; }
.es-select { width: auto; min-width: 160px; max-width: 240px; padding: 6px 28px 6px 10px; font-size: 12px; }
.empresa-selector.compact .es-select { min-width: 120px; }
.es-icon { display: inline-flex; color: var(--text-secondary); }
</style>
