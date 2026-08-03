<script setup lang="ts">
/**
 * Listagem de Relatórios ESG — ESG / Relatórios.
 *
 * Contrato real (ESGController + EsgRelController):
 *   GET  /esg/relatorios                     (lista)
 *   POST /esg/relatorios                     (criar → formulário)
 *   POST /esg/relatorios/{ano}/consolidar    (consolidar por ano → ação por linha)
 *   GET  /esg/relatorios/{relatorioId}/itens (itens → tela de itens, via clique na linha)
 *   POST /esg/relatorios/itens               (adicionar item → diálogo)
 *   POST /esg/relatorios/pendencias          (abrir pendência → diálogo)
 *   POST /esg/relatorios/indicadores         (vincular indicador → diálogo)
 *   POST /esg/relatorios/snapshots           (capturar snapshot → diálogo)
 * Sem GET por id / PUT / DELETE do relatório.
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
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ layout: 'default' })

interface Relatorio {
  id: string
  anoFiscal?: number | null
  nomeRelatorio?: string | null
  totalGeralCo2e?: number | null
  status?: string | null
}

interface RelatorioFiltros {
  busca?: string
}

const router = useRouter()
const toast = useToast()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const lista = useApiList<Relatorio, RelatorioFiltros>('/esg/relatorios', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Relatorio>[] = [
  { key: 'anoFiscal', label: 'Ano fiscal', sortable: true, width: '110px' },
  { key: 'nomeRelatorio', label: 'Nome', sortable: true },
  { key: 'totalGeralCo2e', label: 'Total CO₂e', sortable: false, align: 'right' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Nome ou ano...', grow: true }
]

function novo() {
  router.push('/erp/esg/relatorios/novo')
}

function verItens(item: Relatorio) {
  router.push(`/erp/esg/relatorios/${item.id}/itens`)
}

async function consolidar(item: Relatorio) {
  if (item.anoFiscal == null) {
    toast.error('Relatório sem ano fiscal.')
    return
  }
  const ok = await confirmRef.value?.open(
    'Consolidar relatório',
    `Confirma consolidar o relatório do ano ${item.anoFiscal}?`,
    { textoConfirmar: 'Consolidar' }
  )
  if (!ok) return
  try {
    await useApi('/esg/relatorios/{ano}/consolidar', { method: 'POST', params: { ano: item.anoFiscal } })
    toast.success('Relatório consolidado.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

// --- Diálogo: Adicionar item ---------------------------------------------------
const itemVisivel = ref(false)
const salvandoItem = ref(false)
const itemForm = reactive({
  relatorioId: '' as string,
  sequencia: null as number | string | null,
  quantidade: null as number | null,
  observacao: null as string | null,
  requisitoId: null as string | null,
  tipoConteudo: null as string | null,
  justificativa: null as string | null
})

function abrirItem(item: Relatorio) {
  Object.assign(itemForm, {
    relatorioId: item.id, sequencia: null, quantidade: null, observacao: null,
    requisitoId: null, tipoConteudo: null, justificativa: null
  })
  itemVisivel.value = true
}

async function salvarItem() {
  if (itemForm.sequencia == null) {
    toast.error('Informe a sequência.')
    return
  }
  salvandoItem.value = true
  try {
    await useApi('/esg/relatorios/itens', { method: 'POST', body: { ...itemForm, sequencia: Number(itemForm.sequencia) } })
    toast.success('Item adicionado ao relatório.')
    itemVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoItem.value = false
  }
}

// --- Diálogo: Abrir pendência --------------------------------------------------
const pendVisivel = ref(false)
const salvandoPend = ref(false)
const pendForm = reactive({
  relatorioId: '' as string,
  itemId: null as string | null,
  criticidade: null as string | null,
  responsavelId: '' as string,
  prazo: null as string | null
})

function abrirPendencia(item: Relatorio) {
  Object.assign(pendForm, { relatorioId: item.id, itemId: null, criticidade: null, responsavelId: '', prazo: null })
  pendVisivel.value = true
}

async function salvarPendencia() {
  if (!pendForm.responsavelId) {
    toast.error('Informe o responsável.')
    return
  }
  salvandoPend.value = true
  try {
    await useApi('/esg/relatorios/pendencias', { method: 'POST', body: pendForm })
    toast.success('Pendência aberta.')
    pendVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoPend.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Relatórios ESG" subtitle="Relatórios de sustentabilidade por ano fiscal e frameworks" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo relatório</button>
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
      empty-text="Nenhum relatório encontrado. Crie um novo relatório para começar."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="verItens"
    >
      <template #cell-totalGeralCo2e="{ value }">
        <span v-if="value != null">{{ Number(value).toLocaleString('pt-BR') }}</span>
      </template>
      <template #cell-status="{ value }">
        <span class="badge">{{ value }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Ver itens" @click.stop="verItens(row)">Itens</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Adicionar item" @click.stop="abrirItem(row)">+ Item</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Abrir pendência" @click.stop="abrirPendencia(row)">Pendência</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Consolidar" @click.stop="consolidar(row)">Consolidar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />

    <!-- Diálogo: Adicionar item -->
    <AppDialog v-model="itemVisivel" title="Adicionar item ao relatório" width="560px">
      <div class="dialog-grid">
        <TextField v-model="itemForm.sequencia" label="Sequência" type="number" required />
        <TextField v-model="itemForm.tipoConteudo" label="Tipo de conteúdo" />
        <QuantityInput v-model="itemForm.quantidade" label="Quantidade" />
        <TextField v-model="itemForm.observacao" label="Observação" />
        <TextField v-model="itemForm.justificativa" label="Justificativa" />
        <!-- TODO: requisitoId é uuid; a listagem de requisitos por framework não é exposta pela API. -->
        <TextField v-model="itemForm.requisitoId" label="Requisito (UUID)" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="itemVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoItem" @click="salvarItem">
          <span v-if="salvandoItem" class="spinner"></span><span v-else>Adicionar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Diálogo: Abrir pendência -->
    <AppDialog v-model="pendVisivel" title="Abrir pendência" width="560px">
      <div class="dialog-grid">
        <TextField v-model="pendForm.criticidade" label="Criticidade" placeholder="Ex.: Alta, Média, Baixa" />
        <DateTimeField v-model="pendForm.prazo" label="Prazo" mode="datetime" />
        <!-- TODO: itemId / responsavelId são uuid; sem endpoint de listagem, texto. -->
        <TextField v-model="pendForm.responsavelId" label="Responsável (UUID)" required />
        <TextField v-model="pendForm.itemId" label="Item (UUID)" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="pendVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoPend" @click="salvarPendencia">
          <span v-if="salvandoPend" class="spinner"></span><span v-else>Abrir</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dialog-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 14px; }
</style>
