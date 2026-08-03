<script setup lang="ts">
/**
 * Programação (ESC) — criação (novo) e detalhe/workflow (id existente).
 * POST /producao/esc/programacoes + workflow. Sem PUT.
 * Campos com palpite: responsavelId / planoProducaoId / ordemProducaoId / centroTrabalhoId
 * são uuid sem endpoint de listagem próprio → TextField (uuid). Coleção `operacoes` não
 * editável aqui (endpoint /{id}/operacoes fica como lacuna — ver relatório).
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import WorkflowActions from '~/components/producao-shared/WorkflowActions.vue'
import { rotuloStatusWorkflow, classeBadgeStatus, formatarData } from '~/components/producao-shared/producao'

definePageMeta({ layout: 'default' })

interface ProgramacaoForm {
  codigo: string | null
  responsavelId: string
  planoProducaoId: string | null
  ordemProducaoId: string | null
  centroTrabalhoId: string | null
  prioridade: number | string | null
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

const form = reactive<ProgramacaoForm>({
  codigo: null,
  responsavelId: '',
  planoProducaoId: null,
  ordemProducaoId: null,
  centroTrabalhoId: null,
  prioridade: null
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
    const resposta = await useApi(`/producao/esc/programacoes/${idParam}`)
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
    await useApi('/producao/esc/programacoes', {
      method: 'POST',
      body: { ...form, prioridade: form.prioridade != null ? Number(form.prioridade) : null }
    })
    toast.success('Programação criada com sucesso!')
    router.push('/erp/producao/programacoes')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function executarAcao(chave: string, motivo?: string) {
  acaoEmAndamento.value = true
  try {
    await useApi(`/producao/esc/programacoes/${idParam}/${chave}`, {
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

function cancelar() { router.push('/erp/producao/programacoes') }
const statusAtual = computed(() => registro.value?.status as number | string | undefined)
onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Programação' : 'Nova programação'" :loading="carregando">
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
          <div class="detail-item"><span class="detail-label">Prioridade</span><span>{{ registro.prioridade ?? '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Plano de Produção (ID)</span><span>{{ registro.planoProducaoId || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Ordem de Produção (ID)</span><span>{{ registro.ordemProducaoId || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Centro de Trabalho (ID)</span><span>{{ registro.centroTrabalhoId || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Responsável (ID)</span><span>{{ registro.responsavelId || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Criado em</span><span>{{ formatarData(registro.criadoEm as string, true) }}</span></div>
        </div>
        <p v-else-if="!carregando" class="empty-detail">Registro não encontrado.</p>
      </div>

      <form v-else class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" maxlength="60" hint="Opcional" />
          <TextField v-model="form.responsavelId" label="Responsável (ID)" required :error="erros.responsavelId" hint="UUID do responsável" />
          <TextField v-model="form.planoProducaoId" label="Plano de produção (ID)" hint="UUID (opcional)" />
          <TextField v-model="form.ordemProducaoId" label="Ordem de produção (ID)" hint="UUID (opcional)" />
          <TextField v-model="form.centroTrabalhoId" label="Centro de trabalho (ID)" hint="UUID (opcional)" />
          <TextField v-model="form.prioridade" label="Prioridade" type="number" hint="Número inteiro" />
        </div>
        <p class="form-note">As operações da programação são adicionadas após a criação (endpoint de operações não exposto nesta tela).</p>
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
