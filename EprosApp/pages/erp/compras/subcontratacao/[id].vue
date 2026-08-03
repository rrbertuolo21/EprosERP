<script setup lang="ts">
/**
 * Subcontratação — detalhe (erp/compras/subcontratacao/[id]).
 *
 * Camada de apresentação sobre `SubcontratacoesController`. Tela com abas:
 *   - Ordem: cabeçalho + itens planejados;
 *   - Envio: registra remessa ao terceiro (produto + quantidade enviada);
 *   - Retorno: registra retorno do beneficiamento (retorno/aprovada/perda/sucata/rendimento).
 *
 * Endpoints: estoque-subcontratacoes/{id} (GET), {id}/enviar, {id}/retornar (POST).
 */
import { computed, onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

interface OrdemItem {
  id: string
  produtoId: string
  quantidadePlanejada: number | null
  unidade: string | null
  operacaoTerceirizada: string | null
}
interface OrdemDetalhe {
  id: string
  numeroOrdem: string | null
  fornecedorId: string | null
  ordemProducaoId: string | null
  status: number | string | null
  dataEmissao: string | null
  dataPrevistaRetorno: string | null
  observacao: string | null
  itens: OrdemItem[]
}

interface LinhaEnvio { produtoId: string; quantidadeEnviada: number | null }
interface LinhaRetorno {
  produtoId: string
  quantidadeRetorno: number | null
  quantidadeAprovada: number | null
  quantidadePerda: number | null
  quantidadeSucata: number | null
  rendimento: number | null
}

const route = useRoute()
const toast = useToast()
const { formatarData, formatarNumero } = useHelper()

const ordemId = computed(() => String(route.params.id))
const carregando = ref(false)
const ordem = ref<OrdemDetalhe | null>(null)
const abaAtiva = ref<'ordem' | 'envio' | 'retorno'>('ordem')

const STATUS_TEXTO: Record<string, string> = { '0': 'Aberta', '1': 'Em Processo', '2': 'Retornada', '3': 'Concluída', '4': 'Cancelada' }
const statusTexto = computed(() => STATUS_TEXTO[String(ordem.value?.status)] ?? String(ordem.value?.status ?? '-'))

const colunasItens: DataTableColumn<OrdemItem>[] = [
  { key: 'produtoId', label: 'Produto', formatter: (v) => `Produto ${String(v).slice(0, 8)}` },
  { key: 'quantidadePlanejada', label: 'Qtd. planejada', align: 'right', formatter: (v) => formatarNumero(v as number | null) },
  { key: 'unidade', label: 'Unid.', align: 'center', formatter: (v) => (v as string | null) ?? '-' },
  { key: 'operacaoTerceirizada', label: 'Operação', formatter: (v) => (v as string | null) ?? '-' }
]

async function carregar() {
  carregando.value = true
  try {
    const resposta = await useApi('/estoque-subcontratacoes/{id}', { params: { id: ordemId.value } })
    ordem.value = extrairDados<OrdemDetalhe>(resposta) ?? null
    semearLinhas()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

// --- Envio ---
const dataEnvio = ref(hojeIso())
const documentoEnvio = ref('')
const linhasEnvio = ref<LinhaEnvio[]>([])
const salvandoEnvio = ref(false)

// --- Retorno ---
const dataRetorno = ref(hojeIso())
const documentoRetorno = ref('')
const linhasRetorno = ref<LinhaRetorno[]>([])
const salvandoRetorno = ref(false)

function semearLinhas() {
  const its = ordem.value?.itens ?? []
  linhasEnvio.value = its.map((i) => ({ produtoId: i.produtoId, quantidadeEnviada: i.quantidadePlanejada ?? null }))
  linhasRetorno.value = its.map((i) => ({
    produtoId: i.produtoId, quantidadeRetorno: null, quantidadeAprovada: null,
    quantidadePerda: null, quantidadeSucata: null, rendimento: null
  }))
}

async function registrarEnvio() {
  const itensPayload = linhasEnvio.value
    .filter((l) => (l.quantidadeEnviada ?? 0) > 0)
    .map((l) => ({ produtoId: l.produtoId, quantidadeEnviada: l.quantidadeEnviada, loteId: null, localOrigemId: null }))
  if (!itensPayload.length) {
    toast.error('Informe ao menos um item com quantidade enviada')
    return
  }
  salvandoEnvio.value = true
  try {
    await useApi('/estoque-subcontratacoes/{id}/enviar', {
      method: 'POST',
      params: { id: ordemId.value },
      body: { ordemId: ordemId.value, dataEnvio: dataEnvio.value || null, documentoFiscalId: documentoEnvio.value || null, itens: itensPayload }
    })
    toast.success('Envio registrado')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoEnvio.value = false
  }
}

async function registrarRetorno() {
  const itensPayload = linhasRetorno.value
    .filter((l) => (l.quantidadeRetorno ?? 0) > 0)
    .map((l) => ({
      produtoId: l.produtoId, quantidadeRetorno: l.quantidadeRetorno,
      quantidadeAprovada: l.quantidadeAprovada, quantidadePerda: l.quantidadePerda,
      quantidadeSucata: l.quantidadeSucata, rendimento: l.rendimento
    }))
  if (!itensPayload.length) {
    toast.error('Informe ao menos um item com quantidade de retorno')
    return
  }
  salvandoRetorno.value = true
  try {
    await useApi('/estoque-subcontratacoes/{id}/retornar', {
      method: 'POST',
      params: { id: ordemId.value },
      body: { ordemId: ordemId.value, dataRetorno: dataRetorno.value || null, documentoFiscalId: documentoRetorno.value || null, itens: itensPayload }
    })
    toast.success('Retorno registrado')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoRetorno.value = false
  }
}

function hojeIso(): string {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}
function voltar() {
  navigateTo('/erp/compras/subcontratacao')
}

onMounted(() => void carregar())
</script>

<template>
  <div>
    <PageToolbar
      :title="ordem?.numeroOrdem ? `Ordem ${ordem.numeroOrdem}` : 'Ordem de Subcontratação'"
      :subtitle="`Subcontratação · ${statusTexto}`"
      :loading="carregando"
    >
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
      </template>
    </PageToolbar>

    <div class="tabs">
      <button type="button" class="tab" :class="{ ativa: abaAtiva === 'ordem' }" @click="abaAtiva = 'ordem'">Ordem</button>
      <button type="button" class="tab" :class="{ ativa: abaAtiva === 'envio' }" @click="abaAtiva = 'envio'">Envio</button>
      <button type="button" class="tab" :class="{ ativa: abaAtiva === 'retorno' }" @click="abaAtiva = 'retorno'">Retorno</button>
    </div>

    <!-- Aba Ordem -->
    <div v-show="abaAtiva === 'ordem'">
      <section class="glass-panel bloco">
        <h3 class="bloco-titulo">Dados da Ordem</h3>
        <div class="form-grid">
          <div class="col-4"><TextField :model-value="ordem?.numeroOrdem ?? ''" label="Nº da ordem" readonly /></div>
          <div class="col-4"><TextField :model-value="ordem?.fornecedorId ? `Forn. ${String(ordem.fornecedorId).slice(0, 8)}` : ''" label="Fornecedor" readonly /></div>
          <div class="col-4"><TextField :model-value="statusTexto" label="Status" readonly /></div>
          <div class="col-4"><TextField :model-value="formatarData(ordem?.dataEmissao ?? null)" label="Emissão" readonly /></div>
          <div class="col-4"><TextField :model-value="formatarData(ordem?.dataPrevistaRetorno ?? null)" label="Previsão de retorno" readonly /></div>
          <div class="col-12"><TextField :model-value="ordem?.observacao ?? ''" label="Observação" readonly /></div>
        </div>
      </section>
      <section class="glass-panel bloco itens-bloco">
        <h3 class="bloco-titulo">Itens Planejados</h3>
        <DataTable :items="ordem?.itens ?? []" :columns="colunasItens" :total="ordem?.itens?.length ?? 0" :page="1" :page-size="100" empty-text="Ordem sem itens" />
      </section>
    </div>

    <!-- Aba Envio -->
    <div v-show="abaAtiva === 'envio'">
      <section class="glass-panel bloco">
        <h3 class="bloco-titulo">Registrar Envio (remessa ao terceiro)</h3>
        <div class="form-grid">
          <div class="col-6"><DateTimeField v-model="dataEnvio" label="Data do envio" /></div>
          <div class="col-6"><TextField v-model="documentoEnvio" label="Documento fiscal (ID) — opcional" /></div>
        </div>
        <table class="linhas">
          <thead><tr><th>Produto</th><th class="num">Quantidade enviada</th></tr></thead>
          <tbody>
            <tr v-for="(l, idx) in linhasEnvio" :key="idx">
              <td>Produto {{ String(l.produtoId).slice(0, 8) }}</td>
              <td class="num"><QuantityInput v-model="l.quantidadeEnviada" /></td>
            </tr>
            <tr v-if="!linhasEnvio.length"><td colspan="2" class="vazio">Ordem sem itens para enviar.</td></tr>
          </tbody>
        </table>
        <div class="acoes-form">
          <button type="button" class="btn btn-primary" :disabled="salvandoEnvio || !linhasEnvio.length" @click="registrarEnvio">
            <span v-if="salvandoEnvio" class="spinner"></span>
            <span v-else>Registrar envio</span>
          </button>
        </div>
      </section>
    </div>

    <!-- Aba Retorno -->
    <div v-show="abaAtiva === 'retorno'">
      <section class="glass-panel bloco">
        <h3 class="bloco-titulo">Registrar Retorno (do beneficiamento)</h3>
        <div class="form-grid">
          <div class="col-6"><DateTimeField v-model="dataRetorno" label="Data do retorno" /></div>
          <div class="col-6"><TextField v-model="documentoRetorno" label="Documento fiscal (ID) — opcional" /></div>
        </div>
        <table class="linhas">
          <thead>
            <tr><th>Produto</th><th class="num">Retorno</th><th class="num">Aprovada</th><th class="num">Perda</th><th class="num">Sucata</th><th class="num">Rendimento</th></tr>
          </thead>
          <tbody>
            <tr v-for="(l, idx) in linhasRetorno" :key="idx">
              <td>Produto {{ String(l.produtoId).slice(0, 8) }}</td>
              <td class="num"><QuantityInput v-model="l.quantidadeRetorno" /></td>
              <td class="num"><QuantityInput v-model="l.quantidadeAprovada" /></td>
              <td class="num"><QuantityInput v-model="l.quantidadePerda" /></td>
              <td class="num"><QuantityInput v-model="l.quantidadeSucata" /></td>
              <td class="num"><QuantityInput v-model="l.rendimento" /></td>
            </tr>
            <tr v-if="!linhasRetorno.length"><td colspan="6" class="vazio">Ordem sem itens para retornar.</td></tr>
          </tbody>
        </table>
        <div class="acoes-form">
          <button type="button" class="btn btn-primary" :disabled="salvandoRetorno || !linhasRetorno.length" @click="registrarRetorno">
            <span v-if="salvandoRetorno" class="spinner"></span>
            <span v-else>Registrar retorno</span>
          </button>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
.bloco { margin-bottom: 16px; }
section.bloco { padding: 16px; }
.bloco-titulo { font-size: 15px; font-weight: 600; margin: 0 0 12px; }
.tabs { display: flex; gap: 4px; margin-bottom: 16px; border-bottom: 1px solid var(--border-color, rgba(255, 255, 255, 0.1)); }
.tab { background: none; border: none; padding: 10px 16px; cursor: pointer; color: var(--text-secondary); font-size: 14px; font-weight: 600; border-bottom: 2px solid transparent; margin-bottom: -1px; }
.tab.ativa { color: var(--primary); border-bottom-color: var(--primary); }
.acoes-form { display: flex; justify-content: flex-end; margin-top: 12px; }
.linhas { width: 100%; border-collapse: collapse; margin-top: 8px; font-size: 13px; }
.linhas th, .linhas td { padding: 8px 10px; border-bottom: 1px solid var(--border-color, rgba(255, 255, 255, 0.08)); text-align: left; }
.linhas th.num, .linhas td.num { text-align: right; width: 140px; }
.vazio { text-align: center; color: var(--text-secondary); }
.itens-bloco :deep(.glass-panel) { background: transparent; }
</style>
