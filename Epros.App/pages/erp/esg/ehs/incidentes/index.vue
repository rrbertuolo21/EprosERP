<script setup lang="ts">
/**
 * Listagem de Incidentes EHS — ESG / EHS / Incidentes.
 *
 * Contrato real (EsgEhsController):
 *   GET  /esg/ehs/incidentes       (lista)
 *   POST /esg/ehs/incidentes       (criar → formulário)
 *   POST /esg/ehs/acoes-corretivas (ação corretiva → diálogo por incidente)
 * Sem GET por id / PUT / DELETE.
 */
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import { statusEsgOptions, formatarStatusEsg } from '~/components/esg-comum/statusEsg'

definePageMeta({ layout: 'default' })

interface Incidente {
  id: string
  tipo?: string | null
  dataHora?: string | null
  gravidade?: string | null
  impacto?: string | null
  numeroCat?: string | null
  status?: number | string | null
}

interface IncidenteFiltros {
  busca?: string
  status?: number | null
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<Incidente, IncidenteFiltros>('/esg/ehs/incidentes', {
  filtrosIniciais: { busca: '', status: null },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Incidente>[] = [
  { key: 'tipo', label: 'Tipo', sortable: true, width: '160px' },
  { key: 'dataHora', label: 'Data/hora', sortable: true, width: '160px' },
  { key: 'gravidade', label: 'Gravidade', sortable: true, width: '120px' },
  { key: 'impacto', label: 'Impacto', sortable: false },
  { key: 'numeroCat', label: 'CAT', sortable: false, width: '120px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '120px' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Tipo, gravidade ou impacto...', grow: true },
  { key: 'status', label: 'Status', type: 'select', options: statusEsgOptions }
]

function formatarDataHora(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? String(v) : d.toLocaleString('pt-BR')
}

function novo() {
  router.push('/erp/esg/ehs/incidentes/novo')
}

// --- Diálogo: Ação corretiva ---------------------------------------------------
const acaoVisivel = ref(false)
const salvandoAcao = ref(false)
const acaoForm = reactive({
  incidenteId: '' as string | null,
  fatorRiscoId: null as string | null,
  descricao: null as string | null,
  causa: null as string | null,
  responsavelId: '' as string,
  prazo: null as string | null,
  evidenciaArquivoId: null as string | null
})

function abrirAcao(item: Incidente) {
  acaoForm.incidenteId = item.id
  acaoForm.fatorRiscoId = null
  acaoForm.descricao = null
  acaoForm.causa = null
  acaoForm.responsavelId = ''
  acaoForm.prazo = null
  acaoForm.evidenciaArquivoId = null
  acaoVisivel.value = true
}

async function salvarAcao() {
  if (!acaoForm.responsavelId) {
    toast.error('Informe o responsável.')
    return
  }
  salvandoAcao.value = true
  try {
    await useApi('/esg/ehs/acoes-corretivas', { method: 'POST', body: acaoForm })
    toast.success('Ação corretiva registrada.')
    acaoVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoAcao.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Incidentes EHS" subtitle="Incidentes de saúde e segurança do trabalho e ações corretivas" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo incidente</button>
      </template>
    </PageToolbar>

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
      empty-text="Nenhum incidente registrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-dataHora="{ value }">{{ formatarDataHora(value) }}</template>
      <template #cell-status="{ value }">
        <span class="badge">{{ formatarStatusEsg(value) }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Ação corretiva" @click.stop="abrirAcao(row)">Ação corretiva</button>
      </template>
    </DataTable>

    <!-- Diálogo: Ação corretiva -->
    <AppDialog v-model="acaoVisivel" title="Registrar ação corretiva" width="560px">
      <div class="dialog-grid">
        <TextField v-model="acaoForm.descricao" label="Descrição" />
        <TextField v-model="acaoForm.causa" label="Causa" />
        <DateTimeField v-model="acaoForm.prazo" label="Prazo" mode="datetime" />
        <!-- TODO: responsavelId / fatorRiscoId / evidenciaArquivoId são uuid; sem listagem, texto. -->
        <TextField v-model="acaoForm.responsavelId" label="Responsável (UUID)" required />
        <TextField v-model="acaoForm.fatorRiscoId" label="Fator de risco (UUID)" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="acaoVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoAcao" @click="salvarAcao">
          <span v-if="salvandoAcao" class="spinner"></span><span v-else>Registrar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dialog-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 14px; }
</style>
