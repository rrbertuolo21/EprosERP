<script setup lang="ts">
/**
 * Listagem do Plano de Contas Contábil — Contabilidade Geral / Contas.
 *
 * Contrato da API (contabilidade-geral):
 *   GET  /contabilidade-geral/contas        (lista paginada { total, pagina, tamanho, itens })
 *   DELETE /contabilidade-geral/contas/{id}
 * O endpoint de lista NÃO expõe filtros (só paginação), por isso não há FilterBar.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import { tipoContaLabel } from '~/components/contabilidade-contas/enums'

definePageMeta({ layout: 'default' })

interface ContaContabil {
  id: string
  codigoConta?: string | null
  nomeConta?: string | null
  contaPaiId?: string | null
  nivel: number
  tipoConta: number
  aceitaLancamento: boolean
  ativo: boolean
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<ContaContabil>('/contabilidade-geral/contas', {
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<ContaContabil>[] = [
  { key: 'codigoConta', label: 'Código', sortable: false, width: '140px' },
  { key: 'nomeConta', label: 'Nome da Conta', sortable: false },
  { key: 'nivel', label: 'Nível', sortable: false, align: 'center', width: '80px' },
  { key: 'tipoConta', label: 'Tipo', sortable: false, width: '130px' },
  { key: 'aceitaLancamento', label: 'Aceita Lçto.', sortable: false, align: 'center', width: '110px' },
  { key: 'ativo', label: 'Status', sortable: false, align: 'center', width: '110px' }
]

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<ContaContabil | null>(null)

function novaConta() {
  router.push('/erp/contabilidade/contas/novo')
}

function editarConta(item: ContaContabil) {
  router.push(`/erp/contabilidade/contas/${item.id}`)
}

function pedirExclusao(item: ContaContabil) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi(`/contabilidade-geral/contas/{id}`, { method: 'DELETE', params: { id: itemParaExcluir.value.id } })
    toast.success('Conta contábil excluída com sucesso.')
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
      title="Plano de Contas Contábil"
      subtitle="Contas contábeis da contabilidade geral (estrutura, tipo e níveis)"
      :loading="lista.carregando.value"
    >
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novaConta">+ Nova conta</button>
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
      empty-text="Nenhuma conta contábil encontrada. Adicione uma nova conta para começar."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @row-click="editarConta"
    >
      <template #cell-tipoConta="{ value }">
        <span class="badge badge-info">{{ tipoContaLabel(Number(value)) }}</span>
      </template>
      <template #cell-aceitaLancamento="{ value }">
        <span class="badge" :class="value ? 'badge-success' : 'badge-secondary'">
          {{ value ? 'Sim' : 'Não' }}
        </span>
      </template>
      <template #cell-ativo="{ value }">
        <span class="badge" :class="value ? 'badge-success' : 'badge-danger'">
          {{ value ? 'Ativa' : 'Inativa' }}
        </span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editarConta(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Excluir" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.nomeConta || itemParaExcluir?.codigoConta || undefined"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>
