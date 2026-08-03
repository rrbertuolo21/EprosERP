<script setup lang="ts">
/**
 * Custo de Produção — criação (novo) e detalhe/workflow (id existente).
 * POST /producao/custos + workflow. Sem PUT.
 * Campos com palpite: responsavelId / referenciaId são uuid sem endpoint de listagem →
 * TextField (uuid). Coleção `referencias` não editável aqui (ver relatório).
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import WorkflowActions from '~/components/producao-shared/WorkflowActions.vue'
import { rotuloStatusWorkflow, classeBadgeStatus, formatarData, formatarMoeda } from '~/components/producao-shared/producao'

definePageMeta({ layout: 'default' })

interface CustoForm {
  codigo: string | null
  responsavelId: string
  referenciaOrigem: string | null
  referenciaId: string | null
  custoTotalPrevisto: number | null
  custoTotalRealizado: number | null
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

const form = reactive<CustoForm>({
  codigo: null,
  responsavelId: '',
  referenciaOrigem: null,
  referenciaId: null,
  custoTotalPrevisto: null,
  custoTotalRealizado: null
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.responsavelId) erros.responsavelId = 'Responsável é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/producao/custos/${idParam}`)
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
    await useApi('/producao/custos', { method: 'POST', body: form })
    toast.success('Custo criado com sucesso!')
    router.push('/erp/producao/custos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function executarAcao(chave: string, motivo?: string) {
  acaoEmAndamento.value = true
  try {
    await useApi(`/producao/custos/${idParam}/${chave}`, {
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

function cancelar() { router.push('/erp/producao/custos') }
const statusAtual = computed(() => registro.value?.status as number | string | undefined)
onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Custo de produção' : 'Novo custo'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <WorkflowActions v-if="isEdit" :status="statusAtual" :loading="acaoEmAndamento" @acao="executarAcao" />
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
          <div class="detail-item"><span class="detail-label">Custo Previsto</span><span>{{ formatarMoeda(registro.custoTotalPrevisto as number) }}</span></div>
          <div class="detail-item"><span class="detail-label">Custo Realizado</span><span>{{ formatarMoeda(registro.custoTotalRealizado as number) }}</span></div>
          <div class="detail-item"><span class="detail-label">Desvio Total</span><span>{{ formatarMoeda(registro.desvioTotal as number) }}</span></div>
          <div class="detail-item"><span class="detail-label">Referência Origem</span><span>{{ registro.referenciaOrigem || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Responsável (ID)</span><span>{{ registro.responsavelId || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Criado em</span><span>{{ formatarData(registro.criadoEm as string, true) }}</span></div>
        </div>
        <p v-else-if="!carregando" class="empty-detail">Registro não encontrado.</p>
      </div>

      <form v-else class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" maxlength="60" hint="Opcional" />
          <TextField v-model="form.responsavelId" label="Responsável (ID)" required :error="erros.responsavelId" hint="UUID do responsável" />
          <TextField v-model="form.referenciaOrigem" label="Referência de origem" maxlength="80" hint="Ex.: OrdemProducao, Estimativa" />
          <TextField v-model="form.referenciaId" label="Referência (ID)" hint="UUID (opcional)" />
          <MoneyInput v-model="form.custoTotalPrevisto" label="Custo total previsto" />
          <MoneyInput v-model="form.custoTotalRealizado" label="Custo total realizado" />
        </div>
        <p class="form-note">As referências de custo são gerenciadas após a criação (sem endpoint de manutenção de itens exposto).</p>
      </form>
    </div>
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
