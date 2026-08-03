<script setup lang="ts">
/**
 * Gestão do contrato de locação — Imobiliária / Contratos.
 *
 * Console de operação de uma locação (id na rota). Reúne, em abas, tudo que a locação precisa
 * além do cadastro básico:
 *   - Resumo        → GET /imobiliaria/locacoes/{id}/resumo-aluguel
 *   - Cobranças     → ciclo financeiro do aluguel (gerar / baixa / estorno / recibo — ID8/NF-01)
 *   - Garantias     → adicionar / substituir / liberar (ID6/NF-04)
 *   - Reajustes     → aplicar reajuste ratificado + histórico (ID7/NF-02)
 *   - Ciclo         → transições de estado (formalizar / encerrar / cancelar / renovar) e rescisão (ID7)
 *
 * ⚠️ Só apresentação/orquestração. Juros/multa/desconto e a máquina do recebível vivem no
 * FINANCEIRO; índice de reajuste e cálculo da multa de rescisão são valida-contador — o front
 * apenas informa os valores ratificados e dispara os comandos do backend.
 */
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import {
  useImobiliaria,
  rotularEnum,
  TIPO_COBRANCA,
  STATUS_COBRANCA,
  TIPO_GARANTIA,
  STATUS_GARANTIA,
  type ResumoAluguel,
  type CobrancaAluguel,
  type LocacaoGarantia,
  type LocacaoReajuste
} from '~/composables/useImobiliaria'
import type { SelectOption } from '~/composables/useEnum'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ layout: 'default' })

const route = useRoute()
const toast = useToast()
const imo = useImobiliaria()
const { formatarMoeda, formatarData } = useHelper()

const locacaoId = route.params.id as string

type Aba = 'resumo' | 'cobrancas' | 'garantias' | 'reajustes' | 'ciclo'
const aba = ref<Aba>('resumo')
const abas: { key: Aba; label: string }[] = [
  { key: 'resumo', label: 'Resumo' },
  { key: 'cobrancas', label: 'Cobranças' },
  { key: 'garantias', label: 'Garantias' },
  { key: 'reajustes', label: 'Reajustes' },
  { key: 'ciclo', label: 'Ciclo do contrato' }
]

const resumo = ref<ResumoAluguel | null>(null)
const cobrancas = ref<CobrancaAluguel[]>([])
const garantias = ref<LocacaoGarantia[]>([])
const reajustes = ref<LocacaoReajuste[]>([])
const carregando = ref(false)

const opcoesTipoCobranca: SelectOption[] = Object.entries(TIPO_COBRANCA).map(([v, l]) => ({ label: l, value: Number(v) }))
const opcoesTipoGarantia: SelectOption[] = Object.entries(TIPO_GARANTIA).map(([v, l]) => ({ label: l, value: Number(v) }))

async function carregarResumo() {
  try {
    resumo.value = await imo.resumoAluguel(locacaoId)
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}
async function carregarCobrancas() {
  cobrancas.value = (await imo.listarCobrancas(locacaoId)) ?? []
}
async function carregarGarantias() {
  garantias.value = (await imo.listarGarantias(locacaoId)) ?? []
}
async function carregarReajustes() {
  reajustes.value = (await imo.listarReajustes(locacaoId)) ?? []
}

async function trocarAba(a: Aba) {
  aba.value = a
  carregando.value = true
  try {
    if (a === 'cobrancas') await carregarCobrancas()
    else if (a === 'garantias') await carregarGarantias()
    else if (a === 'reajustes') await carregarReajustes()
    else if (a === 'resumo') await carregarResumo()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

// ============================ COBRANÇAS ============================
const gerarVisivel = ref(false)
const gerando = ref(false)
const hoje = new Date()
const gerarForm = ref<{ ano: number; mes: number; tipo: number; valorOverride: number | null }>({
  ano: hoje.getFullYear(), mes: hoje.getMonth() + 1, tipo: 0, valorOverride: null
})
const opcoesAno: SelectOption[] = Array.from({ length: 6 }, (_, i) => {
  const a = hoje.getFullYear() - 2 + i
  return { label: String(a), value: a }
})
const opcoesMes: SelectOption[] = Array.from({ length: 12 }, (_, i) => ({ label: String(i + 1).padStart(2, '0'), value: i + 1 }))

async function gerarCobranca() {
  gerando.value = true
  try {
    await imo.gerarCobranca(locacaoId, {
      ano: gerarForm.value.ano,
      mes: gerarForm.value.mes,
      tipo: gerarForm.value.tipo,
      valorOverride: gerarForm.value.valorOverride || null
    })
    toast.success('Cobrança gerada.')
    gerarVisivel.value = false
    await carregarCobrancas()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    gerando.value = false
  }
}

// Baixa
const baixaVisivel = ref(false)
const baixando = ref(false)
const cobrancaAlvo = ref<CobrancaAluguel | null>(null)
const baixaForm = ref<{ valorPago: number | null; receberRef: string | null; dataBaixa: string | null }>({
  valorPago: 0, receberRef: null, dataBaixa: null
})
function abrirBaixa(c: CobrancaAluguel) {
  cobrancaAlvo.value = c
  baixaForm.value = { valorPago: c.valor - c.valorPago, receberRef: c.receberRef ?? null, dataBaixa: null }
  baixaVisivel.value = true
}
async function confirmarBaixa() {
  if (!cobrancaAlvo.value) return
  if (!baixaForm.value.valorPago || baixaForm.value.valorPago <= 0) { toast.warning('O valor da baixa deve ser positivo.'); return }
  baixando.value = true
  try {
    await imo.refletirBaixa(cobrancaAlvo.value.id, {
      valorPago: baixaForm.value.valorPago,
      receberRef: baixaForm.value.receberRef,
      dataBaixa: baixaForm.value.dataBaixa
    })
    toast.success('Baixa refletida.')
    baixaVisivel.value = false
    await carregarCobrancas()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    baixando.value = false
  }
}

const acaoCobranca = ref<string | null>(null)
async function execCobranca(c: CobrancaAluguel, fn: () => Promise<unknown>, ok: string) {
  acaoCobranca.value = c.id
  try {
    await fn()
    toast.success(ok)
    await carregarCobrancas()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoCobranca.value = null
  }
}
const estornar = (c: CobrancaAluguel) => execCobranca(c, () => imo.estornarBaixa(c.id), 'Baixa estornada.')
const recibo = (c: CobrancaAluguel) => execCobranca(c, () => imo.emitirRecibo(c.id), 'Recibo emitido.')

// ============================ GARANTIAS ============================
const garantiaVisivel = ref(false)
const salvandoGarantia = ref(false)
const garantiaSubstituir = ref<LocacaoGarantia | null>(null)
const garantiaForm = ref<{ tipo: number; valorLimite: number | null; vigenciaInicio: string | null; vigenciaFim: string | null; descricao: string | null }>({
  tipo: 1, valorLimite: 0, vigenciaInicio: null, vigenciaFim: null, descricao: null
})
function abrirGarantia(base?: LocacaoGarantia) {
  garantiaSubstituir.value = base ?? null
  garantiaForm.value = base
    ? { tipo: base.tipo, valorLimite: base.valorLimite, vigenciaInicio: base.vigenciaInicio ?? null, vigenciaFim: base.vigenciaFim ?? null, descricao: base.descricao ?? null }
    : { tipo: 1, valorLimite: 0, vigenciaInicio: null, vigenciaFim: null, descricao: null }
  garantiaVisivel.value = true
}
async function salvarGarantia() {
  salvandoGarantia.value = true
  const body = {
    tipo: garantiaForm.value.tipo,
    valorLimite: garantiaForm.value.valorLimite ?? 0,
    vigenciaInicio: garantiaForm.value.vigenciaInicio,
    vigenciaFim: garantiaForm.value.vigenciaFim,
    descricao: garantiaForm.value.descricao
  }
  try {
    if (garantiaSubstituir.value) await imo.substituirGarantia(garantiaSubstituir.value.id, body)
    else await imo.adicionarGarantia(locacaoId, body)
    toast.success(garantiaSubstituir.value ? 'Garantia substituída.' : 'Garantia adicionada.')
    garantiaVisivel.value = false
    await carregarGarantias()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoGarantia.value = false
  }
}
const acaoGarantia = ref<string | null>(null)
async function liberarGarantia(g: LocacaoGarantia) {
  acaoGarantia.value = g.id
  try {
    await imo.liberarGarantia(g.id)
    toast.success('Garantia liberada.')
    await carregarGarantias()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoGarantia.value = null
  }
}

// ============================ REAJUSTES ============================
const reajusteVisivel = ref(false)
const aplicandoReajuste = ref(false)
const reajusteForm = ref<{ novoValor: number | null; indice: string | null; dataBase: string | null; percentualAplicado: string | null }>({
  novoValor: 0, indice: null, dataBase: null, percentualAplicado: null
})
function paraNumeroOuNulo(v: string | null): number | null {
  if (v == null || String(v).trim() === '') return null
  const n = Number(v)
  return Number.isNaN(n) ? null : n
}
function abrirReajuste() {
  reajusteForm.value = { novoValor: resumo.value?.valor ?? 0, indice: null, dataBase: null, percentualAplicado: null }
  reajusteVisivel.value = true
}
async function aplicarReajuste() {
  if (!reajusteForm.value.novoValor || reajusteForm.value.novoValor <= 0) { toast.warning('O novo valor deve ser positivo.'); return }
  aplicandoReajuste.value = true
  try {
    await imo.aplicarReajuste(locacaoId, {
      novoValor: reajusteForm.value.novoValor,
      indice: reajusteForm.value.indice,
      dataBase: reajusteForm.value.dataBase,
      percentualAplicado: paraNumeroOuNulo(reajusteForm.value.percentualAplicado)
    })
    toast.success('Reajuste aplicado.')
    reajusteVisivel.value = false
    await Promise.all([carregarReajustes(), carregarResumo()])
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    aplicandoReajuste.value = false
  }
}

// ============================ CICLO ============================
const acaoCiclo = ref<string | null>(null)
async function execCiclo(nome: string, fn: () => Promise<unknown>, ok: string) {
  acaoCiclo.value = nome
  try {
    await fn()
    toast.success(ok)
    await carregarResumo()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoCiclo.value = null
  }
}
const formalizar = () => execCiclo('formalizar', () => imo.formalizar(locacaoId), 'Contrato formalizado (vigente).')
const encerrar = () => execCiclo('encerrar', () => imo.encerrar(locacaoId), 'Contrato encerrado.')

const cancelarVisivel = ref(false)
const confirmarCancelamento = () => {
  cancelarVisivel.value = false
  execCiclo('cancelar', () => imo.cancelar(locacaoId), 'Contrato cancelado.')
}

// Renovar
const renovarVisivel = ref(false)
const renovando = ref(false)
const renovarForm = ref<{ novoPeriodoFinal: string | null }>({ novoPeriodoFinal: null })
async function confirmarRenovar() {
  if (!renovarForm.value.novoPeriodoFinal) { toast.warning('Informe o novo fim do período.'); return }
  renovando.value = true
  try {
    await imo.renovar(locacaoId, { novoPeriodoFinal: renovarForm.value.novoPeriodoFinal })
    toast.success('Contrato renovado.')
    renovarVisivel.value = false
    await carregarResumo()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    renovando.value = false
  }
}

// Rescindir
const rescindirVisivel = ref(false)
const rescindindo = ref(false)
const rescindirForm = ref<{ motivo: string | null; dataRescisao: string | null; avisoPrevioDias: string | null; multaProporcional: number | null }>({
  motivo: null, dataRescisao: null, avisoPrevioDias: null, multaProporcional: 0
})
async function confirmarRescindir() {
  if (!rescindirForm.value.motivo) { toast.warning('Informe o motivo da rescisão.'); return }
  if (!rescindirForm.value.dataRescisao) { toast.warning('Informe a data da rescisão.'); return }
  rescindindo.value = true
  try {
    await imo.rescindir(locacaoId, {
      motivo: rescindirForm.value.motivo,
      dataRescisao: rescindirForm.value.dataRescisao,
      avisoPrevioDias: paraNumeroOuNulo(rescindirForm.value.avisoPrevioDias),
      multaProporcional: rescindirForm.value.multaProporcional ?? 0
    })
    toast.success('Locação rescindida.')
    rescindirVisivel.value = false
    await carregarResumo()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    rescindindo.value = false
  }
}

const saldoCobranca = (c: CobrancaAluguel) => Math.max(0, c.valor - c.valorPago)
const podeBaixar = (c: CobrancaAluguel) => c.status !== 2 && c.status !== 3 // não pago/estornado
const podeEstornar = (c: CobrancaAluguel) => c.status === 1 || c.status === 2 // parcial/pago
const podeRecibo = (c: CobrancaAluguel) => c.status === 2 // pago

onMounted(carregarResumo)
</script>

<template>
  <div>
    <PageToolbar title="Gestão do contrato" subtitle="Cobranças, garantias, reajustes e ciclo da locação" :loading="carregando">
      <template #actions>
        <NuxtLink to="/erp/imobiliaria/locacoes" class="btn btn-secondary">Voltar</NuxtLink>
      </template>
    </PageToolbar>

    <!-- Cabeçalho resumo -->
    <section class="resumo-head glass-panel">
      <div><span class="rh-label">Valor</span><span class="rh-valor">{{ formatarMoeda(resumo?.valor ?? 0) }}</span></div>
      <div><span class="rh-label">Vencimento (dia)</span><span class="rh-valor">{{ resumo?.vencimento ?? '—' }}</span></div>
      <div><span class="rh-label">Período</span><span class="rh-valor">{{ formatarData(resumo?.periodoInicial) || '—' }} → {{ formatarData(resumo?.periodoFinal) || '—' }}</span></div>
      <div><span class="rh-label">Status</span><span class="rh-valor">{{ resumo?.status ?? '—' }}</span></div>
      <div><span class="rh-label">Locatários</span><span class="rh-valor">{{ resumo?.locatarios?.length ?? 0 }}</span></div>
      <div><span class="rh-label">Fiadores</span><span class="rh-valor">{{ resumo?.fiadores?.length ?? 0 }}</span></div>
    </section>

    <!-- Abas -->
    <div class="tabs">
      <button v-for="t in abas" :key="t.key" type="button" class="tab" :class="{ ativo: aba === t.key }" @click="trocarAba(t.key)">
        {{ t.label }}
      </button>
    </div>

    <!-- ABA RESUMO -->
    <div v-if="aba === 'resumo'" class="glass-panel painel">
      <p class="painel-info">
        Use as abas acima para operar a locação: gerar e baixar cobranças, gerir garantias, aplicar reajustes
        e conduzir o ciclo do contrato (formalizar, renovar, encerrar, cancelar ou rescindir).
      </p>
      <ul class="resumo-list">
        <li v-for="(loc, i) in resumo?.locatarios ?? []" :key="`l-${i}`"><strong>Locatário:</strong> {{ loc }}</li>
        <li v-for="(fia, i) in resumo?.fiadores ?? []" :key="`f-${i}`"><strong>Fiador:</strong> {{ fia }}</li>
      </ul>
    </div>

    <!-- ABA COBRANÇAS -->
    <div v-else-if="aba === 'cobrancas'" class="glass-panel painel">
      <div class="painel-topo">
        <h3 class="painel-titulo">Cobranças do aluguel</h3>
        <button type="button" class="btn btn-primary btn-sm" @click="gerarVisivel = true">+ Gerar cobrança</button>
      </div>
      <div v-if="carregando" class="vazio">Carregando…</div>
      <div v-else-if="cobrancas.length === 0" class="vazio">Nenhuma cobrança gerada.</div>
      <table v-else class="tabela">
        <thead>
          <tr>
            <th>Competência</th><th>Tipo</th><th class="num">Valor</th><th class="num">Pago</th><th class="num">Saldo</th>
            <th class="center">Status</th><th class="acoes-col">Ações</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in cobrancas" :key="c.id">
            <td>{{ formatarData(c.competencia) }}</td>
            <td>{{ rotularEnum(TIPO_COBRANCA, c.tipo) }}</td>
            <td class="num">{{ formatarMoeda(c.valor) }}</td>
            <td class="num">{{ formatarMoeda(c.valorPago) }}</td>
            <td class="num">{{ formatarMoeda(saldoCobranca(c)) }}</td>
            <td class="center"><span class="badge" :class="`stc-${c.status}`">{{ rotularEnum(STATUS_COBRANCA, c.status) }}</span></td>
            <td class="acoes-col">
              <button v-if="podeBaixar(c)" type="button" class="btn btn-ghost btn-sm" :disabled="acaoCobranca === c.id" @click="abrirBaixa(c)">Baixar</button>
              <button v-if="podeEstornar(c)" type="button" class="btn btn-ghost btn-sm btn-danger-action" :disabled="acaoCobranca === c.id" @click="estornar(c)">Estornar</button>
              <button v-if="podeRecibo(c)" type="button" class="btn btn-ghost btn-sm" :disabled="acaoCobranca === c.id" @click="recibo(c)">Recibo</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- ABA GARANTIAS -->
    <div v-else-if="aba === 'garantias'" class="glass-panel painel">
      <div class="painel-topo">
        <h3 class="painel-titulo">Garantias</h3>
        <button type="button" class="btn btn-primary btn-sm" @click="abrirGarantia()">+ Adicionar garantia</button>
      </div>
      <div v-if="carregando" class="vazio">Carregando…</div>
      <div v-else-if="garantias.length === 0" class="vazio">Nenhuma garantia registrada.</div>
      <table v-else class="tabela">
        <thead>
          <tr><th>Tipo</th><th class="num">Valor limite</th><th>Vigência</th><th class="center">Status</th><th>Descrição</th><th class="acoes-col">Ações</th></tr>
        </thead>
        <tbody>
          <tr v-for="g in garantias" :key="g.id">
            <td>{{ rotularEnum(TIPO_GARANTIA, g.tipo) }}</td>
            <td class="num">{{ formatarMoeda(g.valorLimite) }}</td>
            <td>{{ formatarData(g.vigenciaInicio) || '—' }} → {{ formatarData(g.vigenciaFim) || '—' }}</td>
            <td class="center"><span class="badge" :class="`stg-${g.status}`">{{ rotularEnum(STATUS_GARANTIA, g.status) }}</span></td>
            <td>{{ g.descricao ?? '—' }}</td>
            <td class="acoes-col">
              <template v-if="g.status === 0">
                <button type="button" class="btn btn-ghost btn-sm" :disabled="acaoGarantia === g.id" @click="abrirGarantia(g)">Substituir</button>
                <button type="button" class="btn btn-ghost btn-sm btn-danger-action" :disabled="acaoGarantia === g.id" @click="liberarGarantia(g)">Liberar</button>
              </template>
              <span v-else class="text-muted">—</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- ABA REAJUSTES -->
    <div v-else-if="aba === 'reajustes'" class="glass-panel painel">
      <div class="painel-topo">
        <h3 class="painel-titulo">Histórico de reajustes</h3>
        <button type="button" class="btn btn-primary btn-sm" @click="abrirReajuste">+ Aplicar reajuste</button>
      </div>
      <p class="painel-info">O índice e o percentual são <strong>ratificados pelo contador</strong>; a Imobiliária apenas registra o valor aprovado.</p>
      <div v-if="carregando" class="vazio">Carregando…</div>
      <div v-else-if="reajustes.length === 0" class="vazio">Nenhum reajuste aplicado.</div>
      <table v-else class="tabela">
        <thead>
          <tr><th>Data base</th><th>Índice</th><th class="num">Valor anterior</th><th class="num">Valor novo</th><th class="num">% aplicado</th></tr>
        </thead>
        <tbody>
          <tr v-for="r in reajustes" :key="r.id">
            <td>{{ formatarData(r.dataBase) }}</td>
            <td>{{ r.indice ?? '—' }}</td>
            <td class="num">{{ formatarMoeda(r.valorAnterior) }}</td>
            <td class="num">{{ formatarMoeda(r.valorNovo) }}</td>
            <td class="num">{{ r.percentualAplicado != null ? `${r.percentualAplicado}%` : '—' }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- ABA CICLO -->
    <div v-else class="glass-panel painel">
      <h3 class="painel-titulo">Ciclo do contrato</h3>
      <p class="painel-info">Transições de estado da locação. A disponibilidade real de cada transição é validada pelo backend conforme o status atual.</p>
      <div class="ciclo-acoes">
        <button type="button" class="btn btn-primary" :disabled="acaoCiclo === 'formalizar'" @click="formalizar">Formalizar (tornar vigente)</button>
        <button type="button" class="btn btn-secondary" @click="renovarVisivel = true">Renovar</button>
        <button type="button" class="btn btn-secondary" :disabled="acaoCiclo === 'encerrar'" @click="encerrar">Encerrar</button>
        <button type="button" class="btn btn-secondary btn-danger-action" @click="rescindirVisivel = true">Rescindir</button>
        <button type="button" class="btn btn-secondary btn-danger-action" @click="cancelarVisivel = true">Cancelar</button>
      </div>
    </div>

    <!-- ===== DIALOGS ===== -->
    <!-- Gerar cobrança -->
    <AppDialog v-model="gerarVisivel" title="Gerar cobrança" width="480px">
      <div class="form-grid">
        <SelectField v-model="gerarForm.ano" label="Ano" :options="opcoesAno" :clearable="false" />
        <SelectField v-model="gerarForm.mes" label="Mês" :options="opcoesMes" :clearable="false" />
        <SelectField v-model="gerarForm.tipo" label="Tipo" :options="opcoesTipoCobranca" :clearable="false" />
        <MoneyInput v-model="gerarForm.valorOverride" label="Valor (opcional)" :min="0" hint="Vazio usa o valor da locação" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="gerarVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="gerando" @click="gerarCobranca">
          <span v-if="gerando" class="spinner"></span><span v-else>Gerar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Baixa -->
    <AppDialog v-model="baixaVisivel" title="Refletir baixa" width="480px">
      <p class="dialog-hint">A baixa é refletida do FINANCEIRO/Contas a Receber. Juros/multa/desconto não são calculados aqui.</p>
      <div class="form-grid">
        <MoneyInput v-model="baixaForm.valorPago" label="Valor pago" required :min="0" />
        <DateTimeField v-model="baixaForm.dataBaixa" label="Data da baixa" />
        <TextField v-model="baixaForm.receberRef" label="Ref. do recebível (opcional)" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="baixaVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="baixando" @click="confirmarBaixa">
          <span v-if="baixando" class="spinner"></span><span v-else>Confirmar baixa</span>
        </button>
      </template>
    </AppDialog>

    <!-- Garantia -->
    <AppDialog v-model="garantiaVisivel" :title="garantiaSubstituir ? 'Substituir garantia' : 'Adicionar garantia'" width="520px">
      <div class="form-grid">
        <SelectField v-model="garantiaForm.tipo" label="Tipo" :options="opcoesTipoGarantia" :clearable="false" />
        <MoneyInput v-model="garantiaForm.valorLimite" label="Valor limite" :min="0" />
        <DateTimeField v-model="garantiaForm.vigenciaInicio" label="Vigência início" />
        <DateTimeField v-model="garantiaForm.vigenciaFim" label="Vigência fim" />
      </div>
      <TextField v-model="garantiaForm.descricao" label="Descrição" class="mt-2" />
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="garantiaVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoGarantia" @click="salvarGarantia">
          <span v-if="salvandoGarantia" class="spinner"></span><span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Reajuste -->
    <AppDialog v-model="reajusteVisivel" title="Aplicar reajuste" width="520px">
      <p class="dialog-hint">Informe o <strong>novo valor ratificado pelo contador</strong>. O índice/percentual são apenas registro.</p>
      <div class="form-grid">
        <MoneyInput v-model="reajusteForm.novoValor" label="Novo valor" required :min="0" />
        <TextField v-model="reajusteForm.indice" label="Índice (ex.: IGP-M)" />
        <DateTimeField v-model="reajusteForm.dataBase" label="Data base" />
        <TextField v-model="reajusteForm.percentualAplicado" type="number" label="% aplicado" suffix="%" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="reajusteVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="aplicandoReajuste" @click="aplicarReajuste">
          <span v-if="aplicandoReajuste" class="spinner"></span><span v-else>Aplicar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Renovar -->
    <AppDialog v-model="renovarVisivel" title="Renovar contrato" width="480px">
      <div class="form-grid">
        <DateTimeField v-model="renovarForm.novoPeriodoFinal" label="Novo fim do período" required />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="renovarVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="renovando" @click="confirmarRenovar">
          <span v-if="renovando" class="spinner"></span><span v-else>Renovar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Rescindir -->
    <AppDialog v-model="rescindirVisivel" title="Rescindir locação" width="520px">
      <p class="dialog-hint">O cálculo da multa é <strong>valida-contador</strong>; informe o valor já ratificado.</p>
      <TextField v-model="rescindirForm.motivo" label="Motivo" required />
      <div class="form-grid mt-2">
        <DateTimeField v-model="rescindirForm.dataRescisao" label="Data da rescisão" required />
        <TextField v-model="rescindirForm.avisoPrevioDias" type="number" label="Aviso prévio (dias)" />
        <MoneyInput v-model="rescindirForm.multaProporcional" label="Multa proporcional" :min="0" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="rescindirVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary btn-danger-action" :disabled="rescindindo" @click="confirmarRescindir">
          <span v-if="rescindindo" class="spinner"></span><span v-else>Rescindir</span>
        </button>
      </template>
    </AppDialog>

    <!-- Cancelar -->
    <AppDialog v-model="cancelarVisivel" title="Cancelar contrato" width="440px">
      <p class="dialog-hint">
        Confirma o cancelamento da locação? Esta transição segue a máquina de estado do backend
        (só é permitida em contratos ainda não vigentes/encerrados).
      </p>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="cancelarVisivel = false">Voltar</button>
        <button type="button" class="btn btn-primary btn-danger-action" :disabled="acaoCiclo === 'cancelar'" @click="confirmarCancelamento">
          <span v-if="acaoCiclo === 'cancelar'" class="spinner"></span><span v-else>Cancelar contrato</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.resumo-head { display: grid; grid-template-columns: repeat(auto-fill, minmax(150px, 1fr)); gap: 14px; padding: 18px 22px; margin-bottom: 16px; }
.rh-label { display: block; font-size: 12px; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.03em; }
.rh-valor { display: block; margin-top: 4px; font-size: 15px; font-weight: 700; color: var(--text-primary); }
.tabs { display: flex; gap: 4px; margin-bottom: 16px; border-bottom: 1px solid var(--border-color); flex-wrap: wrap; }
.tab { padding: 10px 18px; border: none; background: transparent; font-size: 13px; font-weight: 600; color: var(--text-secondary); cursor: pointer; border-bottom: 2px solid transparent; }
.tab.ativo { color: var(--primary); border-bottom-color: var(--primary); }
.painel { padding: 20px 22px; }
.painel-topo { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
.painel-titulo { font-size: 15px; font-weight: 700; color: var(--text-primary); margin: 0; }
.painel-info { font-size: 13px; color: var(--text-secondary); line-height: 1.5; margin: 0 0 16px; }
.resumo-list { list-style: none; margin: 0; padding: 0; font-size: 13px; color: var(--text-primary); }
.resumo-list li { padding: 6px 0; border-bottom: 1px solid var(--border-color); }
.vazio { padding: 40px 0; text-align: center; color: var(--text-secondary); font-size: 13px; }
.tabela { width: 100%; border-collapse: collapse; font-size: 13px; }
.tabela th, .tabela td { padding: 10px 12px; text-align: left; border-bottom: 1px solid var(--border-color); }
.tabela th { font-size: 12px; text-transform: uppercase; letter-spacing: 0.03em; color: var(--text-secondary); }
.tabela .num { text-align: right; white-space: nowrap; }
.tabela .center { text-align: center; }
.acoes-col { white-space: nowrap; text-align: right; }
.badge { display: inline-block; padding: 3px 10px; border-radius: 12px; font-size: 12px; font-weight: 600; background: var(--border-color); color: var(--text-primary); }
.badge.stc-1 { background: color-mix(in srgb, var(--warning) 20%, transparent); color: var(--warning); }
.badge.stc-2 { background: color-mix(in srgb, var(--success) 20%, transparent); color: var(--success); }
.badge.stc-3 { background: color-mix(in srgb, var(--danger) 20%, transparent); color: var(--danger); }
.badge.stg-0 { background: color-mix(in srgb, var(--success) 20%, transparent); color: var(--success); }
.badge.stg-2 { background: color-mix(in srgb, var(--text-secondary) 20%, transparent); color: var(--text-secondary); }
.text-muted { color: var(--text-secondary); }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 14px; }
.dialog-hint { font-size: 13px; color: var(--text-secondary); line-height: 1.5; margin: 0 0 16px; }
.ciclo-acoes { display: flex; flex-wrap: wrap; gap: 12px; }
.mt-2 { margin-top: 12px; }
</style>
