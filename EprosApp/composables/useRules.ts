import { useDocumento } from './useDocumento'

/**
 * useRules — módulo central de regras de validação reutilizáveis (gap L5).
 *
 * Porta `utils/rules.ts` do legado (estilo Vuetify: `(valor) => true | mensagem`).
 * O novo front não usa `vee-validate`/Vuetify — os campos (`fields/*`) exibem erro via
 * prop `error?: string`, então cada regra aqui retorna `string | true`:
 *   - `true`   → válido.
 *   - `string` → mensagem de erro a exibir.
 *
 * Objetivo: parar de duplicar validação ad-hoc por tela (CPF/CNPJ/e-mail/telefone/CEP
 * já viviam em `useDocumento`; este composable adiciona as regras "de formulário"
 * (required, min/max, numéricas, estoque) que antes só existiam no legado).
 *
 * Uso típico num validador de tela:
 *   const erro = rules.required(pessoa.nome)
 *   if (erro !== true) return erro
 */
export function useRules() {
  const { validarCPF, validarCNPJ, validarCpfCnpj, validarEmail, validarTelefone, validarCEP } = useDocumento()

  const required = (valor: unknown): true | string => {
    if (valor === null || valor === undefined) return 'Campo obrigatório'
    if (typeof valor === 'string' && valor.trim() === '') return 'Campo obrigatório'
    return true
  }

  /**
   * Obrigatório condicional. Considera preenchido: números (inclui 0), boolean true/false,
   * strings não vazias. Evita tratar `0` como vazio (ex.: enums com valor 0).
   */
  const requiredIf = (ehObrigatorio: boolean) => (valor: unknown): true | string => {
    if (!ehObrigatorio) return true
    if (valor === null || valor === undefined || valor === '') return 'Campo obrigatório'
    return true
  }

  const email = (valor: string): true | string => {
    if (!valor) return true
    return validarEmail(valor) || 'Email inválido'
  }

  const phone = (valor: string): true | string => {
    if (!valor) return true
    return validarTelefone(valor) || 'Telefone inválido'
  }

  const cep = (valor: string): true | string => {
    if (!valor) return true
    return validarCEP(valor) || 'CEP inválido'
  }

  const cpf = (valor: string): true | string => {
    if (!valor) return true
    return validarCPF(valor) || 'CPF inválido'
  }

  /** Aceita CNPJ numérico (14 dígitos) ou alfanumérico (Nota Técnica RFB 2026). */
  const cnpj = (valor: string): true | string => {
    if (!valor) return true
    return validarCNPJ(valor) || 'CNPJ inválido'
  }

  /** Detecta CPF (<=11) ou CNPJ pelo tamanho e aplica a validação correspondente. */
  const cpfCnpj = (valor: string): true | string => {
    if (!valor) return true
    return validarCpfCnpj(valor) || 'CPF ou CNPJ inválido'
  }

  const minLength = (min: number) => (valor: string): true | string =>
    (valor ?? '').length >= min || `Deve ter pelo menos ${min} caracteres`

  const maxLength = (max: number) => (valor: string | null | undefined): true | string =>
    !valor || valor.length <= max || `Deve ter no máximo ${max} caracteres`

  const onlyNumbers = (valor: string): true | string =>
    /^[0-9]+$/.test(valor ?? '') || 'Campo deve conter apenas números'

  const greaterThanZero = (valor: number | string): true | string => {
    const num = Number(valor)
    return num > 0 || 'Deve ser maior que zero'
  }

  /** Aceita vazio; se preenchido, exige número >= 0 (inclui zero). */
  const nonNegative = (valor: number | string | null | undefined): true | string => {
    if (valor === '' || valor == null) return true
    return Number(valor) >= 0 || 'Informe um valor maior ou igual a zero'
  }

  /**
   * Valida relação entre estoque mínimo e máximo (quando máximo > 0).
   * Gap L6 — porta `rules.estoqueMinimoNaoMaiorQueMaximo` do legado.
   */
  const estoqueMinimoNaoMaiorQueMaximo = (min: number, max: number): true | string => {
    const minimo = Number(min ?? 0)
    const maximo = Number(max ?? 0)
    if (maximo > 0 && minimo > maximo) {
      return 'Estoque mínimo não pode ser maior que o máximo'
    }
    return true
  }

  /** Gap L6 — saldo de estoque não pode ser menor que o reservado. */
  const saldoNaoMenorQueReservado = (saldo: number, reservado: number): true | string => {
    const s = Number(saldo ?? 0)
    const r = Number(reservado ?? 0)
    if (r > 0 && s < r) {
      return 'Saldo de estoque não pode ser menor que o reservado'
    }
    return true
  }

  /** Gap L6 — fator de conversão da unidade de medida deve ser maior que zero. */
  const fatorMaiorQueZero = (fator: number): true | string => {
    const f = Number(fator ?? 0)
    return f > 0 || 'O fator de conversão deve ser maior que zero'
  }

  return {
    required,
    requiredIf,
    email,
    phone,
    cep,
    cpf,
    cnpj,
    cpfCnpj,
    minLength,
    maxLength,
    onlyNumbers,
    greaterThanZero,
    nonNegative,
    estoqueMinimoNaoMaiorQueMaximo,
    saldoNaoMenorQueReservado,
    fatorMaiorQueZero
  }
}
