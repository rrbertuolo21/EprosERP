<script setup lang="ts">
/**
 * Listagem de Induções de Equipamento — Manutenção / Configuração / Induções.
 * Fonte: GET /manutencao/equipamentos-config/inducoes. Ação por linha: aprovar.
 */
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'

definePageMeta({ layout: 'default' })

interface Inducao {
  id: string
  equipamentoId?: string | null
  status?: number | null
  dataInicio?: string | null
  observacao?: string | null
}

const router = useRouter()
const toast = useToast()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const lista = useApiList<Inducao>('/manutencao/equipamentos-config/inducoes', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<Inducao>[] = [
  { key: 'equipamentoId', label: 'Equipamento (ID)', sortable: false },
  { key: 'dataInicio', label: 'Início', sortable: true, width: '160px' },
  { key: 'observacao', label: 'Observação', sortable: false },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' }
]

function formatarDataHora(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? String(v) : d.toLocaleString('pt-BR')
}

function novo() {
  router.push('/erp/manutencao/equipamentos-config/inducoes/novo')
}

async function aprovar(item: Inducao) {
  const ok = await confirmRef.value?.open('Aprovar indução', 'Confirma a aprovação desta indução?', { textoConfirmar: 'Aprovar' })
  if (!ok) return
  try {
    await useApi(`/manutencao/equipamentos-config/inducoes/${item.id}/aprovar`, { method: 'POST', body: {} })
    toast.success('Indução aprovada.')
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
    <PageToolbar title="Induções de equipamento" subtitle="Checklist de indução e aprovação de configuração" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova indução</button>
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
      empty-text="Nenhuma indução encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-dataInicio="{ value }"><span>{{ formatarDataHora(value) }}</span></template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Aprovar" @click.stop="aprovar(row)">Aprovar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
