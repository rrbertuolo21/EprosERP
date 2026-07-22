<script setup lang="ts">
/**
 * Listagem de NCM — Fiscal / NCM.
 *
 * Porta o comportamento de `fiscal/ncm.vue` do legado: tabela paginada server-side com
 * busca por código/descrição. O legado só listava (a tabela NCM é uma base de referência
 * carregada via upload); aqui o backend expõe CRUD completo (`fiscal/ncms`), então a tela
 * ganha criação/edição/exclusão via modal para permitir cadastro manual de códigos NCM.
 * Endpoint: `fiscal/ncms`.
 */
import { ref, reactive, onMounted } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ layout: 'default' })

interface Ncm {
  id: string
  codigoNcm: string
  descricao: string
  dataInicio: string
  dataFim?: string | null
}

interface NcmFiltros {
  localizar?: string
}

interface NcmForm {
  id?: string
  codigoNcm: string
  descricao: string
  dataInicio: string | null
  dataFim: string | null
  tipoAtoIni: string | null
  numeroAtoIni: string | null
  anoAtoIni: string | null
}

const toast = useToast()

const lista = useApiList<Ncm, NcmFiltros>('/fiscal/ncms', {
  filtrosIniciais: { localizar: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Ncm>[] = [
  { key: 'codigoNcm', label: 'Código', sortable: true, width: '140px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'dataInicio', label: 'Início Vigência', sortable: true, width: '150px' },
  { key: 'dataFim', label: 'Fim Vigência', sortable: false, width: '150px' }
]

const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'Código ou descrição...', grow: true }
]

async function buscarPagina() {
  await lista.buscar()
}

function formatarData(v: string | null | undefined): string {
  if (!v) return '-'
  const d = new Date(v)
  if (isNaN(d.getTime())) return String(v)
  return d.toLocaleDateString('pt-BR')
}

// ---- CRUD via modal ----
const dialogVisivel = ref(false)
const salvando = ref(false)
const editando = ref(false)
const erros = reactive<Record<string, string>>({})

const form = reactive<NcmForm>({
  codigoNcm: '',
  descricao: '',
  dataInicio: null,
  dataFim: null,
  tipoAtoIni: null,
  numeroAtoIni: null,
  anoAtoIni: null
})

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function resetForm() {
  form.id = undefined
  form.codigoNcm = ''
  form.descricao = ''
  form.dataInicio = new Date().toISOString().slice(0, 10)
  form.dataFim = null
  form.tipoAtoIni = null
  form.numeroAtoIni = null
  form.anoAtoIni = null
  limparErros()
}

function abrirNovo() {
  editando.value = false
  resetForm()
  dialogVisivel.value = true
}

function abrirEdicao(item: Ncm) {
  editando.value = true
  limparErros()
  form.id = item.id
  form.codigoNcm = item.codigoNcm
  form.descricao = item.descricao
  form.dataInicio = item.dataInicio ? item.dataInicio.slice(0, 10) : null
  form.dataFim = item.dataFim ? item.dataFim.slice(0, 10) : null
  form.tipoAtoIni = null
  form.numeroAtoIni = null
  form.anoAtoIni = null
  dialogVisivel.value = true
}

function validar(): boolean {
  limparErros()
  if (!form.codigoNcm?.trim()) erros.codigoNcm = 'O código NCM é obrigatório.'
  else if (form.codigoNcm.length > 8) erros.codigoNcm = 'O código NCM deve possuir no máximo 8 caracteres.'
  if (form.descricao && form.descricao.length > 1500) erros.descricao = 'A descrição deve possuir no máximo 1500 caracteres.'
  if (!form.dataInicio) erros.dataInicio = 'A data de início é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) return
  salvando.value = true
  try {
    if (editando.value) {
      await useApi(`/fiscal/ncms/${form.id}`, { method: 'PUT', body: form })
    } else {
      await useApi('/fiscal/ncms', { method: 'POST', body: form })
    }
    toast.success('NCM salvo com sucesso!')
    dialogVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<Ncm | null>(null)

function pedirExclusao(item: Ncm) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi(`/fiscal/ncms/${itemParaExcluir.value.id}`, { method: 'DELETE' })
    toast.success('NCM excluído com sucesso.')
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
  void buscarPagina()
})
</script>

<template>
  <div>
    <PageToolbar title="NCM" subtitle="Nomenclatura Comum do Mercosul" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirNovo">+ Novo NCM</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="lista.filtros.value"
      :loading="lista.carregando.value"
      @update:model-value="(v) => (lista.filtros.value = v as typeof lista.filtros.value)"
      @search="lista.aplicarFiltros($event as Partial<typeof lista.filtros.value>)"
      @clear="lista.limpar()"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhum NCM cadastrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="abrirEdicao"
    >
      <template #cell-dataInicio="{ value }">{{ formatarData(value as string) }}</template>
      <template #cell-dataFim="{ value }">{{ formatarData(value as string | null) }}</template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrirEdicao(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <AppDialog v-model="dialogVisivel" :title="editando ? 'Editar NCM' : 'Novo NCM'" width="560px">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigoNcm" label="Código NCM" required maxlength="8" :error="erros.codigoNcm" />
          <DateTimeField v-model="form.dataInicio" label="Início Vigência" required :error="erros.dataInicio" />
          <DateTimeField v-model="form.dataFim" label="Fim Vigência" />
        </div>
        <TextField v-model="form.descricao" label="Descrição" maxlength="1500" :error="erros.descricao" />
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
      :item-label="itemParaExcluir ? `${itemParaExcluir.codigoNcm} - ${itemParaExcluir.descricao}` : undefined"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>

<style scoped>
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 16px;
  margin-bottom: 12px;
}
</style>
