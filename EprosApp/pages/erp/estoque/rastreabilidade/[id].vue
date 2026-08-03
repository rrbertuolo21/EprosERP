<script setup lang="ts">
/**
 * Detalhe do Lote rastreável (erp/estoque/rastreabilidade/[id]).
 * `GET /estoque-rastreabilidade/lotes/{id}` (dados do lote) + `GET .../lotes/{id}/genealogia`
 * (seriais, histórico de bloqueios e recalls). Ações:
 *   POST /lotes/{id}/bloquear     (BloquearLoteCommand: tipo, motivo, quantidade?)
 *   POST /lotes/{id}/desbloquear  (DesbloquearLoteCommand: motivoDesbloqueio?)
 *   POST /recalls                 (AbrirRecallLoteCommand: loteId, motivo, codigoRecall?)
 *   POST /recalls/{recallId}/concluir
 */
import { computed, onMounted, reactive, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import { useEstoqueEnums, classeBadge } from '~/composables/useEstoqueEnums'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { formatarData, formatarDataHora, formatarNumero } = useHelper()
const { statusLote, statusSerial, origemLote, tipoBloqueioLote, statusRecall } = useEstoqueEnums()

const idParam = computed(() => route.params.id as string)

interface Lote {
  id: string
  empresaId: string
  produtoId: string
  localId: string | null
  codigoLote: string
  dataFabricacao: string | null
  dataValidade: string | null
  dataRecebimento: string | null
  origem: number
  status: number
  quantidadeRecebida: number
  quantidadeDisponivel: number
  quantidadeBloqueada: number
  quantidadeConsumida: number
}
interface SerialGen { id: string; numero: string; status: number }
interface BloqueioGen { id: string; tipoBloqueio: number; motivo: string; dataBloqueio: string; dataDesbloqueio: string | null }
interface RecallGen { id: string; codigoRecall: string | null; motivo: string; status: number; dataAbertura: string; dataEncerramento: string | null }

const carregando = ref(false)
const processando = ref(false)
const lote = ref<Lote | null>(null)
const seriais = ref<SerialGen[]>([])
const bloqueios = ref<BloqueioGen[]>([])
const recalls = ref<RecallGen[]>([])

async function carregar() {
  carregando.value = true
  try {
    const [respLote, respGen] = await Promise.all([
      useApi('/estoque-rastreabilidade/lotes/{id}', { params: { id: idParam.value } }),
      useApi('/estoque-rastreabilidade/lotes/{id}/genealogia', { params: { id: idParam.value } })
    ])
    lote.value = extrairDados<Lote>(respLote) ?? null
    const gen = extrairDados<{ seriais?: SerialGen[]; bloqueios?: BloqueioGen[]; recalls?: RecallGen[] }>(respGen)
    seriais.value = gen?.seriais ?? []
    bloqueios.value = gen?.bloqueios ?? []
    recalls.value = gen?.recalls ?? []
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

const recallAberto = computed(() => recalls.value.find((r) => r.status === 0 || r.status === 1) ?? null)

// --------- Bloquear ---------
const bloquearDialog = ref(false)
const formBloqueio = reactive({ tipoBloqueio: 0 as number, motivo: '', quantidade: null as number | null })
async function confirmarBloqueio() {
  if (!lote.value || !formBloqueio.motivo.trim()) {
    toast.error('Informe o motivo do bloqueio.')
    return
  }
  processando.value = true
  try {
    await useApi('/estoque-rastreabilidade/lotes/{id}/bloquear', {
      method: 'POST',
      params: { id: lote.value.id },
      body: { loteId: lote.value.id, tipoBloqueio: formBloqueio.tipoBloqueio, motivo: formBloqueio.motivo.trim(), quantidade: formBloqueio.quantidade }
    })
    toast.success('Lote bloqueado.')
    bloquearDialog.value = false
    formBloqueio.motivo = ''
    formBloqueio.quantidade = null
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}

// --------- Desbloquear ---------
const desbloquearDialog = ref(false)
const motivoDesbloqueio = ref('')
async function confirmarDesbloqueio() {
  if (!lote.value) return
  processando.value = true
  try {
    await useApi('/estoque-rastreabilidade/lotes/{id}/desbloquear', {
      method: 'POST',
      params: { id: lote.value.id },
      body: { loteId: lote.value.id, motivoDesbloqueio: motivoDesbloqueio.value.trim() || null }
    })
    toast.success('Lote desbloqueado.')
    desbloquearDialog.value = false
    motivoDesbloqueio.value = ''
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}

// --------- Recall ---------
const recallDialog = ref(false)
const formRecall = reactive({ motivo: '', codigoRecall: '' })
async function abrirRecall() {
  if (!lote.value || !formRecall.motivo.trim()) {
    toast.error('Informe o motivo do recall.')
    return
  }
  processando.value = true
  try {
    await useApi('/estoque-rastreabilidade/recalls', {
      method: 'POST',
      body: { loteId: lote.value.id, motivo: formRecall.motivo.trim(), codigoRecall: formRecall.codigoRecall.trim() || null }
    })
    toast.success('Recall aberto — lote bloqueado para consumo.')
    recallDialog.value = false
    formRecall.motivo = ''
    formRecall.codigoRecall = ''
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}
async function concluirRecall(recallId: string) {
  processando.value = true
  try {
    await useApi('/estoque-rastreabilidade/recalls/{recallId}/concluir', { method: 'POST', params: { recallId } })
    toast.success('Recall concluído.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}

function voltar() { router.push('/erp/estoque/rastreabilidade') }

onMounted(() => void carregar())
</script>

<template>
  <div>
    <PageToolbar title="Lote rastreável" :subtitle="lote?.codigoLote" :loading="carregando || processando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
        <button type="button" class="btn btn-secondary" :disabled="processando" @click="bloquearDialog = true">Bloquear</button>
        <button type="button" class="btn btn-secondary" :disabled="processando" @click="desbloquearDialog = true">Desbloquear</button>
        <button v-if="!recallAberto" type="button" class="btn btn-danger" :disabled="processando" @click="recallDialog = true">Abrir recall</button>
      </template>
    </PageToolbar>

    <div v-if="lote" class="glass-panel form-panel">
      <div class="cabecalho-info">
        <div><span class="dado-label">Status</span><span class="badge" :class="classeBadge(statusLote.cor(lote.status))">{{ statusLote.label(lote.status) }}</span></div>
        <div><span class="dado-label">Origem</span><span class="dado-valor">{{ origemLote.label(lote.origem) }}</span></div>
        <div><span class="dado-label">Produto (ID)</span><span class="dado-valor">{{ lote.produtoId }}</span></div>
        <div><span class="dado-label">Fabricação</span><span class="dado-valor">{{ formatarData(lote.dataFabricacao) || '-' }}</span></div>
        <div><span class="dado-label">Validade</span><span class="dado-valor">{{ formatarData(lote.dataValidade) || '-' }}</span></div>
        <div><span class="dado-label">Recebido</span><span class="dado-valor">{{ formatarData(lote.dataRecebimento) || '-' }}</span></div>
        <div><span class="dado-label">Qtd. recebida</span><span class="dado-valor">{{ formatarNumero(lote.quantidadeRecebida, 0, 4) }}</span></div>
        <div><span class="dado-label">Disponível</span><span class="dado-valor">{{ formatarNumero(lote.quantidadeDisponivel, 0, 4) }}</span></div>
        <div><span class="dado-label">Bloqueada</span><span class="dado-valor">{{ formatarNumero(lote.quantidadeBloqueada, 0, 4) }}</span></div>
        <div><span class="dado-label">Consumida</span><span class="dado-valor">{{ formatarNumero(lote.quantidadeConsumida, 0, 4) }}</span></div>
      </div>
    </div>

    <!-- Genealogia -->
    <div class="glass-panel secao">
      <h3 class="secao-titulo">Números de série ({{ seriais.length }})</h3>
      <div v-if="seriais.length === 0" class="vazio">Sem seriais vinculados.</div>
      <table v-else class="admin-table">
        <thead><tr><th>Número</th><th>Status</th></tr></thead>
        <tbody>
          <tr v-for="s in seriais" :key="s.id">
            <td>{{ s.numero }}</td>
            <td><span class="badge" :class="classeBadge(statusSerial.cor(s.status))">{{ statusSerial.label(s.status) }}</span></td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="glass-panel secao">
      <h3 class="secao-titulo">Histórico de bloqueios ({{ bloqueios.length }})</h3>
      <div v-if="bloqueios.length === 0" class="vazio">Sem bloqueios registrados.</div>
      <table v-else class="admin-table">
        <thead><tr><th>Tipo</th><th>Motivo</th><th>Bloqueado em</th><th>Desbloqueado em</th></tr></thead>
        <tbody>
          <tr v-for="b in bloqueios" :key="b.id">
            <td>{{ tipoBloqueioLote.label(b.tipoBloqueio) }}</td>
            <td>{{ b.motivo }}</td>
            <td>{{ formatarDataHora(b.dataBloqueio) }}</td>
            <td>{{ b.dataDesbloqueio ? formatarDataHora(b.dataDesbloqueio) : '-' }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="glass-panel secao">
      <h3 class="secao-titulo">Recalls ({{ recalls.length }})</h3>
      <div v-if="recalls.length === 0" class="vazio">Sem recalls.</div>
      <table v-else class="admin-table">
        <thead><tr><th>Código</th><th>Motivo</th><th>Status</th><th>Abertura</th><th>Encerramento</th><th></th></tr></thead>
        <tbody>
          <tr v-for="r in recalls" :key="r.id">
            <td>{{ r.codigoRecall || '-' }}</td>
            <td>{{ r.motivo }}</td>
            <td><span class="badge" :class="classeBadge(statusRecall.cor(r.status))">{{ statusRecall.label(r.status) }}</span></td>
            <td>{{ formatarDataHora(r.dataAbertura) }}</td>
            <td>{{ r.dataEncerramento ? formatarDataHora(r.dataEncerramento) : '-' }}</td>
            <td>
              <button v-if="r.status === 0 || r.status === 1" type="button" class="btn btn-ghost btn-sm" :disabled="processando" @click="concluirRecall(r.id)">Concluir</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Dialogs -->
    <AppDialog v-model="bloquearDialog" title="Bloquear lote" width="480px">
      <div class="form-grid">
        <SelectField v-model="formBloqueio.tipoBloqueio" label="Tipo de bloqueio" :options="tipoBloqueioLote.opcoes" :clearable="false" />
        <QuantityInput v-model="formBloqueio.quantidade" label="Quantidade (parcial)" :decimais="4" hint="Vazio = bloqueio total" />
      </div>
      <TextField v-model="formBloqueio.motivo" label="Motivo" required />
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="processando" @click="bloquearDialog = false">Cancelar</button>
        <button type="button" class="btn btn-danger" :disabled="processando" @click="confirmarBloqueio">
          <span v-if="processando" class="spinner"></span><span v-else>Bloquear</span>
        </button>
      </template>
    </AppDialog>

    <AppDialog v-model="desbloquearDialog" title="Desbloquear lote" width="440px">
      <TextField v-model="motivoDesbloqueio" label="Motivo do desbloqueio" hint="Opcional" />
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="processando" @click="desbloquearDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="processando" @click="confirmarDesbloqueio">
          <span v-if="processando" class="spinner"></span><span v-else>Desbloquear</span>
        </button>
      </template>
    </AppDialog>

    <AppDialog v-model="recallDialog" title="Abrir recall" width="480px" persistent>
      <div class="form-grid">
        <TextField v-model="formRecall.codigoRecall" label="Código do recall" hint="Opcional" />
      </div>
      <TextField v-model="formRecall.motivo" label="Motivo" required />
      <p class="aviso">Abrir um recall bloqueia automaticamente o lote para consumo.</p>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="processando" @click="recallDialog = false">Cancelar</button>
        <button type="button" class="btn btn-danger" :disabled="processando" @click="abrirRecall">
          <span v-if="processando" class="spinner"></span><span v-else>Abrir recall</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-bottom: 12px; }
.secao { padding: 16px 20px; margin-bottom: 12px; }
.secao-titulo { font-size: 14px; margin: 0 0 12px; }
.cabecalho-info { display: grid; grid-template-columns: repeat(auto-fill, minmax(150px, 1fr)); gap: 16px; }
.dado-label { display: block; font-size: 12px; color: var(--text-muted); margin-bottom: 4px; }
.dado-valor { font-size: 14px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; margin-bottom: 12px; }
.vazio { font-size: 13px; color: var(--text-muted); padding: 8px 0; }
.aviso { font-size: 12px; color: var(--text-muted); margin-top: 10px; }
.badge-success { background: rgba(16, 185, 129, 0.1); color: var(--success); border: 1px solid rgba(16, 185, 129, 0.25); }
.badge-danger { background: rgba(239, 68, 68, 0.1); color: var(--danger); border: 1px solid rgba(239, 68, 68, 0.25); }
.badge-warning { background: rgba(245, 158, 11, 0.12); color: var(--warning); border: 1px solid rgba(245, 158, 11, 0.25); }
.badge-info { background: rgba(59, 130, 246, 0.1); color: #3b82f6; border: 1px solid rgba(59, 130, 246, 0.25); }
.badge-muted { background: rgba(120, 120, 130, 0.1); color: var(--text-muted); border: 1px solid var(--border-color); }
</style>
