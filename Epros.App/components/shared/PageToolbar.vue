<script setup lang="ts">
/**
 * PageToolbar — cabeçalho de página (título + subtítulo + ações).
 *
 * Contrato:
 *   props:
 *     title: string
 *     subtitle?: string
 *     loading?: boolean       (mostra barra de progresso fina abaixo)
 *   slots:
 *     actions -> botões à direita (ex.: "Novo", "Exportar")
 *     default -> conteúdo extra abaixo do título (opcional)
 */
withDefaults(
  defineProps<{
    title: string
    subtitle?: string
    loading?: boolean
  }>(),
  { loading: false }
)
</script>

<template>
  <div>
    <div class="page-toolbar">
      <div class="toolbar-title">
        <h1>{{ title }}</h1>
        <span v-if="subtitle" class="subtitle">{{ subtitle }}</span>
      </div>
      <div class="toolbar-actions">
        <slot name="actions" />
      </div>
    </div>
    <div v-if="loading" class="toolbar-progress"><span class="bar"></span></div>
    <slot />
  </div>
</template>

<style scoped>
.toolbar-progress {
  height: 2px;
  background: rgba(255, 255, 255, 0.06);
  border-radius: 2px;
  overflow: hidden;
  margin-bottom: 16px;
}
.toolbar-progress .bar {
  display: block;
  height: 100%;
  width: 40%;
  background: var(--primary);
  animation: indeterminate 1.1s ease-in-out infinite;
}
@keyframes indeterminate {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(320%); }
}
</style>
