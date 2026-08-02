<script setup lang="ts">
/**
 * DeleteAlert — diálogo de confirmação de exclusão (v-model), especializado do ConfirmDialog.
 *
 * Contrato:
 *   props:
 *     modelValue: boolean       (v-model)
 *     itemLabel?: string        (nome do item para reforçar a mensagem)
 *     loading?: boolean         (spinner no botão excluir durante a chamada)
 *   emits:
 *     'update:modelValue': [value: boolean]
 *     confirm: []               (usuário confirmou a exclusão)
 *
 * A tela mantém o modal aberto (loading) até a exclusão terminar e então fecha via v-model.
 */
import AppDialog from './AppDialog.vue'

withDefaults(
  defineProps<{
    modelValue: boolean
    itemLabel?: string
    loading?: boolean
  }>(),
  { loading: false }
)

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirm: []
}>()
</script>

<template>
  <AppDialog
    :model-value="modelValue"
    title="Confirmar exclusão"
    width="420px"
    persistent
    @update:model-value="emit('update:modelValue', $event)"
  >
    <p class="delete-msg">
      Tem certeza que deseja excluir
      <strong v-if="itemLabel">{{ itemLabel }}</strong>
      <template v-else>este registro</template>?
      Esta ação não pode ser desfeita.
    </p>
    <template #footer>
      <button type="button" class="btn btn-secondary" :disabled="loading" @click="emit('update:modelValue', false)">
        Cancelar
      </button>
      <button type="button" class="btn btn-danger" :disabled="loading" @click="emit('confirm')">
        <span v-if="loading" class="spinner"></span>
        <span v-else>Excluir</span>
      </button>
    </template>
  </AppDialog>
</template>

<style scoped>
.delete-msg { color: var(--text-secondary); font-size: 14px; line-height: 1.5; }
</style>
