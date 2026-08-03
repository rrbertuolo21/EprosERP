<script setup lang="ts">
/**
 * Novo Cheque (Tesouraria).
 * A API expõe apenas POST /tesouraria/cheques (sem edição; a situação muda pela listagem).
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'
import { OPCOES_TIPO_CHEQUE, OPCOES_TIPO_PESSOA_CHEQUE, carregarOpcoesDe } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface ChequeForm {
  empresaId: string | null
  tipo: number | null
  tipoPessoa: number | null
  pessoaId: string | null
  emissao: string | null
  vencimento: string | null
  valor: number | null
  contaId: string | null
  caixaId: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const isEdit = computed(() => (route.params.id as string) !== 'novo')
const salvando = ref(false)
const opcoesEmpresa = ref<SelectOption[]>([])
const opcoesPessoa = ref<SelectOption[]>([])
const opcoesConta = ref<SelectOption[]>([])
const opcoesCaixa = ref<SelectOption[]>([])

const form = reactive<ChequeForm>({
  empresaId: null,
  tipo: 0,
  tipoPessoa: 1,
  pessoaId: null,
  emissao: null,
  vencimento: null,
  valor: null,
  contaId: null,
  caixaId: null
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (form.tipo == null) erros.tipo = 'Tipo é obrigatório.'
  if (form.tipoPessoa == null) erros.tipoPessoa = 'Tipo de pessoa é obrigatório.'
  if (!form.emissao) erros.emissao = 'Emissão é obrigatória.'
  if (!form.vencimento) erros.vencimento = 'Vencimento é obrigatório.'
  if (form.valor == null) erros.valor = 'Valor é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/tesouraria/cheques', { method: 'POST', body: { ...form } })
    toast.success('Cheque registrado com sucesso!')
    router.push('/erp/financeiro/tesouraria/cheques')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/tesouraria/cheques')
}

onMounted(async () => {
  if (isEdit.value) {
    toast.error('Edição de cheque não é suportada pela API.')
    router.replace('/erp/financeiro/tesouraria/cheques/novo')
  }
  const [empresas, pessoas, contas, caixas] = await Promise.all([
    carregarOpcoesDe('/cadastros/empresas', ['razaoSocial', 'nomeFantasia', 'nome']),
    carregarOpcoesDe('/cadastros/pessoas', ['nome', 'razaoSocial', 'nomeFantasia']),
    carregarOpcoesDe('/tesouraria/contas', ['nome', 'numeroConta']),
    carregarOpcoesDe('/tesouraria/caixas', ['descricao', 'nome', 'localNome'])
  ])
  opcoesEmpresa.value = empresas
  opcoesPessoa.value = pessoas
  opcoesConta.value = contas
  opcoesCaixa.value = caixas
})
</script>

<template>
  <div>
    <PageToolbar title="Novo cheque" :loading="salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <SelectField v-model="form.tipo" label="Tipo" required :options="OPCOES_TIPO_CHEQUE" :clearable="false" :error="erros.tipo" />
          <SelectField v-model="form.tipoPessoa" label="Tipo de pessoa" required :options="OPCOES_TIPO_PESSOA_CHEQUE" :clearable="false" :error="erros.tipoPessoa" />
          <SelectField v-model="form.empresaId" label="Empresa" :options="opcoesEmpresa" />
          <SelectField v-model="form.pessoaId" label="Pessoa" :options="opcoesPessoa" />
          <DateTimeField v-model="form.emissao" label="Emissão" mode="datetime" required :error="erros.emissao" />
          <DateTimeField v-model="form.vencimento" label="Vencimento" mode="datetime" required :error="erros.vencimento" />
          <MoneyInput v-model="form.valor" label="Valor" :error="erros.valor" />
          <SelectField v-model="form.contaId" label="Conta" :options="opcoesConta" />
          <SelectField v-model="form.caixaId" label="Caixa" :options="opcoesCaixa" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
