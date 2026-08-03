<script setup lang="ts">
/**
 * Listagem de Planos de Contrato — Contratos Financeiros.
 * GET /contratos-financeiros/planos, POST, PUT /{id}. (Sem GET/{id}: edição usa a listagem.)
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { OPCOES_PERIODICIDADE } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface Plano {
  id: string
  descricao?: string | null
  valor?: number | null
  periodicidade?: number | null
}

const router = useRouter()
const { formatarMoeda } = useHelper()

const lista = useApiList<Plano>('/contratos-financeiros/planos', { tamanhoPaginaInicial: 25 })

function periodicidadeLabel(v: unknown): string {
  return OPCOES_PERIODICIDADE.find((o) => o.value === v)?.label ?? ''
}

const colunas: DataTableColumn<Plano>[] = [
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'valor', label: 'Valor', sortable: true, align: 'right', formatter: (v) => formatarMoeda(v as number) },
  { key: 'periodicidade', label: 'Periodicidade', sortable: false, formatter: (v) => periodicidadeLabel(v) }
]

function novo() {
  router.push('/erp/financeiro/contratos-financeiros/planos/novo')
}
function editar(item: Plano) {
  router.push(`/erp/financeiro/contratos-financeiros/planos/${item.id}`)
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Planos de Contrato" subtitle="Planos recorrentes para contratos financeiros" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo plano</button>
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
      empty-text="Nenhum plano cadastrado."
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
