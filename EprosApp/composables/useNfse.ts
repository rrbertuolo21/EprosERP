/**
 * useNfse — NFS-e (Nota Fiscal de Serviço Eletrônica): configuração do provedor municipal,
 * emissão de lote de RPS, consultas (lote/RPS), cancelamento, listagem e geração de PDF a partir
 * de XML.
 *
 * Porta o comportamento de `NfseController` do legado (`api/v1/nfse`) para o padrão EprosERP
 * (`useApi`/`useApiList`, envelope CommandResult).
 *
 * Contrato de rota:
 *   GET  api/v1/nfse/config?codigoMunicipioIbge=
 *   POST api/v1/nfse/emitir-lote
 *   POST api/v1/nfse/consultar-lote
 *   POST api/v1/nfse/consultar-por-rps
 *   POST api/v1/nfse/cancelar
 *   GET  api/v1/nfse?status=&pagina=&tamanhoPagina=
 *   POST api/v1/nfse/gerar-pdf-xml (multipart/form-data)
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export type TipoAmbienteNfse = 1 | 2 // 1=Produção, 2=Homologação

/** Sub-DTO do prestador. Espelha NfsePrestadorCmd. */
export interface NfsePrestadorDto {
  documento: string
  crt?: number
  inscricaoMunicipal?: string | null
  razaoSocial?: string | null
  codigoMunicipioIbge?: number
  uf?: string | null
}

/** Sub-DTO do tomador. Espelha NfseTomadorCmd. */
export interface NfseTomadorDto {
  documento: string
  inscricaoMunicipal?: string | null
  razaoSocial?: string | null
}

/** Sub-DTO do RPS. Espelha NfseRpsCmd. */
export interface NfseRpsDto {
  numero: string
  serie: string
  tipo?: number
  dataEmissao?: string | null
}

/** Sub-DTO do serviço prestado. Espelha NfseServicoCmd. */
export interface NfseServicoDto {
  itemListaServico: string
  valorServicos: number
  aliquotaIss?: number
  valorIss?: number
  valorIssRetido?: number
  valorDeducoes?: number
  descontoIncondicionado?: number
  descontoCondicionado?: number
  issRetido?: number
  codigoCnae?: string | null
  codigoTributacaoMunicipio?: string | null
  codigoNbs?: string | null
  discriminacao?: string | null
  codigoMunicipioIbge?: number
  exigibilidadeIss?: number
  municipioIncidencia?: number
}

/** Corpo de emissão de lote de RPS. Fiel a `nfse/emitir-lote`. */
export interface EmitirLoteNfseDto {
  numeroLote: number
  rps: NfseRpsDto
  prestador: NfsePrestadorDto
  tomador: NfseTomadorDto
  servico: NfseServicoDto
  sincrono?: boolean
  ambiente?: TipoAmbienteNfse
  naturezaOperacao?: number
  regimeEspecialTributacao?: number
  optanteSimplesNacional?: boolean
  incentivoFiscal?: boolean
  competencia?: string | null
}

/** Corpo de consulta de lote. Fiel a `nfse/consultar-lote`. */
export interface ConsultarLoteNfseDto {
  numeroLote: number
  protocolo: string
  prestador: NfsePrestadorDto
  ambiente?: TipoAmbienteNfse
}

/** Corpo de consulta por RPS. Fiel a `nfse/consultar-por-rps`. */
export interface ConsultarNfsePorRpsDto {
  numeroRps: string
  serie: string
  prestador: NfsePrestadorDto
  tipo?: number
  mesCompetencia?: number
  anoCompetencia?: number
  ambiente?: TipoAmbienteNfse
}

/** Corpo de cancelamento. Fiel a `nfse/cancelar`. */
export interface CancelarNfseDto {
  numeroNfse: string
  codigoCancelamento: string
  prestador: NfsePrestadorDto
  motivo?: string | null
  ambiente?: TipoAmbienteNfse
}

/** Registro de NFS-e listado no histórico. */
export interface NotaServico {
  id: string
  status: string
  numeroNfse?: string | null
  numeroRps?: string | null
  serieRps?: string | null
  valorServicos: number
  dataEmissao: string
}

export interface NotaServicoFiltros {
  status?: string
}

export function useNfse() {
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<NotaServico, NotaServicoFiltros>('/nfse')

  /** Obtém a configuração do provedor NFS-e para o município informado. */
  async function obterConfig(codigoMunicipioIbge: number): Promise<unknown> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult>('/nfse/config', { query: { codigoMunicipioIbge } })
      return extrairDados(resposta)
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfse.obterConfig]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  /** Emite um lote de RPS (NFS-e). */
  async function emitirLote(payload: EmitirLoteNfseDto): Promise<unknown> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi.post<CommandResult>('/nfse/emitir-lote', payload)
      return extrairDados(resposta)
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfse.emitirLote]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  /** Consulta a situação de um lote de NFS-e. */
  async function consultarLote(payload: ConsultarLoteNfseDto): Promise<unknown> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi.post<CommandResult>('/nfse/consultar-lote', payload)
      return extrairDados(resposta)
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfse.consultarLote]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  /** Consulta a NFS-e gerada a partir de um RPS. */
  async function consultarPorRps(payload: ConsultarNfsePorRpsDto): Promise<unknown> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi.post<CommandResult>('/nfse/consultar-por-rps', payload)
      return extrairDados(resposta)
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfse.consultarPorRps]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  /** Cancela uma NFS-e autorizada. */
  async function cancelar(payload: CancelarNfseDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/nfse/cancelar', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfse.cancelar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  /** Gera o PDF (DANFSe) a partir de um arquivo XML de NFS-e já emitido. Retorna a URL do objeto Blob. */
  async function gerarPdfDeXml(arquivo: File, codigoMunicipioIbge?: number): Promise<string | null> {
    carregando.value = true
    erro.value = null
    try {
      const formData = new FormData()
      formData.append('arquivo', arquivo)
      const blob = await useApi<Blob>('/nfse/gerar-pdf-xml', {
        method: 'POST',
        body: formData as unknown as Record<string, unknown>,
        query: { codigoMunicipioIbge },
        responseType: 'blob'
      })
      return URL.createObjectURL(blob)
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNfse.gerarPdfDeXml]', e)
      return null
    } finally {
      carregando.value = false
    }
  }

  return {
    carregando,
    erro,
    // listagem server-side (ver useApiList)
    itens: lista.itens,
    total: lista.total,
    buscar: lista.buscar,
    filtros: lista.filtros,
    // ações
    obterConfig,
    emitirLote,
    consultarLote,
    consultarPorRps,
    cancelar,
    gerarPdfDeXml
  }
}
