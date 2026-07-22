/**
 * Composable local da fatia 11 — Inutilização de numeração.
 *
 * Porta `vendas/inutilizacao-numeracao.vue` do legado: envio de faixa de numeração
 * (série + nº inicial/final + justificativa) para inutilização na SEFAZ.
 *
 * No legado a série era pré-preenchida a partir de `authStore.data.empresaParametrosDfe`;
 * aqui a série é editável (o backend novo valida contra os parâmetros DFe da empresa) e,
 * quando os parâmetros estiverem disponíveis na sessão, sugerimos a série corrente.
 *
 * IO exclusivamente pelo cliente compartilhado `useApi` (padrão CommandResult).
 *
 * Envio real via `POST inutilizacao-dfe/inutilizar` (`InutilizacaoDfeController`,
 * `InutilizarFaixaFiscalCommand`: ModeloDocumento/Serie/NrNfInicial/NrNfFinal/Justificativa).
 *
 * Listagem ligada: `GET inutilizacao-dfe` (paginado {Total,Pagina,Itens}) alimenta
 * `carregarLista`; o envio via `POST inutilizacao-dfe/inutilizar` também é real.
 */
import { reactive, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import type { InutilizacaoBody, InutilizacaoItem } from './transmissoesTypes'

/** Tamanho mínimo da justificativa exigido pela SEFAZ. */
export const JUSTIFICATIVA_MIN = 15

export function useInutilizacao() {
  const toast = useToast()

  const enviando = ref(false)
  const carregandoLista = ref(false)
  const itens = ref<InutilizacaoItem[]>([])
  const erros = ref<Record<string, string>>({})

  const body = reactive<InutilizacaoBody>({
    modeloDocumento: '65',
    serie: null,
    nrNfInicial: null,
    nrNfFinal: null,
    justificativa: ''
  })

  /** Valida o formulário; devolve true quando apto para envio. */
  function validar(): boolean {
    const novos: Record<string, string> = {}
    if (!body.modeloDocumento) novos.modeloDocumento = 'Selecione o modelo do documento'
    if (!body.serie) novos.serie = 'Informe a série'
    if (!body.nrNfInicial) novos.nrNfInicial = 'Informe o número inicial'
    if (!body.nrNfFinal) novos.nrNfFinal = 'Informe o número final'
    if (body.nrNfInicial && body.nrNfFinal && Number(body.nrNfFinal) < Number(body.nrNfInicial)) {
      novos.nrNfFinal = 'O número final deve ser maior ou igual ao inicial'
    }
    if ((body.justificativa?.trim().length ?? 0) < JUSTIFICATIVA_MIN) {
      novos.justificativa = `A justificativa deve ter no mínimo ${JUSTIFICATIVA_MIN} caracteres`
    }
    erros.value = novos
    return Object.keys(novos).length === 0
  }

  /** Carrega a lista de inutilizações já realizadas (`GET inutilizacao-dfe`, paginado). */
  async function carregarLista(): Promise<void> {
    carregandoLista.value = true
    try {
      const resp = await useApi('/inutilizacao-dfe', {
        query: {
          modeloDocumento: body.modeloDocumento ? Number(body.modeloDocumento) : undefined,
          pagina: 1,
          tamanhoPagina: 100
        }
      })
      const dados = extrairDados<
        InutilizacaoItem[] | { itens?: InutilizacaoItem[]; Itens?: InutilizacaoItem[] }
      >(resp)
      const lista: InutilizacaoItem[] = Array.isArray(dados)
        ? dados
        : (dados?.itens ?? dados?.Itens ?? [])
      // Mais recentes primeiro (o legado fazia reverse()).
      itens.value = [...lista].reverse()
    } catch (e) {
      console.error('[useInutilizacao] falha ao carregar inutilizações', e)
      itens.value = []
    } finally {
      carregandoLista.value = false
    }
  }

  /** Envia a faixa para inutilização (`POST inutilizacao-dfe/inutilizar`). */
  async function inutilizar(): Promise<boolean> {
    if (!validar()) {
      toast.error('Preencha os campos obrigatórios corretamente')
      return false
    }
    enviando.value = true
    try {
      await useApi('/inutilizacao-dfe/inutilizar', {
        method: 'POST',
        body: {
          modeloDocumento: Number(body.modeloDocumento),
          serie: Number(body.serie),
          nrNfInicial: Number(body.nrNfInicial),
          nrNfFinal: Number(body.nrNfFinal),
          justificativa: body.justificativa.trim()
        }
      })
      toast.success('Numeração inutilizada com sucesso')
      // Limpa a faixa mantendo modelo/série para novo envio.
      body.nrNfInicial = null
      body.nrNfFinal = null
      body.justificativa = ''
      erros.value = {}
      await carregarLista()
      return true
    } catch (e) {
      toast.error(obterMensagemErro(e))
      return false
    } finally {
      enviando.value = false
    }
  }

  return {
    body,
    erros,
    itens,
    enviando,
    carregandoLista,
    validar,
    carregarLista,
    inutilizar
  }
}
