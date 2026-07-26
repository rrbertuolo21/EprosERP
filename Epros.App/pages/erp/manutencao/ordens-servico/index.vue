<script setup lang="ts">
/**
 * Listagem de Ordens de Serviço — Manutenção / Ordens de Serviço.
 * Fonte: GET /manutencao/ordens-servico.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { perfilOrdemOpcoes, statusOrdemServicoOpcoes } from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface OrdemServico {
  id: string
  numero?: string | null
  perfilOrdem?: number | null
  statusCodigo?: number | null
  data?: string | null
  garantia?: boolean | null
}

const router = useRouter()

const lista = useApiList<OrdemServico>('/manutencao/ordens-servico', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<OrdemServico>[] = [
  { key: 'numero', label: 'Número', sortable: true, width: '130px' },
  { key: 'perfilOrdem', label: 'Perfil', sortable: true, width: '120px' },
  { key: 'data', label: 'Data', sortable: true, width: '140px' },
  { key: 'statusCodigo', label: 'Status', sortable: true, align: 'center', width: '140px' },
  { key: 'garantia', label: 'Garantia', sortable: false, align: 'center', width: '100px' }
]

function rotuloPerfil(v: unknown): string {
  return perfilOrdemOpcoes.find((o) => o.value === Number(v))?.label ?? ''
}
function rotuloStatus(v: unknown): string {
  return statusOrdemServicoOpcoes.find((o) => o.value === Number(v))?.label ?? ''
}
function formatarData(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? String(v) : d.toLocaleDateString('pt-BR')
}

function novo() {
  router.push('/erp/manutencao/ordens-servico/novo')
}
function editar(item: OrdemServico) {
  router.push(`/erp/manutencao/ordens-servico/${item.id}`)
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Ordens de serviço" subtitle="Abertura, itens e evolução de status das OS" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova OS</button>
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
      empty-text="Nenhuma ordem de serviço encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="editar"
    >
      <template #cell-perfilOrdem="{ value }"><span>{{ rotuloPerfil(value) }}</span></template>
      <template #cell-statusCodigo="{ value }"><span class="badge badge-info">{{ rotuloStatus(value) }}</span></template>
      <template #cell-data="{ value }"><span>{{ formatarData(value) }}</span></template>
      <template #cell-garantia="{ value }"><span>{{ value ? 'Sim' : 'Não' }}</span></template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Abrir" @click.stop="editar(row)">Abrir</button>
      </template>
    </DataTable>
  </div>
</template>
