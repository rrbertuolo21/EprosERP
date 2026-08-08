<script setup lang="ts">
/**
 * Formulário de Conta Bancária (novo/edição) — Financeiro / Cadastros auxiliares.
 *
 * Porta o comportamento de `financeiro/conta-bancaria/[id].vue` do legado:
 * seleção de banco, tipo de conta, dados de titular/agência/conta, telefone do gerente
 * com máscara e data de encerramento.
 */
import { ref, computed, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useMask } from '~/composables/useMask'
import { useTenant } from '~/composables/useTenant'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface Banco {
  id: string
  codigo: string
  descricao: string
}

interface ContaBancariaForm {
  id: string | null
  empresaId: string
  bancoId: string | null
  tipoContaBancaria: number | null
  apelido: string
  titular: string
  agencia: string
  digitoAgencia: string | null
  conta: string
  gerente: string | null
  foneGerente: string | null
  detalhe: string | null
  dataEncerramento: string | null
}

const TIPOS_CONTA_OPCOES: SelectOption[] = [
  { label: 'Conta Corrente', value: 1 },
  { label: 'Conta Poupança', value: 2 },
  { label: 'Aplicações', value: 3 },
  { label: 'Outras', value: 4 }
]

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { maskTelefone } = useMask()
const { empresaId } = useTenant()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const bancos = ref<Banco[]>([])
const opcoesBancos = computed<SelectOption[]>(() =>
  bancos.value.map((b) => ({ label: `${b.codigo} - ${b.descricao}`, value: b.id }))
)

const form = reactive<ContaBancariaForm>({
  id: isEdit.value ? idParam : null,
  empresaId: String(empresaId.value || ''),
  bancoId: null,
  tipoContaBancaria: null,
  apelido: '',
  titular: '',
  agencia: '',
  digitoAgencia: null,
  conta: '',
  gerente: null,
  foneGerente: null,
  detalhe: null,
  dataEncerramento: null
})

const erros = reactive<Record<string, string>>({})

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()
  if (!form.bancoId) erros.bancoId = 'Banco é obrigatório.'
  if (!form.tipoContaBancaria) erros.tipoContaBancaria = 'Tipo de conta é obrigatório.'
  if (!form.apelido) erros.apelido = 'Apelido é obrigatório.'
  if (!form.titular) erros.titular = 'Titular é obrigatório.'
  if (!form.agencia) erros.agencia = 'Agência é obrigatória.'
  if (!form.conta) erros.conta = 'Conta é obrigatória.'
  return Object.keys(erros).length === 0
}

async function carregarBancos() {
  try {
    const resposta = await useApi('/bancos', { query: { tamanhoPagina: 200 } })
    bancos.value = extrairLista<Banco>(resposta) ?? []
  } catch (e) {
    console.error('[conta-bancaria/[id]] bancos', e)
    bancos.value = []
  }
}

async function carregarConta() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/contas-bancarias/{id}`, { params: { id: idParam } })
    const dados = extrairDados<Partial<ContaBancariaForm>>(resposta)
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
    const payload = {
      empresaId: form.empresaId,
      bancoId: form.bancoId,
      tipoContaBancaria: form.tipoContaBancaria,
      apelido: form.apelido,
      titular: form.titular,
      agencia: form.agencia,
      digitoAgencia: form.digitoAgencia,
      conta: form.conta,
      gerente: form.gerente,
      foneGerente: form.foneGerente,
      detalhe: form.detalhe,
      dataEncerramento: form.dataEncerramento
    }

    if (isEdit.value) {
      await useApi('/contas-bancarias/{id}', { method: 'PUT', params: { id: idParam }, body: { id: idParam, ...payload } })
    } else {
      await useApi('/contas-bancarias', { method: 'POST', body: payload })
    }
    toast.success('Conta bancária salva com sucesso!')
    router.push('/erp/financeiro/conta-bancaria')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/conta-bancaria')
}

onMounted(async () => {
  await carregarBancos()
  await carregarConta()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar conta bancária' : 'Nova conta bancária'" :loading="carregando">
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
          <SelectField v-model="form.bancoId" label="Banco" required :options="opcoesBancos" :error="erros.bancoId" />
          <SelectField v-model="form.tipoContaBancaria" label="Tipo de Conta" required :options="TIPOS_CONTA_OPCOES" :error="erros.tipoContaBancaria" />
          <TextField v-model="form.apelido" label="Apelido" required maxlength="100" :error="erros.apelido" />
          <TextField v-model="form.titular" label="Titular" required maxlength="150" :error="erros.titular" />
          <TextField v-model="form.gerente" label="Gerente" maxlength="100" />
          <TextField
            v-model="form.foneGerente"
            label="Telefone do Gerente"
            @update:model-value="(v) => (form.foneGerente = maskTelefone(v as string))"
          />
          <TextField v-model="form.agencia" label="Agência" required maxlength="20" :error="erros.agencia" />
          <TextField v-model="form.digitoAgencia" label="Dígito" maxlength="2" />
          <TextField v-model="form.conta" label="Conta" required maxlength="30" :error="erros.conta" />
          <DateTimeField v-model="form.dataEncerramento" label="Data de Encerramento" />
        </div>
        <div class="form-grid form-grid-full">
          <TextField v-model="form.detalhe" label="Detalhes" maxlength="255" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
}
.form-grid-full { grid-template-columns: 1fr; margin-top: 16px; }
</style>
