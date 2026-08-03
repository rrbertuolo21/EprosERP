<script setup lang="ts">
/**
 * Grupo de Pessoas — Cadastros / Grupos.
 *
 * Porta o comportamento de `cadastros/grupo/pessoa.vue` do legado:
 * listagem simples (id + descrição) com modal de criação/edição e exclusão.
 * Endpoint: `cadastros/pessoa-grupos`.
 */
import { ref, onMounted, reactive } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ layout: 'default' })

interface PessoaGrupo {
  id: number
  descricao: string
}

const toast = useToast()

const lista = useApiList<PessoaGrupo>('/cadastros/pessoa-grupos', {
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<PessoaGrupo>[] = [
  { key: 'id', label: '#', sortable: true, width: '80px' },
  { key: 'descricao', label: 'Descrição', sortable: true }
]

const dialogVisivel = ref(false)
const salvando = ref(false)
const editando = ref(false)
const form = reactive<PessoaGrupo>({ id: 0, descricao: '' })
const erroDescricao = ref('')

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<PessoaGrupo | null>(null)

function abrirNovo() {
  editando.value = false
  form.id = 0
  form.descricao = ''
  erroDescricao.value = ''
  dialogVisivel.value = true
}

function abrirEdicao(item: PessoaGrupo) {
  editando.value = true
  form.id = item.id
  form.descricao = item.descricao
  erroDescricao.value = ''
  dialogVisivel.value = true
}

async function salvar() {
  if (!form.descricao?.trim()) {
    erroDescricao.value = 'Descrição é obrigatória.'
    return
  }
  erroDescricao.value = ''
  salvando.value = true
  try {
    if (editando.value) {
      await useApi(`/cadastros/pessoa-grupos/${form.id}`, { method: 'PUT', body: { id: form.id, descricao: form.descricao } })
    } else {
      await useApi('/cadastros/pessoa-grupos', { method: 'POST', body: { descricao: form.descricao } })
    }
    toast.success('Registro salvo com sucesso!')
    dialogVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function pedirExclusao(item: PessoaGrupo) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi(`/cadastros/pessoa-grupos/${itemParaExcluir.value.id}`, { method: 'DELETE' })
    toast.success('Grupo excluído com sucesso.')
    excluirVisivel.value = false
    itemParaExcluir.value = null
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Grupo de Pessoas" subtitle="Agrupamentos usados na classificação de clientes e fornecedores" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirNovo">+ Adicionar</button>
      </template>
    </PageToolbar>

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhum grupo de pessoas cadastrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="abrirEdicao"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrirEdicao(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <AppDialog v-model="dialogVisivel" :title="editando ? 'Editar Grupo' : 'Novo Grupo'" width="480px">
      <form class="vertical-form" @submit.prevent="salvar">
        <TextField v-model="form.descricao" label="Descrição" required :error="erroDescricao" />
      </form>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="dialogVisivel = false">Fechar</button>
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
