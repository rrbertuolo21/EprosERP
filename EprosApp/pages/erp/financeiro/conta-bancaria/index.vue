<script setup lang="ts">
/**
 * Listagem de Contas Bancárias — Financeiro / Cadastros auxiliares.
 *
 * Porta o comportamento de `financeiro/conta-bancaria/index.vue` do legado:
 * tabela paginada, resolução de nome do banco e tipo de conta, editar/excluir.
 *
 * Observação: a rota `/contas-bancarias` devolve a paginação dentro de
 * `dados = { total, pagina, itens }`, fora do padrão `useApiList` (que espera `dados`
 * como array e `totalRegistros` no topo do envelope). Por isso a listagem é feita
 * manualmente aqui, mantendo o mesmo par DataTable+FilterBar das demais telas.
 */
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { obterMensagemErro } from '~/composables/useApiList'
import { useApi, extrairDados } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'

definePageMeta({ layout: 'default' })

interface ContaBancaria {
  id: string
  bancoId: string
  apelido: string
  titular: string
  agencia: string
  conta: string
  tipoContaBancaria: number
}

interface Banco {
  id: string
  codigo: string
  descricao: string
}

const TIPOS_CONTA: Record<number, string> = {
  1: 'Conta Corrente',
  2: 'Conta Poupança',
  3: 'Aplicações',
  4: 'Outras'
}

const router = useRouter()
const toast = useToast()

const itens = ref<ContaBancaria[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(25)
const carregando = ref(false)
const filtros = reactive<{ localizar: string }>({ localizar: '' })

const bancos = ref<Banco[]>([])

const colunas: DataTableColumn<ContaBancaria>[] = [
  { key: 'apelido', label: 'Apelido', sortable: false },
  { key: 'bancoId', label: 'Banco', sortable: false },
  { key: 'tipoContaBancaria', label: 'Tipo', sortable: false },
  { key: 'titular', label: 'Titular', sortable: false },
  { key: 'agencia', label: 'Agência', sortable: false, width: '110px' },
  { key: 'conta', label: 'Conta', sortable: false, width: '130px' }
]

const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'Apelido, titular, agência ou conta...', grow: true }
]

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<ContaBancaria | null>(null)

function nomeBanco(bancoId: string): string {
  return bancos.value.find((b) => b.id === bancoId)?.descricao ?? ''
}

function tipoConta(tipo: number): string {
  return TIPOS_CONTA[tipo] ?? ''
}

async function carregarBancos() {
  try {
    const resposta = await useApi<{ dados?: { itens: Banco[] } }>('/bancos', { query: { tamanhoPagina: 200 } })
    const dados = extrairDados<{ itens: Banco[] }>(resposta)
    bancos.value = dados?.itens ?? []
  } catch (e) {
    console.error('[conta-bancaria/index] bancos', e)
    bancos.value = []
  }
}

async function buscar() {
  carregando.value = true
  try {
    const resposta = await useApi<{ dados?: { total: number; itens: ContaBancaria[] } }>('/contas-bancarias', {
      query: { localizar: filtros.localizar || undefined, pagina: pagina.value, tamanhoPagina: tamanhoPagina.value }
    })
    const dados = extrairDados<{ total: number; itens: ContaBancaria[] }>(resposta)
    itens.value = dados?.itens ?? []
    total.value = dados?.total ?? 0
  } catch (e) {
    toast.error(obterMensagemErro(e))
    itens.value = []
    total.value = 0
  } finally {
    carregando.value = false
  }
}

function aplicarFiltros(v: Record<string, unknown>) {
  filtros.localizar = (v.localizar as string) ?? ''
  pagina.value = 1
  void buscar()
}

function limparFiltros() {
  filtros.localizar = ''
  pagina.value = 1
  void buscar()
}

function irParaPagina(p: number) {
  pagina.value = p
  void buscar()
}

function mudarTamanhoPagina(tam: number) {
  tamanhoPagina.value = tam
  pagina.value = 1
  void buscar()
}

function novaConta() {
  router.push('/erp/financeiro/conta-bancaria/novo')
}

function editarConta(item: ContaBancaria) {
  router.push(`/erp/financeiro/conta-bancaria/${item.id}`)
}

function pedirExclusao(item: ContaBancaria) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi('/contas-bancarias/{id}', { method: 'DELETE', params: { id: itemParaExcluir.value.id } })
    toast.success('Conta bancária excluída com sucesso.')
    excluirVisivel.value = false
    itemParaExcluir.value = null
    await buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

onMounted(async () => {
  await carregarBancos()
  await buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Contas Bancárias" subtitle="Contas bancárias vinculadas às empresas" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novaConta">+ Nova conta</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="filtros"
      :loading="carregando"
      @update:model-value="(v) => (filtros.localizar = (v.localizar as string) ?? '')"
      @search="aplicarFiltros"
      @clear="limparFiltros"
    />

    <DataTable
      :items="itens"
      :columns="colunas"
      :total="total"
      :page="pagina"
      :page-size="tamanhoPagina"
      :loading="carregando"
      empty-text="Nenhuma conta bancária encontrada. Adicione uma nova conta para começar."
      @update:page="irParaPagina"
      @update:page-size="mudarTamanhoPagina"
      @row-click="editarConta"
    >
      <template #cell-bancoId="{ value }">
        {{ nomeBanco(String(value)) }}
      </template>
      <template #cell-tipoContaBancaria="{ value }">
        {{ tipoConta(Number(value)) }}
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editarConta(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Excluir" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.apelido"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>
