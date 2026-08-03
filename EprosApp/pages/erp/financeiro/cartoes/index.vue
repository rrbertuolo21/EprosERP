<script setup lang="ts">
/**
 * Listagem de Cartões de Crédito — Financeiro Avançado.
 * GET /cartoes-credito, POST, GET/PUT/DELETE /{id}. Faturas no detalhe.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import { OPCOES_BANDEIRA_CARTAO } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface Cartao {
  id: string
  apelido?: string | null
  titular?: string | null
  bandeiraCartao?: number | null
  contaBancariaNome?: string | null
  observacao?: string | null
}
interface CartaoFiltros {
  busca?: string
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<Cartao, CartaoFiltros>('/cartoes-credito', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

function bandeiraLabel(v: unknown): string {
  return OPCOES_BANDEIRA_CARTAO.find((o) => o.value === v)?.label ?? ''
}

const colunas: DataTableColumn<Cartao>[] = [
  { key: 'apelido', label: 'Apelido', sortable: true },
  { key: 'titular', label: 'Titular', sortable: true },
  { key: 'bandeiraCartao', label: 'Bandeira', sortable: false, formatter: (v) => bandeiraLabel(v) },
  { key: 'contaBancariaNome', label: 'Conta Bancária', sortable: false }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Apelido ou titular...', grow: true }
]

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<Cartao | null>(null)

function novo() {
  router.push('/erp/financeiro/cartoes/novo')
}
function editar(item: Cartao) {
  router.push(`/erp/financeiro/cartoes/${item.id}`)
}
function pedirExclusao(item: Cartao) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}
async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi('/cartoes-credito/{id}', { method: 'DELETE', params: { id: itemParaExcluir.value.id } })
    toast.success('Cartão excluído com sucesso.')
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
    <PageToolbar title="Cartões de Crédito" subtitle="Cartões vinculados às contas bancárias" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo cartão</button>
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
      empty-text="Nenhum cartão cadastrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="editar"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editar(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Excluir" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.apelido || itemParaExcluir?.titular || undefined"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>
