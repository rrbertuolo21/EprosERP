<script setup lang="ts">
/**
 * Entrada de Mercadorias (erp/compras/entrada-mercadorias/[[id]]).
 *
 * Porta o comportamento da tela legada `compras/entrada-mercadorias/[[id]].vue`, reduzido ao que
 * o backend real de Compras suporta hoje (`LancarCompraCommand`) e reconstruído no design system
 * do novo app (sem Vuetify / sem os NfeCards fiscais completos do legado):
 *   - Dados do documento de entrada: chave de acesso, modelo, série, número, situação;
 *   - Preenchimento automático do fornecedor a partir do CNPJ embutido na chave (busca em pessoas);
 *   - Busca/seleção manual do fornecedor (cadastros/pessoas);
 *   - Grade de itens com diálogo de inclusão/edição (estoque-produtos);
 *   - Totais da nota e valor total enviado ao backend;
 *   - Salvar (POST compras/lancar), Cancelar (POST compras/{id}/cancelar) e edição (GET compras/{id}).
 *
 * Endpoints: compras (lancar, {id}, {id}/cancelar), estoque-produtos, cadastros/pessoas.
 */
import { computed, onMounted, ref } from 'vue'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import { useMask } from '~/composables/useMask'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import ProdutosEntradaTable from '~/components/compras-entrada/ProdutosEntradaTable.vue'
import AdicionarProdutoDialog from '~/components/compras-entrada/AdicionarProdutoDialog.vue'
import type { ItemEntrada, LancarCompraPayload } from '~/components/compras-entrada/tipos'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

const SITUACAO_DOCUMENTO_OPTIONS: SelectOption[] = [
  { label: '00 - Documento regular - Situação Normal', value: '00' },
  { label: '01 - Escrituração extemporânea de documento regular', value: '01' },
  { label: '02 - Documento cancelado', value: '02' },
  { label: '03 - Escrituração extemporânea de documento cancelado', value: '03' },
  { label: '04 - NF-e, NFC-e ou CT-e - denegado', value: '04' },
  { label: '05 - NF-e, NFC-e ou CT-e - Numeração inutilizada', value: '05' },
  { label: '06 - Documento Fiscal Complementar', value: '06' },
  { label: '07 - Escrituração extemporânea de documento complementar', value: '07' },
  { label: '08 - Documento com base em Regime Especial ou Norma Específica', value: '08' }
]

const route = useRoute()
const toast = useToast()
const { formatarMoeda } = useHelper()
const { somenteDigitos, maskCpfCnpj } = useMask()

const rotaId = computed(() => {
  const p = route.params.id
  const valor = Array.isArray(p) ? p[0] : p
  return valor && valor !== 'novo' ? String(valor) : null
})
const isEditMode = computed(() => rotaId.value != null)

// --- Estado da nota ---
const carregando = ref(false)
const salvando = ref(false)
const cancelando = ref(false)
const statusCompra = ref<string | number | null>(null)

const chaveAcesso = ref('')
const modelo = ref('')
const serie = ref('')
const numero = ref('')
const situacao = ref('00')
// Mantido como 'YYYY-MM-DD' para o input nativo de data; convertido a DateTime no backend.
const dataEmissao = ref<string>(hojeIso())

function hojeIso(): string {
  const d = new Date()
  const mes = String(d.getMonth() + 1).padStart(2, '0')
  const dia = String(d.getDate()).padStart(2, '0')
  return `${d.getFullYear()}-${mes}-${dia}`
}

function soData(valor: string | null | undefined): string {
  if (!valor) return hojeIso()
  return valor.includes('T') ? valor.split('T')[0] : valor
}

const fornecedorId = ref<number | null>(null)
const fornecedorNome = ref('')
const fornecedorCnpj = ref('')

const itens = ref<ItemEntrada[]>([])

// Nota já lançada/cancelada é somente leitura (o backend só cria/cancela, não atualiza).
const somenteLeitura = computed(() => isEditMode.value)

// --- Totais ---
const totalItens = computed(() =>
  arredondar(itens.value.reduce((acc, i) => acc + (i.quantidade ?? 0) * (i.precoUnitario ?? 0), 0))
)
const totalDesconto = computed(() =>
  arredondar(itens.value.reduce((acc, i) => acc + (i.valorDesconto ?? 0), 0))
)
const totalNota = computed(() => arredondar(itens.value.reduce((acc, i) => acc + (i.totalItem ?? 0), 0)))

function arredondar(v: number): number {
  return Math.round((v + Number.EPSILON) * 100) / 100
}

// --- Fornecedor a partir da chave de acesso (CNPJ nas posições 7..20 da chave) ---
async function aoAlterarChave(valor: string) {
  const limpo = somenteDigitos(valor)
  chaveAcesso.value = limpo
  if (limpo.length !== 44) return
  // Layout da chave NF-e: UF(2) AAMM(4) CNPJ(14) modelo(2) serie(3) numero(9) ...
  modelo.value = limpo.substring(20, 22)
  serie.value = limpo.substring(22, 25)
  numero.value = String(Number(limpo.substring(25, 34)))
  const cnpj = limpo.substring(6, 20)
  fornecedorCnpj.value = cnpj
  await localizarFornecedorPorCnpj(cnpj)
}

async function localizarFornecedorPorCnpj(cnpj: string) {
  if (cnpj.length !== 14) return
  carregando.value = true
  try {
    const resposta = await useApi('/cadastros/pessoas', {
      query: { localizar: cnpj, pagina: 1, tamanhoPagina: 10 }
    })
    const pessoas = extrairLista<PessoaBusca>(resposta) ?? []
    const exato = pessoas.find((p) => somenteDigitos(documentoDaPessoa(p)) === cnpj)
    const escolhida = exato ?? pessoas[0]
    if (escolhida) selecionarFornecedor(escolhida)
    else toast.warning('Fornecedor não encontrado para o CNPJ da chave')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

// --- Busca manual de fornecedor ---
interface PessoaBusca {
  id: number
  pessoaJuridica?: { razaoSocial?: string; cnpj?: string } | null
  pessoaFisica?: { nome?: string; cpf?: string } | null
}

function documentoDaPessoa(p: PessoaBusca): string {
  return p.pessoaJuridica?.cnpj ?? p.pessoaFisica?.cpf ?? ''
}

function nomeDaPessoa(p: PessoaBusca): string {
  return p.pessoaJuridica?.razaoSocial ?? p.pessoaFisica?.nome ?? ''
}

const fornecedorDialog = ref(false)
const termoFornecedor = ref('')
const resultadosFornecedor = ref<PessoaBusca[]>([])
const buscandoFornecedor = ref(false)
let debounceForn: ReturnType<typeof setTimeout> | undefined

function abrirBuscaFornecedor() {
  termoFornecedor.value = ''
  resultadosFornecedor.value = []
  fornecedorDialog.value = true
}

function aoDigitarFornecedor(valor: string) {
  termoFornecedor.value = valor
  if (debounceForn) clearTimeout(debounceForn)
  if (!valor || valor.trim().length < 2) {
    resultadosFornecedor.value = []
    return
  }
  debounceForn = setTimeout(() => void buscarFornecedores(valor.trim()), 400)
}

async function buscarFornecedores(termo: string) {
  buscandoFornecedor.value = true
  try {
    const resposta = await useApi('/cadastros/pessoas', {
      query: { localizar: termo, pagina: 1, tamanhoPagina: 20 }
    })
    resultadosFornecedor.value = extrairLista<PessoaBusca>(resposta) ?? []
  } catch (e) {
    resultadosFornecedor.value = []
    console.error('[entrada-mercadorias] busca fornecedor', e)
  } finally {
    buscandoFornecedor.value = false
  }
}

function selecionarFornecedor(p: PessoaBusca) {
  fornecedorId.value = p.id
  fornecedorNome.value = nomeDaPessoa(p)
  fornecedorCnpj.value = somenteDigitos(documentoDaPessoa(p))
  fornecedorDialog.value = false
}

function limparFornecedor() {
  fornecedorId.value = null
  fornecedorNome.value = ''
  fornecedorCnpj.value = ''
}

const fornecedorCnpjFormatado = computed(() =>
  fornecedorCnpj.value ? maskCpfCnpj(fornecedorCnpj.value) : ''
)

// --- Diálogo de item ---
const produtoDialog = ref(false)
const itemEmEdicao = ref<ItemEntrada | null>(null)

function abrirNovoProduto() {
  itemEmEdicao.value = null
  produtoDialog.value = true
}

function abrirEdicaoProduto(index: number) {
  const item = itens.value[index]
  if (!item) return
  itemEmEdicao.value = { ...item, index }
  produtoDialog.value = true
}

function adicionarItem(item: ItemEntrada) {
  itens.value = [...itens.value, { ...item, index: itens.value.length }]
}

function editarItem(payload: { index: number; item: ItemEntrada }) {
  const copia = [...itens.value]
  copia[payload.index] = { ...payload.item, index: payload.index }
  itens.value = copia
}

function removerItem(index: number) {
  itens.value = itens.value.filter((_, i) => i !== index)
}

// --- Persistência ---
function validarNota(): boolean {
  if (!fornecedorCnpj.value || fornecedorCnpj.value.length !== 14) {
    toast.error('Informe um fornecedor com CNPJ válido')
    return false
  }
  if (!fornecedorNome.value.trim()) {
    toast.error('Informe o nome do fornecedor')
    return false
  }
  if (!numero.value.trim()) {
    toast.error('Informe o número da nota')
    return false
  }
  if (somenteDigitos(chaveAcesso.value).length !== 44) {
    toast.error('A chave de acesso deve possuir 44 dígitos')
    return false
  }
  if (itens.value.length === 0) {
    toast.error('Adicione ao menos um item à nota')
    return false
  }
  return true
}

function montarPayload(): LancarCompraPayload {
  return {
    fornecedorCnpj: fornecedorCnpj.value,
    fornecedorNome: fornecedorNome.value.trim(),
    numeroNota: numero.value.trim(),
    chaveAcesso: somenteDigitos(chaveAcesso.value),
    valorTotal: totalNota.value,
    dataEmissao: dataEmissao.value,
    itens: itens.value.map((i) => ({
      sku: i.sku,
      nomeProduto: i.nomeProduto,
      quantidade: i.quantidade,
      precoUnitario: i.precoUnitario,
      valorIms: i.valorIcms ?? 0,
      valorIpi: i.valorIpi ?? 0
    }))
  }
}

async function salvar() {
  if (salvando.value || somenteLeitura.value) return
  if (!validarNota()) return
  salvando.value = true
  try {
    await useApi('/compras/lancar', { method: 'POST', body: montarPayload() })
    toast.success('Nota de entrada lançada com sucesso')
    await navigateTo('/erp/compras')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

async function cancelar() {
  if (!rotaId.value) return
  const ok = await confirmRef.value?.open(
    'Cancelar compra',
    'Deseja realmente cancelar esta nota de entrada? A movimentação de estoque será estornada.',
    { danger: true, textoConfirmar: 'Cancelar compra', textoCancelar: 'Voltar' }
  )
  if (!ok) return
  cancelando.value = true
  try {
    await useApi('/compras/{id}/cancelar', {
      method: 'POST',
      params: { id: rotaId.value },
      body: { motivo: 'Cancelamento solicitado pela entrada de mercadorias' }
    })
    toast.success('Compra cancelada com sucesso')
    await navigateTo('/erp/compras')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    cancelando.value = false
  }
}

function voltar() {
  navigateTo('/erp/compras')
}

// --- Carga de edição ---
interface CompraDetalheItem {
  sku?: string
  codigoProduto?: string
  nomeProduto?: string
  descricaoProduto?: string
  produtoId?: number
  ncm?: string
  cfop?: string
  unidadeComercial?: string
  codigoEan?: string
  quantidade?: number
  precoUnitario?: number
  valorDesconto?: number
  valorIms?: number
  valorIpi?: number
}
interface CompraDetalhe {
  id: string
  fornecedorCnpj?: string
  fornecedorNome?: string
  numeroNota?: string
  chaveAcesso?: string
  valorTotal?: number
  dataEmissao?: string
  status?: string | number
  itens?: CompraDetalheItem[]
}

async function carregarCompra(id: string) {
  carregando.value = true
  try {
    const resposta = await useApi('/compras/{id}', { params: { id } })
    const compra = extrairDados<CompraDetalhe>(resposta)
    if (!compra) {
      toast.error('Nota de compra não encontrada')
      return
    }
    statusCompra.value = compra.status ?? null
    fornecedorCnpj.value = somenteDigitos(compra.fornecedorCnpj ?? '')
    fornecedorNome.value = compra.fornecedorNome ?? ''
    numero.value = compra.numeroNota ?? ''
    chaveAcesso.value = somenteDigitos(compra.chaveAcesso ?? '')
    dataEmissao.value = soData(compra.dataEmissao)
    if (chaveAcesso.value.length === 44) {
      modelo.value = chaveAcesso.value.substring(20, 22)
      serie.value = chaveAcesso.value.substring(22, 25)
    }
    itens.value = (compra.itens ?? []).map((i, index) => {
      const quantidade = i.quantidade ?? 0
      const precoUnitario = i.precoUnitario ?? 0
      const desconto = i.valorDesconto ?? 0
      return {
        index,
        produtoId: i.produtoId,
        sku: i.sku ?? i.codigoProduto ?? '',
        nomeProduto: i.nomeProduto ?? i.descricaoProduto ?? '',
        ncm: i.ncm,
        cfop: i.cfop,
        unidade: i.unidadeComercial,
        codigoEan: i.codigoEan,
        quantidade,
        precoUnitario,
        valorDesconto: desconto,
        valorIcms: i.valorIms ?? 0,
        valorIpi: i.valorIpi ?? 0,
        totalItem: arredondar(quantidade * precoUnitario - desconto)
      }
    })
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

const tituloPagina = computed(() =>
  isEditMode.value ? `Entrada de Mercadorias · Nota ${numero.value || rotaId.value}` : 'Nova Entrada de Mercadorias'
)

const notaCancelada = computed(() => String(statusCompra.value) === '4')

onMounted(() => {
  if (rotaId.value) void carregarCompra(rotaId.value)
})
</script>

<template>
  <div>
    <PageToolbar :title="tituloPagina" subtitle="Compras" :loading="carregando || salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
        <button
          v-if="isEditMode && !notaCancelada"
          type="button"
          class="btn btn-danger"
          :disabled="cancelando"
          @click="cancelar"
        >
          <span v-if="cancelando" class="spinner"></span>
          <span v-else>Cancelar</span>
        </button>
        <button
          v-if="!somenteLeitura"
          type="button"
          class="btn btn-primary"
          :disabled="salvando"
          @click="salvar"
        >
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div v-if="notaCancelada" class="aviso-cancelada glass-panel">
      Esta nota de entrada está <strong>cancelada</strong>. Os dados são exibidos apenas para consulta.
    </div>

    <!-- Dados do documento de entrada -->
    <section class="glass-panel bloco">
      <h3 class="bloco-titulo">Dados do Documento de Entrada</h3>
      <div class="form-grid">
        <div class="col-6">
          <TextField
            :model-value="chaveAcesso"
            label="Chave de Acesso"
            placeholder="44 dígitos da chave da NF-e"
            :maxlength="44"
            :readonly="somenteLeitura"
            @update:model-value="aoAlterarChave"
          />
        </div>
        <div class="col-2">
          <TextField v-model="modelo" label="Modelo" :readonly="somenteLeitura" />
        </div>
        <div class="col-2">
          <TextField v-model="serie" label="Série" :readonly="somenteLeitura" />
        </div>
        <div class="col-2">
          <TextField v-model="numero" label="Número" :readonly="somenteLeitura" required />
        </div>
        <div class="col-6">
          <SelectField
            v-model="situacao"
            :options="SITUACAO_DOCUMENTO_OPTIONS"
            label="Situação"
            :clearable="false"
            :disabled="somenteLeitura"
          />
        </div>
        <div class="col-6">
          <DateTimeField v-model="dataEmissao" label="Data de Emissão" :disabled="somenteLeitura" />
        </div>
      </div>
    </section>

    <!-- Fornecedor -->
    <section class="glass-panel bloco">
      <div class="bloco-header">
        <h3 class="bloco-titulo">Fornecedor</h3>
        <div v-if="!somenteLeitura" class="bloco-acoes">
          <button type="button" class="btn btn-secondary btn-sm" @click="abrirBuscaFornecedor">Buscar fornecedor</button>
          <button
            v-if="fornecedorId || fornecedorNome"
            type="button"
            class="btn btn-ghost btn-sm"
            @click="limparFornecedor"
          >
            Limpar
          </button>
        </div>
      </div>
      <div class="form-grid">
        <div class="col-8">
          <TextField v-model="fornecedorNome" label="Razão social / Nome" :readonly="somenteLeitura" required />
        </div>
        <div class="col-4">
          <TextField
            :model-value="fornecedorCnpjFormatado"
            label="CNPJ / CPF"
            readonly
            placeholder="Preenchido pela chave ou busca"
          />
        </div>
      </div>
    </section>

    <!-- Itens -->
    <ProdutosEntradaTable
      class="bloco"
      :itens="itens"
      :somente-leitura="somenteLeitura"
      @adicionar="abrirNovoProduto"
      @editar="abrirEdicaoProduto"
      @remover="removerItem"
    />

    <!-- Totais -->
    <section class="glass-panel bloco totais">
      <div class="total-cell">
        <span class="total-label">Qtde. de itens</span>
        <span class="total-valor">{{ itens.length }}</span>
      </div>
      <div class="total-cell">
        <span class="total-label">Total produtos</span>
        <span class="total-valor">{{ formatarMoeda(totalItens) }}</span>
      </div>
      <div class="total-cell">
        <span class="total-label">Total desconto</span>
        <span class="total-valor">{{ formatarMoeda(totalDesconto) }}</span>
      </div>
      <div class="total-cell destaque">
        <span class="total-label">Total da nota</span>
        <span class="total-valor">{{ formatarMoeda(totalNota) }}</span>
      </div>
    </section>

    <!-- Diálogo de item -->
    <AdicionarProdutoDialog
      v-model="produtoDialog"
      :item-em-edicao="itemEmEdicao"
      @adicionar="adicionarItem"
      @editar="editarItem"
    />

    <!-- Diálogo de busca de fornecedor -->
    <AppDialog v-model="fornecedorDialog" title="Buscar Fornecedor" width="640px">
      <div class="field">
        <label class="field-label">Fornecedor</label>
        <input
          class="input"
          type="text"
          placeholder="Nome, razão social ou CNPJ (mín. 2 caracteres)"
          :value="termoFornecedor"
          @input="aoDigitarFornecedor(($event.target as HTMLInputElement).value)"
        />
      </div>
      <div v-if="buscandoFornecedor" class="busca-status">Buscando...</div>
      <ul v-else-if="resultadosFornecedor.length" class="forn-resultados">
        <li
          v-for="p in resultadosFornecedor"
          :key="p.id"
          class="forn-item"
          @click="selecionarFornecedor(p)"
        >
          <span class="forn-nome">{{ nomeDaPessoa(p) }}</span>
          <span class="forn-doc">{{ maskCpfCnpj(documentoDaPessoa(p)) }}</span>
        </li>
      </ul>
      <p v-else-if="termoFornecedor.length >= 2" class="busca-status">Nenhum fornecedor encontrado.</p>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="fornecedorDialog = false">Fechar</button>
      </template>
    </AppDialog>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.bloco {
  margin-bottom: 16px;
}
section.bloco {
  padding: 16px;
}
.bloco-titulo {
  font-size: 15px;
  font-weight: 600;
  margin: 0 0 12px;
}
.bloco-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}
.bloco-header .bloco-titulo {
  margin: 0;
}
.bloco-acoes {
  display: flex;
  gap: 8px;
}
.aviso-cancelada {
  padding: 12px 16px;
  margin-bottom: 16px;
  border-left: 3px solid var(--danger);
  color: var(--text-secondary);
  font-size: 14px;
}
.totais {
  display: flex;
  flex-wrap: wrap;
  gap: 24px;
  justify-content: flex-end;
}
.total-cell {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 2px;
}
.total-label {
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  color: var(--text-secondary);
}
.total-valor {
  font-size: 16px;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}
.total-cell.destaque .total-valor {
  color: var(--primary);
  font-size: 20px;
}
.busca-status {
  margin-top: 8px;
  font-size: 13px;
  color: var(--text-secondary);
}
.forn-resultados {
  list-style: none;
  margin: 8px 0 0;
  padding: 0;
  max-height: 300px;
  overflow-y: auto;
}
.forn-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 12px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
}
.forn-item:hover {
  background: rgba(255, 255, 255, 0.06);
}
.forn-doc {
  color: var(--text-secondary);
  font-size: 12px;
  font-variant-numeric: tabular-nums;
}
</style>
