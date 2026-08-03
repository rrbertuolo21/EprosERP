/**
 * Tipos compartilhados da fatia 11 (Devolução/Retorno + Transmissões + Inutilização).
 *
 * Porta o modelo das telas legadas `vendas/transmissoes.vue` e `vendas/inutilizacao-numeracao.vue`
 * para o design novo. As respostas seguem o padrão CommandResult { sucesso, mensagem, dados }.
 */

/** Modelos fiscais suportados (código SEFAZ). */
export const MODELOS_FISCAIS: { label: string; value: string }[] = [
  { label: 'NF-e (55)', value: '55' },
  { label: 'NFC-e (65)', value: '65' }
]

/** Nome curto do modelo fiscal por código. */
export const NOME_MODELO: Record<string, string> = {
  '55': 'NF-e',
  '65': 'NFC-e'
}

/**
 * Status interno/SEFAZ do documento fiscal (porta o enum `Status` do legado).
 * Recebido = 0, Autorizado = 1, Rejeitado = 2, Cancelado = 3.
 */
export enum StatusDfe {
  Recebido = 0,
  Autorizado = 1,
  Rejeitado = 2,
  Cancelado = 3
}

/** Sub-documento NF-e/NFC-e dentro de um item de transmissão. */
export interface DocumentoFiscalResumo {
  /**
   * Id (Guid) do DocumentoFiscal no módulo Fiscal, quando disponível na listagem.
   * É a chave usada pelas rotas `fiscal/documentos/{id}/...`. Quando ausente, resolvemos
   * o id a partir da `chave` de acesso via `GET fiscal/documentos/chave/{chave}`.
   */
  documentoFiscalId?: string | null
  chave?: string | null
  numero?: string | null
  serie?: string | null
  statusSefaz?: number | null
  statusInterno?: number | null
  motivoRejeicaoSefaz?: string | null
  dataHoraEmissao?: string | null
  cartaCorrecao?: boolean
}

/** Linha da listagem de transmissões (monitor SEFAZ). */
export interface TransmissaoItem {
  id: string | number
  modeloFiscal: string
  destinatario?: { razaoSocial?: string | null } | null
  emitente?: { nomeFantasia?: string | null } | null
  nfe?: DocumentoFiscalResumo | null
  nfce?: DocumentoFiscalResumo | null
  total?: { valorNotaFiscal?: number | null } | null
  situacao?: string | null
  status?: number | null
}

/** Filtros do monitor de transmissões (espelham a query da tela legada). */
export interface TransmissoesFiltros {
  dataVendaInicial: string | null
  dataVendaFinal: string | null
  status: string | null
  chave: string | null
  nrNf: string | null
  cfop: string | null
  modeloFiscal: string | null
  [key: string]: unknown
}

/** Registro de numeração inutilizada (retorno de `fiscal/documentos` / inutilização). */
export interface InutilizacaoItem {
  id?: string | number
  ano?: string | number | null
  serie?: string | number | null
  nrNfInicial?: string | number | null
  nrNfFinal?: string | number | null
  justificativa?: string | null
  protocolo?: string | null
  dataHora?: string | null
  modeloDocumento?: string | null
}

/** Corpo enviado ao inutilizar numeração. */
export interface InutilizacaoBody {
  modeloDocumento: string
  serie: string | null
  nrNfInicial: string | null
  nrNfFinal: string | null
  justificativa: string
  [key: string]: unknown
}

/** Devolve o sub-documento vigente (NF-e tem prioridade sobre NFC-e). */
export function documentoVigente(item: TransmissaoItem): DocumentoFiscalResumo | null {
  return item.nfe ?? item.nfce ?? null
}

/** Retorna o rótulo textual do status interno de um item. */
export function rotuloStatus(item: TransmissaoItem): string {
  const doc = documentoVigente(item)
  const status = doc?.statusInterno ?? item.status ?? null
  switch (status) {
    case StatusDfe.Recebido:
      return 'Recebido'
    case StatusDfe.Autorizado:
      return 'Autorizado'
    case StatusDfe.Rejeitado:
      return 'Rejeitado'
    case StatusDfe.Cancelado:
      return 'Cancelado'
    default:
      return item.situacao ?? '—'
  }
}

/** Classe de chip (design novo) conforme o status do item. */
export function classeChipStatus(item: TransmissaoItem): string {
  const doc = documentoVigente(item)
  const status = doc?.statusInterno ?? item.status ?? null
  switch (status) {
    case StatusDfe.Autorizado:
      return 'chip chip-success'
    case StatusDfe.Cancelado:
      return 'chip chip-danger'
    case StatusDfe.Rejeitado:
      return 'chip chip-warning'
    default:
      return 'chip'
  }
}
