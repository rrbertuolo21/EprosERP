/**
 * useGrupoPessoas — IO de grupos de pessoas (fatia GestaoClientes).
 *
 * Rotas reais (PessoaGruposController): `GET/POST /api/v1/cadastros/pessoa-grupos`,
 * `GET/PUT/DELETE /api/v1/cadastros/pessoa-grupos/{id}`.
 * Observação: o `Listar` deste controller não é paginado (retorna array direto), diferente
 * do padrão CommandResult paginado; `useApiList` já lida com ambos os formatos.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export interface PessoaGrupo {
  id: string
  descricao: string
}

export interface PessoaGrupoCreateDto {
  descricao: string
}

export interface PessoaGrupoUpdateDto extends PessoaGrupoCreateDto {
  id: string
}

export function useGrupoPessoas() {
  const pessoaGrupo = ref<PessoaGrupo | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<PessoaGrupo, Record<string, unknown>>('/cadastros/pessoa-grupos')

  async function buscarPorId(id: string): Promise<PessoaGrupo | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<PessoaGrupo>>('/cadastros/pessoa-grupos/{id}', { params: { id } })
      const dados = extrairDados<PessoaGrupo>(resposta) ?? null
      pessoaGrupo.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useGrupoPessoas.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: PessoaGrupoCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/cadastros/pessoa-grupos', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useGrupoPessoas.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: PessoaGrupoUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/cadastros/pessoa-grupos/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useGrupoPessoas.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluir(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/cadastros/pessoa-grupos/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useGrupoPessoas.excluir]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    pessoaGrupo,
    carregando,
    erro,
    itens: lista.itens,
    total: lista.total,
    buscar: lista.buscar,
    filtros: lista.filtros,
    buscarPorId,
    criar,
    atualizar,
    excluir
  }
}
