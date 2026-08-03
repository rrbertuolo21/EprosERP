<script setup lang="ts">
/**
 * Comércio Exterior — hub (erp/compras/comercio-exterior).
 *
 * Camada de apresentação sobre `ComercioExteriorController` (`/api/v1/compras-comercio-exterior`):
 *   - configuração do rateio landed por empresa/tenant (GET/PUT rateio-landed);
 *   - abertura de uma compra de importação para gerir Incoterm, DI e adições.
 *
 * Não há endpoint de listagem de compras de importação; a compra é aberta por ID
 * (reaproveita a busca em /compras). Falta anotada no relatório.
 *
 * Endpoints: compras-comercio-exterior/rateio-landed (GET, PUT); compras (GET) p/ localizar.
 */
import { onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ middleware: 'auth', layout: 'default' })

const toast = useToast()
const { formatarData, formatarMoeda } = useHelper()

// --- Rateio Landed ---
interface RateioLanded {
  id?: string
  empresaId: string | null
  habilitado: boolean
  incluirTributos: boolean
  incluirFrete: boolean
  incluirDespesas: boolean
  metodo: number | string
}
const METODO_OPTIONS: SelectOption[] = [
  { label: 'Por valor', value: 0 },
  { label: 'Por peso', value: 1 },
  { label: 'Por quantidade', value: 2 }
]
const SIM_NAO: SelectOption[] = [{ label: 'Sim', value: 1 }, { label: 'Não', value: 0 }]

const config = ref<RateioLanded>({ empresaId: null, habilitado: false, incluirTributos: false, incluirFrete: true, incluirDespesas: false, metodo: 0 })
const carregandoConfig = ref(false)
const salvandoConfig = ref(false)

function num(v: number | string): number {
  return typeof v === 'string' ? Number(v) : v
}

async function carregarConfig() {
  carregandoConfig.value = true
  try {
    const resposta = await useApi('/compras-comercio-exterior/rateio-landed')
    const dados = extrairDados<RateioLanded>(resposta)
    if (dados) config.value = { ...config.value, ...dados, metodo: num(dados.metodo) }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregandoConfig.value = false
  }
}

async function salvarConfig() {
  salvandoConfig.value = true
  try {
    await useApi('/compras-comercio-exterior/rateio-landed', {
      method: 'PUT',
      body: {
        empresaId: config.value.empresaId,
        habilitado: config.value.habilitado,
        incluirTributos: config.value.incluirTributos,
        incluirFrete: config.value.incluirFrete,
        incluirDespesas: config.value.incluirDespesas,
        metodo: num(config.value.metodo)
      }
    })
    toast.success('Configuração de rateio landed salva')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoConfig.value = false
  }
}

// --- Localizar compra de importação ---
interface CompraBusca { id: string; numeroNota: string | null; fornecedorNome: string | null; valorTotal: number | null; dataEmissao: string | null }
const termo = ref('')
const buscando = ref(false)
const resultados = ref<CompraBusca[]>([])
let deb: ReturnType<typeof setTimeout> | undefined

function aoDigitar(v: string) {
  termo.value = v
  if (deb) clearTimeout(deb)
  if (!v || v.trim().length < 2) { resultados.value = []; return }
  deb = setTimeout(() => void buscar(v.trim()), 400)
}
async function buscar(t: string) {
  buscando.value = true
  try {
    const resposta = await useApi('/compras', { query: { localizar: t, pagina: 1, tamanhoPagina: 20 } })
    const dados = extrairDados<{ itens: CompraBusca[] }>(resposta)
    resultados.value = dados?.itens ?? []
  } catch (e) {
    resultados.value = []
    console.error('[cex] busca compra', e)
  } finally {
    buscando.value = false
  }
}
function abrirCompra(c: CompraBusca) {
  navigateTo(`/erp/compras/comercio-exterior/${c.id}`)
}

onMounted(() => void carregarConfig())
</script>

<template>
  <div>
    <PageToolbar title="Comércio Exterior" subtitle="Importação — Incoterm, DI, adições e rateio landed" :loading="carregandoConfig" />

    <section class="glass-panel bloco">
      <h3 class="bloco-titulo">Rateio Landed (custo de importação)</h3>
      <p class="aviso">Quando habilitado, tributos/frete/despesas da importação são apropriados ao custo dos itens na nacionalização (default: desligado).</p>
      <div class="form-grid">
        <div class="col-4">
          <SelectField :model-value="config.habilitado ? 1 : 0" :options="SIM_NAO" label="Habilitado" :clearable="false" @update:model-value="(v) => (config.habilitado = num(v as number) === 1)" />
        </div>
        <div class="col-4"><SelectField v-model="config.metodo" :options="METODO_OPTIONS" label="Método" :clearable="false" /></div>
        <div class="col-4">
          <SelectField :model-value="config.incluirTributos ? 1 : 0" :options="SIM_NAO" label="Incluir tributos" :clearable="false" @update:model-value="(v) => (config.incluirTributos = num(v as number) === 1)" />
        </div>
        <div class="col-4">
          <SelectField :model-value="config.incluirFrete ? 1 : 0" :options="SIM_NAO" label="Incluir frete" :clearable="false" @update:model-value="(v) => (config.incluirFrete = num(v as number) === 1)" />
        </div>
        <div class="col-4">
          <SelectField :model-value="config.incluirDespesas ? 1 : 0" :options="SIM_NAO" label="Incluir despesas" :clearable="false" @update:model-value="(v) => (config.incluirDespesas = num(v as number) === 1)" />
        </div>
      </div>
      <div class="acoes-form">
        <button type="button" class="btn btn-primary" :disabled="salvandoConfig" @click="salvarConfig">
          <span v-if="salvandoConfig" class="spinner"></span>
          <span v-else>Salvar configuração</span>
        </button>
      </div>
    </section>

    <section class="glass-panel bloco">
      <h3 class="bloco-titulo">Gerir Comércio Exterior de uma Compra</h3>
      <div class="field">
        <label class="field-label">Localizar compra</label>
        <input class="input" type="text" placeholder="Fornecedor, número da nota ou chave (mín. 2 caracteres)" :value="termo" @input="aoDigitar(($event.target as HTMLInputElement).value)" />
      </div>
      <div v-if="buscando" class="aviso">Buscando...</div>
      <ul v-else-if="resultados.length" class="lista">
        <li v-for="c in resultados" :key="c.id" class="item" @click="abrirCompra(c)">
          <span class="item-nome">{{ c.fornecedorNome || 'Fornecedor não identificado' }} · Nota {{ c.numeroNota || '-' }}</span>
          <span class="item-meta">{{ formatarData(c.dataEmissao) }} · {{ formatarMoeda(c.valorTotal) }}</span>
        </li>
      </ul>
      <p v-else-if="termo.length >= 2" class="aviso">Nenhuma compra encontrada.</p>
    </section>
  </div>
</template>

<style scoped>
.bloco { margin-bottom: 16px; }
section.bloco { padding: 16px; }
.bloco-titulo { font-size: 15px; font-weight: 600; margin: 0 0 12px; }
.aviso { color: var(--text-secondary); font-size: 13px; margin: 4px 0 12px; }
.acoes-form { display: flex; justify-content: flex-end; margin-top: 12px; }
.lista { list-style: none; margin: 8px 0 0; padding: 0; max-height: 320px; overflow-y: auto; }
.item { display: flex; flex-direction: column; gap: 2px; padding: 10px 12px; border-radius: 8px; cursor: pointer; }
.item:hover { background: rgba(255, 255, 255, 0.06); }
.item-nome { font-weight: 600; font-size: 14px; }
.item-meta { font-size: 12px; color: var(--text-secondary); }
</style>
