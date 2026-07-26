<script setup lang="ts">
/**
 * Serviços / Tipos — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'

definePageMeta({ layout: 'default' })

interface TipoServico {
  id: string
  codigo?: string | null
  nome?: string | null
  descricao?: string | null
}

const colunas: DataTableColumn<TipoServico>[] = [
  { key: 'codigo', label: 'Código' },
  { key: 'nome', label: 'Nome' },
  { key: 'descricao', label: 'Descrição' }
]

const campos: CampoForm[] = [
  { key: 'codigo', label: 'Código', tipo: 'text' },
  { key: 'nome', label: 'Nome', tipo: 'text' },
  { key: 'descricao', label: 'Descrição', tipo: 'textarea' }
]
</script>

<template>
  <RecursoLista
    title="Tipos de serviço"
    subtitle="Gestão de serviços — tipos"
    path="/concessionarias/servicos/tipos"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Novo tipo"
  />
</template>
