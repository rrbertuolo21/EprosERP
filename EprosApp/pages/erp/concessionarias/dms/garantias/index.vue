<script setup lang="ts">
/**
 * DMS / Garantias — lista de leitura + ação "Julgar" (POST /dms/garantias/{id}/julgar).
 * Não há POST de criação nesta raiz: apenas GET lista + a ação de julgamento.
 * Rota base: /dms/garantias.
 */
import { ref, reactive, onMounted } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ layout: 'default' })

interface GarantiaDms {
  id: string
  // Campos de exibição são um palpite (o digest não expõe o DTO de listagem).
  protocolo?: string | null
  status?: string | null
  veiculoChassi?: string | null
}

interface FiltroBusca {
  busca?: string
}

const toast = useToast()

const lista = useApiList<GarantiaDms, FiltroBusca>('/dms/garantias', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<GarantiaDms>[] = [
  { key: 'protocolo', label: 'Protocolo' },
  { key: 'status', label: 'Status' },
  { key: 'veiculoChassi', label: 'Chassi' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Pesquisar...', grow: true }
]

// ---- Diálogo de julgamento ---------------------------------------------
const dialogoAberto = ref(false)
const julgando = ref(false)
const alvo = ref<GarantiaDms | null>(null)
const julgamento = reactive<{ novoStatus: string | null; parecer: string | null }>({
  novoStatus: null,
  parecer: null
})

function abrirJulgar(row: GarantiaDms) {
  alvo.value = row
  julgamento.novoStatus = null
  julgamento.parecer = null
  dialogoAberto.value = true
}

async function confirmarJulgamento() {
  if (!alvo.value) return
  julgando.value = true
  try {
    await useApi('/dms/garantias/{id}/julgar', {
      method: 'POST',
      params: { id: alvo.value.id },
      body: { novoStatus: julgamento.novoStatus, parecer: julgamento.parecer }
    })
    toast.success('Garantia julgada com sucesso.')
    dialogoAberto.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    julgando.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Garantias (DMS)" subtitle="DMS — julgamento de solicitações de garantia" :loading="lista.carregando.value" />

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
      empty-text="Nenhuma garantia encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Julgar" @click.stop="abrirJulgar(row)">
          Julgar
        </button>
      </template>
    </DataTable>

    <AppDialog v-model="dialogoAberto" title="Julgar garantia" width="520px" persistent>
      <div class="form-grid">
        <!-- Palpite: novoStatus é um texto livre; o digest não expõe o enum de status. -->
        <TextField
          v-model="julgamento.novoStatus"
          label="Novo status"
          hint="Ex.: aprovada, negada."
        />
        <TextField v-model="julgamento.parecer" label="Parecer" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="julgando" @click="dialogoAberto = false">
          Cancelar
        </button>
        <button type="button" class="btn btn-primary" :disabled="julgando" @click="confirmarJulgamento">
          <span v-if="julgando" class="spinner"></span>
          <span v-else>Julgar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 16px;
}
</style>
