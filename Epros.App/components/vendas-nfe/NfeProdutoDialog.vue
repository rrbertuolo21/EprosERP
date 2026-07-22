<script setup lang="ts">
/**
 * NfeProdutoDialog — diálogo de inclusão/edição de item da NF-e
 * (porta o comportamento de AdicionarProdutoDialog do legado, simplificado ao design novo).
 *
 * Permite buscar um produto em `estoque-produtos` (autocomplete) para preencher código, nome,
 * NCM e valor, ou preencher manualmente. CFOP vem das opções da página. Calcula o total da
 * linha ao vivo. Ao confirmar, emite `confirmar` com o item pronto.
 */
import { ref, computed, watch } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { useHelper } from '~/composables/useHelper'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import type { SelectOption } from '~/composables/useEnum'
import { criarItemVazio, type NfeItem } from './nfeTypes'

interface ProdutoResumo {
  id: number
  codigo?: string
  descricao?: string
  nome?: string
  ncmCodigo?: string
  ncm?: string
  unidadeMedida?: string
  valorVenda?: number
}

const props = defineProps<{
  modelValue: boolean
  cfopsOpcoes: SelectOption[]
  /** Item em edição; quando nulo o diálogo inicia uma nova linha. */
  itemEdicao?: NfeItem | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirmar: [item: NfeItem]
}>()

const { formatarMoeda } = useHelper()

const item = ref<NfeItem>(criarItemVazio())
const busca = ref('')
const resultados = ref<ProdutoResumo[]>([])
const carregandoBusca = ref(false)
const abertoDropdown = ref(false)
let debounce: ReturnType<typeof setTimeout> | undefined

const ehEdicao = computed(() => !!props.itemEdicao)

const totalLinha = computed(() => {
  const bruto = (item.value.quantidade || 0) * (item.value.valorUnitario || 0)
  return Math.max(0, bruto - (item.value.descontoValor || 0))
})

watch(
  () => props.modelValue,
  (aberto) => {
    if (aberto) {
      item.value = props.itemEdicao ? { ...props.itemEdicao } : criarItemVazio()
      busca.value = ''
      resultados.value = []
      abertoDropdown.value = false
    }
  }
)

watch(busca, (texto) => {
  if (debounce) clearTimeout(debounce)
  if (!texto || texto.length < 2) {
    resultados.value = []
    abertoDropdown.value = false
    return
  }
  debounce = setTimeout(() => buscarProdutos(texto), 350)
})

async function buscarProdutos(texto: string) {
  carregandoBusca.value = true
  try {
    const resp = await useApi('/estoque-produtos', { query: { busca: texto, tamanhoPagina: 20 } })
    resultados.value = extrairDados<ProdutoResumo[]>(resp) ?? []
    abertoDropdown.value = resultados.value.length > 0
  } catch (e) {
    console.error('[NfeProdutoDialog] falha na busca de produtos', e)
    resultados.value = []
  } finally {
    carregandoBusca.value = false
  }
}

function selecionarProduto(p: ProdutoResumo) {
  item.value = {
    ...item.value,
    produtoId: p.id,
    codigoProduto: p.codigo ?? String(p.id),
    nomeProduto: p.descricao ?? p.nome ?? '',
    ncm: p.ncmCodigo ?? p.ncm ?? '',
    unidade: p.unidadeMedida ?? item.value.unidade,
    valorUnitario: p.valorVenda ?? item.value.valorUnitario
  }
  busca.value = ''
  resultados.value = []
  abertoDropdown.value = false
}

function fechar() {
  emit('update:modelValue', false)
}

function confirmar() {
  emit('confirmar', { ...item.value, total: totalLinha.value })
  fechar()
}

const podeConfirmar = computed(
  () => item.value.nomeProduto.trim().length > 0 && item.value.quantidade > 0 && !!item.value.cfop
)
</script>

<template>
  <AppDialog
    :model-value="modelValue"
    :title="ehEdicao ? 'Editar Produto' : 'Adicionar Produto'"
    width="720px"
    persistent
    @update:model-value="emit('update:modelValue', $event)"
  >
    <!-- Busca de produto (apenas em inclusão) -->
    <div v-if="!ehEdicao" class="prod-busca">
      <label class="field-label">Buscar produto</label>
      <div class="prod-input-wrap">
        <input
          v-model="busca"
          class="input"
          placeholder="Código, descrição ou EAN"
        />
        <span v-if="carregandoBusca" class="prod-spinner spinner"></span>
        <ul v-if="abertoDropdown" class="prod-dropdown glass-panel">
          <li v-for="p in resultados" :key="p.id" class="prod-opcao" @click="selecionarProduto(p)">
            <span class="prod-opcao-nome">{{ p.descricao ?? p.nome }}</span>
            <span class="prod-opcao-meta">
              {{ p.codigo ?? p.id }} · {{ formatarMoeda(p.valorVenda ?? 0) }}
            </span>
          </li>
        </ul>
      </div>
    </div>

    <div class="form-grid prod-grid">
      <TextField
        v-model="item.codigoProduto"
        label="Código"
      />
      <div class="col-nome">
        <TextField v-model="item.nomeProduto" label="Descrição" required />
      </div>
      <TextField v-model="item.ncm" label="NCM" />
      <SelectField
        v-model="item.cfop"
        label="CFOP"
        :options="cfopsOpcoes"
        required
      />
      <TextField v-model="item.csosnCst" label="CST/CSOSN" />
      <TextField v-model="item.unidade" label="Unidade" />
      <QuantityInput v-model="item.quantidade" label="Quantidade" :suffix="item.unidade" />
      <MoneyInput v-model="item.valorUnitario" label="Valor Unitário" />
      <MoneyInput v-model="item.descontoValor" label="Desconto" />
      <PercentInput v-model="item.aliquotaIcms" label="% ICMS" />
      <PercentInput v-model="item.aliquotaIpi" label="% IPI" />
      <div class="col-info">
        <TextField v-model="item.informacoesAdicionais" label="Informações adicionais do produto" />
      </div>
    </div>

    <div class="prod-total">
      <span>Total da linha</span>
      <strong>{{ formatarMoeda(totalLinha) }}</strong>
    </div>

    <template #footer>
      <button type="button" class="btn btn-secondary" @click="fechar">Cancelar</button>
      <button type="button" class="btn btn-primary" :disabled="!podeConfirmar" @click="confirmar">
        {{ ehEdicao ? 'Salvar' : 'Adicionar' }}
      </button>
    </template>
  </AppDialog>
</template>

<style scoped>
.prod-busca { position: relative; margin-bottom: 16px; }
.prod-input-wrap { position: relative; }
.prod-spinner { position: absolute; right: 12px; top: 12px; }
.prod-dropdown {
  position: absolute; z-index: 30; top: calc(100% + 4px); left: 0; right: 0;
  list-style: none; max-height: 240px; overflow-y: auto; padding: 6px; margin: 0;
}
.prod-opcao { display: flex; flex-direction: column; gap: 2px; padding: 8px 10px; border-radius: 6px; cursor: pointer; }
.prod-opcao:hover { background: rgba(255, 255, 255, 0.05); }
.prod-opcao-nome { font-size: 13.5px; font-weight: 500; }
.prod-opcao-meta { font-size: 11.5px; color: var(--text-muted); }

.prod-grid { grid-template-columns: repeat(4, 1fr); gap: 12px; }
.col-nome { grid-column: span 3; }
.col-info { grid-column: 1 / -1; }
@media (max-width: 720px) {
  .prod-grid { grid-template-columns: 1fr 1fr; }
  .col-nome { grid-column: span 2; }
}

.prod-total {
  display: flex; align-items: center; justify-content: space-between;
  margin-top: 16px; padding: 12px 14px; border-radius: 8px;
  background: rgba(255, 255, 255, 0.03); border: 1px solid var(--border-color);
  font-size: 14px;
}
.prod-total strong { font-size: 16px; color: var(--primary); }
</style>
