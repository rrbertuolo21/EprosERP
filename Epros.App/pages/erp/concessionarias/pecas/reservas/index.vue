<script setup lang="ts">
/**
 * Peças / Reservas — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatNumero } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface ReservaPeca {
  id: string
  demandaId?: string
  pecaId?: string
  localId?: string
  quantidadeReservada?: number
}

const colunas: DataTableColumn<ReservaPeca>[] = [
  { key: 'demandaId', label: 'Demanda' },
  { key: 'pecaId', label: 'Peça' },
  { key: 'quantidadeReservada', label: 'Qtd. reservada', align: 'right', formatter: formatNumero }
]

const campos: CampoForm[] = [
  { key: 'demandaId', label: 'Demanda', tipo: 'fk', fkPath: '/concessionarias/pecas/demandas', obrigatorio: true },
  { key: 'pecaId', label: 'Peça', tipo: 'fk', fkPath: '/concessionarias/pecas/pecas', obrigatorio: true },
  // TODO: sem endpoint de listagem no digest — UUID por ora.
  { key: 'localId', label: 'Local (UUID)', tipo: 'text', obrigatorio: true, hint: 'UUID do local.' },
  { key: 'quantidadeReservada', label: 'Quantidade reservada', tipo: 'quantity', obrigatorio: true }
]
</script>

<template>
  <RecursoLista
    title="Reservas de peças"
    subtitle="Gestão de peças — reservas"
    path="/concessionarias/pecas/reservas"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Nova reserva"
  />
</template>
