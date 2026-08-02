<script setup lang="ts">
/**
 * Listagem de CFOP — Fiscal / CFOP.
 *
 * Porta o comportamento de `fiscal/cfop/index.vue` do legado: busca por código/descrição,
 * tabela paginada server-side com os indicadores principais e ação de importar a partir
 * da tabela padrão de CFOPs (`cfop-padrao`).
 * Endpoints: `cfops`, `cfop-padrao`.
 */
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi, extrairDados } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'

definePageMeta({ layout: 'default' })

interface Cfop {
  id: string
  cfopCodigo: number
  descricao: string
  naturezaOperacao: string
  cfopCorrelacao?: string | null
  cfopDevolucao?: string | null
  integraFaturamento: boolean
  indicadorNfe: boolean
  indicadorNfce: boolean
  indicadorComunicacao: boolean
  indicadorTransporte: boolean
  indicadorDevolucao: boolean
  indicadorRetorno: boolean
  indicadorAnulacao: boolean
  indicadorRemessa: boolean
  indicadorCombustivel: boolean
  indicadorTransferencia: boolean
  indicadorCiap: boolean
  indicadorUsoConsumo: boolean
  indicadorUsoSemOperacao: boolean
  indicadorSt: boolean
  indicadorMei: boolean
  incidenciaSimples?: string | null
}

interface CfopFiltros {
  localizar?: string
}

interface CfopPadrao {
  id: string
  cfopCodigo: number
  descricao: string
  naturezaOperacao: string
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<Cfop, CfopFiltros>('/cfops', {
  filtrosIniciais: { localizar: '' },
  tamanhoPaginaInicial: 25
})

// Colunas de indicador booleano exibidas como badge Sim/Não (mesmo padrão do legado: 19 colunas no total).
const colunasIndicador: { key: keyof Cfop; label: string }[] = [
  { key: 'integraFaturamento', label: 'Integra Fat.' },
  { key: 'indicadorNfe', label: 'NF-e' },
  { key: 'indicadorNfce', label: 'NFC-e' },
  { key: 'indicadorComunicacao', label: 'Comunicação' },
  { key: 'indicadorTransporte', label: 'Transporte' },
  { key: 'indicadorDevolucao', label: 'Devolução' },
  { key: 'indicadorRetorno', label: 'Retorno' },
  { key: 'indicadorAnulacao', label: 'Anulação' },
  { key: 'indicadorRemessa', label: 'Remessa' },
  { key: 'indicadorCombustivel', label: 'Combustível' },
  { key: 'indicadorTransferencia', label: 'Transferência' },
  { key: 'indicadorCiap', label: 'CIAP' },
  { key: 'indicadorUsoConsumo', label: 'Uso/Consumo' },
  { key: 'indicadorUsoSemOperacao', label: 'Uso s/ Operação' },
  { key: 'indicadorSt', label: 'ST' },
  { key: 'indicadorMei', label: 'MEI' }
]

const colunas: DataTableColumn<Cfop>[] = [
  { key: 'cfopCodigo', label: 'CFOP', sortable: true, width: '90px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'naturezaOperacao', label: 'Natureza da Operação', sortable: false },
  ...colunasIndicador.map((c) => ({ key: c.key as string, label: c.label, sortable: false, align: 'center' as const, width: '90px' }))
]

const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'Código ou descrição...', grow: true }
]

async function buscarPagina() {
  await lista.buscar()
}

function novoCfop() {
  router.push('/erp/fiscal/cfop/novo')
}

function editarCfop(item: Cfop) {
  router.push(`/erp/fiscal/cfop/${item.id}`)
}

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<Cfop | null>(null)

function pedirExclusao(item: Cfop) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi(`/cfops/${itemParaExcluir.value.id}`, { method: 'DELETE' })
    toast.success('CFOP excluído com sucesso.')
    excluirVisivel.value = false
    itemParaExcluir.value = null
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

// ---- Modal "Selecionar CFOPs da tabela padrão" (porta o fluxo de importação do legado) ----
const dialogPadraoVisivel = ref(false)
const carregandoPadrao = ref(false)
const buscaPadrao = reactive({ localizar: '' })
const cfopsPadrao = ref<CfopPadrao[]>([])

async function abrirModalPadrao() {
  dialogPadraoVisivel.value = true
  await buscarCfopsPadrao()
}

async function buscarCfopsPadrao() {
  carregandoPadrao.value = true
  try {
    const resposta = await useApi('/cfop-padrao', {
      query: { localizar: buscaPadrao.localizar || undefined, tamanhoPagina: 100 }
    })
    cfopsPadrao.value = extrairDados<CfopPadrao[]>(resposta) ?? []
  } catch (e) {
    toast.error(obterMensagemErro(e))
    cfopsPadrao.value = []
  } finally {
    carregandoPadrao.value = false
  }
}

function usarCfopPadrao(item: CfopPadrao) {
  dialogPadraoVisivel.value = false
  router.push({ path: '/erp/fiscal/cfop/novo', query: { origemCodigo: String(item.cfopCodigo) } })
}

onMounted(() => {
  void buscarPagina()
})
</script>

<template>
  <div>
    <PageToolbar title="CFOP" subtitle="Códigos Fiscais de Operações e Prestações" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="abrirModalPadrao">Importar da tabela padrão</button>
        <button type="button" class="btn btn-primary" @click="novoCfop">+ Novo CFOP</button>
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
      empty-text="Nenhum CFOP cadastrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="editarCfop"
    >
      <template v-for="col in colunasIndicador" :key="col.key" #[`cell-${col.key}`]="{ value }">
        <span class="chip" :class="value ? 'chip-success' : 'chip-danger'">{{ value ? 'Sim' : 'Não' }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="editarCfop(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir ? `${itemParaExcluir.cfopCodigo} - ${itemParaExcluir.descricao}` : undefined"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />

    <AppDialog v-model="dialogPadraoVisivel" title="Selecionar CFOP da tabela padrão" width="720px">
      <div class="busca-padrao">
        <input
          v-model="buscaPadrao.localizar"
          class="input"
          placeholder="Código ou descrição..."
          @keyup.enter="buscarCfopsPadrao"
        />
        <button type="button" class="btn btn-primary btn-sm" @click="buscarCfopsPadrao">Buscar</button>
      </div>
      <div v-if="carregandoPadrao" class="table-loading"><span class="spinner"></span> Carregando...</div>
      <table v-else class="admin-table">
        <thead>
          <tr>
            <th>CFOP</th>
            <th>Descrição</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="cfopsPadrao.length === 0">
            <td colspan="3" class="table-empty">Nenhum CFOP padrão encontrado.</td>
          </tr>
          <tr v-for="item in cfopsPadrao" :key="item.id">
            <td>{{ item.cfopCodigo }}</td>
            <td>{{ item.descricao }}</td>
            <td class="td-right">
              <button type="button" class="btn btn-ghost btn-sm" @click="usarCfopPadrao(item)">Usar</button>
            </td>
          </tr>
        </tbody>
      </table>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="dialogPadraoVisivel = false">Fechar</button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.busca-padrao { display: flex; gap: 8px; margin-bottom: 12px; }
.busca-padrao .input { flex: 1; }
</style>
