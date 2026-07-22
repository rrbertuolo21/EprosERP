/**
 * useVendaAcoes — ações de venda (Onda 7): duplicar venda, cupom não fiscal (MEI), DANFE de
 * pré-visualização e NF-e simplificada (criar/atualizar/transmitir).
 *
 * Porta o comportamento de `VendaController`/`VendaNfeController` do legado (ações soltas, fora
 * do CRUD de venda) para o padrão EprosERP (`useApi`, envelope CommandResult).
 *
 * Contrato de rota:
 *   GET  api/v1/vendas/baixar-cupom-nao-fiscal?vendaId=            (PDF)
 *   GET  api/v1/vendas/obter-informacoes-complementares-por-produtos-ids?empresaId=&produtosIds=
 *   POST api/v1/vendas/nfe-simplificado
 *   PUT  api/v1/vendas/nfe-simplificado/{id}
 *   POST api/v1/vendas/nfe-simplificado-transmitir
 *   PUT  api/v1/vendas-fiscal/{id}/duplicar
 *   POST api/v1/vendas-fiscal/{id}/gerar-danfe-sem-autorizacao                (PDF)
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { obterMensagemErro } from './useApiList'

/** Corpo de criação da venda de NF-e simplificada. Porte de CriarVendaSimplificadaNfeCommand. */
export interface CriarVendaSimplificadaNfeDto {
  caixaId: string
  naturezaOperacao: string
  dataVenda: string
  informacoesComplementares?: string | null
  informacoesAdicionaisFisco?: string | null
  status: string
  modalidadeFrete: number
  incluirFreteNoTotal: boolean
  clienteId?: string | null
  total: number
  valorDesconto: number
  valorFrete: number
  formaPagamento?: string | null
}

/** Corpo de atualização da venda de NF-e simplificada. Porte de AtualizarVendaSimplificadaNfeCommand. */
export interface AtualizarVendaSimplificadaNfeDto {
  id: string
  naturezaOperacao: string
  dataVenda: string
  informacoesComplementares?: string | null
  informacoesAdicionaisFisco?: string | null
  modalidadeFrete: number
  valorDesconto: number
  valorFrete: number
}

/** Corpo de transmissão da venda de NF-e simplificada. Porte de TransmitirVendaSimplificadaNfeCommand. */
export interface TransmitirVendaSimplificadaNfeDto {
  vendaId: string
  serie: number
  numero: number
  dataHoraSaida?: string | null
}

/** Baixa um blob (PDF) e devolve como URL de objeto, pronta para abrir/imprimir. */
function criarUrlObjeto(blob: Blob): string {
  return URL.createObjectURL(blob)
}

export function useVendaAcoes() {
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  /** Baixa o PDF do cupom não fiscal (MEI) da venda. Retorna a URL do objeto Blob (revogar após uso). */
  async function baixarCupomNaoFiscal(vendaId: string): Promise<string | null> {
    carregando.value = true
    erro.value = null
    try {
      const blob = await useApi<Blob>('/vendas/baixar-cupom-nao-fiscal', {
        query: { vendaId },
        responseType: 'blob'
      })
      return criarUrlObjeto(blob)
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useVendaAcoes.baixarCupomNaoFiscal]', e)
      return null
    } finally {
      carregando.value = false
    }
  }

  /** Informações complementares consolidadas dos produtos informados (por NCM/empresa). */
  async function obterInformacoesComplementaresPorProdutos(empresaId: string, produtosIds: string[]): Promise<unknown> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult>('/vendas/obter-informacoes-complementares-por-produtos-ids', {
        query: { empresaId, produtosIds }
      })
      return extrairDados(resposta)
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useVendaAcoes.obterInformacoesComplementaresPorProdutos]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  /** Cria o cabeçalho de uma venda de NF-e simplificada. Retorna o Id da venda criada. */
  async function criarNfeSimplificado(payload: CriarVendaSimplificadaNfeDto): Promise<string | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi.post<CommandResult<{ id: string } | string>>('/vendas/nfe-simplificado', payload)
      const dados = extrairDados<{ id: string } | string>(resposta)
      if (typeof dados === 'string') return dados
      return dados?.id ?? null
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useVendaAcoes.criarNfeSimplificado]', e)
      return null
    } finally {
      carregando.value = false
    }
  }

  /** Atualiza o cabeçalho de uma venda de NF-e simplificada existente. */
  async function atualizarNfeSimplificado(payload: AtualizarVendaSimplificadaNfeDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put(`/vendas/nfe-simplificado/${payload.id}`, payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useVendaAcoes.atualizarNfeSimplificado]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  /** Marca a NF-e simplificada para transmissão (série/número e status Transmitido). */
  async function transmitirNfeSimplificado(payload: TransmitirVendaSimplificadaNfeDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/vendas/nfe-simplificado-transmitir', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useVendaAcoes.transmitirNfeSimplificado]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  /** Duplica a venda como novo rascunho (sem NFC-e/NF-e). Retorna o Id da nova venda. */
  async function duplicar(vendaId: string): Promise<string | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi.put<CommandResult<{ id: string } | string>>(`/vendas-fiscal/${vendaId}/duplicar`)
      const dados = extrairDados<{ id: string } | string>(resposta)
      if (typeof dados === 'string') return dados
      return dados?.id ?? null
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useVendaAcoes.duplicar]', e)
      return null
    } finally {
      carregando.value = false
    }
  }

  /** Gera o PDF de pré-visualização do DANFE (sem valor fiscal) da venda. Retorna a URL do objeto Blob. */
  async function gerarDanfeSemAutorizacao(vendaId: string): Promise<string | null> {
    carregando.value = true
    erro.value = null
    try {
      const blob = await useApi<Blob>(`/vendas-fiscal/${vendaId}/gerar-danfe-sem-autorizacao`, {
        method: 'POST',
        responseType: 'blob'
      })
      return criarUrlObjeto(blob)
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useVendaAcoes.gerarDanfeSemAutorizacao]', e)
      return null
    } finally {
      carregando.value = false
    }
  }

  return {
    carregando,
    erro,
    baixarCupomNaoFiscal,
    obterInformacoesComplementaresPorProdutos,
    criarNfeSimplificado,
    atualizarNfeSimplificado,
    transmitirNfeSimplificado,
    duplicar,
    gerarDanfeSemAutorizacao
  }
}
