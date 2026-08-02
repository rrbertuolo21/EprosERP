<script setup lang="ts">
/**
 * Encerramento de projeto — criação e detalhe com ações de workflow.
 * POST /projetos/encerramento · GET /projetos/encerramento/{id}.
 */
import { ref, computed, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import { rotuloStatusWorkflow, STATUS_FINAL_PROJETO_OPCOES } from '~/components/projetos-shared/statusWorkflow'

definePageMeta({ layout: 'default' })

interface ItemEnc { id: string; sequencia?: number; quantidade?: number | null; observacao?: string | null }
interface Encerramento {
  id: string; codigo?: string | null; descricao?: string | null; status?: number | null
  projetoId?: string | null; responsavelId?: string | null; statusFinalProjeto?: number | null
  motivoRejeicao?: string | null; versao?: number | null; itens?: ItemEnc[]
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const item = ref<Encerramento | null>(null)

const form = reactive({ projetoId: '', codigo: '', descricao: '', responsavelId: '' })
const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.projetoId) erros.projetoId = 'Projeto é obrigatório.'
  if (!form.responsavelId) erros.responsavelId = 'Responsável é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const r = await useApi(`/projetos/encerramento/${idParam}`)
    item.value = extrairDados<Encerramento>(r) ?? null
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { carregando.value = false }
}
async function salvar() {
  if (!validar()) { toast.error('Formulário possui erros de validação.'); return }
  salvando.value = true
  try {
    await useApi('/projetos/encerramento', { method: 'POST', body: { ...form } })
    toast.success('Encerramento criado com sucesso!')
    router.push('/erp/projetos/encerramento')
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { salvando.value = false }
}
function voltar() { router.push('/erp/projetos/encerramento') }

const acaoSalvando = ref(false)
async function acaoSimples(acao: 'submeter' | 'arquivar' | 'retomar' | 'suspender') {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/encerramento/${idParam}/${acao}`, { method: 'POST' })
    toast.success('Ação executada.'); await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

/* aprovar (statusFinalProjeto) */
const aprovarDialog = ref(false)
const statusFinal = ref<number | null>(4)
async function aprovar() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/encerramento/${idParam}/aprovar`, { method: 'POST', body: { statusFinalProjeto: statusFinal.value } })
    toast.success('Encerramento aprovado.'); aprovarDialog.value = false; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

/* rejeitar / encerrar (motivo) */
const motivoDialog = ref<null | 'rejeitar' | 'encerrar'>(null)
const motivo = ref('')
async function confirmarMotivo() {
  if (!motivoDialog.value) return
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/encerramento/${idParam}/${motivoDialog.value}`, { method: 'POST', body: { motivo: motivo.value } })
    toast.success('Ação executada.'); motivoDialog.value = null; motivo.value = ''; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

/* item */
const itemDialog = ref(false)
const itemForm = reactive({ sequencia: 1, quantidade: 0, observacao: '' })
async function adicionarItem() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/encerramento/${idParam}/itens`, {
      method: 'POST', body: { sequencia: itemForm.sequencia, quantidade: itemForm.quantidade, observacao: itemForm.observacao || null }
    })
    toast.success('Item adicionado.'); itemDialog.value = false; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

/* anexo */
const anexoDialog = ref(false)
const arquivoId = ref('')
async function adicionarAnexo() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/encerramento/${idParam}/anexos`, { method: 'POST', body: { arquivoId: arquivoId.value } })
    toast.success('Anexo adicionado.'); anexoDialog.value = false; arquivoId.value = ''
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

onMounted(carregar)
</script>

<template>
  <div>
    <template v-if="!isEdit">
      <PageToolbar title="Novo encerramento">
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
          <TextField v-model="form.responsavelId" label="Responsável (ID)" required :error="erros.responsavelId" hint="UUID do responsável" />
          <TextField v-model="form.codigo" label="Código" maxlength="40" />
          <TextField v-model="form.descricao" label="Descrição" maxlength="500" />
        </div>
      </div>
    </template>

    <template v-else>
      <PageToolbar :title="item?.codigo || 'Encerramento'" :subtitle="rotuloStatusWorkflow(item?.status)" :loading="carregando">
        <template #actions>
          <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
        </template>
      </PageToolbar>

      <div v-if="item" class="glass-panel form-panel">
        <div class="detail-grid">
          <div><span class="dl">Descrição</span><span class="dv">{{ item.descricao || '—' }}</span></div>
          <div><span class="dl">Projeto (ID)</span><span class="dv">{{ item.projetoId || '—' }}</span></div>
          <div><span class="dl">Responsável (ID)</span><span class="dv">{{ item.responsavelId || '—' }}</span></div>
          <div><span class="dl">Versão</span><span class="dv">{{ item.versao ?? '—' }}</span></div>
          <div v-if="item.motivoRejeicao"><span class="dl">Motivo rejeição</span><span class="dv">{{ item.motivoRejeicao }}</span></div>
        </div>
      </div>

      <div class="glass-panel form-panel mt-2">
        <div class="section-head"><h3>Ações</h3></div>
        <div class="btn-row">
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoSimples('submeter')">Submeter</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="aprovarDialog = true">Aprovar</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="motivoDialog = 'rejeitar'">Rejeitar</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="motivoDialog = 'encerrar'">Encerrar</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoSimples('suspender')">Suspender</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoSimples('retomar')">Retomar</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoSimples('arquivar')">Arquivar</button>
        </div>
      </div>

      <div class="glass-panel form-panel mt-2">
        <div class="section-head">
          <h3>Itens de encerramento</h3>
          <div class="btn-row">
            <button type="button" class="btn btn-ghost btn-sm" @click="anexoDialog = true">+ Anexo</button>
            <button type="button" class="btn btn-primary btn-sm" @click="itemDialog = true">+ Item</button>
          </div>
        </div>
        <table class="admin-table">
          <thead><tr><th>Seq.</th><th class="td-right">Quantidade</th><th>Observação</th></tr></thead>
          <tbody>
            <tr v-if="!item?.itens?.length"><td colspan="3"><div class="table-empty">Nenhum item.</div></td></tr>
            <tr v-for="it in item?.itens ?? []" :key="it.id">
              <td>{{ it.sequencia }}</td>
              <td class="td-right">{{ Number(it.quantidade ?? 0).toLocaleString('pt-BR') }}</td>
              <td>{{ it.observacao || '—' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <!-- Aprovar -->
    <AppDialog v-model="aprovarDialog" title="Aprovar encerramento" width="480px">
      <SelectField v-model="statusFinal" label="Status final do projeto" :options="STATUS_FINAL_PROJETO_OPCOES" :clearable="false" />
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="aprovarDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="aprovar">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Aprovar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Motivo (rejeitar/encerrar) -->
    <AppDialog :model-value="motivoDialog !== null" :title="motivoDialog === 'encerrar' ? 'Encerrar' : 'Rejeitar'" width="480px" @update:model-value="motivoDialog = null">
      <TextField v-model="motivo" label="Motivo" />
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="motivoDialog = null">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="confirmarMotivo">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Confirmar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Item -->
    <AppDialog v-model="itemDialog" title="Adicionar item" width="520px">
      <div class="form-grid">
        <QuantityInput v-model="itemForm.sequencia" label="Sequência" :decimais="0" />
        <QuantityInput v-model="itemForm.quantidade" label="Quantidade" :decimais="2" />
        <TextField v-model="itemForm.observacao" label="Observação" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="itemDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="adicionarItem">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Adicionar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Anexo -->
    <AppDialog v-model="anexoDialog" title="Adicionar anexo" width="480px">
      <TextField v-model="arquivoId" label="Arquivo (ID)" required hint="UUID" />
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="anexoDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="adicionarAnexo">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Adicionar</span>
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
.section-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
.section-head h3 { font-size: 15px; }
.btn-row { display: flex; gap: 8px; flex-wrap: wrap; }
.mt-2 { margin-top: 16px; }
</style>
