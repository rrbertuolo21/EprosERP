<script setup lang="ts">
/**
 * Folhas de pagamento — RH / Folha.
 * Fonte: GET /rh/folhas + POST /rh/folhas/processar (processa a folha de um colaborador).
 * Não há GET/{id}/PUT/DELETE — a tela lista e permite processar uma nova folha.
 * O campo `verbas` (array de objetos) do processamento não é editável aqui (estrutura não
 * detalhada no digest) — enviado vazio; ver relatório.
 */
import { ref, onMounted } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import { useOpcoesRh } from '~/components/rh-comum/useOpcoesRh'

definePageMeta({ layout: 'default' })

interface Folha {
  id: string
  colaboradorId?: string | null
  mesCompetencia?: number | null
  anoCompetencia?: number | null
  situacao?: string | null
  valorLiquido?: number | null
}
interface Filtros { busca?: string }

const toast = useToast()
const { colaboradores, carregarColaboradores } = useOpcoesRh()
const lista = useApiList<Folha, Filtros>('/rh/folhas', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Folha>[] = [
  { key: 'mesCompetencia', label: 'Mês', sortable: false, align: 'center', width: '80px' },
  { key: 'anoCompetencia', label: 'Ano', sortable: false, align: 'center', width: '90px' },
  { key: 'situacao', label: 'Situação', sortable: false, align: 'center' },
  { key: 'valorLiquido', label: 'Líquido', sortable: false, align: 'right' }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Competência...', grow: true }
]
const brl = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })

const processarVisivel = ref(false)
const processando = ref(false)
const proc = ref<{ colaboradorId: string | null; mesCompetencia: number | null; anoCompetencia: number | null }>({
  colaboradorId: null,
  mesCompetencia: null,
  anoCompetencia: new Date().getFullYear()
})

async function abrirProcessar() {
  proc.value = { colaboradorId: null, mesCompetencia: null, anoCompetencia: new Date().getFullYear() }
  if (!colaboradores.value.length) await carregarColaboradores()
  processarVisivel.value = true
}

async function processar() {
  if (!proc.value.colaboradorId || !proc.value.mesCompetencia || !proc.value.anoCompetencia) {
    toast.error('Informe colaborador, mês e ano de competência.')
    return
  }
  processando.value = true
  try {
    await useApi('/rh/folhas/processar', {
      method: 'POST',
      body: {
        colaboradorId: proc.value.colaboradorId,
        mesCompetencia: Number(proc.value.mesCompetencia),
        anoCompetencia: Number(proc.value.anoCompetencia),
        verbas: []
      }
    })
    toast.success('Folha processada com sucesso.')
    processarVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}
onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Folhas de pagamento" subtitle="Folhas processadas por colaborador" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirProcessar">Processar folha</button>
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
      row-key="id"
      empty-text="Nenhuma folha encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-valorLiquido="{ value }">
        <span v-if="value != null">{{ brl.format(Number(value)) }}</span>
      </template>
    </DataTable>

    <AppDialog v-model="processarVisivel" title="Processar folha" width="460px" persistent>
      <div class="dlg-form">
        <SelectField v-model="proc.colaboradorId" label="Colaborador" required :options="colaboradores" />
        <TextField v-model="proc.mesCompetencia" label="Mês de competência" type="number" placeholder="1 a 12" />
        <TextField v-model="proc.anoCompetencia" label="Ano de competência" type="number" placeholder="AAAA" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="processando" @click="processarVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="processando" @click="processar">
          <span v-if="processando" class="spinner"></span>
          <span v-else>Processar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dlg-form { display: grid; gap: 14px; }
</style>
