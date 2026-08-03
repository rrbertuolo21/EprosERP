<script setup lang="ts">
/**
 * Movimento Manual de Estoque (erp/estoque/movimento-manual).
 * Porta o comportamento de `estoque/movimento-manual.vue` do legado: lista paginada
 * server-side de movimentações manuais já lançadas + botão para abrir o diálogo de
 * novo lançamento (produto livre, buscado por descrição).
 *
 * Endpoint: estoque-movimentos-manuais.
 */
import { onMounted, ref } from 'vue'
import { useApiList } from '~/composables/useApiList'
import { useEnum, type SelectOption } from '~/composables/useEnum'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import MovimentoManualDialog from '~/components/estoque/MovimentoManualDialog.vue'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

interface MovimentoManualListagem {
  id: number
  produtoId: number
  produtoDescricao?: string | null
  tipoEstoque: number | null
  tipoMovimento: number | null
  quantidadeMovimentada: number | null
  valorUnitario: number | null
}

const { carregarOpcoes } = useEnum()
const { formatarNumero, formatarMoeda } = useHelper()

const lista = useApiList<MovimentoManualListagem>('/estoque-movimentos-manuais', { tamanhoPaginaInicial: 25 })

const tipoEstoqueOpcoes = ref<SelectOption[]>([])
const tipoMovimentoOpcoes = ref<SelectOption[]>([])

function labelOpcao(opcoes: SelectOption[], valor: unknown): string {
  if (valor == null) return '-'
  const encontrado = opcoes.find((o) => String(o.value) === String(valor))
  return encontrado?.label ?? String(valor)
}

const colunas: DataTableColumn<MovimentoManualListagem>[] = [
  {
    key: 'produtoId',
    label: 'Produto',
    formatter: (v, row) => row.produtoDescricao ?? `Produto #${v}`
  },
  {
    key: 'tipoEstoque',
    label: 'Tipo Estoque',
    formatter: (v) => labelOpcao(tipoEstoqueOpcoes.value, v)
  },
  {
    key: 'tipoMovimento',
    label: 'Tipo Movimento',
    formatter: (v) => labelOpcao(tipoMovimentoOpcoes.value, v)
  },
  {
    key: 'quantidadeMovimentada',
    label: 'Quantidade Movimentada',
    align: 'right',
    formatter: (v) => formatarNumero(v as number | null, 0, 3)
  },
  {
    key: 'valorUnitario',
    label: 'Valor Unitário',
    align: 'right',
    formatter: (v) => formatarMoeda(v as number | null)
  }
]

const dialogAberto = ref(false)

function abrirDialog() {
  dialogAberto.value = true
}

async function aoMovimentoCriado() {
  await lista.buscar()
}

async function carregarEnums() {
  try {
    const [tiposEstoque, tiposMovimento] = await Promise.all([
      carregarOpcoes('/estoque-enums/tipo-estoque'),
      carregarOpcoes('/dfe-enums/tipo-movimento')
    ])
    tipoEstoqueOpcoes.value = tiposEstoque
    tipoMovimentoOpcoes.value = tiposMovimento
  } catch (e) {
    console.error('[estoque/movimento-manual] falha ao carregar enums', e)
  }
}

onMounted(async () => {
  await carregarEnums()
  await lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Movimento Manual" subtitle="Lançamentos manuais de entrada/saída de estoque" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirDialog">+ Adicionar Movimento</button>
      </template>
    </PageToolbar>

    <div v-if="lista.erro.value" class="erro-listagem">{{ lista.erro.value }}</div>

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      empty-text="Nenhuma movimentação encontrada"
      @update:page="(p) => lista.irParaPagina(p)"
      @update:page-size="(ps) => lista.buscar({ tamanhoPagina: ps, pagina: 1 })"
    />

    <MovimentoManualDialog v-model="dialogAberto" @created="aoMovimentoCriado" />
  </div>
</template>

<style scoped>
.erro-listagem {
  margin: 0 12px 12px;
  padding: 10px 14px;
  border-radius: 8px;
  background: rgba(220, 53, 69, 0.12);
  border: 1px solid rgba(220, 53, 69, 0.35);
  color: var(--danger, #dc3545);
  font-size: 13px;
}
</style>
