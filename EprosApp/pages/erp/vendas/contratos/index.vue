<script setup lang="ts">
/**
 * Contratos de Venda.
 * Contrato real: base `/vendas/contratos`.
 *   GET (lista) ?status=&tipoId=&usuarioResponsavelId=&localizar= · GET {id}
 *   POST {id}/publicar (PublicarContratoCommand)
 * A criação exige TipoId/responsável (setup por POST tipos/modelos, sem GET) — ver relatório.
 * Lista + publicação. Apresentação — sem regra nova.
 */
import { onMounted, ref } from 'vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { formatMoeda, formatData } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default', middleware: 'auth' })

const STATUS = [
  { value: 0, label: 'Rascunho' },
  { value: 1, label: 'Aguardando assinaturas' },
  { value: 2, label: 'Ativo' },
  { value: 3, label: 'Expirado' },
  { value: 4, label: 'Arquivado' },
  { value: 5, label: 'Cancelado' }
]

interface Contrato {
  id: string
  numeroContrato?: string | null
  assunto?: string | null
  valor?: number | null
  dataInicio?: string | null
  dataFim?: string | null
  status?: string | null
}
interface Filtros { status?: number | null; localizar?: string }

const toast = useToast()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()
const lista = useApiList<Contrato, Filtros>('/vendas/contratos', {
  filtrosIniciais: { status: null, localizar: '' },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<Contrato>[] = [
  { key: 'numeroContrato', label: 'Nº' },
  { key: 'assunto', label: 'Assunto' },
  { key: 'valor', label: 'Valor', align: 'right', formatter: formatMoeda },
  { key: 'dataInicio', label: 'Início', formatter: formatData },
  { key: 'dataFim', label: 'Fim', formatter: formatData },
  { key: 'status', label: 'Status' }
]
const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'Nº ou assunto...', grow: true },
  { key: 'status', label: 'Status', type: 'select', options: STATUS }
]

async function publicar(c: Contrato) {
  const ok = await confirmRef.value!.open('Publicar contrato', `Publicar o contrato "${c.numeroContrato ?? c.assunto ?? ''}"?`)
  if (!ok) return
  try {
    await useApi('/vendas/contratos/{id}/publicar', {
      method: 'POST',
      params: { id: c.id },
      body: { contratoId: c.id }
    })
    toast.success('Contrato publicado.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

onMounted(() => void lista.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Contratos de venda" subtitle="Ciclo de vida dos contratos comerciais" :loading="lista.carregando.value" />

    <FilterBar
      :fields="camposFiltro"
      :model-value="lista.filtros.value"
      :loading="lista.carregando.value"
      @update:model-value="(v) => (lista.filtros.value = v as typeof lista.filtros.value)"
      @search="lista.aplicarFiltros($event as Partial<typeof lista.filtros.value>)"
      @clear="lista.limpar()"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhum contrato encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="publicar(row)">Publicar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
