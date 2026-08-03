<script setup lang="ts">
/**
 * Vendas / Reservas de veículo — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatDataHora } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface ReservaVeiculo {
  id: string
  estoqueVeiculoId?: string
  oportunidadeId?: string
  inicio?: string
  fim?: string
}

const colunas: DataTableColumn<ReservaVeiculo>[] = [
  { key: 'estoqueVeiculoId', label: 'Veículo em estoque' },
  { key: 'oportunidadeId', label: 'Oportunidade' },
  { key: 'inicio', label: 'Início', formatter: formatDataHora },
  { key: 'fim', label: 'Fim', formatter: formatDataHora }
]

const campos: CampoForm[] = [
  { key: 'estoqueVeiculoId', label: 'Veículo em estoque', tipo: 'fk', fkPath: '/concessionarias/vendas/estoque', obrigatorio: true },
  { key: 'oportunidadeId', label: 'Oportunidade', tipo: 'fk', fkPath: '/concessionarias/crm/oportunidades', obrigatorio: true },
  { key: 'inicio', label: 'Início', tipo: 'datetime', obrigatorio: true },
  { key: 'fim', label: 'Fim', tipo: 'datetime', obrigatorio: true }
]
</script>

<template>
  <RecursoLista
    title="Reservas de veículo"
    subtitle="Vendas — reservas de estoque"
    path="/concessionarias/vendas/reservas"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Nova reserva"
  />
</template>
