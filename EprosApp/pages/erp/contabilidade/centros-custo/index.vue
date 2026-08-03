<script setup lang="ts">
/**
 * Listagem de Centros de Custo — Contabilidade Gerencial / Centros de Custo.
 *
 * Contrato:
 *   GET    /contabilidade-gerencial/centros-custo   (lista paginada)
 *   DELETE /contabilidade-gerencial/centros-custo/{id}
 * O endpoint de lista não expõe filtros (só paginação).
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import { estadoCentroCustoLabel } from '~/components/contabilidade-contas/enums'

definePageMeta({ layout: 'default' })

interface CentroCusto {
  id: string
  codigo?: string | null
  descricao?: string | null
  paiId?: string | null
  estado: number
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<CentroCusto>('/contabilidade-gerencial/centros-custo', {
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<CentroCusto>[] = [
  { key: 'codigo', label: 'Código', sortable: false, width: '160px' },
  { key: 'descricao', label: 'Descrição', sortable: false },
  { key: 'estado', label: 'Estado', sortable: false, align: 'center', width: '120px' }
]

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<CentroCusto | null>(null)

function novo() {
  router.push('/erp/contabilidade/centros-custo/novo')
}

function editar(item: CentroCusto) {
  router.push(`/erp/contabilidade/centros-custo/${item.id}`)
}

function pedirExclusao(item: CentroCusto) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi(`/contabilidade-gerencial/centros-custo/{id}`, { method: 'DELETE', params: { id: itemParaExcluir.value.id } })
    toast.success('Centro de custo excluído com sucesso.')
    excluirVisivel.value = false
    itemParaExcluir.value = null
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar
      title="Centros de Custo"
      subtitle="Estrutura de centros de custo da contabilidade gerencial"
      :loading="lista.carregando.value"
    >
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo centro de custo</button>
      </template>
    </PageToolbar>

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhum centro de custo encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @row-click="editar"
    >
      <template #cell-estado="{ value }">
        <span class="badge" :class="Number(value) === 0 ? 'badge-success' : 'badge-secondary'">
          {{ estadoCentroCustoLabel(Number(value)) }}
        </span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editar(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Excluir" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.descricao || itemParaExcluir?.codigo || undefined"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>
