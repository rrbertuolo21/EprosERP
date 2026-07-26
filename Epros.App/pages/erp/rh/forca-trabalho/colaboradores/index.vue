<script setup lang="ts">
/**
 * Força de trabalho — Colaboradores (vínculos) — RH / Força de trabalho.
 *
 * Fonte: GET/POST /rh/forca-trabalho/colaboradores, POST /{id}/demitir e POST /rh/forca-trabalho/comissoes.
 * Sem GET/{id}/PUT/DELETE — a tela cria vínculo, demite e registra comissão.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import { useOpcoesRh } from '~/components/rh-comum/useOpcoesRh'

definePageMeta({ layout: 'default' })

interface Vinculo {
  id: string
  matricula?: string | null
  cargoId?: string | null
  departamentoId?: string | null
  filialId?: string | null
  turnoId?: string | null
  dataAdmissao?: string | null
  salarioBase?: number | null
  tipoRemuneracao?: string | null
}

interface Filtros {
  busca?: string
}

const router = useRouter()
const toast = useToast()
const { colaboradores, carregarColaboradores } = useOpcoesRh()

const lista = useApiList<Vinculo, Filtros>('/rh/forca-trabalho/colaboradores', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Vinculo>[] = [
  { key: 'matricula', label: 'Matrícula', sortable: true },
  { key: 'cargoId', label: 'Cargo', sortable: false },
  { key: 'departamentoId', label: 'Departamento', sortable: false },
  { key: 'salarioBase', label: 'Salário base', sortable: false, align: 'right' },
  { key: 'tipoRemuneracao', label: 'Remuneração', sortable: false },
  { key: 'dataAdmissao', label: 'Admissão', sortable: true, align: 'center' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Matrícula...', grow: true }
]

const brl = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
function formatarData(v?: string | null) {
  return v ? new Date(v).toLocaleDateString('pt-BR') : ''
}

function novo() {
  router.push('/erp/rh/forca-trabalho/colaboradores/novo')
}

// --- Ação: Demitir ---
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()
async function pedirDemitir(item: Vinculo) {
  const ok = await confirmRef.value!.open(
    'Demitir vínculo',
    `Confirma a demissão da matrícula ${item.matricula ?? item.id}?`,
    { danger: true, textoConfirmar: 'Demitir' }
  )
  if (!ok) return
  try {
    await useApi(`/rh/forca-trabalho/colaboradores/${item.id}/demitir`, { method: 'POST' })
    toast.success('Vínculo demitido com sucesso.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

// --- Ação: Registrar comissão ---
const comissaoVisivel = ref(false)
const salvandoComissao = ref(false)
const comissao = ref<{ colaboradorId: string | null; tipoCargo: string | null; valorPercentualComissao: number | null }>({
  colaboradorId: null,
  tipoCargo: null,
  valorPercentualComissao: null
})

async function abrirComissao() {
  comissao.value = { colaboradorId: null, tipoCargo: null, valorPercentualComissao: null }
  if (!colaboradores.value.length) await carregarColaboradores()
  comissaoVisivel.value = true
}

async function salvarComissao() {
  if (!comissao.value.colaboradorId || comissao.value.valorPercentualComissao == null) {
    toast.error('Informe o colaborador e o percentual da comissão.')
    return
  }
  salvandoComissao.value = true
  try {
    await useApi('/rh/forca-trabalho/comissoes', { method: 'POST', body: comissao.value })
    toast.success('Comissão registrada com sucesso.')
    comissaoVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoComissao.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Força de trabalho" subtitle="Vínculos de colaboradores" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="abrirComissao">Registrar comissão</button>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo vínculo</button>
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
      empty-text="Nenhum vínculo encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-salarioBase="{ value }">
        <span v-if="value != null">{{ brl.format(Number(value)) }}</span>
      </template>
      <template #cell-dataAdmissao="{ value }">
        <span>{{ formatarData(value as string) }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Demitir" @click.stop="pedirDemitir(row)">Demitir</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />

    <AppDialog v-model="comissaoVisivel" title="Registrar comissão" width="460px" persistent>
      <div class="dlg-form">
        <SelectField v-model="comissao.colaboradorId" label="Colaborador" required :options="colaboradores" />
        <TextField v-model="comissao.tipoCargo" label="Tipo de cargo" maxlength="60" />
        <PercentInput v-model="comissao.valorPercentualComissao" label="Percentual da comissão" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoComissao" @click="comissaoVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoComissao" @click="salvarComissao">
          <span v-if="salvandoComissao" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dlg-form { display: grid; gap: 14px; }
</style>
