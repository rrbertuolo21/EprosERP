<script setup lang="ts">
/**
 * Garantias / Planos — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'

definePageMeta({ layout: 'default' })

interface PlanoGarantia {
  id: string
  codigo?: string | null
  nome?: string | null
  descricao?: string | null
  duracao?: number
  duracaoTipo?: string | null
}

const colunas: DataTableColumn<PlanoGarantia>[] = [
  { key: 'codigo', label: 'Código' },
  { key: 'nome', label: 'Nome' },
  { key: 'duracao', label: 'Duração', align: 'right' },
  { key: 'duracaoTipo', label: 'Unidade' }
]

const campos: CampoForm[] = [
  { key: 'codigo', label: 'Código', tipo: 'text' },
  { key: 'nome', label: 'Nome', tipo: 'text' },
  { key: 'descricao', label: 'Descrição', tipo: 'textarea' },
  { key: 'duracao', label: 'Duração', tipo: 'int', obrigatorio: true },
  { key: 'duracaoTipo', label: 'Tipo de duração', tipo: 'text', hint: 'Ex.: meses, km.' }
]
</script>

<template>
  <RecursoLista
    title="Planos de garantia"
    subtitle="Garantias — planos disponíveis"
    path="/concessionarias/garantias/planos"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Novo plano"
  />
</template>
