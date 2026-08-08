<script setup lang="ts">
/**
 * Formulário de Ativo Fixo (novo/edição) + painéis de depreciação e movimentação — Contabilidade.
 *
 * Contrato: GET/POST/PUT /ativos-fixos, POST /ativos-fixos/{id}/baixar,
 *           GET/POST /ativos-fixos/{id}/depreciacoes, GET /ativos-fixos/{id}/movimentacoes.
 */
import { computed, reactive, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import {
  useAtivoFixo, type AtivoFixoPayload, type GrupoBem,
  type DepreciacaoAtivo, type MovimentacaoAtivo,
  TIPO_DEPRECIACAO_ATIVO, STATUS_ATIVO_FIXO
} from '~/composables/useAtivoFixo'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { formatarMoeda, formatarData } = useHelper()
const api = useAtivoFixo()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const grupos = ref<GrupoBem[]>([])
const status = ref<number>(0)
const depreciacoes = ref<DepreciacaoAtivo[]>([])
const movimentacoes = ref<MovimentacaoAtivo[]>([])

const form = reactive<AtivoFixoPayload>({
  descricao: '', valorCompra: 0, dataAquisicao: '', grupoBemId: null,
  nome: '', numeroSerie: '', numeroNotaFiscal: '', chaveNfe: '', valorOriginal: null,
  deprecia: true, tipoDepreciacao: 0, taxaAnual: null, taxaMensal: null
})
const erros = reactive<Record<string, string>>({})

const opcoesGrupo = computed<SelectOption[]>(() => grupos.value.map((g) => ({ label: `${g.codigo ? g.codigo + ' — ' : ''}${g.nome}`, value: g.id })))
const opcoesTipoDep: SelectOption[] = TIPO_DEPRECIACAO_ATIVO

// Painel: registrar depreciação
const depForm = reactive({ competencia: '', valor: 0, metodoDepreciacao: '', taxaAplicada: null as number | null })
const salvandoDep = ref(false)
// Painel: baixa
const baixaForm = reactive({ dataBaixa: '', valorBaixa: null as number | null, observacao: '' })
const baixando = ref(false)
const mostrarBaixa = ref(false)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.descricao?.trim()) erros.descricao = 'Descrição é obrigatória.'
  if (!(form.valorCompra > 0)) erros.valorCompra = 'Informe o valor de compra.'
  if (!form.dataAquisicao) erros.dataAquisicao = 'Informe a data de aquisição.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  try { grupos.value = await api.listarGrupos() } catch { /* opcional */ }
  if (!isEdit.value) return
  carregando.value = true
  try {
    const a = await api.obter(idParam)
    if (a) {
      Object.assign(form, {
        descricao: a.descricao, valorCompra: a.valorCompra, dataAquisicao: (a.dataAquisicao ?? '').slice(0, 10),
        grupoBemId: a.grupoBemId ?? null, nome: a.nome ?? '', numeroSerie: a.numeroSerie ?? '',
        numeroNotaFiscal: a.numeroNotaFiscal ?? '', chaveNfe: a.chaveNfe ?? '', valorOriginal: a.valorOriginal ?? null,
        deprecia: a.deprecia, tipoDepreciacao: a.tipoDepreciacao ?? 0, taxaAnual: a.taxaAnual ?? null, taxaMensal: a.taxaMensal ?? null
      })
      status.value = a.status
    }
    depreciacoes.value = await api.listarDepreciacoes(idParam)
    movimentacoes.value = await api.listarMovimentacoes(idParam)
  } catch (e) {
    toast.error('Falha ao carregar o ativo.')
    console.error('[ativos-fixos/[id]] carregar', e)
  } finally {
    carregando.value = false
  }
}

async function salvar() {
  if (!validar()) { toast.error('Revise os campos obrigatórios.'); return }
  salvando.value = true
  try {
    const ok = isEdit.value ? await api.atualizar(idParam, form) : await api.criar(form)
    if (ok) { toast.success('Ativo salvo com sucesso.'); router.push('/erp/contabilidade/ativos-fixos') }
    else toast.error(api.erro.value ?? 'Falha ao salvar.')
  } finally {
    salvando.value = false
  }
}

async function registrarDepreciacao() {
  if (!depForm.competencia || !(depForm.valor > 0)) { toast.error('Informe competência e valor da depreciação.'); return }
  salvandoDep.value = true
  try {
    const ok = await api.registrarDepreciacao(idParam, { ...depForm })
    if (ok) { toast.success('Depreciação registrada.'); depForm.competencia = ''; depForm.valor = 0; depForm.taxaAplicada = null; depreciacoes.value = await api.listarDepreciacoes(idParam) }
    else toast.error(api.erro.value ?? 'Falha ao registrar depreciação.')
  } finally { salvandoDep.value = false }
}

async function confirmarBaixa() {
  if (!baixaForm.dataBaixa) { toast.error('Informe a data de baixa.'); return }
  baixando.value = true
  try {
    const ok = await api.baixar(idParam, { ...baixaForm })
    if (ok) { toast.success('Ativo baixado.'); mostrarBaixa.value = false; await carregar() }
    else toast.error(api.erro.value ?? 'Falha ao baixar.')
  } finally { baixando.value = false }
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar
      :title="isEdit ? 'Editar ativo fixo' : 'Novo ativo fixo'"
      subtitle="Imobilizado — dados do bem, depreciação e movimentações"
      :loading="carregando"
    >
      <template #actions>
        <button type="button" class="btn btn-ghost" @click="router.push('/erp/contabilidade/ativos-fixos')">Cancelar</button>
        <button v-if="isEdit && status === 0" type="button" class="btn btn-ghost btn-danger-action" @click="mostrarBaixa = !mostrarBaixa">Baixar ativo</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">{{ salvando ? 'Salvando…' : 'Salvar' }}</button>
      </template>
    </PageToolbar>

    <div v-if="isEdit" class="chip-status">
      Situação: <strong>{{ STATUS_ATIVO_FIXO[status] ?? status }}</strong>
    </div>

    <div class="card">
      <h3 class="card-title">Dados do bem</h3>
      <div class="grid">
        <TextField v-model="form.descricao" label="Descrição" required :error="erros.descricao" />
        <TextField v-model="form.nome" label="Nome / apelido" />
        <MoneyInput v-model="form.valorCompra" label="Valor de compra" required :error="erros.valorCompra" />
        <DateTimeField v-model="form.dataAquisicao" label="Data de aquisição" required :error="erros.dataAquisicao" />
        <SelectField v-model="form.grupoBemId" :options="opcoesGrupo" label="Grupo de bens" placeholder="Selecione…" />
        <MoneyInput v-model="form.valorOriginal" label="Valor original (opcional)" />
        <TextField v-model="form.numeroSerie" label="Número de série" />
        <TextField v-model="form.numeroNotaFiscal" label="Nota fiscal" />
        <TextField v-model="form.chaveNfe" label="Chave NF-e" />
      </div>
    </div>

    <div class="card">
      <h3 class="card-title">Depreciação</h3>
      <label class="check"><input type="checkbox" v-model="form.deprecia" /> Este bem deprecia</label>
      <div v-if="form.deprecia" class="grid">
        <SelectField v-model="form.tipoDepreciacao" :options="opcoesTipoDep" label="Método de depreciação" />
        <PercentInput v-model="form.taxaAnual" label="Taxa anual (%)" />
        <PercentInput v-model="form.taxaMensal" label="Taxa mensal (%)" />
      </div>
    </div>

    <template v-if="isEdit">
      <div v-if="mostrarBaixa" class="card card-danger">
        <h3 class="card-title">Baixar ativo</h3>
        <div class="grid">
          <DateTimeField v-model="baixaForm.dataBaixa" label="Data da baixa" required />
          <MoneyInput v-model="baixaForm.valorBaixa" label="Valor de baixa" />
          <TextField v-model="baixaForm.observacao" label="Observação" />
        </div>
        <button type="button" class="btn btn-danger" :disabled="baixando" @click="confirmarBaixa">{{ baixando ? 'Processando…' : 'Confirmar baixa' }}</button>
      </div>

      <div class="card">
        <h3 class="card-title">Registrar depreciação mensal</h3>
        <div class="grid">
          <TextField v-model="depForm.competencia" label="Competência (AAAA-MM)" placeholder="2026-08" />
          <MoneyInput v-model="depForm.valor" label="Valor depreciado" />
          <PercentInput v-model="depForm.taxaAplicada" label="Taxa aplicada (%)" />
        </div>
        <button type="button" class="btn btn-secondary" :disabled="salvandoDep" @click="registrarDepreciacao">{{ salvandoDep ? 'Registrando…' : 'Registrar depreciação' }}</button>

        <h4 class="sub-title">Histórico de depreciações</h4>
        <table class="mini-table">
          <thead><tr><th>Competência</th><th class="r">Valor</th><th>Método</th></tr></thead>
          <tbody>
            <tr v-for="d in depreciacoes" :key="d.id"><td>{{ d.competencia }}</td><td class="r">{{ formatarMoeda(d.valor) }}</td><td>{{ d.metodoDepreciacao ?? '—' }}</td></tr>
            <tr v-if="!depreciacoes.length"><td colspan="3" class="empty">Sem depreciações registradas.</td></tr>
          </tbody>
        </table>
      </div>

      <div class="card">
        <h3 class="card-title">Movimentações patrimoniais</h3>
        <table class="mini-table">
          <thead><tr><th>Data</th><th>Tipo</th><th class="r">Valor</th><th>Observação</th></tr></thead>
          <tbody>
            <tr v-for="m in movimentacoes" :key="m.id"><td>{{ formatarData(m.dataMovimentacao) }}</td><td>{{ m.tipoMovimentacao }}</td><td class="r">{{ m.valor != null ? formatarMoeda(m.valor) : '—' }}</td><td>{{ m.observacao ?? '—' }}</td></tr>
            <tr v-if="!movimentacoes.length"><td colspan="4" class="empty">Sem movimentações.</td></tr>
          </tbody>
        </table>
      </div>
    </template>
  </div>
</template>

<style scoped>
.card { background: var(--surface, #fff); border: 1px solid var(--border, #e5e7eb); border-radius: 10px; padding: 1.25rem; margin-bottom: 1rem; }
.card-danger { border-color: #f2b8b5; }
.card-title { margin: 0 0 1rem; font-size: 1rem; font-weight: 600; }
.sub-title { margin: 1.25rem 0 .5rem; font-size: .9rem; font-weight: 600; }
.grid { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 1rem; }
@media (max-width: 900px) { .grid { grid-template-columns: 1fr; } }
.check { display: inline-flex; align-items: center; gap: .5rem; margin-bottom: 1rem; }
.chip-status { margin-bottom: 1rem; font-size: .9rem; color: var(--text-muted, #6b7280); }
.mini-table { width: 100%; border-collapse: collapse; font-size: .875rem; }
.mini-table th, .mini-table td { padding: .5rem .75rem; border-bottom: 1px solid var(--border, #eef0f2); text-align: left; }
.mini-table th.r, .mini-table td.r { text-align: right; }
.mini-table .empty { color: var(--text-muted, #9ca3af); text-align: center; }
</style>
