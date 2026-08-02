<script setup lang="ts">
/**
 * Devolução de Compra — detalhe/criação (erp/compras/devolucao/[id]).
 *
 * Camada de apresentação sobre `DevolucoesCompraController`:
 *   - `novo?origem=<compraId>`: monta a devolução a partir da compra de origem (itens
 *     semeados da nota), POST compras-devolucoes;
 *   - `<id>`: exibe a devolução e permite confirmar/cancelar.
 *
 * Nota: a criação exige FornecedorId (Guid). A projeção da compra (GET /compras/{id}) só
 * expõe CNPJ/nome do fornecedor, não o Guid — por isso o campo Fornecedor (ID) é manual.
 * Falta anotada no relatório.
 *
 * Endpoints: compras/{id} (GET), compras-devolucoes (POST), compras-devolucoes/{id} (GET),
 *   {id}/confirmar, {id}/cancelar (POST).
 */
import { computed, onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ middleware: 'auth', layout: 'default' })

interface LinhaItem {
  produtoId: string
  descricao: string
  compraItemOrigemId: string | null
  quantidade: number | null
  valorUnitario: number | null
}

const route = useRoute()
const toast = useToast()
const { formatarMoeda } = useHelper()

const rotaId = computed(() => {
  const p = route.params.id
  const v = Array.isArray(p) ? p[0] : p
  return v && v !== 'novo' ? String(v) : null
})
const isEdit = computed(() => rotaId.value != null)
const origemId = computed(() => (route.query.origem ? String(route.query.origem) : null))

const TIPO_OPTIONS: SelectOption[] = [{ label: 'Total', value: 0 }, { label: 'Parcial', value: 1 }]
const STATUS_TEXTO: Record<string, string> = { '0': 'Rascunho', '1': 'Confirmada', '2': 'Cancelada' }

const carregando = ref(false)
const salvando = ref(false)
const acaoEmCurso = ref(false)
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

// Cabeçalho
const compraOrigemId = ref('')
const fornecedorId = ref('')
const fornecedorNome = ref('')
const numero = ref('')
const dataDevolucao = ref(hojeIso())
const tipo = ref<number>(0)
const motivo = ref('')
const cfop = ref('')
const statusDevolucao = ref<string | number | null>(null)
const linhas = ref<LinhaItem[]>([])

const totalDevolucao = computed(() =>
  linhas.value.reduce((acc, l) => acc + (l.quantidade ?? 0) * (l.valorUnitario ?? 0), 0)
)
const statusTexto = computed(() => STATUS_TEXTO[String(statusDevolucao.value)] ?? '-')
const ehRascunho = computed(() => String(statusDevolucao.value) === '0')
const somenteLeitura = computed(() => isEdit.value)

// Carregar compra de origem (modo novo)
async function carregarCompraOrigem(id: string) {
  carregando.value = true
  try {
    const resposta = await useApi('/compras/{id}', { params: { id } })
    const compra = extrairDados<{
      id: string
      fornecedorNome?: string
      numeroNota?: string
      itens?: { id: string; produtoId?: string; codigoProduto?: string; descricaoProduto?: string; quantidade?: number; precoUnitario?: number }[]
    }>(resposta)
    if (!compra) { toast.error('Compra de origem não encontrada'); return }
    compraOrigemId.value = compra.id
    fornecedorNome.value = compra.fornecedorNome ?? ''
    numero.value = compra.numeroNota ? `DEV-${compra.numeroNota}` : ''
    linhas.value = (compra.itens ?? []).map((i) => ({
      produtoId: String(i.produtoId ?? ''),
      descricao: i.descricaoProduto ?? i.codigoProduto ?? `Item ${i.id.slice(0, 8)}`,
      compraItemOrigemId: i.id,
      quantidade: i.quantidade ?? null,
      valorUnitario: i.precoUnitario ?? null
    }))
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

// Carregar devolução (modo edição)
async function carregarDevolucao(id: string) {
  carregando.value = true
  try {
    const resposta = await useApi('/compras-devolucoes/{id}', { params: { id } })
    const d = extrairDados<{
      id: string; numero?: string; compraOrigemId?: string; fornecedorId?: string; dataDevolucao?: string
      tipo?: number; motivo?: string; status?: number; cfop?: string
      itens?: { id: string; compraItemOrigemId?: string; produtoId?: string; quantidade?: number; valorUnitario?: number }[]
    }>(resposta)
    if (!d) { toast.error('Devolução não encontrada'); return }
    compraOrigemId.value = d.compraOrigemId ?? ''
    fornecedorId.value = d.fornecedorId ?? ''
    numero.value = d.numero ?? ''
    dataDevolucao.value = soData(d.dataDevolucao)
    tipo.value = typeof d.tipo === 'number' ? d.tipo : 0
    motivo.value = d.motivo ?? ''
    cfop.value = d.cfop ?? ''
    statusDevolucao.value = d.status ?? null
    linhas.value = (d.itens ?? []).map((i) => ({
      produtoId: String(i.produtoId ?? ''),
      descricao: `Produto ${String(i.produtoId ?? '').slice(0, 8)}`,
      compraItemOrigemId: i.compraItemOrigemId ?? null,
      quantidade: i.quantidade ?? null,
      valorUnitario: i.valorUnitario ?? null
    }))
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

function validar(): boolean {
  if (!compraOrigemId.value) { toast.error('Compra de origem ausente'); return false }
  if (!fornecedorId.value.trim()) { toast.error('Informe o fornecedor (ID)'); return false }
  if (!numero.value.trim()) { toast.error('Informe o número da devolução'); return false }
  const validos = linhas.value.filter((l) => (l.quantidade ?? 0) > 0 && l.produtoId)
  if (!validos.length) { toast.error('Informe ao menos um item com quantidade'); return false }
  return true
}

async function salvar() {
  if (!validar()) return
  salvando.value = true
  try {
    await useApi('/compras-devolucoes', {
      method: 'POST',
      body: {
        compraOrigemId: compraOrigemId.value,
        fornecedorId: fornecedorId.value.trim(),
        numero: numero.value.trim(),
        dataDevolucao: dataDevolucao.value,
        tipo: tipo.value,
        motivo: motivo.value.trim() || null,
        cfop: cfop.value.trim() || null,
        itens: linhas.value
          .filter((l) => (l.quantidade ?? 0) > 0 && l.produtoId)
          .map((l) => ({ produtoId: l.produtoId, quantidade: l.quantidade, valorUnitario: l.valorUnitario ?? 0, compraItemOrigemId: l.compraItemOrigemId }))
      }
    })
    toast.success('Devolução criada')
    await navigateTo('/erp/compras/devolucao')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function confirmar() {
  if (!rotaId.value) return
  const ok = await confirmRef.value?.open('Confirmar devolução', 'Confirmar a devolução? A movimentação fiscal/estoque será efetivada.', { textoConfirmar: 'Confirmar', textoCancelar: 'Voltar' })
  if (!ok) return
  acaoEmCurso.value = true
  try {
    await useApi('/compras-devolucoes/{id}/confirmar', { method: 'POST', params: { id: rotaId.value } })
    toast.success('Devolução confirmada')
    await carregarDevolucao(rotaId.value)
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmCurso.value = false
  }
}
async function cancelar() {
  if (!rotaId.value) return
  const ok = await confirmRef.value?.open('Cancelar devolução', 'Cancelar esta devolução?', { danger: true, textoConfirmar: 'Cancelar', textoCancelar: 'Voltar' })
  if (!ok) return
  acaoEmCurso.value = true
  try {
    await useApi('/compras-devolucoes/{id}/cancelar', { method: 'POST', params: { id: rotaId.value } })
    toast.success('Devolução cancelada')
    await carregarDevolucao(rotaId.value)
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmCurso.value = false
  }
}

function hojeIso(): string {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}
function soData(v?: string | null): string {
  if (!v) return hojeIso()
  return v.includes('T') ? v.split('T')[0] : v
}
function voltar() {
  navigateTo('/erp/compras/devolucao')
}

onMounted(() => {
  if (rotaId.value) void carregarDevolucao(rotaId.value)
  else if (origemId.value) void carregarCompraOrigem(origemId.value)
  else toast.warning('Escolha a compra de origem pela lista de devoluções')
})
</script>

<template>
  <div>
    <PageToolbar
      :title="isEdit ? `Devolução ${numero || rotaId}` : 'Nova Devolução de Compra'"
      :subtitle="isEdit ? `Devolução de compra · ${statusTexto}` : 'Devolução de compra'"
      :loading="carregando || salvando"
    >
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
        <template v-if="isEdit && ehRascunho">
          <button type="button" class="btn btn-danger" :disabled="acaoEmCurso" @click="cancelar">Cancelar</button>
          <button type="button" class="btn btn-primary" :disabled="acaoEmCurso" @click="confirmar">Confirmar</button>
        </template>
        <button v-if="!isEdit" type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <section class="glass-panel bloco">
      <h3 class="bloco-titulo">Dados da Devolução</h3>
      <div class="form-grid">
        <div class="col-4"><TextField v-model="numero" label="Número" :readonly="somenteLeitura" required /></div>
        <div class="col-4"><DateTimeField v-model="dataDevolucao" label="Data da devolução" :disabled="somenteLeitura" /></div>
        <div class="col-4"><SelectField v-model="tipo" :options="TIPO_OPTIONS" label="Tipo" :clearable="false" :disabled="somenteLeitura" /></div>
        <div class="col-6"><TextField :model-value="fornecedorNome || (compraOrigemId ? `Compra ${compraOrigemId.slice(0, 8)}` : '')" label="Origem" readonly /></div>
        <div class="col-6"><TextField v-model="fornecedorId" label="Fornecedor (ID)" :readonly="somenteLeitura" required hint="A projeção da compra não expõe o Guid do fornecedor" /></div>
        <div class="col-3"><TextField v-model="cfop" label="CFOP (opcional)" :readonly="somenteLeitura" /></div>
        <div class="col-9"><TextField v-model="motivo" label="Motivo (opcional)" :readonly="somenteLeitura" /></div>
      </div>
    </section>

    <section class="glass-panel bloco">
      <h3 class="bloco-titulo">Itens a Devolver</h3>
      <table class="linhas">
        <thead><tr><th>Produto</th><th class="num">Quantidade</th><th class="num">Valor unit.</th><th class="num">Total</th></tr></thead>
        <tbody>
          <tr v-for="(l, idx) in linhas" :key="idx">
            <td>{{ l.descricao }}</td>
            <td class="num"><QuantityInput v-model="l.quantidade" :readonly="somenteLeitura" /></td>
            <td class="num"><MoneyInput v-model="l.valorUnitario" :readonly="somenteLeitura" /></td>
            <td class="num">{{ formatarMoeda((l.quantidade ?? 0) * (l.valorUnitario ?? 0)) }}</td>
          </tr>
          <tr v-if="!linhas.length"><td colspan="4" class="vazio">Sem itens.</td></tr>
        </tbody>
        <tfoot>
          <tr><td colspan="3" class="num total-label">Total da devolução</td><td class="num total-valor">{{ formatarMoeda(totalDevolucao) }}</td></tr>
        </tfoot>
      </table>
    </section>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.bloco { margin-bottom: 16px; }
section.bloco { padding: 16px; }
.bloco-titulo { font-size: 15px; font-weight: 600; margin: 0 0 12px; }
.linhas { width: 100%; border-collapse: collapse; font-size: 13px; }
.linhas th, .linhas td { padding: 8px 10px; border-bottom: 1px solid var(--border-color, rgba(255, 255, 255, 0.08)); text-align: left; }
.linhas th.num, .linhas td.num { text-align: right; width: 150px; }
.vazio { text-align: center; color: var(--text-secondary); }
.total-label { font-weight: 600; }
.total-valor { font-weight: 700; color: var(--primary); }
</style>
