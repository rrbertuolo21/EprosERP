/**
 * Composable local da fatia 7 (Emissão NF-e).
 *
 * Centraliza estado + IO da tela `pages/erp/vendas/emissao/nfe/[[id]].vue`:
 * carregar venda existente, salvar rascunho fiscal, transmitir/cancelar NF-e,
 * registrar carta de correção, referenciar notas, gerar/baixar DANFE e acompanhar a
 * transmissão em tempo real (SignalR via `useRealtime`).
 *
 * IO exclusivamente pelo cliente compartilhado `useApi` (padrão CommandResult).
 * Endpoints: `vendas`, `vendas-fiscal/{id}/nfe(+transmitir/cancelar/carta-correcao/referenciadas)`,
 * `cfops`, `tipos-operacoes-fiscais`.
 *
 * Observação de porte: no legado o cálculo de impostos/totais era feito no servidor (hub
 * `hubs/venda`). Como o backend novo expõe só REST, os totais são calculados no cliente
 * (`calcularTotais`) a partir dos itens + ajustes manuais. As alíquotas por item são as
 * informadas na linha do produto.
 */
import { computed, reactive, ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useRealtime } from '~/composables/useRealtime'
import { useTenant } from '~/composables/useTenant'
import type { SelectOption } from '~/composables/useEnum'
import {
  criarNfeFormInicial,
  type NfeForm,
  type NfeItem,
  type NfeTotais
} from './nfeTypes'

/** Etapas exibidas no overlay de transmissão (porta `getNfeSaidaSteps` do legado). */
export const PASSOS_TRANSMISSAO = [
  { text: 'Validando dados da nota fiscal...' },
  { text: 'Transmitindo para a SEFAZ...' },
  { text: 'Gerando contas a receber...' },
  { text: 'Baixando estoque...' },
  { text: 'Processando pagamentos...' },
  { text: 'Finalizando operação...' }
]

export function useNfeEmissao() {
  const toast = useToast()
  const { empresaId } = useTenant()
  const realtime = useRealtime('/hubs/venda')

  // --- Estado principal ---
  const nfe = reactive<NfeForm>(criarNfeFormInicial())
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

  // Listas de apoio
  const cfopsOpcoes = ref<SelectOption[]>([])
  const tiposOperacaoOpcoes = ref<SelectOption[]>([])

  const ehEdicao = computed(() => !!nfe.id)
  const podeTransmitir = computed(() => nfe.situacao !== 'Autorizada' && nfe.situacao !== 'Cancelada')
  const emitida = computed(() => nfe.situacao === 'Autorizada')

  // --- Cálculo de totais (client-side) ---
  const totais = computed<NfeTotais>(() => calcularTotais(nfe))

  function recalcularItem(item: NfeItem): void {
    const bruto = (item.quantidade || 0) * (item.valorUnitario || 0)
    item.total = Math.max(0, arred4(bruto - (item.descontoValor || 0)))
  }

  function calcularTotais(form: NfeForm): NfeTotais {
    let valorProduto = 0
    let valorDesconto = 0
    let valorIcms = 0
    let valorIpi = 0
    for (const item of form.itens) {
      const bruto = (item.quantidade || 0) * (item.valorUnitario || 0)
      // Arredonda o item a 4 casas antes de agregar (paridade com o legado, que usava
      // `Math.round((base-desconto+EPSILON)*10000)/10000` em `useNfeProdutos.ts:157`).
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
    const valorRecebimento = form.pagamentos.reduce((acc, p) => acc + (p.valorPagamento || 0), 0)
    return {
      valorProduto: arred(valorProduto),
      valorDesconto: arred(valorDesconto),
      valorFrete: arred(valorFrete),
      valorSeguro: arred(valorSeguro),
      valorOutro: arred(valorOutro),
      valorIcms: arred(valorIcms),
      valorIpi: arred(valorIpi),
      valorNotaFiscal: arred(Math.max(0, valorNotaFiscal)),
      valorRecebimento: arred(valorRecebimento)
    }
  }

  // --- Listas de apoio (CFOP / Tipo de Operação Fiscal) ---
  async function carregarListasApoio(): Promise<void> {
    try {
      const resp = await useApi('/cfops', { query: { tamanhoPagina: 500 } })
      const itens = extrairLista<{ id: number; cfop?: string | number; descricao?: string }>(resp) ?? []
      cfopsOpcoes.value = itens.map((c) => ({
        label: c.cfop ? `${c.cfop} - ${c.descricao ?? ''}`.trim() : c.descricao ?? String(c.id),
        value: c.id
      }))
    } catch (e) {
      console.error('[useNfeEmissao] falha ao carregar CFOPs', e)
    }

    try {
      const resp = await useApi('/tipos-operacoes-fiscais', { query: { tamanhoPagina: 200 } })
      const itens = extrairLista<{ id: number; descricao?: string; nome?: string }>(resp) ?? []
      tiposOperacaoOpcoes.value = itens.map((t) => ({
        label: t.descricao ?? t.nome ?? String(t.id),
        value: t.id
      }))
    } catch (e) {
      console.error('[useNfeEmissao] falha ao carregar tipos de operação fiscal', e)
    }
  }

  // --- Carregar venda existente ---
  async function carregarVenda(id: string): Promise<void> {
    carregando.value = true
    try {
      const resp = await useApi(`/vendas-fiscal/{id}`, { params: { id } })
      const dados = extrairDados<Record<string, unknown>>(resp)
      if (!dados) {
        toast.error('Venda fiscal não encontrada')
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
    const base = criarNfeFormInicial()
    const dest = (dados.destinatario as Record<string, unknown>) ?? {}
    const nfeApi = (dados.nfe as Record<string, unknown>) ?? {}
    Object.assign(nfe, {
      ...base,
      id: (dados.id as string) ?? null,
      numero: (nfeApi.numero as string) ?? null,
      serie: (nfeApi.serie as string) ?? null,
      chave: (nfeApi.chave as string) ?? null,
      situacao: (dados.situacao as string) ?? (nfeApi.situacao as string) ?? null,
      naturezaOperacao: (dados.naturezaOperacao as string) ?? '',
      tipoOperacaoFiscalId: (dados.tipoOperacaoFiscalId as number) ?? null,
      modalidadeFrete: (dados.modalidadeFrete as number) ?? 9,
      tipoAtendimento: (dados.tipoAtendimento as number) ?? 0,
      dataEmissao: (dados.dataEmissao as string) ?? base.dataEmissao,
      dataHoraSaida: (dados.dataHoraSaida as string) ?? base.dataHoraSaida,
      informacoesComplementares: (dados.informacoesComplementares as string) ?? '',
      informacoesAdicionaisFisco: (dados.informacoesAdicionaisFisco as string) ?? '',
      destinatario: {
        ...base.destinatario,
        pessoaId: (dest.pessoaId as number) ?? null,
        nome: (dest.nome as string) ?? (dest.razaoSocial as string) ?? '',
        documento: (dest.documento as string) ?? '',
        enderecoFormatado: (dest.enderecoFormatado as string) ?? ''
      },
      itens: mapearItens(dados.itens as unknown[]),
      duplicatas: mapearDuplicatas((dados.fatura as Record<string, unknown>)?.duplicatas as unknown[]),
      pagamentos: mapearPagamentos(dados.pagamentos as unknown[]),
      chavesReferenciadas: (dados.chavesReferenciadas as string[]) ?? [],
      cartasCorrecao: (dados.cartasCorrecao as NfeForm['cartasCorrecao']) ?? []
    })
  }

  function mapearItens(lista: unknown[] | undefined): NfeItem[] {
    if (!Array.isArray(lista)) return []
    return lista.map((raw) => {
      const i = raw as Record<string, unknown>
      const item: NfeItem = {
        _uid: `l-${Math.random().toString(36).slice(2, 10)}`,
        produtoId: (i.produtoId as number) ?? null,
        codigoProduto: (i.codigoProduto as string) ?? '',
        nomeProduto: (i.nomeProduto as string) ?? (i.descricao as string) ?? '',
        ncm: (i.ncm as string) ?? '',
        cfop: (i.cfop as number) ?? null,
        csosnCst: (i.csosnCst as string) ?? (i.cst as string) ?? '',
        unidade: (i.unidade as string) ?? (i.unidadeComercial as string) ?? 'UN',
        quantidade: (i.quantidade as number) ?? (i.quantidadeComercial as number) ?? 0,
        valorUnitario: (i.valorUnitario as number) ?? (i.valorUnitarioComercial as number) ?? 0,
        descontoValor: (i.descontoValor as number) ?? (i.valorDesconto as number) ?? 0,
        aliquotaIcms: (i.aliquotaIcms as number) ?? 0,
        aliquotaIpi: (i.aliquotaIpi as number) ?? 0,
        total: 0,
        informacoesAdicionais: (i.informacoesAdicionaisDoProduto as string) ?? ''
      }
      recalcularItem(item)
      return item
    })
  }

  function mapearDuplicatas(lista: unknown[] | undefined): NfeForm['duplicatas'] {
    if (!Array.isArray(lista)) return []
    return lista.map((raw) => {
      const d = raw as Record<string, unknown>
      return {
        _uid: `d-${Math.random().toString(36).slice(2, 10)}`,
        numero: (d.numeroDuplicata as string) ?? (d.numero as string) ?? '',
        dataVencimento: (d.dataVencimento as string) ?? '',
        valor: (d.valorDuplicata as number) ?? (d.valor as number) ?? 0
      }
    })
  }

  function mapearPagamentos(lista: unknown[] | undefined): NfeForm['pagamentos'] {
    if (!Array.isArray(lista)) return []
    return lista.map((raw) => {
      const p = raw as Record<string, unknown>
      return {
        _uid: `p-${Math.random().toString(36).slice(2, 10)}`,
        tipoPagamento: (p.tipoPagamento as number) ?? 1,
        valorPagamento: (p.valorPagamento as number) ?? 0,
        valorTroco: (p.valorTroco as number) ?? 0
      }
    })
  }

  // --- Montagem do payload para a API ---
  function montarPayloadVenda(): Record<string, unknown> {
    const t = totais.value
    return {
      empresaId: empresaId.value,
      naturezaOperacao: nfe.naturezaOperacao || null,
      tipoOperacaoFiscalId: nfe.tipoOperacaoFiscalId,
      modalidadeFrete: nfe.modalidadeFrete,
      tipoAtendimento: nfe.tipoAtendimento,
      dataEmissao: nfe.dataEmissao,
      dataHoraSaida: nfe.dataHoraSaida,
      informacoesComplementares: nfe.informacoesComplementares || null,
      informacoesAdicionaisFisco: nfe.informacoesAdicionaisFisco || null,
      destinatarioPessoaId: nfe.destinatario.pessoaId,
      transportadoraPessoaId: nfe.transportadora.pessoaId,
      itens: nfe.itens.map((i) => ({
        produtoId: i.produtoId,
        codigoProduto: i.codigoProduto,
        nomeProduto: i.nomeProduto,
        ncm: i.ncm,
        cfop: i.cfop,
        csosnCst: i.csosnCst,
        unidade: i.unidade,
        quantidade: i.quantidade,
        valorUnitario: i.valorUnitario,
        descontoValor: i.descontoValor,
        aliquotaIcms: i.aliquotaIcms,
        aliquotaIpi: i.aliquotaIpi,
        informacoesAdicionaisDoProduto: i.informacoesAdicionais || null
      })),
      fatura: nfe.duplicatas.length
        ? {
            valorOriginal: t.valorNotaFiscal,
            duplicatas: nfe.duplicatas.map((d) => ({
              numeroDuplicata: d.numero,
              dataVencimento: d.dataVencimento || null,
              valorDuplicata: d.valor
            }))
          }
        : null,
      pagamentos: nfe.pagamentos.map((p) => ({
        tipoPagamento: p.tipoPagamento,
        valorPagamento: p.valorPagamento,
        valorTroco: p.valorTroco
      })),
      chavesReferenciadas: nfe.chavesReferenciadas,
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
    if (!nfe.destinatario.pessoaId) novosErros.destinatario = 'Selecione o destinatário'
    if (!nfe.naturezaOperacao.trim()) novosErros.naturezaOperacao = 'Informe a natureza da operação'
    if (!nfe.dataEmissao) novosErros.dataEmissao = 'Data de emissão é obrigatória'
    if (nfe.itens.length === 0) novosErros.itens = 'Adicione ao menos um produto'
    for (const item of nfe.itens) {
      if (!item.produtoId && !item.nomeProduto.trim()) {
        novosErros.itens = 'Há produtos incompletos na lista'
        break
      }
      if (!item.cfop) {
        novosErros.itens = 'Informe o CFOP de todos os produtos'
        break
      }
    }
    erros.value = novosErros
    return Object.keys(novosErros).length === 0
  }

  // --- Salvar rascunho (cria/atualiza a venda fiscal) ---
  async function salvarRascunho(): Promise<boolean> {
    if (!nfe.destinatario.pessoaId) {
      toast.error('Selecione o destinatário antes de salvar')
      return false
    }
    salvando.value = true
    try {
      const payload = montarPayloadVenda()
      if (nfe.id) {
        await useApi(`/vendas-fiscal/{id}`, { method: 'PUT', params: { id: nfe.id }, body: { id: nfe.id, ...payload } })
      } else {
        const resp = await useApi.post<CommandResult<{ id?: string } | string>>('/vendas-fiscal', payload)
        const dados = extrairDados<{ id?: string } | string>(resp)
        const novoId = typeof dados === 'string' ? dados : dados?.id
        if (novoId) nfe.id = novoId
      }
      toast.success('Venda salva com sucesso')
      return true
    } catch (e) {
      toast.error(obterMensagemErro(e))
      return false
    } finally {
      salvando.value = false
    }
  }

  // --- Transmitir NF-e ---
  async function transmitir(): Promise<void> {
    if (!validar()) {
      toast.error('Preencha os campos obrigatórios antes de transmitir')
      return
    }
    // Garante rascunho persistido (precisa do id para a rota vendas-fiscal/{id}/nfe).
    if (!nfe.id) {
      const ok = await salvarRascunho()
      if (!ok || !nfe.id) return
    } else {
      const ok = await salvarRascunho()
      if (!ok) return
    }

    overlayErro.value = null
    overlayPasso.value = 0
    overlayVisivel.value = true

    // Conecta ao hub para acompanhar o progresso da transmissão em tempo real.
    await realtime.conectar({
      NfTransmissionStep: (...args: unknown[]) => {
        const passo = Number(args[0])
        if (!Number.isNaN(passo)) overlayPasso.value = passo
      }
    })

    try {
      // Inclui a NF-e no agregado fiscal (idempotente no backend).
      await useApi(`/vendas-fiscal/{id}/nfe`, {
        method: 'POST',
        params: { id: nfe.id },
        body: { vendaId: nfe.id }
      })
      // Transmite para a SEFAZ.
      const resp = await useApi<CommandResult<Record<string, unknown>>>(`/vendas-fiscal/{id}/nfe/transmitir`, {
        method: 'POST',
        params: { id: nfe.id },
        body: { vendaId: nfe.id }
      })
      const dados = extrairDados<Record<string, unknown>>(resp)
      overlayPasso.value = PASSOS_TRANSMISSAO.length - 1
      if (dados) {
        nfe.numero = (dados.numero as string) ?? nfe.numero
        nfe.serie = (dados.serie as string) ?? nfe.serie
        nfe.chave = (dados.chave as string) ?? nfe.chave
        nfe.situacao = (dados.situacao as string) ?? 'Autorizada'
      } else {
        nfe.situacao = 'Autorizada'
      }
      toast.success('NF-e transmitida e autorizada com sucesso')
      setTimeout(() => (overlayVisivel.value = false), 800)
    } catch (e) {
      overlayErro.value = obterMensagemErro(e)
    } finally {
      await realtime.desconectar()
    }
  }

  // --- Cancelar NF-e (após autorizada) ---
  async function cancelarNfe(justificativa: string): Promise<boolean> {
    if (!nfe.id) return false
    salvando.value = true
    try {
      await useApi(`/vendas-fiscal/{id}/nfe/cancelar`, {
        method: 'POST',
        params: { id: nfe.id },
        body: { vendaId: nfe.id, justificativa }
      })
      nfe.situacao = 'Cancelada'
      toast.success('NF-e cancelada com sucesso')
      return true
    } catch (e) {
      toast.error(obterMensagemErro(e))
      return false
    } finally {
      salvando.value = false
    }
  }

  // --- Carta de correção ---
  async function registrarCartaCorrecao(texto: string): Promise<boolean> {
    if (!nfe.id) return false
    salvando.value = true
    try {
      await useApi(`/vendas-fiscal/{id}/nfe/carta-correcao`, {
        method: 'POST',
        params: { id: nfe.id },
        body: { vendaId: nfe.id, textoCorrecao: texto }
      })
      nfe.cartasCorrecao.push({
        sequencia: nfe.cartasCorrecao.length + 1,
        texto,
        dataRegistro: new Date().toISOString()
      })
      toast.success('Carta de correção registrada com sucesso')
      return true
    } catch (e) {
      toast.error(obterMensagemErro(e))
      return false
    } finally {
      salvando.value = false
    }
  }

  // --- Notas referenciadas ---
  async function salvarReferenciadas(chaves: string[]): Promise<void> {
    nfe.chavesReferenciadas = [...chaves]
    // Persiste apenas quando já existe venda fiscal (id).
    if (!nfe.id) return
    try {
      await useApi(`/vendas-fiscal/{id}/nfe/referenciadas`, {
        method: 'POST',
        params: { id: nfe.id },
        body: { vendaId: nfe.id, chaves }
      })
    } catch (e) {
      console.error('[useNfeEmissao] falha ao salvar notas referenciadas', e)
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
      const blob = await useApi<Blob>(`/vendas-fiscal/{id}/nfe/danfe`, {
        params: { id: nfe.id },
        responseType: 'blob'
      })
      danfeSrc.value = blob
      danfeVisivel.value = true
    } catch (e) {
      // Endpoint de DANFE pode não existir ainda no backend novo (ver relatório).
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
    a.download = `danfe-${nfe.numero ?? nfe.id ?? 'nfe'}.pdf`
    a.click()
    URL.revokeObjectURL(url)
  }

  function imprimirDanfe(): void {
    if (!(danfeSrc.value instanceof Blob)) return
    const url = URL.createObjectURL(danfeSrc.value)
    window.open(url, '_blank')?.focus()
  }

  // --- Manipulação de itens/pagamentos/duplicatas ---
  function adicionarItem(item: NfeItem): void {
    recalcularItem(item)
    nfe.itens.push(item)
  }

  function atualizarItem(uid: string, item: NfeItem): void {
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

  function limparTela(): void {
    Object.assign(nfe, criarNfeFormInicial())
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
    podeTransmitir,
    emitida,
    // overlay
    overlayVisivel,
    overlayPasso,
    overlayErro,
    passosTransmissao: PASSOS_TRANSMISSAO,
    // danfe
    danfeSrc,
    danfeVisivel,
    // apoio
    cfopsOpcoes,
    tiposOperacaoOpcoes,
    // ações
    carregarListasApoio,
    carregarVenda,
    salvarRascunho,
    transmitir,
    cancelarNfe,
    registrarCartaCorrecao,
    salvarReferenciadas,
    gerarDanfe,
    baixarDanfe,
    imprimirDanfe,
    adicionarItem,
    atualizarItem,
    removerItem,
    recalcularItem,
    limparTela
  }
}

/** Arredonda para 2 casas decimais evitando ruído de ponto flutuante. */
function arred(v: number): number {
  return Math.round((v + Number.EPSILON) * 100) / 100
}

/**
 * Arredonda para 4 casas decimais (gap L7 — paridade com o legado
 * `useNfeProdutos.ts:157`, que usa 4 casas no item antes de somar os totais).
 */
function arred4(v: number): number {
  return Math.round((v + Number.EPSILON) * 10000) / 10000
}
