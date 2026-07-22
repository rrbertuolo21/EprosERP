<script setup lang="ts">
/**
 * DevolucaoRetornoCard — cartão específico da devolução/retorno de NF-e.
 *
 * Reúne o que difere da emissão normal: a finalidade da NF-e (fixada em Devolução/Retorno)
 * e a lista de chaves das NF-e de origem (as notas que estão sendo devolvidas), que se tornam
 * as notas referenciadas do documento.
 *
 * Contrato:
 *   props:
 *     finalidade: number
 *     finalidades: { label: string; value: number }[]
 *     chavesOrigem: string[]
 *     readonly?: boolean
 *   emits:
 *     adicionar-chave: [chave: string]
 *     remover-chave: [index: number]
 *
 * A finalidade é somente-leitura nesta tela (a rota já define o tipo de operação).
 */
import { ref } from 'vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import type { SelectOption } from '~/composables/useEnum'

const props = withDefaults(
  defineProps<{
    finalidade: number
    finalidades: { label: string; value: number }[]
    chavesOrigem: string[]
    readonly?: boolean
  }>(),
  { readonly: false }
)

const emit = defineEmits<{
  'adicionar-chave': [chave: string]
  'remover-chave': [index: number]
}>()

const chaveAtual = ref('')

const opcoesFinalidade = (): SelectOption[] =>
  props.finalidades.map((f) => ({ label: f.label, value: f.value }))

function adicionar() {
  const v = chaveAtual.value.trim()
  if (!v) return
  emit('adicionar-chave', v)
  chaveAtual.value = ''
}
</script>

<template>
  <section class="glass-panel card devolucao-card">
    <header class="card-header">
      <h2 class="card-title">Devolução / Retorno</h2>
    </header>

    <div class="card-body">
      <div class="grid-2">
        <SelectField
          :model-value="finalidade"
          label="Finalidade da NF-e"
          :options="opcoesFinalidade()"
          :clearable="false"
          disabled
          hint="Definida automaticamente para operações de devolução/retorno."
        />
      </div>

      <div class="origem-notas">
        <label class="field-label">Notas fiscais de origem (chave de 44 dígitos)</label>
        <div class="origem-input">
          <input
            v-model="chaveAtual"
            class="input"
            placeholder="Somente números"
            maxlength="60"
            :disabled="readonly"
            @keyup.enter="adicionar"
          />
          <button
            type="button"
            class="btn btn-primary"
            :disabled="readonly || !chaveAtual.trim()"
            @click="adicionar"
          >
            Adicionar
          </button>
        </div>

        <div class="table-wrap origem-tabela">
          <table class="admin-table">
            <thead>
              <tr>
                <th>Chave da NF-e de origem</th>
                <th class="td-actions">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="chavesOrigem.length === 0">
                <td colspan="2" class="table-empty">Nenhuma nota de origem informada.</td>
              </tr>
              <tr v-for="(chave, i) in chavesOrigem" :key="i">
                <td class="origem-chave">{{ chave }}</td>
                <td class="td-actions">
                  <button
                    type="button"
                    class="btn btn-ghost btn-sm"
                    :disabled="readonly"
                    @click="emit('remover-chave', i)"
                  >
                    🗑
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.card { padding: 16px 18px; margin-bottom: 16px; }
.card-header { margin-bottom: 12px; }
.card-title { font-size: 15px; font-weight: 700; }
.grid-2 { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 12px; }
.origem-notas { margin-top: 16px; }
.origem-input { display: flex; gap: 10px; align-items: center; margin: 8px 0 12px; }
.origem-input .input { flex: 1; }
.origem-chave { font-family: monospace; font-size: 12px; word-break: break-all; }
.origem-tabela { max-height: 240px; overflow-y: auto; }
@media (max-width: 720px) {
  .grid-2 { grid-template-columns: 1fr; }
}
</style>
