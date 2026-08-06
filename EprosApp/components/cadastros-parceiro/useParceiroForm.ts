/**
 * Lógica do formulário de parceiro (fatia Cadastro Parceiros).
 *
 * Local à fatia (não é composable compartilhado) — só é usado por
 * `pages/erp/cadastros/parceiros/[id].vue` e `ParceiroForm.vue`.
 *
 * IO exclusivamente via `useApi` (composables/useApi.ts).
 */
import { computed, reactive, ref } from 'vue'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useMask } from '~/composables/useMask'
import { useDocumento } from '~/composables/useDocumento'
import {
  contatoPadrao,
  enderecoPadrao,
  pessoaFormPadrao,
  type GeoItem,
  type PessoaContato,
  type PessoaEndereco,
  type PessoaForm,
  type PessoaVeiculo
} from './types'

/** Item de UF retornado por `cadastros/geografia` (shape defensivo). */
interface UfApi {
  id?: string | number
  sigla?: string
  uf?: string
  descricao?: string
  nome?: string
}

interface MunicipioApi {
  id?: string | number
  nome?: string
  descricao?: string
}

export function useParceiroForm(idRota: string) {
  const toast = useToast()
  const { maskCpfCnpj, somenteDigitos } = useMask()
  const { validarCpfCnpj, validarEmail } = useDocumento()

  const ehNovo = computed(() => !idRota || idRota === 'novo')

  const carregando = ref(false)
  const salvando = ref(false)
  const abaAtiva = ref<'dados_gerais' | 'bancarios' | 'contatos' | 'motorista' | 'transportadora'>('dados_gerais')

  const pessoa = reactive<PessoaForm>(pessoaFormPadrao())

  const ufs = ref<GeoItem[]>([])
  const municipios = ref<GeoItem[]>([])
  const carregandoMunicipios = ref(false)

  // --- Endereço (modal) ---
  const enderecoModalAberto = ref(false)
  const enderecoEditandoIndex = ref<number | null>(null)
  const enderecoDraft = ref<PessoaEndereco>(enderecoPadrao())

  // --- Contato (modal) ---
  const contatoModalAberto = ref(false)
  const contatoEditandoIndex = ref<number | null>(null)
  const contatoDraft = ref<PessoaContato>(contatoPadrao())

  // --- Veículo (motorista/transportadora) ---
  const veiculoModalAberto = ref(false)
  const veiculoModalOrigem = ref<'motorista' | 'transportadora' | null>(null)
  const veiculoEditandoIndex = ref<number | null>(null)
  const veiculoDraft = ref<PessoaVeiculo>({ paisId: 1058, tipoVeiculo: null, uf: null, placa: '', rntrc: '' })

  const classificacoes: { field: keyof PessoaForm; label: string }[] = [
    { field: 'ehCliente', label: 'Cliente' },
    { field: 'ehFornecedor', label: 'Fornecedor' },
    { field: 'ehTransportadora', label: 'Transportadora' },
    { field: 'ehPrestadorServico', label: 'Prestador de Serviço' },
    { field: 'ehProdutorRural', label: 'Produtor Rural' },
    { field: 'ehMotorista', label: 'Motorista' },
    { field: 'ehFuncionario', label: 'Funcionário' }
  ]

  const CLASSIFICACOES_OBRIGATORIAS: (keyof PessoaForm)[] = [
    'ehCliente',
    'ehFornecedor',
    'ehTransportadora',
    'ehPrestadorServico',
    'ehMotorista',
    'ehFuncionario'
  ]

  const temClassificacaoObrigatoria = computed(() =>
    CLASSIFICACOES_OBRIGATORIAS.some((f) => Boolean(pessoa[f]))
  )

  function alternarClassificacao(campo: keyof PessoaForm, ativo: boolean) {
    ;(pessoa as unknown as Record<string, unknown>)[campo as string] = ativo

    if (campo === 'ehCliente' && ativo && !pessoa.pessoaCliente) {
      pessoa.pessoaCliente = { ehConsumidorFinal: true, tipoContribuinte: 'NaoInformado' }
    }
    if (campo === 'ehPrestadorServico' && ativo && !pessoa.pessoaPrestadorServico) {
      pessoa.pessoaPrestadorServico = { cei: null }
    }
    if (campo === 'ehTransportadora' && ativo && !pessoa.pessoaTransportadora) {
      pessoa.pessoaTransportadora = { veiculos: [] }
    }
    if (campo === 'ehMotorista' && ativo && !pessoa.pessoaMotorista) {
      pessoa.pessoaMotorista = { tipoVinculoMotorista: null, veiculos: [] }
    }
    if (campo === 'ehFuncionario' && ativo && !pessoa.pessoaFuncionario) {
      pessoa.pessoaFuncionario = { tipoCargo: null, valorPercentualComissao: 0 }
    }
    if (campo === 'ehProdutorRural' && ativo && !pessoa.pessoaJuridica) {
      pessoa.pessoaJuridica = { cnpj: '', razaoSocial: '', nomeFantasia: '' }
    }
  }

  function aoMudarTipoPessoa(tipo: PessoaForm['tipoPessoa']) {
    pessoa.tipoPessoa = tipo
    if (tipo === 'PessoaFisica' && !pessoa.pessoaFisica) {
      pessoa.pessoaFisica = { cpf: '', nome: '', sobrenome: '' }
    }
    if (tipo === 'PessoaJuridica' && !pessoa.pessoaJuridica) {
      pessoa.pessoaJuridica = { cnpj: '', razaoSocial: '', nomeFantasia: '' }
    }
    if (tipo === 'PessoaEstrangeira' && !pessoa.pessoaEstrangeiro) {
      pessoa.pessoaEstrangeiro = { nome: '', identificacaoEstrangeiro: '' }
    }
  }

  /** Atualiza indicador de IE quando a inscrição estadual muda (regra portada do legado). */
  function atualizarIndicadorIe() {
    const ie = pessoa.pessoaJuridica?.inscricaoEstadual
    if (!pessoa.pessoaJuridica) return
    if (ie === 'ISENTO' || !ie) {
      pessoa.tipoIndicadorIe = 'Isento'
      if (pessoa.pessoaCliente) pessoa.pessoaCliente.ehConsumidorFinal = true
    } else {
      pessoa.tipoIndicadorIe = 'ContribuinteICMS'
      if (pessoa.pessoaCliente) pessoa.pessoaCliente.ehConsumidorFinal = false
    }
  }

  // --- Geografia ---
  async function carregarUfs() {
    try {
      const resposta = await useApi<unknown>('/cadastros/geografia/paises')
      // UFs não têm endpoint dedicado nesta API; usamos lista estática BR como fallback seguro.
      void resposta
    } catch (e) {
      console.error('[useParceiroForm.carregarUfs]', e)
    }
    if (ufs.value.length === 0) {
      ufs.value = UFS_BRASIL.map((s) => ({ id: s, descricao: s }))
    }
  }

  async function carregarMunicipios(uf: string) {
    if (!uf) {
      municipios.value = []
      return
    }
    carregandoMunicipios.value = true
    try {
      const resposta = await useApi<unknown>('/cadastros/geografia/municipios/obter-por-uf/{uf}', {
        params: { uf }
      })
      const lista = extrairLista<MunicipioApi>(resposta) ?? (Array.isArray(resposta) ? (resposta as MunicipioApi[]) : [])
      municipios.value = lista.map((m) => ({ id: String(m.id ?? ''), descricao: m.nome ?? m.descricao ?? '' }))
    } catch (e) {
      console.error('[useParceiroForm.carregarMunicipios]', e)
      municipios.value = []
    } finally {
      carregandoMunicipios.value = false
    }
  }

  async function consultarCep(cep: string) {
    const limpo = somenteDigitos(cep)
    if (limpo.length !== 8) return
    try {
      const resposta = await useApi<{ uf?: string; municipio?: string; bairro?: string; logradouro?: string }>(
        '/cadastros/geografia/cep/{cep}',
        { params: { cep: limpo } }
      )
      if (resposta) {
        enderecoDraft.value.bairro = resposta.bairro ?? enderecoDraft.value.bairro
        enderecoDraft.value.logradouro = resposta.logradouro ?? enderecoDraft.value.logradouro
        if (resposta.uf) {
          enderecoDraft.value.uf = resposta.uf
          await carregarMunicipios(resposta.uf)
          const encontrado = municipios.value.find(
            (m) => m.descricao.toLocaleUpperCase() === (resposta.municipio ?? '').toLocaleUpperCase()
          )
          if (encontrado) enderecoDraft.value.municipioId = String(encontrado.id)
        }
      }
    } catch (e) {
      toast.warning('CEP não encontrado. Preencha o endereço manualmente.')
      console.error('[useParceiroForm.consultarCep]', e)
    }
  }

  // --- Endereços ---
  function novoEndereco() {
    enderecoEditandoIndex.value = null
    enderecoDraft.value = enderecoPadrao()
    if (pessoa.enderecos.length === 0) {
      enderecoDraft.value.tipoEndereco = 'Principal'
    }
    enderecoModalAberto.value = true
  }

  async function editarEndereco(index: number) {
    const item = pessoa.enderecos[index]
    if (!item) return
    enderecoEditandoIndex.value = index
    enderecoDraft.value = { ...item }
    if (item.uf) await carregarMunicipios(item.uf)
    enderecoModalAberto.value = true
  }

  function fecharModalEndereco() {
    enderecoModalAberto.value = false
    enderecoEditandoIndex.value = null
  }

  function removerEndereco(index: number) {
    pessoa.enderecos.splice(index, 1)
  }

  function confirmarEndereco(): boolean {
    const e = enderecoDraft.value
    const estrangeiro = e.paisId !== 1058

    if (!e.paisId || (!estrangeiro && (!e.uf || !e.municipioId)) || !e.numero || !e.bairro || !e.logradouro || !e.tipoEndereco) {
      toast.warning('Preencha todos os campos obrigatórios do endereço.')
      return false
    }

    if (e.tipoEndereco === 'Entrega' && (!e.nomeDoRecebedor || !e.documentoDoRecebedor)) {
      toast.warning('Informe nome e documento do recebedor para endereço de entrega.')
      return false
    }

    const jaTemPrincipal = pessoa.enderecos.some(
      (end, i) => end.tipoEndereco === 'Principal' && i !== enderecoEditandoIndex.value
    )
    if (e.tipoEndereco === 'Principal' && jaTemPrincipal) {
      toast.warning('Já existe um endereço principal cadastrado.')
      return false
    }

    const completo: PessoaEndereco = { ...e }
    if (enderecoEditandoIndex.value !== null) {
      pessoa.enderecos[enderecoEditandoIndex.value] = completo
    } else {
      pessoa.enderecos.push(completo)
    }
    fecharModalEndereco()
    return true
  }

  // --- Contatos ---
  const contatosOrdenados = computed(() =>
    [...pessoa.contatos].sort((a, b) => Number(b.ehPrincipal) - Number(a.ehPrincipal))
  )

  function novoContato() {
    contatoEditandoIndex.value = null
    contatoDraft.value = contatoPadrao()
    contatoModalAberto.value = true
  }

  function editarContato(index: number) {
    const item = pessoa.contatos[index]
    if (!item) return
    contatoEditandoIndex.value = index
    contatoDraft.value = { ...item }
    contatoModalAberto.value = true
  }

  function fecharModalContato() {
    contatoModalAberto.value = false
    contatoEditandoIndex.value = null
  }

  function removerContato(index: number) {
    pessoa.contatos.splice(index, 1)
  }

  function definirContatoPrincipal(index: number) {
    pessoa.contatos.forEach((c, i) => {
      c.ehPrincipal = i === index
    })
  }

  function confirmarContato(): boolean {
    const c = contatoDraft.value
    if (!c.nome) {
      toast.warning('O campo Nome do contato é obrigatório.')
      return false
    }
    if (c.email && !validarEmail(c.email)) {
      toast.warning('E-mail de contato inválido.')
      return false
    }
    const ehPrimeiro = pessoa.contatos.length === 0 && contatoEditandoIndex.value === null
    const dados: PessoaContato = { ...c, ehPrincipal: ehPrimeiro || c.ehPrincipal }

    if (contatoEditandoIndex.value !== null) {
      pessoa.contatos[contatoEditandoIndex.value] = dados
    } else {
      pessoa.contatos.push(dados)
    }
    if (dados.ehPrincipal) {
      pessoa.contatos.forEach((item, i) => {
        const idx = contatoEditandoIndex.value ?? pessoa.contatos.length - 1
        if (i !== idx) item.ehPrincipal = false
      })
    }
    fecharModalContato()
    return true
  }

  // --- Veículos (motorista/transportadora) ---
  function novoVeiculo(origem: 'motorista' | 'transportadora') {
    veiculoModalOrigem.value = origem
    veiculoEditandoIndex.value = null
    veiculoDraft.value = { paisId: 1058, tipoVeiculo: null, uf: null, placa: '', rntrc: '' }
    veiculoModalAberto.value = true
  }

  function editarVeiculo(origem: 'motorista' | 'transportadora', index: number) {
    const lista = origem === 'motorista' ? pessoa.pessoaMotorista?.veiculos : pessoa.pessoaTransportadora?.veiculos
    const item = lista?.[index]
    if (!item) return
    veiculoModalOrigem.value = origem
    veiculoEditandoIndex.value = index
    veiculoDraft.value = { ...item }
    veiculoModalAberto.value = true
  }

  function fecharModalVeiculo() {
    veiculoModalAberto.value = false
    veiculoModalOrigem.value = null
    veiculoEditandoIndex.value = null
  }

  function removerVeiculo(origem: 'motorista' | 'transportadora', index: number) {
    if (origem === 'motorista') pessoa.pessoaMotorista?.veiculos.splice(index, 1)
    else pessoa.pessoaTransportadora?.veiculos.splice(index, 1)
  }

  function confirmarVeiculo(): boolean {
    const v = veiculoDraft.value
    if (!v.paisId || !v.tipoVeiculo || !v.uf || !v.placa) {
      toast.warning('Preencha todos os campos obrigatórios do veículo.')
      return false
    }
    const placa = v.placa.trim().toUpperCase().replace(/[^0-9A-Z]/g, '')
    const linha: PessoaVeiculo = { ...v, placa, rntrc: v.rntrc?.trim() || null }

    const origem = veiculoModalOrigem.value
    const lista =
      origem === 'motorista'
        ? (pessoa.pessoaMotorista ??= { tipoVinculoMotorista: null, veiculos: [] }).veiculos
        : (pessoa.pessoaTransportadora ??= { veiculos: [] }).veiculos

    if (veiculoEditandoIndex.value !== null) lista[veiculoEditandoIndex.value] = linha
    else lista.push(linha)

    fecharModalVeiculo()
    return true
  }

  // --- Carregar / Salvar ---
  async function carregar() {
    if (ehNovo.value) return
    carregando.value = true
    try {
      const resposta = await useApi<unknown>('/cadastros/pessoas/{id}', { params: { id: idRota } })
      const dados = extrairDados<Partial<PessoaForm>>(resposta) ?? (resposta as Partial<PessoaForm>)
      Object.assign(pessoa, pessoaFormPadrao(), dados)
    } catch (e) {
      toast.error(obterMensagemErro(e))
      console.error('[useParceiroForm.carregar]', e)
    } finally {
      carregando.value = false
    }
  }

  function validarDadosGerais(): string | null {
    if (!pessoa.tipoPessoa) return 'Selecione o Tipo de Pessoa.'
    if (pessoa.tipoPessoa === 'PessoaFisica') {
      if (!pessoa.pessoaFisica?.nome) return 'Informe o nome da pessoa física.'
      if (!validarCpfCnpj(pessoa.pessoaFisica.cpf)) return 'CPF inválido.'
    }
    if (pessoa.tipoPessoa === 'PessoaJuridica') {
      if (!pessoa.pessoaJuridica?.razaoSocial) return 'Informe a razão social.'
      if (!validarCpfCnpj(pessoa.pessoaJuridica.cnpj)) return 'CNPJ inválido.'
    }
    if (pessoa.tipoPessoa === 'PessoaEstrangeira' && !pessoa.pessoaEstrangeiro?.nome) {
      return 'Informe o nome da pessoa estrangeira.'
    }
    if (!temClassificacaoObrigatoria.value) {
      return 'Selecione ao menos uma classificação: Cliente, Fornecedor, Transportadora, Prestador de Serviço, Motorista ou Funcionário.'
    }
    if (!pessoa.enderecos.some((e) => e.tipoEndereco === 'Principal')) {
      return 'É necessário informar ao menos um endereço principal.'
    }
    return null
  }

  async function salvar(): Promise<boolean> {
    const erroValidacao = validarDadosGerais()
    if (erroValidacao) {
      abaAtiva.value = 'dados_gerais'
      toast.warning(erroValidacao)
      return false
    }

    salvando.value = true
    try {
      const payload: Record<string, unknown> = {
        ...pessoa,
        pessoaFisica:
          pessoa.tipoPessoa === 'PessoaFisica' && pessoa.pessoaFisica
            ? { ...pessoa.pessoaFisica, cpf: somenteDigitos(pessoa.pessoaFisica.cpf) }
            : null,
        pessoaJuridica:
          (pessoa.tipoPessoa === 'PessoaJuridica' || pessoa.ehProdutorRural) && pessoa.pessoaJuridica
            ? { ...pessoa.pessoaJuridica, cnpj: somenteDigitos(pessoa.pessoaJuridica.cnpj) }
            : null,
        pessoaEstrangeiro: pessoa.tipoPessoa === 'PessoaEstrangeira' ? pessoa.pessoaEstrangeiro : null,
        pessoaCliente: pessoa.ehCliente ? pessoa.pessoaCliente : null,
        pessoaFuncionario: pessoa.ehFuncionario ? pessoa.pessoaFuncionario : null,
        pessoaPrestadorServico: pessoa.ehPrestadorServico ? pessoa.pessoaPrestadorServico : null,
        pessoaMotorista: pessoa.ehMotorista ? pessoa.pessoaMotorista : null,
        pessoaTransportadora: pessoa.ehTransportadora ? pessoa.pessoaTransportadora : null,
        enderecos: pessoa.enderecos.map((e) => ({ ...e, cep: e.cep ? somenteDigitos(e.cep) : null })),
        contatos: pessoa.contatos.map((c) => ({ ...c, numeroTelefone: c.numeroTelefone ? somenteDigitos(c.numeroTelefone) : null }))
      }

      if (ehNovo.value) {
        await useApi.post('/cadastros/pessoas', payload)
        toast.success('Parceiro cadastrado com sucesso.')
      } else {
        await useApi.put(`/cadastros/pessoas/{id}`, { ...payload, id: idRota }, { params: { id: idRota } })
        toast.success('Parceiro atualizado com sucesso.')
      }
      return true
    } catch (e) {
      toast.error(obterMensagemErro(e))
      console.error('[useParceiroForm.salvar]', e)
      return false
    } finally {
      salvando.value = false
    }
  }

  return {
    ehNovo,
    carregando,
    salvando,
    abaAtiva,
    pessoa,
    ufs,
    municipios,
    carregandoMunicipios,
    classificacoes,
    temClassificacaoObrigatoria,
    alternarClassificacao,
    aoMudarTipoPessoa,
    atualizarIndicadorIe,
    carregarUfs,
    carregarMunicipios,
    consultarCep,
    // endereços
    enderecoModalAberto,
    enderecoEditandoIndex,
    enderecoDraft,
    novoEndereco,
    editarEndereco,
    fecharModalEndereco,
    removerEndereco,
    confirmarEndereco,
    // contatos
    contatoModalAberto,
    contatoEditandoIndex,
    contatoDraft,
    contatosOrdenados,
    novoContato,
    editarContato,
    fecharModalContato,
    removerContato,
    definirContatoPrincipal,
    confirmarContato,
    // veículos
    veiculoModalAberto,
    veiculoModalOrigem,
    veiculoEditandoIndex,
    veiculoDraft,
    novoVeiculo,
    editarVeiculo,
    fecharModalVeiculo,
    removerVeiculo,
    confirmarVeiculo,
    // IO
    carregar,
    salvar,
    maskCpfCnpj
  }
}

/** UFs do Brasil — fallback estático (backend não expõe endpoint dedicado de UF). */
const UFS_BRASIL = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
]
