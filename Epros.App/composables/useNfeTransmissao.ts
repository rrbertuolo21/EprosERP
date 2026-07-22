/**
 * useNfeTransmissao — transmissão/consulta/cancelamento de NF-e/NFC-e na venda + operações
 * genéricas de DF-e (status SEFAZ, protocolo, inutilização) e download de documentos.
 *
 * Porta os composables `vendas/useNfeTransmissao.ts` e `vendas/useSignalr.ts` (parte de IO) do
 * legado para o padrão EprosERP (`useApi`/`useApiList`, envelope CommandResult), cobrindo:
 *  - `api/v1/vendas-fiscal/{id}/nfce` (incluir/transmitir/cancelar NFC-e)
 *  - `api/v1/vendas-fiscal/{id}/nfe` (incluir/transmitir/cancelar NF-e + carta de correção)
 *  - `api/v1/baixa-documentos-dfe` (verificar-status, consultar-protocolo, inutilizar,
 *    inutilizações por documento)
 *
 * NÃO cobre downloads de arquivo (XML/PDF) — ver `useDownloadDfe.ts`.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export type TipoAmbiente = 1 | 2 // 1=Produção, 2=Homologação

/** Payload de inclusão dos dados da NFC-e na venda. Porte de Venda.IncluirNfce. */
export interface IncluirNfceDto {
  serie: number
  numero: number
  idCsc: string
  csc: string
}

/** Registro da transmissão autorizada da NFC-e. Porte de Venda.TransmitirNfce. */
export interface TransmitirNfceDto {
  chave: string
  dataHoraEmissao: string
  statusSefaz: number
  protocolo: string
}

/** Cancelamento da NFC-e. Porte de Venda.CancelarNfce. */
export interface CancelarNfceDto {
  dataHoraCancelamento: string
  protocoloCancelamento: string
  statusSefazCancelamento: number
  motivoCancelamento: string
}

/** Payload de inclusão dos dados da NF-e na venda. Porte de Venda.IncluirNfe. */
export interface IncluirNfeDto {
  serie: number
  numero: number
  dataHoraSaida?: string | null
  embuteFrete?: boolean
  embuteSeguro?: boolean
  embuteAcrescimo?: boolean
  embuteOutro?: boolean
}

/** Registro da transmissão autorizada da NF-e. Porte de Venda.TransmitirNfe. */
export interface TransmitirNfeDto {
  chave: string
  naturezaOperacao: string
  dataHoraEmissao: string
  statusSefaz: number
  protocolo: string
}

/** Cancelamento da NF-e. Porte de Venda.CancelarNfe. */
export interface CancelarNfeDto {
  dataHoraCancelamento: string
  protocoloCancelamento: string
  statusSefazCancelamento: number
  motivoCancelamento: string
}

/** Carta de correção (CC-e) da NF-e da venda. Porte de VendaNfe.AdicionarCartaCorrecao. */
export interface AdicionarCartaCorrecaoDto {
  ambiente: TipoAmbiente
  textoCorrecao: string
}

/** NF-e referenciada (devolução/complementar). Porte de Venda.AdicionarNfeReferenciada. */
export interface AdicionarNfeReferenciadaDto {
  chaveAcesso: string
}

/** Verificação de status do serviço SEFAZ. Fiel a `baixa-documentos-dfe/verificar-status`. */
export interface VerificarStatusServicoDto {
  documento: string
  uf: string
  ambiente?: TipoAmbiente
  modeloDocumento?: 55 | 65
}

/** Consulta de protocolo/situação pela chave. Fiel a `baixa-documentos-dfe/consultar-protocolo`. */
export interface ConsultarProtocoloDto {
  chave: string
  ambiente?: TipoAmbiente
  modeloDocumento?: 55 | 65
}

/** Inutilização de faixa de numeração. Fiel a `baixa-documentos-dfe/inutilizar`. */
export interface InutilizarFaixaFiscalDto {
  modeloDocumento: 55 | 65
  serie: number
  nrNfInicial: number
  nrNfFinal: number
  justificativa: string
  ambiente?: TipoAmbiente
  ano?: number
}

/** Registro de inutilização de faixa, conforme listado pelo histórico. */
export interface InutilizacaoFiscal {
  id: string
  modeloDocumento: number
  serie: number
  nrNfInicial: number
  nrNfFinal: number
  justificativa: string
  ambiente: number
  protocolo?: string | null
  statusSefaz?: number | null
  dataInutilizacao?: string | null
}

export interface InutilizacaoFiltros {
  modeloDocumento?: number
  ambiente?: number
}

export function useNfeTransmissao() {
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const inutilizacoes = useApiList<InutilizacaoFiscal, InutilizacaoFiltros>('/baixa-documentos-dfe/obter-inutilizacoes-por-documento')

  // ---------- NFC-e ----------

  async function incluirNfce(vendaId: string, payload: IncluirNfceDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post(`/vendas-fiscal/${vendaId}/nfce`, { vendaId, ...payload })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfeTransmissao.incluirNfce]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function transmitirNfce(vendaId: string, payload: TransmitirNfceDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post(`/vendas-fiscal/${vendaId}/nfce/transmitir`, { vendaId, ...payload })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfeTransmissao.transmitirNfce]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function cancelarNfce(vendaId: string, payload: CancelarNfceDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post(`/vendas-fiscal/${vendaId}/nfce/cancelar`, { vendaId, ...payload })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfeTransmissao.cancelarNfce]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  // ---------- NF-e ----------

  async function incluirNfe(vendaId: string, payload: IncluirNfeDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post(`/vendas-fiscal/${vendaId}/nfe`, { vendaId, ...payload })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfeTransmissao.incluirNfe]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function transmitirNfe(vendaId: string, payload: TransmitirNfeDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post(`/vendas-fiscal/${vendaId}/nfe/transmitir`, { vendaId, ...payload })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfeTransmissao.transmitirNfe]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function cancelarNfe(vendaId: string, payload: CancelarNfeDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post(`/vendas-fiscal/${vendaId}/nfe/cancelar`, { vendaId, ...payload })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfeTransmissao.cancelarNfe]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function adicionarCartaCorrecao(vendaId: string, payload: AdicionarCartaCorrecaoDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post(`/vendas-fiscal/${vendaId}/nfe/carta-correcao`, { vendaId, ...payload })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfeTransmissao.adicionarCartaCorrecao]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function adicionarNfeReferenciada(vendaId: string, payload: AdicionarNfeReferenciadaDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post(`/vendas-fiscal/${vendaId}/nfe/referenciadas`, { vendaId, ...payload })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfeTransmissao.adicionarNfeReferenciada]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  // ---------- Consultas online SEFAZ / inutilização (BaixaDocumentoDfeController) ----------

  async function verificarStatusServico(payload: VerificarStatusServicoDto): Promise<unknown> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi.post<CommandResult>('/baixa-documentos-dfe/verificar-status', payload)
      return extrairDados(resposta)
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfeTransmissao.verificarStatusServico]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function consultarProtocolo(payload: ConsultarProtocoloDto): Promise<unknown> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi.post<CommandResult>('/baixa-documentos-dfe/consultar-protocolo', payload)
      return extrairDados(resposta)
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfeTransmissao.consultarProtocolo]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function inutilizar(payload: InutilizarFaixaFiscalDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/baixa-documentos-dfe/inutilizar', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfeTransmissao.inutilizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  /** Lista inutilizações por documento/ambiente. Rota: obter-inutilizacoes-por-documento/{documento}/{ambiente}. */
  async function listarInutilizacoesPorDocumento(
    documento: string,
    ambiente: TipoAmbiente,
    opcoes: { modeloDocumento?: number; pagina?: number; tamanhoPagina?: number } = {}
  ): Promise<void> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<unknown>>(
        `/baixa-documentos-dfe/obter-inutilizacoes-por-documento/${documento}/${ambiente}`,
        {
          query: {
            modeloDocumento: opcoes.modeloDocumento,
            pagina: opcoes.pagina ?? 1,
            tamanhoPagina: opcoes.tamanhoPagina ?? 50
          }
        }
      )
      const d = extrairDados<{ itens?: InutilizacaoFiscal[]; Itens?: InutilizacaoFiscal[]; total?: number; Total?: number }>(resposta)
      inutilizacoes.itens.value = d?.itens ?? d?.Itens ?? []
      inutilizacoes.total.value = d?.total ?? d?.Total ?? inutilizacoes.itens.value.length
    } catch (e) {
      erro.value = obterMensagemErro(e)
      inutilizacoes.itens.value = []
      inutilizacoes.total.value = 0
      console.error('[useNfeTransmissao.listarInutilizacoesPorDocumento]', e)
    } finally {
      carregando.value = false
    }
  }

  return {
    carregando,
    erro,
    // NFC-e
    incluirNfce,
    transmitirNfce,
    cancelarNfce,
    // NF-e
    incluirNfe,
    transmitirNfe,
    cancelarNfe,
    adicionarCartaCorrecao,
    adicionarNfeReferenciada,
    // SEFAZ / inutilização
    verificarStatusServico,
    consultarProtocolo,
    inutilizar,
    inutilizacoes: inutilizacoes.itens,
    totalInutilizacoes: inutilizacoes.total,
    listarInutilizacoesPorDocumento
  }
}
