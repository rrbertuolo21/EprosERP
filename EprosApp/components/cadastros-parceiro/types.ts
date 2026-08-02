/**
 * Tipos de domínio da fatia Cadastro Parceiros (cadastros/pessoas).
 *
 * Não há geração de tipos OpenAPI no novo frontend ainda — tipamos manualmente
 * o suficiente para tornar a tela segura e legível. Espelha o agregado Pessoa
 * do backend (Epros.Modules.GestaoClientes).
 */

export type TipoPessoa = 'PessoaFisica' | 'PessoaJuridica' | 'PessoaEstrangeira'

export type TipoIndicadorIe = 'ContribuinteICMS' | 'Isento' | 'NaoContribuinte'

export type TipoEndereco = 'Principal' | 'Cobranca' | 'Entrega'

export interface PessoaFisica {
  cpf: string
  nome: string
  sobrenome: string
  rgNumero?: string | null
  rgOrgaoEmissor?: string | null
  tipoGenero?: string | null
}

export interface PessoaJuridica {
  cnpj: string
  razaoSocial: string
  nomeFantasia?: string | null
  inscricaoEstadual?: string | null
  inscricaoMunicipal?: string | null
  cnae?: string | null
}

export interface PessoaEstrangeira {
  nome: string
  identificacaoEstrangeiro?: string | null
}

export interface PessoaCliente {
  ehConsumidorFinal: boolean
  tipoContribuinte?: string | null
}

export interface PessoaFuncionario {
  tipoCargo?: string | null
  valorPercentualComissao?: number | null
}

export interface PessoaPrestadorServico {
  cei?: string | null
}

export interface PessoaVeiculo {
  paisId?: number | string | null
  tipoVeiculo?: string | null
  uf?: string | null
  placa: string
  rntrc?: string | null
}

export interface PessoaMotorista {
  tipoVinculoMotorista?: string | null
  veiculos: PessoaVeiculo[]
}

export interface PessoaTransportadora {
  veiculos: PessoaVeiculo[]
}

export interface PessoaEndereco {
  id?: string
  paisId: number
  uf?: string | null
  municipioId?: string | null
  municipioNome?: string | null
  cep?: string | null
  logradouro: string
  numero: string
  bairro: string
  complemento?: string | null
  referencia?: string | null
  tipoEndereco: TipoEndereco
  nomeDoRecebedor?: string | null
  documentoDoRecebedor?: string | null
}

export interface PessoaContato {
  id?: string
  nome: string
  email?: string | null
  tipoContatoEmail?: string | null
  numeroTelefone?: string | null
  tipoContatoTelefonico?: string | null
  ehPrincipal: boolean
}

/** Estado editável do formulário de parceiro (aba "Dados Gerais" + demais). */
export interface PessoaForm {
  id?: string
  tipoPessoa: TipoPessoa | null
  tipoIndicadorIe: TipoIndicadorIe | null
  inscricaoSuframa?: string | null
  pessoaGrupoId?: string | null

  ehCliente: boolean
  ehFornecedor: boolean
  ehTransportadora: boolean
  ehPrestadorServico: boolean
  ehProdutorRural: boolean
  ehMotorista: boolean
  ehFuncionario: boolean

  pessoaFisica?: PessoaFisica | null
  pessoaJuridica?: PessoaJuridica | null
  pessoaEstrangeiro?: PessoaEstrangeira | null
  pessoaCliente?: PessoaCliente | null
  pessoaFuncionario?: PessoaFuncionario | null
  pessoaPrestadorServico?: PessoaPrestadorServico | null
  pessoaMotorista?: PessoaMotorista | null
  pessoaTransportadora?: PessoaTransportadora | null

  titularContaBancaria?: string | null
  agenciaContaBancaria?: string | null
  numeroContaBancaria?: string | null
  tipoPix?: string | null
  chavePix?: string | null
  observacoes?: string | null

  enderecos: PessoaEndereco[]
  contatos: PessoaContato[]
}

/** Linha da listagem — shape defensivo, aceita variações de payload do backend. */
export interface PessoaListaItem {
  [key: string]: unknown
  id: string
  sequenciaTenantId?: number
  tipoPessoa?: TipoPessoa | string | null
  pessoaFisica?: { nome?: string; sobrenome?: string; cpf?: string } | null
  pessoaJuridica?: { razaoSocial?: string; nomeFantasia?: string; cnpj?: string; inscricaoEstadual?: string } | null
  pessoaEstrangeiro?: { nome?: string; identificacaoEstrangeiro?: string } | null
  ehCliente?: boolean
  ehFornecedor?: boolean
  ehTransportadora?: boolean
  ehFuncionario?: boolean
  ehPrestadorServico?: boolean
  ehMotorista?: boolean
  enderecos?: PessoaEndereco[]
  contatos?: PessoaContato[]
}

/** Item genérico { id, descricao } usado por UF/município/país locais. */
export interface GeoItem {
  id: string | number
  descricao: string
}

export function pessoaFormPadrao(): PessoaForm {
  return {
    tipoPessoa: 'PessoaFisica',
    tipoIndicadorIe: 'NaoContribuinte',
    inscricaoSuframa: null,
    ehCliente: false,
    ehFornecedor: false,
    ehTransportadora: false,
    ehPrestadorServico: false,
    ehProdutorRural: false,
    ehMotorista: false,
    ehFuncionario: false,
    pessoaFisica: { cpf: '', nome: '', sobrenome: '' },
    pessoaJuridica: null,
    pessoaEstrangeiro: null,
    pessoaCliente: null,
    pessoaFuncionario: null,
    pessoaPrestadorServico: null,
    pessoaMotorista: null,
    pessoaTransportadora: null,
    titularContaBancaria: null,
    agenciaContaBancaria: null,
    numeroContaBancaria: null,
    tipoPix: null,
    chavePix: null,
    observacoes: null,
    enderecos: [],
    contatos: []
  }
}

export function enderecoPadrao(): PessoaEndereco {
  return {
    paisId: 1058,
    uf: null,
    municipioId: null,
    municipioNome: null,
    cep: '',
    logradouro: '',
    numero: '',
    bairro: '',
    complemento: '',
    referencia: '',
    tipoEndereco: 'Principal',
    nomeDoRecebedor: null,
    documentoDoRecebedor: null
  }
}

export function contatoPadrao(): PessoaContato {
  return {
    nome: '',
    email: '',
    tipoContatoEmail: null,
    numeroTelefone: '',
    tipoContatoTelefonico: null,
    ehPrincipal: false
  }
}
