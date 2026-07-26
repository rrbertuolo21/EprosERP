<script setup lang="ts">
/**
 * Licenças — RH / Talentos.
 * Fonte: GET/POST /rh/talentos/licencas + POST /{id}/aprovar e /{id}/rejeitar
 * (ambos com body: aprovadoPorId (uuid) + comentario).
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
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ layout: 'default' })

interface Licenca {
  id: string
  motivo?: string | null
  dataInicio?: string | null
  dataFim?: string | null
  totalDias?: number | null
  situacao?: string | null
}
interface Filtros { busca?: string }

const router = useRouter()
const toast = useToast()
const lista = useApiList<Licenca, Filtros>('/rh/talentos/licencas', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Licenca>[] = [
  { key: 'motivo', label: 'Motivo', sortable: false },
  { key: 'dataInicio', label: 'Início', sortable: true, align: 'center' },
  { key: 'dataFim', label: 'Fim', sortable: false, align: 'center' },
  { key: 'totalDias', label: 'Dias', sortable: false, align: 'right', width: '90px' },
  { key: 'situacao', label: 'Situação', sortable: false, align: 'center' }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Motivo...', grow: true }
]
function formatarData(v?: string | null) {
  return v ? new Date(v).toLocaleDateString('pt-BR') : ''
}
function novo() {
  router.push('/erp/rh/talentos/licencas/novo')
}

// --- Aprovar / Rejeitar (com body) ---
const decisaoVisivel = ref(false)
const decidindo = ref(false)
const alvo = ref<Licenca | null>(null)
const tipoDecisao = ref<'aprovar' | 'rejeitar'>('aprovar')
const decisao = ref<{ aprovadoPorId: string; comentario: string | null }>({ aprovadoPorId: '', comentario: null })

function abrirDecisao(item: Licenca, tipo: 'aprovar' | 'rejeitar') {
  alvo.value = item
  tipoDecisao.value = tipo
  decisao.value = { aprovadoPorId: '', comentario: null }
  decisaoVisivel.value = true
}

async function confirmarDecisao() {
  if (!alvo.value || !decisao.value.aprovadoPorId) {
    toast.error('Informe o responsável (UUID) pela decisão.')
    return
  }
  decidindo.value = true
  try {
    await useApi(`/rh/talentos/licencas/${alvo.value.id}/${tipoDecisao.value}`, {
      method: 'POST',
      body: decisao.value
    })
    toast.success(tipoDecisao.value === 'aprovar' ? 'Licença aprovada.' : 'Licença rejeitada.')
    decisaoVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    decidindo.value = false
  }
}
onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Licenças" subtitle="Solicitações de licença/afastamento" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova licença</button>
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
      empty-text="Nenhuma licença encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-dataInicio="{ value }"><span>{{ formatarData(value as string) }}</span></template>
      <template #cell-dataFim="{ value }"><span>{{ formatarData(value as string) }}</span></template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Aprovar" @click.stop="abrirDecisao(row, 'aprovar')">Aprovar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Rejeitar" @click.stop="abrirDecisao(row, 'rejeitar')">Rejeitar</button>
      </template>
    </DataTable>

    <AppDialog
      v-model="decisaoVisivel"
      :title="tipoDecisao === 'aprovar' ? 'Aprovar licença' : 'Rejeitar licença'"
      width="460px"
      persistent
    >
      <div class="dlg-form">
        <!-- TODO: sem endpoint de listagem para Usuário no digest — UUID manual. -->
        <TextField v-model="decisao.aprovadoPorId" label="Responsável pela decisão (UUID)" required placeholder="UUID" />
        <TextField v-model="decisao.comentario" label="Comentário" maxlength="300" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="decidindo" @click="decisaoVisivel = false">Cancelar</button>
        <button
          type="button"
          class="btn"
          :class="tipoDecisao === 'aprovar' ? 'btn-primary' : 'btn-danger'"
          :disabled="decidindo"
          @click="confirmarDecisao"
        >
          <span v-if="decidindo" class="spinner"></span>
          <span v-else>{{ tipoDecisao === 'aprovar' ? 'Aprovar' : 'Rejeitar' }}</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dlg-form { display: grid; gap: 14px; }
</style>
