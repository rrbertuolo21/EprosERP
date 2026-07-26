<script setup lang="ts">
/**
 * Listagem de Contas Financeiras (Tesouraria).
 * GET /tesouraria/contas, POST, PUT/{id}, POST /{id}/fechar. Saldo/transações no detalhe.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'

definePageMeta({ layout: 'default' })

interface ContaFinanceira {
  id: string
  nome?: string | null
  numeroConta?: string | null
  nota?: string | null
  statusDescricao?: string | null
}
interface ContaFiltros {
  busca?: string
}

const router = useRouter()
const toast = useToast()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const lista = useApiList<ContaFinanceira, ContaFiltros>('/tesouraria/contas', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<ContaFinanceira>[] = [
  { key: 'nome', label: 'Nome', sortable: true },
  { key: 'numeroConta', label: 'Número da Conta', sortable: true },
  { key: 'nota', label: 'Nota', sortable: false },
  { key: 'statusDescricao', label: 'Status', sortable: false, align: 'center' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Nome ou número...', grow: true }
]

function novo() {
  router.push('/erp/financeiro/tesouraria/contas/novo')
}
function editar(item: ContaFinanceira) {
  router.push(`/erp/financeiro/tesouraria/contas/${item.id}`)
}
async function fechar(item: ContaFinanceira) {
  const ok = await confirmRef.value!.open('Fechar conta', 'Confirma o fechamento desta conta financeira?', { danger: true })
  if (!ok) return
  try {
    await useApi('/tesouraria/contas/{id}/fechar', { method: 'POST', params: { id: item.id } })
    toast.success('Conta fechada.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Contas Financeiras" subtitle="Contas operacionais da tesouraria" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova conta</button>
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
      empty-text="Nenhuma conta cadastrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="editar"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editar(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Fechar" @click.stop="fechar(row)">Fechar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
