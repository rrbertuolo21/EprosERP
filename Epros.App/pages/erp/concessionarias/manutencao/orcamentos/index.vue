<script setup lang="ts">
/**
 * Manutenção / Orçamentos — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatDataHora, formatMoeda } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface Orcamento {
  id: string
  ordemServicoId?: string
  validade?: string
  valorTotal?: number
}

const colunas: DataTableColumn<Orcamento>[] = [
  { key: 'ordemServicoId', label: 'Ordem de serviço' },
  { key: 'validade', label: 'Validade', formatter: formatDataHora },
  { key: 'valorTotal', label: 'Valor total', align: 'right', formatter: formatMoeda }
]

const campos: CampoForm[] = [
  { key: 'ordemServicoId', label: 'Ordem de serviço', tipo: 'fk', fkPath: '/concessionarias/manutencao/ordens-servico', obrigatorio: true },
  { key: 'validade', label: 'Validade', tipo: 'datetime', obrigatorio: true },
  { key: 'valorTotal', label: 'Valor total', tipo: 'money', obrigatorio: true }
]
</script>

<template>
  <RecursoLista
    title="Orçamentos"
    subtitle="Manutenção — orçamentos de serviço"
    path="/concessionarias/manutencao/orcamentos"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Novo orçamento"
  />
</template>
