<script setup lang="ts">
/**
 * Listagem de Fichas de Produção (Ordens de Serviço) — Produção / Fichas.
 * GET lista (filtros situacao/pessoaId/vendaId) + GET/{id} + POST criar + PUT configuracao
 * + iniciar/concluir. Fonte: ProducaoOrdensServicoController + FichaProducaoQueries.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import { opcoesSituacaoFicha, rotuloSituacaoFicha, classeBadgeStatus, formatarData } from '~/components/producao-shared/producao'

definePageMeta({ layout: 'default' })

interface Ficha {
  id: string
  situacao?: number | string | null
  entrada?: string | null
  saida?: string | null
  transportadora?: string | null
  anoModelo?: string | null
  pessoaId?: string | null
}
interface FichaFiltros {
  situacao?: number | string | null
  pessoaId?: string | null
  vendaId?: string | null
}

const router = useRouter()
const lista = useApiList<Ficha, FichaFiltros>('/producao/fichas', {
  filtrosIniciais: { situacao: null, pessoaId: null, vendaId: null },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<Ficha>[] = [
  { key: 'situacao', label: 'Situação', align: 'center', width: '170px' },
  { key: 'entrada', label: 'Entrada', width: '150px' },
  { key: 'saida', label: 'Saída', width: '150px' },
  { key: 'transportadora', label: 'Transportadora' },
  { key: 'anoModelo', label: 'Ano/Modelo', width: '130px' }
]
const camposFiltro: FilterField[] = [
  { key: 'situacao', label: 'Situação', type: 'select', options: opcoesSituacaoFicha },
  { key: 'pessoaId', label: 'Pessoa (ID)', type: 'text', placeholder: 'UUID da pessoa' },
  { key: 'vendaId', label: 'Venda (ID)', type: 'text', placeholder: 'UUID da venda' }
]

function novo() { router.push('/erp/producao/fichas/novo') }
function abrir(item: Ficha) { router.push(`/erp/producao/fichas/${item.id}`) }

onMounted(() => { void lista.buscar() })
</script>

<template>
  <div>
    <PageToolbar title="Fichas de Produção" subtitle="Ordens de serviço de produção (bancos/estofamento)" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova ficha</button>
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
      empty-text="Nenhuma ficha de produção encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @row-click="abrir"
    >
      <template #cell-situacao="{ value }">
        <span class="badge" :class="classeBadgeStatus(rotuloSituacaoFicha(value as number | string))">{{ rotuloSituacaoFicha(value as number | string) }}</span>
      </template>
      <template #cell-entrada="{ value }">{{ formatarData(value as string) }}</template>
      <template #cell-saida="{ value }">{{ formatarData(value as string) }}</template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrir(row)">Ver</button>
      </template>
    </DataTable>
  </div>
</template>
