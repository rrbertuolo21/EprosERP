<script setup lang="ts">
/**
 * Emissão de NF-e de Entrada (fatia 13) — `erp/compras/emissao/nfe-entrada/[[id]]`.
 *
 * Porta o comportamento da tela legada `compras/emissao/nfe-entrada/[[id]].vue`
 * (que usava `useNfeEmissaoPage(nfeEntradaConfig)`) para o design system novo: dados da nota do
 * fornecedor, fornecedor, produtos, totais/impostos (modo entrada) e informações complementares;
 * ações de salvar/lançar, transmitir (com overlay + realtime), cancelar e pré-visualizar a DANFE.
 *
 * Sem parâmetro (`/nfe-entrada`) inicia uma nota nova; com um GUID carrega a compra existente.
 * Toda a lógica de IO e estado vive em `useNfeEntrada`; esta página é orquestração de UI.
 *
 * Endpoints: compras (lancar, {id}, {id}/cancelar); vendas-fiscal (DANFE, quando existir).
 */
import { computed, onMounted, onBeforeUnmount, ref } from 'vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DanfeViewer from '~/components/shared/DanfeViewer.vue'
import TransmissionOverlay from '~/components/shared/TransmissionOverlay.vue'
import EntradaDadosBasicosCard from '~/components/compras-nfe-entrada/EntradaDadosBasicosCard.vue'
import EntradaFornecedorCard from '~/components/compras-nfe-entrada/EntradaFornecedorCard.vue'
import EntradaTransporteCard from '~/components/compras-nfe-entrada/EntradaTransporteCard.vue'
import EntradaProdutosCard from '~/components/compras-nfe-entrada/EntradaProdutosCard.vue'
import EntradaProdutoDialog from '~/components/compras-nfe-entrada/EntradaProdutoDialog.vue'
import EntradaTotaisCard from '~/components/compras-nfe-entrada/EntradaTotaisCard.vue'
import EntradaPagamentosCard from '~/components/compras-nfe-entrada/EntradaPagamentosCard.vue'
import EntradaInformacoesCard from '~/components/compras-nfe-entrada/EntradaInformacoesCard.vue'
import EntradaReferenciadasDialog from '~/components/compras-nfe-entrada/EntradaReferenciadasDialog.vue'
import { useNfeEntrada } from '~/components/compras-nfe-entrada/useNfeEntrada'
import { useToast } from '~/composables/useToast'
import { useApi, extrairDados } from '~/composables/useApi'
import type { SelectOption } from '~/composables/useEnum'
import type { EntradaItem } from '~/components/compras-nfe-entrada/tipos'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = computed(() => {
  const bruto = route.params.id
  const valor = Array.isArray(bruto) ? bruto[0] : bruto
  return valor && String(valor).trim() !== '' ? String(valor) : null
})

const {
  nfe,
  carregando,
  salvando,
  erros,
  totais,
  lancada,
  cancelada,
  podeTransmitir,
  semFornecedor,
  overlayVisivel,
  overlayPasso,
  overlayErro,
  passosTransmissao,
  danfeSrc,
  danfeVisivel,
  carregarCompra,
  salvar,
  transmitir,
  cancelar,
  gerarDanfe,
  baixarDanfe,
  imprimirDanfe,
  adicionarItem,
  atualizarItem,
  removerItem,
  salvarReferenciadas
} = useNfeEntrada('entrada')

const readonly = computed(() => lancada.value || cancelada.value)

// --- CFOPs de apoio ---
const cfopsOpcoes = ref<SelectOption[]>([])
async function carregarCfops() {
  try {
    const resp = await useApi('/cfops', { query: { tamanhoPagina: 500 } })
    const itens = extrairDados<Array<{ id: number; cfop?: string | number; descricao?: string }>>(resp) ?? []
    cfopsOpcoes.value = itens.map((c) => ({
      label: c.cfop ? `${c.cfop} - ${c.descricao ?? ''}`.trim() : c.descricao ?? String(c.id),
      value: c.id
    }))
  } catch (e) {
    console.error('[nfe-entrada] falha ao carregar CFOPs', e)
  }
}

// --- Estado de UI (diálogos) ---
const mostrarProdutoDialog = ref(false)
const itemEmEdicao = ref<EntradaItem | null>(null)
const mostrarReferenciadas = ref(false)
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const titulo = computed(() =>
  nfe.id ? `Edição de NF-e de Entrada ${nfe.numero ? `#${nfe.numero}` : ''}`.trim() : 'Emissão de NF-e de Entrada'
)
const bloqueiaTroca = computed(() => !!(nfe.fornecedor.pessoaId || nfe.fornecedor.nome) && nfe.itens.length > 0)

// --- Produtos ---
function abrirNovoProduto() {
  if (semFornecedor.value) {
    toast.warning('Informe o fornecedor antes de adicionar produtos')
    return
  }
  itemEmEdicao.value = null
  mostrarProdutoDialog.value = true
}

function abrirEdicaoProduto(item: EntradaItem) {
  itemEmEdicao.value = item
  mostrarProdutoDialog.value = true
}

function confirmarProduto(item: EntradaItem) {
  if (itemEmEdicao.value) atualizarItem(itemEmEdicao.value._uid, item)
  else adicionarItem(item)
  itemEmEdicao.value = null
}

async function confirmarRemocaoProduto(uid: string) {
  const ok = await confirmRef.value?.open('Remover produto', 'Deseja remover este item da nota?', { danger: true })
  if (ok) removerItem(uid)
}

// --- Troca de fornecedor bloqueada (remove itens) ---
async function solicitarTrocaFornecedor() {
  const ok = await confirmRef.value?.open(
    'Alterar fornecedor',
    'Ao alterar o fornecedor, todos os produtos da nota serão removidos. Deseja continuar?',
    { danger: true }
  )
  if (ok) {
    nfe.itens = []
    nfe.fornecedor = { pessoaId: null, nome: '', documento: '', inscricaoEstadual: '', enderecoFormatado: '' }
  }
}

// --- Ações de barra ---
async function onSalvar() {
  await salvar()
}

async function onTransmitir() {
  await transmitir()
}

async function onCancelar() {
  const ok = await confirmRef.value?.open(
    'Cancelar NF-e de entrada',
    nfe.id
      ? 'Tem certeza que deseja cancelar esta NF-e de entrada? A operação é irreversível.'
      : 'Descartar os dados desta nota?',
    { danger: true, textoConfirmar: 'Cancelar' }
  )
  if (!ok) return
  await cancelar('Cancelamento solicitado pelo emitente')
}

async function onNovaNota() {
  const ok = await confirmRef.value?.open(
    'Nova nota',
    'Descartar a nota atual e iniciar uma nova? Alterações não salvas serão perdidas.'
  )
  if (ok) router.push('/erp/compras/emissao/nfe-entrada')
}

function onConfirmarReferenciadas(chaves: string[]) {
  salvarReferenciadas(chaves)
}

// --- Atalho F2 adiciona produto ---
function aoTeclar(ev: KeyboardEvent) {
  if (ev.key === 'F2') {
    ev.preventDefault()
    abrirNovoProduto()
  }
}

onMounted(async () => {
  await carregarCfops()
  if (idParam.value) {
    await carregarCompra(idParam.value)
  }
  window.addEventListener('keydown', aoTeclar)
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', aoTeclar)
})
</script>

<template>
  <div class="entrada-page">
    <PageToolbar :title="titulo" subtitle="Compras · NF-e de Entrada" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-ghost" @click="router.push('/erp/compras')">Compras</button>
        <button type="button" class="btn btn-ghost" @click="onNovaNota">Nova</button>
        <button
          v-if="nfe.id"
          type="button"
          class="btn btn-secondary"
          :disabled="carregando"
          @click="gerarDanfe"
        >
          Pré-visualizar DANFE
        </button>
        <button
          v-if="lancada"
          type="button"
          class="btn btn-danger"
          :disabled="salvando"
          @click="onCancelar"
        >
          Cancelar NF-e
        </button>
        <template v-else>
          <button type="button" class="btn btn-ghost" :disabled="salvando" @click="onCancelar">Cancelar</button>
          <button type="button" class="btn btn-secondary" :disabled="salvando" @click="onSalvar">
            <span v-if="salvando" class="spinner"></span>
            <span v-else>Salvar</span>
          </button>
          <button
            type="button"
            class="btn btn-primary"
            :disabled="salvando || !podeTransmitir"
            @click="onTransmitir"
          >
            Transmitir
          </button>
        </template>
      </template>
    </PageToolbar>

    <!-- Faixa de status -->
    <div v-if="nfe.situacao" class="entrada-status-bar glass-panel">
      <span class="entrada-status-label">Situação:</span>
      <span
        class="chip"
        :class="{
          'chip-success': lancada,
          'chip-danger': cancelada
        }"
      >{{ nfe.situacao }}</span>
      <span v-if="nfe.numero" class="entrada-status-meta">Nº {{ nfe.numero }}<span v-if="nfe.serie"> · Série {{ nfe.serie }}</span></span>
      <span v-if="nfe.chave" class="entrada-status-chave">Chave: {{ nfe.chave }}</span>
    </div>

    <EntradaDadosBasicosCard v-model="nfe" :erros="erros" :readonly="readonly" />

    <EntradaFornecedorCard
      v-model="nfe"
      :bloqueado="bloqueiaTroca"
      :quantidade-itens="nfe.itens.length"
      :readonly="readonly"
      :erro="erros.fornecedor"
      @solicitar-troca="solicitarTrocaFornecedor"
      @novo-fornecedor="router.push('/erp/cadastros/parceiros/novo')"
    />

    <EntradaTransporteCard v-model="nfe" :readonly="readonly" />

    <EntradaProdutosCard
      :itens="nfe.itens"
      :bloqueado="semFornecedor"
      :readonly="readonly"
      :erro="erros.itens"
      @adicionar="abrirNovoProduto"
      @editar="abrirEdicaoProduto"
      @remover="confirmarRemocaoProduto"
    />

    <EntradaTotaisCard v-model="nfe" :totais="totais" :readonly="readonly" />

    <EntradaPagamentosCard v-model="nfe" :total-nota="totais.valorNotaFiscal" :readonly="readonly" />

    <EntradaInformacoesCard
      v-model="nfe"
      :readonly="readonly"
      mostrar-referenciadas
      @abrir-referenciadas="mostrarReferenciadas = true"
    />

    <!-- Diálogos -->
    <EntradaProdutoDialog
      v-model="mostrarProdutoDialog"
      :cfops-opcoes="cfopsOpcoes"
      :item-edicao="itemEmEdicao"
      @confirmar="confirmarProduto"
    />

    <EntradaReferenciadasDialog
      v-model="mostrarReferenciadas"
      :chaves="nfe.chavesReferenciadas"
      @confirmar="onConfirmarReferenciadas"
    />

    <!-- DANFE -->
    <AppDialog v-model="danfeVisivel" title="Pré-visualização da DANFE" width="90%">
      <DanfeViewer :src="danfeSrc" title="DANFE" @download="baixarDanfe" @print="imprimirDanfe" />
    </AppDialog>

    <!-- Overlay de transmissão em tempo real -->
    <TransmissionOverlay
      v-model="overlayVisivel"
      title="Transmitindo NF-e de Entrada"
      message="Aguarde enquanto a nota de entrada é lançada."
      :steps="passosTransmissao"
      :current-step="overlayPasso"
      :error="overlayErro"
    />

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.entrada-page { padding-bottom: 40px; }
.entrada-status-bar {
  display: flex; align-items: center; flex-wrap: wrap; gap: 12px;
  padding: 12px 18px; margin-bottom: 16px;
}
.entrada-status-label { font-size: 13px; font-weight: 600; color: var(--text-secondary); }
.entrada-status-meta { font-size: 13px; color: var(--text-secondary); }
.entrada-status-chave { font-size: 12px; color: var(--text-muted); font-family: monospace; word-break: break-all; }
</style>
