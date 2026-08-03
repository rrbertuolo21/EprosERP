<script setup lang="ts">
/**
 * Listagem de Períodos Orçamentários — Planejamento/Orçamento.
 * GET /planejamento-orcamento/periodos, POST. Ações: aprovar/ativar/encerrar.
 * Budgets do período no detalhe (/{id}).
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'

definePageMeta({ layout: 'default' })

interface Periodo {
  id: string
  dataInicio?: string | null
  dataFim?: string | null
  statusDescricao?: string | null
}

const router = useRouter()
const toast = useToast()
const { formatarData } = useHelper()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const lista = useApiList<Periodo>('/planejamento-orcamento/periodos', { tamanhoPaginaInicial: 25 })

const colunas: DataTableColumn<Periodo>[] = [
  { key: 'dataInicio', label: 'Início', sortable: true, formatter: (v) => (v ? formatarData(v as string) : '') },
  { key: 'dataFim', label: 'Fim', sortable: true, formatter: (v) => (v ? formatarData(v as string) : '') },
  { key: 'statusDescricao', label: 'Status', sortable: false, align: 'center' }
]

function novo() {
  router.push('/erp/financeiro/planejamento-orcamento/periodos/novo')
}
function abrir(item: Periodo) {
  router.push(`/erp/financeiro/planejamento-orcamento/periodos/${item.id}`)
}

async function acao(item: Periodo, nome: 'aprovar' | 'ativar' | 'encerrar', titulo: string) {
  const ok = await confirmRef.value!.open(titulo, 'Confirma esta operação no período?', { danger: nome === 'encerrar' })
  if (!ok) return
  try {
    await useApi(`/planejamento-orcamento/periodos/{id}/${nome}`, { method: 'POST', params: { id: item.id } })
    toast.success('Operação concluída.')
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
    <PageToolbar title="Períodos Orçamentários" subtitle="Períodos e budgets do orçamento" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo período</button>
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
      empty-text="Nenhum período cadastrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="abrir"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Budgets" @click.stop="abrir(row)">Budgets</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Aprovar" @click.stop="acao(row, 'aprovar', 'Aprovar período')">Aprovar</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Ativar" @click.stop="acao(row, 'ativar', 'Ativar período')">Ativar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Encerrar" @click.stop="acao(row, 'encerrar', 'Encerrar período')">Encerrar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
