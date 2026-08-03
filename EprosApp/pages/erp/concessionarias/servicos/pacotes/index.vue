<script setup lang="ts">
/**
 * Serviços / Pacotes — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'

definePageMeta({ layout: 'default' })

interface PacoteServico {
  id: string
  codigo?: string | null
  nome?: string | null
}

const colunas: DataTableColumn<PacoteServico>[] = [
  { key: 'codigo', label: 'Código' },
  { key: 'nome', label: 'Nome' }
]

const campos: CampoForm[] = [
  { key: 'codigo', label: 'Código', tipo: 'text' },
  { key: 'nome', label: 'Nome', tipo: 'text' }
]
</script>

<template>
  <RecursoLista
    title="Pacotes de serviço"
    subtitle="Gestão de serviços — pacotes"
    path="/concessionarias/servicos/pacotes"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Novo pacote"
  />
</template>
