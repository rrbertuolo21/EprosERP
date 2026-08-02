<script setup lang="ts">
/**
 * Aba "Combustível" do formulário de Produto — dados específicos de combustíveis
 * (produto-especifico) e grade de origens por UF. Porta o comportamento da aba
 * "combustivel" de `ProdutoItemForm.vue` do legado.
 */
import { computed, reactive } from 'vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import { useToast } from '~/composables/useToast'
import type { SelectOption } from '~/composables/useEnum'
import type { ProdutoCombustivelOrigem, ProdutoEspecificoForm } from './produtoTypes'

const props = defineProps<{
  modelValue: ProdutoEspecificoForm
  ufsOpcoes: SelectOption[]
  origensCombustivelOpcoes: SelectOption[]
}>()

const emit = defineEmits<{
  'update:modelValue': [value: ProdutoEspecificoForm]
}>()

const toast = useToast()

function atualizarCampo<K extends keyof ProdutoEspecificoForm>(campo: K, valor: ProdutoEspecificoForm[K]) {
  emit('update:modelValue', { ...props.modelValue, [campo]: valor })
}

const novaOrigem = reactive<ProdutoCombustivelOrigem>({
  identificadorOrigemImportacao: null,
  ufOrigem: '',
  valorPercentualUf: 0
})

function descricaoOrigem(id: number | null): string {
  return props.origensCombustivelOpcoes.find((o) => o.value === id)?.label ?? ''
}

function adicionarOrigem() {
  if (!novaOrigem.ufOrigem || !novaOrigem.valorPercentualUf || novaOrigem.identificadorOrigemImportacao === null) {
    toast.error('Preencha todos os campos de origem antes de adicionar')
    return
  }
  const origens = [...props.modelValue.origens, { ...novaOrigem }]
  emit('update:modelValue', { ...props.modelValue, origens })
  novaOrigem.identificadorOrigemImportacao = null
  novaOrigem.ufOrigem = ''
  novaOrigem.valorPercentualUf = 0
}

function removerOrigem(index: number) {
  const origens = props.modelValue.origens.filter((_, i) => i !== index)
  emit('update:modelValue', { ...props.modelValue, origens })
}

const temOrigens = computed(() => props.modelValue.origens.length > 0)
</script>

<template>
  <div>
    <div class="form-grid">
      <div class="col-2">
        <PercentInput
          :model-value="modelValue.valorPercentualGlpDerivadoPetroleo"
          label="% GLP derivado do petróleo"
          @update:model-value="(v) => atualizarCampo('valorPercentualGlpDerivadoPetroleo', v ?? 0)"
        />
      </div>
      <div class="col-2">
        <PercentInput
          :model-value="modelValue.valorPercentualGasNaturalImportado"
          label="% gás natural importado"
          @update:model-value="(v) => atualizarCampo('valorPercentualGasNaturalImportado', v ?? 0)"
        />
      </div>
      <div class="col-2">
        <PercentInput
          :model-value="modelValue.valorPercentualGasNaturalNacional"
          label="% gás natural nacional"
          @update:model-value="(v) => atualizarCampo('valorPercentualGasNaturalNacional', v ?? 0)"
        />
      </div>
      <div class="col-3">
        <MoneyInput
          :model-value="modelValue.valorPartida"
          label="Valor de partida"
          @update:model-value="(v) => atualizarCampo('valorPartida', v ?? 0)"
        />
      </div>
      <div class="col-3">
        <SelectField
          :model-value="modelValue.ufConsumo"
          label="UF de consumo"
          :options="ufsOpcoes"
          @update:model-value="(v) => atualizarCampo('ufConsumo', String(v ?? ''))"
        />
      </div>
    </div>

    <h3 class="secao-titulo">Origem do combustível</h3>
    <div class="form-grid">
      <div class="col-4">
        <SelectField
          v-model="novaOrigem.identificadorOrigemImportacao"
          label="Indicador de importação"
          :options="origensCombustivelOpcoes"
        />
      </div>
      <div class="col-4">
        <SelectField v-model="novaOrigem.ufOrigem" label="UF de origem" :options="ufsOpcoes" />
      </div>
      <div class="col-3">
        <PercentInput v-model="novaOrigem.valorPercentualUf" label="Percentual originária para a UF" />
      </div>
      <div class="col-1">
        <label class="field-label">&nbsp;</label>
        <button type="button" class="btn btn-primary" style="width:100%;" @click="adicionarOrigem">+</button>
      </div>
    </div>

    <div class="glass-panel" style="margin-top: 16px; padding: 8px 12px;">
      <table class="admin-table">
        <thead>
          <tr>
            <th>Origem</th>
            <th>UF</th>
            <th class="td-right">Percentual</th>
            <th class="td-actions">Ação</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="!temOrigens">
            <td colspan="4" class="table-empty">Nenhuma origem cadastrada</td>
          </tr>
          <tr v-for="(origem, index) in modelValue.origens" v-else :key="index">
            <td>{{ descricaoOrigem(origem.identificadorOrigemImportacao) }}</td>
            <td>{{ origem.ufOrigem }}</td>
            <td class="td-right">{{ origem.valorPercentualUf }}%</td>
            <td class="td-actions">
              <button type="button" class="btn btn-ghost btn-sm" title="Remover" @click="removerOrigem(index)">🗑</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.secao-titulo {
  font-size: 14px;
  font-weight: 700;
  color: var(--text-secondary);
  margin: 24px 0 12px;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}
</style>
