<script setup lang="ts">
/**
 * Finanças / Jornadas — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'

definePageMeta({ layout: 'default' })

interface Jornada {
  id: string
  oportunidadeId?: string
  vendaId?: string | null
  clienteId?: string
  veiculoId?: string
}

const colunas: DataTableColumn<Jornada>[] = [
  { key: 'oportunidadeId', label: 'Oportunidade' },
  { key: 'clienteId', label: 'Cliente' },
  { key: 'veiculoId', label: 'Veículo' },
  { key: 'vendaId', label: 'Venda' }
]

const campos: CampoForm[] = [
  { key: 'oportunidadeId', label: 'Oportunidade', tipo: 'fk', fkPath: '/concessionarias/crm/oportunidades', obrigatorio: true },
  { key: 'vendaId', label: 'Venda', tipo: 'fk', fkPath: '/dms/vendas' },
  // TODO: sem endpoint de listagem de clientes no digest — UUID por ora.
  { key: 'clienteId', label: 'Cliente (UUID)', tipo: 'text', obrigatorio: true, hint: 'UUID do cliente.' },
  { key: 'veiculoId', label: 'Veículo', tipo: 'fk', fkPath: '/veiculos', obrigatorio: true }
]
</script>

<template>
  <RecursoLista
    title="Jornadas de compra"
    subtitle="Finanças — jornadas do cliente"
    path="/concessionarias/financas/jornadas"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Nova jornada"
  />
</template>
