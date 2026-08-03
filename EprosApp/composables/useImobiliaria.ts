import { useApi, extrairDados } from '~/composables/useApi'

/**
 * useImobiliaria — IO do submódulo IMO-001 (Gestão Imobiliária).
 *
 * Cobre o que as telas de gestão do contrato precisam além do CRUD básico de locação:
 * ciclo financeiro do aluguel (cobrança/baixa/estorno/recibo — ID8/NF-01/NF-05), garantias
 * (ID6/NF-04), reajuste e rescisão (ID7/NF-02), transições de estado do contrato
 * (formalizar/encerrar/cancelar/renovar) e o fluxo de propostas (ID2).
 *
 * ⚠️ Apresentação apenas. Nenhuma regra de negócio é decidida aqui: juros/multa/desconto e a
 * máquina do recebível vivem no FINANCEIRO; índice/percentual de reajuste e cálculo de multa de
 * rescisão são valida-contador. As telas só disparam os comandos e mostram o resumo devolvido.
 *
 * Endpoints conferidos em ImobiliariaController.cs (rota base `api/v1/imobiliaria`).
 */

// ---- Enums (espelho de ImobiliariaEnums.cs) --------------------------------
export const TIPO_GARANTIA: Record<number, string> = {
  0: 'Fiador',
  1: 'Caução',
  2: 'Seguro-fiança',
  3: 'Garantia bancária'
}
export const STATUS_GARANTIA: Record<number, string> = {
  0: 'Ativa',
  1: 'Substituída',
  2: 'Liberada'
}
export const TIPO_COBRANCA: Record<number, string> = {
  0: 'Aluguel',
  1: 'Encargo',
  2: 'Multa',
  3: 'Reajuste'
}
export const STATUS_COBRANCA: Record<number, string> = {
  0: 'Em aberto',
  1: 'Parcial',
  2: 'Pago',
  3: 'Estornado'
}
export const TIPO_PROPOSTA: Record<number, string> = {
  0: 'Locação',
  1: 'Aquisição'
}
export const STATUS_PROPOSTA: Record<number, string> = {
  0: 'Rascunho',
  1: 'Aprovada',
  2: 'Rejeitada',
  3: 'Expirada',
  4: 'Convertida'
}
export const PAPEL_PARTE_PROPOSTA: Record<number, string> = {
  0: 'Proponente',
  1: 'Interessado'
}
export const STATUS_LOCACAO: Record<number, string> = {
  0: 'Em elaboração',
  1: 'Vigente',
  2: 'Encerrada',
  3: 'Cancelada'
}

export function rotularEnum(mapa: Record<number, string>, valor: unknown): string {
  if (valor === null || valor === undefined) return '—'
  const n = Number(valor)
  if (!Number.isNaN(n) && mapa[n] !== undefined) return mapa[n]
  return String(valor)
}

// ---- Tipos de leitura -------------------------------------------------------
export interface CobrancaAluguel {
  id: string
  locacaoId: string
  competencia: string
  tipo: number
  valor: number
  vencimento: number
  status: number
  valorPago: number
  receberRef?: string | null
  dataBaixa?: string | null
}
export interface LocacaoGarantia {
  id: string
  locacaoId: string
  tipo: number
  valorLimite: number
  vigenciaInicio?: string | null
  vigenciaFim?: string | null
  status: number
  descricao?: string | null
  substituiId?: string | null
  fiadorPessoaId?: string | null
}
export interface LocacaoReajuste {
  id: string
  locacaoId: string
  indice?: string | null
  dataBase: string
  valorAnterior: number
  valorNovo: number
  percentualAplicado?: number | null
}
export interface PropostaParte {
  id: string
  propostaId: string
  pessoaId: string
  papel: number
}
export interface Proposta {
  id: string
  tipo: number
  imovelId: string
  status: number
  validade: string
  valorProposto: number
  observacao?: string | null
  contrapropostaDeId?: string | null
  locacaoGeradaId?: string | null
  partes?: PropostaParte[]
}
export interface ResumoAluguel {
  locacaoId: string
  imovelId?: string | null
  valor?: number | null
  vencimento?: number | null
  periodoInicial?: string | null
  periodoFinal?: string | null
  status?: string | null
  locatarios?: string[]
  fiadores?: string[]
}

export function useImobiliaria() {
  async function get<T>(path: string): Promise<T | null> {
    const r = await useApi(path, { method: 'GET' })
    return extrairDados<T>(r) ?? null
  }
  const post = <B = unknown>(path: string, body?: B) => useApi(path, { method: 'POST', body })

  // ------- Resumo da locação -------
  const resumoAluguel = (locacaoId: string) => get<ResumoAluguel>(`/imobiliaria/locacoes/${locacaoId}/resumo-aluguel`)

  // ------- Ciclo de vida do contrato (ID1/PRD-01) -------
  const formalizar = (locacaoId: string) => post(`/imobiliaria/locacoes/${locacaoId}/formalizar`)
  const encerrar = (locacaoId: string) => post(`/imobiliaria/locacoes/${locacaoId}/encerrar`)
  const cancelar = (locacaoId: string) => post(`/imobiliaria/locacoes/${locacaoId}/cancelar`)
  const renovar = (locacaoId: string, body: { novoPeriodoFinal: string | null }) =>
    post(`/imobiliaria/locacoes/${locacaoId}/renovar`, body)

  // ------- Reajuste (ID7/NF-02) -------
  const listarReajustes = (locacaoId: string) => get<LocacaoReajuste[]>(`/imobiliaria/locacoes/${locacaoId}/reajustes`)
  const aplicarReajuste = (
    locacaoId: string,
    body: { novoValor: number; indice?: string | null; dataBase?: string | null; percentualAplicado?: number | null }
  ) => post(`/imobiliaria/locacoes/${locacaoId}/reajustar`, body)

  // ------- Rescisão (ID7) -------
  const rescindir = (
    locacaoId: string,
    body: { motivo: string; dataRescisao: string; avisoPrevioDias?: number | null; multaProporcional?: number }
  ) => post(`/imobiliaria/locacoes/${locacaoId}/rescindir`, body)

  // ------- Ciclo financeiro do aluguel (ID8/NF-01) -------
  const listarCobrancas = (locacaoId: string) => get<CobrancaAluguel[]>(`/imobiliaria/locacoes/${locacaoId}/cobrancas`)
  const gerarCobranca = (
    locacaoId: string,
    body: { ano: number; mes: number; tipo?: number; valorOverride?: number | null }
  ) => post(`/imobiliaria/locacoes/${locacaoId}/cobrancas`, body)
  const refletirBaixa = (
    cobrancaId: string,
    body: { valorPago: number; receberRef?: string | null; dataBaixa?: string | null }
  ) => post(`/imobiliaria/cobrancas/${cobrancaId}/baixa`, body)
  const estornarBaixa = (cobrancaId: string) => post(`/imobiliaria/cobrancas/${cobrancaId}/estorno`)
  const emitirRecibo = (cobrancaId: string) => post(`/imobiliaria/cobrancas/${cobrancaId}/recibo`)

  // ------- Garantias (ID6/NF-04) -------
  const listarGarantias = (locacaoId: string) => get<LocacaoGarantia[]>(`/imobiliaria/locacoes/${locacaoId}/garantias`)
  const adicionarGarantia = (
    locacaoId: string,
    body: {
      tipo: number
      valorLimite: number
      vigenciaInicio?: string | null
      vigenciaFim?: string | null
      descricao?: string | null
      fiadorPessoaId?: string | null
    }
  ) => post(`/imobiliaria/locacoes/${locacaoId}/garantias`, body)
  const substituirGarantia = (
    garantiaId: string,
    body: {
      tipo: number
      valorLimite: number
      vigenciaInicio?: string | null
      vigenciaFim?: string | null
      descricao?: string | null
      fiadorPessoaId?: string | null
    }
  ) => post(`/imobiliaria/garantias/${garantiaId}/substituir`, body)
  const liberarGarantia = (garantiaId: string) => post(`/imobiliaria/garantias/${garantiaId}/liberar`)

  // ------- Propostas (ID2) -------
  const listarPropostas = (imovelId?: string | null) =>
    get<Proposta[]>(`/imobiliaria/propostas${imovelId ? `?imovelId=${imovelId}` : ''}`)
  const obterProposta = (id: string) => get<Proposta>(`/imobiliaria/propostas/${id}`)
  const criarProposta = (body: {
    tipo: number
    imovelId: string
    validade: string
    valorProposto: number
    observacao?: string | null
    contrapropostaDeId?: string | null
    partes?: { pessoaId: string; papel: number }[]
  }) => post('/imobiliaria/propostas', body)
  const aprovarProposta = (id: string) => post(`/imobiliaria/propostas/${id}/aprovar`)
  const rejeitarProposta = (id: string) => post(`/imobiliaria/propostas/${id}/rejeitar`)
  const converterProposta = (
    id: string,
    body: { periodoInicial?: string | null; periodoFinal?: string | null; vencimento?: number }
  ) => post(`/imobiliaria/propostas/${id}/converter`, body)

  return {
    resumoAluguel,
    formalizar,
    encerrar,
    cancelar,
    renovar,
    listarReajustes,
    aplicarReajuste,
    rescindir,
    listarCobrancas,
    gerarCobranca,
    refletirBaixa,
    estornarBaixa,
    emitirRecibo,
    listarGarantias,
    adicionarGarantia,
    substituirGarantia,
    liberarGarantia,
    listarPropostas,
    obterProposta,
    criarProposta,
    aprovarProposta,
    rejeitarProposta,
    converterProposta
  }
}
