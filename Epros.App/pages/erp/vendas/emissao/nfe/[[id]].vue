<script setup lang="ts">
/**
 * Emissão de NF-e (fatia 7) — `erp/vendas/emissao/nfe/[[id]]`.
 *
 * Porta o comportamento da tela legada `vendas/emissao/nfe/[[id]].vue` para o design system novo:
 * dados básicos, destinatário, produtos, cálculo/totais, transporte, recebimentos e informações
 * complementares; ações de salvar rascunho, transmitir (com overlay + realtime), cancelar,
 * carta de correção, notas referenciadas e pré-visualização da DANFE.
 *
 * Sem parâmetro (`/nfe`) inicia uma nota nova; com um GUID (`/nfe/<id>`) carrega a venda fiscal.
 * Toda a lógica de IO e estado vive em `useNfeEmissao`; esta página é orquestração de UI.
 *
 * Endpoints: vendas, vendas-fiscal/{id}/nfe(+transmitir/cancelar/carta-correcao/referenciadas),
 * cfops, tipos-operacoes-fiscais.
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
import NfeCartaCorrecaoDialog from '~/components/vendas-nfe/NfeCartaCorrecaoDialog.vue'
import NfeTransmissoesDialog from '~/components/vendas-nfe/NfeTransmissoesDialog.vue'
import { useNfeEmissao } from '~/components/vendas-nfe/useNfeEmissao'
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
  cancelarNfe,
  registrarCartaCorrecao,
  salvarReferenciadas,
  gerarDanfe,
  baixarDanfe,
  imprimirDanfe,
  adicionarItem,
  atualizarItem,
  removerItem
} = useNfeEmissao()

// --- Estado de UI (diálogos) ---
const mostrarProdutoDialog = ref(false)
const itemEmEdicao = ref<NfeItem | null>(null)
const mostrarReferenciadas = ref(false)
const mostrarCartaCorrecao = ref(false)
const mostrarTransmissoes = ref(false)
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const titulo = computed(() =>
  nfe.id ? `Edição de NF-e ${nfe.numero ? `#${nfe.numero}` : ''}`.trim() : 'Emissão de NF-e'
)
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

// --- Troca de destinatário bloqueada (remove itens) ---
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
  await salvarRascunho()
}

async function onTransmitir() {
  await transmitir()
}

async function onCancelarNfe() {
  const ok = await confirmRef.value?.open(
    'Cancelar NF-e',
    'Tem certeza que deseja cancelar esta NF-e autorizada? A operação é irreversível.',
    { danger: true, textoConfirmar: 'Cancelar NF-e' }
  )
  if (!ok) return
  await cancelarNfe('Cancelamento solicitado pelo emitente')
}

async function onNovaNota() {
  const ok = await confirmRef.value?.open(
    'Nova nota',
    'Descartar a nota atual e iniciar uma nova? Alterações não salvas serão perdidas.'
  )
  if (ok) router.push('/erp/vendas/emissao/nfe')
}

function abrirTransmissao(id: string) {
  router.push(`/erp/vendas/emissao/nfe/${id}`)
}

async function onRegistrarCartaCorrecao(texto: string) {
  const ok = await registrarCartaCorrecao(texto)
  if (ok) mostrarCartaCorrecao.value = false
}

function onConfirmarReferenciadas(chaves: string[]) {
  void salvarReferenciadas(chaves)
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
    await carregarVenda(idParam.value)
  }
  window.addEventListener('keydown', aoTeclar)
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', aoTeclar)
})
</script>

<template>
  <div class="nfe-page">
    <PageToolbar :title="titulo" subtitle="Vendas · Emissão de NF-e" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-ghost" @click="mostrarTransmissoes = true">Vendas</button>
        <button v-if="nfe.id && emitida" type="button" class="btn btn-ghost" @click="mostrarCartaCorrecao = true">
          Carta de correção
        </button>
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
          v-if="emitida"
          type="button"
          class="btn btn-danger"
          :disabled="salvando"
          @click="onCancelarNfe"
        >
          Cancelar NF-e
        </button>
        <template v-else>
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

    <!-- Faixa de status quando a nota está autorizada/cancelada -->
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

    <NfeCartaCorrecaoDialog
      v-model="mostrarCartaCorrecao"
      :numero="nfe.numero"
      :serie="nfe.serie"
      :chave="nfe.chave"
      :loading="salvando"
      @registrar="onRegistrarCartaCorrecao"
    />

    <NfeTransmissoesDialog
      v-model="mostrarTransmissoes"
      @abrir="abrirTransmissao"
    />

    <!-- DANFE -->
    <AppDialog v-model="danfeVisivel" title="Pré-visualização da DANFE" width="90%">
      <DanfeViewer
        :src="danfeSrc"
        title="DANFE"
        @download="baixarDanfe"
        @print="imprimirDanfe"
      />
    </AppDialog>

    <!-- Overlay de transmissão em tempo real -->
    <TransmissionOverlay
      v-model="overlayVisivel"
      title="Transmitindo NF-e"
      message="Aguarde enquanto a nota é validada e enviada à SEFAZ."
      :steps="passosTransmissao"
      :current-step="overlayPasso"
      :error="overlayErro"
    />

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.nfe-page { padding-bottom: 40px; }
.nfe-status-bar {
  display: flex; align-items: center; flex-wrap: wrap; gap: 12px;
  padding: 12px 18px; margin-bottom: 16px;
}
.nfe-status-label { font-size: 13px; font-weight: 600; color: var(--text-secondary); }
.nfe-status-meta { font-size: 13px; color: var(--text-secondary); }
.nfe-status-chave { font-size: 12px; color: var(--text-muted); font-family: monospace; word-break: break-all; }
</style>
