/**
 * useEstoqueEnums — enums de domínio do módulo Estoque (opções de select + rótulos/badges).
 *
 * Centraliza os enums do módulo Estoque como constantes locais (valores fixos e estáveis,
 * espelhando `src/Modules/Epros.Modules.Estoque/Domain/Enums/*`), evitando dependência de rede
 * para filtros/badges das telas. Segue o contrato `SelectOption` de `useEnum` para reaproveitar
 * `SelectField`/`FilterBar`, e expõe helpers `label`/`badge` para renderização.
 *
 * Fonte dos valores: enums do backend (Domain/Enums do módulo Estoque). Se um enum ganhar/perder
 * membros no backend, atualizar aqui.
 */
import type { SelectOption } from './useEnum'

/** Cor semântica usada pelos badges das telas (classes `badge-*`). */
export type BadgeCor = 'success' | 'warning' | 'danger' | 'info' | 'muted'

export interface EnumMeta {
  value: number
  label: string
  cor: BadgeCor
}

function paraOpcoes(metas: EnumMeta[]): SelectOption[] {
  return metas.map((m) => ({ label: m.label, value: m.value }))
}

function criarHelpers(metas: EnumMeta[]) {
  const mapa = new Map(metas.map((m) => [m.value, m]))
  return {
    metas,
    opcoes: paraOpcoes(metas),
    label: (v: number | null | undefined): string => (v == null ? '-' : mapa.get(v)?.label ?? String(v)),
    cor: (v: number | null | undefined): BadgeCor => (v == null ? 'muted' : mapa.get(v)?.cor ?? 'muted')
  }
}

/** ETipoAjusteEstoque */
export const tipoAjuste = criarHelpers([
  { value: 0, label: 'Normal', cor: 'info' },
  { value: 1, label: 'Anormal', cor: 'warning' }
])

/** ETipoCusteioEstoque */
export const tipoCusteio = criarHelpers([
  { value: 0, label: 'Custo médio', cor: 'info' },
  { value: 1, label: 'PEPS (primeiro que entra, primeiro que sai)', cor: 'info' },
  { value: 2, label: 'UEPS (último que entra, primeiro que sai)', cor: 'info' }
])

/** EStatusPlanejamentoEstoque */
export const statusPlanejamento = criarHelpers([
  { value: 0, label: 'Normal', cor: 'success' },
  { value: 1, label: 'Em alerta de reposição', cor: 'danger' },
  { value: 2, label: 'Acima do máximo', cor: 'warning' },
  { value: 3, label: 'Sem política completa', cor: 'muted' }
])

/** ETipoInventario */
export const tipoInventario = criarHelpers([
  { value: 0, label: 'Geral', cor: 'info' },
  { value: 1, label: 'Cíclico', cor: 'info' },
  { value: 2, label: 'Parcial', cor: 'info' },
  { value: 3, label: 'Por produto', cor: 'info' },
  { value: 4, label: 'Por local', cor: 'info' }
])

/** ESituacaoInventario */
export const situacaoInventario = criarHelpers([
  { value: 0, label: 'Rascunho', cor: 'muted' },
  { value: 1, label: 'Em contagem', cor: 'info' },
  { value: 2, label: 'Em conferência', cor: 'warning' },
  { value: 3, label: 'Aprovado', cor: 'success' },
  { value: 4, label: 'Ajustado', cor: 'success' },
  { value: 5, label: 'Cancelado', cor: 'danger' }
])

/** EStatusLoteRastreabilidade */
export const statusLote = criarHelpers([
  { value: 0, label: 'Ativo', cor: 'success' },
  { value: 1, label: 'Bloqueado', cor: 'danger' },
  { value: 2, label: 'Consumido', cor: 'muted' },
  { value: 3, label: 'Vencido', cor: 'warning' },
  { value: 4, label: 'Cancelado', cor: 'danger' }
])

/** EStatusNumeroSerial */
export const statusSerial = criarHelpers([
  { value: 0, label: 'Disponível', cor: 'success' },
  { value: 1, label: 'Reservado', cor: 'info' },
  { value: 2, label: 'Consumido', cor: 'muted' },
  { value: 3, label: 'Bloqueado', cor: 'danger' },
  { value: 4, label: 'Cancelado', cor: 'danger' }
])

/** EOrigemLote */
export const origemLote = criarHelpers([
  { value: 0, label: 'Compra', cor: 'info' },
  { value: 1, label: 'Produção', cor: 'info' },
  { value: 2, label: 'Ajuste', cor: 'info' },
  { value: 3, label: 'Importação', cor: 'info' },
  { value: 4, label: 'Manual', cor: 'info' }
])

/** ETipoBloqueioLote */
export const tipoBloqueioLote = criarHelpers([
  { value: 0, label: 'Qualidade', cor: 'warning' },
  { value: 1, label: 'Recall', cor: 'danger' },
  { value: 2, label: 'Validade', cor: 'warning' },
  { value: 3, label: 'Manual', cor: 'muted' }
])

/** EStatusRecallLote */
export const statusRecall = criarHelpers([
  { value: 0, label: 'Aberto', cor: 'warning' },
  { value: 1, label: 'Em andamento', cor: 'info' },
  { value: 2, label: 'Concluído', cor: 'success' },
  { value: 3, label: 'Cancelado', cor: 'danger' }
])

/** ETipoAlertaEstoque */
export const tipoAlerta = criarHelpers([
  { value: 0, label: 'Reposição', cor: 'danger' },
  { value: 1, label: 'Excesso de máximo', cor: 'warning' }
])

/** EStatusAlertaEstoque */
export const statusAlerta = criarHelpers([
  { value: 0, label: 'Aberto', cor: 'warning' },
  { value: 1, label: 'Ignorado', cor: 'muted' },
  { value: 2, label: 'Resolvido', cor: 'success' }
])

/** EStatusConviteFornecedor */
export const statusConvite = criarHelpers([
  { value: 0, label: 'Rascunho', cor: 'muted' },
  { value: 1, label: 'Enviado', cor: 'info' },
  { value: 2, label: 'Aceito', cor: 'success' },
  { value: 3, label: 'Expirado', cor: 'warning' },
  { value: 4, label: 'Cancelado', cor: 'danger' }
])

/** EStatusCotacaoPublicada */
export const statusCotacao = criarHelpers([
  { value: 0, label: 'Aberta', cor: 'info' },
  { value: 1, label: 'Respondida', cor: 'success' },
  { value: 2, label: 'Em análise', cor: 'warning' },
  { value: 3, label: 'Encerrada', cor: 'muted' },
  { value: 4, label: 'Cancelada', cor: 'danger' }
])

/** EStatusRequisicaoInterna */
export const statusRequisicao = criarHelpers([
  { value: 0, label: 'Rascunho', cor: 'muted' },
  { value: 1, label: 'Confirmada', cor: 'info' },
  { value: 2, label: 'Atendida', cor: 'success' },
  { value: 3, label: 'Cancelada', cor: 'danger' }
])

/** Converte a cor semântica do enum na classe de badge usada nas telas. */
export function classeBadge(cor: BadgeCor): string {
  return `badge-${cor}`
}

export function useEstoqueEnums() {
  return {
    tipoAjuste,
    tipoCusteio,
    statusPlanejamento,
    tipoInventario,
    situacaoInventario,
    statusLote,
    statusSerial,
    origemLote,
    tipoBloqueioLote,
    statusRecall,
    tipoAlerta,
    statusAlerta,
    statusConvite,
    statusCotacao,
    statusRequisicao,
    classeBadge
  }
}
