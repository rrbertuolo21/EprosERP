/**
 * Opções de enum e helper de carregamento de selects para os módulos
 * FINANCEIROS AVANÇADOS (tesouraria, serviços financeiros, câmbio/risco, contratos,
 * planejamento/orçamento, subsídios/fundos, cartões).
 *
 * Valores extraídos dos enums do backend (Epros.Modules.Financeiro.Domain.Enums e
 * Epros.Shared.Domain.Enums). Mantidos como constantes locais deste módulo — as telas
 * enviam o valor numérico (id do enum) no corpo do POST/PUT, que é como a API serializa.
 */
import { useApi, extrairDados } from '~/composables/useApi'
import type { SelectOption } from '~/composables/useEnum'

/** EPeriodicidadeContrato (ContratosFinanceirosEnums). */
export const OPCOES_PERIODICIDADE: SelectOption[] = [
  { label: 'Mensal', value: 0 },
  { label: 'Bimestral', value: 1 },
  { label: 'Trimestral', value: 2 },
  { label: 'Semestral', value: 3 },
  { label: 'Anual', value: 4 }
]

/** ETipoContaBancaria (Shared). */
export const OPCOES_TIPO_CONTA_BANCARIA: SelectOption[] = [
  { label: 'Conta Corrente', value: 1 },
  { label: 'Conta Poupança', value: 2 },
  { label: 'Aplicações', value: 3 },
  { label: 'Outras', value: 4 }
]

/** EBandeiraCartao (Shared). */
export const OPCOES_BANDEIRA_CARTAO: SelectOption[] = [
  { label: 'Não utiliza', value: -1 },
  { label: 'Visa', value: 1 },
  { label: 'Mastercard', value: 2 },
  { label: 'American Express', value: 3 },
  { label: 'Sorocred', value: 4 },
  { label: 'Diners Club', value: 5 },
  { label: 'Elo', value: 6 },
  { label: 'Hipercard', value: 7 },
  { label: 'Aura', value: 8 },
  { label: 'Cabal', value: 9 },
  { label: 'Alelo', value: 10 },
  { label: 'Banes Card', value: 11 },
  { label: 'CalCard', value: 12 },
  { label: 'Credz', value: 13 },
  { label: 'Discover', value: 14 },
  { label: 'GoodCard', value: 15 },
  { label: 'GreenCard', value: 16 },
  { label: 'Hiper', value: 17 },
  { label: 'JcB', value: 18 },
  { label: 'Mais', value: 19 },
  { label: 'MaxVan', value: 20 },
  { label: 'Policard', value: 21 },
  { label: 'RedeCompras', value: 22 },
  { label: 'Sodexo', value: 23 },
  { label: 'ValeCard', value: 24 },
  { label: 'Verocheque', value: 25 },
  { label: 'VR', value: 26 },
  { label: 'Ticket', value: 27 }
]

/** ETipoFaturaCobranca (ServicosFinanceirosEnums). */
export const OPCOES_TIPO_FATURA: SelectOption[] = [
  { label: 'Avulsa', value: 0 },
  { label: 'Periódica', value: 1 },
  { label: 'Carnê', value: 2 }
]

/** ELayoutCnab (ServicosFinanceirosEnums). */
export const OPCOES_LAYOUT_CNAB: SelectOption[] = [
  { label: 'CNAB 240', value: 240 },
  { label: 'CNAB 400', value: 400 }
]

/** EOrigemTaxaCambio (CambioRiscoEnums). */
export const OPCOES_ORIGEM_TAXA: SelectOption[] = [
  { label: 'Manual', value: 0 },
  { label: 'PTAX', value: 1 }
]

/** ESituacaoCheque (TesourariaEnums). */
export const OPCOES_SITUACAO_CHEQUE: SelectOption[] = [
  { label: 'Emitido', value: 0 },
  { label: 'Compensado', value: 1 },
  { label: 'Devolvido', value: 2 },
  { label: 'Cancelado', value: 3 },
  { label: 'Repassado', value: 4 }
]

/** ETipoTransacaoConta (TesourariaEnums). */
export const OPCOES_TIPO_TRANSACAO: SelectOption[] = [
  { label: 'Crédito', value: 0 },
  { label: 'Débito', value: 1 }
]

/** EEscopoMeta (PlanejamentoOrcamentoEnums). */
export const OPCOES_ESCOPO_META: SelectOption[] = [
  { label: 'Qualquer', value: 0 },
  { label: 'Próprio', value: 1 }
]

/**
 * Tipo do cheque (Cheque.Tipo — int "emitido/recebido"). O backend guarda apenas int;
 * não há enum nomeado. Palpite de mapeamento (0=Emitido, 1=Recebido).
 */
export const OPCOES_TIPO_CHEQUE: SelectOption[] = [
  { label: 'Emitido', value: 0 },
  { label: 'Recebido', value: 1 }
]

/** Tipo de pessoa do cheque (Cheque.TipoPessoa — int). Palpite (1=Física, 2=Jurídica). */
export const OPCOES_TIPO_PESSOA_CHEQUE: SelectOption[] = [
  { label: 'Pessoa Física', value: 1 },
  { label: 'Pessoa Jurídica', value: 2 }
]

/**
 * Carrega opções de select a partir de um endpoint de listagem da API.
 *
 * Aceita as respostas envelopadas do EprosERP (`dados` como array direto ou como
 * `{ itens: [...] }`). Mapeia o campo de rótulo escolhendo o primeiro disponível
 * entre `labelKeys`, com fallback para o id.
 *
 * Falha de forma silenciosa (retorna []) — selects de apoio não devem quebrar a tela.
 */
export async function carregarOpcoesDe(
  path: string,
  labelKeys: string[] = ['nome', 'razaoSocial', 'descricao', 'apelido', 'titular'],
  valueKey = 'id',
  query?: Record<string, unknown>
): Promise<SelectOption[]> {
  try {
    const resposta = await useApi(path, { query: query ?? { pagina: 1, tamanhoPagina: 200 } })
    const bruto = extrairDados<unknown>(resposta)
    let itens: Record<string, unknown>[] = []
    if (Array.isArray(bruto)) {
      itens = bruto as Record<string, unknown>[]
    } else if (bruto && typeof bruto === 'object') {
      const o = bruto as { itens?: unknown[]; Itens?: unknown[] }
      itens = (o.itens ?? o.Itens ?? []) as Record<string, unknown>[]
    }
    return itens.map((it) => {
      const chave = labelKeys.find((k) => it[k] != null && it[k] !== '')
      const label = chave ? String(it[chave]) : String(it[valueKey] ?? '')
      return { label, value: it[valueKey] as string | number }
    })
  } catch (e) {
    console.error(`[carregarOpcoesDe:${path}]`, e)
    return []
  }
}
