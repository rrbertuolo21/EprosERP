<script setup lang="ts">
/**
 * NfeReferenciadasDialog — inclusão/edição de chaves de NF-e referenciadas
 * (porta ReferenceDialog do legado, sem Vuetify).
 *
 * Mantém uma lista local de chaves (44 dígitos). Ao confirmar, emite `confirmar` com a lista;
 * a página persiste via `vendas-fiscal/{id}/nfe/referenciadas`.
 */
import { ref, watch } from 'vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import { useMask } from '~/composables/useMask'

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
const chaveAtual = ref('')
const editandoIndex = ref<number | null>(null)

watch(
  () => props.modelValue,
  (aberto) => {
    if (aberto) {
      lista.value = [...props.chaves]
      chaveAtual.value = ''
      editandoIndex.value = null
    }
  }
)

const chaveValida = (v: string) => somenteDigitos(v).length === 44

function inserir() {
  const chave = somenteDigitos(chaveAtual.value)
  if (!chaveValida(chave)) return
  if (editandoIndex.value !== null) {
    lista.value.splice(editandoIndex.value, 1, chave)
    editandoIndex.value = null
  } else {
    lista.value.push(chave)
  }
  chaveAtual.value = ''
}

function editar(index: number) {
  chaveAtual.value = lista.value[index] ?? ''
  editandoIndex.value = index
}

function excluir(index: number) {
  lista.value.splice(index, 1)
  if (editandoIndex.value === index) {
    editandoIndex.value = null
    chaveAtual.value = ''
  }
}

function cancelarEdicao() {
  editandoIndex.value = null
  chaveAtual.value = ''
}

function confirmar() {
  emit('confirmar', [...lista.value])
  emit('update:modelValue', false)
}
</script>

<template>
  <AppDialog
    :model-value="modelValue"
    title="Referenciar Notas Fiscais"
    width="720px"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <div class="ref-form">
      <div class="ref-input">
        <label class="field-label">Chave da Nota Fiscal (44 dígitos)</label>
        <input
          v-model="chaveAtual"
          class="input"
          placeholder="Somente números"
          maxlength="60"
          @keyup.enter="inserir"
        />
      </div>
      <div class="ref-acoes">
        <button v-if="editandoIndex !== null" type="button" class="btn btn-secondary" @click="cancelarEdicao">
          Cancelar
        </button>
        <button type="button" class="btn btn-primary" :disabled="!chaveValida(chaveAtual)" @click="inserir">
          {{ editandoIndex !== null ? 'Editar' : 'Inserir' }}
        </button>
      </div>
    </div>

    <div class="table-wrap ref-tabela">
      <table class="admin-table">
        <thead>
          <tr>
            <th>Chave</th>
            <th class="td-actions">Ações</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="lista.length === 0">
            <td colspan="2" class="table-empty">Nenhuma nota referenciada.</td>
          </tr>
          <tr v-for="(chave, i) in lista" :key="i">
            <td class="ref-chave">{{ chave }}</td>
            <td class="td-actions">
              <button type="button" class="btn btn-ghost btn-sm" @click="editar(i)">✎</button>
              <button type="button" class="btn btn-ghost btn-sm" @click="excluir(i)">🗑</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <template #footer>
      <button type="button" class="btn btn-secondary" @click="emit('update:modelValue', false)">Fechar</button>
      <button type="button" class="btn btn-primary" @click="confirmar">Confirmar</button>
    </template>
  </AppDialog>
</template>

<style scoped>
.ref-form { display: flex; gap: 12px; align-items: end; margin-bottom: 16px; }
.ref-input { flex: 1; }
.ref-acoes { display: flex; gap: 8px; }
.ref-chave { font-family: monospace; font-size: 12px; word-break: break-all; }
.ref-tabela { max-height: 300px; overflow-y: auto; }
</style>
