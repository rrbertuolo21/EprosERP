<script setup lang="ts">
/**
 * Listagem de Contadores — Cadastros / Contadores.
 *
 * Porta o comportamento de `cadastros/contador/index.vue` do legado:
 * busca textual, tabela paginada server-side, editar/excluir.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useMask } from '~/composables/useMask'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'

definePageMeta({ layout: 'default' })

interface Contador {
  id: number
  nomeContador?: string | null
  razaoSocial?: string | null
  cpf?: string | null
  cnpj?: string | null
  email?: string | null
  telefone?: string | null
  ativo: boolean
}

interface ContadorFiltros {
  busca?: string
}

const router = useRouter()
const toast = useToast()
const { maskCPF, maskCpfCnpj, maskTelefone } = useMask()

const lista = useApiList<Contador, ContadorFiltros>('/contadores', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Contador>[] = [
  { key: 'id', label: '#', sortable: true, width: '70px' },
  { key: 'nomeContador', label: 'Nome', sortable: true },
  { key: 'cpf', label: 'CPF', sortable: true },
  { key: 'cnpj', label: 'CNPJ', sortable: true },
  { key: 'email', label: 'E-mail', sortable: true },
  { key: 'telefone', label: 'Telefone', sortable: false },
  { key: 'ativo', label: 'Status', sortable: true, align: 'center', width: '110px' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Nome, CPF, CNPJ ou e-mail...', grow: true }
]

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<Contador | null>(null)

async function buscarPagina() {
  await lista.buscar()
}

function novoContador() {
  router.push('/erp/cadastros/contadores/novo')
}

function editarContador(item: Contador) {
  router.push(`/erp/cadastros/contadores/${item.id}`)
}

function pedirExclusao(item: Contador) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi(`/contadores/${itemParaExcluir.value.id}`, { method: 'DELETE' })
    toast.success('Contador excluído com sucesso.')
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
    <PageToolbar title="Contadores" subtitle="Cadastro de contadores e responsáveis contábeis das empresas" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novoContador">+ Novo contador</button>
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
      empty-text="Nenhum contador encontrado. Adicione um novo contador para começar."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="editarContador"
    >
      <template #cell-cpf="{ value }">
        <span v-if="value">{{ maskCPF(String(value)) }}</span>
      </template>
      <template #cell-cnpj="{ value }">
        <span v-if="value">{{ maskCpfCnpj(String(value)) }}</span>
      </template>
      <template #cell-email="{ value }">
        <a v-if="value" :href="`mailto:${value}`" @click.stop>{{ value }}</a>
      </template>
      <template #cell-telefone="{ value }">
        <span v-if="value">{{ maskTelefone(String(value)) }}</span>
      </template>
      <template #cell-ativo="{ value }">
        <span class="badge" :class="value ? 'badge-success' : 'badge-danger'">
          {{ value ? 'Ativo' : 'Inativo' }}
        </span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editarContador(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Excluir" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.nomeContador || itemParaExcluir?.razaoSocial || undefined"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>
