<script setup lang="ts">
/**
 * Ordem de Manufatura (MES) — criação (novo) e detalhe/workflow (id existente).
 * POST /producao/mes/ordens + workflow (EStatusOrdemMes) + POST /{id}/finalizar. Sem PUT.
 *
 * O status MES (EStatusOrdemMes) não coincide com o workflow padrão, então as ações são
 * apresentadas fixas (o backend valida a transição). Campos com palpite: empresaId /
 * estruturaId / produtoAcabadoId / variacaoProdutoAcabadoId / localEstoqueId são uuid sem
 * endpoint de listagem próprio → TextField (uuid). Coleção `itens` e sub-endpoints
 * (itens/producao, servicos) ficam como lacuna — ver relatório.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import { rotuloStatusMes, classeBadgeStatus, formatarData, formatarMoeda } from '~/components/producao-shared/producao'

definePageMeta({ layout: 'default' })

interface MesForm {
  empresaId: string
  referencia: string | null
  inicio: string | null
  previsaoEntrega: string | null
  estruturaId: string | null
  produtoAcabadoId: string | null
  variacaoProdutoAcabadoId: string | null
  custoTotalPrevisto: number | null
  percentualVenda: number | null
  percentualEstoque: number | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const acaoEmAndamento = ref(false)
const registro = ref<Record<string, unknown> | null>(null)
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const rejeitarVisivel = ref(false)
const motivoRejeicao = ref('')

const finalizarVisivel = ref(false)
const finalizar = reactive({
  dataTransacao: null as string | null,
  localEstoqueId: '' as string,
  valorTotalFinal: null as number | null,
  desperdicioUnidades: null as number | null,
  lote: null as string | null,
  validade: null as string | null
})

const form = reactive<MesForm>({
  empresaId: '',
  referencia: null,
  inicio: null,
  previsaoEntrega: null,
  estruturaId: null,
  produtoAcabadoId: null,
  variacaoProdutoAcabadoId: null,
  custoTotalPrevisto: 0,
  percentualVenda: null,
  percentualEstoque: null
})
const erros = reactive<Record<string, string>>({})

const acoesFixas = [
  { chave: 'submeter', rotulo: 'Submeter', danger: false },
  { chave: 'aprovar', rotulo: 'Aprovar', danger: false },
  { chave: 'encerrar', rotulo: 'Encerrar', danger: false },
  { chave: 'reativar', rotulo: 'Reativar', danger: false },
  { chave: 'inativar', rotulo: 'Inativar', danger: true }
]

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.empresaId) erros.empresaId = 'Empresa é obrigatória.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/producao/mes/ordens/${idParam}`)
    registro.value = extrairDados<Record<string, unknown>>(resposta) ?? null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function salvar() {
  if (!validar()) { toast.error('Formulário possui erros de validação.'); return }
  salvando.value = true
  try {
    await useApi('/producao/mes/ordens', { method: 'POST', body: form })
    toast.success('Ordem criada com sucesso!')
    router.push('/erp/producao/ordens-mes')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function executarAcao(chave: string) {
  const ok = await confirmRef.value?.open(`${chave}?`, `Confirmar a ação para esta ordem?`, { textoConfirmar: 'Confirmar', danger: chave === 'inativar' })
  if (!ok) return
  acaoEmAndamento.value = true
  try {
    await useApi(`/producao/mes/ordens/${idParam}/${chave}`, { method: 'POST' })
    toast.success('Ação executada com sucesso.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmAndamento.value = false
  }
}

async function rejeitar() {
  acaoEmAndamento.value = true
  try {
    await useApi(`/producao/mes/ordens/${idParam}/rejeitar`, { method: 'POST', body: motivoRejeicao.value })
    toast.success('Ordem rejeitada.')
    rejeitarVisivel.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmAndamento.value = false
  }
}

async function confirmarFinalizacao() {
  if (!finalizar.dataTransacao || !finalizar.localEstoqueId) {
    toast.error('Informe a data da transação e o local de estoque.')
    return
  }
  acaoEmAndamento.value = true
  try {
    await useApi(`/producao/mes/ordens/${idParam}/finalizar`, {
      method: 'POST',
      body: {
        ordemId: idParam,
        dataTransacao: finalizar.dataTransacao,
        localEstoqueId: finalizar.localEstoqueId,
        valorTotalFinal: finalizar.valorTotalFinal ?? 0,
        desperdicioUnidades: finalizar.desperdicioUnidades ?? 0,
        lote: finalizar.lote,
        validade: finalizar.validade
      }
    })
    toast.success('Ordem finalizada.')
    finalizarVisivel.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmAndamento.value = false
  }
}

function cancelar() { router.push('/erp/producao/ordens-mes') }
const statusAtual = computed(() => registro.value?.status as number | string | undefined)
onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Ordem de Manufatura (MES)' : 'Nova ordem'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <template v-if="isEdit">
          <button v-for="a in acoesFixas" :key="a.chave" type="button" class="btn btn-sm" :class="a.danger ? 'btn-danger' : 'btn-primary'" :disabled="acaoEmAndamento" @click="executarAcao(a.chave)">{{ a.rotulo }}</button>
          <button type="button" class="btn btn-sm btn-danger" :disabled="acaoEmAndamento" @click="rejeitarVisivel = true">Rejeitar</button>
          <button type="button" class="btn btn-sm btn-primary" :disabled="acaoEmAndamento" @click="finalizarVisivel = true">Finalizar</button>
        </template>
        <button v-else type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div v-if="isEdit">
        <div v-if="registro" class="detail-grid">
          <div class="detail-item"><span class="detail-label">Referência</span><span>{{ registro.referencia || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Status</span><span class="badge" :class="classeBadgeStatus(rotuloStatusMes(statusAtual))">{{ rotuloStatusMes(statusAtual) }}</span></div>
          <div class="detail-item"><span class="detail-label">Custo Previsto</span><span>{{ formatarMoeda(registro.custoTotalPrevisto as number) }}</span></div>
          <div class="detail-item"><span class="detail-label">Custo Realizado</span><span>{{ formatarMoeda(registro.custoTotalRealizado as number) }}</span></div>
          <div class="detail-item"><span class="detail-label">Valor Final</span><span>{{ formatarMoeda(registro.valorTotalFinal as number) }}</span></div>
          <div class="detail-item"><span class="detail-label">Início</span><span>{{ formatarData(registro.inicio as string) }}</span></div>
          <div class="detail-item"><span class="detail-label">Previsão de Entrega</span><span>{{ formatarData(registro.previsaoEntrega as string) }}</span></div>
          <div class="detail-item"><span class="detail-label">Término</span><span>{{ formatarData(registro.termino as string) }}</span></div>
          <div class="detail-item"><span class="detail-label">Lote</span><span>{{ registro.lote || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Finalizada</span><span>{{ registro.finalizada ? 'Sim' : 'Não' }}</span></div>
          <div class="detail-item"><span class="detail-label">Empresa (ID)</span><span>{{ registro.empresaId || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Criado em</span><span>{{ formatarData(registro.criadoEm as string, true) }}</span></div>
        </div>
        <p v-else-if="!carregando" class="empty-detail">Registro não encontrado.</p>
      </div>

      <form v-else class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.empresaId" label="Empresa (ID)" required :error="erros.empresaId" hint="UUID da empresa" />
          <TextField v-model="form.referencia" label="Referência" maxlength="60" />
          <DateTimeField v-model="form.inicio" label="Início" mode="datetime" />
          <DateTimeField v-model="form.previsaoEntrega" label="Previsão de entrega" mode="datetime" />
          <TextField v-model="form.estruturaId" label="Estrutura (ID)" hint="UUID (opcional)" />
          <TextField v-model="form.produtoAcabadoId" label="Produto acabado (ID)" hint="UUID (opcional)" />
          <TextField v-model="form.variacaoProdutoAcabadoId" label="Variação do produto (ID)" hint="UUID (opcional)" />
          <MoneyInput v-model="form.custoTotalPrevisto" label="Custo total previsto" />
          <PercentInput v-model="form.percentualVenda" label="% Venda" />
          <PercentInput v-model="form.percentualEstoque" label="% Estoque" />
        </div>
        <p class="form-note">Os itens da ordem e os serviços são adicionados após a criação (sub-endpoints não expostos nesta tela).</p>
      </form>
    </div>

    <ConfirmDialog ref="confirmRef" />

    <AppDialog v-model="rejeitarVisivel" title="Rejeitar ordem" width="440px" persistent>
      <TextField v-model="motivoRejeicao" label="Motivo da rejeição" placeholder="Descreva o motivo..." />
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="rejeitarVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-danger" :disabled="acaoEmAndamento || !motivoRejeicao" @click="rejeitar">Rejeitar</button>
      </template>
    </AppDialog>

    <AppDialog v-model="finalizarVisivel" title="Finalizar ordem" width="560px">
      <div class="form-grid">
        <DateTimeField v-model="finalizar.dataTransacao" label="Data da transação" mode="datetime" required />
        <TextField v-model="finalizar.localEstoqueId" label="Local de estoque (ID)" required hint="UUID do local de estoque" />
        <MoneyInput v-model="finalizar.valorTotalFinal" label="Valor total final" />
        <QuantityInput v-model="finalizar.desperdicioUnidades" label="Desperdício (unidades)" />
        <TextField v-model="finalizar.lote" label="Lote" />
        <DateTimeField v-model="finalizar.validade" label="Validade" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="finalizarVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoEmAndamento" @click="confirmarFinalizacao">Finalizar</button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.detail-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 16px; }
.detail-item { display: flex; flex-direction: column; gap: 4px; }
.detail-label { font-size: 12px; color: var(--text-secondary); font-weight: 600; }
.form-note { margin-top: 16px; font-size: 12.5px; color: var(--text-secondary); }
.empty-detail { color: var(--text-secondary); }
</style>
