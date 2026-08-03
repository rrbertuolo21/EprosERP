/**
 * Opções e rótulos de enums do módulo PROJETOS.
 *
 * Os enums do backend são serializados como número; montamos as opções a partir do
 * digest/domínio (Epros.Modules.Projetos.Domain.Enums) já que não há rota de enums dedicada.
 */
import type { SelectOption } from '~/composables/useEnum'
import { useApi, extrairDados } from '~/composables/useApi'

/**
 * Carrega os projetos (GET /projetos) e devolve opções { label: nome, value: id }
 * para os selects das telas "por projeto" (orçamento, tarefas, riscos, alocações).
 */
export async function carregarProjetosOpcoes(): Promise<SelectOption[]> {
  try {
    const resposta = await useApi('/projetos')
    const dados = extrairDados<Array<{ id: string; nome?: string | null }>>(resposta) ?? []
    return dados.map((p) => ({ label: p.nome ?? p.id, value: p.id }))
  } catch (e) {
    console.error('[projetos-shared] carregarProjetosOpcoes', e)
    return []
  }
}

/** EProjetoWorkflowStatus — workflow canônico (Orçamento/Recursos/Faturamento/Portfólio/Encerramento/Risco). */
export const STATUS_WORKFLOW_OPCOES: SelectOption[] = [
  { value: 0, label: 'Rascunho' },
  { value: 1, label: 'Em análise' },
  { value: 2, label: 'Ativo' },
  { value: 3, label: 'Suspenso' },
  { value: 4, label: 'Encerrado' },
  { value: 5, label: 'Inativo' },
  { value: 6, label: 'Cancelado' }
]

export function rotuloStatusWorkflow(v: unknown): string {
  const found = STATUS_WORKFLOW_OPCOES.find((o) => String(o.value) === String(v))
  return found ? found.label : String(v ?? '—')
}

/** EModalidadeFaturamento */
export const MODALIDADE_FATURAMENTO_OPCOES: SelectOption[] = [
  { value: 0, label: 'Marco' },
  { value: 1, label: 'Tempo e material (T&M)' },
  { value: 2, label: 'Despesa' },
  { value: 3, label: 'Preço fixo' }
]

/** ETipoItemFaturamento */
export const TIPO_ITEM_FATURAMENTO_OPCOES: SelectOption[] = [
  { value: 0, label: 'Marco' },
  { value: 1, label: 'Hora' },
  { value: 2, label: 'Despesa' },
  { value: 3, label: 'Parcela' },
  { value: 4, label: 'Ajuste' }
]

/** EBillingType (orçamento) */
export const BILLING_TYPE_OPCOES: SelectOption[] = [
  { value: 0, label: 'Por hora' },
  { value: 1, label: 'Fixo' }
]

/** EMarcoStatus */
export const MARCO_STATUS_OPCOES: SelectOption[] = [
  { value: 0, label: 'Incompleto' },
  { value: 1, label: 'Completo' }
]

/** ETimesheetTipo (apontamento) */
export const TIMESHEET_TIPO_OPCOES: SelectOption[] = [
  { value: 0, label: 'Ponto (entrada/saída)' },
  { value: 1, label: 'Projeto' },
  { value: 2, label: 'Manual' }
]

/** ETipoDependencia (tarefas) */
export const TIPO_DEPENDENCIA_OPCOES: SelectOption[] = [
  { value: 0, label: 'Fim → Início' },
  { value: 1, label: 'Início → Início' },
  { value: 2, label: 'Fim → Fim' },
  { value: 3, label: 'Início → Fim' }
]

/** EPrioridadeRisco */
export const PRIORIDADE_RISCO_OPCOES: SelectOption[] = [
  { value: 0, label: 'Baixa' },
  { value: 1, label: 'Média' },
  { value: 2, label: 'Alta' }
]

export function rotuloPrioridadeRisco(v: unknown): string {
  const found = PRIORIDADE_RISCO_OPCOES.find((o) => String(o.value) === String(v))
  return found ? found.label : String(v ?? '—')
}

/** EStatusFinalProjeto (encerramento) */
export const STATUS_FINAL_PROJETO_OPCOES: SelectOption[] = [
  { value: 0, label: 'Não iniciado' },
  { value: 1, label: 'Em andamento' },
  { value: 2, label: 'Suspenso' },
  { value: 3, label: 'Cancelado' },
  { value: 4, label: 'Concluído' },
  { value: 5, label: 'Arquivado' }
]

/** Formatação utilitária compartilhada pelas telas do módulo. */
export function fmtData(v: unknown): string {
  if (!v) return '—'
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? '—' : d.toLocaleDateString('pt-BR')
}
export function fmtMoeda(v: unknown): string {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(Number(v ?? 0))
}
