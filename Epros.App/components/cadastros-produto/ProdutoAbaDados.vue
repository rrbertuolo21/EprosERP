<script setup lang="ts">
/**
 * Aba "Dados" do formulário de Produto — dados gerais, preços, classificação fiscal (NCM/CEST)
 * e balança. Porta o comportamento da aba "dados" de `ProdutoItemForm.vue` do legado.
 *
 * Todo o estado do produto é do componente pai (`v-model`); este componente é "burro":
 * recebe o objeto produto e as listas de apoio, emite atualizações via v-model.
 */
import { computed } from 'vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import type { SelectOption } from '~/composables/useEnum'
import type { ProdutoForm } from './produtoTypes'

const props = defineProps<{
  modelValue: ProdutoForm
  marcasOpcoes: SelectOption[]
  categoriasOpcoes: SelectOption[]
  unidadesOpcoes: SelectOption[]
  ncmOpcoes: SelectOption[]
  cestOpcoes: SelectOption[]
  balancasOpcoes: SelectOption[]
  buscandoNcm?: boolean
  buscandoCest?: boolean
  erros?: Record<string, string>
}>()

const emit = defineEmits<{
  'update:modelValue': [value: ProdutoForm]
  'buscar-ncm': [texto: string]
  'buscar-cest': [texto: string]
}>()

const produto = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v)
})

function atualizarCampo<K extends keyof ProdutoForm>(campo: K, valor: ProdutoForm[K]) {
  emit('update:modelValue', { ...props.modelValue, [campo]: valor })
}
</script>

<template>
  <div class="form-grid">
    <div class="col-3">
      <TextField
        :model-value="produto.codigo"
        label="Código"
        hint="Preenchimento automático"
        placeholder="Código do produto"
        @update:model-value="(v) => atualizarCampo('codigo', v as string)"
      />
    </div>
    <div class="col-3">
      <TextField
        :model-value="produto.ean"
        label="EAN"
        hint="Se deixado em branco, será salvo como 'SEM GTIN'"
        placeholder="Código de barras"
        :error="erros?.ean"
        @update:model-value="(v) => atualizarCampo('ean', v as string)"
      />
    </div>
    <div class="col-6">
      <TextField
        :model-value="produto.descricao"
        label="Descrição"
        required
        placeholder="Descrição do produto"
        :error="erros?.descricao"
        @update:model-value="(v) => atualizarCampo('descricao', v as string)"
      />
    </div>

    <div class="col-4">
      <SelectField
        :model-value="produto.marcaProdutoId"
        label="Marca"
        :options="marcasOpcoes"
        @update:model-value="(v) => atualizarCampo('marcaProdutoId', v as number | null)"
      />
    </div>
    <div class="col-4">
      <SelectField
        :model-value="produto.categoriaId"
        label="Categoria"
        :options="categoriasOpcoes"
        @update:model-value="(v) => atualizarCampo('categoriaId', v as number | null)"
      />
    </div>
    <div class="col-4">
      <SelectField
        :model-value="produto.unidadeMedidaComercialId"
        label="Unidade de medida comercial"
        required
        :options="unidadesOpcoes"
        :error="erros?.unidadeMedidaComercialId"
        @update:model-value="(v) => atualizarCampo('unidadeMedidaComercialId', v as number | null)"
      />
    </div>

    <div class="col-3">
      <MoneyInput
        :model-value="produto.valorCompra"
        label="Valor compra"
        required
        :error="erros?.valorCompra"
        @update:model-value="(v) => atualizarCampo('valorCompra', v ?? 0)"
      />
    </div>
    <div class="col-3">
      <MoneyInput
        :model-value="produto.valorVenda"
        label="Valor venda"
        @update:model-value="(v) => atualizarCampo('valorVenda', v ?? 0)"
      />
    </div>
    <div class="col-3">
      <MoneyInput
        :model-value="produto.valorVendaPrazo"
        label="Valor venda a prazo"
        @update:model-value="(v) => atualizarCampo('valorVendaPrazo', v ?? 0)"
      />
    </div>

    <div class="col-4">
      <label class="field-label">NCM<span class="required">*</span></label>
      <input
        class="input"
        :class="{ 'is-invalid': !!erros?.ncmId }"
        list="lista-ncm"
        placeholder="Digite código ou descrição do NCM"
        :value="produto.ncmDescricao"
        @input="emit('buscar-ncm', ($event.target as HTMLInputElement).value)"
      />
      <datalist id="lista-ncm">
        <option v-for="opt in ncmOpcoes" :key="String(opt.value)" :value="opt.label" :data-id="opt.value" />
      </datalist>
      <select
        class="select"
        style="margin-top: 6px;"
        :value="produto.ncmId ?? ''"
        @change="atualizarCampo('ncmId', ($event.target as HTMLSelectElement).value ? Number(($event.target as HTMLSelectElement).value) : null)"
      >
        <option value="">{{ buscandoNcm ? 'Buscando...' : 'Selecione o NCM encontrado' }}</option>
        <option v-for="opt in ncmOpcoes" :key="String(opt.value)" :value="opt.value">{{ opt.label }}</option>
      </select>
      <span v-if="erros?.ncmId" class="field-error">{{ erros.ncmId }}</span>
    </div>
    <div class="col-4">
      <SelectField
        :model-value="produto.cestId"
        label="CEST"
        :options="cestOpcoes"
        hint="Selects após digitar 3+ caracteres para busca"
        @update:model-value="(v) => atualizarCampo('cestId', v as number | null)"
      />
    </div>
    <div class="col-4 field">
      <label class="field-label">&nbsp;</label>
      <div style="display:flex; gap:20px; padding-top: 8px;">
        <label class="filter-checkbox">
          <input
            type="checkbox"
            :checked="produto.ativo"
            @change="atualizarCampo('ativo', ($event.target as HTMLInputElement).checked)"
          />
          <span>Ativo</span>
        </label>
        <label class="filter-checkbox">
          <input
            type="checkbox"
            :checked="produto.utilizaBalanca"
            @change="atualizarCampo('utilizaBalanca', ($event.target as HTMLInputElement).checked)"
          />
          <span>Utiliza balança</span>
        </label>
      </div>
    </div>

    <template v-if="produto.utilizaBalanca">
      <div class="col-4">
        <TextField
          :model-value="produto.codigoProdutoBalanca"
          label="Código produto balança"
          required
          :error="erros?.codigoProdutoBalanca"
          @update:model-value="(v) => atualizarCampo('codigoProdutoBalanca', v as string)"
        />
      </div>
      <div class="col-4">
        <SelectField
          :model-value="produto.balancaId"
          label="Balança"
          required
          :options="balancasOpcoes"
          :error="erros?.balancaId"
          @update:model-value="(v) => atualizarCampo('balancaId', v as number | null)"
        />
      </div>
    </template>

    <div class="col-3">
      <MoneyInput
        :model-value="produto.pesoLiquido"
        label="Peso líquido (kg)"
        :symbol="false"
        @update:model-value="(v) => atualizarCampo('pesoLiquido', v ?? 0)"
      />
    </div>
    <div class="col-3">
      <MoneyInput
        :model-value="produto.pesoBruto"
        label="Peso bruto (kg)"
        :symbol="false"
        @update:model-value="(v) => atualizarCampo('pesoBruto', v ?? 0)"
      />
    </div>
  </div>
</template>
