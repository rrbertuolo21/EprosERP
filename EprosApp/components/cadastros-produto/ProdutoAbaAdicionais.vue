<script setup lang="ts">
/**
 * Aba "Adicionais" do formulário de Produto — vincula/desvincula itens da lista de
 * adicionais (endpoint `adicionais`) ao produto. Porta o comportamento da aba "grade"
 * de `ProdutoItemForm.vue` do legado, substituindo o drag-and-drop (lib externa, não
 * permitida aqui) por botões "Adicionar"/"Remover" — mesmo resultado funcional.
 */
import { computed } from 'vue'
import type { ProdutoAdicionalVinculo } from './produtoTypes'

interface AdicionalDisponivel {
  id: number
  descricao: string
}

const props = defineProps<{
  modelValue: ProdutoAdicionalVinculo[]
  disponiveis: AdicionalDisponivel[]
  produtoDescricao?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: ProdutoAdicionalVinculo[]]
}>()

const vinculadosIds = computed(() => new Set(props.modelValue.map((v) => v.adicionaisId)))

const naoVinculados = computed(() =>
  props.disponiveis.filter((a) => !vinculadosIds.value.has(a.id))
)

function vincular(adicional: AdicionalDisponivel) {
  emit('update:modelValue', [
    ...props.modelValue,
    { adicionaisId: adicional.id, descricao: adicional.descricao }
  ])
}

function desvincular(adicionaisId: number) {
  emit('update:modelValue', props.modelValue.filter((v) => v.adicionaisId !== adicionaisId))
}

const tituloGrade = computed(() => (props.produtoDescricao ? `${props.produtoDescricao} com:` : 'Adicionais do produto'))
</script>

<template>
  <div class="adicionais-grid">
    <div class="glass-panel adicionais-col">
      <header class="adicionais-header">Adicionais disponíveis</header>
      <ul class="adicionais-lista">
        <li v-if="naoVinculados.length === 0" class="adicionais-vazio">Nenhum adicional disponível</li>
        <li v-for="item in naoVinculados" :key="item.id" class="adicionais-item">
          <span>{{ item.descricao }}</span>
          <button type="button" class="btn btn-ghost btn-sm" @click="vincular(item)">Adicionar →</button>
        </li>
      </ul>
    </div>

    <div class="glass-panel adicionais-col">
      <header class="adicionais-header">{{ tituloGrade }}</header>
      <ul class="adicionais-lista">
        <li v-if="modelValue.length === 0" class="adicionais-vazio">Nenhum adicional vinculado</li>
        <li v-for="item in modelValue" :key="item.adicionaisId" class="adicionais-item">
          <span>{{ item.descricao }}</span>
          <button type="button" class="btn btn-ghost btn-sm" @click="desvincular(item.adicionaisId)">← Remover</button>
        </li>
      </ul>
    </div>
  </div>
</template>

<style scoped>
.adicionais-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}
@media (max-width: 900px) {
  .adicionais-grid { grid-template-columns: 1fr; }
}
.adicionais-col { padding: 16px; }
.adicionais-header {
  font-size: 13px;
  font-weight: 700;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.3px;
  margin-bottom: 12px;
}
.adicionais-lista { list-style: none; display: flex; flex-direction: column; gap: 8px; }
.adicionais-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.03);
  font-size: 13px;
}
.adicionais-vazio { color: var(--text-muted); font-size: 13px; padding: 8px 0; }
</style>
