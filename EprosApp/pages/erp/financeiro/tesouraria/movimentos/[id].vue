<script setup lang="ts">
/**
 * Novo Movimento Financeiro (Tesouraria).
 * A API expõe apenas POST /tesouraria/movimentos (sem edição).
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'
import { carregarOpcoesDe } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface MovimentoForm {
  empresaId: string | null
  emissao: string | null
  caixaId: string | null
  contaId: string | null
  credito: number | null
  debito: number | null
  chequeId: string | null
  contasPagarId: string | null
  contasReceberId: string | null
  pagamentoId: string | null
  planejamento: boolean
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const isEdit = computed(() => (route.params.id as string) !== 'novo')
const salvando = ref(false)
const opcoesEmpresa = ref<SelectOption[]>([])
const opcoesConta = ref<SelectOption[]>([])
const opcoesCaixa = ref<SelectOption[]>([])

const form = reactive<MovimentoForm>({
  empresaId: null,
  emissao: null,
  caixaId: null,
  contaId: null,
  credito: 0,
  debito: 0,
  chequeId: null,
  contasPagarId: null,
  contasReceberId: null,
  pagamentoId: null,
  planejamento: false
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.emissao) erros.emissao = 'Emissão é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/tesouraria/movimentos', { method: 'POST', body: { ...form } })
    toast.success('Movimento registrado com sucesso!')
    router.push('/erp/financeiro/tesouraria/movimentos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/tesouraria/movimentos')
}

onMounted(async () => {
  if (isEdit.value) {
    toast.error('Edição de movimento não é suportada pela API.')
    router.replace('/erp/financeiro/tesouraria/movimentos/novo')
  }
  const [empresas, contas, caixas] = await Promise.all([
    carregarOpcoesDe('/cadastros/empresas', ['razaoSocial', 'nomeFantasia', 'nome']),
    carregarOpcoesDe('/tesouraria/contas', ['nome', 'numeroConta']),
    carregarOpcoesDe('/tesouraria/caixas', ['localNome', 'usuarioNome'])
  ])
  opcoesEmpresa.value = empresas
  opcoesConta.value = contas
  opcoesCaixa.value = caixas
})
</script>

<template>
  <div>
    <PageToolbar title="Novo movimento financeiro" :loading="salvando">
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
          <DateTimeField v-model="form.emissao" label="Emissão" mode="datetime" required :error="erros.emissao" />
          <SelectField v-model="form.empresaId" label="Empresa" :options="opcoesEmpresa" />
          <SelectField v-model="form.contaId" label="Conta" :options="opcoesConta" />
          <SelectField v-model="form.caixaId" label="Caixa" :options="opcoesCaixa" />
          <MoneyInput v-model="form.credito" label="Crédito" />
          <MoneyInput v-model="form.debito" label="Débito" />
          <!-- TODO: chequeId/contasPagarId/contasReceberId/pagamentoId são UUID sem endpoint de listagem próprio no digest. -->
          <TextField v-model="form.chequeId" label="ID do cheque" hint="UUID" />
          <TextField v-model="form.contasPagarId" label="ID conta a pagar" hint="UUID" />
          <TextField v-model="form.contasReceberId" label="ID conta a receber" hint="UUID" />
          <TextField v-model="form.pagamentoId" label="ID do pagamento" hint="UUID" />
          <label class="field toggle-row">
            <span class="field-label">{{ form.planejamento ? 'Planejamento' : 'Efetivo' }}</span>
            <input v-model="form.planejamento" type="checkbox" />
          </label>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.toggle-row { display: flex; align-items: center; gap: 10px; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
</style>
