<script setup lang="ts">
/**
 * Propostas — Imobiliária / Propostas (ID2).
 *
 * Fonte: GET /imobiliaria/propostas?imovelId (via useImobiliaria.listarPropostas).
 * Fluxo: criar (POST), aprovar / rejeitar (rascunho) e converter (aprovada → locação/aquisição).
 * As transições espelham a máquina de estado do backend; o front só dispara os comandos.
 */
import { ref, computed, onMounted } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import {
  useImobiliaria,
  rotularEnum,
  TIPO_PROPOSTA,
  STATUS_PROPOSTA,
  PAPEL_PARTE_PROPOSTA,
  type Proposta
} from '~/composables/useImobiliaria'
import type { SelectOption } from '~/composables/useEnum'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ layout: 'default' })

const imo = useImobiliaria()
const toast = useToast()
const { formatarMoeda, formatarData } = useHelper()

interface Imovel { id: string; descricao?: string | null }
interface Pessoa { id: string; nome?: string | null; razaoSocial?: string | null }

const propostas = ref<Proposta[]>([])
const imoveis = ref<Imovel[]>([])
const pessoas = ref<Pessoa[]>([])
const carregando = ref(false)
const filtroImovel = ref<string | null>(null)

const opcoesTipo: SelectOption[] = Object.entries(TIPO_PROPOSTA).map(([v, l]) => ({ label: l, value: Number(v) }))
const opcoesPapel: SelectOption[] = Object.entries(PAPEL_PARTE_PROPOSTA).map(([v, l]) => ({ label: l, value: Number(v) }))
const opcoesImoveis = computed<SelectOption[]>(() => imoveis.value.map((i) => ({ label: i.descricao ?? i.id, value: i.id })))
const opcoesImoveisFiltro = computed<SelectOption[]>(() => [{ label: 'Todos os imóveis', value: '' }, ...opcoesImoveis.value])
const opcoesPessoas = computed<SelectOption[]>(() => pessoas.value.map((p) => ({ label: p.razaoSocial ?? p.nome ?? p.id, value: p.id })))

function nomeImovel(id: string): string {
  return imoveis.value.find((i) => i.id === id)?.descricao ?? id
}

async function carregar() {
  carregando.value = true
  try {
    propostas.value = (await imo.listarPropostas(filtroImovel.value)) ?? []
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function carregarApoios() {
  try {
    const [ri, rp] = await Promise.all([
      useApi('/imobiliaria/imoveis'),
      useApi('/cadastros/pessoas', { query: { tamanhoPagina: 500 } })
    ])
    imoveis.value = extrairDados<Imovel[]>(ri) ?? []
    pessoas.value = extrairDados<Pessoa[]>(rp) ?? []
  } catch (e) {
    console.error('[propostas] apoios', e)
  }
}

// ---- Criar proposta ----
interface ParteInput { pessoaId: string | null; papel: number }
interface NovaProposta {
  tipo: number
  imovelId: string | null
  validade: string | null
  valorProposto: number | null
  observacao: string | null
  partes: ParteInput[]
}
const criarVisivel = ref(false)
const salvando = ref(false)
const form = ref<NovaProposta>(novoForm())
function novoForm(): NovaProposta {
  return { tipo: 0, imovelId: null, validade: null, valorProposto: 0, observacao: null, partes: [{ pessoaId: null, papel: 0 }] }
}
function abrirCriar() {
  form.value = novoForm()
  criarVisivel.value = true
}
function addParte() { form.value.partes.push({ pessoaId: null, papel: 1 }) }
function removerParte(i: number) { form.value.partes.splice(i, 1) }

async function salvarProposta() {
  if (!form.value.imovelId) { toast.warning('Selecione o imóvel.'); return }
  if (!form.value.validade) { toast.warning('Informe a validade.'); return }
  if (!form.value.valorProposto || form.value.valorProposto <= 0) { toast.warning('O valor proposto deve ser positivo.'); return }
  salvando.value = true
  try {
    await imo.criarProposta({
      tipo: form.value.tipo,
      imovelId: form.value.imovelId,
      validade: form.value.validade,
      valorProposto: form.value.valorProposto,
      observacao: form.value.observacao,
      partes: form.value.partes.filter((p) => p.pessoaId).map((p) => ({ pessoaId: p.pessoaId as string, papel: p.papel }))
    })
    toast.success('Proposta criada com sucesso.')
    criarVisivel.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

// ---- Transições ----
const acaoEmAndamento = ref<string | null>(null)
async function executar(id: string, fn: () => Promise<unknown>, ok: string) {
  acaoEmAndamento.value = id
  try {
    await fn()
    toast.success(ok)
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmAndamento.value = null
  }
}
const aprovar = (p: Proposta) => executar(p.id, () => imo.aprovarProposta(p.id), 'Proposta aprovada.')
const rejeitar = (p: Proposta) => executar(p.id, () => imo.rejeitarProposta(p.id), 'Proposta rejeitada.')

// ---- Converter ----
const converterVisivel = ref(false)
const convertendo = ref(false)
const propostaConverter = ref<Proposta | null>(null)
const conv = ref<{ periodoInicial: string | null; periodoFinal: string | null; vencimento: number }>({
  periodoInicial: null, periodoFinal: null, vencimento: 10
})
const opcoesVencimento: SelectOption[] = Array.from({ length: 31 }, (_, i) => ({ label: String(i + 1), value: i + 1 }))
function abrirConverter(p: Proposta) {
  propostaConverter.value = p
  conv.value = { periodoInicial: null, periodoFinal: null, vencimento: 10 }
  converterVisivel.value = true
}
async function confirmarConverter() {
  if (!propostaConverter.value) return
  convertendo.value = true
  try {
    await imo.converterProposta(propostaConverter.value.id, {
      periodoInicial: conv.value.periodoInicial,
      periodoFinal: conv.value.periodoFinal,
      vencimento: conv.value.vencimento
    })
    toast.success('Proposta convertida.')
    converterVisivel.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    convertendo.value = false
  }
}

function podeDecidir(p: Proposta): boolean { return p.status === 0 } // Rascunho
function podeConverter(p: Proposta): boolean { return p.status === 1 } // Aprovada

onMounted(async () => {
  await carregarApoios()
  await carregar()
})
</script>

<template>
  <div>
    <PageToolbar title="Propostas" subtitle="Propostas de locação e aquisição (ID2)" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirCriar">+ Nova proposta</button>
      </template>
    </PageToolbar>

    <div class="filtro-bar glass-panel">
      <SelectField v-model="filtroImovel" label="Imóvel" :options="opcoesImoveisFiltro" :clearable="false" @change="carregar" />
    </div>

    <div class="glass-panel tabela-panel">
      <div v-if="carregando" class="vazio">Carregando…</div>
      <div v-else-if="propostas.length === 0" class="vazio">Nenhuma proposta encontrada.</div>
      <table v-else class="tabela">
        <thead>
          <tr>
            <th>Imóvel</th>
            <th>Tipo</th>
            <th>Validade</th>
            <th class="num">Valor proposto</th>
            <th class="center">Status</th>
            <th class="acoes-col">Ações</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="p in propostas" :key="p.id">
            <td>{{ nomeImovel(p.imovelId) }}</td>
            <td>{{ rotularEnum(TIPO_PROPOSTA, p.tipo) }}</td>
            <td>{{ formatarData(p.validade) }}</td>
            <td class="num">{{ formatarMoeda(p.valorProposto) }}</td>
            <td class="center"><span class="badge" :class="`st-${p.status}`">{{ rotularEnum(STATUS_PROPOSTA, p.status) }}</span></td>
            <td class="acoes-col">
              <button v-if="podeDecidir(p)" type="button" class="btn btn-ghost btn-sm" :disabled="acaoEmAndamento === p.id" @click="aprovar(p)">Aprovar</button>
              <button v-if="podeDecidir(p)" type="button" class="btn btn-ghost btn-sm btn-danger-action" :disabled="acaoEmAndamento === p.id" @click="rejeitar(p)">Rejeitar</button>
              <button v-if="podeConverter(p)" type="button" class="btn btn-ghost btn-sm" :disabled="acaoEmAndamento === p.id" @click="abrirConverter(p)">Converter</button>
              <span v-if="!podeDecidir(p) && !podeConverter(p)" class="text-muted">—</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Criar proposta -->
    <AppDialog v-model="criarVisivel" title="Nova proposta" width="600px">
      <div class="form-grid">
        <SelectField v-model="form.tipo" label="Tipo" :options="opcoesTipo" :clearable="false" />
        <SelectField v-model="form.imovelId" label="Imóvel" :options="opcoesImoveis" required />
        <DateTimeField v-model="form.validade" label="Validade" required />
        <MoneyInput v-model="form.valorProposto" label="Valor proposto" required :min="0" />
      </div>
      <TextField v-model="form.observacao" label="Observação" class="mt-2" />

      <section class="repeater-section">
        <h4 class="section-title">Partes</h4>
        <div v-for="(parte, i) in form.partes" :key="`parte-${i}`" class="repeater-row">
          <SelectField v-model="parte.pessoaId" label="Pessoa" :options="opcoesPessoas" class="repeater-grow" />
          <SelectField v-model="parte.papel" label="Papel" :options="opcoesPapel" :clearable="false" class="repeater-papel" />
          <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="removerParte(i)">Remover</button>
        </div>
        <button type="button" class="btn btn-secondary btn-sm mt-2" @click="addParte">+ Adicionar parte</button>
      </section>

      <template #footer>
        <button type="button" class="btn btn-secondary" @click="criarVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvarProposta">
          <span v-if="salvando" class="spinner"></span><span v-else>Criar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Converter proposta -->
    <AppDialog v-model="converterVisivel" title="Converter proposta" width="520px">
      <p class="dialog-hint">
        Converter a proposta aprovada. Para propostas de <strong>locação</strong>, gera-se a locação (Em elaboração)
        com o valor e proponentes da proposta. Informe o período e o vencimento da locação gerada.
      </p>
      <div class="form-grid">
        <DateTimeField v-model="conv.periodoInicial" label="Início do período" />
        <DateTimeField v-model="conv.periodoFinal" label="Fim do período" />
        <SelectField v-model="conv.vencimento" label="Vencimento (dia)" :options="opcoesVencimento" :clearable="false" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="converterVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="convertendo" @click="confirmarConverter">
          <span v-if="convertendo" class="spinner"></span><span v-else>Converter</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.filtro-bar { display: flex; align-items: flex-end; gap: 16px; padding: 16px 18px; margin-bottom: 16px; max-width: 360px; }
.tabela-panel { padding: 8px 4px; overflow-x: auto; }
.vazio { padding: 40px 0; text-align: center; color: var(--text-secondary); font-size: 13px; }
.tabela { width: 100%; border-collapse: collapse; font-size: 13px; }
.tabela th, .tabela td { padding: 10px 12px; text-align: left; border-bottom: 1px solid var(--border-color); }
.tabela th { font-size: 12px; text-transform: uppercase; letter-spacing: 0.03em; color: var(--text-secondary); }
.tabela .num { text-align: right; white-space: nowrap; }
.tabela .center { text-align: center; }
.acoes-col { white-space: nowrap; text-align: right; }
.badge { display: inline-block; padding: 3px 10px; border-radius: 12px; font-size: 12px; font-weight: 600; background: var(--border-color); color: var(--text-primary); }
.badge.st-1 { background: color-mix(in srgb, var(--success) 20%, transparent); color: var(--success); }
.badge.st-2 { background: color-mix(in srgb, var(--danger) 20%, transparent); color: var(--danger); }
.badge.st-4 { background: color-mix(in srgb, var(--primary) 20%, transparent); color: var(--primary); }
.text-muted { color: var(--text-secondary); }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 14px; }
.repeater-section { margin-top: 20px; padding-top: 14px; border-top: 1px solid var(--border-color); }
.section-title { margin: 0 0 12px; font-size: 14px; font-weight: 700; color: var(--text-primary); }
.repeater-row { display: flex; align-items: flex-end; gap: 12px; margin-bottom: 12px; }
.repeater-grow { flex: 1; }
.repeater-papel { width: 160px; }
.dialog-hint { font-size: 13px; color: var(--text-secondary); line-height: 1.5; margin: 0 0 16px; }
.mt-2 { margin-top: 12px; }
</style>
