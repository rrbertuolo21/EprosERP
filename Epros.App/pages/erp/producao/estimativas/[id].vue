<script setup lang="ts">
/**
 * Estimativa de Produção — criação (novo) e detalhe/workflow (id existente).
 *
 * 'novo' → formulário de criação (POST /producao/estimativas).
 * id existente → detalhe somente-leitura (GET /producao/estimativas/{id}) + ações de
 * workflow (submeter/aprovar/rejeitar/inativar/reativar/encerrar) e conversão em planejamento.
 * Sem PUT no backend: não há edição de campos após criado.
 *
 * Campos com palpite: responsavelId, propostaReferenciaId, estruturaRascunhoId são uuid
 * sem endpoint de listagem próprio no módulo → entram como TextField (uuid). A coleção
 * `componentes` do POST não é editável aqui (sem sub-endpoint de manutenção) — ver relatório.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import WorkflowActions from '~/components/producao-shared/WorkflowActions.vue'
import { rotuloStatusWorkflow, classeBadgeStatus, formatarData, formatarMoeda } from '~/components/producao-shared/producao'

definePageMeta({ layout: 'default' })

interface EstimativaForm {
  codigo: string | null
  responsavelId: string
  propostaReferenciaId: string | null
  estruturaRascunhoId: string | null
  custoPrevistoTotal: number | null
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
const converterVisivel = ref(false)
const planejamentoOrigemId = ref('')

const form = reactive<EstimativaForm>({
  codigo: null,
  responsavelId: '',
  propostaReferenciaId: null,
  estruturaRascunhoId: null,
  custoPrevistoTotal: null
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
    const resposta = await useApi(`/producao/estimativas/${idParam}`)
    registro.value = extrairDados<Record<string, unknown>>(resposta) ?? null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/producao/estimativas', { method: 'POST', body: form })
    toast.success('Estimativa criada com sucesso!')
    router.push('/erp/producao/estimativas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function executarAcao(chave: string, motivo?: string) {
  acaoEmAndamento.value = true
  try {
    await useApi(`/producao/estimativas/${idParam}/${chave}`, {
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

async function converter() {
  if (!planejamentoOrigemId.value) {
    toast.error('Informe o planejamento de origem.')
    return
  }
  acaoEmAndamento.value = true
  try {
    await useApi(`/producao/estimativas/${idParam}/converter`, {
      method: 'POST',
      body: { id: idParam, planejamentoOrigemId: planejamentoOrigemId.value }
    })
    toast.success('Estimativa convertida em planejamento.')
    converterVisivel.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmAndamento.value = false
  }
}

function cancelar() {
  router.push('/erp/producao/estimativas')
}

const statusAtual = computed(() => registro.value?.status as number | string | undefined)

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Estimativa' : 'Nova estimativa'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <template v-if="isEdit">
          <button type="button" class="btn btn-secondary" :disabled="acaoEmAndamento" @click="converterVisivel = true">Converter</button>
          <WorkflowActions :status="statusAtual" :loading="acaoEmAndamento" @acao="executarAcao" />
        </template>
        <button v-else type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <!-- Detalhe (id existente) -->
      <div v-if="isEdit">
        <div v-if="registro" class="detail-grid">
          <div class="detail-item"><span class="detail-label">Código</span><span>{{ registro.codigo || '—' }}</span></div>
          <div class="detail-item">
            <span class="detail-label">Status</span>
            <span class="badge" :class="classeBadgeStatus(rotuloStatusWorkflow(statusAtual))">{{ rotuloStatusWorkflow(statusAtual) }}</span>
          </div>
          <div class="detail-item"><span class="detail-label">Custo Previsto Total</span><span>{{ formatarMoeda(registro.custoPrevistoTotal as number) }}</span></div>
          <div class="detail-item"><span class="detail-label">Responsável (ID)</span><span>{{ registro.responsavelId || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Proposta (ID)</span><span>{{ registro.propostaReferenciaId || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Estrutura Rascunho (ID)</span><span>{{ registro.estruturaRascunhoId || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Motivo Rejeição</span><span>{{ registro.motivoRejeicao || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Criado em</span><span>{{ formatarData(registro.criadoEm as string, true) }}</span></div>
        </div>
        <p v-else-if="!carregando" class="empty-detail">Registro não encontrado.</p>
      </div>

      <!-- Criação (novo) -->
      <form v-else class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" maxlength="60" hint="Opcional — gerado se vazio" />
          <TextField v-model="form.responsavelId" label="Responsável (ID)" required :error="erros.responsavelId" hint="UUID do responsável" />
          <TextField v-model="form.propostaReferenciaId" label="Proposta de referência (ID)" hint="UUID (opcional)" />
          <TextField v-model="form.estruturaRascunhoId" label="Estrutura rascunho (ID)" hint="UUID (opcional)" />
          <MoneyInput v-model="form.custoPrevistoTotal" label="Custo previsto total" />
        </div>
        <p class="form-note">Os componentes da estimativa são gerenciados após a criação (não há endpoint de manutenção de itens exposto).</p>
      </form>
    </div>

    <AppDialog v-model="converterVisivel" title="Converter estimativa" width="440px">
      <TextField v-model="planejamentoOrigemId" label="Planejamento de origem (ID)" hint="UUID do planejamento" />
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="converterVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoEmAndamento || !planejamentoOrigemId" @click="converter">Converter</button>
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
