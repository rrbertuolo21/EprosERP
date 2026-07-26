<script setup lang="ts">
/**
 * Tarefa — criação e detalhe com ações (concluir, mover, progresso).
 * POST /projetos/rastreamento/tarefas · GET /projetos/rastreamento/tarefas/{id}.
 */
import { ref, computed, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import { fmtData } from '~/components/projetos-shared/statusWorkflow'

definePageMeta({ layout: 'default' })

interface Tarefa {
  id: string; projetoId?: string | null; titulo?: string | null; descricao?: string | null
  estado?: number | null; prioridade?: string | null; dataInicio?: string | null; dataTermino?: string | null
  duracao?: number | null; esforcoEstimado?: number | null; percentualConcluido?: number | null
  indicadorMarco?: boolean; visibilidade?: string | null; ordem?: number | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const item = ref<Tarefa | null>(null)

const ESTADO_TAREFA: Record<number, string> = {
  0: 'Planejada', 1: 'Em execução', 2: 'Bloqueada', 3: 'Concluída', 4: 'Adiada', 5: 'Cancelada', 6: 'Arquivada'
}
function rotuloEstado(v: unknown): string { return ESTADO_TAREFA[Number(v)] ?? '—' }

const form = reactive({
  projetoId: (route.query.projetoId as string) || '',
  titulo: '', descricao: '', estagioId: '', marcoId: '', prioridade: '',
  dataInicio: null as string | null, dataTermino: null as string | null,
  duracao: 0, esforcoEstimado: 0, tarefaSuperiorId: '', indicadorMarco: false, visibilidade: '', ordem: 0
})
const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.projetoId) erros.projetoId = 'Projeto é obrigatório.'
  if (!form.titulo) erros.titulo = 'Título é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const r = await useApi(`/projetos/rastreamento/tarefas/${idParam}`)
    item.value = extrairDados<Tarefa>(r) ?? null
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { carregando.value = false }
}
async function salvar() {
  if (!validar()) { toast.error('Formulário possui erros de validação.'); return }
  salvando.value = true
  try {
    await useApi('/projetos/rastreamento/tarefas', {
      method: 'POST',
      body: {
        projetoId: form.projetoId, titulo: form.titulo, descricao: form.descricao || null,
        estagioId: form.estagioId || null, marcoId: form.marcoId || null, prioridade: form.prioridade || null,
        dataInicio: form.dataInicio, dataTermino: form.dataTermino, duracao: form.duracao,
        esforcoEstimado: form.esforcoEstimado, tarefaSuperiorId: form.tarefaSuperiorId || null,
        indicadorMarco: form.indicadorMarco, visibilidade: form.visibilidade || null, ordem: form.ordem
      }
    })
    toast.success('Tarefa criada com sucesso!')
    router.push('/erp/projetos/tarefas')
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { salvando.value = false }
}
function voltar() { router.push('/erp/projetos/tarefas') }

const acaoSalvando = ref(false)
async function concluir() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/rastreamento/tarefas/${idParam}/concluir`, { method: 'POST' })
    toast.success('Tarefa concluída.'); await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

const moverDialog = ref(false)
const moverForm = reactive({ estagioId: '', novaOrdem: 0 })
async function mover() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/rastreamento/tarefas/${idParam}/mover`, { method: 'POST', body: { ...moverForm } })
    toast.success('Tarefa movida.'); moverDialog.value = false; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

const progDialog = ref(false)
const percentual = ref(0)
async function salvarProgresso() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/rastreamento/tarefas/${idParam}/progresso`, { method: 'POST', body: { percentualConcluido: percentual.value } })
    toast.success('Progresso atualizado.'); progDialog.value = false; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

onMounted(carregar)
</script>

<template>
  <div>
    <template v-if="!isEdit">
      <PageToolbar title="Nova tarefa">
        <template #actions>
          <button type="button" class="btn btn-secondary" @click="voltar">Cancelar</button>
          <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
            <span v-if="salvando" class="spinner"></span><span v-else>Salvar</span>
          </button>
        </template>
      </PageToolbar>
      <div class="glass-panel form-panel">
        <div class="form-grid">
          <TextField v-model="form.projetoId" label="Projeto (ID)" required :error="erros.projetoId" hint="UUID do projeto" />
          <TextField v-model="form.titulo" label="Título" required :error="erros.titulo" />
          <TextField v-model="form.prioridade" label="Prioridade" />
          <TextField v-model="form.estagioId" label="Estágio (ID)" hint="UUID (opcional)" />
          <TextField v-model="form.marcoId" label="Marco (ID)" hint="UUID (opcional)" />
          <TextField v-model="form.tarefaSuperiorId" label="Tarefa superior (ID)" hint="UUID (opcional)" />
          <DateTimeField v-model="form.dataInicio" label="Início" mode="datetime" />
          <DateTimeField v-model="form.dataTermino" label="Término" mode="datetime" />
          <QuantityInput v-model="form.duracao" label="Duração" :decimais="2" />
          <QuantityInput v-model="form.esforcoEstimado" label="Esforço estimado" :decimais="2" />
          <TextField v-model="form.visibilidade" label="Visibilidade" />
          <QuantityInput v-model="form.ordem" label="Ordem" :decimais="0" />
          <TextField v-model="form.descricao" label="Descrição" />
          <label class="field toggle-row">
            <span class="field-label">É marco?</span>
            <input v-model="form.indicadorMarco" type="checkbox" />
          </label>
        </div>
      </div>
    </template>

    <template v-else>
      <PageToolbar :title="item?.titulo || 'Tarefa'" :subtitle="rotuloEstado(item?.estado)" :loading="carregando">
        <template #actions>
          <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
        </template>
      </PageToolbar>

      <div v-if="item" class="glass-panel form-panel">
        <div class="detail-grid">
          <div><span class="dl">Prioridade</span><span class="dv">{{ item.prioridade || '—' }}</span></div>
          <div><span class="dl">Início</span><span class="dv">{{ fmtData(item.dataInicio) }}</span></div>
          <div><span class="dl">Término</span><span class="dv">{{ fmtData(item.dataTermino) }}</span></div>
          <div><span class="dl">Conclusão</span><span class="dv">{{ Number(item.percentualConcluido ?? 0).toFixed(0) }}%</span></div>
          <div><span class="dl">Esforço estimado</span><span class="dv">{{ item.esforcoEstimado == null ? '—' : Number(item.esforcoEstimado).toLocaleString('pt-BR') }}</span></div>
        </div>
        <p v-if="item.descricao" class="descricao">{{ item.descricao }}</p>
      </div>

      <div class="glass-panel form-panel mt-2">
        <div class="section-head"><h3>Ações</h3></div>
        <div class="btn-row">
          <button type="button" class="btn btn-ghost btn-sm" @click="concluir">Concluir</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="moverDialog = true">Mover</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="progDialog = true">Progresso</button>
        </div>
      </div>
    </template>

    <AppDialog v-model="moverDialog" title="Mover tarefa" width="480px">
      <div class="form-grid">
        <TextField v-model="moverForm.estagioId" label="Estágio destino (ID)" required hint="UUID" />
        <QuantityInput v-model="moverForm.novaOrdem" label="Nova ordem" :decimais="0" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="moverDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="mover">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Mover</span>
        </button>
      </template>
    </AppDialog>

    <AppDialog v-model="progDialog" title="Atualizar progresso" width="420px">
      <PercentInput v-model="percentual" label="Percentual concluído" />
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="progDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="salvarProgresso">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.detail-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; }
.detail-grid .dl { display: block; font-size: 12px; color: var(--text-secondary); }
.detail-grid .dv { display: block; font-size: 14px; color: var(--text-primary); font-weight: 600; }
.descricao { margin-top: 16px; color: var(--text-secondary); font-size: 14px; }
.section-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
.section-head h3 { font-size: 15px; }
.btn-row { display: flex; gap: 8px; flex-wrap: wrap; }
.toggle-row { display: flex; align-items: center; gap: 10px; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
.mt-2 { margin-top: 16px; }
</style>
