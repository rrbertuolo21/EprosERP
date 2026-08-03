/**
 * useDownloadDfe — download/impressão de documentos DF-e autorizados (NF-e/NFC-e).
 *
 * Porta o comportamento de download do legado (`BaixaDocumentoDfeController`/`nfce-nfe`) para o
 * padrão EprosERP (`useApi` com `responseType: 'blob'`). Cobre metadados, XML/PDF autorizado,
 * cancelamento, carta de correção (CC-e), XML de envio (venda/compra) e ZIP para o contador.
 *
 * Contrato de rota (BaixaDocumentoDfeController, `api/v1/baixa-documentos-dfe`):
 *   GET  obter-por-chave/{chave}
 *   GET  obter-por-referencia/{mes}/{ano}/{pagina}/{tamanhoPagina}?documentoDestinatario=
 *   GET  download-xml/{chave}                    (XML)
 *   GET  download-pdf/{chave}                     (PDF)
 *   POST gerar-baixar-novo-pdf-nfe/{chave}        (PDF)
 *   GET  download-xml-cancelamento/{chave}        (ZIP)
 *   GET  download-pdf-cancelamento/{chave}
 *   GET  download-xml-cce/{chave}                 (XML)
 *   GET  download-pdf-cce/{chave}
 *   GET  download-xml-envio/{vendaId}             (XML)
 *   GET  download-xml-compra-envio/{compraId}     (XML)
 *   POST download-documentos-contador             (ZIP, body: período)
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

/** Metadados de um DF-e (NF-e/NFC-e), conforme `obter-por-chave`/`obter-por-referencia`. */
export interface DocumentoFiscalResumo {
  id: string
  modelo: string
  status: string
  chaveAcesso: string | null
  protocolo: string | null
  serie: number
  numero: number
  total: number
  destinatarioCnpjCpf: string | null
  destinatarioNome: string | null
  dataEmissao: string
  dataAutorizacao: string | null
  urlDanfe: string | null
  urlXml: string | null
}

export interface DocumentoPorReferenciaFiltros {
  documentoDestinatario?: string
}

/** Corpo do ZIP de entrega ao contador. Fiel a `DownloadDocumentosContadorQuery`. */
export interface DownloadDocumentosContadorDto {
  dataEmissaoInicial: string
  dataEmissaoFinal: string
  documentoDestinatario?: string | null
  incluiPdfs?: boolean
}

/** Dispara o download de um Blob no navegador (cria um link temporário e clica nele). */
function dispararDownload(blob: Blob, nomeArquivo: string): void {
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = nomeArquivo
  document.body.appendChild(link)
  link.click()
  link.remove()
  URL.revokeObjectURL(url)
}

export function useDownloadDfe() {
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const documentosPorReferencia = useApiList<DocumentoFiscalResumo, DocumentoPorReferenciaFiltros>(
    '/baixa-documentos-dfe/obter-por-referencia'
  )

  /** Obtém os metadados do documento fiscal (incluindo urlDanfe/urlXml) pela chave de acesso. */
  async function obterPorChave(chave: string): Promise<DocumentoFiscalResumo | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<DocumentoFiscalResumo> | DocumentoFiscalResumo>(
        `/baixa-documentos-dfe/obter-por-chave/${chave}`
      )
      return extrairDados<DocumentoFiscalResumo>(resposta) ?? null
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useDownloadDfe.obterPorChave]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  /** Lista DF-e por mês/ano de referência (e documento do destinatário, opcional), paginado. */
  async function listarPorReferencia(
    mes: number,
    ano: number,
    opcoes: { pagina?: number; tamanhoPagina?: number; documentoDestinatario?: string } = {}
  ): Promise<void> {
    carregando.value = true
    erro.value = null
    try {
      const pagina = opcoes.pagina ?? 1
      const tamanhoPagina = opcoes.tamanhoPagina ?? 50
      const resposta = await useApi<CommandResult<unknown>>(
        `/baixa-documentos-dfe/obter-por-referencia/${mes}/${ano}/${pagina}/${tamanhoPagina}`,
        { query: { documentoDestinatario: opcoes.documentoDestinatario } }
      )
      const d = extrairDados<{ itens?: DocumentoFiscalResumo[]; Itens?: DocumentoFiscalResumo[]; total?: number; Total?: number }>(resposta)
      documentosPorReferencia.itens.value = d?.itens ?? d?.Itens ?? []
      documentosPorReferencia.total.value = d?.total ?? d?.Total ?? documentosPorReferencia.itens.value.length
    } catch (e) {
      erro.value = obterMensagemErro(e)
      documentosPorReferencia.itens.value = []
      documentosPorReferencia.total.value = 0
      console.error('[useDownloadDfe.listarPorReferencia]', e)
    } finally {
      carregando.value = false
    }
  }

  /** Baixa (dispara download no navegador) um arquivo binário de uma rota GET simples. */
  async function baixarArquivo(rota: string, nomeArquivoFallback: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      const blob = await useApi<Blob>(rota, { responseType: 'blob' })
      dispararDownload(blob, nomeArquivoFallback)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error(`[useDownloadDfe.baixarArquivo:${rota}]`, e)
      return false
    } finally {
      carregando.value = false
    }
  }

  /** Baixa o XML autorizado (retorno) do documento fiscal pela chave de acesso. */
  function baixarXml(chave: string): Promise<boolean> {
    return baixarArquivo(`/baixa-documentos-dfe/download-xml/${chave}`, `${chave}.xml`)
  }

  /** Baixa/gera o PDF do DANFE (NF-e) / cupom (NFC-e) autorizado pela chave de acesso. */
  function baixarPdf(chave: string): Promise<boolean> {
    return baixarArquivo(`/baixa-documentos-dfe/download-pdf/${chave}`, `${chave}.pdf`)
  }

  /** Regera e baixa o PDF do DANFE da NF-e pela chave (alias de baixarPdf, via POST). */
  async function gerarBaixarNovoPdfNfe(chave: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      const blob = await useApi<Blob>(`/baixa-documentos-dfe/gerar-baixar-novo-pdf-nfe/${chave}`, {
        method: 'POST',
        responseType: 'blob'
      })
      dispararDownload(blob, `${chave}.pdf`)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useDownloadDfe.gerarBaixarNovoPdfNfe]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  /** Baixa o XML do cancelamento agrupado com o XML autorizado (ZIP). */
  function baixarXmlCancelamento(chave: string): Promise<boolean> {
    return baixarArquivo(`/baixa-documentos-dfe/download-xml-cancelamento/${chave}`, `docs-${chave}-normal-canc.zip`)
  }

  /** Baixa o comprovante (PDF/XML) do cancelamento pela chave. */
  function baixarPdfCancelamento(chave: string): Promise<boolean> {
    return baixarArquivo(`/baixa-documentos-dfe/download-pdf-cancelamento/${chave}`, `${chave}-canc.xml`)
  }

  /** Baixa o XML da última carta de correção (CC-e) pela chave. */
  function baixarXmlCce(chave: string): Promise<boolean> {
    return baixarArquivo(`/baixa-documentos-dfe/download-xml-cce/${chave}`, `carta-correcao-${chave}.xml`)
  }

  /** Baixa o comprovante (PDF/XML) da última carta de correção (CC-e) pela chave. */
  function baixarPdfCce(chave: string): Promise<boolean> {
    return baixarArquivo(`/baixa-documentos-dfe/download-pdf-cce/${chave}`, `carta-correcao-${chave}.xml`)
  }

  /** Baixa o XML de envio (pré-autorização) do DF-e a partir da venda de origem. */
  function baixarXmlEnvioPorVenda(vendaId: string): Promise<boolean> {
    return baixarArquivo(`/baixa-documentos-dfe/download-xml-envio/${vendaId}`, `${vendaId}-envio.xml`)
  }

  /** Baixa o XML de envio do DF-e a partir da compra de origem. */
  function baixarXmlEnvioPorCompra(compraId: string): Promise<boolean> {
    return baixarArquivo(`/baixa-documentos-dfe/download-xml-compra-envio/${compraId}`, `${compraId}-envio.xml`)
  }

  /** Gera e baixa o ZIP com todos os XMLs (autorizados + cancelamentos + CC-e + inutilizações) do período. */
  async function baixarDocumentosContador(payload: DownloadDocumentosContadorDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      const blob = await useApi<Blob>('/baixa-documentos-dfe/download-documentos-contador', {
        method: 'POST',
        body: payload,
        responseType: 'blob'
      })
      const nomeArquivo = `docs-contador-${payload.dataEmissaoInicial}-${payload.dataEmissaoFinal}.zip`
      dispararDownload(blob, nomeArquivo)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useDownloadDfe.baixarDocumentosContador]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    carregando,
    erro,
    // metadados / listagem
    obterPorChave,
    documentosPorReferencia: documentosPorReferencia.itens,
    totalDocumentosPorReferencia: documentosPorReferencia.total,
    listarPorReferencia,
    // downloads
    baixarXml,
    baixarPdf,
    gerarBaixarNovoPdfNfe,
    baixarXmlCancelamento,
    baixarPdfCancelamento,
    baixarXmlCce,
    baixarPdfCce,
    baixarXmlEnvioPorVenda,
    baixarXmlEnvioPorCompra,
    baixarDocumentosContador
  }
}
