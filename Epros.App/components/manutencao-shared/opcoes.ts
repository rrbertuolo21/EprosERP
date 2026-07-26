/**
 * Opções de enum e helpers compartilhados do módulo MANUTENÇÃO.
 *
 * Os enums abaixo são portados de `Domain/Enums/ManutencaoEnums.cs` (backend), pois o
 * módulo não expõe rota de enums. Os valores (`value`) são o backing int do enum C#.
 * Os campos `obj` do digest de API correspondem a esses enums e são enviados como inteiro.
 */
import { useApi, extrairDados } from '~/composables/useApi'
import type { SelectOption } from '~/composables/useEnum'

// Ciclo de vida padrão (EStatusRegistroManutencao)
export const statusRegistroOpcoes: SelectOption[] = [
  { value: 0, label: 'Rascunho' },
  { value: 1, label: 'Em análise' },
  { value: 2, label: 'Ativo' },
  { value: 3, label: 'Suspenso' },
  { value: 4, label: 'Encerrado' },
  { value: 5, label: 'Inativo' }
]

export function rotuloStatusRegistro(valor: unknown): string {
  const n = typeof valor === 'number' ? valor : Number(valor)
  return statusRegistroOpcoes.find((o) => o.value === n)?.label ?? String(valor ?? '')
}

// EPerfilOrdem
export const perfilOrdemOpcoes: SelectOption[] = [
  { value: 0, label: 'Oficina' },
  { value: 1, label: 'Campo' }
]

// EStatusOrdemServico
export const statusOrdemServicoOpcoes: SelectOption[] = [
  { value: 1, label: 'Aberta' },
  { value: 2, label: 'Em orçamento' },
  { value: 3, label: 'Aprovada' },
  { value: 4, label: 'Montagem' },
  { value: 5, label: 'Pronta' },
  { value: 6, label: 'Entregue' },
  { value: 7, label: 'Cancelada' }
]

// ETipoItemOrdemServico
export const tipoItemOsOpcoes: SelectOption[] = [
  { value: 0, label: 'Produto' },
  { value: 1, label: 'Serviço' }
]

// ETipoSaidaItem
export const tipoSaidaItemOpcoes: SelectOption[] = [
  { value: 0, label: 'Venda' },
  { value: 1, label: 'Troca' },
  { value: 2, label: 'Bonificação' },
  { value: 3, label: 'Comodato' }
]

// TipoPessoa (int, sem enum no backend — mapeamento padrão do ERP)
export const tipoPessoaOpcoes: SelectOption[] = [
  { value: 1, label: 'Física' },
  { value: 2, label: 'Jurídica' }
]

// ETipoParada
export const tipoParadaOpcoes: SelectOption[] = [
  { value: 0, label: 'Planejada' },
  { value: 1, label: 'Não planejada' },
  { value: 2, label: 'Setup' }
]

// ETipoIndicadorConfiabilidade
export const tipoIndicadorConfiabilidadeOpcoes: SelectOption[] = [
  { value: 0, label: 'MTTR' },
  { value: 1, label: 'MTBF' },
  { value: 2, label: 'Disponibilidade' },
  { value: 3, label: 'RPN' },
  { value: 4, label: 'Outro' }
]

// ECalculadoPorConfiabilidade
export const calculadoPorOpcoes: SelectOption[] = [
  { value: 0, label: 'Sistema' },
  { value: 1, label: 'Usuário' }
]

// EEstrategiaManutencao
export const estrategiaManutencaoOpcoes: SelectOption[] = [
  { value: 0, label: 'Preventiva' },
  { value: 1, label: 'Preditiva' },
  { value: 2, label: 'Corretiva controlada' },
  { value: 3, label: 'Operação até falha' },
  { value: 4, label: 'Revisar plano' },
  { value: 5, label: 'Manter plano' }
]

// ETipoPeriodicidade
export const tipoPeriodicidadeOpcoes: SelectOption[] = [
  { value: 0, label: 'Calendário' },
  { value: 1, label: 'Contador' },
  { value: 2, label: 'Combinado' }
]

// ETipoRegraMonitoramento
export const tipoRegraMonitoramentoOpcoes: SelectOption[] = [
  { value: 0, label: 'Limite' },
  { value: 1, label: 'Tendência' },
  { value: 2, label: 'Desvio' },
  { value: 3, label: 'SLA' },
  { value: 4, label: 'Outro' }
]

// Criticidade (string livre no backend, valores usuais)
export const criticidadeOpcoes: SelectOption[] = [
  { value: 'Alta', label: 'Alta' },
  { value: 'Media', label: 'Média' },
  { value: 'Baixa', label: 'Baixa' }
]

/**
 * Converte um valor de input (string dos campos `type="number"`) para número ou null.
 * Usado ao montar o body dos POSTs, pois o backend não faz coerção string→number.
 */
export function numeroOuNulo(v: unknown): number | null {
  if (v === null || v === undefined || v === '') return null
  const n = typeof v === 'number' ? v : Number(v)
  return isNaN(n) ? null : n
}

interface EquipamentoListItem {
  id: string
  nome?: string | null
  codigo?: string | null
}

/**
 * Carrega os equipamentos do próprio módulo para alimentar selects de `equipamentoId`.
 * Retorna [] em caso de erro (o campo continua utilizável como texto pela tela chamadora).
 */
export async function carregarEquipamentoOpcoes(): Promise<SelectOption[]> {
  try {
    const resposta = await useApi('/manutencao/equipamentos', { query: { pagina: 1, tamanhoPagina: 500 } })
    const dados = extrairDados<{ itens?: EquipamentoListItem[] } | EquipamentoListItem[]>(resposta)
    const itens = Array.isArray(dados) ? dados : dados?.itens ?? []
    return itens.map((e) => ({
      value: e.id,
      label: [e.codigo, e.nome].filter(Boolean).join(' - ') || e.id
    }))
  } catch (e) {
    console.error('[manutencao-shared] carregarEquipamentoOpcoes', e)
    return []
  }
}
