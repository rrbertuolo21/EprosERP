<script setup lang="ts">
/**
 * Listagem de Planos de Contas Financeiro — Financeiro / Cadastros auxiliares.
 *
 * Porta o comportamento de `financeiro/plano-de-contas/index.vue` do legado:
 * tabela paginada, editar/excluir. A importação por planilha (xlsx) do legado não
 * tem endpoint equivalente no backend novo — não portada (ver observações finais).
 *
 * A rota `/plano-de-contas-financeiro` devolve a paginação dentro de
 * `dados = { total, pagina, itens }`, fora do padrão `useApiList`; listagem manual aqui.
 */
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { obterMensagemErro } from '~/composables/useApiList'
import { useApi, extrairDados } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'

definePageMeta({ layout: 'default' })

interface PlanoDeContas {
  id: string
  sequenciaExibicao: number
  descricao: string
  mascara: string
  ehPadrao: boolean
}

const router = useRouter()
const toast = useToast()

const itens = ref<PlanoDeContas[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(25)
const carregando = ref(false)
const filtros = reactive<{ localizar: string }>({ localizar: '' })

const colunas: DataTableColumn<PlanoDeContas>[] = [
  { key: 'descricao', label: 'Descrição', sortable: false },
  { key: 'mascara', label: 'Máscara', sortable: false, width: '160px' },
  { key: 'ehPadrao', label: 'Status', sortable: false, align: 'center', width: '140px' }
]

const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'Descrição ou máscara...', grow: true }
]

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<PlanoDeContas | null>(null)

async function buscar() {
  carregando.value = true
  try {
    const resposta = await useApi<{ dados?: { total: number; itens: PlanoDeContas[] } }>('/plano-de-contas-financeiro', {
      query: { localizar: filtros.localizar || undefined, pagina: pagina.value, tamanhoPagina: tamanhoPagina.value }
    })
    const dados = extrairDados<{ total: number; itens: PlanoDeContas[] }>(resposta)
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

function novoPlano() {
  router.push('/erp/financeiro/plano-de-contas/novo')
}

function editarPlano(item: PlanoDeContas) {
  router.push(`/erp/financeiro/plano-de-contas/${item.id}`)
}

function pedirExclusao(item: PlanoDeContas) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi('/plano-de-contas-financeiro/{id}', { method: 'DELETE', params: { id: itemParaExcluir.value.id } })
    toast.success('Plano de contas excluído com sucesso.')
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
    <PageToolbar title="Planos de Contas Financeiro" subtitle="Estrutura de contas usada para classificar movimentações financeiras" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novoPlano">+ Novo plano</button>
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
      empty-text="Nenhum plano de contas encontrado. Adicione um novo para começar."
      @update:page="irParaPagina"
      @update:page-size="mudarTamanhoPagina"
      @row-click="editarPlano"
    >
      <template #cell-ehPadrao="{ value }">
        <span class="badge" :class="value ? 'badge-paga' : 'badge-cancelada'">
          {{ value ? 'Padrão' : 'Modelo' }}
        </span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editarPlano(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Excluir" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.descricao"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>
