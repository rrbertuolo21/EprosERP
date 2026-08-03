import { useApi, extrairDados } from '~/composables/useApi'
import { useHelper } from '~/composables/useHelper'

/**
 * useRelatorios — camada de leitura dos módulos de RELATÓRIOS (RPT).
 *
 * Concentra o IO das telas de relatórios operacionais (RPT-OPB, `api/v1/relatorios/operacionais/*`)
 * e do BI gerencial (RPT-ONM, `api/v1/relatorios/bi/*`). Somente leitura (query-side): as telas
 * só apresentam — nenhuma regra de negócio nova mora aqui. Cada função devolve o DTO já
 * desembrulhado do envelope CommandResult via `extrairDados`.
 *
 * Endpoints conferidos em:
 *   - RelatoriosOperacionaisController.cs  (rota base `api/v1/relatorios/operacionais`)
 *   - RelatoriosBiController.cs            (rota base `api/v1/relatorios/bi`)
 * DTOs em RelatoriosOperacionaisDtos.cs / BiDtos.cs.
 */

// ---- Filtro comum de período -----------------------------------------------
export interface PeriodoFiltro {
  inicio: string | null
  fim: string | null
}

// ---- ESTOQUE ----------------------------------------------------------------
export interface PosicaoEstoqueLinha {
  produtoId: string
  sku: string
  produto: string
  categoriaId?: string | null
  categoria?: string | null
  saldoEstoque: number
  estoqueMinimo: number
  estoqueReservado: number
  valorSaldo: number
  custoMedio: number
  abaixoDoMinimo: boolean
}
export interface PosicaoEstoqueRelatorio {
  linhas: PosicaoEstoqueLinha[]
  totalSaldoEstoque: number
  totalValorSaldo: number
  quantidadeItens: number
}

export interface GiroEstoqueLinha {
  produtoId: string
  produto: string
  quantidadeSaida: number
  saldoAtual: number
  giro: number
}
export interface GiroEstoqueRelatorio {
  linhas: GiroEstoqueLinha[]
  totalSaida: number
}

// ---- VENDAS -----------------------------------------------------------------
export interface VendasPorPeriodoLinha {
  dia: string
  quantidadeVendas: number
  valorTotal: number
}
export interface VendasPorPeriodoRelatorio {
  linhas: VendasPorPeriodoLinha[]
  valorTotal: number
  quantidadeVendas: number
}

export interface VendasPorProdutoLinha {
  produtoId: string
  produto: string
  quantidade: number
  valorTotal: number
}
export interface VendasPorProdutoRelatorio {
  linhas: VendasPorProdutoLinha[]
  valorTotal: number
}

export interface VendasPorClienteLinha {
  clienteId?: string | null
  quantidadeVendas: number
  valorTotal: number
}
export interface VendasPorClienteRelatorio {
  linhas: VendasPorClienteLinha[]
  valorTotal: number
}

// ---- FINANCEIRO — AGING -----------------------------------------------------
export interface AgingFaixa {
  faixa: string
  quantidadeTitulos: number
  saldoDevido: number
}
export interface AgingRelatorio {
  natureza: string
  dataReferencia: string
  faixas: AgingFaixa[]
  saldoTotal: number
  saldoVencido: number
  saldoAVencer: number
}

// ---- FINANCEIRO — FLUXO DE CAIXA -------------------------------------------
export interface FluxoCaixaLinha {
  dia: string
  entradas: number
  saidas: number
  saldo: number
}
export interface FluxoCaixaRelatorio {
  linhas: FluxoCaixaLinha[]
  totalEntradas: number
  totalSaidas: number
  saldoLiquido: number
}

// ---- BI — KPIs / SÉRIES -----------------------------------------------------
export interface KpiValor {
  kpi: string
  valor: number
  unidade: string
  formula: string
  inicio?: string | null
  fim?: string | null
}
export interface SerieTemporalPonto {
  periodo: string
  valor: number
}
export interface SerieTemporal {
  kpi: string
  unidade: string
  granularidade: string
  pontos: SerieTemporalPonto[]
  total: number
}
export interface MargemKpi {
  receita: number
  custo: number
  margemValor: number
  margemPercentual: number
  inicio?: string | null
  fim?: string | null
}
export interface InadimplenciaKpi {
  saldoTotal: number
  saldoVencido: number
  percentualInadimplencia: number
  dataReferencia: string
}
export interface RupturaKpi {
  totalItens: number
  itensEmRuptura: number
  itensAbaixoMinimo: number
  percentualRuptura: number
}
export interface PainelGerencial {
  faturamentoPeriodo: number
  margem: MargemKpi
  inadimplencia: InadimplenciaKpi
  ruptura: RupturaKpi
  saldoCaixaLiquido: number
  inicio?: string | null
  fim?: string | null
}
export interface TopProduto {
  produtoId: string
  produto: string
  quantidade: number
  valorTotal: number
}
export interface TopProdutos {
  criterio: string
  topN: number
  itens: TopProduto[]
}

export function useRelatorios() {
  const { paraIsoData } = useHelper()

  /** Primeiro dia do mês corrente (ISO YYYY-MM-DD). */
  function primeiroDiaMes(): string {
    const h = new Date()
    return paraIsoData(new Date(h.getFullYear(), h.getMonth(), 1)) ?? ''
  }
  /** Último dia do mês corrente (ISO YYYY-MM-DD). */
  function ultimoDiaMes(): string {
    const h = new Date()
    return paraIsoData(new Date(h.getFullYear(), h.getMonth() + 1, 0)) ?? ''
  }
  /** Período padrão: mês corrente. */
  function periodoMesCorrente(): PeriodoFiltro {
    return { inicio: primeiroDiaMes(), fim: ultimoDiaMes() }
  }

  async function get<T>(path: string, query?: Record<string, unknown>): Promise<T | null> {
    const resposta = await useApi(path, { method: 'GET', query })
    return extrairDados<T>(resposta) ?? null
  }

  // ------- Operacionais: ESTOQUE -------
  const posicaoEstoque = (f: { categoriaId?: string | null; produtoId?: string | null; somenteAbaixoMinimo?: boolean } = {}) =>
    get<PosicaoEstoqueRelatorio>('/relatorios/operacionais/estoque/posicao', {
      categoriaId: f.categoriaId || undefined,
      produtoId: f.produtoId || undefined,
      somenteAbaixoMinimo: f.somenteAbaixoMinimo ?? false
    })

  const giroEstoque = (f: PeriodoFiltro & { produtoId?: string | null }) =>
    get<GiroEstoqueRelatorio>('/relatorios/operacionais/estoque/giro', {
      inicio: f.inicio || undefined,
      fim: f.fim || undefined,
      produtoId: f.produtoId || undefined
    })

  // ------- Operacionais: VENDAS -------
  const vendasPorPeriodo = (f: PeriodoFiltro & { incluirCanceladas?: boolean }) =>
    get<VendasPorPeriodoRelatorio>('/relatorios/operacionais/vendas/por-periodo', {
      inicio: f.inicio || undefined,
      fim: f.fim || undefined,
      incluirCanceladas: f.incluirCanceladas ?? false
    })

  const vendasPorProduto = (f: PeriodoFiltro & { produtoId?: string | null; incluirCanceladas?: boolean }) =>
    get<VendasPorProdutoRelatorio>('/relatorios/operacionais/vendas/por-produto', {
      inicio: f.inicio || undefined,
      fim: f.fim || undefined,
      produtoId: f.produtoId || undefined,
      incluirCanceladas: f.incluirCanceladas ?? false
    })

  const vendasPorCliente = (f: PeriodoFiltro & { clienteId?: string | null; incluirCanceladas?: boolean }) =>
    get<VendasPorClienteRelatorio>('/relatorios/operacionais/vendas/por-cliente', {
      inicio: f.inicio || undefined,
      fim: f.fim || undefined,
      clienteId: f.clienteId || undefined,
      incluirCanceladas: f.incluirCanceladas ?? false
    })

  // ------- Operacionais: FINANCEIRO -------
  const agingReceber = (dataReferencia?: string | null) =>
    get<AgingRelatorio>('/relatorios/operacionais/financeiro/aging-receber', { dataReferencia: dataReferencia || undefined })

  const agingPagar = (dataReferencia?: string | null) =>
    get<AgingRelatorio>('/relatorios/operacionais/financeiro/aging-pagar', { dataReferencia: dataReferencia || undefined })

  const fluxoCaixa = (f: PeriodoFiltro & { incluirPlanejamento?: boolean }) =>
    get<FluxoCaixaRelatorio>('/relatorios/operacionais/financeiro/fluxo-caixa', {
      inicio: f.inicio || undefined,
      fim: f.fim || undefined,
      incluirPlanejamento: f.incluirPlanejamento ?? false
    })

  // ------- BI -------
  const kpiFaturamento = (f: PeriodoFiltro) =>
    get<KpiValor>('/relatorios/bi/kpi/faturamento', { inicio: f.inicio || undefined, fim: f.fim || undefined })

  const serieFaturamento = (f: PeriodoFiltro & { granularidade?: string }) =>
    get<SerieTemporal>('/relatorios/bi/serie/faturamento', {
      inicio: f.inicio || undefined,
      fim: f.fim || undefined,
      granularidade: f.granularidade ?? 'Dia'
    })

  const kpiMargem = (f: PeriodoFiltro) =>
    get<MargemKpi>('/relatorios/bi/kpi/margem', { inicio: f.inicio || undefined, fim: f.fim || undefined })

  const kpiInadimplencia = (dataReferencia?: string | null) =>
    get<InadimplenciaKpi>('/relatorios/bi/kpi/inadimplencia', { dataReferencia: dataReferencia || undefined })

  const kpiRuptura = () => get<RupturaKpi>('/relatorios/bi/kpi/ruptura')

  const topProdutos = (f: PeriodoFiltro & { topN?: number; criterio?: string }) =>
    get<TopProdutos>('/relatorios/bi/top-produtos', {
      inicio: f.inicio || undefined,
      fim: f.fim || undefined,
      topN: f.topN ?? 10,
      criterio: f.criterio ?? 'Valor'
    })

  const painelGerencial = (f: PeriodoFiltro) =>
    get<PainelGerencial>('/relatorios/bi/painel-gerencial', { inicio: f.inicio || undefined, fim: f.fim || undefined })

  return {
    primeiroDiaMes,
    ultimoDiaMes,
    periodoMesCorrente,
    posicaoEstoque,
    giroEstoque,
    vendasPorPeriodo,
    vendasPorProduto,
    vendasPorCliente,
    agingReceber,
    agingPagar,
    fluxoCaixa,
    kpiFaturamento,
    serieFaturamento,
    kpiMargem,
    kpiInadimplencia,
    kpiRuptura,
    topProdutos,
    painelGerencial
  }
}
