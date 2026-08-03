/**
 * useImportacaoXml — importação de XML de compra (NF-e de entrada) e histórico de importações.
 *
 * Porta o comportamento de `ComprasController.ImportarXml` + `ImportacoesXmlController` do
 * legado para o padrão EprosERP (`useApi`/`useApiList`, envelope CommandResult).
 *
 * Contrato de rota:
 *   POST api/v1/compras/importar-xml (multipart/form-data, campo `arquivo`)
 *   GET  api/v1/compras/importacoes-xml?ordenarPor=&ordemAscendente=&pagina=&tamanhoPagina=
 *   GET  api/v1/compras/importacoes-xml/{id}
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

/** Registro de importação de XML (listagem — sem o conteúdo do XML). */
export interface ImportacaoXml {
  id: string
  empresaId: string
  tipoDeXml: number
  nfeId: string | null
  codigoSefaz: string | null
  tipoEvento: string | null
  statusImportacaoXml: number
  mensagemErroImportacaoXml: string | null
  statusCadastro: number
  mensagemErroCadastro: string | null
  statusSalvarPdf: number
  mensagemErroSalvarPdf: string | null
  dataImportacao: string
}

/** Detalhe de uma importação de XML (inclui o conteúdo do XML). */
export interface ImportacaoXmlDetalhe extends ImportacaoXml {
  xml: string
}

export interface ImportacaoXmlFiltros {
  ordenarPor?: string
  ordemAscendente?: boolean
}

export function useImportacaoXml() {
  const importacao = ref<ImportacaoXmlDetalhe | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<ImportacaoXml, ImportacaoXmlFiltros>('/compras/importacoes-xml', {
    filtrosIniciais: { ordenarPor: 'data', ordemAscendente: false }
  })

  /** Importa uma NF-e de entrada a partir de um arquivo XML (multipart/form-data). */
  async function importar(arquivo: File): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      const formData = new FormData()
      formData.append('arquivo', arquivo)
      await useApi.post('/compras/importar-xml', formData as unknown as Record<string, unknown>)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useImportacaoXml.importar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  /** Busca o detalhe de uma importação de XML por Id (inclui o conteúdo do XML). */
  async function buscarPorId(id: string): Promise<ImportacaoXmlDetalhe | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<ImportacaoXmlDetalhe>>('/compras/importacoes-xml/{id}', { params: { id } })
      const dados = extrairDados<ImportacaoXmlDetalhe>(resposta) ?? null
      importacao.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useImportacaoXml.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  return {
    importacao,
    carregando,
    erro,
    // listagem server-side (ver useApiList)
    itens: lista.itens,
    total: lista.total,
    buscar: lista.buscar,
    filtros: lista.filtros,
    // ações
    importar,
    buscarPorId
  }
}
