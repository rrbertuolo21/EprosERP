import { useApi, extrairDados } from '~/composables/useApi'
import { useAuth } from '~/composables/useAuth'

/**
 * usePdvCaixa — IO da gestão de caixa do PDV (abertura/fechamento, sangria/suprimento).
 *
 * Porta as rotas reais do módulo Vendas (controller `VendasController`, base `/vendas`):
 *   GET  /vendas/caixas/status?operadorId=...        -> situação do caixa do operador
 *   GET  /vendas/caixas/{id}/detalhado               -> saldos + movimentos do caixa
 *   POST /vendas/caixas/abrir      { operadorId, saldoAbertura }
 *   POST /vendas/caixas/fechar     { caixaId, saldoFechamento }
 *   POST /vendas/caixas/movimentar { caixaId, tipo: 'Suprimento'|'Sangria', valor, observacao }
 *
 * Toda IO passa pelo `useApi` compartilhado (prefixo /api/v1, token/tenant no plugin).
 * Sem regra de negócio nova: apenas apresentação/orquestração das chamadas.
 */

/** Situação retornada por GET /vendas/caixas/status. */
export interface CaixaStatus {
  status: 'Aberto' | 'Fechado' | string
  id?: string | null
  operadorId?: string | null
  saldoAbertura?: number | null
  criadoEm?: string | null
}

/** Movimento (sangria/suprimento) de um caixa. */
export interface CaixaMovimento {
  id: string
  tipo: string
  valor: number
  observacao?: string | null
  criadoEm?: string | null
}

/** Detalhamento retornado por GET /vendas/caixas/{id}/detalhado. */
export interface CaixaDetalhado {
  id: string
  operadorId?: string | null
  status?: number | string | null
  saldoAbertura?: number | null
  saldoFechamento?: number | null
  diferencaFechamento?: number | null
  fechadoEm?: string | null
  criadoEm?: string | null
  totalVendas?: number | null
  totalVendasDinheiro?: number | null
  totalSuprimentos?: number | null
  totalSangrias?: number | null
  saldoCalculadoDinheiro?: number | null
  movimentos?: CaixaMovimento[]
}

export type TipoMovimentoCaixa = 'Suprimento' | 'Sangria'

export function usePdvCaixa() {
  const { getUser } = useAuth()

  /** Id do operador logado (usado como chave do caixa). */
  function operadorAtual(): string {
    const u = getUser()
    return String(u?.id ?? u?.email ?? '')
  }

  async function obterStatus(operadorId: string): Promise<CaixaStatus> {
    const resposta = await useApi('/vendas/caixas/status', { query: { operadorId } })
    const dados = extrairDados<CaixaStatus>(resposta)
    // Rota devolve envelope CommandResult; normaliza chaves (Status/status).
    const raw = (dados ?? resposta) as Record<string, unknown>
    return {
      status: String(raw.status ?? raw.Status ?? 'Fechado'),
      id: (raw.id ?? raw.Id ?? null) as string | null,
      operadorId: (raw.operadorId ?? raw.OperadorId ?? null) as string | null,
      saldoAbertura: (raw.saldoAbertura ?? raw.SaldoAbertura ?? null) as number | null,
      criadoEm: (raw.criadoEm ?? raw.CriadoEm ?? null) as string | null
    }
  }

  async function obterDetalhado(caixaId: string): Promise<CaixaDetalhado | null> {
    const resposta = await useApi('/vendas/caixas/{id}/detalhado', { params: { id: caixaId } })
    return extrairDados<CaixaDetalhado>(resposta)
  }

  async function abrir(operadorId: string, saldoAbertura: number): Promise<void> {
    await useApi('/vendas/caixas/abrir', { method: 'POST', body: { operadorId, saldoAbertura } })
  }

  async function fechar(caixaId: string, saldoFechamento: number): Promise<void> {
    await useApi('/vendas/caixas/fechar', { method: 'POST', body: { caixaId, saldoFechamento } })
  }

  async function movimentar(
    caixaId: string,
    tipo: TipoMovimentoCaixa,
    valor: number,
    observacao?: string | null
  ): Promise<void> {
    await useApi('/vendas/caixas/movimentar', {
      method: 'POST',
      body: { caixaId, tipo, valor, observacao: observacao ?? null }
    })
  }

  return { operadorAtual, obterStatus, obterDetalhado, abrir, fechar, movimentar }
}
