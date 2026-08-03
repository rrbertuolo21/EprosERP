<script setup lang="ts">
/**
 * Reavaliação Cambial — novo (POST) e detalhe (GET /{id}) com ações.
 * POST /cambio-risco/reavaliacoes (cria header) · GET /{id} (detalhe) ·
 * POST /{id}/aprovar · /{id}/cancelar · /{id}/contabilizar.
 * Observação: o corpo do POST aceita `itens` (array de objetos) cujo schema não é
 * detalhado no digest; a tela cria apenas o cabeçalho (itens fica como lacuna).
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'

definePageMeta({ layout: 'default' })

interface ReavaliacaoDetalhe {
  id?: string
  dataReavaliacao: string | null
  observacao: string | null
  statusDescricao?: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { formatarData } = useHelper()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const form = reactive<ReavaliacaoDetalhe>({ dataReavaliacao: null, observacao: null })
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.dataReavaliacao) erros.dataReavaliacao = 'Data da reavaliação é obrigatória.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi('/cambio-risco/reavaliacoes/{id}', { params: { id: idParam } })
    const dados = extrairDados<ReavaliacaoDetalhe>(resposta)
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
    await useApi('/cambio-risco/reavaliacoes', {
      method: 'POST',
      body: { dataReavaliacao: form.dataReavaliacao, observacao: form.observacao, itens: [] }
    })
    toast.success('Reavaliação criada com sucesso!')
    router.push('/erp/financeiro/cambio-risco/reavaliacoes')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function acao(nome: 'aprovar' | 'cancelar' | 'contabilizar', titulo: string) {
  const ok = await confirmRef.value!.open(titulo, 'Confirma esta operação na reavaliação?', { danger: nome === 'cancelar' })
  if (!ok) return
  try {
    await useApi(`/cambio-risco/reavaliacoes/{id}/${nome}`, { method: 'POST', params: { id: idParam } })
    toast.success('Operação concluída.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

function cancelar() {
  router.push('/erp/financeiro/cambio-risco/reavaliacoes')
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Reavaliação cambial' : 'Nova reavaliação'" :loading="carregando || salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <template v-if="isEdit">
          <button type="button" class="btn btn-primary" @click="acao('aprovar', 'Aprovar reavaliação')">Aprovar</button>
          <button type="button" class="btn btn-primary" @click="acao('contabilizar', 'Contabilizar reavaliação')">Contabilizar</button>
          <button type="button" class="btn btn-danger" @click="acao('cancelar', 'Cancelar reavaliação')">Cancelar reavaliação</button>
        </template>
        <button v-else type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div v-if="isEdit && form.statusDescricao" class="status-linha">
        Status: <strong>{{ form.statusDescricao }}</strong>
      </div>
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <DateTimeField v-model="form.dataReavaliacao" label="Data da reavaliação" mode="datetime" required :disabled="isEdit" :error="erros.dataReavaliacao" />
          <TextField v-model="form.observacao" label="Observação" maxlength="200" :disabled="isEdit" />
        </div>
      </form>
      <p v-if="isEdit && form.dataReavaliacao" class="obs">
        Reavaliação de {{ formatarData(form.dataReavaliacao) }}. A edição de itens não é exposta pela API.
      </p>
    </div>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.status-linha { margin-bottom: 14px; color: var(--text-secondary); font-size: 13px; }
.obs { margin-top: 14px; color: var(--text-secondary); font-size: 12.5px; }
</style>
