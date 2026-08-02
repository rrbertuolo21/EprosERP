<script setup lang="ts">
/**
 * PPP (Perfil Profissiográfico Previdenciário) — RH / SSO.
 * Fonte: GET/POST /rh/sso/ppp + POST /rh/sso/exames (registra exame do SSO).
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import { useOpcoesRh } from '~/components/rh-comum/useOpcoesRh'

definePageMeta({ layout: 'default' })

interface Ppp {
  id: string
  colaboradorId?: string | null
  observacao?: string | null
}
interface Filtros { busca?: string }

const router = useRouter()
const toast = useToast()
const { colaboradores, carregarColaboradores } = useOpcoesRh()
const lista = useApiList<Ppp, Filtros>('/rh/sso/ppp', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Ppp>[] = [
  { key: 'colaboradorId', label: 'Colaborador', sortable: false },
  { key: 'observacao', label: 'Observação', sortable: false }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Observação...', grow: true }
]
function novo() {
  router.push('/erp/rh/sso/ppp/novo')
}

// --- Registrar exame ---
const exameVisivel = ref(false)
const salvandoExame = ref(false)
const exame = ref<{
  pppId: string | null
  colaboradorId: string | null
  dataUltimo: string | null
  tipo: string | null
  natureza: string | null
  exame: string | null
  indicacaoResultados: string | null
}>({
  pppId: null,
  colaboradorId: null,
  dataUltimo: null,
  tipo: null,
  natureza: null,
  exame: null,
  indicacaoResultados: null
})

async function abrirExame() {
  exame.value = { pppId: null, colaboradorId: null, dataUltimo: null, tipo: null, natureza: null, exame: null, indicacaoResultados: null }
  if (!colaboradores.value.length) await carregarColaboradores()
  exameVisivel.value = true
}

async function salvarExame() {
  if (!exame.value.colaboradorId) {
    toast.error('Informe o colaborador do exame.')
    return
  }
  salvandoExame.value = true
  try {
    await useApi('/rh/sso/exames', { method: 'POST', body: exame.value })
    toast.success('Exame registrado com sucesso.')
    exameVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoExame.value = false
  }
}
onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="PPP" subtitle="Perfil Profissiográfico Previdenciário" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="abrirExame">Registrar exame</button>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo PPP</button>
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
      empty-text="Nenhum PPP encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    />

    <AppDialog v-model="exameVisivel" title="Registrar exame (SSO)" width="480px" persistent>
      <div class="dlg-form">
        <SelectField v-model="exame.colaboradorId" label="Colaborador" required :options="colaboradores" />
        <!-- TODO: sem endpoint de listagem para PPP existente aqui — UUID manual/opcional. -->
        <TextField v-model="exame.pppId" label="PPP vinculado (UUID)" placeholder="UUID (opcional)" />
        <DateTimeField v-model="exame.dataUltimo" label="Data do último exame" />
        <TextField v-model="exame.tipo" label="Tipo" maxlength="40" />
        <TextField v-model="exame.natureza" label="Natureza" maxlength="40" />
        <TextField v-model="exame.exame" label="Exame" maxlength="120" />
        <TextField v-model="exame.indicacaoResultados" label="Indicação de resultados" maxlength="200" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoExame" @click="exameVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoExame" @click="salvarExame">
          <span v-if="salvandoExame" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dlg-form { display: grid; gap: 14px; }
</style>
