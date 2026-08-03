<script setup lang="ts">
/**
 * Contrato GCC — detalhe (erp/compras/contratos-gcc/[id]).
 *
 * Camada de apresentação sobre `GccContratosCompraController`. Tela com abas:
 *   - Contrato: cabeçalho + itens (comprometido/consumido/saldo) + ações de fluxo
 *     (enviar-aprovacao, aprovar);
 *   - Aditivos: registra aditivo (preço/quantidade/vigência/condições);
 *   - Consumo: registra consumo de item do contrato;
 *   - Performance: aderência (consumido/comprometido), saldo e vigência.
 *
 * Endpoints: estoque-gcc-contratos/{id} (GET), {id}/enviar-aprovacao, {id}/aprovar,
 *   /consumos, {id}/aditivos (POST), {id}/performance (GET).
 */
import { computed, onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ middleware: 'auth', layout: 'default' })

interface ContratoItem {
  id: string
  produtoId: string
  precoUnitario: number | null
  quantidadeComprometida: number | null
  quantidadeConsumida: number | null
  saldoQuantidade: number | null
  saldoValor: number | null
}
interface ContratoDetalhe {
  id: string
  fornecedorId: string | null
  numeroContrato: string | null
  vigenciaInicio: string | null
  vigenciaFim: string | null
  valorTotal: number | null
  situacao: number | string | null
  observacao: string | null
  itens: ContratoItem[]
}
interface PerformanceItem {
  produtoId: string
  quantidadeComprometida: number | null
  quantidadeConsumida: number | null
  saldoQuantidade: number | null
  valorComprometido: number | null
  valorConsumido: number | null
  saldoValor: number | null
  aderenciaQuantidadePercent: number | null
}
interface Performance {
  id: string
  situacao: number | string | null
  vigenciaInicio: string | null
  vigenciaFim: string | null
  vigenciaVencida: boolean
  diasParaVencer: number | null
  totalComprometido: number | null
  totalConsumido: number | null
  saldoTotal: number | null
  aderenciaGlobalPercent: number | null
  quantidadeAditivos: number | null
  itens: PerformanceItem[]
}

const route = useRoute()
const toast = useToast()
const { formatarData, formatarMoeda, formatarNumero } = useHelper()

const contratoId = computed(() => String(route.params.id))
const carregando = ref(false)
const acaoEmCurso = ref(false)
const contrato = ref<ContratoDetalhe | null>(null)

const abaAtiva = ref<'contrato' | 'aditivos' | 'consumo' | 'performance'>('contrato')

const SITUACAO_TEXTO: Record<string, string> = {
  '0': 'Rascunho', '1': 'Em Aprovação', '2': 'Aprovado', '3': 'Suspenso',
  '4': 'Encerrado', '5': 'Cancelado', '6': 'Expirado'
}
const situacaoTexto = computed(() => SITUACAO_TEXTO[String(contrato.value?.situacao)] ?? String(contrato.value?.situacao ?? '-'))
const ehRascunho = computed(() => String(contrato.value?.situacao) === '0')
const emAprovacao = computed(() => String(contrato.value?.situacao) === '1')
const aprovado = computed(() => String(contrato.value?.situacao) === '2')

const colunasItens: DataTableColumn<ContratoItem>[] = [
  { key: 'produtoId', label: 'Produto', formatter: (v) => `Produto ${String(v).slice(0, 8)}` },
  { key: 'precoUnitario', label: 'Preço unit.', align: 'right', formatter: (v) => formatarMoeda(v as number | null) },
  { key: 'quantidadeComprometida', label: 'Comprometida', align: 'right', formatter: (v) => formatarNumero(v as number | null) },
  { key: 'quantidadeConsumida', label: 'Consumida', align: 'right', formatter: (v) => formatarNumero(v as number | null) },
  { key: 'saldoQuantidade', label: 'Saldo qtd.', align: 'right', formatter: (v) => formatarNumero(v as number | null) },
  { key: 'saldoValor', label: 'Saldo valor', align: 'right', formatter: (v) => formatarMoeda(v as number | null) }
]

async function carregar() {
  carregando.value = true
  try {
    const resposta = await useApi('/estoque-gcc-contratos/{id}', { params: { id: contratoId.value } })
    contrato.value = extrairDados<ContratoDetalhe>(resposta) ?? null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function enviarAprovacao() {
  acaoEmCurso.value = true
  try {
    await useApi('/estoque-gcc-contratos/{id}/enviar-aprovacao', { method: 'POST', params: { id: contratoId.value } })
    toast.success('Contrato enviado para aprovação')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmCurso.value = false
  }
}

async function aprovar() {
  acaoEmCurso.value = true
  try {
    await useApi('/estoque-gcc-contratos/{id}/aprovar', { method: 'POST', params: { id: contratoId.value } })
    toast.success('Contrato aprovado')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmCurso.value = false
  }
}

// --- Aditivos ---
const TIPO_ADITIVO_OPTIONS: SelectOption[] = [
  { label: 'Preço', value: 0 },
  { label: 'Quantidade', value: 1 },
  { label: 'Vigência', value: 2 },
  { label: 'Condições', value: 3 }
]
const aditivo = ref({
  tipoAditivo: 2 as number,
  numeroAditivo: '',
  justificativa: '',
  dataAditivo: hojeIso(),
  novaVigenciaFim: '',
  novoValorTotal: null as number | null,
  novaObservacao: '',
  contratoCompraItemId: '',
  novoPreco: null as number | null,
  quantidadeAdicional: null as number | null
})
const salvandoAditivo = ref(false)

const itemOptions = computed<SelectOption[]>(() =>
  (contrato.value?.itens ?? []).map((i) => ({ label: `Produto ${String(i.produtoId).slice(0, 8)}`, value: i.id }))
)

async function registrarAditivo() {
  if (!aprovado.value) {
    toast.error('Aditivos só podem ser registrados em contrato aprovado')
    return
  }
  salvandoAditivo.value = true
  try {
    await useApi('/estoque-gcc-contratos/{id}/aditivos', {
      method: 'POST',
      params: { id: contratoId.value },
      body: {
        tipoAditivo: aditivo.value.tipoAditivo,
        numeroAditivo: aditivo.value.numeroAditivo.trim() || null,
        justificativa: aditivo.value.justificativa.trim() || null,
        dataAditivo: aditivo.value.dataAditivo || null,
        novaVigenciaFim: aditivo.value.novaVigenciaFim || null,
        novoValorTotal: aditivo.value.novoValorTotal,
        novaObservacao: aditivo.value.novaObservacao.trim() || null,
        contratoCompraItemId: aditivo.value.contratoCompraItemId || null,
        novoPreco: aditivo.value.novoPreco,
        quantidadeAdicional: aditivo.value.quantidadeAdicional
      }
    })
    toast.success('Aditivo registrado')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoAditivo.value = false
  }
}

// --- Consumo ---
const consumo = ref({ contratoCompraItemId: '', compraId: '', quantidadeConsumida: null as number | null, valorConsumido: null as number | null })
const salvandoConsumo = ref(false)

async function registrarConsumo() {
  if (!consumo.value.contratoCompraItemId) {
    toast.error('Selecione o item do contrato')
    return
  }
  salvandoConsumo.value = true
  try {
    await useApi('/estoque-gcc-contratos/consumos', {
      method: 'POST',
      body: {
        contratoCompraId: contratoId.value,
        contratoCompraItemId: consumo.value.contratoCompraItemId,
        compraId: consumo.value.compraId || null,
        quantidadeConsumida: consumo.value.quantidadeConsumida ?? 0,
        valorConsumido: consumo.value.valorConsumido ?? 0
      }
    })
    toast.success('Consumo registrado')
    consumo.value = { contratoCompraItemId: '', compraId: '', quantidadeConsumida: null, valorConsumido: null }
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoConsumo.value = false
  }
}

// --- Performance ---
const performance = ref<Performance | null>(null)
const carregandoPerf = ref(false)

async function carregarPerformance() {
  carregandoPerf.value = true
  try {
    const resposta = await useApi('/estoque-gcc-contratos/{id}/performance', { params: { id: contratoId.value } })
    performance.value = extrairDados<Performance>(resposta) ?? null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregandoPerf.value = false
  }
}

function trocarAba(aba: typeof abaAtiva.value) {
  abaAtiva.value = aba
  if (aba === 'performance' && !performance.value) void carregarPerformance()
}

function hojeIso(): string {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

function voltar() {
  navigateTo('/erp/compras/contratos-gcc')
}

onMounted(() => void carregar())
</script>

<template>
  <div>
    <PageToolbar
      :title="contrato?.numeroContrato ? `Contrato ${contrato.numeroContrato}` : 'Contrato de Compra'"
      :subtitle="`Contratos GCC · ${situacaoTexto}`"
      :loading="carregando"
    >
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
        <button v-if="ehRascunho" type="button" class="btn btn-primary" :disabled="acaoEmCurso" @click="enviarAprovacao">
          Enviar para aprovação
        </button>
        <button v-if="emAprovacao" type="button" class="btn btn-primary" :disabled="acaoEmCurso" @click="aprovar">
          Aprovar
        </button>
      </template>
    </PageToolbar>

    <div class="tabs">
      <button type="button" class="tab" :class="{ ativa: abaAtiva === 'contrato' }" @click="trocarAba('contrato')">Contrato</button>
      <button type="button" class="tab" :class="{ ativa: abaAtiva === 'aditivos' }" @click="trocarAba('aditivos')">Aditivos</button>
      <button type="button" class="tab" :class="{ ativa: abaAtiva === 'consumo' }" @click="trocarAba('consumo')">Consumo</button>
      <button type="button" class="tab" :class="{ ativa: abaAtiva === 'performance' }" @click="trocarAba('performance')">Performance</button>
    </div>

    <!-- Aba Contrato -->
    <div v-show="abaAtiva === 'contrato'">
      <section class="glass-panel bloco">
        <h3 class="bloco-titulo">Dados do Contrato</h3>
        <div class="form-grid">
          <div class="col-4"><TextField :model-value="contrato?.numeroContrato ?? ''" label="Nº do contrato" readonly /></div>
          <div class="col-4"><TextField :model-value="contrato?.fornecedorId ? `Forn. ${String(contrato.fornecedorId).slice(0, 8)}` : ''" label="Fornecedor" readonly /></div>
          <div class="col-4"><TextField :model-value="situacaoTexto" label="Situação" readonly /></div>
          <div class="col-4"><TextField :model-value="formatarData(contrato?.vigenciaInicio ?? null)" label="Vigência início" readonly /></div>
          <div class="col-4"><TextField :model-value="formatarData(contrato?.vigenciaFim ?? null)" label="Vigência fim" readonly /></div>
          <div class="col-4"><TextField :model-value="formatarMoeda(contrato?.valorTotal ?? null)" label="Valor total" readonly /></div>
          <div class="col-12"><TextField :model-value="contrato?.observacao ?? ''" label="Observação" readonly /></div>
        </div>
      </section>

      <section class="glass-panel bloco itens-bloco">
        <h3 class="bloco-titulo">Itens do Contrato</h3>
        <DataTable
          :items="contrato?.itens ?? []"
          :columns="colunasItens"
          :total="contrato?.itens?.length ?? 0"
          :page="1"
          :page-size="100"
          empty-text="Contrato sem itens"
        />
      </section>
    </div>

    <!-- Aba Aditivos -->
    <div v-show="abaAtiva === 'aditivos'">
      <section class="glass-panel bloco">
        <h3 class="bloco-titulo">Registrar Aditivo</h3>
        <p v-if="!aprovado" class="aviso">Aditivos exigem contrato aprovado.</p>
        <div class="form-grid">
          <div class="col-4"><SelectField v-model="aditivo.tipoAditivo" :options="TIPO_ADITIVO_OPTIONS" label="Tipo de aditivo" :clearable="false" /></div>
          <div class="col-4"><TextField v-model="aditivo.numeroAditivo" label="Nº do aditivo" /></div>
          <div class="col-4"><DateTimeField v-model="aditivo.dataAditivo" label="Data do aditivo" /></div>

          <div class="col-4"><DateTimeField v-model="aditivo.novaVigenciaFim" label="Nova vigência fim" hint="Aditivo de vigência" /></div>
          <div class="col-4"><MoneyInput v-model="aditivo.novoValorTotal" label="Novo valor total" /></div>
          <div class="col-4"><SelectField v-model="aditivo.contratoCompraItemId" :options="itemOptions" label="Item (preço/quantidade)" /></div>

          <div class="col-4"><MoneyInput v-model="aditivo.novoPreco" label="Novo preço" hint="Aditivo de preço" /></div>
          <div class="col-4"><QuantityInput v-model="aditivo.quantidadeAdicional" label="Quantidade adicional" hint="Aditivo de quantidade" /></div>
          <div class="col-4"><TextField v-model="aditivo.novaObservacao" label="Nova observação" hint="Aditivo de condições" /></div>

          <div class="col-12"><TextField v-model="aditivo.justificativa" label="Justificativa" /></div>
        </div>
        <div class="acoes-form">
          <button type="button" class="btn btn-primary" :disabled="salvandoAditivo || !aprovado" @click="registrarAditivo">
            <span v-if="salvandoAditivo" class="spinner"></span>
            <span v-else>Registrar aditivo</span>
          </button>
        </div>
      </section>
    </div>

    <!-- Aba Consumo -->
    <div v-show="abaAtiva === 'consumo'">
      <section class="glass-panel bloco">
        <h3 class="bloco-titulo">Registrar Consumo</h3>
        <div class="form-grid">
          <div class="col-6"><SelectField v-model="consumo.contratoCompraItemId" :options="itemOptions" label="Item do contrato" required /></div>
          <div class="col-6"><TextField v-model="consumo.compraId" label="Compra (ID) — opcional" /></div>
          <div class="col-6"><QuantityInput v-model="consumo.quantidadeConsumida" label="Quantidade consumida" /></div>
          <div class="col-6"><MoneyInput v-model="consumo.valorConsumido" label="Valor consumido" /></div>
        </div>
        <div class="acoes-form">
          <button type="button" class="btn btn-primary" :disabled="salvandoConsumo" @click="registrarConsumo">
            <span v-if="salvandoConsumo" class="spinner"></span>
            <span v-else>Registrar consumo</span>
          </button>
        </div>
      </section>
    </div>

    <!-- Aba Performance -->
    <div v-show="abaAtiva === 'performance'">
      <section v-if="performance" class="cards-resumo">
        <div class="glass-panel card-resumo">
          <span class="card-resumo-label">Aderência global</span>
          <span class="card-resumo-valor">{{ formatarNumero(performance.aderenciaGlobalPercent ?? 0, 0, 2) }}%</span>
        </div>
        <div class="glass-panel card-resumo">
          <span class="card-resumo-label">Total comprometido</span>
          <span class="card-resumo-valor">{{ formatarMoeda(performance.totalComprometido ?? 0) }}</span>
        </div>
        <div class="glass-panel card-resumo">
          <span class="card-resumo-label">Total consumido</span>
          <span class="card-resumo-valor">{{ formatarMoeda(performance.totalConsumido ?? 0) }}</span>
        </div>
        <div class="glass-panel card-resumo">
          <span class="card-resumo-label">Saldo</span>
          <span class="card-resumo-valor">{{ formatarMoeda(performance.saldoTotal ?? 0) }}</span>
        </div>
        <div class="glass-panel card-resumo">
          <span class="card-resumo-label">Vigência</span>
          <span class="card-resumo-valor" :class="{ 'valor-negativo': performance.vigenciaVencida }">
            {{ performance.vigenciaVencida ? 'Vencida' : `${performance.diasParaVencer ?? '-'} dia(s)` }}
          </span>
        </div>
        <div class="glass-panel card-resumo">
          <span class="card-resumo-label">Aditivos</span>
          <span class="card-resumo-valor">{{ performance.quantidadeAditivos ?? 0 }}</span>
        </div>
      </section>
      <p v-else-if="carregandoPerf" class="aviso">Carregando performance...</p>
      <p v-else class="aviso">Sem dados de performance.</p>
    </div>
  </div>
</template>

<style scoped>
.bloco { margin-bottom: 16px; }
section.bloco { padding: 16px; }
.bloco-titulo { font-size: 15px; font-weight: 600; margin: 0 0 12px; }
.tabs { display: flex; gap: 4px; margin-bottom: 16px; border-bottom: 1px solid var(--border-color, rgba(255, 255, 255, 0.1)); }
.tab {
  background: none; border: none; padding: 10px 16px; cursor: pointer;
  color: var(--text-secondary); font-size: 14px; font-weight: 600;
  border-bottom: 2px solid transparent; margin-bottom: -1px;
}
.tab.ativa { color: var(--primary); border-bottom-color: var(--primary); }
.acoes-form { display: flex; justify-content: flex-end; margin-top: 12px; }
.aviso { color: var(--text-secondary); font-size: 13px; margin: 8px 0; }
.cards-resumo { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 12px; }
.card-resumo { padding: 16px 18px; display: flex; flex-direction: column; gap: 6px; }
.card-resumo-label { font-size: 12px; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.4px; }
.card-resumo-valor { font-size: 22px; font-weight: 700; color: var(--text-primary); }
.card-resumo-valor.valor-negativo { color: var(--danger); }
.itens-bloco :deep(.glass-panel) { background: transparent; }
</style>
