/**
 * Composable de máscaras de entrada BR (utilitário próprio — sem libs externas).
 *
 * Fornece funções de formatação/desformatação para CPF, CNPJ, CEP, telefone, placa e moeda.
 * Consumido pelos campos compartilhados (`fields/*`) e por telas de parceiro/empresa.
 */
export function useMask() {
  /** Remove tudo que não for dígito. */
  const somenteDigitos = (valor: string | null | undefined): string =>
    (valor ?? '').replace(/\D/g, '')

  /** Remove tudo que não for dígito ou letra (CNPJ alfanumérico). */
  const somenteAlfanumerico = (valor: string | null | undefined): string =>
    (valor ?? '').replace(/[^A-Za-z0-9]/g, '').toUpperCase()

  /** 000.000.000-00 */
  const maskCPF = (valor: string): string =>
    somenteDigitos(valor)
      .slice(0, 11)
      .replace(/(\d{3})(\d)/, '$1.$2')
      .replace(/(\d{3})(\d)/, '$1.$2')
      .replace(/(\d{3})(\d{1,2})$/, '$1-$2')

  /** 00.000.000/0000-00 (suporta CNPJ alfanumérico da nova especificação). */
  const maskCNPJ = (valor: string): string =>
    somenteAlfanumerico(valor)
      .slice(0, 14)
      .replace(/^([A-Z0-9]{2})([A-Z0-9])/, '$1.$2')
      .replace(/^([A-Z0-9]{2})\.([A-Z0-9]{3})([A-Z0-9])/, '$1.$2.$3')
      .replace(/\.([A-Z0-9]{3})([A-Z0-9])/, '.$1/$2')
      .replace(/([A-Z0-9]{4})([A-Z0-9]{1,2})$/, '$1-$2')

  /** Aplica CPF ou CNPJ automaticamente conforme o comprimento. */
  const maskCpfCnpj = (valor: string): string => {
    const limpo = somenteAlfanumerico(valor)
    return limpo.length <= 11 ? maskCPF(valor) : maskCNPJ(valor)
  }

  /** 00000-000 */
  const maskCEP = (valor: string): string =>
    somenteDigitos(valor)
      .slice(0, 8)
      .replace(/(\d{5})(\d)/, '$1-$2')

  /** (00) 0000-0000 ou (00) 00000-0000 conforme o número de dígitos. */
  const maskTelefone = (valor: string): string => {
    const d = somenteDigitos(valor).slice(0, 11)
    if (d.length <= 10) {
      return d
        .replace(/(\d{2})(\d)/, '($1) $2')
        .replace(/(\d{4})(\d)/, '$1-$2')
    }
    return d
      .replace(/(\d{2})(\d)/, '($1) $2')
      .replace(/(\d{5})(\d)/, '$1-$2')
  }

  /** Placa Mercosul AAA-0A00. */
  const maskPlaca = (valor: string): string =>
    somenteAlfanumerico(valor)
      .slice(0, 7)
      .replace(/^([A-Z]{3})([A-Z0-9])/, '$1-$2')

  /** Formata número como moeda BRL (R$ 1.234,56). */
  const maskMoeda = (valor: number | string, comSimbolo = true): string => {
    const num = typeof valor === 'string' ? Number(valor.replace(/\D/g, '')) / 100 : valor
    if (isNaN(num)) return comSimbolo ? 'R$ 0,00' : '0,00'
    return new Intl.NumberFormat('pt-BR', {
      style: comSimbolo ? 'currency' : 'decimal',
      currency: 'BRL',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(num)
  }

  /** Converte uma string monetária (R$ 1.234,56) para número (1234.56). */
  const desmascararMoeda = (valor: string): number => {
    const limpo = (valor ?? '')
      .replace(/[^\d,-]/g, '')
      .replace(/\./g, '')
      .replace(',', '.')
    const num = Number(limpo)
    return isNaN(num) ? 0 : num
  }

  return {
    somenteDigitos,
    somenteAlfanumerico,
    maskCPF,
    maskCNPJ,
    maskCpfCnpj,
    maskCEP,
    maskTelefone,
    maskPlaca,
    maskMoeda,
    desmascararMoeda
  }
}
