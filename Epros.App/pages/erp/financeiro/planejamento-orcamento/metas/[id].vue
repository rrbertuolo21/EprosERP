<script setup lang="ts">
/**
 * Meta — novo (POST) e detalhe (GET /{id}) com ações.
 * POST /planejamento-orcamento/metas · GET /{id} · POST /{id}/ativar ·
 * POST /{id}/contribuicoes · POST /{id}/tracking.
 * Observação: categoriaId é UUID e a API só expõe POST de categorias (sem GET de listagem),
 * então o campo é texto (UUID) — crie a categoria pela tela de listagem de metas.
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
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'

definePageMeta({ layout: 'default' })

interface MetaDetalhe {
  id?: string
  categoriaId: string | null
  tipo: string | null
  prioridade: string | null
  dataInicio: string | null
  dataAlvo: string | null
  statusDescricao?: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const form = reactive<MetaDetalhe>({ categoriaId: null, tipo: null, prioridade: null, dataInicio: null, dataAlvo: null })
const erros = reactive<Record<string, string>>({})

// contribuicao
const contribVisivel = ref(false)
const salvandoContrib = ref(false)
const contrib = reactive<{ valor: number | null; tipo: string | null; data: string | null }>({ valor: null, tipo: null, data: null })

// tracking
const trackingVisivel = ref(false)
const salvandoTracking = ref(false)
const tracking = reactive<{ percentual: number | null; statusProgresso: string | null; data: string | null }>({ percentual: null, statusProgresso: null, data: null })

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.categoriaId) erros.categoriaId = 'Categoria é obrigatória.'
  if (!form.dataInicio) erros.dataInicio = 'Data de início é obrigatória.'
  if (!form.dataAlvo) erros.dataAlvo = 'Data alvo é obrigatória.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi('/planejamento-orcamento/metas/{id}', { params: { id: idParam } })
    const dados = extrairDados<MetaDetalhe>(resposta)
    if (dados) Object.assign(form, dados)
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
    await useApi('/planejamento-orcamento/metas', {
      method: 'POST',
      body: { categoriaId: form.categoriaId, tipo: form.tipo, prioridade: form.prioridade, dataInicio: form.dataInicio, dataAlvo: form.dataAlvo }
    })
    toast.success('Meta criada com sucesso!')
    router.push('/erp/financeiro/planejamento-orcamento/metas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function ativar() {
  const ok = await confirmRef.value!.open('Ativar meta', 'Confirma a ativação desta meta?')
  if (!ok) return
  try {
    await useApi('/planejamento-orcamento/metas/{id}/ativar', { method: 'POST', params: { id: idParam } })
    toast.success('Meta ativada.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

async function salvarContribuicao() {
  if (contrib.valor == null) {
    toast.error('Informe o valor da contribuição.')
    return
  }
  salvandoContrib.value = true
  try {
    await useApi('/planejamento-orcamento/metas/{id}/contribuicoes', {
      method: 'POST',
      params: { id: idParam },
      body: { metaId: idParam, valor: contrib.valor, tipo: contrib.tipo, data: contrib.data }
    })
    toast.success('Contribuição registrada.')
    contribVisivel.value = false
    contrib.valor = null
    contrib.tipo = null
    contrib.data = null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoContrib.value = false
  }
}

async function salvarTracking() {
  if (tracking.percentual == null) {
    toast.error('Informe o percentual de progresso.')
    return
  }
  salvandoTracking.value = true
  try {
    await useApi('/planejamento-orcamento/metas/{id}/tracking', {
      method: 'POST',
      params: { id: idParam },
      body: { metaId: idParam, percentual: tracking.percentual, statusProgresso: tracking.statusProgresso, data: tracking.data }
    })
    toast.success('Progresso registrado.')
    trackingVisivel.value = false
    tracking.percentual = null
    tracking.statusProgresso = null
    tracking.data = null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoTracking.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/planejamento-orcamento/metas')
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Meta' : 'Nova meta'" :loading="carregando || salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <template v-if="isEdit">
          <button type="button" class="btn btn-secondary" @click="contribVisivel = true">Contribuição</button>
          <button type="button" class="btn btn-secondary" @click="trackingVisivel = true">Progresso</button>
          <button type="button" class="btn btn-primary" @click="ativar">Ativar</button>
        </template>
        <button v-else type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div v-if="isEdit && form.statusDescricao" class="status-linha">Status: <strong>{{ form.statusDescricao }}</strong></div>
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <!-- categoriaId: UUID (a API não expõe GET de categorias). -->
          <TextField v-model="form.categoriaId" label="ID da categoria" required hint="UUID" :disabled="isEdit" :error="erros.categoriaId" />
          <TextField v-model="form.tipo" label="Tipo" maxlength="40" :disabled="isEdit" />
          <TextField v-model="form.prioridade" label="Prioridade" maxlength="40" :disabled="isEdit" />
          <DateTimeField v-model="form.dataInicio" label="Data de início" mode="datetime" required :disabled="isEdit" :error="erros.dataInicio" />
          <DateTimeField v-model="form.dataAlvo" label="Data alvo" mode="datetime" required :disabled="isEdit" :error="erros.dataAlvo" />
        </div>
      </form>
    </div>

    <AppDialog v-model="contribVisivel" title="Registrar contribuição" width="440px">
      <div class="form-grid-modal">
        <MoneyInput v-model="contrib.valor" label="Valor" />
        <TextField v-model="contrib.tipo" label="Tipo" maxlength="40" />
        <DateTimeField v-model="contrib.data" label="Data" mode="datetime" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="contribVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoContrib" @click="salvarContribuicao">
          <span v-if="salvandoContrib" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>

    <AppDialog v-model="trackingVisivel" title="Registrar progresso" width="440px">
      <div class="form-grid-modal">
        <PercentInput v-model="tracking.percentual" label="Percentual" />
        <TextField v-model="tracking.statusProgresso" label="Status do progresso" maxlength="60" />
        <DateTimeField v-model="tracking.data" label="Data" mode="datetime" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="trackingVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoTracking" @click="salvarTracking">
          <span v-if="salvandoTracking" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.form-grid-modal { display: grid; grid-template-columns: 1fr; gap: 14px; }
.status-linha { margin-bottom: 14px; color: var(--text-secondary); font-size: 13px; }
</style>
