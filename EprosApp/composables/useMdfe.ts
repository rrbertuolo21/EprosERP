/**
 * useMdfe — MDF-e (Manifesto Eletrônico de Documentos Fiscais): emissão, encerramento e
 * listagem.
 *
 * Porta o comportamento de `MdfeController` do legado (`api/v1/mdfe`) para o padrão EprosERP
 * (`useApi`/`useApiList`, envelope CommandResult).
 *
 * Contrato de rota:
 *   GET  api/v1/mdfe?status=&pagina=&tamanhoPagina=
 *   POST api/v1/mdfe/emitir
 *   POST api/v1/mdfe/encerrar/{chave}/{codigoMunicipio}
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export type TipoAmbienteMdfe = 1 | 2 // 1=Produção, 2=Homologação

/** Corpo de emissão de MDF-e. Fiel a `EmitirMdfeCommand`. */
export interface EmitirMdfeDto {
  serie: number
  numero: number
  ufInicio: string
  ufFim: string
  quantidadeCarregados: number
  valorCarga: number
  ambiente?: TipoAmbienteMdfe
  modal?: number
  tipoEmitente?: number
}

/** Registro de MDF-e listado no histórico. */
export interface Mdfe {
  id: string
  status: string
  chave?: string | null
  serie: number
  numero: number
  valorCarga: number
  dataEmissao: string
}

export interface MdfeFiltros {
  status?: string
}

export function useMdfe() {
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<Mdfe, MdfeFiltros>('/mdfe')

  /** Emite um MDF-e de forma síncrona. */
  async function emitir(payload: EmitirMdfeDto): Promise<unknown> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi.post<CommandResult>('/mdfe/emitir', payload)
      return extrairDados(resposta)
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useMdfe.emitir]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  /** Encerra um MDF-e previamente autorizado. */
  async function encerrar(chave: string, codigoMunicipio: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post(`/mdfe/encerrar/${chave}/${codigoMunicipio}`)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useMdfe.encerrar]', e)
      return false
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
    emitir,
    encerrar
  }
}
