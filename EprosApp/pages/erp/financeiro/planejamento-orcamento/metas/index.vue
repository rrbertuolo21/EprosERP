<script setup lang="ts">
/**
 * Listagem de Metas — Planejamento/Orçamento.
 * GET /planejamento-orcamento/metas, POST. Categorias: POST /metas/categorias (diálogo).
 * Detalhe da meta com ações em /{id}.
 */
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import { OPCOES_ESCOPO_META } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface Meta {
  id: string
  categoriaNome?: string | null
  tipo?: string | null
  prioridade?: string | null
  dataInicio?: string | null
  dataAlvo?: string | null
  statusDescricao?: string | null
}

const router = useRouter()
const toast = useToast()
const { formatarData } = useHelper()

const lista = useApiList<Meta>('/planejamento-orcamento/metas', { tamanhoPaginaInicial: 25 })

const colunas: DataTableColumn<Meta>[] = [
  { key: 'categoriaNome', label: 'Categoria', sortable: false },
  { key: 'tipo', label: 'Tipo', sortable: false },
  { key: 'prioridade', label: 'Prioridade', sortable: false },
  { key: 'dataAlvo', label: 'Data Alvo', sortable: true, formatter: (v) => (v ? formatarData(v as string) : '') },
  { key: 'statusDescricao', label: 'Status', sortable: false, align: 'center' }
]

function nova() {
  router.push('/erp/financeiro/planejamento-orcamento/metas/novo')
}
function abrir(item: Meta) {
  router.push(`/erp/financeiro/planejamento-orcamento/metas/${item.id}`)
}

// --- Nova categoria
const categoriaVisivel = ref(false)
const salvandoCategoria = ref(false)
const categoria = reactive<{ nome: string | null; codigo: string | null; escopo: number | null }>({ nome: null, codigo: null, escopo: 0 })

async function salvarCategoria() {
  if (!categoria.nome) {
    toast.error('Informe o nome da categoria.')
    return
  }
  salvandoCategoria.value = true
  try {
    await useApi('/planejamento-orcamento/metas/categorias', {
      method: 'POST',
      body: { nome: categoria.nome, codigo: categoria.codigo, escopo: categoria.escopo }
    })
    toast.success('Categoria criada.')
    categoriaVisivel.value = false
    categoria.nome = null
    categoria.codigo = null
    categoria.escopo = 0
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoCategoria.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Metas" subtitle="Metas financeiras e acompanhamento" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="categoriaVisivel = true">Nova categoria</button>
        <button type="button" class="btn btn-primary" @click="nova">+ Nova meta</button>
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
      empty-text="Nenhuma meta cadastrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="abrir"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Abrir" @click.stop="abrir(row)">Abrir</button>
      </template>
    </DataTable>

    <AppDialog v-model="categoriaVisivel" title="Nova categoria de meta" width="440px">
      <div class="form-grid-modal">
        <TextField v-model="categoria.nome" label="Nome" maxlength="80" />
        <TextField v-model="categoria.codigo" label="Código" maxlength="30" />
        <SelectField v-model="categoria.escopo" label="Escopo" :options="OPCOES_ESCOPO_META" :clearable="false" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="categoriaVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoCategoria" @click="salvarCategoria">
          <span v-if="salvandoCategoria" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-grid-modal { display: grid; grid-template-columns: 1fr; gap: 14px; }
</style>
