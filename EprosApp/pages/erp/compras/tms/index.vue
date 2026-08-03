<script setup lang="ts">
/**
 * TMS — Transporte & Frete de Compra (erp/compras/tms).
 *
 * Camada de apresentação sobre `TmsTransportesController` (`/api/v1/estoque-tms-transportes`):
 *   - listagem paginada (filtro por compra) com situação do transporte;
 *   - criação de transporte (compra + veículo/placa/UF + volumes);
 *   - abrir detalhe (confirmar/cancelar + ratear frete).
 *
 * Obs.: enums são serializados como inteiros pela API (sem JsonStringEnumConverter), portanto
 * UF (EEstado, base 0 na ordem de declaração) é enviada como índice numérico.
 *
 * Endpoints: estoque-tms-transportes (GET, POST).
 */
import { onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import { UF_OPTIONS } from '~/composables/compras/ufOptions'

definePageMeta({ middleware: 'auth', layout: 'default' })

interface TmsTransporte {
  id: string
  compraId: string | null
  situacao: number | string | null
  criadoEm: string | null
}

const STATUS_TEXTO: Record<string, { texto: string; classe: string }> = {
  '0': { texto: 'Rascunho', classe: 'pendente' },
  '1': { texto: 'Confirmado', classe: 'ok' },
  '2': { texto: 'Faturado', classe: 'ok' },
  '3': { texto: 'Cancelado', classe: 'cancelado' },
  '4': { texto: 'Estornado', classe: 'erro' }
}

const toast = useToast()
const { formatarData } = useHelper()

const itens = ref<TmsTransporte[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(20)
const carregando = ref(false)
const filtroCompra = ref('')

const filtrosForm = ref<Record<string, unknown>>({ compraId: '' })
const camposFiltro: FilterField[] = [
  { key: 'compraId', label: 'Compra (ID)', type: 'text', placeholder: 'ID da compra', grow: true }
]

const colunas: DataTableColumn<TmsTransporte>[] = [
  { key: 'compraId', label: 'Compra', formatter: (v) => (v ? `Compra ${String(v).slice(0, 8)}` : '-') },
  { key: 'situacao', label: 'Situação', align: 'center', width: '140px' },
  { key: 'criadoEm', label: 'Criado', width: '130px', formatter: (v) => formatarData(v as string | null) }
]

async function buscar(): Promise<void> {
  carregando.value = true
  try {
    const query: Record<string, unknown> = { pagina: pagina.value, tamanhoPagina: tamanhoPagina.value }
    if (filtroCompra.value.trim()) query.compraId = filtroCompra.value.trim()
    const resposta = await useApi('/estoque-tms-transportes', { query })
    const dados = extrairDados<{ total: number; itens: TmsTransporte[] }>(resposta)
    itens.value = dados?.itens ?? []
    total.value = dados?.total ?? itens.value.length
  } catch (e) {
    itens.value = []
    total.value = 0
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

let deb: ReturnType<typeof setTimeout> | undefined
function aoMudar(v: Record<string, unknown>) {
  filtrosForm.value = v
  if (deb) clearTimeout(deb)
  deb = setTimeout(() => { filtroCompra.value = (v.compraId as string) || ''; pagina.value = 1; void buscar() }, 400)
}
function aoBuscar(v: Record<string, unknown>) {
  if (deb) clearTimeout(deb)
  filtroCompra.value = (v.compraId as string) || ''
  pagina.value = 1
  void buscar()
}
function aoLimpar() {
  filtrosForm.value = { compraId: '' }
  filtroCompra.value = ''
  pagina.value = 1
  void buscar()
}

function abrir(item: TmsTransporte) {
  navigateTo(`/erp/compras/tms/${item.id}`)
}

// Novo transporte
const novaVisivel = ref(false)
const salvando = ref(false)
const nova = ref({ compraId: '', placa: '', uf: 24 as number | null, rntrc: '', quantidadeVolumes: null as number | null, pesoBruto: null as number | null, pesoLiquido: null as number | null })
function abrirNova() {
  nova.value = { compraId: '', placa: '', uf: 24, rntrc: '', quantidadeVolumes: null, pesoBruto: null, pesoLiquido: null }
  novaVisivel.value = true
}
async function salvarNova() {
  if (!nova.value.compraId.trim()) {
    toast.error('Informe a compra (ID)')
    return
  }
  salvando.value = true
  try {
    const veiculo = nova.value.placa.trim()
      ? { veiculoId: null, placa: nova.value.placa.trim(), uf: nova.value.uf ?? 0, rntrc: nova.value.rntrc.trim() || null }
      : null
    const volumes = (nova.value.quantidadeVolumes ?? 0) > 0
      ? [{ quantidadeVolumes: nova.value.quantidadeVolumes, especie: null, numeroVolumes: null, pesoLiquido: nova.value.pesoLiquido ?? 0, pesoBruto: nova.value.pesoBruto ?? 0, marca: null }]
      : []
    await useApi('/estoque-tms-transportes', {
      method: 'POST',
      body: { compraId: nova.value.compraId.trim(), transportadora: null, veiculo, reboques: [], volumes }
    })
    toast.success('Transporte criado')
    novaVisivel.value = false
    await buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

onMounted(() => void buscar())
</script>

<template>
  <div>
    <PageToolbar title="TMS / Frete" subtitle="Transporte de compra e rateio de frete de entrada" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirNova">+ Novo Transporte</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="filtrosForm"
      :loading="carregando"
      @update:model-value="aoMudar"
      @search="aoBuscar"
      @clear="aoLimpar"
    />

    <DataTable
      :items="itens"
      :columns="colunas"
      :total="total"
      :page="pagina"
      :page-size="tamanhoPagina"
      :loading="carregando"
      empty-text="Nenhum transporte encontrado"
      @update:page="(p) => { pagina = p; buscar() }"
      @update:page-size="(ps) => { tamanhoPagina = ps; pagina = 1; buscar() }"
      @row-click="abrir"
    >
      <template #cell-situacao="{ row }">
        <span class="badge" :class="`st-${(STATUS_TEXTO[String(row.situacao)] || {}).classe || 'pendente'}`">
          {{ (STATUS_TEXTO[String(row.situacao)] || {}).texto || row.situacao }}
        </span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Abrir" @click.stop="abrir(row)">➔</button>
      </template>
    </DataTable>

    <AppDialog v-model="novaVisivel" title="Novo Transporte de Compra" width="600px" persistent>
      <div class="form-grid">
        <div class="col-12"><TextField v-model="nova.compraId" label="Compra (ID)" required /></div>
        <div class="col-5"><TextField v-model="nova.placa" label="Placa do veículo (opcional)" /></div>
        <div class="col-4"><SelectField v-model="nova.uf" :options="UF_OPTIONS" label="UF" :clearable="false" /></div>
        <div class="col-3"><TextField v-model="nova.rntrc" label="RNTRC" /></div>
        <div class="col-4"><QuantityInput v-model="nova.quantidadeVolumes" label="Qtd. volumes" :decimais="0" /></div>
        <div class="col-4"><QuantityInput v-model="nova.pesoBruto" label="Peso bruto" /></div>
        <div class="col-4"><QuantityInput v-model="nova.pesoLiquido" label="Peso líquido" /></div>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="novaVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvarNova">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Criar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.badge.st-ok { background: rgba(16, 185, 129, 0.1); border: 1px solid rgba(16, 185, 129, 0.3); color: var(--success); }
.badge.st-erro { background: rgba(239, 68, 68, 0.1); border: 1px solid rgba(239, 68, 68, 0.3); color: var(--danger); }
.badge.st-pendente { background: rgba(245, 158, 11, 0.1); border: 1px solid rgba(245, 158, 11, 0.3); color: var(--warning); }
.badge.st-cancelado { background: rgba(113, 113, 122, 0.1); border: 1px solid rgba(113, 113, 122, 0.3); color: #a1a1aa; }
</style>
