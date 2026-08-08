<script setup lang="ts">
/**
 * Risco de projeto — criação e detalhe com ações de workflow.
 * POST /projetos/riscos · GET /projetos/riscos/{id} · GET /projetos/riscos/estagios (para o select).
 */
import { ref, computed, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import type { SelectOption } from '~/composables/useEnum'
import { rotuloStatusWorkflow, rotuloPrioridadeRisco, PRIORIDADE_RISCO_OPCOES } from '~/components/projetos-shared/statusWorkflow'

definePageMeta({ layout: 'default' })

interface Comentario { id: string; usuarioId?: string | null; comentario?: string | null }
interface Risco {
  id: string; projetoId?: string | null; titulo?: string | null; descricao?: string | null
  prioridade?: number | null; status?: number | null; estagioId?: string | null
  probabilidade?: number | null; impacto?: number | null; motivoRejeicao?: string | null
  comentarios?: Comentario[]
}
interface EstagioRisco { id: string; nome?: string | null }

const route = useRoute()
const router = useRouter()
const toast = useToast()
const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const item = ref<Risco | null>(null)
const estagios = ref<SelectOption[]>([])

const form = reactive({
  projetoId: (route.query.projetoId as string) || '',
  titulo: '', prioridade: 1 as number | null, descricao: '', estagioId: '',
  responsaveis: '', criadorId: ''
})
const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.projetoId) erros.projetoId = 'Projeto é obrigatório.'
  if (!form.titulo) erros.titulo = 'Título é obrigatório.'
  if (!form.estagioId) erros.estagioId = 'Estágio é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregarEstagios() {
  try {
    const r = await useApi('/projetos/riscos/estagios')
    const d = extrairLista<EstagioRisco>(r) ?? []
    estagios.value = d.map((e) => ({ label: e.nome ?? e.id, value: e.id }))
  } catch (e) { console.error('[riscos/[id]] estagios', e); estagios.value = [] }
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const r = await useApi(`/projetos/riscos/${idParam}`)
    item.value = extrairDados<Risco>(r) ?? null
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { carregando.value = false }
}
async function salvar() {
  if (!validar()) { toast.error('Formulário possui erros de validação.'); return }
  salvando.value = true
  try {
    const responsaveis = form.responsaveis.split(',').map((s) => s.trim()).filter(Boolean)
    await useApi('/projetos/riscos', {
      method: 'POST',
      body: {
        projetoId: form.projetoId, titulo: form.titulo, prioridade: form.prioridade,
        descricao: form.descricao || null, estagioId: form.estagioId,
        responsaveis: responsaveis.length ? responsaveis : null, criadorId: form.criadorId || null
      }
    })
    toast.success('Risco criado com sucesso!')
    router.push('/erp/projetos/riscos')
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { salvando.value = false }
}
function voltar() { router.push('/erp/projetos/riscos') }

/* ações com usuarioId */
const acaoSalvando = ref(false)
const acaoUsuario = ref<null | 'submeter' | 'aprovar' | 'escalonar'>(null)
const usuarioId = ref('')
const rotuloAcao: Record<string, string> = { submeter: 'Submeter', aprovar: 'Aprovar', escalonar: 'Escalonar' }
async function confirmarAcaoUsuario() {
  if (!acaoUsuario.value) return
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/riscos/${idParam}/${acaoUsuario.value}`, { method: 'POST', body: { usuarioId: usuarioId.value } })
    toast.success('Ação executada.'); acaoUsuario.value = null; usuarioId.value = ''; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

/* rejeitar / encerrar */
const motivoDialog = ref<null | 'rejeitar' | 'encerrar'>(null)
const motivoForm = reactive({ motivo: '', usuarioId: '' })
async function confirmarMotivo() {
  if (!motivoDialog.value) return
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/riscos/${idParam}/${motivoDialog.value}`, { method: 'POST', body: { ...motivoForm } })
    toast.success('Ação executada.'); motivoDialog.value = null; motivoForm.motivo = ''; motivoForm.usuarioId = ''; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

/* mover */
const moverDialog = ref(false)
const moverForm = reactive({ estagioDestinoId: '', usuarioId: '' })
async function mover() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/riscos/${idParam}/mover`, { method: 'POST', body: { ...moverForm } })
    toast.success('Risco movido.'); moverDialog.value = false; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

/* prioridade */
const prioDialog = ref(false)
const prioForm = reactive({ prioridade: 1 as number | null, usuarioId: '' })
async function alterarPrioridade() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/riscos/${idParam}/prioridade`, { method: 'POST', body: { ...prioForm } })
    toast.success('Prioridade atualizada.'); prioDialog.value = false; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

/* comentário */
const comentarioDialog = ref(false)
const comentarioForm = reactive({ usuarioId: '', comentario: '' })
async function comentar() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/riscos/${idParam}/comentarios`, { method: 'POST', body: { ...comentarioForm } })
    toast.success('Comentário adicionado.'); comentarioDialog.value = false; comentarioForm.comentario = ''; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

onMounted(async () => {
  await carregarEstagios()
  await carregar()
})
</script>

<template>
  <div>
    <template v-if="!isEdit">
      <PageToolbar title="Novo risco">
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
          <SelectField v-model="form.prioridade" label="Prioridade" :options="PRIORIDADE_RISCO_OPCOES" :clearable="false" />
          <SelectField v-model="form.estagioId" label="Estágio" required :options="estagios" :error="erros.estagioId" placeholder="Selecione..." />
          <TextField v-model="form.criadorId" label="Criador (ID)" hint="UUID (opcional)" />
          <TextField v-model="form.responsaveis" label="Responsáveis (IDs)" hint="UUIDs separados por vírgula" />
          <TextField v-model="form.descricao" label="Descrição" />
        </div>
      </div>
    </template>

    <template v-else>
      <PageToolbar :title="item?.titulo || 'Risco'" :subtitle="rotuloStatusWorkflow(item?.status)" :loading="carregando">
        <template #actions>
          <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
        </template>
      </PageToolbar>

      <div v-if="item" class="glass-panel form-panel">
        <div class="detail-grid">
          <div><span class="dl">Prioridade</span><span class="dv">{{ rotuloPrioridadeRisco(item.prioridade) }}</span></div>
          <div><span class="dl">Probabilidade</span><span class="dv">{{ item.probabilidade ?? '—' }}</span></div>
          <div><span class="dl">Impacto</span><span class="dv">{{ item.impacto ?? '—' }}</span></div>
          <div v-if="item.motivoRejeicao"><span class="dl">Motivo rejeição</span><span class="dv">{{ item.motivoRejeicao }}</span></div>
        </div>
        <p v-if="item.descricao" class="descricao">{{ item.descricao }}</p>
      </div>

      <div class="glass-panel form-panel mt-2">
        <div class="section-head"><h3>Ações</h3></div>
        <div class="btn-row">
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoUsuario = 'submeter'">Submeter</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoUsuario = 'aprovar'">Aprovar</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoUsuario = 'escalonar'">Escalonar</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="moverDialog = true">Mover</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="prioDialog = true">Prioridade</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="comentarioDialog = true">Comentar</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="motivoDialog = 'rejeitar'">Rejeitar</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="motivoDialog = 'encerrar'">Encerrar</button>
        </div>
      </div>

      <div class="glass-panel form-panel mt-2">
        <div class="section-head"><h3>Comentários</h3></div>
        <table class="admin-table">
          <thead><tr><th>Usuário (ID)</th><th>Comentário</th></tr></thead>
          <tbody>
            <tr v-if="!item?.comentarios?.length"><td colspan="2"><div class="table-empty">Nenhum comentário.</div></td></tr>
            <tr v-for="c in item?.comentarios ?? []" :key="c.id">
              <td>{{ c.usuarioId }}</td>
              <td>{{ c.comentario }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <!-- ação usuarioId -->
    <AppDialog :model-value="acaoUsuario !== null" :title="acaoUsuario ? rotuloAcao[acaoUsuario] : ''" width="420px" @update:model-value="acaoUsuario = null">
      <TextField v-model="usuarioId" label="Usuário (ID)" required hint="UUID" />
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="acaoUsuario = null">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="confirmarAcaoUsuario">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Confirmar</span>
        </button>
      </template>
    </AppDialog>

    <!-- motivo -->
    <AppDialog :model-value="motivoDialog !== null" :title="motivoDialog === 'encerrar' ? 'Encerrar risco' : 'Rejeitar risco'" width="480px" @update:model-value="motivoDialog = null">
      <div class="form-grid">
        <TextField v-model="motivoForm.usuarioId" label="Usuário (ID)" required hint="UUID" />
        <TextField v-model="motivoForm.motivo" label="Motivo" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="motivoDialog = null">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="confirmarMotivo">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Confirmar</span>
        </button>
      </template>
    </AppDialog>

    <!-- mover -->
    <AppDialog v-model="moverDialog" title="Mover risco" width="480px">
      <div class="form-grid">
        <SelectField v-model="moverForm.estagioDestinoId" label="Estágio destino" :options="estagios" placeholder="Selecione..." />
        <TextField v-model="moverForm.usuarioId" label="Usuário (ID)" required hint="UUID" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="moverDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="mover">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Mover</span>
        </button>
      </template>
    </AppDialog>

    <!-- prioridade -->
    <AppDialog v-model="prioDialog" title="Alterar prioridade" width="480px">
      <div class="form-grid">
        <SelectField v-model="prioForm.prioridade" label="Prioridade" :options="PRIORIDADE_RISCO_OPCOES" :clearable="false" />
        <TextField v-model="prioForm.usuarioId" label="Usuário (ID)" required hint="UUID" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="prioDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="alterarPrioridade">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>

    <!-- comentário -->
    <AppDialog v-model="comentarioDialog" title="Adicionar comentário" width="480px">
      <div class="form-grid">
        <TextField v-model="comentarioForm.usuarioId" label="Usuário (ID)" required hint="UUID" />
        <TextField v-model="comentarioForm.comentario" label="Comentário" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="comentarioDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="comentar">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Comentar</span>
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
.mt-2 { margin-top: 16px; }
</style>
