<script setup lang="ts">
/**
 * Devolução/Retorno de NF-e (fatia 11) — `erp/vendas/emissao/devolucao-retorno/nfe/[[id]]`.
 *
 * Porta a tela legada `vendas/emissao/devolucao-retorno/nfe/[[id]].vue` para o design novo.
 * Reutiliza os cartões da emissão de NF-e (fatia 7, read-only) — dados básicos, destinatário,
 * produtos, cálculo/totais, transporte, recebimentos e informações — e acrescenta o cartão
 * de devolução/retorno (finalidade fixa + notas de origem referenciadas).
 *
 * Sem parâmetro inicia uma devolução nova; com um GUID carrega a venda fiscal existente.
 * Toda a lógica de IO/estado vive em `useDevolucaoRetorno` (que encapsula `useNfeEmissao`).
 *
 * Endpoints: vendas, vendas-fiscal/{id}/nfe(+transmitir), .../nfe/referenciadas.
 */
import { computed, onMounted, onBeforeUnmount, ref } from 'vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DanfeViewer from '~/components/shared/DanfeViewer.vue'
import TransmissionOverlay from '~/components/shared/TransmissionOverlay.vue'
import NfeDadosBasicosCard from '~/components/vendas-nfe/NfeDadosBasicosCard.vue'
import NfeDestinatarioCard from '~/components/vendas-nfe/NfeDestinatarioCard.vue'
import NfeProdutosCard from '~/components/vendas-nfe/NfeProdutosCard.vue'
import NfeProdutoDialog from '~/components/vendas-nfe/NfeProdutoDialog.vue'
import NfeCalculoImpostosCard from '~/components/vendas-nfe/NfeCalculoImpostosCard.vue'
import NfeTransporteCard from '~/components/vendas-nfe/NfeTransporteCard.vue'
import NfeRecebimentosCard from '~/components/vendas-nfe/NfeRecebimentosCard.vue'
import NfeInformacoesCard from '~/components/vendas-nfe/NfeInformacoesCard.vue'
import NfeReferenciadasDialog from '~/components/vendas-nfe/NfeReferenciadasDialog.vue'
import DevolucaoRetornoCard from '~/components/vendas-transmissoes/DevolucaoRetornoCard.vue'
import { useDevolucaoRetorno } from '~/components/vendas-transmissoes/useDevolucaoRetorno'
import { useToast } from '~/composables/useToast'
import type { NfeItem } from '~/components/vendas-nfe/nfeTypes'

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
  emitida,
  podeTransmitir,
  overlayVisivel,
  overlayPasso,
  overlayErro,
  passosTransmissao,
  danfeSrc,
  danfeVisivel,
  cfopsOpcoes,
  tiposOperacaoOpcoes,
  carregarListasApoio,
  carregarVenda,
  salvarRascunho,
  transmitir,
  gerarDanfe,
  baixarDanfe,
  imprimirDanfe,
  adicionarItem,
  atualizarItem,
  removerItem,
  // específicos da devolução/retorno
  finalidade,
  finalidades,
  chavesOrigem,
  titulo,
  adicionarChaveOrigem,
  removerChaveOrigem,
  iniciarDeOrigem,
  salvarReferenciadas
} = useDevolucaoRetorno()

// --- Estado de UI (diálogos) ---
const mostrarProdutoDialog = ref(false)
const itemEmEdicao = ref<NfeItem | null>(null)
const mostrarReferenciadas = ref(false)
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const semDestinatario = computed(() => !nfe.destinatario.pessoaId)
const bloqueiaTroca = computed(() => !!nfe.destinatario.pessoaId && nfe.itens.length > 0)

// --- Produtos ---
function abrirNovoProduto() {
  if (semDestinatario.value) {
    toast.warning('Selecione um destinatário antes de adicionar produtos')
    return
  }
  itemEmEdicao.value = null
  mostrarProdutoDialog.value = true
}

function abrirEdicaoProduto(item: NfeItem) {
  itemEmEdicao.value = item
  mostrarProdutoDialog.value = true
}

function confirmarProduto(item: NfeItem) {
  if (itemEmEdicao.value) atualizarItem(itemEmEdicao.value._uid, item)
  else adicionarItem(item)
  itemEmEdicao.value = null
}

async function confirmarRemocaoProduto(uid: string) {
  const ok = await confirmRef.value?.open('Remover produto', 'Deseja remover este item da nota?', { danger: true })
  if (ok) removerItem(uid)
}

async function solicitarTrocaDestinatario() {
  const ok = await confirmRef.value?.open(
    'Alterar destinatário',
    'Ao alterar o destinatário, todos os produtos da nota serão removidos. Deseja continuar?',
    { danger: true }
  )
  if (ok) {
    nfe.itens = []
    nfe.destinatario = {
      pessoaId: null,
      nome: '',
      documento: '',
      enderecoEntregaId: null,
      enderecoCobrancaId: null,
      enderecoFormatado: ''
    }
  }
}

// --- Ações de barra ---
async function onSalvar() {
  const ok = await salvarRascunho()
  if (ok) await salvarReferenciadas()
}

async function onTransmitir() {
  if (chavesOrigem.value.length === 0) {
    toast.warning('Informe ao menos uma nota fiscal de origem antes de transmitir')
    return
  }
  await transmitir()
  // Persiste as notas de origem referenciadas após a transmissão (id já garantido).
  await salvarReferenciadas()
}

async function onAbandonar() {
  const ok = await confirmRef.value?.open(
    'Abandonar devolução',
    'Descartar a devolução/retorno atual? Alterações não salvas serão perdidas.'
  )
  if (ok) router.push('/erp/vendas/transmissoes')
}

function onConfirmarReferenciadas(chaves: string[]) {
  // Mantém sincronizadas as notas de origem exibidas no cartão.
  chavesOrigem.value = [...chaves]
  nfe.chavesReferenciadas = [...chaves]
}

// --- Atalhos de teclado (F2 adiciona produto) ---
function aoTeclar(ev: KeyboardEvent) {
  if (ev.key === 'F2') {
    ev.preventDefault()
    abrirNovoProduto()
  }
}

onMounted(async () => {
  await carregarListasApoio()
  if (idParam.value) {
    // Edição de uma devolução já existente.
    await carregarVenda(idParam.value)
  } else {
    // Fluxo a partir do monitor de transmissões: origem passada por query.
    const origemId = route.query.origemId ? String(route.query.origemId) : null
    const chaveOrigem = route.query.chaveOrigem ? String(route.query.chaveOrigem) : null
    if (origemId) {
      await iniciarDeOrigem(origemId, chaveOrigem)
    } else if (chaveOrigem) {
      adicionarChaveOrigem(chaveOrigem)
    }
  }
  window.addEventListener('keydown', aoTeclar)
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', aoTeclar)
})
</script>

<template>
  <div class="devolucao-page">
    <PageToolbar :title="titulo" subtitle="Vendas · Devolução / Retorno de NF-e" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-ghost" @click="onAbandonar">Abandonar</button>
        <button
          v-if="nfe.id"
          type="button"
          class="btn btn-secondary"
          :disabled="carregando"
          @click="gerarDanfe"
        >
          Pré-visualizar DANFE
        </button>
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
    </PageToolbar>

    <div v-if="nfe.situacao" class="nfe-status-bar glass-panel">
      <span class="nfe-status-label">Situação:</span>
      <span
        class="chip"
        :class="{
          'chip-success': emitida,
          'chip-danger': nfe.situacao === 'Cancelada',
          'chip-warning': nfe.situacao === 'Rejeitada'
        }"
      >{{ nfe.situacao }}</span>
      <span v-if="nfe.numero" class="nfe-status-meta">Nº {{ nfe.numero }} · Série {{ nfe.serie }}</span>
      <span v-if="nfe.chave" class="nfe-status-chave">Chave: {{ nfe.chave }}</span>
    </div>

    <DevolucaoRetornoCard
      :finalidade="finalidade"
      :finalidades="finalidades"
      :chaves-origem="chavesOrigem"
      :readonly="emitida"
      @adicionar-chave="adicionarChaveOrigem"
      @remover-chave="removerChaveOrigem"
    />

    <NfeDadosBasicosCard
      v-model="nfe"
      :tipos-operacao-opcoes="tiposOperacaoOpcoes"
      :erros="erros"
    />

    <NfeDestinatarioCard
      v-model="nfe"
      :bloqueado="bloqueiaTroca"
      :quantidade-itens="nfe.itens.length"
      :erro="erros.destinatario"
      @solicitar-troca="solicitarTrocaDestinatario"
      @novo-parceiro="router.push('/erp/cadastros/parceiros/novo')"
    />

    <NfeProdutosCard
      :itens="nfe.itens"
      :bloqueado="semDestinatario"
      :readonly="emitida"
      :erro="erros.itens"
      @adicionar="abrirNovoProduto"
      @editar="abrirEdicaoProduto"
      @remover="confirmarRemocaoProduto"
    />

    <NfeCalculoImpostosCard
      v-model="nfe"
      :totais="totais"
      :readonly="emitida"
    />

    <NfeTransporteCard v-model="nfe" :readonly="emitida" />

    <NfeRecebimentosCard
      v-model="nfe"
      :total-nota="totais.valorNotaFiscal"
      :readonly="emitida"
    />

    <NfeInformacoesCard
      v-model="nfe"
      :readonly="emitida"
      @abrir-referenciadas="mostrarReferenciadas = true"
    />

    <!-- Diálogos -->
    <NfeProdutoDialog
      v-model="mostrarProdutoDialog"
      :cfops-opcoes="cfopsOpcoes"
      :item-edicao="itemEmEdicao"
      @confirmar="confirmarProduto"
    />

    <NfeReferenciadasDialog
      v-model="mostrarReferenciadas"
      :chaves="nfe.chavesReferenciadas"
      @confirmar="onConfirmarReferenciadas"
    />

    <AppDialog v-model="danfeVisivel" title="Pré-visualização da DANFE" width="90%">
      <DanfeViewer
        :src="danfeSrc"
        title="DANFE"
        @download="baixarDanfe"
        @print="imprimirDanfe"
      />
    </AppDialog>

    <TransmissionOverlay
      v-model="overlayVisivel"
      title="Transmitindo Devolução/Retorno"
      message="Aguarde enquanto a nota é validada e enviada à SEFAZ."
      :steps="passosTransmissao"
      :current-step="overlayPasso"
      :error="overlayErro"
    />

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.devolucao-page { padding-bottom: 40px; }
.nfe-status-bar {
  display: flex; align-items: center; flex-wrap: wrap; gap: 12px;
  padding: 12px 18px; margin-bottom: 16px;
}
.nfe-status-label { font-size: 13px; font-weight: 600; color: var(--text-secondary); }
.nfe-status-meta { font-size: 13px; color: var(--text-secondary); }
.nfe-status-chave { font-size: 12px; color: var(--text-muted); font-family: monospace; word-break: break-all; }
</style>
