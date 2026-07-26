<script setup lang="ts">
/**
 * Registrar Saldo de Abertura — Contabilidade Geral / Saldos de Abertura.
 *
 * Contrato: POST /contabilidade-geral/saldos-abertura
 * Body: numero?, data, contaContabilId, codigoConta?, tipoSaldo (enum), valor, historico?.
 * A API expõe SOMENTE a criação (não há lista/GET/PUT/DELETE), então esta é uma tela
 * de registro. Após salvar, o formulário é limpo para novo lançamento.
 */
import { computed, reactive, ref, onMounted, watch } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import type { SelectOption } from '~/composables/useEnum'
import { tiposSaldoContabil } from '~/components/contabilidade-contas/enums'

definePageMeta({ layout: 'default' })

interface SaldoAberturaForm {
  numero: string
  data: string
  contaContabilId: string | null
  codigoConta: string
  tipoSaldo: number
  valor: number
  historico: string
}

interface ContaOpcao { id: string; codigoConta?: string | null; nomeConta?: string | null }

const toast = useToast()

const salvando = ref(false)
const contas = ref<ContaOpcao[]>([])

const opcoesConta = computed<SelectOption[]>(() =>
  contas.value.map((c) => ({ label: `${c.codigoConta ?? ''} — ${c.nomeConta ?? ''}`.trim(), value: c.id }))
)

function estadoInicial(): SaldoAberturaForm {
  return {
    numero: '',
    data: new Date().toISOString().slice(0, 10),
    contaContabilId: null,
    codigoConta: '',
    tipoSaldo: 0,
    valor: 0,
    historico: ''
  }
}

const form = reactive<SaldoAberturaForm>(estadoInicial())
const erros = reactive<Record<string, string>>({})

// Autopreenche o código da conta ao escolher a conta contábil.
watch(
  () => form.contaContabilId,
  (id) => {
    const conta = contas.value.find((c) => c.id === id)
    if (conta?.codigoConta) form.codigoConta = conta.codigoConta
  }
)

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()
  if (!form.data) erros.data = 'Data é obrigatória.'
  if (!form.contaContabilId) erros.contaContabilId = 'Conta contábil é obrigatória.'
  if (form.tipoSaldo == null) erros.tipoSaldo = 'Tipo de saldo é obrigatório.'
  if (!form.valor || form.valor <= 0) erros.valor = 'Valor deve ser maior que zero.'
  return Object.keys(erros).length === 0
}

async function carregarContas() {
  try {
    const resposta = await useApi('/contabilidade-geral/contas', { query: { tamanhoPagina: 100 } })
    const dados = extrairDados<{ itens?: ContaOpcao[] } | ContaOpcao[]>(resposta)
    contas.value = Array.isArray(dados) ? dados : dados?.itens ?? []
  } catch (e) {
    console.error('[contabilidade/saldos-abertura] contas', e)
  }
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/contabilidade-geral/saldos-abertura', {
      method: 'POST',
      body: {
        numero: form.numero || null,
        data: form.data,
        contaContabilId: form.contaContabilId,
        codigoConta: form.codigoConta || null,
        tipoSaldo: form.tipoSaldo,
        valor: form.valor,
        historico: form.historico || null
      }
    })
    toast.success('Saldo de abertura registrado com sucesso!')
    Object.assign(form, estadoInicial())
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function limpar() {
  Object.assign(form, estadoInicial())
  limparErros()
}

onMounted(carregarContas)
</script>

<template>
  <div>
    <PageToolbar
      title="Saldos de Abertura"
      subtitle="Registro dos saldos iniciais das contas contábeis (débito/crédito)"
      :loading="salvando"
    >
      <template #actions>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="limpar">Limpar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Registrar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div class="form-grid">
        <TextField v-model="form.numero" label="Número" maxlength="30" />
        <DateTimeField v-model="form.data" label="Data" required :error="erros.data" />
        <SelectField v-model="form.contaContabilId" label="Conta Contábil" required :options="opcoesConta" :error="erros.contaContabilId" />
        <TextField v-model="form.codigoConta" label="Código da Conta" maxlength="30" hint="Preenchido ao selecionar a conta" />
        <SelectField v-model="form.tipoSaldo" label="Tipo de Saldo" required :options="tiposSaldoContabil" :clearable="false" :error="erros.tipoSaldo" />
        <MoneyInput v-model="form.valor" label="Valor" required :error="erros.valor" />
        <TextField v-model="form.historico" label="Histórico" maxlength="200" />
      </div>
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
</style>
