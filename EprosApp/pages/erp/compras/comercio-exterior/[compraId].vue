<script setup lang="ts">
/**
 * Comércio Exterior — DI + adições de uma compra (erp/compras/comercio-exterior/[compraId]).
 *
 * Camada de apresentação sobre `ComercioExteriorController`:
 *   - Incoterm / moeda / taxa de câmbio da compra (PUT compras/{compraId});
 *   - Nacionalizar importação (POST compras/{compraId}/nacionalizar);
 *   - Declarações de Importação (DI) por item da compra: listar, registrar, excluir;
 *   - Adições da DI: listar (dentro da DI), adicionar, excluir.
 *
 * Endpoints: compras/{id} (GET itens), compras-comercio-exterior/compras/{compraId} (PUT),
 *   .../compras/{compraId}/nacionalizar (POST), .../itens/{compraItemId}/declaracoes (GET/POST),
 *   .../declaracoes/{id} (DELETE), .../declaracoes/{id}/adicoes (POST), .../adicoes/{id} (DELETE).
 */
import { computed, onMounted, ref, watch } from 'vue'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import type { SelectOption } from '~/composables/useEnum'
import { UF_OPTIONS } from '~/composables/compras/ufOptions'

definePageMeta({ middleware: 'auth', layout: 'default' })

interface CompraItem { id: string; codigoProduto: string | null; descricaoProduto: string | null; quantidade: number | null }
interface CompraDetalhe { id: string; numeroNota: string | null; fornecedorNome: string | null; itens: CompraItem[] }

interface Adicao {
  id: string
  numeroAdicao: number
  numeroSequencialAdicao: number
  codigoFabricante: string | null
  valorDesconto: number | null
  numeroAtoConcessorio: string | null
}
interface Declaracao {
  id: string
  compraItemId: string
  numeroDeclaracaoImportacao: string
  dataDeclaracaoImportacao: string | null
  localDesembaraco: string | null
  ufDesembaraco: number | null
  dataDesembaraco: string | null
  tipoViaTransporte: number | null
  valorAFRMM: number | null
  tipoIntermedio: number | null
  codigoExportador: string | null
  adicoes: Adicao[]
}

const route = useRoute()
const toast = useToast()
const { formatarData, formatarMoeda } = useHelper()

const compraId = computed(() => String(route.params.compraId))
const carregando = ref(false)
const compra = ref<CompraDetalhe | null>(null)
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const INCOTERM_OPTIONS: SelectOption[] = [
  { label: 'Não informado', value: 0 }, { label: 'EXW', value: 1 }, { label: 'FCA', value: 2 },
  { label: 'FAS', value: 3 }, { label: 'FOB', value: 4 }, { label: 'CFR', value: 5 }, { label: 'CIF', value: 6 },
  { label: 'CPT', value: 7 }, { label: 'CIP', value: 8 }, { label: 'DAP', value: 9 }, { label: 'DPU', value: 10 }, { label: 'DDP', value: 11 }
]
const VIA_OPTIONS: SelectOption[] = [
  { label: 'Marítima', value: 1 }, { label: 'Fluvial', value: 2 }, { label: 'Lacustre', value: 3 },
  { label: 'Aérea', value: 4 }, { label: 'Postal', value: 5 }, { label: 'Ferroviária', value: 6 },
  { label: 'Rodoviária', value: 7 }, { label: 'Conduto/Rede', value: 8 }, { label: 'Meios próprios', value: 9 }, { label: 'Entrada/Saída ficta', value: 10 }
]
const INTERMEDIO_OPTIONS: SelectOption[] = [
  { label: 'Importação direta', value: 1 }, { label: 'Conta e ordem', value: 2 }, { label: 'Encomenda', value: 3 }
]

// --- Incoterm / câmbio ---
const incoterm = ref<number>(4)
const moeda = ref('USD')
const taxaCambio = ref<number | null>(null)
const salvandoCex = ref(false)
const nacionalizando = ref(false)

async function carregarCompra() {
  carregando.value = true
  try {
    const resposta = await useApi('/compras/{id}', { params: { id: compraId.value } })
    compra.value = extrairDados<CompraDetalhe>(resposta) ?? null
    if (compra.value?.itens?.length) itemSelecionado.value = compra.value.itens[0].id
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function salvarCex() {
  salvandoCex.value = true
  try {
    await useApi('/compras-comercio-exterior/compras/{compraId}', {
      method: 'PUT',
      params: { compraId: compraId.value },
      body: { incoterm: incoterm.value, moeda: moeda.value.trim() || null, taxaCambio: taxaCambio.value }
    })
    toast.success('Comércio exterior definido para a compra')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoCex.value = false
  }
}

async function nacionalizar() {
  const ok = await confirmRef.value?.open('Nacionalizar importação', 'Confirmar nacionalização? Publica a entrada no estoque com o custo (landed, se habilitado) e os títulos de tributos/frete. Idempotente por compra.', { textoConfirmar: 'Nacionalizar', textoCancelar: 'Voltar' })
  if (!ok) return
  nacionalizando.value = true
  try {
    await useApi('/compras-comercio-exterior/compras/{compraId}/nacionalizar', { method: 'POST', params: { compraId: compraId.value }, body: {} })
    toast.success('Importação nacionalizada')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    nacionalizando.value = false
  }
}

// --- DI por item ---
const itemSelecionado = ref<string>('')
const itemOptions = computed<SelectOption[]>(() =>
  (compra.value?.itens ?? []).map((i) => ({ label: `${i.codigoProduto ?? ''} ${i.descricaoProduto ?? ''}`.trim() || `Item ${i.id.slice(0, 8)}`, value: i.id }))
)
const declaracoes = ref<Declaracao[]>([])
const carregandoDi = ref(false)

async function carregarDeclaracoes() {
  if (!itemSelecionado.value) { declaracoes.value = []; return }
  carregandoDi.value = true
  try {
    const resposta = await useApi('/compras-comercio-exterior/itens/{compraItemId}/declaracoes', { params: { compraItemId: itemSelecionado.value } })
    declaracoes.value = extrairLista<Declaracao>(resposta) ?? []
  } catch (e) {
    declaracoes.value = []
    toast.error(obterMensagemErro(e))
  } finally {
    carregandoDi.value = false
  }
}

watch(itemSelecionado, () => void carregarDeclaracoes())

// Registrar DI
const diVisivel = ref(false)
const salvandoDi = ref(false)
const diForm = ref({
  numeroDeclaracaoImportacao: '', dataDeclaracaoImportacao: hojeIso(), localDesembaraco: '', ufDesembaraco: 24 as number,
  dataDesembaraco: hojeIso(), tipoViaTransporte: 1 as number, valorAFRMM: null as number | null, tipoIntermedio: 1 as number,
  cnpj: '', cpf: '', ufTerceiro: null as number | null, codigoExportador: ''
})
function abrirNovaDi() {
  diForm.value = {
    numeroDeclaracaoImportacao: '', dataDeclaracaoImportacao: hojeIso(), localDesembaraco: '', ufDesembaraco: 24,
    dataDesembaraco: hojeIso(), tipoViaTransporte: 1, valorAFRMM: null, tipoIntermedio: 1,
    cnpj: '', cpf: '', ufTerceiro: null, codigoExportador: ''
  }
  diVisivel.value = true
}
async function salvarDi() {
  if (!itemSelecionado.value) { toast.error('Selecione o item da compra'); return }
  if (!diForm.value.numeroDeclaracaoImportacao.trim()) { toast.error('Informe o número da DI'); return }
  salvandoDi.value = true
  try {
    await useApi('/compras-comercio-exterior/itens/{compraItemId}/declaracoes', {
      method: 'POST',
      params: { compraItemId: itemSelecionado.value },
      body: {
        numeroDeclaracaoImportacao: diForm.value.numeroDeclaracaoImportacao.trim(),
        dataDeclaracaoImportacao: diForm.value.dataDeclaracaoImportacao,
        localDesembaraco: diForm.value.localDesembaraco.trim(),
        ufDesembaraco: diForm.value.ufDesembaraco,
        dataDesembaraco: diForm.value.dataDesembaraco,
        tipoViaTransporte: diForm.value.tipoViaTransporte,
        valorAFRMM: diForm.value.valorAFRMM ?? 0,
        tipoIntermedio: diForm.value.tipoIntermedio,
        cnpj: diForm.value.cnpj.trim() || null,
        cpf: diForm.value.cpf.trim() || null,
        ufTerceiro: diForm.value.ufTerceiro,
        codigoExportador: diForm.value.codigoExportador.trim()
      }
    })
    toast.success('DI registrada')
    diVisivel.value = false
    await carregarDeclaracoes()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoDi.value = false
  }
}
async function excluirDi(d: Declaracao) {
  const ok = await confirmRef.value?.open('Excluir DI', `Excluir a DI ${d.numeroDeclaracaoImportacao}?`, { danger: true, textoConfirmar: 'Excluir', textoCancelar: 'Voltar' })
  if (!ok) return
  try {
    await useApi('/compras-comercio-exterior/declaracoes/{id}', { method: 'DELETE', params: { id: d.id } })
    toast.success('DI excluída')
    await carregarDeclaracoes()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

// Adição
const adicaoVisivel = ref(false)
const salvandoAdicao = ref(false)
const diAlvo = ref<Declaracao | null>(null)
const adicaoForm = ref({ numeroAdicao: 1, numeroSequencialAdicao: 1, codigoFabricante: '', valorDesconto: null as number | null, numeroAtoConcessorio: '' })
function abrirNovaAdicao(d: Declaracao) {
  diAlvo.value = d
  adicaoForm.value = { numeroAdicao: (d.adicoes?.length ?? 0) + 1, numeroSequencialAdicao: 1, codigoFabricante: '', valorDesconto: null, numeroAtoConcessorio: '' }
  adicaoVisivel.value = true
}
async function salvarAdicao() {
  if (!diAlvo.value) return
  if (!adicaoForm.value.codigoFabricante.trim()) { toast.error('Informe o código do fabricante'); return }
  salvandoAdicao.value = true
  try {
    await useApi('/compras-comercio-exterior/declaracoes/{id}/adicoes', {
      method: 'POST',
      params: { id: diAlvo.value.id },
      body: {
        numeroAdicao: Number(adicaoForm.value.numeroAdicao),
        numeroSequencialAdicao: Number(adicaoForm.value.numeroSequencialAdicao),
        codigoFabricante: adicaoForm.value.codigoFabricante.trim(),
        valorDesconto: adicaoForm.value.valorDesconto ?? 0,
        numeroAtoConcessorio: adicaoForm.value.numeroAtoConcessorio.trim() || null
      }
    })
    toast.success('Adição incluída')
    adicaoVisivel.value = false
    await carregarDeclaracoes()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoAdicao.value = false
  }
}
async function excluirAdicao(d: Declaracao, a: Adicao) {
  const ok = await confirmRef.value?.open('Excluir adição', `Excluir a adição ${a.numeroAdicao}?`, { danger: true, textoConfirmar: 'Excluir', textoCancelar: 'Voltar' })
  if (!ok) return
  try {
    await useApi('/compras-comercio-exterior/declaracoes/{declaracaoId}/adicoes/{adicaoId}', {
      method: 'DELETE', params: { declaracaoId: d.id, adicaoId: a.id }
    })
    toast.success('Adição excluída')
    await carregarDeclaracoes()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

function hojeIso(): string {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}
function voltar() {
  navigateTo('/erp/compras/comercio-exterior')
}

onMounted(() => void carregarCompra())
</script>

<template>
  <div>
    <PageToolbar
      :title="compra?.numeroNota ? `Importação · Nota ${compra.numeroNota}` : 'Importação'"
      :subtitle="compra?.fornecedorNome ?? 'Comércio Exterior'"
      :loading="carregando"
    >
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
        <button type="button" class="btn btn-primary" :disabled="nacionalizando" @click="nacionalizar">
          <span v-if="nacionalizando" class="spinner"></span>
          <span v-else>Nacionalizar</span>
        </button>
      </template>
    </PageToolbar>

    <section class="glass-panel bloco">
      <h3 class="bloco-titulo">Incoterm & Câmbio</h3>
      <div class="form-grid">
        <div class="col-4"><SelectField v-model="incoterm" :options="INCOTERM_OPTIONS" label="Incoterm" :clearable="false" /></div>
        <div class="col-4"><TextField v-model="moeda" label="Moeda" placeholder="USD, EUR..." /></div>
        <div class="col-4"><MoneyInput v-model="taxaCambio" label="Taxa de câmbio" :symbol="false" /></div>
      </div>
      <div class="acoes-form">
        <button type="button" class="btn btn-primary" :disabled="salvandoCex" @click="salvarCex">
          <span v-if="salvandoCex" class="spinner"></span>
          <span v-else>Salvar comércio exterior</span>
        </button>
      </div>
    </section>

    <section class="glass-panel bloco">
      <div class="bloco-header">
        <h3 class="bloco-titulo">Declarações de Importação (DI) por item</h3>
        <button type="button" class="btn btn-secondary btn-sm" :disabled="!itemSelecionado" @click="abrirNovaDi">+ Nova DI</button>
      </div>
      <div class="form-grid">
        <div class="col-12"><SelectField v-model="itemSelecionado" :options="itemOptions" label="Item da compra" :clearable="false" /></div>
      </div>

      <div v-if="carregandoDi" class="aviso">Carregando declarações...</div>
      <div v-else-if="declaracoes.length" class="di-lista">
        <article v-for="d in declaracoes" :key="d.id" class="di-card">
          <header class="di-head">
            <div>
              <strong>DI {{ d.numeroDeclaracaoImportacao }}</strong>
              <span class="di-meta">Desembaraço {{ formatarData(d.dataDesembaraco) }} · {{ d.localDesembaraco }} · AFRMM {{ formatarMoeda(d.valorAFRMM) }}</span>
            </div>
            <div class="di-acoes">
              <button type="button" class="btn btn-ghost btn-sm" title="Adicionar adição" @click="abrirNovaAdicao(d)">+ Adição</button>
              <button type="button" class="btn btn-ghost btn-sm" title="Excluir DI" @click="excluirDi(d)">🗑</button>
            </div>
          </header>
          <table v-if="d.adicoes?.length" class="tab">
            <thead><tr><th>Adição</th><th>Seq.</th><th>Cód. fabricante</th><th class="num">Desconto</th><th>Ato concessório</th><th></th></tr></thead>
            <tbody>
              <tr v-for="a in d.adicoes" :key="a.id">
                <td>{{ a.numeroAdicao }}</td>
                <td>{{ a.numeroSequencialAdicao }}</td>
                <td>{{ a.codigoFabricante || '-' }}</td>
                <td class="num">{{ formatarMoeda(a.valorDesconto) }}</td>
                <td>{{ a.numeroAtoConcessorio || '-' }}</td>
                <td class="num"><button type="button" class="btn btn-ghost btn-sm" title="Excluir adição" @click="excluirAdicao(d, a)">🗑</button></td>
              </tr>
            </tbody>
          </table>
          <p v-else class="aviso sem-adicao">Sem adições nesta DI.</p>
        </article>
      </div>
      <p v-else-if="itemSelecionado" class="aviso">Nenhuma DI registrada para este item.</p>
    </section>

    <!-- Diálogo Nova DI -->
    <AppDialog v-model="diVisivel" title="Nova Declaração de Importação" width="720px" persistent>
      <div class="form-grid">
        <div class="col-6"><TextField v-model="diForm.numeroDeclaracaoImportacao" label="Número da DI" required /></div>
        <div class="col-6"><DateTimeField v-model="diForm.dataDeclaracaoImportacao" label="Data da DI" /></div>
        <div class="col-6"><TextField v-model="diForm.localDesembaraco" label="Local de desembaraço" /></div>
        <div class="col-3"><SelectField v-model="diForm.ufDesembaraco" :options="UF_OPTIONS" label="UF desembaraço" :clearable="false" /></div>
        <div class="col-3"><DateTimeField v-model="diForm.dataDesembaraco" label="Data desembaraço" /></div>
        <div class="col-4"><SelectField v-model="diForm.tipoViaTransporte" :options="VIA_OPTIONS" label="Via de transporte" :clearable="false" /></div>
        <div class="col-4"><MoneyInput v-model="diForm.valorAFRMM" label="Valor AFRMM" /></div>
        <div class="col-4"><SelectField v-model="diForm.tipoIntermedio" :options="INTERMEDIO_OPTIONS" label="Tipo de intermédio" :clearable="false" /></div>
        <div class="col-6"><TextField v-model="diForm.codigoExportador" label="Código do exportador" /></div>
        <div class="col-6"><TextField v-model="diForm.cnpj" label="CNPJ (terceiro, opcional)" /></div>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoDi" @click="diVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoDi" @click="salvarDi">
          <span v-if="salvandoDi" class="spinner"></span>
          <span v-else>Registrar DI</span>
        </button>
      </template>
    </AppDialog>

    <!-- Diálogo Nova Adição -->
    <AppDialog v-model="adicaoVisivel" title="Nova Adição" width="560px" persistent>
      <div class="form-grid">
        <div class="col-4"><TextField v-model="adicaoForm.numeroAdicao" label="Nº adição" /></div>
        <div class="col-4"><TextField v-model="adicaoForm.numeroSequencialAdicao" label="Nº sequencial" /></div>
        <div class="col-4"><MoneyInput v-model="adicaoForm.valorDesconto" label="Valor desconto" /></div>
        <div class="col-6"><TextField v-model="adicaoForm.codigoFabricante" label="Código do fabricante" required /></div>
        <div class="col-6"><TextField v-model="adicaoForm.numeroAtoConcessorio" label="Ato concessório (opcional)" /></div>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoAdicao" @click="adicaoVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoAdicao" @click="salvarAdicao">
          <span v-if="salvandoAdicao" class="spinner"></span>
          <span v-else>Incluir adição</span>
        </button>
      </template>
    </AppDialog>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.bloco { margin-bottom: 16px; }
section.bloco { padding: 16px; }
.bloco-titulo { font-size: 15px; font-weight: 600; margin: 0 0 12px; }
.bloco-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
.bloco-header .bloco-titulo { margin: 0; }
.aviso { color: var(--text-secondary); font-size: 13px; margin: 8px 0; }
.acoes-form { display: flex; justify-content: flex-end; margin-top: 12px; }
.di-lista { display: flex; flex-direction: column; gap: 12px; margin-top: 8px; }
.di-card { border: 1px solid var(--border-color, rgba(255, 255, 255, 0.1)); border-radius: 10px; padding: 12px 14px; }
.di-head { display: flex; align-items: flex-start; justify-content: space-between; gap: 12px; }
.di-meta { display: block; font-size: 12px; color: var(--text-secondary); margin-top: 2px; }
.di-acoes { display: flex; gap: 6px; flex-shrink: 0; }
.sem-adicao { margin: 8px 0 0; }
.tab { width: 100%; border-collapse: collapse; font-size: 13px; margin-top: 10px; }
.tab th, .tab td { padding: 8px 10px; border-bottom: 1px solid var(--border-color, rgba(255, 255, 255, 0.08)); text-align: left; }
.tab th.num, .tab td.num { text-align: right; }
</style>
