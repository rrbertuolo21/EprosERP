<script setup lang="ts">
/**
 * Alocações a Centro de Custo — Contabilidade Gerencial / Alocações.
 *
 * A API não expõe uma lista geral de alocações: elas são consultadas POR TÍTULO
 * (contas a pagar/receber). Esta tela é uma consulta por título:
 *   GET    /contabilidade-gerencial/titulos/{tituloId}/alocacoes
 *   POST   /contabilidade-gerencial/alocacoes                       (tituloId, tipoTitulo, centroCustoId, percentual, valorRateado?)
 *   DELETE /contabilidade-gerencial/alocacoes/{id}
 *   POST   /contabilidade-gerencial/alocacoes/{alocacaoId}/dimensoes (vincula dimensão analítica)
 * O `tituloId` é um GUID de um título financeiro (não há endpoint de browse para escolhê-lo aqui).
 */
import { computed, reactive, ref, onMounted } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import type { SelectOption } from '~/composables/useEnum'
import { tiposTituloAlocacao } from '~/components/contabilidade-contas/enums'

definePageMeta({ layout: 'default' })

interface Alocacao {
  id: string
  tituloId: string
  tipoTitulo: number
  centroCustoId: string
  percentual: number
  valorRateado?: number | null
}
interface CentroOpcao { id: string; codigo?: string | null; descricao?: string | null }
interface DimensaoOpcao { id: string; tipo?: string | null; valor?: string | null }

const toast = useToast()
const { formatarMoeda } = useHelper()

const tituloId = ref('')
const tipoTituloConsulta = ref<number>(0)
const carregando = ref(false)
const consultado = ref(false)
const alocacoes = ref<Alocacao[]>([])

const centros = ref<CentroOpcao[]>([])
const dimensoes = ref<DimensaoOpcao[]>([])

const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const opcoesCentro = computed<SelectOption[]>(() =>
  centros.value.map((c) => ({ label: `${c.codigo ?? ''} — ${c.descricao ?? ''}`.trim(), value: c.id }))
)
const opcoesDimensao = computed<SelectOption[]>(() =>
  dimensoes.value.map((d) => ({ label: `${d.tipo ?? ''}: ${d.valor ?? ''}`.trim(), value: d.id }))
)

function centroLabel(id: string): string {
  const c = centros.value.find((x) => x.id === id)
  return c ? `${c.codigo ?? ''} — ${c.descricao ?? ''}`.trim() : id
}

async function carregarAuxiliares() {
  try {
    const [rc, rd] = await Promise.all([
      useApi('/contabilidade-gerencial/centros-custo', { query: { tamanhoPagina: 100 } }),
      useApi('/contabilidade-gerencial/dimensoes')
    ])
    const dc = extrairDados<{ itens?: CentroOpcao[] } | CentroOpcao[]>(rc)
    centros.value = Array.isArray(dc) ? dc : dc?.itens ?? []
    dimensoes.value = extrairDados<DimensaoOpcao[]>(rd) ?? []
  } catch (e) {
    console.error('[contabilidade/alocacoes] auxiliares', e)
  }
}

async function consultar() {
  if (!tituloId.value.trim()) {
    toast.error('Informe o ID do título.')
    return
  }
  carregando.value = true
  try {
    const resposta = await useApi(`/contabilidade-gerencial/titulos/{tituloId}/alocacoes`, { params: { tituloId: tituloId.value.trim() } })
    alocacoes.value = extrairDados<Alocacao[]>(resposta) ?? []
    consultado.value = true
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

/* ----- Nova alocação ----- */
const alocDialogVisivel = ref(false)
const salvandoAloc = ref(false)
const novaAloc = reactive<{ centroCustoId: string | null; percentual: number; valorRateado: number }>({
  centroCustoId: null, percentual: 0, valorRateado: 0
})

function abrirAlocDialog() {
  if (!tituloId.value.trim()) {
    toast.error('Consulte um título antes de alocar.')
    return
  }
  Object.assign(novaAloc, { centroCustoId: null, percentual: 0, valorRateado: 0 })
  alocDialogVisivel.value = true
}

async function salvarAloc() {
  if (!novaAloc.centroCustoId) {
    toast.error('Selecione o centro de custo.')
    return
  }
  salvandoAloc.value = true
  try {
    await useApi('/contabilidade-gerencial/alocacoes', {
      method: 'POST',
      body: {
        tituloId: tituloId.value.trim(),
        tipoTitulo: tipoTituloConsulta.value,
        centroCustoId: novaAloc.centroCustoId,
        percentual: novaAloc.percentual || 0,
        valorRateado: novaAloc.valorRateado || null
      }
    })
    toast.success('Alocação criada.')
    alocDialogVisivel.value = false
    await consultar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoAloc.value = false
  }
}

async function removerAloc(item: Alocacao) {
  const ok = await confirmRef.value!.open('Remover alocação', 'Remover esta alocação a centro de custo?', { danger: true, textoConfirmar: 'Remover' })
  if (!ok) return
  try {
    await useApi(`/contabilidade-gerencial/alocacoes/{id}`, { method: 'DELETE', params: { id: item.id } })
    toast.success('Alocação removida.')
    await consultar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

/* ----- Vincular dimensão ----- */
const dimDialogVisivel = ref(false)
const salvandoDim = ref(false)
const alocSelecionada = ref<Alocacao | null>(null)
const dimensaoSelecionada = ref<string | null>(null)

function abrirDimDialog(item: Alocacao) {
  alocSelecionada.value = item
  dimensaoSelecionada.value = null
  dimDialogVisivel.value = true
}

async function salvarDim() {
  if (!alocSelecionada.value || !dimensaoSelecionada.value) {
    toast.error('Selecione a dimensão analítica.')
    return
  }
  salvandoDim.value = true
  try {
    await useApi(`/contabilidade-gerencial/alocacoes/{alocacaoId}/dimensoes`, {
      method: 'POST',
      params: { alocacaoId: alocSelecionada.value.id },
      body: { alocacaoCentroCustoId: alocSelecionada.value.id, dimensaoAnaliticaId: dimensaoSelecionada.value }
    })
    toast.success('Dimensão vinculada à alocação.')
    dimDialogVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoDim.value = false
  }
}

onMounted(carregarAuxiliares)
</script>

<template>
  <div>
    <PageToolbar
      title="Alocações a Centro de Custo"
      subtitle="Consulta e rateio de um título (contas a pagar/receber) por centro de custo"
      :loading="carregando"
    />

    <div class="glass-panel form-panel">
      <div class="consulta-grid">
        <TextField v-model="tituloId" label="ID do Título" placeholder="GUID do título financeiro" />
        <SelectField v-model="tipoTituloConsulta" label="Tipo do Título" :options="tiposTituloAlocacao" :clearable="false" />
        <button type="button" class="btn btn-primary consulta-btn" :disabled="carregando" @click="consultar">
          <span v-if="carregando" class="spinner"></span>
          <span v-else>Consultar</span>
        </button>
      </div>
    </div>

    <div v-if="consultado" class="glass-panel form-panel">
      <div class="tab-actions">
        <button type="button" class="btn btn-secondary btn-sm" @click="abrirAlocDialog">+ Nova alocação</button>
      </div>
      <table class="admin-table">
        <thead>
          <tr><th>Centro de Custo</th><th class="td-right">Percentual</th><th class="td-right">Valor Rateado</th><th class="td-actions">Ações</th></tr>
        </thead>
        <tbody>
          <tr v-if="!alocacoes.length"><td colspan="4"><div class="table-empty">Nenhuma alocação para este título.</div></td></tr>
          <tr v-for="a in alocacoes" :key="a.id">
            <td>{{ centroLabel(a.centroCustoId) }}</td>
            <td class="td-right">{{ a.percentual }}%</td>
            <td class="td-right">{{ formatarMoeda(a.valorRateado ?? 0) }}</td>
            <td class="td-actions">
              <button type="button" class="btn btn-ghost btn-sm" @click="abrirDimDialog(a)">Dimensão</button>
              <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="removerAloc(a)">Remover</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Dialog: nova alocação -->
    <AppDialog v-model="alocDialogVisivel" title="Nova alocação" width="480px">
      <div class="dialog-form">
        <SelectField v-model="novaAloc.centroCustoId" label="Centro de Custo" required :options="opcoesCentro" />
        <PercentInput v-model="novaAloc.percentual" label="Percentual" />
        <MoneyInput v-model="novaAloc.valorRateado" label="Valor Rateado" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoAloc" @click="alocDialogVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoAloc" @click="salvarAloc">
          <span v-if="salvandoAloc" class="spinner"></span>
          <span v-else>Criar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Dialog: vincular dimensão -->
    <AppDialog v-model="dimDialogVisivel" title="Vincular dimensão analítica" width="480px">
      <div class="dialog-form">
        <SelectField v-model="dimensaoSelecionada" label="Dimensão Analítica" required :options="opcoesDimensao" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoDim" @click="dimDialogVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoDim" @click="salvarDim">
          <span v-if="salvandoDim" class="spinner"></span>
          <span v-else>Vincular</span>
        </button>
      </template>
    </AppDialog>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-bottom: 16px; }
.consulta-grid { display: grid; grid-template-columns: 2fr 1fr auto; gap: 16px; align-items: end; }
.consulta-btn { height: 40px; }
.tab-actions { display: flex; justify-content: flex-end; margin-bottom: 10px; }
.dialog-form { display: flex; flex-direction: column; gap: 14px; }
@media (max-width: 800px) {
  .consulta-grid { grid-template-columns: 1fr; }
}
</style>
