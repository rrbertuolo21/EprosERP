/**
 * usePdvNfce — camada de IO exclusiva da fatia PDV.
 *
 * Único ponto de acesso à API desta tela: usa SEMPRE `useApi` compartilhado
 * (baseURL/token/tenant já embutidos pelo plugin) — nunca `$fetch` direto nem URL fixa.
 *
 * Consome os endpoints definidos no MAPA para a fatia PDV:
 *   - GET  estoque-produtos            (busca de produtos por empresa/termo)
 *   - GET  balancas                    (decodificação de código de barras pesado)
 *   - POST vendas-fiscal               (cria o cabeçalho fiscal da venda)
 *   - POST vendas-fiscal/{id}/nfce     (inclui os dados da NFC-e)
 *   - POST vendas-fiscal/{id}/nfce/transmitir  (registra a transmissão autorizada)
 *   - POST vendas-fiscal/{id}/nfce/cancelar    (cancela a NFC-e)
 *
 * OBS (reportado ao integrador): o backend expõe o fluxo fiscal em passos de baixo nível
 * (CQRS). Não há, hoje, um único endpoint "gravar venda + emitir NFC-e" que orquestre
 * emitente/itens/impostos/totais/pagamentos + comunicação SEFAZ como o legado tinha
 * (`POST /vendas/nfce` + `/vendas/nfce-transmitir`). Enquanto esse endpoint agregado não
 * existir, `gravarVenda`/`transmitir` abaixo chamam os passos disponíveis e o resultado da
 * SEFAZ (chave/protocolo) chega por tempo real (SignalR) — ver `usePagina`/TransmissionOverlay.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult, extrairLista} from '~/composables/useApi'
import { useTenant } from '~/composables/useTenant'
import { obterMensagemErro } from '~/composables/useApiList'
import type { BalancaPdv, DestinatarioPdv, ItemPdv, ModeloFiscal, PagamentoPdv, ProdutoPdv } from './tipos'

/** Corpo enviado ao criar o cabeçalho fiscal da venda (subset do CriarVendaFiscalCommand). */
export interface GravarVendaPayload {
  modeloFiscal: ModeloFiscal
  status: number
  modalidadeFrete: number
  destinatario: DestinatarioPdv
  itens: ItemPdv[]
  pagamentos: PagamentoPdv[]
  valorDesconto: number
  valorAcrescimo: number
  valorFrete: number
  informacoesComplementares: string
}

/** Resultado normalizado de uma emissão. */
export interface ResultadoEmissao {
  vendaId: string | null
  numero: number | null
  chave: string | null
}

export function usePdvNfce() {
  const { empresaId } = useTenant()

  const carregando = ref(false)
  const erro = ref<string | null>(null)

  /** Busca produtos por termo (nome, código ou EAN) dentro da empresa ativa. */
  async function buscarProdutos(termo: string): Promise<ProdutoPdv[]> {
    if (!termo?.trim()) return []
    try {
      const resposta = await useApi<CommandResult<ProdutoPdv[]>>('/estoque-produtos', {
        query: {
          empresaId: empresaId.value || undefined,
          localizar: termo.trim(),
          pagina: 1,
          tamanhoPagina: 20
        }
      })
      return extrairLista<ProdutoPdv>(resposta) ?? []
    } catch (e) {
      console.error('[usePdvNfce.buscarProdutos]', e)
      return []
    }
  }

  /** Carrega as balanças cadastradas (para leitura de código de barras pesado). */
  async function carregarBalancas(): Promise<BalancaPdv[]> {
    try {
      const resposta = await useApi<CommandResult<BalancaPdv[]>>('/balancas', {
        query: { pagina: 1, tamanhoPagina: 100 }
      })
      return extrairLista<BalancaPdv>(resposta) ?? []
    } catch (e) {
      console.error('[usePdvNfce.carregarBalancas]', e)
      return []
    }
  }

  /**
   * Grava o cabeçalho fiscal da venda. Retorna o id (Guid) da venda criada.
   * Passos posteriores (nfce/transmitir) usam esse id.
   */
  async function gravarVenda(payload: GravarVendaPayload): Promise<string | null> {
    carregando.value = true
    erro.value = null
    try {
      const total = calcularTotal(payload)
      const corpo = {
        caixaId: '',
        modeloFiscal: payload.modeloFiscal,
        naturezaOperacao: 'VENDA',
        dataVenda: new Date().toISOString(),
        informacoesComplementares: payload.informacoesComplementares.replace(/\n/g, ';'),
        informacoesAdicionaisFisco: '',
        status: String(payload.status),
        modalidadeFrete: payload.modalidadeFrete,
        vendaOrigem: 0,
        incluirFreteNoTotal: false,
        clienteId: payload.destinatario.pessoaId || null,
        total,
        valorDesconto: payload.valorDesconto,
        valorFrete: payload.valorFrete,
        formaPagamento: null,
        // Coleções auxiliares — o backend pode ignorá-las neste endpoint e exigir os
        // sub-endpoints (itens/pagamentos). Enviamos para compatibilidade futura.
        itens: payload.itens.map((item) => ({
          produtoId: item.produtoId,
          quantidadeComercial: item.quantidadeComercial,
          valorUnitarioComercial: item.valorUnitarioComercial,
          valorDesconto: item.valorDesconto
        })),
        pagamentos: payload.pagamentos.map((p) => ({
          tipoPagamento: p.tipoPagamento,
          valorPagamento: p.valorPagamento,
          valorTroco: p.valorTroco
        }))
      }

      const resposta = await useApi.post<CommandResult<{ id?: string }>>('/vendas-fiscal', corpo)
      const dados = extrairDados<{ id?: string }>(resposta)
      return dados?.id ?? null
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePdvNfce.gravarVenda]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  /** Inclui os dados da NFC-e na venda (série/número/CSC ficam a cargo do backend). */
  async function incluirNfce(vendaId: string): Promise<void> {
    await useApi.post<CommandResult>('/vendas-fiscal/{id}/nfce', { vendaId }, { params: { id: vendaId } })
  }

  /**
   * Transmite a NFC-e da venda à SEFAZ. Retorna a chave/número quando a resposta
   * síncrona os traz; caso a autorização venha por tempo real (SignalR), estes campos
   * podem chegar nulos e serem preenchidos pelo evento do hub.
   */
  async function transmitir(vendaId: string): Promise<ResultadoEmissao> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi.post<CommandResult<Record<string, unknown>>>(
        '/vendas-fiscal/{id}/nfce/transmitir',
        { vendaId },
        { params: { id: vendaId } }
      )
      const dados = extrairDados<Record<string, unknown>>(resposta) ?? {}
      return {
        vendaId,
        numero: numeroOuNulo(dados['numero']),
        chave: stringOuNulo(dados['chave']) ?? stringOuNulo(dados['chaveAcesso'])
      }
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePdvNfce.transmitir]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  /** Cancela a NFC-e transmitida, com a justificativa informada. */
  async function cancelar(vendaId: string, motivo: string): Promise<void> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post<CommandResult>(
        '/vendas-fiscal/{id}/nfce/cancelar',
        { vendaId, motivoCancelamento: motivo },
        { params: { id: vendaId } }
      )
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePdvNfce.cancelar]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  /** Baixa o PDF da DANFE (cupom) da NFC-e emitida, como Blob para o DanfeViewer. */
  async function baixarCupom(chave: string): Promise<Blob | null> {
    try {
      return await useApi<Blob>('/vendas-fiscal/nfce/{chave}/pdf', {
        params: { chave },
        responseType: 'blob'
      })
    } catch (e) {
      console.error('[usePdvNfce.baixarCupom]', e)
      return null
    }
  }

  return {
    carregando,
    erro,
    buscarProdutos,
    carregarBalancas,
    gravarVenda,
    incluirNfce,
    transmitir,
    cancelar,
    baixarCupom
  }
}

/** Total final da venda = itens - desconto item + acréscimo + frete - desconto geral. */
function calcularTotal(payload: GravarVendaPayload): number {
  const totalItens = payload.itens.reduce((acc, item) => {
    const base = item.quantidadeComercial * (item.valorUnitarioComercial ?? 0)
    return acc + arredondar2(base - item.valorDesconto)
  }, 0)
  const total = totalItens + payload.valorFrete + payload.valorAcrescimo - payload.valorDesconto
  return arredondar2(total)
}

function arredondar2(v: number): number {
  return Math.round((v + Number.EPSILON) * 100) / 100
}

function numeroOuNulo(v: unknown): number | null {
  const n = Number(v)
  return Number.isFinite(n) && v != null && v !== '' ? n : null
}

function stringOuNulo(v: unknown): string | null {
  return v != null && String(v).length > 0 ? String(v) : null
}
