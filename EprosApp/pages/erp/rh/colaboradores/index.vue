<script setup lang="ts">
/**
 * Listagem de Colaboradores — RH / Colaboradores.
 *
 * Fonte: GET/POST /rh/colaboradores + POST /rh/colaboradores/{id}/desligar.
 * A API não expõe GET/{id}, PUT nem DELETE — a tela oferece criação e a ação "Desligar".
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useMask } from '~/composables/useMask'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ layout: 'default' })

interface Colaborador {
  id: string
  nome?: string | null
  cpf?: string | null
  email?: string | null
  cargo?: string | null
  departamento?: string | null
  salarioBase?: number | null
  dataAdmissao?: string | null
}

interface Filtros {
  busca?: string
}

const router = useRouter()
const toast = useToast()
const { maskCPF } = useMask()

const lista = useApiList<Colaborador, Filtros>('/rh/colaboradores', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Colaborador>[] = [
  { key: 'nome', label: 'Nome', sortable: true },
  { key: 'cpf', label: 'CPF', sortable: false },
  { key: 'email', label: 'E-mail', sortable: false },
  { key: 'cargo', label: 'Cargo', sortable: false },
  { key: 'departamento', label: 'Departamento', sortable: false },
  { key: 'salarioBase', label: 'Salário base', sortable: false, align: 'right' },
  { key: 'dataAdmissao', label: 'Admissão', sortable: true, align: 'center' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Nome, CPF ou e-mail...', grow: true }
]

const brl = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
function formatarData(v?: string | null) {
  return v ? new Date(v).toLocaleDateString('pt-BR') : ''
}

function novo() {
  router.push('/erp/rh/colaboradores/novo')
}

// --- Ação: Desligar ---
const desligarVisivel = ref(false)
const desligando = ref(false)
const alvo = ref<Colaborador | null>(null)
const dataDemissao = ref<string | null>(null)

function pedirDesligar(item: Colaborador) {
  alvo.value = item
  dataDemissao.value = null
  desligarVisivel.value = true
}

async function confirmarDesligar() {
  if (!alvo.value || !dataDemissao.value) {
    toast.error('Informe a data de demissão.')
    return
  }
  desligando.value = true
  try {
    await useApi(`/rh/colaboradores/${alvo.value.id}/desligar`, {
      method: 'POST',
      body: { dataDemissao: dataDemissao.value }
    })
    toast.success('Colaborador desligado com sucesso.')
    desligarVisivel.value = false
    alvo.value = null
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    desligando.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Colaboradores" subtitle="Quadro de colaboradores do RH" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo colaborador</button>
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
      empty-text="Nenhum colaborador encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-cpf="{ value }">
        <span v-if="value">{{ maskCPF(String(value)) }}</span>
      </template>
      <template #cell-email="{ value }">
        <a v-if="value" :href="`mailto:${value}`" @click.stop>{{ value }}</a>
      </template>
      <template #cell-salarioBase="{ value }">
        <span v-if="value != null">{{ brl.format(Number(value)) }}</span>
      </template>
      <template #cell-dataAdmissao="{ value }">
        <span>{{ formatarData(value as string) }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Desligar" @click.stop="pedirDesligar(row)">Desligar</button>
      </template>
    </DataTable>

    <AppDialog v-model="desligarVisivel" title="Desligar colaborador" width="440px" persistent>
      <p class="dlg-msg">
        Informe a data de demissão de <strong>{{ alvo?.nome }}</strong>.
      </p>
      <DateTimeField v-model="dataDemissao" label="Data de demissão" required />
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="desligando" @click="desligarVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-danger" :disabled="desligando" @click="confirmarDesligar">
          <span v-if="desligando" class="spinner"></span>
          <span v-else>Desligar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dlg-msg { color: var(--text-secondary); font-size: 14px; line-height: 1.5; margin-bottom: 14px; }
</style>
