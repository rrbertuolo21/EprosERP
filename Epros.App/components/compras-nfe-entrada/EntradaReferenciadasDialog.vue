<script setup lang="ts">
/**
 * EntradaReferenciadasDialog — gestão das chaves de NF-e referenciadas.
 *
 * Porta o `ReferenceDialog` do legado. Usado na NF-e de entrada de devolução/retorno para
 * referenciar a(s) nota(s) de origem (chave de 44 dígitos). Também disponível na entrada normal.
 *
 * Recebe a lista atual via prop `chaves` e emite `confirmar` com a lista editada.
 */
import { ref, watch } from 'vue'
import { useMask } from '~/composables/useMask'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'

const props = defineProps<{
  modelValue: boolean
  chaves: string[]
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirmar: [chaves: string[]]
}>()

const { somenteDigitos } = useMask()

const lista = ref<string[]>([])
const nova = ref('')
const erro = ref('')

watch(
  () => props.modelValue,
  (aberto) => {
    if (aberto) {
      lista.value = [...props.chaves]
      nova.value = ''
      erro.value = ''
    }
  }
)

function adicionar() {
  const chave = somenteDigitos(nova.value)
  if (chave.length !== 44) {
    erro.value = 'A chave de acesso deve ter 44 dígitos'
    return
  }
  if (lista.value.includes(chave)) {
    erro.value = 'Esta chave já foi referenciada'
    return
  }
  lista.value.push(chave)
  nova.value = ''
  erro.value = ''
}

function remover(idx: number) {
  lista.value.splice(idx, 1)
}

function confirmar() {
  emit('confirmar', [...lista.value])
  emit('update:modelValue', false)
}
</script>

<template>
  <AppDialog
    :model-value="modelValue"
    title="NF-e referenciadas"
    width="620px"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <div class="ref-input">
      <TextField
        v-model="nova"
        label="Chave de acesso da NF-e de origem (44 dígitos)"
        placeholder="Cole ou digite a chave de acesso"
        :maxlength="54"
        @update:model-value="erro = ''"
      />
      <button type="button" class="btn btn-secondary btn-sm" @click="adicionar">Adicionar</button>
    </div>
    <span v-if="erro" class="field-error">{{ erro }}</span>

    <ul v-if="lista.length" class="ref-lista">
      <li v-for="(chave, idx) in lista" :key="chave">
        <span class="ref-chave">{{ chave }}</span>
        <button type="button" class="btn btn-ghost btn-sm" title="Remover" @click="remover(idx)">🗑</button>
      </li>
    </ul>
    <p v-else class="ref-vazio">Nenhuma NF-e referenciada.</p>

    <template #footer>
      <button type="button" class="btn btn-secondary" @click="emit('update:modelValue', false)">Cancelar</button>
      <button type="button" class="btn btn-primary" @click="confirmar">Confirmar</button>
    </template>
  </AppDialog>
</template>

<style scoped>
.ref-input { display: flex; align-items: flex-end; gap: 10px; }
.ref-input :deep(.field) { flex: 1; }
.ref-lista { list-style: none; margin-top: 14px; display: flex; flex-direction: column; gap: 6px; }
.ref-lista li { display: flex; align-items: center; justify-content: space-between; gap: 10px; padding: 8px 12px; background: rgba(255,255,255,0.03); border-radius: 8px; }
.ref-chave { font-family: monospace; font-size: 12.5px; word-break: break-all; }
.ref-vazio { font-size: 13px; color: var(--text-muted); margin-top: 12px; }
</style>
