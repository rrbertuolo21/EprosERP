<script setup lang="ts">
/**
 * Garantias / Veículos em garantia — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatDataHora } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface VeiculoGarantia {
  id: string
  veiculoId?: string
  vendaId?: string
  chassiVin?: string | null
  planoVersaoId?: string
  dataEntrega?: string
  inicioVigencia?: string
  fimVigencia?: string
}

const colunas: DataTableColumn<VeiculoGarantia>[] = [
  { key: 'chassiVin', label: 'Chassi/VIN' },
  { key: 'dataEntrega', label: 'Entrega', formatter: formatDataHora },
  { key: 'inicioVigencia', label: 'Início vigência', formatter: formatDataHora },
  { key: 'fimVigencia', label: 'Fim vigência', formatter: formatDataHora }
]

const campos: CampoForm[] = [
  { key: 'veiculoId', label: 'Veículo', tipo: 'fk', fkPath: '/veiculos', obrigatorio: true },
  { key: 'vendaId', label: 'Venda', tipo: 'fk', fkPath: '/dms/vendas', obrigatorio: true },
  { key: 'chassiVin', label: 'Chassi/VIN', tipo: 'text' },
  // Palpite: planoVersaoId referencia a versão de um plano; usamos a lista de planos.
  { key: 'planoVersaoId', label: 'Versão do plano', tipo: 'fk', fkPath: '/concessionarias/garantias/planos', obrigatorio: true },
  { key: 'dataEntrega', label: 'Data de entrega', tipo: 'datetime', obrigatorio: true },
  { key: 'inicioVigencia', label: 'Início da vigência', tipo: 'datetime', obrigatorio: true },
  { key: 'fimVigencia', label: 'Fim da vigência', tipo: 'datetime', obrigatorio: true },
  { key: 'quilometragemInicio', label: 'Km inicial', tipo: 'quantity' },
  { key: 'quilometragemLimite', label: 'Km limite', tipo: 'quantity' }
]
</script>

<template>
  <RecursoLista
    title="Veículos em garantia"
    subtitle="Garantias — vínculos de veículo e plano"
    path="/concessionarias/garantias/veiculos"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Novo vínculo"
  />
</template>
