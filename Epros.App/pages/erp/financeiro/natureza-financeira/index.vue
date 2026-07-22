<script setup lang="ts">
/**
 * Listagem de Naturezas Financeiras — Financeiro / Cadastros auxiliares.
 *
 * Porta o comportamento de `financeiro/natureza-financeira/index.vue` do legado:
 * tabela paginada, editar/excluir. A importação por planilha (xlsx) do legado não
 * tem endpoint equivalente no backend novo — não portada (ver observações finais).
 *
 * A rota `/configuracao-codigo-naturezas-financeiras` devolve a paginação dentro de
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

interface NaturezaFinanceira {
  id: string
  descricao: string
  tipoConfiguracaoNatureza: number
}

const TIPOS_NATUREZA: Record<number, string> = {
  1: 'Recebimento',
  2: 'Pagamento'
}

const router = useRouter()
const toast = useToast()

const itens = ref<NaturezaFinanceira[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(25)
const carregando = ref(false)
const filtros = reactive<{ localizar: string }>({ localizar: '' })

const colunas: DataTableColumn<NaturezaFinanceira>[] = [
  { key: 'descricao', label: 'Descrição', sortable: false },
  { key: 'tipoConfiguracaoNatureza', label: 'Tipo', sortable: false, width: '160px' }
]

const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'Descrição...', grow: true }
]

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<NaturezaFinanceira | null>(null)

function tipoNatureza(tipo: number): string {
  return TIPOS_NATUREZA[tipo] ?? ''
}

async function buscar() {
  carregando.value = true
  try {
    const resposta = await useApi<{ dados?: { total: number; itens: NaturezaFinanceira[] } }>(
      '/configuracao-codigo-naturezas-financeiras',
      { query: { localizar: filtros.localizar || undefined, pagina: pagina.value, tamanhoPagina: tamanhoPagina.value } }
    )
    const dados = extrairDados<{ total: number; itens: NaturezaFinanceira[] }>(resposta)
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

function novaNatureza() {
  router.push('/erp/financeiro/natureza-financeira/novo')
}

function editarNatureza(item: NaturezaFinanceira) {
  router.push(`/erp/financeiro/natureza-financeira/${item.id}`)
}

function pedirExclusao(item: NaturezaFinanceira) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi('/configuracao-codigo-naturezas-financeiras/{id}', {
      method: 'DELETE',
      params: { id: itemParaExcluir.value.id }
    })
    toast.success('Natureza financeira excluída com sucesso.')
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
    <PageToolbar title="Natureza Financeira" subtitle="Configuração de códigos de natureza financeira (formas de recebimento/pagamento)" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novaNatureza">+ Nova natureza</button>
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
      empty-text="Nenhuma natureza financeira encontrada. Adicione uma nova para começar."
      @update:page="irParaPagina"
      @update:page-size="mudarTamanhoPagina"
      @row-click="editarNatureza"
    >
      <template #cell-tipoConfiguracaoNatureza="{ value }">
        <span class="badge" :class="Number(value) === 1 ? 'badge-paga' : 'badge-pendente'">
          {{ tipoNatureza(Number(value)) }}
        </span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editarNatureza(row)">Editar</button>
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
