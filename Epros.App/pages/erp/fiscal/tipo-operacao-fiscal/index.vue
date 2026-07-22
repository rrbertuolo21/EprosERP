<script setup lang="ts">
/**
 * Listagem de Tipos de Operação Fiscal — Fiscal / Tipo Operação Fiscal.
 *
 * Porta o comportamento de `fiscal/tipo-operacao-fiscal/index.vue` do legado: tabela com
 * finalidade, atendimento, tipo de frete e tipo de movimento formatados; editar/excluir.
 * Endpoint: `tipos-operacoes-fiscais`.
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
import {
  descricaoFinalidade,
  descricaoAtendimento,
  descricaoTipoFrete,
  descricaoTipoMovimento
} from './_enums'

definePageMeta({ layout: 'default' })

interface TipoOperacaoFiscal {
  id: string
  descricao: string
  finalidade: number
  atendimento: number
  tipoFrete: number
  tipoMovimento: number
  sobescreveTributacaoNcm: boolean
}

interface TipoOperacaoFiscalFiltros {
  localizar?: string
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<TipoOperacaoFiscal, TipoOperacaoFiscalFiltros>('/tipos-operacoes-fiscais', {
  filtrosIniciais: { localizar: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<TipoOperacaoFiscal>[] = [
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'finalidade', label: 'Finalidade', sortable: false },
  { key: 'atendimento', label: 'Atendimento', sortable: false },
  { key: 'tipoFrete', label: 'Tipo Frete', sortable: false },
  { key: 'tipoMovimento', label: 'Tipo Movimento', sortable: false },
  { key: 'sobescreveTributacaoNcm', label: 'Sobrescreve NCM', sortable: false, align: 'center', width: '140px' }
]

const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'Descrição...', grow: true }
]

async function buscarPagina() {
  await lista.buscar()
}

function novoTipoOperacao() {
  router.push('/erp/fiscal/tipo-operacao-fiscal/novo')
}

function editarTipoOperacao(item: TipoOperacaoFiscal) {
  router.push(`/erp/fiscal/tipo-operacao-fiscal/${item.id}`)
}

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<TipoOperacaoFiscal | null>(null)

function pedirExclusao(item: TipoOperacaoFiscal) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi(`/tipos-operacoes-fiscais/${itemParaExcluir.value.id}`, { method: 'DELETE' })
    toast.success('Tipo de operação fiscal excluído com sucesso.')
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
    <PageToolbar title="Tipos de Operação Fiscal" subtitle="Regras de finalidade, atendimento, frete e movimento por operação" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novoTipoOperacao">+ Novo tipo de operação</button>
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
      empty-text="Nenhum tipo de operação fiscal cadastrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="editarTipoOperacao"
    >
      <template #cell-finalidade="{ value }">{{ descricaoFinalidade(value as number) }}</template>
      <template #cell-atendimento="{ value }">{{ descricaoAtendimento(value as number) }}</template>
      <template #cell-tipoFrete="{ value }">{{ descricaoTipoFrete(value as number) }}</template>
      <template #cell-tipoMovimento="{ value }">{{ descricaoTipoMovimento(value as number) }}</template>
      <template #cell-sobescreveTributacaoNcm="{ value }">
        <span class="badge" :class="value ? 'badge-success' : 'badge-cancelada'">{{ value ? 'Sim' : 'Não' }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="editarTipoOperacao(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click.stop="pedirExclusao(row)">Excluir</button>
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
