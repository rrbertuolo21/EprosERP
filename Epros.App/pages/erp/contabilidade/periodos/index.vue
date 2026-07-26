<script setup lang="ts">
/**
 * Períodos Contábeis — Contabilidade Geral / Períodos.
 *
 * Contrato:
 *   GET  /contabilidade-geral/periodos?anoFiscal=
 *   POST /contabilidade-geral/periodos                 (anoFiscal, dataInicio?, dataFim?)
 *   POST /contabilidade-geral/periodos/{id}/iniciar-fechamento
 *   POST /contabilidade-geral/periodos/{id}/fechar     (dataFechamento, usuarioFechamentoId?)
 *   POST /contabilidade-geral/periodos/{id}/reabrir    (motivo, usuarioReaberturaId?)
 * Não há GET/{id}, PUT nem DELETE: criação por diálogo, mudanças de estado por ação.
 */
import { ref, reactive, onMounted } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import { estadoPeriodoLabel, estadoPeriodoClasse } from '~/components/contabilidade-contas/enums'

definePageMeta({ layout: 'default' })

interface PeriodoContabil {
  id: string
  anoFiscal: number
  dataInicio?: string | null
  dataFim?: string | null
  dataFechamento?: string | null
  estado: number
}

interface PeriodoFiltros {
  anoFiscal?: string | null
}

const toast = useToast()
const { formatarData } = useHelper()

const lista = useApiList<PeriodoContabil, PeriodoFiltros>('/contabilidade-geral/periodos', {
  filtrosIniciais: { anoFiscal: '' },
  tamanhoPaginaInicial: 100
})

const colunas: DataTableColumn<PeriodoContabil>[] = [
  { key: 'anoFiscal', label: 'Ano Fiscal', sortable: false, width: '120px' },
  { key: 'dataInicio', label: 'Início', sortable: false, width: '130px' },
  { key: 'dataFim', label: 'Fim', sortable: false, width: '130px' },
  { key: 'dataFechamento', label: 'Fechamento', sortable: false, width: '130px' },
  { key: 'estado', label: 'Estado', sortable: false, align: 'center', width: '150px' }
]

const camposFiltro: FilterField[] = [
  { key: 'anoFiscal', label: 'Ano fiscal', type: 'text', placeholder: 'Ex.: 2026' }
]

/* ----- Criar período ----- */
const criarVisivel = ref(false)
const salvandoCriar = ref(false)
const novoPeriodo = reactive<{ anoFiscal: number | null; dataInicio: string | null; dataFim: string | null }>({
  anoFiscal: new Date().getFullYear(),
  dataInicio: null,
  dataFim: null
})

function abrirCriar() {
  novoPeriodo.anoFiscal = new Date().getFullYear()
  novoPeriodo.dataInicio = null
  novoPeriodo.dataFim = null
  criarVisivel.value = true
}

async function salvarCriar() {
  if (!novoPeriodo.anoFiscal) {
    toast.error('Ano fiscal é obrigatório.')
    return
  }
  salvandoCriar.value = true
  try {
    await useApi('/contabilidade-geral/periodos', {
      method: 'POST',
      body: { anoFiscal: novoPeriodo.anoFiscal, dataInicio: novoPeriodo.dataInicio, dataFim: novoPeriodo.dataFim }
    })
    toast.success('Período contábil criado com sucesso!')
    criarVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoCriar.value = false
  }
}

/* ----- Iniciar fechamento ----- */
async function iniciarFechamento(item: PeriodoContabil) {
  try {
    await useApi(`/contabilidade-geral/periodos/{id}/iniciar-fechamento`, { method: 'POST', params: { id: item.id } })
    toast.success('Fechamento iniciado.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

/* ----- Fechar período ----- */
const fecharVisivel = ref(false)
const salvandoFechar = ref(false)
const periodoFechar = ref<PeriodoContabil | null>(null)
const dataFechamento = ref<string | null>(new Date().toISOString().slice(0, 10))

function abrirFechar(item: PeriodoContabil) {
  periodoFechar.value = item
  dataFechamento.value = new Date().toISOString().slice(0, 10)
  fecharVisivel.value = true
}

async function confirmarFechar() {
  if (!periodoFechar.value || !dataFechamento.value) {
    toast.error('Data de fechamento é obrigatória.')
    return
  }
  salvandoFechar.value = true
  try {
    await useApi(`/contabilidade-geral/periodos/{id}/fechar`, {
      method: 'POST',
      params: { id: periodoFechar.value.id },
      body: { id: periodoFechar.value.id, usuarioFechamentoId: null, dataFechamento: dataFechamento.value }
    })
    toast.success('Período fechado com sucesso.')
    fecharVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoFechar.value = false
  }
}

/* ----- Reabrir período ----- */
const reabrirVisivel = ref(false)
const salvandoReabrir = ref(false)
const periodoReabrir = ref<PeriodoContabil | null>(null)
const motivoReabertura = ref('')

function abrirReabrir(item: PeriodoContabil) {
  periodoReabrir.value = item
  motivoReabertura.value = ''
  reabrirVisivel.value = true
}

async function confirmarReabrir() {
  if (!periodoReabrir.value || !motivoReabertura.value.trim()) {
    toast.error('Informe o motivo da reabertura.')
    return
  }
  salvandoReabrir.value = true
  try {
    await useApi(`/contabilidade-geral/periodos/{id}/reabrir`, {
      method: 'POST',
      params: { id: periodoReabrir.value.id },
      body: { id: periodoReabrir.value.id, usuarioReaberturaId: null, motivo: motivoReabertura.value }
    })
    toast.success('Período reaberto.')
    reabrirVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoReabrir.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar
      title="Períodos Contábeis"
      subtitle="Exercícios/períodos da contabilidade geral e seu ciclo de fechamento"
      :loading="lista.carregando.value"
    >
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirCriar">+ Novo período</button>
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
      empty-text="Nenhum período contábil encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
    >
      <template #cell-dataInicio="{ value }">{{ value ? formatarData(String(value)) : '—' }}</template>
      <template #cell-dataFim="{ value }">{{ value ? formatarData(String(value)) : '—' }}</template>
      <template #cell-dataFechamento="{ value }">{{ value ? formatarData(String(value)) : '—' }}</template>
      <template #cell-estado="{ value }">
        <span class="badge" :class="`badge-${estadoPeriodoClasse(Number(value))}`">
          {{ estadoPeriodoLabel(Number(value)) }}
        </span>
      </template>
      <template #actions="{ row }">
        <button
          v-if="row.estado === 0 || row.estado === 3"
          type="button"
          class="btn btn-ghost btn-sm"
          title="Iniciar fechamento"
          @click.stop="iniciarFechamento(row)"
        >Iniciar fechamento</button>
        <button
          v-if="row.estado === 1"
          type="button"
          class="btn btn-ghost btn-sm"
          title="Fechar"
          @click.stop="abrirFechar(row)"
        >Fechar</button>
        <button
          v-if="row.estado === 2"
          type="button"
          class="btn btn-ghost btn-sm"
          title="Reabrir"
          @click.stop="abrirReabrir(row)"
        >Reabrir</button>
      </template>
    </DataTable>

    <!-- Criar período -->
    <AppDialog v-model="criarVisivel" title="Novo período contábil" width="480px">
      <div class="dialog-form">
        <TextField v-model.number="novoPeriodo.anoFiscal" type="number" label="Ano Fiscal" required />
        <DateTimeField v-model="novoPeriodo.dataInicio" label="Data de Início" />
        <DateTimeField v-model="novoPeriodo.dataFim" label="Data de Fim" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoCriar" @click="criarVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoCriar" @click="salvarCriar">
          <span v-if="salvandoCriar" class="spinner"></span>
          <span v-else>Criar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Fechar período -->
    <AppDialog v-model="fecharVisivel" title="Fechar período" width="440px">
      <div class="dialog-form">
        <p class="dialog-msg">Confirme o fechamento do período <strong>{{ periodoFechar?.anoFiscal }}</strong>.</p>
        <DateTimeField v-model="dataFechamento" label="Data de Fechamento" required />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoFechar" @click="fecharVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoFechar" @click="confirmarFechar">
          <span v-if="salvandoFechar" class="spinner"></span>
          <span v-else>Fechar período</span>
        </button>
      </template>
    </AppDialog>

    <!-- Reabrir período -->
    <AppDialog v-model="reabrirVisivel" title="Reabrir período" width="440px">
      <div class="dialog-form">
        <p class="dialog-msg">Reabertura do período <strong>{{ periodoReabrir?.anoFiscal }}</strong>. Informe o motivo.</p>
        <TextField v-model="motivoReabertura" label="Motivo" required maxlength="200" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoReabrir" @click="reabrirVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoReabrir" @click="confirmarReabrir">
          <span v-if="salvandoReabrir" class="spinner"></span>
          <span v-else>Reabrir</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dialog-form { display: flex; flex-direction: column; gap: 14px; }
.dialog-msg { color: var(--text-secondary); font-size: 14px; line-height: 1.5; margin: 0; }
</style>
