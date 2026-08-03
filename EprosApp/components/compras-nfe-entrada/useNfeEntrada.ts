/**
 * Composable local da fatia 13 (Compras — NF-e de Entrada).
 *
 * Centraliza estado + IO das telas:
 *   - `pages/erp/compras/emissao/nfe-entrada/[[id]].vue`
 *   - `pages/erp/compras/emissao/devolucao-retorno/nfe-entrada/[[id]].vue`
 *
 * Porta o comportamento das telas legadas equivalentes (que usavam `useNfeEmissaoPage(nfeEntradaConfig)`):
 * carregar compra existente, salvar/lançar a NF-e de entrada, transmitir com overlay + realtime,
 * cancelar, gerar/baixar DANFE e (na devolução) referenciar as notas de origem.
 *
 * IO exclusivamente pelo cliente compartilhado `useApi` (padrão CommandResult).
 * Endpoints: `compras` (lancar, {id}, {id}/cancelar) e — quando existirem no backend —
 * `vendas-fiscal` (agregado fiscal/DANFE) e `fiscal/documentos`.
 *
 * Observação de porte: no legado o cálculo de impostos/totais e a transmissão fiscal ocorriam
 * no servidor (hub `hubs/compra`/`entrada-propria`). No backend novo `compras/lancar` persiste o
 * agregado de compra (fornecedor + itens) de forma síncrona; os totais são calculados no cliente
 * (`calcularTotais`) a partir dos itens + ajustes manuais.
 */
import { computed, reactive, ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useRealtime } from '~/composables/useRealtime'
import {
  criarEntradaFormInicial,
  type EntradaForm,
  type EntradaItem,
  type EntradaTotais
} from './tipos'

/** Etapas exibidas no overlay de transmissão (porta `getNfeEntradaSteps` do legado). */
export const PASSOS_TRANSMISSAO = [
  { text: 'Validando dados da nota de entrada...' },
  { text: 'Transmitindo para a SEFAZ...' },
  { text: 'Lançando a compra e movimentando o estoque...' },
  { text: 'Gerando contas a pagar...' },
  { text: 'Finalizando operação...' }
]

export function useNfeEntrada(modo: 'entrada' | 'devolucao' = 'entrada') {
  const toast = useToast()
  const realtime = useRealtime('/hubs/compra')

  // --- Estado principal ---
  const nfe = reactive<EntradaForm>(criarEntradaFormInicial(modo))
  const carregando = ref(false)
  const salvando = ref(false)
  const erros = ref<Record<string, string>>({})

  // Overlay de transmissão
  const overlayVisivel = ref(false)
  const overlayPasso = ref(0)
  const overlayErro = ref<string | null>(null)

  // DANFE
  const danfeSrc = ref<Blob | string | null>(null)
  const danfeVisivel = ref(false)

  const ehEdicao = computed(() => !!nfe.id)
  const lancada = computed(() => nfe.situacao === 'Autorizada' || nfe.situacao === 'Lançada')
  const cancelada = computed(() => nfe.situacao === 'Cancelada')
  const podeTransmitir = computed(() => !lancada.value && !cancelada.value)
  const semFornecedor = computed(() => !nfe.fornecedor.pessoaId && !nfe.fornecedor.documento)

  // --- Cálculo de totais (client-side) ---
  const totais = computed<EntradaTotais>(() => calcularTotais(nfe))

  function recalcularItem(item: EntradaItem): void {
    const bruto = (item.quantidade || 0) * (item.valorUnitario || 0)
    item.total = Math.max(0, arred4(bruto - (item.descontoValor || 0)))
  }

  function calcularTotais(form: EntradaForm): EntradaTotais {
    let valorProduto = 0
    let valorDesconto = 0
    let valorIcms = 0
    let valorIpi = 0
    for (const item of form.itens) {
      const bruto = (item.quantidade || 0) * (item.valorUnitario || 0)
      // Arredonda o item a 4 casas antes de agregar (paridade com o legado, gap L7).
      const liquido = Math.max(0, arred4(bruto - (item.descontoValor || 0)))
      valorProduto += bruto
      valorDesconto += item.descontoValor || 0
      valorIcms += liquido * ((item.aliquotaIcms || 0) / 100)
      valorIpi += liquido * ((item.aliquotaIpi || 0) / 100)
    }
    valorDesconto += form.descontoManual || 0
    const valorFrete = form.freteManual || 0
    const valorSeguro = form.seguroManual || 0
    const valorOutro = form.outroManual || 0
    const valorNotaFiscal =
      valorProduto - valorDesconto + valorFrete + valorSeguro + valorOutro + valorIpi
    return {
      valorProduto: arred(valorProduto),
      valorDesconto: arred(valorDesconto),
      valorFrete: arred(valorFrete),
      valorSeguro: arred(valorSeguro),
      valorOutro: arred(valorOutro),
      valorIcms: arred(valorIcms),
      valorIpi: arred(valorIpi),
      valorNotaFiscal: arred(Math.max(0, valorNotaFiscal))
    }
  }

  // --- Carregar compra existente ---
  async function carregarCompra(id: string): Promise<void> {
    carregando.value = true
    try {
      const resp = await useApi(`/compras/{id}`, { params: { id } })
      const dados = extrairDados<Record<string, unknown>>(resp)
      if (!dados) {
        toast.error('Compra não encontrada')
        return
      }
      mapearRespostaParaForm(dados)
    } catch (e) {
      toast.error(obterMensagemErro(e))
    } finally {
      carregando.value = false
    }
  }

  /** Mescla o retorno da API no formulário reativo, preservando o formato local. */
  function mapearRespostaParaForm(dados: Record<string, unknown>): void {
    const base = criarEntradaFormInicial(modo)
    const forn = (dados.fornecedor as Record<string, unknown>) ?? {}
    Object.assign(nfe, {
      ...base,
      id: (dados.id as string) ?? null,
      numero: (dados.numeroNota as string) ?? (dados.numero as string) ?? null,
      serie: (dados.serie as string) ?? null,
      chave: (dados.chaveAcesso as string) ?? (dados.chave as string) ?? null,
      situacao: (dados.situacao as string) ?? null,
      naturezaOperacao: (dados.naturezaOperacao as string) ?? base.naturezaOperacao,
      modalidadeFrete: (dados.modalidadeFrete as number) ?? base.modalidadeFrete,
      dataEmissao: (dados.dataEmissao as string) ?? base.dataEmissao,
      dataEntrada: (dados.dataEntrada as string) ?? base.dataEntrada,
      informacoesComplementares: (dados.informacoesComplementares as string) ?? '',
      informacoesAdicionaisFisco: (dados.informacoesAdicionaisFisco as string) ?? '',
      fornecedor: {
        ...base.fornecedor,
        pessoaId: (forn.pessoaId as number) ?? (dados.fornecedorPessoaId as number) ?? null,
        nome: (forn.nome as string) ?? (dados.fornecedorNome as string) ?? '',
        documento: (forn.documento as string) ?? (dados.fornecedorCnpj as string) ?? '',
        inscricaoEstadual: (forn.inscricaoEstadual as string) ?? '',
        enderecoFormatado: (forn.enderecoFormatado as string) ?? ''
      },
      itens: mapearItens(dados.itens as unknown[]),
      duplicatas: (dados.duplicatas as EntradaForm['duplicatas']) ?? [],
      pagamentos: (dados.pagamentos as EntradaForm['pagamentos']) ?? [],
      chavesReferenciadas: (dados.chavesReferenciadas as string[]) ?? []
    })
  }

  function mapearItens(lista: unknown[] | undefined): EntradaItem[] {
    if (!Array.isArray(lista)) return []
    return lista.map((raw) => {
      const i = raw as Record<string, unknown>
      const item: EntradaItem = {
        _uid: `l-${Math.random().toString(36).slice(2, 10)}`,
        produtoId: (i.produtoId as number) ?? null,
        codigoProduto: (i.sku as string) ?? (i.codigoProduto as string) ?? '',
        nomeProduto: (i.nomeProduto as string) ?? (i.descricao as string) ?? '',
        ncm: (i.ncm as string) ?? '',
        cfop: (i.cfop as number) ?? null,
        csosnCst: (i.csosnCst as string) ?? (i.cst as string) ?? '',
        unidade: (i.unidade as string) ?? 'UN',
        quantidade: (i.quantidade as number) ?? 0,
        valorUnitario: (i.precoUnitario as number) ?? (i.valorUnitario as number) ?? 0,
        descontoValor: (i.descontoValor as number) ?? (i.valorDesconto as number) ?? 0,
        aliquotaIcms: (i.aliquotaIcms as number) ?? 0,
        aliquotaIpi: (i.aliquotaIpi as number) ?? 0,
        total: 0,
        informacoesAdicionais: (i.informacoesAdicionais as string) ?? ''
      }
      recalcularItem(item)
      return item
    })
  }

  // --- Montagem do payload para a API (compras/lancar) ---
  function montarPayloadCompra(): Record<string, unknown> {
    const t = totais.value
    return {
      // Contrato de `LancarCompraCommand` (backend novo).
      fornecedorCnpj: soDigitos(nfe.fornecedor.documento),
      fornecedorNome: nfe.fornecedor.nome,
      numeroNota: nfe.numero ?? '',
      chaveAcesso: soDigitos(nfe.chave ?? ''),
      valorTotal: t.valorNotaFiscal,
      dataEmissao: nfe.dataEmissao,
      itens: nfe.itens.map((i) => ({
        sku: i.codigoProduto,
        nomeProduto: i.nomeProduto,
        quantidade: i.quantidade,
        precoUnitario: i.valorUnitario,
        valorIms: arred((Math.max(0, arred4(i.quantidade * i.valorUnitario - i.descontoValor))) * ((i.aliquotaIcms || 0) / 100)),
        valorIpi: arred((Math.max(0, arred4(i.quantidade * i.valorUnitario - i.descontoValor))) * ((i.aliquotaIpi || 0) / 100))
      })),
      // Campos adicionais (fornecedor/frete/observações/referências) — enviados para o
      // caso de o backend evoluir o contrato; ignorados pela versão atual do command.
      fornecedorPessoaId: nfe.fornecedor.pessoaId,
      naturezaOperacao: nfe.naturezaOperacao || null,
      modalidadeFrete: nfe.modalidadeFrete,
      dataEntrada: nfe.dataEntrada,
      informacoesComplementares: nfe.informacoesComplementares || null,
      informacoesAdicionaisFisco: nfe.informacoesAdicionaisFisco || null,
      chavesReferenciadas: nfe.chavesReferenciadas,
      // Pagamentos/duplicatas ao fornecedor — enviados para o caso de o backend evoluir o
      // contrato de `LancarCompraCommand` (hoje aceita apenas o agregado fornecedor+itens).
      pagamentos: nfe.pagamentos.map((p) => ({
        tipoPagamento: p.tipoPagamento,
        valorPagamento: p.valorPagamento,
        valorTroco: p.valorTroco
      })),
      duplicatas: nfe.duplicatas.map((d) => ({
        numero: d.numero,
        dataVencimento: d.dataVencimento,
        valor: d.valor
      })),
      total: {
        valorProduto: t.valorProduto,
        valorDesconto: t.valorDesconto,
        valorFrete: t.valorFrete,
        valorSeguro: t.valorSeguro,
        valorOutro: t.valorOutro,
        valorIcms: t.valorIcms,
        valorIpi: t.valorIpi,
        valorNotaFiscal: t.valorNotaFiscal
      }
    }
  }

  // --- Validação ---
  function validar(): boolean {
    const novosErros: Record<string, string> = {}
    if (!nfe.fornecedor.pessoaId && !nfe.fornecedor.documento.trim()) {
      novosErros.fornecedor = 'Selecione o fornecedor'
    }
    if (!nfe.fornecedor.nome.trim()) novosErros.fornecedor = 'Informe o nome do fornecedor'
    if (!nfe.numero || !nfe.numero.trim()) novosErros.numero = 'Informe o número da nota'
    if (!nfe.chave || soDigitos(nfe.chave).length !== 44) {
      novosErros.chave = 'A chave de acesso deve ter 44 dígitos'
    }
    if (!nfe.dataEmissao) novosErros.dataEmissao = 'Data de emissão é obrigatória'
    if (nfe.itens.length === 0) novosErros.itens = 'Adicione ao menos um produto'
    for (const item of nfe.itens) {
      if (!item.codigoProduto.trim() || !item.nomeProduto.trim()) {
        novosErros.itens = 'Há produtos incompletos na lista (código e descrição obrigatórios)'
        break
      }
      if (item.quantidade <= 0 || item.valorUnitario <= 0) {
        novosErros.itens = 'Quantidade e valor unitário devem ser maiores que zero'
        break
      }
    }
    // Na devolução/retorno é obrigatório referenciar a(s) nota(s) de origem.
    if (modo === 'devolucao' && nfe.chavesReferenciadas.length === 0) {
      novosErros.referenciadas = 'Referencie ao menos uma NF-e de origem'
    }
    erros.value = novosErros
    return Object.keys(novosErros).length === 0
  }

  // --- Salvar (lançar) a compra/NF-e de entrada ---
  async function salvar(): Promise<boolean> {
    if (!validar()) {
      toast.error('Preencha os campos obrigatórios antes de salvar')
      return false
    }
    salvando.value = true
    try {
      const payload = montarPayloadCompra()
      const resp = await useApi.post<CommandResult<{ id?: string } | string>>('/compras/lancar', payload)
      const dados = extrairDados<{ id?: string } | string>(resp)
      const novoId = typeof dados === 'string' ? dados : dados?.id
      if (novoId) nfe.id = novoId
      nfe.situacao = 'Lançada'
      toast.success('NF-e de entrada lançada com sucesso')
      return true
    } catch (e) {
      toast.error(obterMensagemErro(e))
      return false
    } finally {
      salvando.value = false
    }
  }

  // --- Transmitir (lançar com overlay + realtime) ---
  async function transmitir(): Promise<void> {
    if (!validar()) {
      toast.error('Preencha os campos obrigatórios antes de transmitir')
      return
    }

    overlayErro.value = null
    overlayPasso.value = 0
    overlayVisivel.value = true

    // Conecta ao hub para acompanhar o progresso em tempo real (se disponível).
    await realtime.conectar({
      NfTransmissionStep: (...args: unknown[]) => {
        const passo = Number(args[0])
        if (!Number.isNaN(passo)) overlayPasso.value = passo
      }
    })

    try {
      const payload = montarPayloadCompra()
      const resp = await useApi.post<CommandResult<Record<string, unknown> | string>>('/compras/lancar', payload)
      const dados = extrairDados<Record<string, unknown> | string>(resp)
      overlayPasso.value = PASSOS_TRANSMISSAO.length - 1
      if (dados && typeof dados === 'object') {
        nfe.id = (dados.id as string) ?? nfe.id
        nfe.chave = (dados.chaveAcesso as string) ?? nfe.chave
      } else if (typeof dados === 'string') {
        nfe.id = dados
      }
      nfe.situacao = 'Autorizada'
      toast.success('NF-e de entrada transmitida e lançada com sucesso')
      setTimeout(() => (overlayVisivel.value = false), 800)
    } catch (e) {
      overlayErro.value = obterMensagemErro(e)
    } finally {
      await realtime.desconectar()
    }
  }

  // --- Cancelar a compra/NF-e de entrada ---
  async function cancelar(motivo: string): Promise<boolean> {
    if (!nfe.id) {
      // Ainda não persistida: apenas limpa a tela.
      limparTela()
      return true
    }
    salvando.value = true
    try {
      await useApi(`/compras/{id}/cancelar`, {
        method: 'POST',
        params: { id: nfe.id },
        body: { motivo }
      })
      nfe.situacao = 'Cancelada'
      toast.success('NF-e de entrada cancelada com sucesso')
      return true
    } catch (e) {
      toast.error(obterMensagemErro(e))
      return false
    } finally {
      salvando.value = false
    }
  }

  // --- DANFE ---
  async function gerarDanfe(): Promise<void> {
    if (!nfe.id) {
      toast.warning('Salve a nota antes de visualizar a DANFE')
      return
    }
    carregando.value = true
    try {
      // Endpoint de DANFE de entrada ainda não existe no backend novo (ver relatório de gaps).
      // Tentamos o agregado fiscal genérico; se falhar, avisamos o usuário.
      const blob = await useApi<Blob>(`/vendas-fiscal/{id}/nfe/danfe`, {
        params: { id: nfe.id },
        responseType: 'blob'
      })
      danfeSrc.value = blob
      danfeVisivel.value = true
    } catch (e) {
      toast.error(obterMensagemErro(e))
    } finally {
      carregando.value = false
    }
  }

  function baixarDanfe(): void {
    if (!(danfeSrc.value instanceof Blob)) return
    const url = URL.createObjectURL(danfeSrc.value)
    const a = document.createElement('a')
    a.href = url
    a.download = `danfe-entrada-${nfe.numero ?? nfe.id ?? 'nfe'}.pdf`
    a.click()
    URL.revokeObjectURL(url)
  }

  function imprimirDanfe(): void {
    if (!(danfeSrc.value instanceof Blob)) return
    const url = URL.createObjectURL(danfeSrc.value)
    window.open(url, '_blank')?.focus()
  }

  // --- Manipulação de itens ---
  function adicionarItem(item: EntradaItem): void {
    recalcularItem(item)
    nfe.itens.push(item)
  }

  function atualizarItem(uid: string, item: EntradaItem): void {
    const idx = nfe.itens.findIndex((i) => i._uid === uid)
    if (idx >= 0) {
      recalcularItem(item)
      nfe.itens.splice(idx, 1, item)
    }
  }

  function removerItem(uid: string): void {
    const idx = nfe.itens.findIndex((i) => i._uid === uid)
    if (idx >= 0) nfe.itens.splice(idx, 1)
  }

  function salvarReferenciadas(chaves: string[]): void {
    nfe.chavesReferenciadas = [...chaves]
  }

  function limparTela(): void {
    Object.assign(nfe, criarEntradaFormInicial(modo))
    erros.value = {}
    danfeSrc.value = null
    danfeVisivel.value = false
  }

  return {
    // estado
    nfe,
    carregando,
    salvando,
    erros,
    totais,
    ehEdicao,
    lancada,
    cancelada,
    podeTransmitir,
    semFornecedor,
    // overlay
    overlayVisivel,
    overlayPasso,
    overlayErro,
    passosTransmissao: PASSOS_TRANSMISSAO,
    // danfe
    danfeSrc,
    danfeVisivel,
    // ações
    carregarCompra,
    salvar,
    transmitir,
    cancelar,
    gerarDanfe,
    baixarDanfe,
    imprimirDanfe,
    adicionarItem,
    atualizarItem,
    removerItem,
    recalcularItem,
    salvarReferenciadas,
    limparTela
  }
}

/** Arredonda para 2 casas decimais evitando ruído de ponto flutuante. */
function arred(v: number): number {
  return Math.round((v + Number.EPSILON) * 100) / 100
}

/** Arredonda para 4 casas decimais (gap L7 — paridade com o legado no item antes de somar). */
function arred4(v: number): number {
  return Math.round((v + Number.EPSILON) * 10000) / 10000
}

/** Remove tudo que não for dígito (para CNPJ/chave). */
function soDigitos(v: string): string {
  return (v ?? '').replace(/\D/g, '')
}
