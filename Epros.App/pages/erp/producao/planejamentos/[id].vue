<script setup lang="ts">
/**
 * Planejamento de Produção — criação (novo) e detalhe/workflow (id existente).
 * POST /producao/planejamentos (codigo, responsavelId) + workflow + POST /{id}/snapshots.
 * Sem PUT.
 * Campos com palpite: responsavelId / ordemProducaoId são uuid sem endpoint de listagem
 * próprio → TextField (uuid).
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import WorkflowActions from '~/components/producao-shared/WorkflowActions.vue'
import { rotuloStatusWorkflow, classeBadgeStatus, formatarData } from '~/components/producao-shared/producao'

definePageMeta({ layout: 'default' })

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const acaoEmAndamento = ref(false)
const registro = ref<Record<string, unknown> | null>(null)

const form = reactive<{ codigo: string | null; responsavelId: string }>({ codigo: null, responsavelId: '' })
const erros = reactive<Record<string, string>>({})

const snapshotVisivel = ref(false)
const snapshot = reactive({
  ordemProducaoId: '' as string,
  inicio: null as string | null,
  previsaoEntrega: null as string | null,
  termino: null as string | null,
  porcentoVenda: null as number | null,
  porcentoEstoque: null as number | null,
  custoTotalPrevisto: null as number | null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.responsavelId) erros.responsavelId = 'Responsável é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/producao/planejamentos/${idParam}`)
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
    await useApi('/producao/planejamentos', { method: 'POST', body: form })
    toast.success('Planejamento criado com sucesso!')
    router.push('/erp/producao/planejamentos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function executarAcao(chave: string, motivo?: string) {
  acaoEmAndamento.value = true
  try {
    await useApi(`/producao/planejamentos/${idParam}/${chave}`, {
      method: 'POST',
      body: chave === 'rejeitar' ? (motivo ?? '') : undefined
    })
    toast.success('Ação executada com sucesso.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmAndamento.value = false
  }
}

async function adicionarSnapshot() {
  acaoEmAndamento.value = true
  try {
    await useApi(`/producao/planejamentos/${idParam}/snapshots`, {
      method: 'POST',
      body: {
        planejamentoId: idParam,
        ordemProducaoId: snapshot.ordemProducaoId || null,
        inicio: snapshot.inicio,
        previsaoEntrega: snapshot.previsaoEntrega,
        termino: snapshot.termino,
        porcentoVenda: snapshot.porcentoVenda,
        porcentoEstoque: snapshot.porcentoEstoque,
        custoTotalPrevisto: snapshot.custoTotalPrevisto
      }
    })
    toast.success('Snapshot adicionado ao planejamento.')
    snapshotVisivel.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmAndamento.value = false
  }
}

function cancelar() { router.push('/erp/producao/planejamentos') }
const statusAtual = computed(() => registro.value?.status as number | string | undefined)
onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Planejamento de Produção' : 'Novo planejamento'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <template v-if="isEdit">
          <button type="button" class="btn btn-secondary" :disabled="acaoEmAndamento" @click="snapshotVisivel = true">+ Snapshot</button>
          <WorkflowActions :status="statusAtual" :loading="acaoEmAndamento" @acao="executarAcao" />
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
          <div class="detail-item"><span class="detail-label">Código</span><span>{{ registro.codigo || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Status</span><span class="badge" :class="classeBadgeStatus(rotuloStatusWorkflow(statusAtual))">{{ rotuloStatusWorkflow(statusAtual) }}</span></div>
          <div class="detail-item"><span class="detail-label">Responsável (ID)</span><span>{{ registro.responsavelId || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Motivo Rejeição</span><span>{{ registro.motivoRejeicao || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Criado em</span><span>{{ formatarData(registro.criadoEm as string, true) }}</span></div>
        </div>
        <p v-else-if="!carregando" class="empty-detail">Registro não encontrado.</p>
      </div>

      <form v-else class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" maxlength="60" hint="Opcional" />
          <TextField v-model="form.responsavelId" label="Responsável (ID)" required :error="erros.responsavelId" hint="UUID do responsável" />
        </div>
      </form>
    </div>

    <AppDialog v-model="snapshotVisivel" title="Novo snapshot de operação" width="560px">
      <div class="form-grid">
        <TextField v-model="snapshot.ordemProducaoId" label="Ordem de produção (ID)" hint="UUID (opcional)" />
        <DateTimeField v-model="snapshot.inicio" label="Início" mode="datetime" />
        <DateTimeField v-model="snapshot.previsaoEntrega" label="Previsão de entrega" mode="datetime" />
        <DateTimeField v-model="snapshot.termino" label="Término" mode="datetime" />
        <PercentInput v-model="snapshot.porcentoVenda" label="% Venda" />
        <PercentInput v-model="snapshot.porcentoEstoque" label="% Estoque" />
        <MoneyInput v-model="snapshot.custoTotalPrevisto" label="Custo total previsto" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="snapshotVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoEmAndamento" @click="adicionarSnapshot">Adicionar</button>
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
.empty-detail { color: var(--text-secondary); }
</style>
