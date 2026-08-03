<script setup lang="ts">
/**
 * Desenvolvimento de rede / Nós da rede — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatDataHora } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface NoRede {
  id: string
  codigo?: string | null
  tipoNo?: string | null
  paiId?: string | null
  pessoaEmpresaId?: string | null
  localId?: string | null
  inicioVigencia?: string | null
  fimVigencia?: string | null
}

const colunas: DataTableColumn<NoRede>[] = [
  { key: 'codigo', label: 'Código' },
  { key: 'tipoNo', label: 'Tipo de nó' },
  { key: 'inicioVigencia', label: 'Início vigência', formatter: formatDataHora },
  { key: 'fimVigencia', label: 'Fim vigência', formatter: formatDataHora }
]

const campos: CampoForm[] = [
  { key: 'codigo', label: 'Código', tipo: 'text' },
  { key: 'tipoNo', label: 'Tipo de nó', tipo: 'text' },
  { key: 'paiId', label: 'Nó pai', tipo: 'fk', fkPath: '/concessionarias/desenvolvimento/rede' },
  // TODO: sem endpoint de listagem no digest — UUID por ora.
  { key: 'pessoaEmpresaId', label: 'Pessoa/Empresa (UUID)', tipo: 'text', hint: 'UUID da pessoa/empresa.' },
  { key: 'localId', label: 'Local (UUID)', tipo: 'text', hint: 'UUID do local.' },
  { key: 'inicioVigencia', label: 'Início da vigência', tipo: 'datetime' },
  { key: 'fimVigencia', label: 'Fim da vigência', tipo: 'datetime' }
]
</script>

<template>
  <RecursoLista
    title="Nós da rede"
    subtitle="Desenvolvimento de concessionárias — estrutura da rede"
    path="/concessionarias/desenvolvimento/rede"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Novo nó"
  />
</template>
