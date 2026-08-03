import { useMask } from './useMask'

/**
 * Composable de validação de documentos BR (CPF, CNPJ, CEP, e-mail, telefone).
 *
 * Implementa os algoritmos de dígito verificador sem dependências externas.
 * Consumido por telas de parceiro/empresa para validar entradas.
 *
 * CNPJ alfanumérico (Nota Técnica RFB 2026): a validação usa `somenteAlfanumerico`
 * (preserva letras) e o valor de cada caractere é `código ASCII - 48` (dígitos '0'-'9'
 * seguem valendo 0-9; letras 'A'-'Z' valem 17-42). Espelha fielmente
 * `src/External/Epros.ERP.Shared/Validations/Documentos/CNPJValidacao.cs:Validar`.
 */
export function useDocumento() {
  const { somenteDigitos, somenteAlfanumerico } = useMask()

  /** Valida CPF pelo algoritmo dos dígitos verificadores. */
  function validarCPF(valor: string): boolean {
    const cpf = somenteDigitos(valor)
    if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) return false

    const calc = (fatorInicial: number): number => {
      let soma = 0
      for (let i = 0; i < fatorInicial - 1; i++) {
        soma += parseInt(cpf[i]) * (fatorInicial - i)
      }
      const resto = (soma * 10) % 11
      return resto === 10 ? 0 : resto
    }
    return calc(10) === parseInt(cpf[9]) && calc(11) === parseInt(cpf[10])
  }

  /**
   * Valida CNPJ (numérico ou alfanumérico) pelo algoritmo dos dígitos verificadores.
   * Cada caractere entra no somatório como `charCodeAt(0) - 48` (dígitos e letras).
   */
  function validarCNPJ(valor: string): boolean {
    const cnpj = somenteAlfanumerico(valor)
    if (cnpj.length !== 14) return false
    if (/^(.)\1{13}$/.test(cnpj)) return false

    const multipDV1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]
    const multipDV2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]

    const calcDV1 = cnpj.substring(0, 12)
    const calcDV2 = cnpj.substring(0, 13)

    const valorChar = (c: string): number => c.charCodeAt(0) - 48

    let soma = 0
    for (let i = 0; i < multipDV1.length; i++) {
      soma += valorChar(calcDV1[i]) * multipDV1[i]
    }
    let resto = soma % 11
    const digito1 = resto <= 1 ? 0 : 11 - resto

    soma = 0
    for (let i = 0; i < multipDV2.length; i++) {
      soma += valorChar(calcDV2[i]) * multipDV2[i]
    }
    resto = soma % 11
    const digito2 = resto <= 1 ? 0 : 11 - resto

    return cnpj === `${calcDV1}${digito1}${digito2}`
  }

  /** Valida CPF ou CNPJ (alfanumérico) conforme o comprimento. */
  function validarCpfCnpj(valor: string): boolean {
    const d = somenteAlfanumerico(valor)
    return d.length <= 11 ? validarCPF(valor) : validarCNPJ(valor)
  }

  /** Valida CEP (8 dígitos). */
  function validarCEP(valor: string): boolean {
    return somenteDigitos(valor).length === 8
  }

  /** Valida e-mail (regex simples). */
  function validarEmail(valor: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test((valor ?? '').trim())
  }

  /**
   * Valida telefone brasileiro (fixo ou celular), incluindo operador e 9º dígito.
   * Celular (11 dígitos): DDD 1-9 + 9 + 8 dígitos (`^[1-9]{2}9\d{8}$`).
   * Fixo (10 dígitos): DDD 1-9 + 1º dígito do número entre 2-5 (`^[1-9]{2}[2-5]\d{7}$`).
   * Espelha `utils/br-validators.ts:isPhoneBR` do legado.
   */
  function validarTelefone(valor: string): boolean {
    const d = somenteDigitos(valor)
    if (d.length === 11) return /^[1-9]{2}9\d{8}$/.test(d)
    if (d.length === 10) return /^[1-9]{2}[2-5]\d{7}$/.test(d)
    return false
  }

  return {
    validarCPF,
    validarCNPJ,
    validarCpfCnpj,
    validarCEP,
    validarEmail,
    validarTelefone
  }
}
