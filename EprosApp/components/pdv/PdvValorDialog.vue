<script setup lang="ts">
/**
 * PdvValorDialog — modal simples para digitar um valor monetário (pagamento, desconto
 * ou acréscimo). Substitui o `VDialog` de valor do `components/pos/pagamento.vue` do legado.
 *
 * Controlado por v-model; ao abrir, foca o campo. Enter confirma.
 */
import { nextTick, ref, watch } from 'vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'

const props = defineProps<{
  modelValue: boolean
  titulo: string
  valorInicial?: number
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirmar: [valor: number]
}>()

const valor = ref(props.valorInicial ?? 0)
const moneyRef = ref<InstanceType<typeof MoneyInput> | null>(null)

watch(
  () => props.modelValue,
  (aberto) => {
    if (aberto) {
      valor.value = props.valorInicial ?? 0
      nextTick(() => {
        const input = moneyRef.value?.$el?.querySelector('input') as HTMLInputElement | undefined
        input?.focus()
        input?.select()
      })
    }
  }
)

function confirmar() {
  emit('confirmar', valor.value)
  emit('update:modelValue', false)
}
</script>

<template>
  <AppDialog
    :model-value="modelValue"
    :title="titulo"
    width="400px"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <MoneyInput
      ref="moneyRef"
      v-model="valor"
      label="Valor"
      @keydown.enter.prevent="confirmar"
    />
    <template #footer>
      <button type="button" class="btn btn-secondary" @click="emit('update:modelValue', false)">Cancelar</button>
      <button type="button" class="btn btn-success" @click="confirmar">Adicionar</button>
    </template>
  </AppDialog>
</template>
