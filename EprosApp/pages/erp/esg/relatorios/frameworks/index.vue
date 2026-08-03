<script setup lang="ts">
/**
 * Listagem de Frameworks de Relatório — ESG / Relatórios / Frameworks.
 *
 * Contrato real (EsgRelController):
 *   GET  /esg/relatorios/frameworks    (lista)
 *   POST /esg/relatorios/frameworks    (criar → formulário)
 *   POST /esg/relatorios/requisitos    (adicionar requisito → diálogo por framework)
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

definePageMeta({ layout: 'default' })

interface Framework {
  id: string
  codigo?: string | null
  versao?: string | null
  descricao?: string | null
  ativo?: boolean | null
}

interface FrameworkFiltros {
  busca?: string
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<Framework, FrameworkFiltros>('/esg/relatorios/frameworks', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Framework>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '160px' },
  { key: 'versao', label: 'Versão', sortable: true, width: '120px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'ativo', label: 'Ativo', sortable: true, align: 'center', width: '100px' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Código ou descrição...', grow: true }
]

function novo() {
  router.push('/erp/esg/relatorios/frameworks/novo')
}

// --- Diálogo: Adicionar requisito ----------------------------------------------
const reqVisivel = ref(false)
const salvandoReq = ref(false)
const reqForm = reactive({
  frameworkId: '' as string,
  codigo: null as string | null,
  titulo: null as string | null,
  tipoResposta: null as string | null,
  obrigatorio: false,
  ordem: null as number | string | null
})

function abrirRequisito(item: Framework) {
  Object.assign(reqForm, {
    frameworkId: item.id, codigo: null, titulo: null, tipoResposta: null, obrigatorio: false, ordem: null
  })
  reqVisivel.value = true
}

async function salvarRequisito() {
  if (reqForm.ordem == null) {
    toast.error('Informe a ordem.')
    return
  }
  salvandoReq.value = true
  try {
    await useApi('/esg/relatorios/requisitos', { method: 'POST', body: { ...reqForm, ordem: Number(reqForm.ordem) } })
    toast.success('Requisito adicionado.')
    reqVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoReq.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Frameworks de Relatório" subtitle="Frameworks de reporte (GRI, SASB, TCFD…) e seus requisitos" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo framework</button>
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
      empty-text="Nenhum framework cadastrado. Crie um novo framework para começar."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-ativo="{ value }">
        <span class="badge" :class="value ? 'badge-success' : 'badge-danger'">{{ value ? 'Sim' : 'Não' }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Adicionar requisito" @click.stop="abrirRequisito(row)">+ Requisito</button>
      </template>
    </DataTable>

    <!-- Diálogo: Adicionar requisito -->
    <AppDialog v-model="reqVisivel" title="Adicionar requisito" width="560px">
      <div class="dialog-grid">
        <TextField v-model="reqForm.codigo" label="Código" />
        <TextField v-model="reqForm.titulo" label="Título" />
        <TextField v-model="reqForm.tipoResposta" label="Tipo de resposta" placeholder="Ex.: Texto, Numérico, Booleano" />
        <TextField v-model="reqForm.ordem" label="Ordem" type="number" required />
        <label class="field toggle-row">
          <span class="field-label">{{ reqForm.obrigatorio ? 'Obrigatório' : 'Opcional' }}</span>
          <input v-model="reqForm.obrigatorio" type="checkbox" />
        </label>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="reqVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoReq" @click="salvarRequisito">
          <span v-if="salvandoReq" class="spinner"></span><span v-else>Adicionar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dialog-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 14px; }
.toggle-row { display: flex; align-items: center; gap: 10px; justify-content: flex-start; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
</style>
