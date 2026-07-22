/**
 * useFiscalReferencias — seletores de dados fiscais de referência (NCM, CEST, CFOP).
 *
 * Porta o comportamento dos composables `fiscal/useNcm.ts`, `fiscal/useCest.ts`,
 * `vendas/useFetchCFOP.ts` do legado para o padrão EprosERP (`useApiList`, envelope
 * CommandResult). São as tabelas de apoio que os diálogos de produto/tributação da emissão
 * de NF-e consomem para autocompletar NCM, CEST e CFOP.
 *
 * Contrato de rota (Listar aceita `localizar` + paginação):
 *   GET api/v1/fiscal/ncms   → { total, pagina, itens }
 *   GET api/v1/fiscal/cests  → { total, pagina, itens }
 *   GET api/v1/cfops         → { total, pagina, itens }
 *
 * Cada seletor expõe `itens`, `carregando`, `buscar(localizar?)` e é independente — a tela
 * usa só o que precisa (`const { ncm, cest, cfop } = useFiscalReferencias()`).
 */
import { useApiList } from './useApiList'

export interface NcmRef {
  id: string
  codigo: string
  descricao: string
}

export interface CestRef {
  id: string
  codigo: string
  descricao: string
  ncm?: string | null
}

export interface CfopRef {
  id: string
  codigo: string
  descricao: string
  aplicacao?: string | null
}

interface FiltroLocalizar {
  localizar?: string
  [key: string]: unknown
}

/** Fábrica de seletor paginado com busca por texto (`localizar`). */
function criarSeletor<T>(rota: string) {
  const lista = useApiList<T, FiltroLocalizar>(rota, { tamanhoPaginaInicial: 20 })

  /** Busca aplicando o texto em `localizar` e voltando à página 1. */
  async function buscar(localizar?: string): Promise<void> {
    await lista.aplicarFiltros({ localizar: localizar ?? '' })
  }

  return {
    itens: lista.itens,
    total: lista.total,
    pagina: lista.pagina,
    tamanhoPagina: lista.tamanhoPagina,
    carregando: lista.carregando,
    erro: lista.erro,
    filtros: lista.filtros,
    buscar,
    buscarPagina: lista.buscar
  }
}

export function useFiscalReferencias() {
  const ncm = criarSeletor<NcmRef>('/fiscal/ncms')
  const cest = criarSeletor<CestRef>('/fiscal/cests')
  const cfop = criarSeletor<CfopRef>('/cfops')

  return { ncm, cest, cfop }
}

/** Seletores individuais (quando a tela precisa só de um). */
export function useNcm() {
  return criarSeletor<NcmRef>('/fiscal/ncms')
}
export function useCest() {
  return criarSeletor<CestRef>('/fiscal/cests')
}
export function useCfop() {
  return criarSeletor<CfopRef>('/cfops')
}
