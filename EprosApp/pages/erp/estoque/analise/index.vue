<script setup lang="ts">
/**
 * Análise e Planejamento de Estoque (erp/estoque/analise).
 * Duas abas (AnalisePlanejamentoEstoqueController):
 *   Posições: GET /estoque-analise/posicoes?produtoId&apenasEmAlerta — disponível (saldo−reservado)
 *             + status de planejamento; ação Parametrizar (PUT /posicoes/{id}/parametros: mín/máx/custeio).
 *   Alertas:  GET /estoque-analise/alertas?status&tipo — alertas de reposição/excesso; resolver/ignorar
 *             (POST /alertas/{id}/resolver?ignorar) e reprocessar (POST /alertas/reprocessar).
 */
import { onMounted, reactive, ref, watch } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import { useEstoqueEnums, classeBadge } from '~/composables/useEstoqueEnums'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

const toast = useToast()
const { formatarData, formatarNumero, formatarMoeda } = useHelper()
const { tipoCusteio, statusPlanejamento, tipoAlerta, statusAlerta } = useEstoqueEnums()

type Aba = 'posicoes' | 'alertas'
const aba = ref<Aba>('posicoes')
const processando = ref(false)

// --------- Posições ---------
interface Posicao {
  id: string
  empresaId: string
  produtoId: string
  quantidadeSaldoEstoque: number
  quantidadeEstoqueMinimo: number
  quantidadeEstoqueMaximo: number
  quantidadeEstoqueReservado: number
  quantidadeDisponivel: number
  valorSaldo: number
  valorCustoMedio: number
  tipoCusteioEstoque: number
  statusPlanejamento: number
}
interface PosicaoFiltros extends Record<string, unknown> { produtoId?: string; apenasEmAlerta?: boolean }

const listaPos = useApiList<Posicao, PosicaoFiltros>('/estoque-analise/posicoes', { filtrosIniciais: { apenasEmAlerta: false }, tamanhoPaginaInicial: 25 })
const camposPos: FilterField[] = [
  { key: 'produtoId', label: 'Produto (ID)', type: 'text', placeholder: 'GUID do produto', grow: true },
  { key: 'apenasEmAlerta', label: 'Apenas em alerta', type: 'boolean', placeholder: 'Somente em alerta' }
]
const colunasPos: DataTableColumn<Posicao>[] = [
  { key: 'produtoId', label: 'Produto (ID)' },
  { key: 'quantidadeSaldoEstoque', label: 'Saldo', align: 'right', formatter: (v) => formatarNumero(v as number, 0, 4) },
  { key: 'quantidadeDisponivel', label: 'Disponível', align: 'right', formatter: (v) => formatarNumero(v as number, 0, 4) },
  { key: 'quantidadeEstoqueMinimo', label: 'Mínimo', align: 'right', formatter: (v) => formatarNumero(v as number, 0, 4) },
  { key: 'quantidadeEstoqueMaximo', label: 'Máximo', align: 'right', formatter: (v) => formatarNumero(v as number, 0, 4) },
  { key: 'valorSaldo', label: 'Valor', align: 'right', formatter: (v) => formatarMoeda(v as number) },
  { key: 'statusPlanejamento', label: 'Planejamento', align: 'center', width: '160px' }
]
function normalizarPos(v: Record<string, unknown>): Partial<PosicaoFiltros> {
  return { produtoId: (v.produtoId as string) || undefined, apenasEmAlerta: Boolean(v.apenasEmAlerta) }
}

// --------- Parametrizar (dialog) ---------
const paramDialog = ref(false)
const posSelecionada = ref<Posicao | null>(null)
const formParam = reactive({ quantidadeEstoqueMinimo: 0 as number | null, quantidadeEstoqueMaximo: 0 as number | null, tipoCusteioEstoque: 0 as number, justificativa: '' })
function abrirParametrizar(p: Posicao) {
  posSelecionada.value = p
  formParam.quantidadeEstoqueMinimo = p.quantidadeEstoqueMinimo
  formParam.quantidadeEstoqueMaximo = p.quantidadeEstoqueMaximo
  formParam.tipoCusteioEstoque = p.tipoCusteioEstoque
  formParam.justificativa = ''
  paramDialog.value = true
}
async function salvarParametros() {
  if (!posSelecionada.value) return
  processando.value = true
  try {
    await useApi('/estoque-analise/posicoes/{id}/parametros', {
      method: 'PUT',
      params: { id: posSelecionada.value.id },
      body: {
        estoqueProdutoId: posSelecionada.value.id,
        quantidadeEstoqueMinimo: Number(formParam.quantidadeEstoqueMinimo ?? 0),
        quantidadeEstoqueMaximo: Number(formParam.quantidadeEstoqueMaximo ?? 0),
        tipoCusteioEstoque: formParam.tipoCusteioEstoque,
        justificativa: formParam.justificativa.trim() || null
      }
    })
    toast.success('Parâmetros atualizados.')
    paramDialog.value = false
    await listaPos.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}

// --------- Alertas ---------
interface Alerta {
  id: string
  empresaId: string
  produtoId: string
  tipoAlerta: number
  quantidadeReferencia: number
  quantidadeAtual: number
  statusAlerta: number
  dataAlerta: string
}
interface AlertaFiltros extends Record<string, unknown> { status?: number | null; tipo?: number | null }

const listaAlertas = useApiList<Alerta, AlertaFiltros>('/estoque-analise/alertas', { tamanhoPaginaInicial: 25 })
const camposAlerta: FilterField[] = [
  { key: 'status', label: 'Status', type: 'select', options: statusAlerta.opcoes, grow: true },
  { key: 'tipo', label: 'Tipo', type: 'select', options: tipoAlerta.opcoes }
]
const colunasAlerta: DataTableColumn<Alerta>[] = [
  { key: 'dataAlerta', label: 'Data', width: '150px', formatter: (v) => formatarData(v as string) },
  { key: 'produtoId', label: 'Produto (ID)' },
  { key: 'tipoAlerta', label: 'Tipo', align: 'center', width: '150px' },
  { key: 'quantidadeAtual', label: 'Atual', align: 'right', formatter: (v) => formatarNumero(v as number, 0, 4) },
  { key: 'quantidadeReferencia', label: 'Referência', align: 'right', formatter: (v) => formatarNumero(v as number, 0, 4) },
  { key: 'statusAlerta', label: 'Status', align: 'center', width: '120px' }
]
function normalizarAlerta(v: Record<string, unknown>): Partial<AlertaFiltros> {
  return {
    status: v.status === '' || v.status == null ? undefined : Number(v.status),
    tipo: v.tipo === '' || v.tipo == null ? undefined : Number(v.tipo)
  }
}
async function resolverAlerta(a: Alerta, ignorar: boolean) {
  processando.value = true
  try {
    await useApi('/estoque-analise/alertas/{id}/resolver', { method: 'POST', params: { id: a.id }, query: { ignorar } })
    toast.success(ignorar ? 'Alerta ignorado.' : 'Alerta resolvido.')
    await listaAlertas.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}
async function reprocessar() {
  processando.value = true
  try {
    await useApi('/estoque-analise/alertas/reprocessar', { method: 'POST' })
    toast.success('Alertas reprocessados.')
    await listaAlertas.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}

watch(aba, (a) => {
  if (a === 'alertas' && listaAlertas.itens.value.length === 0) void listaAlertas.buscar()
})

onMounted(() => void listaPos.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Análise e Planejamento" subtitle="Posição de planejamento, parametrização de mín/máx e alertas de reposição/excesso" :loading="listaPos.carregando.value || listaAlertas.carregando.value || processando">
      <template #actions>
        <button v-if="aba === 'alertas'" type="button" class="btn btn-primary" :disabled="processando" @click="reprocessar">Reprocessar alertas</button>
      </template>
    </PageToolbar>

    <div class="tabs">
      <button type="button" class="tab" :class="{ ativo: aba === 'posicoes' }" @click="aba = 'posicoes'">Posições</button>
      <button type="button" class="tab" :class="{ ativo: aba === 'alertas' }" @click="aba = 'alertas'">Alertas</button>
    </div>

    <!-- Posições -->
    <template v-if="aba === 'posicoes'">
      <FilterBar
        :fields="camposPos"
        :model-value="listaPos.filtros.value"
        :loading="listaPos.carregando.value"
        @update:model-value="(v) => (listaPos.filtros.value = v as typeof listaPos.filtros.value)"
        @search="(v) => listaPos.aplicarFiltros(normalizarPos(v as Record<string, unknown>))"
        @clear="listaPos.limpar()"
      />
      <DataTable
        :items="listaPos.itens.value"
        :columns="colunasPos"
        :total="listaPos.total.value"
        :page="listaPos.pagina.value"
        :page-size="listaPos.tamanhoPagina.value"
        :loading="listaPos.carregando.value"
        row-key="id"
        empty-text="Nenhuma posição encontrada"
        @update:page="(p) => listaPos.irParaPagina(p)"
        @update:page-size="(ps) => listaPos.buscar({ tamanhoPagina: ps, pagina: 1 })"
      >
        <template #cell-statusPlanejamento="{ value }">
          <span class="badge" :class="classeBadge(statusPlanejamento.cor(value as number))">{{ statusPlanejamento.label(value as number) }}</span>
        </template>
        <template #actions="{ row }">
          <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrirParametrizar(row)">Parametrizar</button>
        </template>
      </DataTable>
    </template>

    <!-- Alertas -->
    <template v-else>
      <FilterBar
        :fields="camposAlerta"
        :model-value="listaAlertas.filtros.value"
        :loading="listaAlertas.carregando.value"
        @update:model-value="(v) => (listaAlertas.filtros.value = v as typeof listaAlertas.filtros.value)"
        @search="(v) => listaAlertas.aplicarFiltros(normalizarAlerta(v as Record<string, unknown>))"
        @clear="listaAlertas.limpar()"
      />
      <DataTable
        :items="listaAlertas.itens.value"
        :columns="colunasAlerta"
        :total="listaAlertas.total.value"
        :page="listaAlertas.pagina.value"
        :page-size="listaAlertas.tamanhoPagina.value"
        :loading="listaAlertas.carregando.value"
        row-key="id"
        empty-text="Nenhum alerta encontrado"
        @update:page="(p) => listaAlertas.irParaPagina(p)"
        @update:page-size="(ps) => listaAlertas.buscar({ tamanhoPagina: ps, pagina: 1 })"
      >
        <template #cell-tipoAlerta="{ value }">
          <span class="badge" :class="classeBadge(tipoAlerta.cor(value as number))">{{ tipoAlerta.label(value as number) }}</span>
        </template>
        <template #cell-statusAlerta="{ value }">
          <span class="badge" :class="classeBadge(statusAlerta.cor(value as number))">{{ statusAlerta.label(value as number) }}</span>
        </template>
        <template #actions="{ row }">
          <template v-if="row.statusAlerta === 0">
            <button type="button" class="btn btn-ghost btn-sm" :disabled="processando" @click.stop="resolverAlerta(row, false)">Resolver</button>
            <button type="button" class="btn btn-ghost btn-sm" :disabled="processando" @click.stop="resolverAlerta(row, true)">Ignorar</button>
          </template>
        </template>
      </DataTable>
    </template>

    <!-- Dialog parametrizar -->
    <AppDialog v-model="paramDialog" title="Parametrizar posição" width="520px">
      <p v-if="posSelecionada" class="dialog-sub">Produto: <strong>{{ posSelecionada.produtoId }}</strong></p>
      <div class="form-grid">
        <QuantityInput v-model="formParam.quantidadeEstoqueMinimo" label="Estoque mínimo" :decimais="4" />
        <QuantityInput v-model="formParam.quantidadeEstoqueMaximo" label="Estoque máximo" :decimais="4" />
        <SelectField v-model="formParam.tipoCusteioEstoque" label="Tipo de custeio" :options="tipoCusteio.opcoes" :clearable="false" />
      </div>
      <TextField v-model="formParam.justificativa" label="Justificativa" hint="Opcional (registrada na auditoria)" />
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="processando" @click="paramDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="processando" @click="salvarParametros">
          <span v-if="processando" class="spinner"></span><span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.tabs { display: flex; gap: 4px; margin: 8px 0 12px; }
.tab { padding: 8px 16px; border: 1px solid var(--border-color); background: transparent; color: var(--text-secondary); border-radius: 8px; cursor: pointer; font-size: 13px; }
.tab.ativo { background: var(--primary); color: #fff; border-color: var(--primary); }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; margin-bottom: 12px; }
.dialog-sub { font-size: 13px; color: var(--text-secondary); margin-bottom: 12px; }
</style>
