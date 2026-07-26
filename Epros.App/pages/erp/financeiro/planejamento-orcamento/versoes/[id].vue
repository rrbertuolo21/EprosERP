<script setup lang="ts">
/**
 * Versão Orçamentária — novo (POST) e detalhe (GET /{id}) com ações.
 * POST /planejamento-orcamento/versoes · GET /{id} ·
 * POST /{id}/aprovar | /ativar | /encerrar · GET /{id}/previsto-realizado.
 * Observação: o POST aceita `linhas` (array de objetos) sem schema detalhado no digest;
 * a tela cria apenas o cabeçalho (linhas fica como lacuna).
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'

definePageMeta({ layout: 'default' })

interface VersaoDetalhe {
  id?: string
  nome: string | null
  periodoInicio: string | null
  periodoFim: string | null
  statusDescricao?: string | null
}
interface LinhaPrevistoRealizado {
  descricao?: string | null
  contaNome?: string | null
  previsto?: number | null
  realizado?: number | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { formatarMoeda } = useHelper()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const form = reactive<VersaoDetalhe>({ nome: null, periodoInicio: null, periodoFim: null })
const erros = reactive<Record<string, string>>({})
const previstoRealizado = ref<LinhaPrevistoRealizado[]>([])

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.nome) erros.nome = 'Nome é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi('/planejamento-orcamento/versoes/{id}', { params: { id: idParam } })
    const dados = extrairDados<VersaoDetalhe>(resposta)
    if (dados) Object.assign(form, dados)
    await carregarPrevistoRealizado()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function carregarPrevistoRealizado() {
  try {
    const resposta = await useApi('/planejamento-orcamento/versoes/{id}/previsto-realizado', { params: { id: idParam } })
    const dados = extrairDados<LinhaPrevistoRealizado[] | { itens?: LinhaPrevistoRealizado[] }>(resposta)
    previstoRealizado.value = Array.isArray(dados) ? dados : (dados?.itens ?? [])
  } catch (e) {
    console.error('[versoes/[id]] previsto-realizado', e)
  }
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/planejamento-orcamento/versoes', {
      method: 'POST',
      body: { nome: form.nome, periodoInicio: form.periodoInicio, periodoFim: form.periodoFim, linhas: [] }
    })
    toast.success('Versão criada com sucesso!')
    router.push('/erp/financeiro/planejamento-orcamento/versoes')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function acao(nome: 'aprovar' | 'ativar' | 'encerrar', titulo: string) {
  const ok = await confirmRef.value!.open(titulo, 'Confirma esta operação na versão orçamentária?', { danger: nome === 'encerrar' })
  if (!ok) return
  try {
    await useApi(`/planejamento-orcamento/versoes/{id}/${nome}`, { method: 'POST', params: { id: idParam } })
    toast.success('Operação concluída.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

function cancelar() {
  router.push('/erp/financeiro/planejamento-orcamento/versoes')
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Versão orçamentária' : 'Nova versão'" :loading="carregando || salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <template v-if="isEdit">
          <button type="button" class="btn btn-secondary" @click="acao('aprovar', 'Aprovar versão')">Aprovar</button>
          <button type="button" class="btn btn-secondary" @click="acao('ativar', 'Ativar versão')">Ativar</button>
          <button type="button" class="btn btn-danger" @click="acao('encerrar', 'Encerrar versão')">Encerrar</button>
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
          <TextField v-model="form.nome" label="Nome" required maxlength="120" :disabled="isEdit" :error="erros.nome" />
          <DateTimeField v-model="form.periodoInicio" label="Início do período" mode="datetime" :disabled="isEdit" />
          <DateTimeField v-model="form.periodoFim" label="Fim do período" mode="datetime" :disabled="isEdit" />
        </div>
      </form>
    </div>

    <div v-if="isEdit" class="glass-panel form-panel">
      <h3 class="secao-titulo">Previsto x Realizado</h3>
      <table class="admin-table">
        <thead><tr><th>Descrição</th><th class="td-right">Previsto</th><th class="td-right">Realizado</th></tr></thead>
        <tbody>
          <tr v-if="previstoRealizado.length === 0"><td colspan="3"><div class="table-empty">Sem dados.</div></td></tr>
          <tr v-for="(l, i) in previstoRealizado" v-else :key="i">
            <td>{{ l.descricao ?? l.contaNome }}</td>
            <td class="td-right">{{ l.previsto != null ? formatarMoeda(l.previsto) : '' }}</td>
            <td class="td-right">{{ l.realizado != null ? formatarMoeda(l.realizado) : '' }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.secao-titulo { font-size: 15px; margin-bottom: 14px; }
.status-linha { margin-bottom: 14px; color: var(--text-secondary); font-size: 13px; }
</style>
