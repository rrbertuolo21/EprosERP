/**
 * Espelha os enums do backend consumidos pelo formulário de NCM Tributação
 * (Epros.Modules.Fiscal.Domain.Enums / Epros.Shared.Domain.Enums). Não há endpoint de enum
 * dedicado para essas listas — se um `fiscais-enums/*` for exposto no backend, trocar por
 * `useEnum().carregarOpcoes(...)`.
 *
 * Arquivo auxiliar local desta fatia (Fiscal — CFOP + Tipo Operação + NCM); não é uma rota.
 */
import type { SelectOption } from '~/composables/useEnum'

export const opcoesOrigemMercadoria: SelectOption[] = [
  { label: '0 - Nacional exceto as indicadas nos códigos 3, 4, 5 e 8', value: 0 },
  { label: '1 - Estrangeira - Importação direta', value: 1 },
  { label: '2 - Estrangeira - Adquirida no mercado interno', value: 2 },
  { label: '3 - Nacional, conteúdo de importação superior a 40% e igual ou inferior a 70%', value: 3 },
  { label: '4 - Nacional, processos produtivos básicos', value: 4 },
  { label: '5 - Nacional, conteúdo de importação inferior ou igual a 40%', value: 5 },
  { label: '6 - Estrangeira - Importação direta, sem similar nacional, lista CAMEX', value: 6 },
  { label: '7 - Estrangeira - mercado interno, sem similar nacional, lista CAMEX', value: 7 },
  { label: '8 - Nacional, conteúdo de importação superior a 70%', value: 8 }
]

export const opcoesCsosn: SelectOption[] = [
  { label: '101 - Tributada pelo Simples Nacional com permissão de crédito', value: 101 },
  { label: '102 - Tributada pelo Simples Nacional sem permissão de crédito', value: 102 },
  { label: '103 - Isenção do ICMS no Simples Nacional para faixa de receita bruta', value: 103 },
  { label: '201 - Tributada com permissão de crédito e com ICMS por substituição tributária', value: 201 },
  { label: '202 - Tributada sem permissão de crédito e com ICMS por substituição tributária', value: 202 },
  { label: '203 - Isenção do ICMS para faixa de receita bruta e com ICMS por substituição tributária', value: 203 },
  { label: '300 - Imune', value: 300 },
  { label: '400 - Não tributada pelo Simples Nacional', value: 400 },
  { label: '500 - ICMS cobrado anteriormente por substituição tributária ou por antecipação', value: 500 },
  { label: '900 - Outros', value: 900 }
]

export const opcoesCstIcms: SelectOption[] = [
  { label: '00 - Tributada integralmente', value: 0 },
  { label: '02 - Tributação Monofásica própria sobre Combustíveis', value: 2 },
  { label: '10 - Tributada e com cobrança do ICMS por substituição tributária', value: 10 },
  { label: '12 - ICMS devido por ST relativo às operações antecedentes', value: 12 },
  { label: '13 - ICMS devido por ST relativo às operações concomitantes', value: 13 },
  { label: '15 - Tributação Monofásica própria com retenção sobre Combustíveis', value: 15 },
  { label: '20 - Com redução de base de cálculo', value: 20 },
  { label: '30 - Isenta ou não tributada e com cobrança do ICMS por ST', value: 30 },
  { label: '40 - Isenta', value: 40 },
  { label: '41 - Não tributada', value: 41 },
  { label: '50 - Suspensão', value: 50 },
  { label: '51 - Diferimento', value: 51 },
  { label: '52 - Diferimento com ICMS devido por ST relativo às operações subsequentes', value: 52 },
  { label: '53 - Tributação Monofásica sobre Combustíveis com recolhimento diferido', value: 53 },
  { label: '60 - ICMS cobrado anteriormente por substituição tributária', value: 60 },
  { label: '61 - Tributação Monofásica sobre Combustíveis cobrada anteriormente', value: 61 },
  { label: '70 - Com redução de base de cálculo e cobrança do ICMS por ST', value: 70 },
  { label: '72 - Redução de base de cálculo e ICMS devido por ST (antecedentes)', value: 72 },
  { label: '74 - Redução de base de cálculo e ICMS devido por ST (concomitantes)', value: 74 },
  { label: '90 - Outras', value: 90 }
]

export const opcoesCstPisCofins: SelectOption[] = [
  { label: '01 - Operação Tributável com Alíquota Básica', value: 1 },
  { label: '02 - Operação Tributável com Alíquota Diferenciada', value: 2 },
  { label: '03 - Operação Tributável com Alíquota por Unidade de Medida', value: 3 },
  { label: '04 - Operação Tributável Monofásica – Revenda a Alíquota Zero', value: 4 },
  { label: '05 - Operação Tributável por Substituição Tributária', value: 5 },
  { label: '06 - Operação Tributável a Alíquota Zero', value: 6 },
  { label: '07 - Operação Isenta da Contribuição', value: 7 },
  { label: '08 - Operação sem Incidência da Contribuição', value: 8 },
  { label: '09 - Operação com Suspensão da Contribuição', value: 9 },
  { label: '49 - Outras Operações de Saída', value: 49 },
  { label: '50 - Direito a Crédito – Receita Tributada no Mercado Interno', value: 50 },
  { label: '70 - Aquisição sem Direito a Crédito', value: 70 },
  { label: '98 - Outras Operações de Entrada', value: 98 },
  { label: '99 - Outras Operações', value: 99 }
]

export const opcoesCstIpi: SelectOption[] = [
  { label: '00 - Entrada com Recuperação de Crédito', value: 0 },
  { label: '01 - Entrada Tributada com Alíquota Zero', value: 1 },
  { label: '02 - Entrada Isenta', value: 2 },
  { label: '03 - Entrada Não Tributada', value: 3 },
  { label: '04 - Entrada Imune', value: 4 },
  { label: '05 - Entrada com Suspensão', value: 5 },
  { label: '49 - Outras Entradas', value: 49 },
  { label: '50 - Saída Tributada', value: 50 },
  { label: '51 - Saída Tributável com Alíquota Zero', value: 51 },
  { label: '52 - Saída Isenta', value: 52 },
  { label: '53 - Saída Não-Tributada', value: 53 },
  { label: '54 - Saída Imune', value: 54 },
  { label: '55 - Saída com Suspensão', value: 55 },
  { label: '99 - Outras Saídas', value: 99 }
]

export const opcoesTipoReducao: SelectOption[] = [
  { label: 'Não Utiliza', value: 0 },
  { label: 'Em (- subtração da base de cálculo)', value: 1 },
  { label: 'Para (× multiplicação da base de cálculo)', value: 2 }
]

export const opcoesDestinoReducao: SelectOption[] = [
  { label: 'Não Utiliza', value: -1 },
  { label: 'Isento', value: 1 },
  { label: 'Outras', value: 2 }
]

export const opcoesCodigoValorFiscalIcms: SelectOption[] = [
  { label: 'Não Utiliza', value: -1 },
  { label: 'Tributado', value: 0 },
  { label: 'Isento', value: 1 },
  { label: 'Outros', value: 2 }
]

export const opcoesTipoCalculoSt: SelectOption[] = [
  { label: 'Margem Agregada', value: 0 },
  { label: 'Valor Fixo', value: 1 }
]

export const ufsBrasil: SelectOption[] = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
].map((uf) => ({ label: uf, value: uf }))

export const TIPO_REDUCAO_NAO_UTILIZA = 0
export const CODIGO_VALOR_FISCAL_TRIBUTADO = 0
