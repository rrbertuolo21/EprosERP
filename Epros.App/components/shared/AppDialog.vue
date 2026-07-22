<script setup lang="ts">
/**
 * AppDialog — modal base (substitui v-dialog). Controlado por v-model.
 *
 * Contrato:
 *   props:
 *     modelValue: boolean      (v-model — visível?)
 *     title?: string
 *     width?: string           (ex.: '560px'; default '560px')
 *     persistent?: boolean     (não fecha ao clicar no backdrop)
 *   emits:
 *     'update:modelValue': [value: boolean]
 *   slots:
 *     default  -> corpo do modal
 *     footer   -> rodapé (ações). Se ausente, nada é renderizado no rodapé.
 *     title    -> título customizado (sobrepõe a prop title)
 */
import { watch, onBeforeUnmount } from 'vue'

const props = withDefaults(
  defineProps<{
    modelValue: boolean
    title?: string
    width?: string
    persistent?: boolean
  }>(),
  { width: '560px' }
)

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

function fechar() {
  if (!props.persistent) emit('update:modelValue', false)
}

function aoTeclar(ev: KeyboardEvent) {
  if (ev.key === 'Escape') fechar()
}

watch(
  () => props.modelValue,
  (aberto) => {
    if (import.meta.client) {
      document.body.style.overflow = aberto ? 'hidden' : ''
      if (aberto) window.addEventListener('keydown', aoTeclar)
      else window.removeEventListener('keydown', aoTeclar)
    }
  }
)

onBeforeUnmount(() => {
  if (import.meta.client) {
    document.body.style.overflow = ''
    window.removeEventListener('keydown', aoTeclar)
  }
})
</script>

<template>
  <Teleport to="body">
    <Transition name="dialog-fade">
      <div v-if="modelValue" class="overlay-backdrop" @click.self="fechar">
        <div class="modal glass-panel" :style="{ maxWidth: width }" role="dialog" aria-modal="true">
          <div class="modal-header">
            <h3 class="modal-title">
              <slot name="title">{{ title }}</slot>
            </h3>
            <button type="button" class="modal-close" aria-label="Fechar" @click="emit('update:modelValue', false)">×</button>
          </div>
          <div class="modal-body">
            <slot />
          </div>
          <div v-if="$slots.footer" class="modal-footer">
            <slot name="footer" />
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.dialog-fade-enter-active,
.dialog-fade-leave-active { transition: opacity 0.2s ease; }
.dialog-fade-enter-from,
.dialog-fade-leave-to { opacity: 0; }
</style>
