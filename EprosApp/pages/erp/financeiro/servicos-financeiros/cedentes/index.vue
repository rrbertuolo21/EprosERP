<script setup lang="ts">
/**
 * Cedente (configuração de cobrança) — Serviços Financeiros.
 * A API expõe POST /servicos-financeiros/cedentes e PUT /{id}, mas NÃO há GET de listagem
 * nem GET/{id} no digest — portanto esta é uma tela de cadastro (criação). A edição por PUT
 * existe no backend, porém sem endpoint de leitura para pré-carregar (lacuna registrada).
 */
import { ref, reactive } from 'vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { onMounted } from 'vue'
import type { SelectOption } from '~/composables/useEnum'
import { carregarOpcoesDe } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface CedenteForm {
  empresaId: string | null
  nome: string | null
  email: string | null
  documento: string | null
  endereco: string | null
  numero: string | null
  bairro: string | null
  cidade: string | null
  cep: string | null
  uf: string | null
  logo: string | null
  receberAteDias: number | null
  diasAntecedencia: number | null
  multaAtraso: number | null
  juro: number | null
  instrucao1: string | null
  instrucao2: string | null
  instrucao3: string | null
  instrucao4: string | null
}

const ufsBrasil: SelectOption[] = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
].map((uf) => ({ label: uf, value: uf }))

const toast = useToast()
const salvando = ref(false)
const opcoesEmpresa = ref<SelectOption[]>([])
const form = reactive<CedenteForm>({
  empresaId: null,
  nome: null,
  email: null,
  documento: null,
  endereco: null,
  numero: null,
  bairro: null,
  cidade: null,
  cep: null,
  uf: null,
  logo: null,
  receberAteDias: 0,
  diasAntecedencia: 0,
  multaAtraso: 0,
  juro: 0,
  instrucao1: null,
  instrucao2: null,
  instrucao3: null,
  instrucao4: null
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.empresaId) erros.empresaId = 'Empresa é obrigatória.'
  if (!form.nome) erros.nome = 'Nome é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/servicos-financeiros/cedentes', { method: 'POST', body: { ...form } })
    toast.success('Cedente criado com sucesso!')
    form.nome = null
    form.email = null
    form.documento = null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

onMounted(async () => {
  opcoesEmpresa.value = await carregarOpcoesDe('/cadastros/empresas', ['razaoSocial', 'nomeFantasia', 'nome'])
})
</script>

<template>
  <div>
    <PageToolbar title="Cedente" subtitle="Configuração de cedente para cobrança" :loading="salvando">
      <template #actions>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <SelectField v-model="form.empresaId" label="Empresa" required :options="opcoesEmpresa" :error="erros.empresaId" />
          <TextField v-model="form.nome" label="Nome" required maxlength="120" :error="erros.nome" />
          <TextField v-model="form.documento" label="Documento" maxlength="20" />
          <TextField v-model="form.email" label="E-mail" type="email" maxlength="150" />
          <TextField v-model="form.endereco" label="Endereço" maxlength="120" />
          <TextField v-model="form.numero" label="Número" maxlength="10" />
          <TextField v-model="form.bairro" label="Bairro" maxlength="60" />
          <TextField v-model="form.cidade" label="Cidade" maxlength="60" />
          <SelectField v-model="form.uf" label="UF" :options="ufsBrasil" />
          <TextField v-model="form.cep" label="CEP" maxlength="9" />
          <TextField v-model="form.logo" label="Logo (URL)" maxlength="200" />
          <TextField
            :model-value="form.receberAteDias"
            label="Receber até (dias)"
            type="number"
            @update:model-value="(v) => (form.receberAteDias = v === '' ? null : Number(v))"
          />
          <TextField
            :model-value="form.diasAntecedencia"
            label="Dias de antecedência"
            type="number"
            @update:model-value="(v) => (form.diasAntecedencia = v === '' ? null : Number(v))"
          />
          <MoneyInput v-model="form.multaAtraso" label="Multa por atraso" />
          <MoneyInput v-model="form.juro" label="Juro" />
          <TextField v-model="form.instrucao1" label="Instrução 1" maxlength="120" />
          <TextField v-model="form.instrucao2" label="Instrução 2" maxlength="120" />
          <TextField v-model="form.instrucao3" label="Instrução 3" maxlength="120" />
          <TextField v-model="form.instrucao4" label="Instrução 4" maxlength="120" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
