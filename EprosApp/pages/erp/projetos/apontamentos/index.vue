<script setup lang="ts">
/**
 * Apontamentos de horas (timesheet) — PROJETOS / Gestão de Recursos.
 * GET /projetos/recursos/apontamentos · POST criar · ações submeter/aprovar por linha.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { rotuloStatusWorkflow, fmtData, TIMESHEET_TIPO_OPCOES } from '~/components/projetos-shared/statusWorkflow'

definePageMeta({ layout: 'default' })

interface Apontamento {
  id: string; usuarioId?: string | null; projetoId?: string | null; tarefaId?: string | null
  data?: string | null; horas?: number | null; minutos?: number | null; notas?: string | null
  tipo?: number | null; status?: number | null
}

const router = useRouter()
const toast = useToast()
const lista = useApiList<Apontamento, Record<string, unknown>>('/projetos/recursos/apontamentos', {
  tamanhoPaginaInicial: 25
})
const processando = ref(false)

function rotuloTipo(v: unknown): string {
  const f = TIMESHEET_TIPO_OPCOES.find((o) => String(o.value) === String(v))
  return f ? f.label : '—'
}

const colunas: DataTableColumn<Apontamento>[] = [
  { key: 'data', label: 'Data', sortable: true, width: '120px' },
  { key: 'projetoId', label: 'Projeto (ID)', sortable: false },
  { key: 'usuarioId', label: 'Usuário (ID)', sortable: false },
  { key: 'tipo', label: 'Tipo', sortable: true, width: '160px' },
  { key: 'tempo', label: 'Tempo', sortable: false, align: 'center', width: '110px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '120px' }
]

function novo() { router.push('/erp/projetos/apontamentos/novo') }

async function executar(item: Apontamento, acao: 'submeter' | 'aprovar') {
  processando.value = true
  try {
    await useApi(`/projetos/recursos/apontamentos/${item.id}/${acao}`, { method: 'POST' })
    toast.success('Ação executada.')
    await lista.buscar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { processando.value = false }
}

onMounted(() => { void lista.buscar() })
</script>

<template>
  <div>
    <PageToolbar title="Apontamentos" subtitle="Registro de horas dos recursos" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo apontamento</button>
      </template>
    </PageToolbar>

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhum apontamento encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-data="{ value }">{{ fmtData(value) }}</template>
      <template #cell-tipo="{ value }">{{ rotuloTipo(value) }}</template>
      <template #cell-tempo="{ row }">{{ Number(row.horas ?? 0) }}h {{ Number(row.minutos ?? 0) }}m</template>
      <template #cell-status="{ value }"><span class="badge badge-cancelada">{{ rotuloStatusWorkflow(value) }}</span></template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" :disabled="processando" @click.stop="executar(row, 'submeter')">Submeter</button>
        <button type="button" class="btn btn-ghost btn-sm" :disabled="processando" @click.stop="executar(row, 'aprovar')">Aprovar</button>
      </template>
    </DataTable>
  </div>
</template>
