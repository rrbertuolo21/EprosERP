<script setup lang="ts">
/**
 * Programa de Subsídio — novo/edição + saldo, utilizações, encerrar, prestação de contas.
 * POST /subsidios-fundos/programas · GET/PUT /{id} · POST /{id}/encerrar ·
 * POST /{id}/prestacao-contas · GET /{id}/saldo ·
 * GET/POST /{id}/utilizacoes · DELETE /utilizacoes/{utilizacaoId}.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'

definePageMeta({ layout: 'default' })

interface ProgramaForm {
  id?: string
  orgao: string | null
  valorTotal: number | null
  vigenciaInicio: string | null
  vigenciaFim: string | null
  statusDescricao?: string | null
}
interface Utilizacao {
  id: string
  tituloPagarId?: string | null
  valorElegivel?: number | null
  dataDescricao?: string | null
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
const form = reactive<ProgramaForm>({ orgao: null, valorTotal: null, vigenciaInicio: null, vigenciaFim: null })
const erros = reactive<Record<string, string>>({})

const saldo = ref<number | null>(null)
const utilizacoes = ref<Utilizacao[]>([])
const novaUtil = reactive<{ tituloPagarId: string | null; valorElegivel: number | null }>({ tituloPagarId: null, valorElegivel: null })
const salvandoUtil = ref(false)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.orgao) erros.orgao = 'Órgão é obrigatório.'
  if (form.valorTotal == null) erros.valorTotal = 'Valor total é obrigatório.'
  if (!form.vigenciaInicio) erros.vigenciaInicio = 'Início da vigência é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi('/subsidios-fundos/programas/{id}', { params: { id: idParam } })
    const dados = extrairDados<ProgramaForm>(resposta)
    if (dados) Object.assign(form, dados)
    await Promise.all([carregarSaldo(), carregarUtilizacoes()])
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function carregarSaldo() {
  try {
    const resposta = await useApi('/subsidios-fundos/programas/{id}/saldo', { params: { id: idParam } })
    const dados = extrairDados<{ saldo?: number } | number>(resposta)
    saldo.value = typeof dados === 'number' ? dados : (dados?.saldo ?? null)
  } catch (e) {
    console.error('[programas/[id]] saldo', e)
  }
}
async function carregarUtilizacoes() {
  try {
    const resposta = await useApi('/subsidios-fundos/programas/{id}/utilizacoes', { params: { id: idParam } })
    utilizacoes.value = extrairLista<Utilizacao>(resposta) ?? []
  } catch (e) {
    console.error('[programas/[id]] utilizacoes', e)
  }
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    if (isEdit.value) {
      await useApi('/subsidios-fundos/programas/{id}', { method: 'PUT', params: { id: idParam }, body: { id: idParam, ...form } })
    } else {
      await useApi('/subsidios-fundos/programas', {
        method: 'POST',
        body: { orgao: form.orgao, valorTotal: form.valorTotal, vigenciaInicio: form.vigenciaInicio, vigenciaFim: form.vigenciaFim }
      })
    }
    toast.success('Registro salvo com sucesso!')
    router.push('/erp/financeiro/subsidios-fundos/programas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function encerrar() {
  const ok = await confirmRef.value!.open('Encerrar programa', 'Confirma o encerramento deste programa?', { danger: true })
  if (!ok) return
  try {
    await useApi('/subsidios-fundos/programas/{id}/encerrar', { method: 'POST', params: { id: idParam } })
    toast.success('Programa encerrado.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

async function prestacaoContas() {
  const ok = await confirmRef.value!.open('Prestação de contas', 'Confirma a geração da prestação de contas?')
  if (!ok) return
  try {
    await useApi('/subsidios-fundos/programas/{id}/prestacao-contas', { method: 'POST', params: { id: idParam } })
    toast.success('Prestação de contas registrada.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

async function adicionarUtilizacao() {
  if (!novaUtil.tituloPagarId || novaUtil.valorElegivel == null) {
    toast.error('Informe o título a pagar e o valor elegível.')
    return
  }
  salvandoUtil.value = true
  try {
    await useApi('/subsidios-fundos/programas/{id}/utilizacoes', {
      method: 'POST',
      params: { id: idParam },
      body: { programaSubsidioId: idParam, tituloPagarId: novaUtil.tituloPagarId, valorElegivel: novaUtil.valorElegivel }
    })
    toast.success('Utilização registrada.')
    novaUtil.tituloPagarId = null
    novaUtil.valorElegivel = null
    await Promise.all([carregarUtilizacoes(), carregarSaldo()])
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoUtil.value = false
  }
}

async function removerUtilizacao(u: Utilizacao) {
  const ok = await confirmRef.value!.open('Remover utilização', 'Confirma a remoção desta utilização?', { danger: true })
  if (!ok) return
  try {
    await useApi('/subsidios-fundos/utilizacoes/{utilizacaoId}', { method: 'DELETE', params: { utilizacaoId: u.id } })
    toast.success('Utilização removida.')
    await Promise.all([carregarUtilizacoes(), carregarSaldo()])
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

function cancelar() {
  router.push('/erp/financeiro/subsidios-fundos/programas')
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Programa de subsídio' : 'Novo programa'" :loading="carregando || salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <template v-if="isEdit">
          <button type="button" class="btn btn-secondary" @click="prestacaoContas">Prestação de contas</button>
          <button type="button" class="btn btn-danger" @click="encerrar">Encerrar</button>
        </template>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div v-if="isEdit" class="status-linha">
        <span v-if="form.statusDescricao">Status: <strong>{{ form.statusDescricao }}</strong></span>
        <span v-if="saldo != null"> · Saldo: <strong>{{ formatarMoeda(saldo) }}</strong></span>
      </div>
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.orgao" label="Órgão" required maxlength="120" :error="erros.orgao" />
          <MoneyInput v-model="form.valorTotal" label="Valor total" :error="erros.valorTotal" />
          <DateTimeField v-model="form.vigenciaInicio" label="Início da vigência" mode="datetime" required :error="erros.vigenciaInicio" />
          <DateTimeField v-model="form.vigenciaFim" label="Fim da vigência" mode="datetime" />
        </div>
      </form>
    </div>

    <div v-if="isEdit" class="glass-panel form-panel">
      <h3 class="secao-titulo">Utilizações</h3>
      <div class="form-grid nova-linha">
        <TextField v-model="novaUtil.tituloPagarId" label="ID do título a pagar" hint="UUID (sem endpoint de listagem no digest)" />
        <MoneyInput v-model="novaUtil.valorElegivel" label="Valor elegível" />
        <div class="acao-linha">
          <button type="button" class="btn btn-secondary" :disabled="salvandoUtil" @click="adicionarUtilizacao">+ Adicionar utilização</button>
        </div>
      </div>
      <table class="admin-table mt">
        <thead><tr><th>Título a Pagar</th><th class="td-right">Valor Elegível</th><th class="td-actions">Ações</th></tr></thead>
        <tbody>
          <tr v-if="utilizacoes.length === 0"><td colspan="3"><div class="table-empty">Nenhuma utilização.</div></td></tr>
          <tr v-for="u in utilizacoes" v-else :key="u.id">
            <td>{{ u.tituloPagarId }}</td>
            <td class="td-right">{{ u.valorElegivel != null ? formatarMoeda(u.valorElegivel) : '' }}</td>
            <td class="td-actions">
              <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Remover" @click="removerUtilizacao(u)">Remover</button>
            </td>
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
.nova-linha { align-items: end; }
.acao-linha { display: flex; align-items: flex-end; }
.secao-titulo { font-size: 15px; margin-bottom: 14px; }
.status-linha { margin-bottom: 14px; color: var(--text-secondary); font-size: 13px; }
.mt { margin-top: 18px; width: 100%; }
</style>
