<script setup lang="ts">
/**
 * ConfirmDialog — confirmação imperativa (substitui useSwal/ConfirmDialog do legado).
 *
 * Uso:
 *   <ConfirmDialog ref="confirmRef" />
 *   const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()
 *   const ok = await confirmRef.value!.open('Excluir?', 'Esta ação é irreversível.', { danger: true })
 *   if (ok) { ... }
 *
 * Método exposto:
 *   open(titulo, mensagem?, opcoes?): Promise<boolean>
 *     opcoes: { textoConfirmar?, textoCancelar?, danger?: boolean }
 * Sem props/emits — todo o controle é via `open()`.
 */
import { ref } from 'vue'
import AppDialog from './AppDialog.vue'

interface OpcoesConfirmacao {
  textoConfirmar?: string
  textoCancelar?: string
  danger?: boolean
}

const visivel = ref(false)
const titulo = ref('')
const mensagem = ref('')
const textoConfirmar = ref('Confirmar')
const textoCancelar = ref('Cancelar')
const danger = ref(false)

let resolver: ((v: boolean) => void) | null = null

function open(t: string, m = '', opcoes: OpcoesConfirmacao = {}): Promise<boolean> {
  titulo.value = t
  mensagem.value = m
  textoConfirmar.value = opcoes.textoConfirmar ?? 'Confirmar'
  textoCancelar.value = opcoes.textoCancelar ?? 'Cancelar'
  danger.value = opcoes.danger ?? false
  visivel.value = true
  return new Promise<boolean>((resolve) => {
    resolver = resolve
  })
}

function responder(valor: boolean) {
  visivel.value = false
  resolver?.(valor)
  resolver = null
}

defineExpose({ open })
</script>

<template>
  <AppDialog v-model="visivel" :title="titulo" width="420px" persistent>
    <p class="confirm-msg">{{ mensagem }}</p>
    <template #footer>
      <button type="button" class="btn btn-secondary" @click="responder(false)">{{ textoCancelar }}</button>
      <button type="button" class="btn" :class="danger ? 'btn-danger' : 'btn-primary'" @click="responder(true)">
        {{ textoConfirmar }}
      </button>
    </template>
  </AppDialog>
</template>

<style scoped>
.confirm-msg { color: var(--text-secondary); font-size: 14px; line-height: 1.5; }
</style>
