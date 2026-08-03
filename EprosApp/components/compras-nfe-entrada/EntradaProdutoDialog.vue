<script setup lang="ts">
/**
 * EntradaProdutoDialog — modal de inclusão/edição de item da NF-e de entrada.
 *
 * Porta o `NfeAdicionarProdutoDialog` do legado para o design novo. Permite buscar um produto
 * cadastrado (endpoint `estoque-produtos`) ou informar código/descrição manualmente, além de
 * quantidade, valor unitário, desconto, CFOP, CST/CSOSN e alíquotas de ICMS/IPI.
 *
 * Emite `confirmar` com o item pronto; a página decide se adiciona ou atualiza.
 */
import { ref, watch, computed } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import type { SelectOption } from '~/composables/useEnum'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import { criarItemVazio, type EntradaItem } from './tipos'

const props = defineProps<{
  modelValue: boolean
  /** Opções de CFOP (carregadas na página). */
  cfopsOpcoes: SelectOption[]
  /** Item em edição; null = novo. */
  itemEdicao: EntradaItem | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirmar: [item: EntradaItem]
}>()

interface ProdutoResumo {
  id: number
  sku?: string
  codigo?: string
  nome?: string
  descricao?: string
  ncm?: string
  unidade?: string
  precoCusto?: number
  valorCusto?: number
}

const item = ref<EntradaItem>(criarItemVazio())
const erro = ref('')

// Busca de produto
const termo = ref('')
const buscando = ref(false)
const resultados = ref<ProdutoResumo[]>([])
const mostrarResultados = ref(false)

const ehEdicao = computed(() => !!props.itemEdicao)

watch(
  () => props.modelValue,
  (aberto) => {
    if (aberto) {
      item.value = props.itemEdicao ? { ...props.itemEdicao } : criarItemVazio()
      erro.value = ''
      termo.value = ''
      resultados.value = []
      mostrarResultados.value = false
    }
  }
)

async function buscarProduto() {
  const q = termo.value.trim()
  if (q.length < 2) {
    resultados.value = []
    return
  }
  buscando.value = true
  try {
    const resp = await useApi('/estoque-produtos', {
      query: { localizar: q, pagina: 1, tamanhoPagina: 20 }
    })
    resultados.value = extrairDados<ProdutoResumo[]>(resp) ?? []
    mostrarResultados.value = true
  } catch (e) {
    console.error('[EntradaProdutoDialog] falha ao buscar produto', e)
    resultados.value = []
  } finally {
    buscando.value = false
  }
}

function selecionarProduto(p: ProdutoResumo) {
  item.value.produtoId = p.id
  item.value.codigoProduto = p.sku ?? p.codigo ?? String(p.id)
  item.value.nomeProduto = p.nome ?? p.descricao ?? ''
  item.value.ncm = p.ncm ?? item.value.ncm
  item.value.unidade = p.unidade ?? item.value.unidade
  if (item.value.valorUnitario === 0) {
    item.value.valorUnitario = p.precoCusto ?? p.valorCusto ?? 0
  }
  mostrarResultados.value = false
  termo.value = ''
  resultados.value = []
}

const totalLinha = computed(() =>
  Math.max(0, (item.value.quantidade || 0) * (item.value.valorUnitario || 0) - (item.value.descontoValor || 0))
)

function confirmar() {
  if (!item.value.codigoProduto.trim()) {
    erro.value = 'Informe o código/SKU do produto'
    return
  }
  if (!item.value.nomeProduto.trim()) {
    erro.value = 'Informe a descrição do produto'
    return
  }
  if ((item.value.quantidade || 0) <= 0) {
    erro.value = 'A quantidade deve ser maior que zero'
    return
  }
  if ((item.value.valorUnitario || 0) <= 0) {
    erro.value = 'O valor unitário deve ser maior que zero'
    return
  }
  emit('confirmar', { ...item.value })
  emit('update:modelValue', false)
}

function fechar() {
  emit('update:modelValue', false)
}
</script>

<template>
  <AppDialog
    :model-value="modelValue"
    :title="ehEdicao ? 'Editar produto' : 'Adicionar produto'"
    width="720px"
    persistent
    @update:model-value="emit('update:modelValue', $event)"
  >
    <!-- Busca (apenas em inclusão) -->
    <div v-if="!ehEdicao" class="prod-busca">
      <div class="busca-input">
        <TextField
          v-model="termo"
          label="Buscar produto cadastrado"
          placeholder="Código, SKU ou descrição..."
          @update:model-value="mostrarResultados = false"
        />
        <button type="button" class="btn btn-secondary btn-sm" :disabled="buscando" @click="buscarProduto">
          <span v-if="buscando" class="spinner"></span>
          <span v-else>Buscar</span>
        </button>
      </div>
      <ul v-if="mostrarResultados && resultados.length" class="busca-resultados">
        <li v-for="p in resultados" :key="p.id" @click="selecionarProduto(p)">
          <span class="res-cod">{{ p.sku ?? p.codigo }}</span>
          <span class="res-nome">{{ p.nome ?? p.descricao }}</span>
        </li>
      </ul>
      <p v-else-if="mostrarResultados && !resultados.length" class="busca-vazio">
        Nenhum produto encontrado. Você pode informar os dados manualmente abaixo.
      </p>
    </div>

    <div class="form-grid">
      <TextField v-model="item.codigoProduto" label="Código / SKU" required />
      <TextField class="col-span-3" v-model="item.nomeProduto" label="Descrição" required />
      <TextField v-model="item.ncm" label="NCM" placeholder="NCM" />
      <SelectField
        v-model="item.cfop"
        label="CFOP"
        :options="cfopsOpcoes"
        placeholder="Selecione o CFOP"
      />
      <TextField v-model="item.csosnCst" label="CST / CSOSN" placeholder="Ex.: 000" />
      <TextField v-model="item.unidade" label="Unidade" placeholder="UN" />
      <QuantityInput v-model="item.quantidade" label="Quantidade" :suffix="item.unidade" required />
      <MoneyInput v-model="item.valorUnitario" label="Valor unitário" required />
      <MoneyInput v-model="item.descontoValor" label="Desconto" />
      <PercentInput v-model="item.aliquotaIcms" label="% ICMS" />
      <PercentInput v-model="item.aliquotaIpi" label="% IPI" />
      <TextField class="col-span-4" v-model="item.informacoesAdicionais" label="Informações adicionais do item" />
    </div>

    <div class="prod-total">
      <span>Total da linha:</span>
      <strong>{{ new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(totalLinha) }}</strong>
    </div>

    <span v-if="erro" class="field-error">{{ erro }}</span>

    <template #footer>
      <button type="button" class="btn btn-secondary" @click="fechar">Cancelar</button>
      <button type="button" class="btn btn-primary" @click="confirmar">
        {{ ehEdicao ? 'Salvar' : 'Adicionar' }}
      </button>
    </template>
  </AppDialog>
</template>

<style scoped>
.prod-busca { margin-bottom: 16px; }
.busca-input { display: flex; align-items: flex-end; gap: 10px; }
.busca-input :deep(.field) { flex: 1; }
.busca-resultados { list-style: none; margin-top: 6px; border: 1px solid var(--border-color); border-radius: 8px; overflow: hidden; max-height: 220px; overflow-y: auto; }
.busca-resultados li { display: flex; gap: 12px; padding: 8px 12px; cursor: pointer; font-size: 13px; }
.busca-resultados li:hover { background: rgba(255,255,255,0.05); }
.res-cod { font-family: monospace; color: var(--text-muted); min-width: 90px; }
.busca-vazio { font-size: 12.5px; color: var(--text-muted); margin-top: 6px; }
.form-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px 14px; }
.col-span-3 { grid-column: span 3; }
.col-span-4 { grid-column: span 4; }
.prod-total { display: flex; justify-content: flex-end; gap: 10px; align-items: baseline; margin-top: 14px; font-size: 14px; }
.prod-total strong { font-size: 16px; color: var(--primary); }
@media (max-width: 760px) {
  .form-grid { grid-template-columns: repeat(2, 1fr); }
  .col-span-3, .col-span-4 { grid-column: span 2; }
}
</style>
