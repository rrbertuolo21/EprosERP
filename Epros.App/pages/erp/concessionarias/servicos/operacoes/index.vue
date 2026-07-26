<script setup lang="ts">
/**
 * Serviços / Operações — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatNumero } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface OperacaoServico {
  id: string
  tipoServicoId?: string
  codigo?: string | null
  descricao?: string | null
  tmoQuantidade?: number
  tmoUnidade?: string | null
}

const colunas: DataTableColumn<OperacaoServico>[] = [
  { key: 'codigo', label: 'Código' },
  { key: 'descricao', label: 'Descrição' },
  { key: 'tmoQuantidade', label: 'TMO', align: 'right', formatter: formatNumero },
  { key: 'tmoUnidade', label: 'Unidade TMO' }
]

const campos: CampoForm[] = [
  { key: 'tipoServicoId', label: 'Tipo de serviço', tipo: 'fk', fkPath: '/concessionarias/servicos/tipos', obrigatorio: true },
  { key: 'codigo', label: 'Código', tipo: 'text' },
  { key: 'descricao', label: 'Descrição', tipo: 'text' },
  { key: 'tmoQuantidade', label: 'TMO (quantidade)', tipo: 'quantity', obrigatorio: true, hint: 'Tempo-padrão da operação.' },
  { key: 'tmoUnidade', label: 'Unidade do TMO', tipo: 'text', hint: 'Ex.: horas.' },
  { key: 'naturezaPadrao', label: 'Natureza padrão', tipo: 'text' }
]
</script>

<template>
  <RecursoLista
    title="Operações de serviço"
    subtitle="Gestão de serviços — operações"
    path="/concessionarias/servicos/operacoes"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Nova operação"
  />
</template>
