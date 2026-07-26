<script setup lang="ts">
/**
 * Listagem de Grupos de Recorrência — Serviços Financeiros.
 * GET /servicos-financeiros/grupos-recorrencia, POST, PUT/{id}. Sem GET/{id}: edição via listagem.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Grupo {
  id: string
  descricao?: string | null
  meses?: number | null
  diaVencimento?: number | null
  valor?: number | null
}

const router = useRouter()
const { formatarMoeda } = useHelper()

const lista = useApiList<Grupo>('/servicos-financeiros/grupos-recorrencia', { tamanhoPaginaInicial: 25 })

const colunas: DataTableColumn<Grupo>[] = [
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'meses', label: 'Meses', sortable: false, align: 'center' },
  { key: 'diaVencimento', label: 'Dia Vencimento', sortable: false, align: 'center' },
  { key: 'valor', label: 'Valor', sortable: true, align: 'right', formatter: (v) => formatarMoeda(v as number) }
]

function novo() {
  router.push('/erp/financeiro/servicos-financeiros/grupos-recorrencia/novo')
}
function editar(item: Grupo) {
  router.push(`/erp/financeiro/servicos-financeiros/grupos-recorrencia/${item.id}`)
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Grupos de Recorrência" subtitle="Grupos de cobrança recorrente" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo grupo</button>
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
      empty-text="Nenhum grupo cadastrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="editar"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editar(row)">Editar</button>
      </template>
    </DataTable>
  </div>
</template>
