<script setup lang="ts">
/**
 * DanfeViewer — visualizador de DANFE/PDF (substitui pdfobject-vue do legado).
 *
 * Contrato:
 *   props:
 *     src?: string | Blob | null    (URL, dataURI ou Blob do PDF)
 *     title?: string                (default 'DANFE')
 *     height?: string               (altura do visualizador; default '80vh')
 *   emits:
 *     download: []                  (usuário pediu download)
 *     print: []                     (usuário pediu impressão)
 *
 * Aceita tanto uma URL quanto um Blob (gera object URL internamente e o revoga ao trocar/desmontar).
 * Renderiza via <iframe>, sem dependências externas.
 */
import { computed, watch, onBeforeUnmount, ref } from 'vue'

const props = withDefaults(
  defineProps<{
    src?: string | Blob | null
    title?: string
    height?: string
  }>(),
  { title: 'DANFE', height: '80vh' }
)

const emit = defineEmits<{
  download: []
  print: []
}>()

const objectUrl = ref<string | null>(null)

const url = computed<string | null>(() => {
  if (!props.src) return null
  if (typeof props.src === 'string') return props.src
  return objectUrl.value
})

watch(
  () => props.src,
  (novo) => {
    revogar()
    if (novo instanceof Blob) {
      objectUrl.value = URL.createObjectURL(novo)
    }
  },
  { immediate: true }
)

function revogar() {
  if (objectUrl.value) {
    URL.revokeObjectURL(objectUrl.value)
    objectUrl.value = null
  }
}

onBeforeUnmount(revogar)
</script>

<template>
  <div class="danfe-viewer glass-panel">
    <div class="danfe-header">
      <span class="danfe-title">{{ title }}</span>
      <div class="danfe-actions">
        <button type="button" class="btn btn-ghost btn-sm" @click="emit('print')">Imprimir</button>
        <button type="button" class="btn btn-secondary btn-sm" @click="emit('download')">Baixar</button>
      </div>
    </div>
    <div class="danfe-body" :style="{ height }">
      <iframe v-if="url" :src="url" :title="title" class="danfe-frame"></iframe>
      <div v-else class="danfe-empty">Nenhum documento carregado.</div>
    </div>
  </div>
</template>

<style scoped>
.danfe-viewer { padding: 12px; display: flex; flex-direction: column; gap: 10px; }
.danfe-header { display: flex; align-items: center; justify-content: space-between; }
.danfe-title { font-weight: 600; font-size: 14px; }
.danfe-actions { display: flex; gap: 8px; }
.danfe-body {
  width: 100%;
  background: #1a1a1f;
  border-radius: 8px;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}
.danfe-frame { width: 100%; height: 100%; border: none; background: #fff; }
.danfe-empty { color: var(--text-muted); font-size: 13px; }
</style>
