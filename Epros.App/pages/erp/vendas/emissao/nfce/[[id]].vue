<script setup lang="ts">
/**
 * Emissão de NFC-e (modelo 65) — pages/erp/vendas/emissao/nfce/[[id]].vue
 *
 * Porta o COMPORTAMENTO de `vendas/emissao/nfce/[[id]].vue` do legado (fluxo PDV sob layouts
 * pos/pos2) para o formato de tela ERP com o design system próprio (glass/CSS vars), sem Vuetify.
 *
 * Fluxo:
 *  - Rota sem id  -> nova NFC-e (rascunho).
 *  - Rota com id  -> carrega a venda existente para edição/retransmissão.
 *  - Itens: adicionar/editar/remover (NfceItemDialog + NfceItensTable).
 *  - Consumidor: CPF/CNPJ opcional (NfceClientePanel).
 *  - Pagamentos: formas + saldo/troco (NfcePagamentosPanel).
 *  - Salvar rascunho: POST/PUT em vendas-fiscal (nfce). Salvar+transmitir: + .../nfce/transmitir.
 *  - Progresso de transmissão via TransmissionOverlay ligado ao useRealtime (hub de vendas).
 *  - Sucesso: exibe a DANFE (DanfeViewer) e permite iniciar nova venda.
 *  - Cancelamento fiscal (após transmitido): .../nfce/cancelar.
 *
 * IO exclusivamente por useApi (baseURL do runtimeConfig). Endpoints (api/v1):
 *   POST   /vendas-fiscal/{id}/nfce                 (grava a NFC-e da venda)
 *   POST   /vendas-fiscal/{id}/nfce/transmitir      (transmite à SEFAZ)
 *   POST   /vendas-fiscal/{id}/nfce/cancelar        (cancela documento transmitido)
 *   GET    /vendas/{id}                             (carrega a venda para edição)
 */
import { computed, onMounted, onBeforeUnmount, reactive, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useTenant } from '~/composables/useTenant'
import { useHelper } from '~/composables/useHelper'
import { useRealtime } from '~/composables/useRealtime'
import { useMask } from '~/composables/useMask'
import { useDocumento } from '~/composables/useDocumento'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import TransmissionOverlay, { type TransmissionStep } from '~/components/shared/TransmissionOverlay.vue'
import DanfeViewer from '~/components/shared/DanfeViewer.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import NfceItensTable from '~/components/vendas-nfce/NfceItensTable.vue'
import NfceItemDialog from '~/components/vendas-nfce/NfceItemDialog.vue'
import NfceClientePanel from '~/components/vendas-nfce/NfceClientePanel.vue'
import NfcePagamentosPanel from '~/components/vendas-nfce/NfcePagamentosPanel.vue'
import {
  criarNfceInicial,
  StatusNfce,
  ModeloFiscal,
  type Nfce,
  type NfceItem,
  type NfceResultado
} from '~/components/vendas-nfce/types'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { empresaId } = useTenant()
const { formatarMoeda } = useHelper()
const { somenteDigitos } = useMask()
const { validarCpfCnpj } = useDocumento()

// --- Identificação da rota (id opcional)
const idParam = computed(() => {
  const p = route.params.id
  const bruto = Array.isArray(p) ? p[0] : p
  const n = bruto ? Number(bruto) : Number.NaN
  return !Number.isNaN(n) && n > 0 ? n : null
})
const isEdicao = computed(() => idParam.value != null)

// --- Estado do documento
const nfce = reactive<Nfce>(criarNfceInicial(empresaId.value))
const descontoTotal = ref(0)
const acrescimoTotal = ref(0)

const carregando = ref(false)
const salvando = ref(false)

// --- Diálogo de item
const dialogItemAberto = ref(false)
const itemEmEdicao = ref<NfceItem | null>(null)
const indiceEmEdicao = ref<number | null>(null)

// --- Overlay de transmissão
const overlayVisivel = ref(false)
const overlayErro = ref<string | null>(null)
const passoAtual = ref(0)
const passosTransmissao: TransmissionStep[] = [
  { text: 'Validando a venda' },
  { text: 'Gerando o XML da NFC-e' },
  { text: 'Assinando digitalmente' },
  { text: 'Transmitindo à SEFAZ' },
  { text: 'Processando retorno' }
]

// --- Resultado / DANFE
const dialogDanfeAberto = ref(false)
const resultado = ref<NfceResultado>({ vendaId: null, numero: null, chave: null, url: null })
const danfeSrc = ref<string | Blob | null>(null)

const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

// --- Realtime (progresso de transmissão em tempo real)
const realtime = useRealtime('/hubs/vendas')

// #region Computed / totais
const round2 = (v: number) => Math.round((v + Number.EPSILON) * 100) / 100

const totalItens = computed(() =>
  nfce.itens.reduce((acc, item) => {
    const base = item.quantidadeComercial * (item.valorUnitarioComercial ?? 0)
    return acc + round2(base - item.valorDesconto)
  }, 0)
)

const totalFinal = computed(() =>
  round2(totalItens.value + acrescimoTotal.value - descontoTotal.value)
)

const semItens = computed(() => nfce.itens.length === 0)

const jaTransmitido = computed(() => nfce.status === StatusNfce.TRANSMITIDO || !!resultado.value.chave)

const totalPago = computed(() => nfce.pagamentos.reduce((acc, p) => acc + p.valorPagamento, 0))
const saldoRestante = computed(() => {
  const s = round2(totalFinal.value - totalPago.value)
  return s > 0 ? s : 0
})

const tituloPagina = computed(() =>
  isEdicao.value ? `NFC-e — Venda #${idParam.value}` : 'Nova NFC-e'
)
// #endregion

// #region Itens
function abrirNovoItem() {
  itemEmEdicao.value = null
  indiceEmEdicao.value = null
  dialogItemAberto.value = true
}

function editarItem(index: number) {
  itemEmEdicao.value = { ...nfce.itens[index] }
  indiceEmEdicao.value = index
  dialogItemAberto.value = true
}

function removerItem(index: number) {
  nfce.itens.splice(index, 1)
}

function aoConfirmarItem(item: NfceItem) {
  if (indiceEmEdicao.value != null) {
    nfce.itens.splice(indiceEmEdicao.value, 1, item)
  } else {
    nfce.itens.unshift(item)
  }
  itemEmEdicao.value = null
  indiceEmEdicao.value = null
}
// #endregion

// #region Validações
/** Reproduz as validações do legado (itens, pagamentos e documento do consumidor). */
function validar(transmitir: boolean): string | null {
  if (semItens.value) return 'Adicione ao menos um item à venda.'

  if (transmitir) {
    if (nfce.pagamentos.length === 0) return 'Informe ao menos uma forma de pagamento.'
    if (saldoRestante.value > 0) {
      return `Pagamentos insuficientes. Saldo restante: ${formatarMoeda(saldoRestante.value)}.`
    }
  }

  const doc = somenteDigitos(nfce.destinatario.documentoConsumidor)
  if (doc) {
    if (doc.length !== 11 && doc.length !== 14) return 'CPF/CNPJ do consumidor incompleto.'
    if (!validarCpfCnpj(doc)) return 'CPF/CNPJ do consumidor inválido.'
  }
  return null
}
// #endregion

// #region Persistência
/** Monta o corpo enviado à API (padrão do backend de vendas-fiscal). */
function montarBody() {
  return {
    id: idParam.value ?? undefined,
    empresaId: empresaId.value,
    modeloFiscal: ModeloFiscal.NFCe,
    destinatario: {
      pessoaId: nfce.destinatario.pessoaId,
      documentoConsumidor: somenteDigitos(nfce.destinatario.documentoConsumidor) || null,
      enviarDestinatarioNaNfce: nfce.destinatario.enviarDestinatarioNaNfce,
      descricao: nfce.destinatario.descricao || null
    },
    itens: nfce.itens.map((item) => ({
      produtoId: item.produtoId,
      quantidadeComercial: item.quantidadeComercial,
      valorUnitarioComercial: item.valorUnitarioComercial,
      valorDesconto: item.valorDesconto
    })),
    pagamentos: nfce.pagamentos.map((p) => ({
      tipoPagamento: p.tipoPagamento,
      valorPagamento: p.valorPagamento,
      valorTroco: p.valorTroco
    })),
    total: {
      valorDesconto: descontoTotal.value,
      valorAcrescimo: acrescimoTotal.value
    },
    informacoesComplementares: nfce.informacoesComplementares || null,
    informacoesAdicionaisFisco: nfce.informacoesAdicionaisFisco || null
  }
}

/**
 * Grava a NFC-e da venda em vendas-fiscal.
 * Usa a rota `/vendas-fiscal/{id}/nfce`; para venda nova, o backend cria a venda a partir
 * do corpo (id 0 = criar). Retorna o id da venda gravada.
 */
async function gravar(): Promise<number | null> {
  const idAlvo = idParam.value ?? 0
  const resposta = await useApi<Record<string, unknown>>('/vendas-fiscal/{id}/nfce', {
    method: 'POST',
    params: { id: idAlvo },
    body: montarBody()
  })
  const dados = extrairDados<Record<string, unknown>>(resposta) ?? {}
  const vendaId =
    (dados.vendaId as number | undefined) ??
    (dados.id as number | undefined) ??
    idParam.value ??
    null
  return vendaId != null ? Number(vendaId) : null
}

/** Extrai chave/número/url da resposta de transmissão. */
function extrairResultado(resposta: unknown, vendaIdFallback: number | null): NfceResultado {
  const dados = (extrairDados<Record<string, unknown>>(resposta) ?? {}) as Record<string, unknown>
  const nfceNode = (dados.nfce as Record<string, unknown> | undefined) ?? dados
  const chave =
    (nfceNode.chave as string | undefined) ??
    (nfceNode.chaveAcesso as string | undefined) ??
    (dados.chaveAcesso as string | undefined) ??
    null
  const numero = (nfceNode.numero as number | undefined) ?? (dados.numero as number | undefined) ?? null
  const url =
    (nfceNode.url as string | undefined) ??
    (nfceNode.urlDanfe as string | undefined) ??
    (dados.urlDanfe as string | undefined) ??
    null
  const vendaId =
    (dados.vendaId as number | undefined) ?? (dados.id as number | undefined) ?? vendaIdFallback
  return {
    chave: chave && String(chave).length ? String(chave) : null,
    numero: numero != null ? Number(numero) : null,
    url: url && String(url).length ? String(url) : null,
    vendaId: vendaId != null ? Number(vendaId) : null
  }
}

/** Salva a venda como rascunho (sem transmitir). */
async function salvarRascunho() {
  const erro = validar(false)
  if (erro) {
    toast.warning(erro)
    return
  }
  salvando.value = true
  try {
    const vendaId = await gravar()
    nfce.status = StatusNfce.SALVAR
    toast.success('Venda salva com sucesso!')
    if (vendaId && !isEdicao.value) {
      await router.replace(`/erp/vendas/emissao/nfce/${vendaId}`)
    }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

/** Salva e transmite a NFC-e à SEFAZ, com overlay de progresso. */
async function salvarTransmitir() {
  const erro = validar(true)
  if (erro) {
    toast.warning(erro)
    return
  }

  salvando.value = true
  overlayErro.value = null
  passoAtual.value = 0
  overlayVisivel.value = true

  // Conecta ao hub para receber o progresso; se indisponível, o overlay auto-avança.
  await realtime.conectar({
    NfceTransmissionStep: (...args: unknown[]) => {
      const idx = Number(args[0])
      if (!Number.isNaN(idx)) passoAtual.value = idx
    }
  })

  try {
    passoAtual.value = 0
    const vendaId = await gravar()
    if (vendaId == null) {
      throw new Error('Não foi possível obter o identificador da venda para transmitir.')
    }
    if (!isEdicao.value) {
      // Mantém a URL alinhada à venda gravada (permite retransmitir pela lista/refresh).
      await router.replace(`/erp/vendas/emissao/nfce/${vendaId}`)
    }

    passoAtual.value = 1
    const respostaTx = await useApi<Record<string, unknown>>('/vendas-fiscal/{id}/nfce/transmitir', {
      method: 'POST',
      params: { id: vendaId }
    })

    passoAtual.value = passosTransmissao.length - 1
    const res = extrairResultado(respostaTx, vendaId)
    resultado.value = res
    nfce.id = res.vendaId
    nfce.status = StatusNfce.TRANSMITIDO

    overlayVisivel.value = false
    toast.success(
      res.numero ? `NFC-e nº ${res.numero} transmitida com sucesso!` : 'NFC-e transmitida com sucesso!'
    )
    await carregarDanfe(res)
    dialogDanfeAberto.value = true
  } catch (e) {
    overlayErro.value = obterMensagemErro(e)
    toast.error(
      'A venda foi salva, mas a NFC-e não foi transmitida. Corrija o erro e transmita novamente.'
    )
  } finally {
    salvando.value = false
    await realtime.desconectar()
  }
}

/** Carrega o PDF da DANFE (quando a API expõe uma URL ou endpoint de impressão). */
async function carregarDanfe(res: NfceResultado) {
  danfeSrc.value = null
  if (res.url) {
    danfeSrc.value = res.url
    return
  }
  if (res.vendaId == null) return
  try {
    const blob = await useApi<Blob>('/vendas-fiscal/{id}/nfce', {
      params: { id: res.vendaId },
      query: { formato: 'pdf' },
      responseType: 'blob'
    })
    if (blob instanceof Blob) danfeSrc.value = blob
  } catch (e) {
    // DANFE indisponível não bloqueia o fluxo; apenas registra.
    console.error('[nfce/[[id]]] erro ao carregar DANFE', e)
  }
}

/** Cancela o documento fiscal já transmitido (exige justificativa). */
async function cancelarDocumento() {
  if (!jaTransmitido.value || resultado.value.vendaId == null) return
  const ok = await confirmRef.value?.open(
    'Cancelar NFC-e',
    'Esta ação solicita o cancelamento do documento fiscal na SEFAZ e não pode ser desfeita. Deseja continuar?',
    { danger: true, textoConfirmar: 'Cancelar NFC-e', textoCancelar: 'Voltar' }
  )
  if (!ok) return

  salvando.value = true
  try {
    await useApi('/vendas-fiscal/{id}/nfce/cancelar', {
      method: 'POST',
      params: { id: resultado.value.vendaId },
      body: { justificativa: 'Cancelamento solicitado pelo operador.' }
    })
    toast.success('Solicitação de cancelamento enviada com sucesso.')
    nfce.status = StatusNfce.SALVAR
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
// #endregion

// #region Ações de UI
/** Descarta a venda em edição e volta ao estado inicial. */
async function cancelarVenda() {
  if (!semItens.value || nfce.pagamentos.length) {
    const ok = await confirmRef.value?.open('Cancelar venda', 'Deseja descartar a venda atual?', {
      textoConfirmar: 'Descartar',
      textoCancelar: 'Voltar'
    })
    if (!ok) return
  }
  Object.assign(nfce, criarNfceInicial(empresaId.value))
  descontoTotal.value = 0
  acrescimoTotal.value = 0
  resultado.value = { vendaId: null, numero: null, chave: null, url: null }
  danfeSrc.value = null
  if (isEdicao.value) await router.replace('/erp/vendas/emissao/nfce')
}

/** Após transmitir e visualizar a DANFE, inicia uma nova venda. */
async function novaVenda() {
  dialogDanfeAberto.value = false
  Object.assign(nfce, criarNfceInicial(empresaId.value))
  descontoTotal.value = 0
  acrescimoTotal.value = 0
  resultado.value = { vendaId: null, numero: null, chave: null, url: null }
  danfeSrc.value = null
  await router.replace('/erp/vendas/emissao/nfce')
}

function imprimirDanfe() {
  if (typeof danfeSrc.value === 'string') window.open(danfeSrc.value, '_blank')
}

function baixarDanfe() {
  if (typeof danfeSrc.value === 'string') {
    const a = document.createElement('a')
    a.href = danfeSrc.value
    a.download = `nfce-${resultado.value.numero ?? 'documento'}.pdf`
    a.click()
  } else if (danfeSrc.value instanceof Blob) {
    const url = URL.createObjectURL(danfeSrc.value)
    const a = document.createElement('a')
    a.href = url
    a.download = `nfce-${resultado.value.numero ?? 'documento'}.pdf`
    a.click()
    URL.revokeObjectURL(url)
  }
}
// #endregion

// #region Carregamento inicial
interface VendaCarregada {
  id?: number
  status?: number
  statusSefaz?: number
  destinatario?: Partial<Nfce['destinatario']>
  itens?: NfceItem[]
  pagamentos?: Nfce['pagamentos']
  total?: { valorDesconto?: number; valorAcrescimo?: number }
  nfce?: { chave?: string; numero?: number; url?: string }
  informacoesComplementares?: string
  informacoesAdicionaisFisco?: string
}

async function carregarVenda() {
  if (!isEdicao.value || idParam.value == null) return
  carregando.value = true
  try {
    const resposta = await useApi(`/vendas/{id}`, { params: { id: idParam.value } })
    const venda = extrairDados<VendaCarregada>(resposta)
    if (!venda) return

    nfce.id = venda.id ?? idParam.value
    nfce.status = (venda.status as StatusNfce) ?? StatusNfce.SALVAR
    nfce.statusSefaz = venda.statusSefaz ?? null
    if (venda.destinatario) {
      nfce.destinatario = {
        pessoaId: venda.destinatario.pessoaId ?? null,
        documentoConsumidor: venda.destinatario.documentoConsumidor ?? '',
        enviarDestinatarioNaNfce: venda.destinatario.enviarDestinatarioNaNfce ?? false,
        descricao: venda.destinatario.descricao ?? ''
      }
    }
    nfce.itens = venda.itens ?? []
    nfce.pagamentos = venda.pagamentos ?? []
    descontoTotal.value = venda.total?.valorDesconto ?? 0
    acrescimoTotal.value = venda.total?.valorAcrescimo ?? 0
    nfce.informacoesComplementares = venda.informacoesComplementares ?? ''
    nfce.informacoesAdicionaisFisco = venda.informacoesAdicionaisFisco ?? ''

    if (venda.nfce?.chave) {
      resultado.value = {
        vendaId: nfce.id ?? null,
        chave: venda.nfce.chave,
        numero: venda.nfce.numero ?? null,
        url: venda.nfce.url ?? null
      }
      nfce.status = StatusNfce.TRANSMITIDO
    }
  } catch (e) {
    toast.error('Não foi possível carregar a venda.')
    console.error('[nfce/[[id]]] erro ao carregar venda', e)
    await router.replace('/erp/vendas/emissao/nfce')
  } finally {
    carregando.value = false
  }
}

onMounted(() => {
  carregarVenda()
})

onBeforeUnmount(() => {
  realtime.desconectar()
})
// #endregion
</script>

<template>
  <div class="nfce-page">
    <PageToolbar :title="tituloPagina" subtitle="Emissão de Nota Fiscal de Consumidor Eletrônica (modelo 65)" :loading="carregando || salvando">
      <template #actions>
        <button
          v-if="jaTransmitido"
          type="button"
          class="btn btn-danger"
          :disabled="salvando"
          @click="cancelarDocumento"
        >
          Cancelar NFC-e
        </button>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="cancelarVenda">
          Cancelar venda
        </button>
        <button
          type="button"
          class="btn btn-outline"
          :disabled="salvando || semItens || jaTransmitido"
          @click="salvarRascunho"
        >
          Salvar rascunho
        </button>
        <button
          type="button"
          class="btn btn-primary"
          :disabled="salvando || semItens || jaTransmitido"
          @click="salvarTransmitir"
        >
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Transmitir NFC-e</span>
        </button>
      </template>
    </PageToolbar>

    <div v-if="jaTransmitido" class="alerta-transmitido glass-panel">
      <span class="badge badge-success">Transmitida</span>
      <span v-if="resultado.numero">NFC-e nº {{ resultado.numero }}</span>
      <span v-if="resultado.chave" class="chave">Chave: {{ resultado.chave }}</span>
      <button type="button" class="btn btn-ghost btn-sm" @click="dialogDanfeAberto = true">Ver DANFE</button>
    </div>

    <div class="nfce-grid">
      <!-- Coluna principal: consumidor + itens -->
      <div class="coluna-principal">
        <NfceClientePanel v-model="nfce.destinatario" :somente-leitura="jaTransmitido" />

        <div class="itens-bloco">
          <div class="itens-toolbar">
            <button type="button" class="btn btn-primary btn-sm" :disabled="jaTransmitido" @click="abrirNovoItem">
              + Adicionar item
            </button>
          </div>
          <NfceItensTable
            :itens="nfce.itens"
            :somente-leitura="jaTransmitido"
            @editar="editarItem"
            @remover="removerItem"
          />
        </div>

        <div class="descontos glass-panel">
          <MoneyInput v-model="descontoTotal" label="Desconto total (R$)" :disabled="jaTransmitido" />
          <MoneyInput v-model="acrescimoTotal" label="Acréscimo total (R$)" :disabled="jaTransmitido" />
        </div>

        <div class="observacoes glass-panel">
          <div class="field">
            <label class="field-label">Informações complementares</label>
            <textarea
              v-model="nfce.informacoesComplementares"
              class="input textarea"
              rows="2"
              :disabled="jaTransmitido"
            ></textarea>
          </div>
        </div>
      </div>

      <!-- Coluna lateral: pagamentos + total -->
      <div class="coluna-lateral">
        <NfcePagamentosPanel
          :pagamentos="nfce.pagamentos"
          :total-final="totalFinal"
          :bloqueado="semItens"
          :somente-leitura="jaTransmitido"
          @update:pagamentos="nfce.pagamentos = $event"
        />

        <div class="total-final glass-panel">
          <span class="tf-label">Total da venda</span>
          <span class="tf-valor">{{ formatarMoeda(totalFinal) }}</span>
        </div>
      </div>
    </div>

    <!-- Diálogo de item -->
    <NfceItemDialog v-model="dialogItemAberto" :item="itemEmEdicao" @confirmar="aoConfirmarItem" />

    <!-- Overlay de transmissão -->
    <TransmissionOverlay
      v-model="overlayVisivel"
      title="Transmitindo NFC-e"
      message="Aguarde enquanto a nota é enviada à SEFAZ."
      :steps="passosTransmissao"
      :current-step="passoAtual"
      :error="overlayErro"
    />

    <!-- Diálogo com a DANFE -->
    <AppDialog v-model="dialogDanfeAberto" title="NFC-e transmitida" width="720px">
      <DanfeViewer
        :src="danfeSrc"
        title="DANFE NFC-e"
        height="60vh"
        @print="imprimirDanfe"
        @download="baixarDanfe"
      />
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="dialogDanfeAberto = false">Fechar</button>
        <button type="button" class="btn btn-primary" @click="novaVenda">Nova venda</button>
      </template>
    </AppDialog>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.nfce-page { display: flex; flex-direction: column; gap: 16px; }
.alerta-transmitido {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 14px;
  font-size: 13px;
}
.alerta-transmitido .chave { color: var(--text-muted); font-family: monospace; font-size: 12px; }
.nfce-grid { display: grid; grid-template-columns: 1.6fr 1fr; gap: 16px; align-items: start; }
.coluna-principal { display: flex; flex-direction: column; gap: 16px; }
.coluna-lateral { display: flex; flex-direction: column; gap: 16px; position: sticky; top: 16px; }
.itens-bloco { display: flex; flex-direction: column; gap: 8px; }
.itens-toolbar { display: flex; justify-content: flex-end; }
.descontos { display: grid; grid-template-columns: 1fr 1fr; gap: 12px 16px; padding: 12px; }
.observacoes { padding: 12px; }
.textarea { width: 100%; resize: vertical; font-family: inherit; }
.total-final {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 4px;
  padding: 16px;
}
.tf-label { font-size: 13px; color: var(--text-muted); }
.tf-valor { font-size: 30px; font-weight: 800; color: var(--primary); }
@media (max-width: 1024px) {
  .nfce-grid { grid-template-columns: 1fr; }
  .coluna-lateral { position: static; }
}
</style>
