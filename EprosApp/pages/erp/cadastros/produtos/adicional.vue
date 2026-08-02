<script setup lang="ts">
/**
 * Cadastro de Adicionais (itens complementares de produto, ex.: PDV/hortifruti).
 * Porta o comportamento de `cadastros/produto/adicional.vue` do legado.
 *
 * Endpoint: adicionais (api/v1/adicionais)
 */
import { ref, reactive, onMounted } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

interface Adicional extends Record<string, unknown> {
  id: number
  descricao: string
  valorPreco: number
}

interface AdicionalForm {
  id?: number
  descricao: string
  valorPreco: number | null
}

const toast = useToast()
const { formatarMoeda } = useHelper()

const lista = useApiList<Adicional>('/adicionais', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<Adicional>[] = [
  { key: 'id', label: '#', width: '70px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'valorPreco', label: 'Valor', align: 'right', width: '140px', formatter: (v) => formatarMoeda(v as number) }
]

const dialogAberto = ref(false)
const salvando = ref(false)
const erros = reactive<{ descricao: string; valorPreco: string }>({ descricao: '', valorPreco: '' })
const form = reactive<AdicionalForm>({ descricao: '', valorPreco: 0 })
const editando = ref(false)

function limparErros() {
  erros.descricao = ''
  erros.valorPreco = ''
}

function abrirNovo() {
  editando.value = false
  form.id = undefined
  form.descricao = ''
  form.valorPreco = 0
  limparErros()
  dialogAberto.value = true
}

function abrirEdicao(item: Adicional) {
  editando.value = true
  form.id = item.id
  form.descricao = item.descricao
  form.valorPreco = item.valorPreco
  limparErros()
  dialogAberto.value = true
}

function validar(): boolean {
  limparErros()
  let ok = true
  if (!form.descricao.trim()) {
    erros.descricao = 'Informe a descrição'
    ok = false
  }
  if (form.valorPreco == null) {
    erros.valorPreco = 'Informe o valor'
    ok = false
  }
  return ok
}

async function salvar() {
  if (!validar()) return
  salvando.value = true
  try {
    if (editando.value && form.id) {
      await useApi.put(`/adicionais/{id}`, { ...form }, { params: { id: form.id } })
    } else {
      await useApi.post('/adicionais', { descricao: form.descricao, valorPreco: form.valorPreco })
    }
    toast.success('Adicional salvo com sucesso')
    dialogAberto.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

const itemParaExcluir = ref<Adicional | null>(null)
const excluindo = ref(false)

function solicitarExclusao(item: Adicional) {
  itemParaExcluir.value = item
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi.delete(`/adicionais/{id}`, { params: { id: itemParaExcluir.value.id } })
    toast.success('Adicional excluído com sucesso')
    itemParaExcluir.value = null
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

onMounted(() => lista.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Cadastro de Adicionais" subtitle="Itens complementares de produto" :loading="lista.carregando.value">
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
      empty-text="Nenhum adicional cadastrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click="abrirEdicao(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="solicitarExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <AppDialog v-model="dialogAberto" :title="editando ? `Editando ${form.descricao}` : 'Novo Adicional'" width="480px">
      <form class="form-grid" @submit.prevent="salvar">
        <TextField v-model="form.descricao" label="Descrição" required :error="erros.descricao" />
        <MoneyInput v-model="form.valorPreco" label="Valor" required :error="erros.valorPreco" />
      </form>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="dialogAberto = false">Fechar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>

    <DeleteAlert
      :model-value="!!itemParaExcluir"
      :item-label="itemParaExcluir?.descricao"
      :loading="excluindo"
      @update:model-value="itemParaExcluir = null"
      @confirm="confirmarExclusao"
    />
  </div>
</template>

<style scoped>
.form-grid { display: flex; flex-direction: column; gap: 12px; }
</style>
