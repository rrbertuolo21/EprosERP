/**
 * Composable de helpers de formatação (datas, moeda, números).
 *
 * Usa APIs nativas (`Intl`) para não acoplar a uma lib de datas específica no scaffolding.
 * Se o projeto adotar `date-fns` depois, estas funções podem ser reimplementadas mantendo a assinatura.
 * Consumido de forma geral pelas telas (nunca `moment`).
 */
export function useHelper() {
  /** Converte entrada para Date de forma segura, evitando problemas de timezone em datas YYYY-MM-DD. */
  function paraData(valor: string | Date | number | null | undefined): Date | null {
    if (valor == null || valor === '') return null
    if (valor instanceof Date) return isNaN(valor.getTime()) ? null : valor
    if (typeof valor === 'number') {
      const d = new Date(valor)
      return isNaN(d.getTime()) ? null : d
    }
    // String: para 'YYYY-MM-DD' faz parse local para não sofrer deslocamento de fuso.
    const soData = /^\d{4}-\d{2}-\d{2}$/
    if (soData.test(valor)) {
      const [ano, mes, dia] = valor.split('-').map(Number)
      return new Date(ano, mes - 1, dia)
    }
    const d = new Date(valor)
    return isNaN(d.getTime()) ? null : d
  }

  /** Formata data como DD/MM/YYYY. */
  function formatarData(valor: string | Date | number | null | undefined): string {
    const d = paraData(typeof valor === 'string' && valor.includes('T') ? valor.split('T')[0] : valor)
    if (!d) return ''
    return new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' }).format(d)
  }

  /** Formata data e hora como DD/MM/YYYY HH:mm. */
  function formatarDataHora(valor: string | Date | number | null | undefined): string {
    const d = paraData(valor)
    if (!d) return ''
    return new Intl.DateTimeFormat('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(d)
  }

  /** Converte Date para string ISO (YYYY-MM-DD), útil para enviar à API. */
  function paraIsoData(valor: Date | null | undefined): string | null {
    if (!valor || isNaN(valor.getTime())) return null
    const ano = valor.getFullYear()
    const mes = String(valor.getMonth() + 1).padStart(2, '0')
    const dia = String(valor.getDate()).padStart(2, '0')
    return `${ano}-${mes}-${dia}`
  }

  /** Formata número como moeda BRL. */
  function formatarMoeda(valor: number | null | undefined, comSimbolo = true): string {
    const num = valor ?? 0
    return new Intl.NumberFormat('pt-BR', {
      style: comSimbolo ? 'currency' : 'decimal',
      currency: 'BRL',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(num)
  }

  /** Formata número com casas decimais configuráveis (pt-BR). */
  function formatarNumero(valor: number | null | undefined, minDecimais = 0, maxDecimais = 3): string {
    const num = valor ?? 0
    return new Intl.NumberFormat('pt-BR', {
      minimumFractionDigits: minDecimais,
      maximumFractionDigits: maxDecimais
    }).format(num)
  }

  /** Formata número como porcentagem (12,5%). Recebe o valor já em pontos percentuais. */
  function formatarPorcentagem(valor: number | null | undefined, decimais = 2): string {
    const num = valor ?? 0
    return `${new Intl.NumberFormat('pt-BR', {
      minimumFractionDigits: 0,
      maximumFractionDigits: decimais
    }).format(num)}%`
  }

  /** Diferença em dias (inteiros) entre duas datas. */
  function diferencaEmDias(de: Date, ate: Date): number {
    const umDia = 1000 * 60 * 60 * 24
    const a = new Date(de.getFullYear(), de.getMonth(), de.getDate()).getTime()
    const b = new Date(ate.getFullYear(), ate.getMonth(), ate.getDate()).getTime()
    return Math.round((b - a) / umDia)
  }

  return {
    paraData,
    formatarData,
    formatarDataHora,
    paraIsoData,
    formatarMoeda,
    formatarNumero,
    formatarPorcentagem,
    diferencaEmDias
  }
}
