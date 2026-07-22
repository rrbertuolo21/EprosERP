/**
 * usePessoa — IO do cadastro de Pessoa (fatia GestaoClientes).
 *
 * Rotas reais (PessoasController): `GET /api/v1/cadastros/pessoas` (listagem paginada com
 * filtros extensos de papel/status), `GET /api/v1/cadastros/pessoas/{id}`,
 * `POST /api/v1/cadastros/pessoas`, `PUT/DELETE /api/v1/cadastros/pessoas/{id}`,
 * `GET /api/v1/cadastros/pessoas/localizar-cliente?query=`.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export interface EnderecoPessoa {
  tipoEndereco: number
  paisId: string
  municipioId: string
  subdivisaoId?: string | null
  uf: string
  cep?: string | null
  logradouro: string
  numero?: string | null
  complemento?: string | null
  bairro: string
  referencia?: string | null
  codigoPostalInternacional?: string | null
  linhaEndereco1?: string | null
  linhaEndereco2?: string | null
  latitude?: number | null
  longitude?: number | null
}

export interface ContatoPessoa {
  nome?: string | null
  tipoContatoTelefonico: number
  numeroTelefone?: string | null
  tipoContatoEmail: number
  email?: string | null
  ehPrincipal: boolean
}

export interface VeiculoPessoa {
  paisId: number
  tipoVeiculo: number
  uf: string
  placa: string
  rntrc?: string | null
}

export interface Pessoa {
  id: string
  tipoPessoa: number
  tipoIndicadorIe: number
  pessoaGrupoId?: string | null
  inscricaoSuframa?: number | null
  titularContaBancaria?: string | null
  agenciaContaBancaria?: string | null
  numeroContaBancaria?: string | null
  tipoPix?: number | null
  chavePix?: string | null
  observacoes?: string | null

  fisicaCpf?: string | null
  fisicaNome?: string | null
  fisicaSobrenome?: string | null
  rgNumero?: string | null
  rgOrgaoEmissor?: string | null
  tipoGenero?: number | null
  dataNascimento?: string | null

  juridicaCnpj?: string | null
  razaoSocial?: string | null
  nomeFantasia?: string | null
  inscricaoEstadual?: string | null
  inscricaoMunicipal?: string | null
  cnae?: string | null

  estrangeiroNome?: string | null
  identificacaoEstrangeiro?: string | null

  ehCliente: boolean
  ehFornecedor: boolean
  ehTransportadora: boolean
  ehMotorista: boolean
  ehPrestadorServico: boolean
  ehFuncionario: boolean
  ehProdutorRural: boolean

  clienteEhConsumidorFinal?: boolean | null
  clienteTipoContribuinte?: number | null
  funcionarioTipoCargo?: number | null
  funcionarioComissao?: number | null
  motoristaTipoVinculo?: number | null
  motoristaCategoriaCnh?: number | null
  motoristaDataEmissaoCnh?: string | null
  motoristaDataVencimentoCnh?: string | null
  motoristaRntrc?: string | null
  transportadoraCiot?: string | null
  transportadoraRntrc?: string | null
  prestadorCei?: string | null

  enderecos?: EnderecoPessoa[] | null
  contatos?: ContatoPessoa[] | null
  veiculos?: VeiculoPessoa[] | null
}

export type PessoaCreateDto = Omit<Pessoa, 'id'>

export interface PessoaUpdateDto extends PessoaCreateDto {
  id: string
}

export interface PessoaFiltros {
  localizar?: string
  inscricaoEstadual?: string
  uf?: string
  ehCliente?: boolean
  ehFornecedor?: boolean
  ehTransportadora?: boolean
  ehMotorista?: boolean
  ehPrestadorServico?: boolean
  ehFuncionario?: boolean
  ehProdutorRural?: boolean
  status?: number
}

export function usePessoa() {
  const pessoa = ref<Pessoa | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<Pessoa, PessoaFiltros>('/cadastros/pessoas')

  async function buscarPorId(id: string): Promise<Pessoa | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<Pessoa>>('/cadastros/pessoas/{id}', { params: { id } })
      const dados = extrairDados<Pessoa>(resposta) ?? null
      pessoa.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePessoa.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: PessoaCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/cadastros/pessoas', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePessoa.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: PessoaUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/cadastros/pessoas/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePessoa.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluir(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/cadastros/pessoas/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePessoa.excluir]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  /** Localiza clientes por termo (nome/CPF/CNPJ) — usado em autocomplete de venda. */
  async function localizarCliente(query: string): Promise<Pessoa[]> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<Pessoa[]>>('/cadastros/pessoas/localizar-cliente', { query: { query } })
      return extrairDados<Pessoa[]>(resposta) ?? []
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePessoa.localizarCliente]', e)
      return []
    } finally {
      carregando.value = false
    }
  }

  return {
    pessoa,
    carregando,
    erro,
    itens: lista.itens,
    total: lista.total,
    buscar: lista.buscar,
    filtros: lista.filtros,
    buscarPorId,
    criar,
    atualizar,
    excluir,
    localizarCliente
  }
}
