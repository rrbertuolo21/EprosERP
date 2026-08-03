<script setup lang="ts">
/**
 * Bancos — Financeiro / Cadastros auxiliares.
 *
 * Porta o comportamento de `financeiro/bancos.vue` do legado (que estava incompleto/comentado):
 * listagem paginada + CRUD (criar/editar/excluir) em modal, já que o backend novo expõe
 * o CRUD completo de `/bancos` (o legado só tinha a listagem funcional).
 */
import { ref, reactive, onMounted } from 'vue'
import { obterMensagemErro } from '~/composables/useApiList'
import { useApi, extrairDados } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ layout: 'default' })

interface Banco {
  id: string
  codigo: string
  descricao: string
}

interface BancoForm {
  id: string | null
  codigo: string
  descricao: string
}

/**
 * A rota `/bancos` (como as demais deste módulo Financeiro) devolve a paginação
 * dentro de `dados = { total, pagina, itens }`, fora do padrão `useApiList`
 * (que espera `dados` como array e `totalRegistros` no topo do envelope).
 * Por isso a listagem é feita manualmente aqui, mantendo o mesmo par DataTable+FilterBar.
 */
const toast = useToast()

const itens = ref<Banco[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(25)
const carregando = ref(false)
const filtros = reactive<{ localizar: string }>({ localizar: '' })

const colunas: DataTableColumn<Banco>[] = [
  { key: 'codigo', label: 'Código', sortable: false, width: '140px' },
  { key: 'descricao', label: 'Descrição', sortable: false }
]

const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'Código ou descrição...', grow: true }
]

async function buscar() {
  carregando.value = true
  try {
    const resposta = await useApi<{ dados?: { total: number; itens: Banco[] } }>('/bancos', {
      query: { localizar: filtros.localizar || undefined, pagina: pagina.value, tamanhoPagina: tamanhoPagina.value }
    })
    const dados = extrairDados<{ total: number; itens: Banco[] }>(resposta)
    itens.value = dados?.itens ?? []
    total.value = dados?.total ?? 0
  } catch (e) {
    toast.error(obterMensagemErro(e))
    itens.value = []
    total.value = 0
  } finally {
    carregando.value = false
  }
}

function aplicarFiltros(v: Record<string, unknown>) {
  filtros.localizar = (v.localizar as string) ?? ''
  pagina.value = 1
  void buscar()
}

function limparFiltros() {
  filtros.localizar = ''
  pagina.value = 1
  void buscar()
}

function irParaPagina(p: number) {
  pagina.value = p
  void buscar()
}

function mudarTamanhoPagina(tam: number) {
  tamanhoPagina.value = tam
  pagina.value = 1
  void buscar()
}

const dialogVisivel = ref(false)
const salvando = ref(false)
const formErros = reactive<Record<string, string>>({})
const form = reactive<BancoForm>({ id: null, codigo: '', descricao: '' })

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<Banco | null>(null)

function limparForm() {
  form.id = null
  form.codigo = ''
  form.descricao = ''
  for (const k of Object.keys(formErros)) delete formErros[k]
}

function novoBanco() {
  limparForm()
  dialogVisivel.value = true
}

function editarBanco(item: Banco) {
  form.id = item.id
  form.codigo = item.codigo
  form.descricao = item.descricao
  for (const k of Object.keys(formErros)) delete formErros[k]
  dialogVisivel.value = true
}

function validar(): boolean {
  for (const k of Object.keys(formErros)) delete formErros[k]
  if (!form.codigo) formErros.codigo = 'Código é obrigatório.'
  if (!form.descricao) formErros.descricao = 'Descrição é obrigatória.'
  return Object.keys(formErros).length === 0
}

async function salvar() {
  if (!validar()) return
  salvando.value = true
  try {
    if (form.id) {
      await useApi(`/bancos/{id}`, {
        method: 'PUT',
        params: { id: form.id },
        body: { id: form.id, codigo: form.codigo, descricao: form.descricao }
      })
    } else {
      await useApi('/bancos', { method: 'POST', body: { codigo: form.codigo, descricao: form.descricao } })
    }
    toast.success('Banco salvo com sucesso!')
    dialogVisivel.value = false
    await buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function pedirExclusao(item: Banco) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi('/bancos/{id}', { method: 'DELETE', params: { id: itemParaExcluir.value.id } })
    toast.success('Banco excluído com sucesso.')
    excluirVisivel.value = false
    itemParaExcluir.value = null
    await buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

onMounted(() => {
  void buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Bancos" subtitle="Cadastro de instituições financeiras (FEBRABAN)" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novoBanco">+ Novo banco</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="filtros"
      :loading="carregando"
      @update:model-value="(v) => (filtros.localizar = (v.localizar as string) ?? '')"
      @search="aplicarFiltros"
      @clear="limparFiltros"
    />

    <DataTable
      :items="itens"
      :columns="colunas"
      :total="total"
      :page="pagina"
      :page-size="tamanhoPagina"
      :loading="carregando"
      empty-text="Nenhum banco encontrado."
      @update:page="irParaPagina"
      @update:page-size="mudarTamanhoPagina"
      @row-click="editarBanco"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editarBanco(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Excluir" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <AppDialog v-model="dialogVisivel" :title="form.id ? 'Editar banco' : 'Novo banco'" width="480px">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" required maxlength="10" :error="formErros.codigo" />
          <TextField v-model="form.descricao" label="Descrição" required maxlength="150" :error="formErros.descricao" />
        </div>
      </form>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="dialogVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.descricao"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>

<style scoped>
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 16px;
}
</style>
