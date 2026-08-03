<script setup lang="ts">
/**
 * Emissão de NF-e Simplificada (fatia 8).
 *
 * Porta o comportamento do legado `pages/vendas/emissao/nfe-simplificada/[[id]].vue`
 * para o design system novo (sem Vuetify). Fluxo:
 *   1. Seleciona cliente/destinatário (obrigatório para modelo 55).
 *   2. Adiciona itens (produto + qtd + valor + desconto).
 *   3. Informa pagamentos (deve cobrir o total).
 *   4. Grava a venda em `vendas` (POST novo / PUT edição) e transmite a NF-e
 *      em `vendas-fiscal/{id}/nfe/transmitir`.
 *   5. Acompanha o progresso via SignalR (`useRealtime`) no TransmissionOverlay e,
 *      ao concluir, exibe a DANFE no DanfeViewer.
 *
 * IO exclusivamente por `useApi`; UI por componentes compartilhados + locais da fatia.
 */
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados, type CommandResult } from '~/composables/useApi'
import { useTenant } from '~/composables/useTenant'
import { useHelper } from '~/composables/useHelper'
import { useToast } from '~/composables/useToast'
import { useRealtime } from '~/composables/useRealtime'

import PageToolbar from '~/components/shared/PageToolbar.vue'
import DanfeViewer from '~/components/shared/DanfeViewer.vue'
import TransmissionOverlay, { type TransmissionStep } from '~/components/shared/TransmissionOverlay.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'

import ClienteCard from '~/components/vendas-nfe-simplificada/ClienteCard.vue'
import ItemForm from '~/components/vendas-nfe-simplificada/ItemForm.vue'
import ItemLista from '~/components/vendas-nfe-simplificada/ItemLista.vue'
import PagamentoPanel from '~/components/vendas-nfe-simplificada/PagamentoPanel.vue'
import {
  novaNfeSimplificada,
  MODELO_FISCAL_NFE,
  type ItemVenda,
  type NfeSimplificada,
  type VendaGravarBody
} from '~/components/vendas-nfe-simplificada/tipos'

definePageMeta({ layout: 'default', middleware: 'auth' })

const route = useRoute()
const router = useRouter()
const { empresaId } = useTenant()
const { formatarMoeda } = useHelper()
const toast = useToast()
const { conectar, desconectar } = useRealtime('/hubs/vendas')

const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()
const itemFormRef = ref<InstanceType<typeof ItemForm>>()

const nfe = reactive<NfeSimplificada>(novaNfeSimplificada())

const carregando = ref(false)
const gravando = ref(false)

// Overlay de transmissão
const mostrarOverlay = ref(false)
const passoAtual = ref(0)
const erroOverlay = ref<string | null>(null)
const passos: TransmissionStep[] = [
  { text: 'Gravando a venda' },
  { text: 'Validando dados da NF-e' },
  { text: 'Transmitindo à SEFAZ' },
  { text: 'Processando retorno' }
]

// DANFE resultante
const danfeSrc = ref<Blob | string | null>(null)
const chaveEmitida = ref<string | null>(null)

const round2 = (v: number) => Math.round((v + Number.EPSILON) * 100) / 100

const totalItens = computed(() =>
  nfe.itens.reduce((acc, item) => {
    const base = item.quantidadeComercial * item.valorUnitarioComercial - item.valorDesconto
    return acc + (base > 0 ? round2(base) : 0)
  }, 0)
)

const totalFinal = computed(() => round2(totalItens.value))

const totalPagamentos = computed(() =>
  nfe.pagamentos.reduce((acc, p) => acc + p.valorPagamento, 0)
)

const semItens = computed(() => nfe.itens.length === 0)

const modoEdicao = computed(() => !!route.params.id)

// #region Itens
function adicionarItem(item: ItemVenda) {
  nfe.itens.unshift(item)
}

function editarItem(indice: number) {
  const item = nfe.itens[indice]
  if (!item) return
  nfe.itens.splice(indice, 1)
  itemFormRef.value?.carregarParaEdicao(item)
}

function removerItem(indice: number) {
  nfe.itens.splice(indice, 1)
}
// #endregion

// #region Validação
function validar(): string | null {
  if (nfe.itens.length === 0) return 'Adicione ao menos um item.'
  if (!nfe.destinatario.pessoaId) return 'Selecione o cliente/destinatário (obrigatório para NF-e).'
  if (!nfe.destinatario.documentoConsumidor) return 'Informe o documento do consumidor.'
  if (nfe.pagamentos.length === 0) return 'Informe ao menos uma forma de pagamento.'
  if (round2(totalPagamentos.value) < totalFinal.value) return 'O total dos pagamentos é menor que o total da nota.'
  return null
}
// #endregion

// #region Montagem do corpo
function montarBody(vendaId: number | null): VendaGravarBody {
  const body: VendaGravarBody = {
    modeloFiscal: MODELO_FISCAL_NFE,
    emitente: { empresaId: empresaId.value },
    destinatario: {
      pessoaId: nfe.destinatario.pessoaId,
      documentoConsumidor: nfe.destinatario.documentoConsumidor,
      enviarDestinatarioNaNfe: nfe.destinatario.enviarDestinatarioNaNfe
    },
    itens: nfe.itens.map((item) => ({
      produtoId: item.produtoId,
      quantidadeComercial: item.quantidadeComercial,
      valorUnitarioComercial: item.valorUnitarioComercial,
      valorDesconto: item.valorDesconto
    })),
    pagamentos: nfe.pagamentos.map((p) => ({ ...p })),
    total: {
      valorFrete: 0,
      valorDesconto: 0,
      valorOutro: 0
    },
    informacoesComplementares: nfe.informacoesComplementares.replace(/\n/g, ';'),
    informacoesAdicionaisFisco: nfe.informacoesAdicionaisFisco
  }
  if (vendaId != null) body.id = vendaId
  return body
}
// #endregion

// #region Gravar + Transmitir
interface RespostaTransmissao {
  nfe?: { chave?: string; numero?: number }
  chave?: string
}

async function gravarETransmitir() {
  const erro = validar()
  if (erro) {
    toast.warning(erro)
    return
  }

  gravando.value = true
  erroOverlay.value = null
  passoAtual.value = 0
  mostrarOverlay.value = true
  danfeSrc.value = null

  // Conecta o hub para acompanhar o progresso em tempo real (best-effort).
  await conectar({
    NfeSimplificadaProgresso: (...args: unknown[]) => {
      const passo = Number(args[0])
      if (!Number.isNaN(passo)) passoAtual.value = Math.min(passo, passos.length - 1)
    }
  })

  try {
    const idRota = route.params.id ? Number(route.params.id) : null
    const idPersistido = nfe.id != null ? Number(nfe.id) : null
    const idAtualizacao = (idRota && idRota > 0 ? idRota : null) ?? idPersistido

    // 1. Grava a venda (POST novo / PUT edição).
    passoAtual.value = 0
    let vendaId: number | null = idAtualizacao
    if (idAtualizacao != null) {
      await useApi.put<CommandResult>('/vendas/{id}', montarBody(idAtualizacao), {
        params: { id: idAtualizacao }
      })
    } else {
      const respGravar = await useApi.post<CommandResult<{ id?: number }>>(
        '/vendas',
        montarBody(null)
      )
      const dados = extrairDados<{ id?: number }>(respGravar)
      vendaId = dados?.id ?? null
      if (vendaId != null) nfe.id = vendaId
    }

    if (vendaId == null || Number.isNaN(vendaId)) {
      throw new Error('Não foi possível identificar a venda para transmitir.')
    }

    // 2. Transmite a NF-e.
    passoAtual.value = 2
    const respTx = await useApi.post<CommandResult<RespostaTransmissao>>(
      '/vendas-fiscal/{id}/nfe/transmitir',
      {},
      { params: { id: vendaId } }
    )
    if (respTx.sucesso === false) {
      throw new Error(respTx.mensagem || 'Falha ao transmitir a NF-e.')
    }

    // 3. Retorno + DANFE.
    passoAtual.value = 3
    const dadosTx = extrairDados<RespostaTransmissao>(respTx)
    const chave = dadosTx?.nfe?.chave ?? dadosTx?.chave ?? null
    chaveEmitida.value = chave

    if (chave) {
      await baixarDanfe(vendaId, chave)
    }

    mostrarOverlay.value = false
    toast.success('NF-e simplificada emitida com sucesso!')
  } catch (e) {
    const mensagem = e instanceof Error ? e.message : 'Erro ao emitir a NF-e simplificada.'
    erroOverlay.value = mensagem
    console.error('[nfe-simplificada.gravarETransmitir]', e)
  } finally {
    gravando.value = false
    await desconectar()
  }
}

/** Baixa a DANFE emitida (PDF) para exibir no visualizador. */
async function baixarDanfe(vendaId: number, chave: string) {
  try {
    const blob = await useApi<Blob>('/vendas-fiscal/{id}/nfe', {
      params: { id: vendaId },
      query: { chave },
      responseType: 'blob'
    })
    if (blob instanceof Blob && blob.size > 0) {
      danfeSrc.value = blob
    }
  } catch (e) {
    console.error('[nfe-simplificada.baixarDanfe]', e)
    toast.warning('NF-e emitida, mas não foi possível carregar a DANFE.')
  }
}
// #endregion

// #region Carregamento (edição)
interface VendaCarregada {
  id?: number
  statusSefaz?: number
  destinatario?: { pessoaId?: number; cpf?: string; cnpj?: string; razaoSocial?: string; documentoConsumidor?: string }
  itens?: Array<{
    produtoId: number
    codigoProduto?: string
    descricaoProduto?: string
    quantidadeComercial: number
    valorUnitarioComercial: number
    valorDesconto?: number
  }>
  pagamentos?: Array<{ id?: number; tipoPagamento: number; valorPagamento: number; valorTroco?: number }>
  informacoesComplementares?: string
  informacoesAdicionaisFisco?: string
}

async function carregarVenda(id: number) {
  carregando.value = true
  try {
    const resposta = await useApi<CommandResult<VendaCarregada>>('/vendas/{id}', { params: { id } })
    const venda = extrairDados<VendaCarregada>(resposta)
    if (!venda) {
      toast.warning('Venda não encontrada.')
      return
    }
    nfe.id = id
    nfe.statusSefaz = venda.statusSefaz
    nfe.destinatario = {
      pessoaId: venda.destinatario?.pessoaId ?? 0,
      documentoConsumidor:
        venda.destinatario?.documentoConsumidor ||
        venda.destinatario?.cnpj ||
        venda.destinatario?.cpf ||
        '',
      descricao: venda.destinatario?.razaoSocial ?? '',
      enviarDestinatarioNaNfe: true
    }
    nfe.itens = (venda.itens ?? []).map((item) => ({
      produtoId: item.produtoId,
      produto: {
        id: item.produtoId,
        codigo: item.codigoProduto,
        descricao: item.descricaoProduto
      },
      descricao: item.descricaoProduto || `Produto #${item.produtoId}`,
      quantidadeComercial: item.quantidadeComercial,
      valorUnitarioComercial: item.valorUnitarioComercial,
      valorDesconto: item.valorDesconto ?? 0
    }))
    nfe.pagamentos = (venda.pagamentos ?? []).map((p) => ({
      id: p.id,
      tipoPagamento: p.tipoPagamento,
      valorPagamento: p.valorPagamento,
      valorTroco: p.valorTroco
    }))
    nfe.informacoesComplementares = venda.informacoesComplementares ?? ''
    nfe.informacoesAdicionaisFisco = venda.informacoesAdicionaisFisco ?? ''
  } catch (e) {
    toast.error('Erro ao carregar a venda.')
    console.error('[nfe-simplificada.carregarVenda]', e)
  } finally {
    carregando.value = false
  }
}
// #endregion

// #region Ações auxiliares
async function limparCaixa() {
  const ok = await confirmRef.value?.open(
    'Cancelar venda',
    'Deseja descartar os dados da venda atual?',
    { danger: true, textoConfirmar: 'Descartar' }
  )
  if (!ok) return
  Object.assign(nfe, novaNfeSimplificada())
  itemFormRef.value?.limpar()
  danfeSrc.value = null
  chaveEmitida.value = null
  if (route.params.id) await router.push('/erp/vendas/emissao/nfe-simplificada')
}

function fecharOverlay() {
  mostrarOverlay.value = false
  erroOverlay.value = null
}

function imprimirDanfe() {
  if (danfeSrc.value instanceof Blob) {
    const url = URL.createObjectURL(danfeSrc.value)
    window.open(url, '_blank')
  } else if (typeof danfeSrc.value === 'string') {
    window.open(danfeSrc.value, '_blank')
  }
}

function baixarDanfeArquivo() {
  if (!(danfeSrc.value instanceof Blob)) return
  const url = URL.createObjectURL(danfeSrc.value)
  const link = document.createElement('a')
  link.href = url
  link.download = `${chaveEmitida.value || 'danfe'}.pdf`
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}
// #endregion

onMounted(async () => {
  const id = route.params.id ? Number(route.params.id) : null
  if (id && !Number.isNaN(id)) await carregarVenda(id)
})

onUnmounted(() => {
  desconectar()
})
</script>

<template>
  <div class="nfe-simplificada">
    <PageToolbar
      :title="modoEdicao ? 'Editar NF-e Simplificada' : 'Emissão de NF-e Simplificada'"
      subtitle="Nota fiscal eletrônica (modelo 55) — fluxo simplificado"
      :loading="carregando"
    >
      <template #actions>
        <button type="button" class="btn btn-ghost" @click="limparCaixa">Cancelar</button>
        <button
          type="button"
          class="btn btn-success"
          :disabled="gravando || semItens"
          @click="gravarETransmitir"
        >
          {{ gravando ? 'Emitindo...' : 'Gravar e transmitir' }}
        </button>
      </template>
    </PageToolbar>

    <div class="ns-grid">
      <!-- Coluna esquerda: entradas -->
      <div class="ns-coluna">
        <ClienteCard v-model:destinatario="nfe.destinatario" />
        <ItemForm ref="itemFormRef" @adicionar-item="adicionarItem" />
        <PagamentoPanel
          v-model:pagamentos="nfe.pagamentos"
          :total-final="totalFinal"
          :bloqueado="semItens"
        />
      </div>

      <!-- Coluna direita: itens + total + DANFE -->
      <div class="ns-coluna">
        <ItemLista :itens="nfe.itens" @editar="editarItem" @remover="removerItem" />

        <div class="ns-total glass-panel">
          <span class="ns-total-label">Total da nota</span>
          <span class="ns-total-valor">{{ formatarMoeda(totalFinal) }}</span>
        </div>

        <DanfeViewer
          v-if="danfeSrc"
          :src="danfeSrc"
          title="DANFE — NF-e Simplificada"
          height="60vh"
          @print="imprimirDanfe"
          @download="baixarDanfeArquivo"
        />
      </div>
    </div>

    <TransmissionOverlay
      v-model="mostrarOverlay"
      title="Transmitindo NF-e Simplificada"
      :steps="passos"
      :current-step="passoAtual"
      :error="erroOverlay"
      @close="fecharOverlay"
    />

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.nfe-simplificada { display: flex; flex-direction: column; gap: 16px; }
.ns-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; align-items: start; }
.ns-coluna { display: flex; flex-direction: column; gap: 16px; }
.ns-total {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 18px;
}
.ns-total-label { font-size: 15px; color: var(--text-secondary); }
.ns-total-valor { font-size: 28px; font-weight: 700; }
@media (max-width: 960px) {
  .ns-grid { grid-template-columns: 1fr; }
}
</style>
