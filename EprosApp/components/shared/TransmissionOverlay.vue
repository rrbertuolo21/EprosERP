<script setup lang="ts">
/**
 * TransmissionOverlay — overlay de progresso de transmissão de NF-e/NFC-e.
 *
 * Contrato:
 *   props:
 *     modelValue: boolean           (v-model — visível)
 *     title?: string                (default 'Transmitindo Documento Fiscal')
 *     message?: string
 *     steps: TransmissionStep[]      (etapas do progresso)
 *     currentStep?: number          (índice controlado externamente; se ausente, auto-avança)
 *     error?: string | null         (mensagem de erro; muda o overlay para estado de falha)
 *   emits:
 *     'update:modelValue': [value: boolean]
 *     close: []
 *
 * TransmissionStep: { text: string; icon?: string }
 *
 * A tela liga isto ao useRealtime: cada evento do hub avança `currentStep`; ao concluir/erro,
 * mostra o resultado e permite fechar.
 */
import { ref, computed, watch, onBeforeUnmount } from 'vue'

export interface TransmissionStep {
  text: string
  icon?: string
}

const props = withDefaults(
  defineProps<{
    modelValue: boolean
    title?: string
    message?: string
    steps: TransmissionStep[]
    currentStep?: number
    error?: string | null
  }>(),
  { title: 'Transmitindo Documento Fiscal', error: null }
)

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  close: []
}>()

const autoIndex = ref(0)
let timer: ReturnType<typeof setInterval> | null = null

const indiceAtual = computed(() => props.currentStep ?? autoIndex.value)

function iniciarAuto() {
  pararAuto()
  if (props.currentStep === undefined) {
    timer = setInterval(() => {
      if (autoIndex.value < props.steps.length - 1) autoIndex.value++
    }, 2000)
  }
}

function pararAuto() {
  if (timer) {
    clearInterval(timer)
    timer = null
  }
}

watch(
  () => props.modelValue,
  (aberto) => {
    if (aberto) {
      autoIndex.value = 0
      iniciarAuto()
    } else {
      pararAuto()
    }
  },
  { immediate: true }
)

watch(() => props.error, (e) => { if (e) pararAuto() })

onBeforeUnmount(pararAuto)

function fechar() {
  emit('update:modelValue', false)
  emit('close')
}
</script>

<template>
  <Teleport to="body">
    <Transition name="overlay-fade">
      <div v-if="modelValue" class="overlay-backdrop transmission-overlay">
        <div class="modal glass-panel transmission-card">
          <template v-if="error">
            <div class="tr-icon tr-icon-error">✕</div>
            <h3 class="tr-title">Falha na transmissão</h3>
            <p class="tr-error">{{ error }}</p>
            <button type="button" class="btn btn-secondary" @click="fechar">Fechar</button>
          </template>

          <template v-else>
            <div class="tr-spinner"><span class="spinner spinner-lg"></span></div>
            <h3 class="tr-title">{{ title }}</h3>
            <p v-if="message" class="tr-message">{{ message }}</p>

            <ul class="tr-steps">
              <li
                v-for="(step, i) in steps"
                :key="i"
                class="tr-step"
                :class="{ done: i < indiceAtual, active: i === indiceAtual }"
              >
                <span class="tr-step-marker">
                  <template v-if="i < indiceAtual">✓</template>
                  <template v-else-if="i === indiceAtual" class="spinner"></template>
                  <template v-else>{{ i + 1 }}</template>
                </span>
                <span class="tr-step-text">{{ step.text }}</span>
              </li>
            </ul>
          </template>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.transmission-overlay { z-index: 9999; }
.transmission-card {
  max-width: 440px;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}
.tr-spinner { margin: 8px 0; }
.spinner-lg { width: 40px; height: 40px; border-width: 3px; border-top-color: var(--primary); }
.tr-title { font-size: 18px; font-weight: 700; }
.tr-message { font-size: 13px; color: var(--text-secondary); }
.tr-error { font-size: 13px; color: var(--danger); line-height: 1.5; }
.tr-icon {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 26px;
  font-weight: 700;
}
.tr-icon-error { background: var(--danger-glow); color: var(--danger); border: 1px solid rgba(239, 68, 68, 0.3); }
.tr-steps { list-style: none; width: 100%; text-align: left; display: flex; flex-direction: column; gap: 10px; margin-top: 8px; }
.tr-step { display: flex; align-items: center; gap: 10px; font-size: 13px; color: var(--text-muted); }
.tr-step.active { color: var(--text-primary); font-weight: 600; }
.tr-step.done { color: var(--success); }
.tr-step-marker {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  border: 1px solid var(--border-color);
  flex-shrink: 0;
}
.tr-step.done .tr-step-marker { background: var(--success-glow); border-color: rgba(16, 185, 129, 0.4); }
.tr-step.active .tr-step-marker { border-color: var(--primary); }
.overlay-fade-enter-active,
.overlay-fade-leave-active { transition: opacity 0.2s ease; }
.overlay-fade-enter-from,
.overlay-fade-leave-to { opacity: 0; }
</style>
