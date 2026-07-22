<script setup lang="ts">
/**
 * MovimentoManualDialog — modal para lançar uma movimentação manual de estoque.
 * Porta o comportamento de `components/estoque/MovimentoManualDialog.vue` do legado.
 *
 * Quando `produtoId` é informado (uso embutido na tela de estoque por produto), o campo
 * produto fica fixo (somente leitura). Caso contrário (uso standalone na tela de
 * movimento-manual), permite buscar o produto por descrição/código.
 *
 * Endpoints: estoque-movimentos-manuais (POST), estoque-produtos (busca de produto por descrição).
 */
import { computed, ref, watch } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useEnum, type SelectOption } from '~/composables/useEnum'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'

interface ProdutoBusca {
  id: number
  descricao: string
  codigo?: string | null
}

const props = defineProps<{
  modelValue: boolean
  /** Quando informado, o produto fica fixo (uso embutido na tela de estoque por produto). */
  produtoId?: number | null
  produtoLabel?: string | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  created: []
}>()

const toast = useToast()
const { carregarOpcoes } = useEnum()

const produtoFixo = computed(() => props.produtoId != null && props.produtoId > 0)

const salvando = ref(false)
const erros = ref<Record<string, string>>({})

const tipoEstoqueOpcoes = ref<SelectOption[]>([])
const tipoMovimentoOpcoes = ref<SelectOption[]>([])
const carregandoEnums = ref(false)

// --- Busca de produto (quando não é embutido) ---
const buscaProduto = ref('')
const produtosEncontrados = ref<ProdutoBusca[]>([])
const buscandoProduto = ref(false)
const produtoSelecionado = ref<ProdutoBusca | null>(null)
let debounceBusca: ReturnType<typeof setTimeout> | undefined

const form = ref({
  produtoId: null as number | null,
  tipoEstoque: null as number | string | null,
  tipoMovimento: null as number | string | null,
  quantidadeMovimentada: 0 as number | null,
  valorUnitario: 0 as number | null
})

function estadoInicial() {
  form.value = {
    produtoId: produtoFixo.value ? (props.produtoId ?? null) : null,
    tipoEstoque: null,
    tipoMovimento: null,
    quantidadeMovimentada: 0,
    valorUnitario: 0
  }
  erros.value = {}
  produtoSelecionado.value = null
  buscaProduto.value = ''
  produtosEncontrados.value = []
}

async function carregarEnums() {
  carregandoEnums.value = true
  try {
    const [tiposEstoque, tiposMovimento] = await Promise.all([
      carregarOpcoes('/estoque-enums/tipo-estoque'),
      carregarOpcoes('/dfe-enums/tipo-movimento')
    ])
    tipoEstoqueOpcoes.value = tiposEstoque
    tipoMovimentoOpcoes.value = tiposMovimento
  } catch (e) {
    console.error('[MovimentoManualDialog] falha ao carregar enums', e)
  } finally {
    carregandoEnums.value = false
  }
}

function buscarProdutos(texto: string) {
  buscaProduto.value = texto
  if (debounceBusca) clearTimeout(debounceBusca)
  if (!texto || texto.trim().length < 2) {
    produtosEncontrados.value = produtoSelecionado.value ? [produtoSelecionado.value] : []
    return
  }
  debounceBusca = setTimeout(async () => {
    buscandoProduto.value = true
    try {
      const resp = await useApi('/estoque-produtos', { query: { tamanhoPagina: 50, descricao: texto } })
      const itens = extrairDados<Array<{ id: number; produtoId?: number; descricao?: string; codigo?: string }>>(resp) ?? []
      produtosEncontrados.value = itens.map((i) => ({
        id: i.produtoId ?? i.id,
        descricao: i.descricao ?? `Produto #${i.produtoId ?? i.id}`,
        codigo: i.codigo ?? null
      }))
    } catch (e) {
      console.error('[MovimentoManualDialog] falha na busca de produto', e)
      produtosEncontrados.value = []
    } finally {
      buscandoProduto.value = false
    }
  }, 400)
}

function selecionarProduto(id: number | string | null) {
  const encontrado = produtosEncontrados.value.find((p) => p.id === id) ?? null
  produtoSelecionado.value = encontrado
  form.value.produtoId = encontrado?.id ?? null
}

function fechar() {
  emit('update:modelValue', false)
}

function validar(): boolean {
  const novosErros: Record<string, string> = {}
  if (!form.value.produtoId) novosErros.produtoId = 'Selecione um produto'
  if (form.value.tipoEstoque == null) novosErros.tipoEstoque = 'Selecione o tipo de estoque'
  if (form.value.tipoMovimento == null) novosErros.tipoMovimento = 'Selecione o tipo de movimento'
  const quantidade = Number(form.value.quantidadeMovimentada ?? 0)
  if (quantidade <= 0) novosErros.quantidadeMovimentada = 'A quantidade deve ser maior que zero'
  const valorUnitario = Number(form.value.valorUnitario ?? 0)
  if (valorUnitario < 0) novosErros.valorUnitario = 'O valor unitário não pode ser negativo'
  erros.value = novosErros
  return Object.keys(novosErros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação')
    return
  }

  salvando.value = true
  try {
    await useApi('/estoque-movimentos-manuais', {
      method: 'POST',
      body: {
        produtoId: Number(form.value.produtoId),
        tipoEstoque: Number(form.value.tipoEstoque),
        tipoMovimento: Number(form.value.tipoMovimento),
        quantidadeMovimentada: Number(form.value.quantidadeMovimentada ?? 0),
        valorUnitario: Number(form.value.valorUnitario ?? 0)
      }
    })
    toast.success('Movimentação criada com sucesso')
    fechar()
    estadoInicial()
    emit('created')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

watch(
  () => props.modelValue,
  async (aberto) => {
    if (!aberto) return
    estadoInicial()
    await carregarEnums()
  }
)
</script>

<template>
  <AppDialog
    :model-value="modelValue"
    title="Adicionar movimentação manual"
    width="640px"
    persistent
    @update:model-value="emit('update:modelValue', $event)"
  >
    <div class="form-grid">
      <div v-if="produtoFixo" class="col-12">
        <TextField :model-value="produtoLabel ?? `Produto #${produtoId}`" label="Produto" readonly />
      </div>
      <div v-else class="col-12">
        <label class="field-label">Produto<span class="required">*</span></label>
        <input
          class="input"
          :class="{ 'is-invalid': !!erros.produtoId }"
          placeholder="Digite para pesquisar produto"
          :value="buscaProduto"
          @input="buscarProdutos(($event.target as HTMLInputElement).value)"
        />
        <span v-if="erros.produtoId" class="field-error">{{ erros.produtoId }}</span>
        <div v-if="buscandoProduto" class="field-hint">Buscando produtos...</div>
        <ul v-else-if="buscaProduto.trim() && produtosEncontrados.length" class="produto-sugestoes">
          <li
            v-for="p in produtosEncontrados"
            :key="p.id"
            :class="{ ativo: produtoSelecionado?.id === p.id }"
            @click="selecionarProduto(p.id); buscaProduto = p.descricao"
          >
            {{ p.codigo ? `${p.codigo} - ` : '' }}{{ p.descricao }}
          </li>
        </ul>
        <div v-else-if="buscaProduto.trim() && !buscandoProduto" class="field-hint">Nenhum produto encontrado</div>
      </div>

      <div class="col-6">
        <SelectField
          v-model="form.tipoEstoque"
          label="Tipo Estoque"
          required
          :options="tipoEstoqueOpcoes"
          :disabled="carregandoEnums"
          :error="erros.tipoEstoque"
        />
      </div>
      <div class="col-6">
        <SelectField
          v-model="form.tipoMovimento"
          label="Tipo Movimento"
          required
          :options="tipoMovimentoOpcoes"
          :disabled="carregandoEnums"
          :error="erros.tipoMovimento"
        />
      </div>
      <div class="col-6">
        <QuantityInput
          v-model="form.quantidadeMovimentada"
          label="Quantidade Movimentada"
          required
          :decimais="3"
          :error="erros.quantidadeMovimentada"
        />
      </div>
      <div class="col-6">
        <MoneyInput v-model="form.valorUnitario" label="Valor Unitário" :error="erros.valorUnitario" />
      </div>
    </div>

    <template #footer>
      <button type="button" class="btn btn-secondary" :disabled="salvando" @click="fechar">Cancelar</button>
      <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
        <span v-if="salvando" class="spinner"></span>
        <span v-else>Salvar</span>
      </button>
    </template>
  </AppDialog>
</template>

<style scoped>
.produto-sugestoes {
  list-style: none;
  margin: 4px 0 0;
  padding: 4px 0;
  max-height: 180px;
  overflow-y: auto;
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.03);
}
.produto-sugestoes li {
  padding: 8px 12px;
  cursor: pointer;
  font-size: 13px;
  color: var(--text-secondary);
}
.produto-sugestoes li:hover,
.produto-sugestoes li.ativo {
  background: rgba(255, 255, 255, 0.06);
  color: var(--text-primary);
}
</style>
