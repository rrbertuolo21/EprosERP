<script setup lang="ts">
/**
 * Garantias / Solicitações — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatDataHora, formatNumero } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface SolicitacaoGarantia {
  id: string
  veiculoGarantiaId?: string
  protocolo?: string | null
  dataOcorrencia?: string
  quilometragem?: number
  sintoma?: string | null
}

const colunas: DataTableColumn<SolicitacaoGarantia>[] = [
  { key: 'protocolo', label: 'Protocolo' },
  { key: 'dataOcorrencia', label: 'Ocorrência', formatter: formatDataHora },
  { key: 'quilometragem', label: 'Km', align: 'right', formatter: formatNumero },
  { key: 'sintoma', label: 'Sintoma' }
]

const campos: CampoForm[] = [
  { key: 'veiculoGarantiaId', label: 'Veículo em garantia', tipo: 'fk', fkPath: '/concessionarias/garantias/veiculos', obrigatorio: true },
  { key: 'protocolo', label: 'Protocolo', tipo: 'text' },
  { key: 'dataOcorrencia', label: 'Data da ocorrência', tipo: 'datetime', obrigatorio: true },
  { key: 'quilometragem', label: 'Quilometragem', tipo: 'quantity', obrigatorio: true },
  { key: 'sintoma', label: 'Sintoma', tipo: 'text' },
  { key: 'relatoCliente', label: 'Relato do cliente', tipo: 'textarea' },
  { key: 'ordemServicoId', label: 'Ordem de serviço', tipo: 'fk', fkPath: '/concessionarias/manutencao/ordens-servico' }
]
</script>

<template>
  <RecursoLista
    title="Solicitações de garantia"
    subtitle="Garantias — solicitações abertas"
    path="/concessionarias/garantias/solicitacoes"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Nova solicitação"
  />
</template>
