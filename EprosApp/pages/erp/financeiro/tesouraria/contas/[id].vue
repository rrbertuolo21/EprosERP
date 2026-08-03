<script setup lang="ts">
/**
 * Conta Financeira (Tesouraria) — novo/edição + saldo e transações.
 * POST /tesouraria/contas · PUT /{id} (sem GET/{id}: edição via listagem) ·
 * GET /{id}/saldo · GET/POST /{id}/transacoes.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { OPCOES_TIPO_TRANSACAO } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface ContaForm {
  id?: string
  nome: string | null
  numeroConta: string | null
  tipoContaId: string | null
  nota: string | null
  saldoAbertura: number | null
}
interface Transacao {
  id: string
  valor?: number | null
  tipo?: number | null
  subtipo?: string | null
  dataOperacao?: string | null
  nota?: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { formatarData, formatarMoeda } = useHelper()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const form = reactive<ContaForm>({ nome: null, numeroConta: null, tipoContaId: null, nota: null, saldoAbertura: null })
const erros = reactive<Record<string, string>>({})

const saldo = ref<number | null>(null)
const transacoes = ref<Transacao[]>([])
const novaTransacao = reactive<{ valor: number | null; tipo: number | null; subtipo: string | null; dataOperacao: string | null; nota: string | null }>({
  valor: null,
  tipo: 0,
  subtipo: null,
  dataOperacao: null,
  nota: null
})
const salvandoTransacao = ref(false)

function tipoLabel(v: unknown): string {
  return OPCOES_TIPO_TRANSACAO.find((o) => o.value === v)?.label ?? ''
}

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.nome) erros.nome = 'Nome é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi('/tesouraria/contas', { query: { pagina: 1, tamanhoPagina: 500 } })
    const bruto = extrairDados<unknown>(resposta)
    const itens = (Array.isArray(bruto) ? bruto : (bruto as { itens?: ContaForm[] })?.itens) ?? []
    const encontrada = (itens as ContaForm[]).find((c) => String(c.id) === idParam)
    if (encontrada) Object.assign(form, encontrada)
    await Promise.all([carregarSaldo(), carregarTransacoes()])
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function carregarSaldo() {
  try {
    const resposta = await useApi('/tesouraria/contas/{id}/saldo', { params: { id: idParam } })
    const dados = extrairDados<{ saldo?: number } | number>(resposta)
    saldo.value = typeof dados === 'number' ? dados : (dados?.saldo ?? null)
  } catch (e) {
    console.error('[contas/[id]] saldo', e)
  }
}
async function carregarTransacoes() {
  try {
    const resposta = await useApi('/tesouraria/contas/{id}/transacoes', { params: { id: idParam } })
    transacoes.value = extrairDados<Transacao[]>(resposta) ?? []
  } catch (e) {
    console.error('[contas/[id]] transacoes', e)
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
      await useApi('/tesouraria/contas/{id}', {
        method: 'PUT',
        params: { id: idParam },
        body: { id: idParam, nome: form.nome, numeroConta: form.numeroConta, tipoContaId: form.tipoContaId, nota: form.nota }
      })
    } else {
      await useApi('/tesouraria/contas', {
        method: 'POST',
        body: { nome: form.nome, numeroConta: form.numeroConta, tipoContaId: form.tipoContaId, nota: form.nota, saldoAbertura: form.saldoAbertura }
      })
    }
    toast.success('Registro salvo com sucesso!')
    router.push('/erp/financeiro/tesouraria/contas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function adicionarTransacao() {
  if (novaTransacao.valor == null || novaTransacao.tipo == null || !novaTransacao.dataOperacao) {
    toast.error('Preencha valor, tipo e data da operação.')
    return
  }
  salvandoTransacao.value = true
  try {
    await useApi('/tesouraria/contas/{id}/transacoes', {
      method: 'POST',
      params: { id: idParam },
      body: {
        contaFinanceiraId: idParam,
        valor: novaTransacao.valor,
        tipo: novaTransacao.tipo,
        subtipo: novaTransacao.subtipo,
        dataOperacao: novaTransacao.dataOperacao,
        nota: novaTransacao.nota
      }
    })
    toast.success('Transação registrada.')
    novaTransacao.valor = null
    novaTransacao.tipo = 0
    novaTransacao.subtipo = null
    novaTransacao.dataOperacao = null
    novaTransacao.nota = null
    await Promise.all([carregarTransacoes(), carregarSaldo()])
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoTransacao.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/tesouraria/contas')
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar conta financeira' : 'Nova conta financeira'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div v-if="isEdit && saldo != null" class="status-linha">Saldo atual: <strong>{{ formatarMoeda(saldo) }}</strong></div>
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.nome" label="Nome" required maxlength="120" :error="erros.nome" />
          <TextField v-model="form.numeroConta" label="Número da conta" maxlength="30" />
          <!-- TODO: tipoContaId é UUID sem endpoint de listagem no digest. -->
          <TextField v-model="form.tipoContaId" label="ID do tipo de conta" hint="UUID" />
          <TextField v-model="form.nota" label="Nota" maxlength="200" />
          <MoneyInput v-if="!isEdit" v-model="form.saldoAbertura" label="Saldo de abertura" />
        </div>
      </form>
    </div>

    <div v-if="isEdit" class="glass-panel form-panel">
      <h3 class="secao-titulo">Transações</h3>
      <div class="form-grid nova-linha">
        <MoneyInput v-model="novaTransacao.valor" label="Valor" />
        <SelectField v-model="novaTransacao.tipo" label="Tipo" :options="OPCOES_TIPO_TRANSACAO" :clearable="false" />
        <TextField v-model="novaTransacao.subtipo" label="Subtipo" maxlength="60" />
        <DateTimeField v-model="novaTransacao.dataOperacao" label="Data da operação" mode="datetime" />
        <TextField v-model="novaTransacao.nota" label="Nota" maxlength="200" />
        <div class="acao-linha">
          <button type="button" class="btn btn-secondary" :disabled="salvandoTransacao" @click="adicionarTransacao">+ Adicionar</button>
        </div>
      </div>
      <table class="admin-table mt">
        <thead><tr><th>Data</th><th>Tipo</th><th>Subtipo</th><th class="td-right">Valor</th><th>Nota</th></tr></thead>
        <tbody>
          <tr v-if="transacoes.length === 0"><td colspan="5"><div class="table-empty">Nenhuma transação.</div></td></tr>
          <tr v-for="t in transacoes" v-else :key="t.id">
            <td>{{ t.dataOperacao ? formatarData(t.dataOperacao) : '' }}</td>
            <td>{{ tipoLabel(t.tipo) }}</td>
            <td>{{ t.subtipo }}</td>
            <td class="td-right">{{ formatarMoeda(t.valor as number) }}</td>
            <td>{{ t.nota }}</td>
          </tr>
        </tbody>
      </table>
    </div>
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
