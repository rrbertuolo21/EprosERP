/**
 * Composable local da fatia 11 — Monitor de Transmissões (SEFAZ).
 *
 * Porta o comportamento de `vendas/transmissoes.vue` do legado: listagem paginada das
 * vendas fiscais (NF-e/NFC-e) por período/status, com ações de download da DANFE,
 * geração de PDF, cancelamento e disparo de devolução/retorno.
 *
 * IO exclusivamente pelo cliente compartilhado `useApi`/`useApiList` (padrão CommandResult).
 * Endpoints: `fiscal/documentos` (XML/emitir/cancelar/DANFE/carta-correção, real),
 * `vendas-fiscal` (listagem + cancelamento, real).
 *
 * ROTAS REAIS (ligadas): o `DocumentosFiscaisController` já expõe
 * `GET fiscal/documentos/{id}/danfe` (PDF do DANFE/cupom) e
 * `POST fiscal/documentos/carta-correcao` ({ documentoFiscalId, textoCorrecao }).
 * Como a listagem de transmissões nem sempre traz o Guid do documento, resolvemos o id
 * a partir da chave de acesso via `GET fiscal/documentos/chave/{chave}` quando necessário.
 *
 * STUB HONESTO: a exportação de planilha (`relatorios/vendas/simplificado01`) ainda NÃO
 * existe no backend — a ação fica bloqueada com mensagem "em implementação". Nunca
 * simulamos sucesso nem Blob fake.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from '~/composables/useApi'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import {
  documentoVigente,
  type DocumentoFiscalResumo,
  type TransmissaoItem,
  type TransmissoesFiltros
} from './transmissoesTypes'

/** Primeiro/último dia do mês corrente em ISO (YYYY-MM-DD), para o filtro padrão. */
function periodoMesCorrente(): { inicio: string; fim: string } {
  const hoje = new Date()
  const primeiro = new Date(hoje.getFullYear(), hoje.getMonth(), 1)
  const ultimo = new Date(hoje.getFullYear(), hoje.getMonth() + 1, 0)
  const iso = (d: Date) => {
    const ano = d.getFullYear()
    const mes = String(d.getMonth() + 1).padStart(2, '0')
    const dia = String(d.getDate()).padStart(2, '0')
    return `${ano}-${mes}-${dia}`
  }
  return { inicio: iso(primeiro), fim: iso(ultimo) }
}

/**
 * Único endpoint ainda não exposto pelo backend (ver cabeçalho do arquivo).
 * Enquanto o gap não fechar, a exportação de planilha fica bloqueada.
 */
export const EXPORTAR_PLANILHA_EM_IMPLEMENTACAO = true

/** DTO de resumo do documento fiscal retornado por `fiscal/documentos/{id|chave}`. */
interface DocumentoFiscalDto {
  id?: string | null
  chaveAcesso?: string | null
  status?: string | null
}

export function useTransmissoes() {
  const toast = useToast()

  const { inicio, fim } = periodoMesCorrente()

  const filtrosIniciais: TransmissoesFiltros = {
    dataVendaInicial: inicio,
    dataVendaFinal: fim,
    status: null,
    chave: null,
    nrNf: null,
    cfop: null,
    modeloFiscal: null
  }

  // Listagem paginada server-side sobre a listagem de vendas fiscais.
  const lista = useApiList<TransmissaoItem, TransmissoesFiltros>('/vendas-fiscal', {
    filtrosIniciais,
    tamanhoPaginaInicial: 20
  })

  const baixando = ref(false)
  const cancelando = ref(false)

  /** Aplica filtros e recarrega a partir da página 1. */
  async function aplicar(novos: Partial<TransmissoesFiltros>): Promise<void> {
    await lista.aplicarFiltros(novos)
  }

  /** Limpa filtros voltando ao mês corrente. */
  async function limpar(): Promise<void> {
    const { inicio: i, fim: f } = periodoMesCorrente()
    lista.filtros.value = {
      ...filtrosIniciais,
      dataVendaInicial: i,
      dataVendaFinal: f
    }
    lista.pagina.value = 1
    await lista.buscar()
  }

  /**
   * Resolve o Guid do DocumentoFiscal de um sub-documento. Usa `documentoFiscalId` quando a
   * listagem já o trouxe; caso contrário consulta `fiscal/documentos/chave/{chave}`.
   */
  async function resolverDocumentoFiscalId(doc: DocumentoFiscalResumo | null): Promise<string | null> {
    if (!doc) return null
    if (doc.documentoFiscalId) return doc.documentoFiscalId
    if (!doc.chave) return null
    try {
      const resp = await useApi<CommandResult<DocumentoFiscalDto>>('/fiscal/documentos/chave/{chave}', {
        params: { chave: doc.chave }
      })
      const dados = extrairDados<DocumentoFiscalDto>(resp)
      return dados?.id ?? null
    } catch (e) {
      console.error('[useTransmissoes] falha ao resolver documentoFiscalId por chave', e)
      return null
    }
  }

  /**
   * Baixa o PDF do DANFE (NF-e) / cupom (NFC-e) via `GET fiscal/documentos/{id}/danfe`.
   * Documentos não autorizados saem marcados "SEM VALOR FISCAL" pelo próprio backend.
   * Devolve o Blob para a página abrir/baixar (nova aba ou download).
   */
  async function baixarDanfe(item: TransmissaoItem): Promise<Blob | null> {
    const doc = documentoVigente(item)
    baixando.value = true
    try {
      const id = await resolverDocumentoFiscalId(doc)
      if (!id) {
        toast.warning('Documento fiscal não localizado para gerar o DANFE.')
        return null
      }
      const blob = await useApi<Blob>('/fiscal/documentos/{id}/danfe', {
        params: { id },
        responseType: 'blob'
      })
      return blob
    } catch (e) {
      toast.error(obterMensagemErro(e))
      return null
    } finally {
      baixando.value = false
    }
  }

  /**
   * Gera/baixa o PDF da NF-e — mesmo endpoint do DANFE (`{id}/danfe`), que já gera o PDF sob
   * demanda quando ainda não há arquivo persistido. Mantido como ação separada por compatibilidade
   * com a UI; devolve o Blob.
   */
  async function gerarPdf(item: TransmissaoItem): Promise<Blob | null> {
    return baixarDanfe(item)
  }

  /**
   * Envia uma Carta de Correção Eletrônica (CC-e) via `POST fiscal/documentos/carta-correcao`
   * ({ documentoFiscalId, textoCorrecao }). O texto deve ter entre 15 e 1000 caracteres
   * (validação do backend). Retorna `true` em sucesso.
   */
  async function enviarCartaCorrecao(item: TransmissaoItem, textoCorrecao: string): Promise<boolean> {
    const doc = item.nfe ?? documentoVigente(item)
    baixando.value = true
    try {
      const id = await resolverDocumentoFiscalId(doc)
      if (!id) {
        toast.warning('Documento fiscal não localizado para a carta de correção.')
        return false
      }
      await useApi('/fiscal/documentos/carta-correcao', {
        method: 'POST',
        body: { documentoFiscalId: id, textoCorrecao }
      })
      toast.success('Carta de correção enviada com sucesso')
      return true
    } catch (e) {
      toast.error(obterMensagemErro(e))
      return false
    } finally {
      baixando.value = false
    }
  }

  /** Cancela a venda fiscal (NF-e/NFC-e autorizada) informando a justificativa. */
  async function cancelar(item: TransmissaoItem, justificativa: string): Promise<boolean> {
    cancelando.value = true
    try {
      const ehNfce = !!item.nfce && !item.nfe
      const rota = ehNfce ? '/vendas-fiscal/{id}/nfce/cancelar' : '/vendas-fiscal/{id}/nfe/cancelar'
      await useApi(rota, {
        method: 'POST',
        params: { id: String(item.id) },
        body: { vendaId: item.id, justificativa }
      })
      toast.success('Documento cancelado com sucesso')
      await lista.buscar()
      return true
    } catch (e) {
      toast.error(obterMensagemErro(e))
      return false
    } finally {
      cancelando.value = false
    }
  }

  /**
   * Exporta o relatório simplificado de vendas do período (planilha).
   * Devolve o Blob para a página disparar o download.
   */
  async function exportarPlanilha(): Promise<Blob | null> {
    if (EXPORTAR_PLANILHA_EM_IMPLEMENTACAO) {
      toast.error('Exportação de planilha em implementação — o backend ainda não expõe este endpoint.')
      return null
    }
    const f = lista.filtros.value
    if (!f.dataVendaInicial || !f.dataVendaFinal) {
      toast.warning('Preencha as datas inicial e final')
      return null
    }
    baixando.value = true
    try {
      const blob = await useApi<Blob>('/relatorios/vendas/simplificado01', {
        query: {
          dataVendaInicial: f.dataVendaInicial,
          dataVendaFinal: f.dataVendaFinal,
          status: f.status ?? undefined
        },
        responseType: 'blob'
      })
      return blob
    } catch (e) {
      toast.error(obterMensagemErro(e))
      return null
    } finally {
      baixando.value = false
    }
  }

  /**
   * Consulta as chaves de NF-e referenciadas de uma venda (para pré-preencher a
   * devolução/retorno). Endpoint `vendas-fiscal/{id}/nfe/referenciadas`.
   */
  async function obterReferenciadas(id: string | number): Promise<string[]> {
    try {
      const resp = await useApi<CommandResult<string[]>>('/vendas-fiscal/{id}/nfe/referenciadas', {
        params: { id: String(id) }
      })
      return extrairDados<string[]>(resp) ?? []
    } catch (e) {
      console.error('[useTransmissoes] falha ao obter referenciadas', e)
      return []
    }
  }

  return {
    // listagem
    itens: lista.itens,
    total: lista.total,
    pagina: lista.pagina,
    tamanhoPagina: lista.tamanhoPagina,
    ordenacao: lista.ordenacao,
    filtros: lista.filtros,
    carregando: lista.carregando,
    erro: lista.erro,
    buscar: lista.buscar,
    aplicar,
    limpar,
    // ações
    baixando,
    cancelando,
    baixarDanfe,
    gerarPdf,
    enviarCartaCorrecao,
    cancelar,
    exportarPlanilha,
    obterReferenciadas
  }
}
