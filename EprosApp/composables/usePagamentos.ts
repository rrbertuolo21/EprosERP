/**
 * usePagamentos — pagamentos da venda (fatia Vendas/Fiscal).
 *
 * Porta `composables/vendas/usePagamentos.ts` do legado: adicionar/excluir pagamento da venda.
 * Usa exclusivamente `useApi` (padrão CommandResult do EprosERP).
 *
 * Contrato de rota (igual ao backend, VendasFiscalController):
 *   POST   api/v1/vendas-fiscal/{id}/pagamentos
 *   DELETE api/v1/vendas-fiscal/{id}/pagamentos/{pagamentoId}
 */
import { ref } from 'vue'
import { useApi } from './useApi'
import { obterMensagemErro } from './useApiList'

/** Porte fiel de ETipoPagamento (Epros.Shared.Domain.Enums). */
export type TipoPagamento =
  | 1 // Dinheiro
  | 2 // Cheque
  | 3 // CartaoCredito
  | 4 // CartaoDebito
  | 5 // CartaoDaLoja
  | 10 // ValeAlimentacao
  | 11 // ValeRefeicao
  | 12 // ValePresente
  | 13 // ValeCombustivel
  | 14 // DuplicataMercantil
  | 15 // BoletoBancario

export const TIPO_PAGAMENTO_OPTIONS: { label: string; value: TipoPagamento }[] = [
  { label: 'Dinheiro', value: 1 },
  { label: 'Cheque', value: 2 },
  { label: 'Cartão de Crédito', value: 3 },
  { label: 'Cartão de Débito', value: 4 },
  { label: 'Cartão da Loja', value: 5 },
  { label: 'Vale Alimentação', value: 10 },
  { label: 'Vale Refeição', value: 11 },
  { label: 'Vale Presente', value: 12 },
  { label: 'Vale Combustível', value: 13 },
  { label: 'Duplicata Mercantil', value: 14 },
  { label: 'Boleto Bancário', value: 15 }
]

export function labelTipoPagamento(valor?: number | null): string {
  if (valor == null) return '-'
  return TIPO_PAGAMENTO_OPTIONS.find((o) => o.value === valor)?.label ?? String(valor)
}

/** Porte fiel de ETipoIntegracaoPagamentoCArtao. */
export type TipoIntegracaoPagamentoCartao =
  | -1 // NaoUtiliza
  | 1 // PagamentoIntegradoComSistemaAutomacao
  | 2 // PagamentoNaoIntegradoComSistemaAutomacao

export const TIPO_INTEGRACAO_PAGAMENTO_CARTAO_OPTIONS: { label: string; value: TipoIntegracaoPagamentoCartao }[] = [
  { label: 'Não utiliza', value: -1 },
  { label: 'Integrado (TEF/e-commerce)', value: 1 },
  { label: 'Não integrado (POS)', value: 2 }
]

/** Porte fiel de EBandeiraCartao. */
export type BandeiraCartao = -1 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9

export const BANDEIRA_CARTAO_OPTIONS: { label: string; value: BandeiraCartao }[] = [
  { label: 'Não utiliza', value: -1 },
  { label: 'Visa', value: 1 },
  { label: 'Mastercard', value: 2 },
  { label: 'American Express', value: 3 },
  { label: 'Sorocred', value: 4 },
  { label: 'Diners Club', value: 5 },
  { label: 'Elo', value: 6 },
  { label: 'Hipercard', value: 7 },
  { label: 'Aura', value: 8 },
  { label: 'Cabal', value: 9 }
]

/** Corpo de inclusão de pagamento. Porte de Venda.AdicionarPagamento. */
export interface AdicionarPagamentoDto {
  valorTroco: number
  tipoPagamento: TipoPagamento
  valorPagamento: number
  cartaoTipoIntegracao: TipoIntegracaoPagamentoCartao
  cartaoCnpjIntermediadorFinanceira?: string | null
  cartaoBandeira: BandeiraCartao
  cartaoCodigoAutorizacaoOperacao?: string | null
}

export function usePagamentos() {
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  /** Adiciona um pagamento à venda. POST /vendas-fiscal/{vendaId}/pagamentos. */
  async function adicionar(vendaId: string, payload: AdicionarPagamentoDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post(`/vendas-fiscal/${vendaId}/pagamentos`, { vendaId, ...payload })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePagamentos.adicionar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  /** Exclui (soft delete) um pagamento da venda. DELETE /vendas-fiscal/{vendaId}/pagamentos/{pagamentoId}. */
  async function remover(vendaId: string, pagamentoId: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete(`/vendas-fiscal/${vendaId}/pagamentos/${pagamentoId}`)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePagamentos.remover]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    carregando,
    erro,
    adicionar,
    remover
  }
}
