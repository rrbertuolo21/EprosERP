import { useNuxtApp } from '#app'
import type { FetchOptions } from 'ofetch'

/**
 * Envelope padrão de resposta da API EprosERP (CommandResult).
 * A API pode devolver `dados` (padrão CommandResult) ou `data` (algumas rotas legadas)
 * e, em listagens, `totalRegistros`. Tipamos ambos como opcionais para os helpers.
 */
export interface CommandResult<T = unknown> {
  sucesso: boolean
  mensagem?: string | null
  dados?: T
  /** Alias usado por algumas rotas ao invés de `dados`. */
  data?: T
  /**
   * Lista de erros específicos. Em `CommandResult.Falha(string)` o back coloca a razão REAL da
   * recusa (regra de negócio) aqui, enquanto `mensagem` fica genérica ("Ocorreu um erro no
   * processamento."). Para exibir o motivo ao usuário, priorize `erros` sobre `mensagem`.
   */
  erros?: string[] | null
  /** Total de registros no servidor, presente nas listagens paginadas. */
  totalRegistros?: number
}

export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'

/** Prefixo global de todas as rotas da API. A baseURL (host) vem do runtimeConfig via plugin. */
export const API_PREFIX = '/api/v1'

export interface UseApiOptions<B = unknown> {
  /** Método HTTP. Default GET. */
  method?: HttpMethod
  /** Parâmetros de query string (?a=1&b=2). */
  query?: Record<string, unknown>
  /** Corpo da requisição (POST/PUT/PATCH). */
  body?: B
  /** Substituição de placeholders `{param}` na rota. Ex.: `/empresas/{id}` + { id: 5 }. */
  params?: Record<string, string | number>
  /** Cabeçalhos adicionais. */
  headers?: Record<string, string>
  /** Tipo de resposta esperado (ex.: 'blob' para download de PDF/XML). */
  responseType?: FetchOptions['responseType']
}

/**
 * Substitui placeholders `{chave}` na rota pelos valores de `params`.
 * Ex.: montarRota('/empresas/{id}/contatos', { id: 7 }) => '/empresas/7/contatos'
 */
function montarRota(path: string, params?: Record<string, string | number>): string {
  let rota = path.startsWith('/') ? path : `/${path}`
  if (params) {
    for (const [chave, valor] of Object.entries(params)) {
      rota = rota.replace(`{${chave}}`, encodeURIComponent(String(valor)))
    }
  }
  return `${API_PREFIX}${rota}`
}

/**
 * Cliente de API tipado — ÚNICO ponto de IO das telas.
 *
 * A baseURL (host) e a injeção de token/tenant são feitas pelo plugin `plugins/api.ts` (`$api`).
 * Aqui apenas prefixamos `/api/v1`, resolvemos `{param}` da rota e devolvemos o payload já tipado.
 *
 * Retorna diretamente o corpo da resposta (`T`). Para o envelope CommandResult completo,
 * tipar `T` como `CommandResult<Algo>` ou usar os helpers `useApi.get/post/...`.
 *
 * @example
 *   const empresa = await useApi<Empresa>('/cadastros/empresas/{id}', { params: { id } })
 *   await useApi('/cadastros/empresas/{id}', { method: 'DELETE', params: { id } })
 */
export async function useApi<T = unknown, B = unknown>(
  path: string,
  options: UseApiOptions<B> = {}
): Promise<T> {
  const { method = 'GET', query, body, params, headers, responseType } = options
  const rota = montarRota(path, params)

  const { $api } = useNuxtApp()

  // Cast necessário: nossa assinatura genérica de método (HttpMethod) é mais ampla que
  // o union literal esperado pelo $fetch. O runtime é idêntico.
  const opcoes = {
    method,
    query,
    body: body as Record<string, unknown> | undefined,
    headers,
    responseType
  } as FetchOptions

  return (await $api(rota, opcoes as never)) as T
}

/** Atalhos tipados get/post/put/patch/delete sobre `useApi`. */
useApi.get = <T = unknown>(path: string, options: Omit<UseApiOptions, 'method' | 'body'> = {}) =>
  useApi<T>(path, { ...options, method: 'GET' })

useApi.post = <T = unknown, B = unknown>(path: string, body?: B, options: Omit<UseApiOptions<B>, 'method' | 'body'> = {}) =>
  useApi<T, B>(path, { ...options, method: 'POST', body })

useApi.put = <T = unknown, B = unknown>(path: string, body?: B, options: Omit<UseApiOptions<B>, 'method' | 'body'> = {}) =>
  useApi<T, B>(path, { ...options, method: 'PUT', body })

useApi.patch = <T = unknown, B = unknown>(path: string, body?: B, options: Omit<UseApiOptions<B>, 'method' | 'body'> = {}) =>
  useApi<T, B>(path, { ...options, method: 'PATCH', body })

useApi.delete = <T = unknown>(path: string, options: Omit<UseApiOptions, 'method' | 'body'> = {}) =>
  useApi<T>(path, { ...options, method: 'DELETE' })

/**
 * Extrai o payload de dados de uma resposta, aceitando tanto `dados` (CommandResult)
 * quanto `data` (rotas legadas) ou o próprio corpo, quando a API não envelopa.
 */
export function extrairDados<T>(resposta: unknown): T | undefined {
  if (resposta && typeof resposta === 'object') {
    const r = resposta as CommandResult<T>
    if (r.dados !== undefined) return r.dados
    if (r.data !== undefined) return r.data
  }
  return resposta as T | undefined
}

/**
 * Extrai uma LISTA de uma resposta, tolerando os dois contratos que a API usa:
 *  - array cru em `dados`/`data` (endpoints que já devolvem lista);
 *  - página no formato `{ total, pagina, itens }` (handlers de listagem paginada).
 *
 * Use este helper (em vez de `extrairDados<Array<...>>`) sempre que o consumo for uma
 * lista de apoio para dropdown/autocomplete. Fazer `.map` direto no `dados` paginado
 * gera `TypeError: .map is not a function` e quebra o formulário inteiro (Classe B).
 * Retorna sempre um array (vazio quando não há dados), então é seguro encadear `.map`.
 */
export function extrairLista<T>(resposta: unknown): T[] {
  const d = extrairDados<unknown>(resposta)
  if (Array.isArray(d)) return d as T[]
  if (d && typeof d === 'object') {
    const o = d as { itens?: T[]; Itens?: T[] }
    if (Array.isArray(o.itens)) return o.itens
    if (Array.isArray(o.Itens)) return o.Itens
  }
  return []
}
