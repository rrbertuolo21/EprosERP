<script setup lang="ts">
/**
 * Sacado — novo/edição — Serviços Financeiros.
 * POST /servicos-financeiros/sacados · PUT /{id}. Sem GET/{id}: edição via listagem.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'
import { carregarOpcoesDe } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface SacadoForm {
  id?: string
  pessoaId: string | null
  grupoRecorrenciaId: string | null
  nome: string | null
  documento: string | null
  rg: string | null
  inscricao: string | null
  endereco: string | null
  numero: string | null
  complemento: string | null
  bairro: string | null
  cidade: string | null
  uf: string | null
  cep: string | null
  telefone: string | null
  email: string | null
  observacao: string | null
  valor: number | null
}

const ufsBrasil: SelectOption[] = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
].map((uf) => ({ label: uf, value: uf }))

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const opcoesGrupo = ref<SelectOption[]>([])
const form = reactive<SacadoForm>({
  pessoaId: null,
  grupoRecorrenciaId: null,
  nome: null,
  documento: null,
  rg: null,
  inscricao: null,
  endereco: null,
  numero: null,
  complemento: null,
  bairro: null,
  cidade: null,
  uf: null,
  cep: null,
  telefone: null,
  email: null,
  observacao: null,
  valor: 0
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.nome) erros.nome = 'Nome é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi('/servicos-financeiros/sacados', { query: { pagina: 1, tamanhoPagina: 500 } })
    const bruto = extrairDados<unknown>(resposta)
    const itens = (Array.isArray(bruto) ? bruto : (bruto as { itens?: SacadoForm[] })?.itens) ?? []
    const encontrado = (itens as SacadoForm[]).find((s) => String(s.id) === idParam)
    if (encontrado) Object.assign(form, encontrado)
    else toast.error('Sacado não encontrado.')
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
    if (isEdit.value) {
      await useApi('/servicos-financeiros/sacados/{id}', { method: 'PUT', params: { id: idParam }, body: { id: idParam, ...form } })
    } else {
      await useApi('/servicos-financeiros/sacados', { method: 'POST', body: { ...form } })
    }
    toast.success('Registro salvo com sucesso!')
    router.push('/erp/financeiro/servicos-financeiros/sacados')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/servicos-financeiros/sacados')
}

onMounted(async () => {
  opcoesGrupo.value = await carregarOpcoesDe('/servicos-financeiros/grupos-recorrencia', ['descricao'])
  await carregar()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar sacado' : 'Novo sacado'" :loading="carregando">
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
          <TextField v-model="form.nome" label="Nome" required maxlength="120" :error="erros.nome" />
          <TextField v-model="form.documento" label="Documento (CPF/CNPJ)" maxlength="20" />
          <TextField v-model="form.rg" label="RG" maxlength="20" />
          <TextField v-model="form.inscricao" label="Inscrição" maxlength="30" />
          <SelectField v-model="form.grupoRecorrenciaId" label="Grupo de recorrência" :options="opcoesGrupo" />
          <!-- TODO: pessoaId é UUID; select opcional carregado de /cadastros/pessoas se necessário. -->
          <TextField v-model="form.pessoaId" label="ID da pessoa" hint="UUID (opcional)" />
          <TextField v-model="form.endereco" label="Endereço" maxlength="120" />
          <TextField v-model="form.numero" label="Número" maxlength="10" />
          <TextField v-model="form.complemento" label="Complemento" maxlength="60" />
          <TextField v-model="form.bairro" label="Bairro" maxlength="60" />
          <TextField v-model="form.cidade" label="Cidade" maxlength="60" />
          <SelectField v-model="form.uf" label="UF" :options="ufsBrasil" />
          <TextField v-model="form.cep" label="CEP" maxlength="9" />
          <TextField v-model="form.telefone" label="Telefone" maxlength="20" />
          <TextField v-model="form.email" label="E-mail" type="email" maxlength="150" />
          <MoneyInput v-model="form.valor" label="Valor" />
          <TextField v-model="form.observacao" label="Observação" maxlength="200" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
