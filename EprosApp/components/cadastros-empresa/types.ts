/**
 * Tipos do formulário de Empresa (fatia Cadastro Empresas).
 *
 * Espelham os commands/DTOs da API (`CriarEmpresaCommand`/`AtualizarEmpresaCommand`,
 * `EmpresaParametrosDfeCommands`) — payload plano (Guid como string), sem os campos
 * `endereco.municipioId` do legado (a API nova usa `Cidade`/`Estado` em texto).
 */

export type RegimeTributario = 1 | 2 | 3 // 1 Simples Nacional, 2 Lucro Presumido, 3 Lucro Real
export type RegimeApuracao = 1 | 2 | 3 // 1 Cumulativo, 2 Não Cumulativo, 3 Misto
export type TipoAmbienteDfe = 1 | 2 // 1 Produção, 2 Homologação

export interface EmpresaEndereco {
  logradouro: string
  numero: string
  complemento?: string | null
  bairro: string
  cep: string
  cidade: string
  estado: string
}

export interface ParametrosDfeNfe {
  nfeSerieProducao: number
  nfeProximoNrProducao: number
  nfeSerieHomologacao: number
  nfeProximoNrHomologacao: number
  valorAliquotaCreditoIcms: number
  nfeGerarContingenciaEmHomologacao: boolean
  indicadorSt: boolean
  emitirNfeConjugada: boolean
}

export interface ParametrosDfeNfceHomologacao {
  nfceCscHomologacao?: string | null
  nfceIdCscHomologacao?: string | null
  nfceSerieHomologacao: number
  nfceProximoNrHomologacao: number
  nfceGerarContingenciaEmHomologacao: boolean
}

export interface ParametrosDfeNfceProducao {
  nfceCscProducao?: string | null
  nfceIdCscProducao?: string | null
  nfceSerieProducao: number
  nfceProximoNrProducao: number
}

export interface EmpresaParametrosDfe {
  id?: string | null
  destacarIcmsSt: boolean
  nfe: ParametrosDfeNfe
  nfceHomologacao: ParametrosDfeNfceHomologacao
  nfceProducao: ParametrosDfeNfceProducao
  tipoAmbienteNfce: TipoAmbienteDfe
  tipoAmbienteNfe: TipoAmbienteDfe
}

export interface EmpresaContato {
  id?: string
  nome: string
  telefone?: string | null
  email?: string | null
  tipoTelefone?: number | null
}

/** Estado editável do formulário (tela `[id].vue`). */
export interface EmpresaFormState {
  id?: string
  razaoSocial: string
  nomeFantasia?: string | null
  cnpj: string
  inscricaoEstadual?: string | null
  inscricaoMunicipal?: string | null
  inscricaoSuframa?: string | null
  cnae?: string | null
  regimeTributario: RegimeTributario
  regimeApuracao: RegimeApuracao
  pessoaGrupoId?: string | null
  produtoGrupoId?: string | null
  planoContasFinanceiroId?: string | null
  tributarioGrupoId?: string | null
  ncmTributacaoId?: string | null
  certificadoDigitalId?: string | null
  empresaParametrosDfeId?: string | null
  linkWebApiAppVendas?: string | null
  tokenMercadoPagoPix?: string | null
  logo?: string | null
  endereco: EmpresaEndereco
  empresaParametrosDfe: EmpresaParametrosDfe
  certificadoDigitalDataValidade?: string | null
}

/** Valores iniciais para "nova empresa". */
export function criarEmpresaFormInicial(): EmpresaFormState {
  return {
    razaoSocial: '',
    nomeFantasia: '',
    cnpj: '',
    inscricaoEstadual: '',
    inscricaoMunicipal: '',
    inscricaoSuframa: '',
    cnae: '',
    regimeTributario: 1,
    regimeApuracao: 1,
    pessoaGrupoId: null,
    produtoGrupoId: null,
    planoContasFinanceiroId: null,
    tributarioGrupoId: null,
    ncmTributacaoId: null,
    certificadoDigitalId: null,
    empresaParametrosDfeId: null,
    linkWebApiAppVendas: '',
    tokenMercadoPagoPix: '',
    logo: '',
    endereco: {
      logradouro: '',
      numero: '',
      complemento: '',
      bairro: '',
      cep: '',
      cidade: '',
      estado: ''
    },
    empresaParametrosDfe: {
      destacarIcmsSt: false,
      nfe: {
        nfeSerieProducao: 1,
        nfeProximoNrProducao: 1,
        nfeSerieHomologacao: 1,
        nfeProximoNrHomologacao: 1,
        valorAliquotaCreditoIcms: 0,
        nfeGerarContingenciaEmHomologacao: false,
        indicadorSt: false,
        emitirNfeConjugada: false
      },
      nfceHomologacao: {
        nfceCscHomologacao: '',
        nfceIdCscHomologacao: '',
        nfceSerieHomologacao: 1,
        nfceProximoNrHomologacao: 1,
        nfceGerarContingenciaEmHomologacao: false
      },
      nfceProducao: {
        nfceCscProducao: '',
        nfceIdCscProducao: '',
        nfceSerieProducao: 1,
        nfceProximoNrProducao: 1
      },
      tipoAmbienteNfce: 2,
      tipoAmbienteNfe: 2
    },
    certificadoDigitalDataValidade: null
  }
}
