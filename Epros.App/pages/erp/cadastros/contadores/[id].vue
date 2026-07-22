<script setup lang="ts">
/**
 * Formulário de Contador (novo/edição) — Cadastros / Contadores.
 *
 * Porta o comportamento de `cadastros/contador/[id].vue` do legado:
 * abas Contador/Endereço, autopreenchimento por CEP, validações de CPF/CNPJ,
 * seleção de UF/Município e permissão de transmissão.
 */
import { ref, computed, reactive, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useMask } from '~/composables/useMask'
import { useDocumento } from '~/composables/useDocumento'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface EnderecoContador {
  paisId: number
  municipioId: number | null
  tipoEndereco: number
  cep: string
  uf: string | null
  logradouro: string
  numero: string
  complemento: string | null
  bairro: string
  referencia: string | null
}

interface ContadorForm {
  id?: number
  razaoSocial: string | null
  nomeContador: string | null
  cpf: string
  cnpj: string | null
  numeroCrc: string
  ufCrc: string
  dataVencimentoCrc: string | null
  qualificacao: string | null
  funcao: string | null
  telefone: string | null
  email: string | null
  permissaoTransmissao: number | null
  ativo: boolean
  endereco: EnderecoContador
}

interface Municipio {
  id: number
  nome: string
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { maskCPF, maskCEP, maskTelefone, maskCpfCnpj, somenteDigitos } = useMask()
const { validarCPF, validarCNPJ, validarEmail, validarTelefone, validarCEP } = useDocumento()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')
const contadorId = computed(() => (isEdit.value ? Number(idParam) : undefined))

const carregando = ref(false)
const salvando = ref(false)
const abaAtiva = ref<'contador' | 'endereco'>('contador')
const municipios = ref<Municipio[]>([])
const opcoesPermissaoTransmissao = ref<SelectOption[]>([])
const carregandoPermissoes = ref(false)

const ufsBrasil: SelectOption[] = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
].map((uf) => ({ label: uf, value: uf }))

const form = reactive<ContadorForm>({
  id: contadorId.value,
  razaoSocial: null,
  nomeContador: null,
  cpf: '',
  cnpj: null,
  numeroCrc: '',
  ufCrc: '',
  dataVencimentoCrc: null,
  qualificacao: null,
  funcao: null,
  telefone: null,
  email: null,
  permissaoTransmissao: null,
  ativo: true,
  endereco: {
    paisId: 1058,
    municipioId: null,
    tipoEndereco: 1,
    cep: '',
    uf: null,
    logradouro: '',
    numero: '',
    complemento: null,
    bairro: '',
    referencia: null
  }
})

const erros = reactive<Record<string, string>>({})

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()

  if (!form.cpf) erros.cpf = 'CPF é obrigatório.'
  else if (!validarCPF(form.cpf)) erros.cpf = 'CPF inválido.'

  if (form.cnpj && !validarCNPJ(form.cnpj)) erros.cnpj = 'CNPJ inválido.'

  if (!form.nomeContador && !form.razaoSocial) {
    erros.nomeContador = 'Preencha Razão Social ou Nome do Contador.'
  }

  if (form.email && !validarEmail(form.email)) erros.email = 'E-mail inválido.'
  if (form.telefone && !validarTelefone(form.telefone)) erros.telefone = 'Telefone inválido.'

  if (!form.numeroCrc) erros.numeroCrc = 'Número CRC é obrigatório.'
  if (!form.ufCrc) erros.ufCrc = 'UF CRC é obrigatória.'
  if (!form.dataVencimentoCrc) erros.dataVencimentoCrc = 'Data de vencimento do CRC é obrigatória.'
  if (form.permissaoTransmissao == null) erros.permissaoTransmissao = 'Permissão de transmissão é obrigatória.'

  if (!form.endereco.cep) erros.cep = 'CEP é obrigatório.'
  else if (!validarCEP(form.endereco.cep)) erros.cep = 'CEP inválido.'
  if (!form.endereco.logradouro) erros.logradouro = 'Logradouro é obrigatório.'
  if (!form.endereco.numero) erros.numero = 'Número é obrigatório.'
  if (!form.endereco.bairro) erros.bairro = 'Bairro é obrigatório.'
  if (!form.endereco.uf) erros.ufEndereco = 'UF é obrigatória.'
  if (!form.endereco.municipioId) erros.municipioId = 'Município é obrigatório.'

  return Object.keys(erros).length === 0
}

async function carregarPermissoesTransmissao() {
  carregandoPermissoes.value = true
  try {
    const resposta = await useApi('/contadores/permissoes-transmissao')
    const lista = extrairDados<Array<{ id: number; descricaoFormatada?: string; descricao?: string }>>(resposta) ?? []
    opcoesPermissaoTransmissao.value = lista.map((p) => ({
      label: p.descricaoFormatada ?? p.descricao ?? String(p.id),
      value: p.id
    }))
  } catch (e) {
    console.error('[contadores/[id]] permissoes-transmissao', e)
    opcoesPermissaoTransmissao.value = []
  } finally {
    carregandoPermissoes.value = false
  }
}

async function carregarContador() {
  if (!isEdit.value || !contadorId.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/contadores/${contadorId.value}`)
    const dados = extrairDados<Partial<ContadorForm>>(resposta)
    if (dados) {
      Object.assign(form, dados)
      if (dados.endereco) {
        Object.assign(form.endereco, dados.endereco)
      }
      if (form.endereco.uf) {
        await carregarMunicipiosPorUf(form.endereco.uf)
      }
    }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function carregarMunicipiosPorUf(uf: string) {
  if (!uf) {
    municipios.value = []
    return
  }
  try {
    const resposta = await useApi(`/cadastros/geografia/municipios/obter-por-uf/${uf}`)
    municipios.value = extrairDados<Municipio[]>(resposta) ?? (Array.isArray(resposta) ? (resposta as Municipio[]) : [])
  } catch (e) {
    console.error('[contadores/[id]] municipios-por-uf', e)
    municipios.value = []
  }
}

let ufAnterior = ''
watch(
  () => form.endereco.uf,
  async (uf) => {
    if (!uf || uf === ufAnterior) return
    ufAnterior = uf
    await carregarMunicipiosPorUf(uf)
  }
)

let cepAnterior = ''
watch(
  () => form.endereco.cep,
  async (cep) => {
    const limpo = somenteDigitos(cep ?? '')
    if (limpo === cepAnterior || limpo.length !== 8) return
    cepAnterior = limpo
    try {
      const resposta = await useApi(`/cadastros/geografia/cep/${limpo}`)
      const dados = extrairDados<{
        logradouro?: string
        bairro?: string
        uf?: string
        municipio?: string
      }>(resposta)
      if (dados) {
        form.endereco.logradouro = dados.logradouro ?? form.endereco.logradouro
        form.endereco.bairro = dados.bairro ?? form.endereco.bairro
        if (dados.uf) form.endereco.uf = dados.uf
      }
    } catch (e) {
      console.error('[contadores/[id]] cep', e)
    }
  }
)

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }

  salvando.value = true
  try {
    if (isEdit.value) {
      await useApi(`/contadores/${contadorId.value}`, { method: 'PUT', body: form })
    } else {
      await useApi('/contadores', { method: 'POST', body: form })
    }
    toast.success('Registro salvo com sucesso!')
    router.push('/erp/cadastros/contadores')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/cadastros/contadores')
}

onMounted(async () => {
  await carregarPermissoesTransmissao()
  await carregarContador()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar contador' : 'Novo contador'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <nav class="form-tabs">
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'contador' }" @click="abaAtiva = 'contador'">Contador</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'endereco' }" @click="abaAtiva = 'endereco'">Endereço</button>
      </nav>

      <form class="vertical-form mt-4" @submit.prevent="salvar">
        <div v-show="abaAtiva === 'contador'" class="form-tab-content">
          <div class="form-grid">
            <TextField
              v-model="form.cpf"
              label="CPF"
              required
              :error="erros.cpf"
              @update:model-value="(v) => (form.cpf = maskCPF(v as string))"
            />
            <TextField v-model="form.nomeContador" label="Nome do Contador" maxlength="100" :error="erros.nomeContador" />
            <TextField
              v-model="form.cnpj"
              label="CNPJ"
              :error="erros.cnpj"
              @update:model-value="(v) => (form.cnpj = maskCpfCnpj(v as string))"
            />
            <TextField v-model="form.razaoSocial" label="Razão Social" maxlength="100" />
            <TextField
              v-model="form.telefone"
              label="Telefone"
              :error="erros.telefone"
              @update:model-value="(v) => (form.telefone = maskTelefone(v as string))"
            />
            <TextField v-model="form.email" label="E-mail" type="email" maxlength="150" :error="erros.email" />
            <TextField v-model="form.numeroCrc" label="Número CRC" required maxlength="15" :error="erros.numeroCrc" />
            <SelectField v-model="form.ufCrc" label="UF CRC" required :options="ufsBrasil" :error="erros.ufCrc" />
            <DateTimeField v-model="form.dataVencimentoCrc" label="Data Vencimento CRC" required :error="erros.dataVencimentoCrc" />
            <SelectField
              v-model="form.permissaoTransmissao"
              label="Permissão de Transmissão"
              required
              :options="opcoesPermissaoTransmissao"
              :error="erros.permissaoTransmissao"
              :disabled="carregandoPermissoes"
            />
            <TextField v-model="form.qualificacao" label="Qualificação" maxlength="60" />
            <TextField v-model="form.funcao" label="Função" maxlength="60" />
            <label class="field toggle-row">
              <span class="field-label">{{ form.ativo ? 'Ativo' : 'Inativo' }}</span>
              <input v-model="form.ativo" type="checkbox" />
            </label>
          </div>
        </div>

        <div v-show="abaAtiva === 'endereco'" class="form-tab-content">
          <div class="form-grid">
            <TextField
              v-model="form.endereco.cep"
              label="CEP"
              required
              :error="erros.cep"
              @update:model-value="(v) => (form.endereco.cep = maskCEP(v as string))"
            />
            <TextField v-model="form.endereco.logradouro" label="Logradouro" required :error="erros.logradouro" />
            <TextField v-model="form.endereco.numero" label="Número" required :error="erros.numero" />
            <TextField v-model="form.endereco.complemento" label="Complemento" />
            <TextField v-model="form.endereco.referencia" label="Referência" />
            <TextField v-model="form.endereco.bairro" label="Bairro" required :error="erros.bairro" />
            <SelectField v-model="form.endereco.uf" label="UF" required :options="ufsBrasil" :error="erros.ufEndereco" />
            <SelectField
              v-model="form.endereco.municipioId"
              label="Município"
              required
              :options="municipios.map((m) => ({ label: m.nome, value: m.id }))"
              :placeholder="form.endereco.uf ? 'Selecione...' : 'Selecione a UF primeiro'"
              :error="erros.municipioId"
            />
          </div>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-tabs { display: flex; gap: 8px; border-bottom: 1px solid var(--border-color); padding-bottom: 12px; }
.tab-link {
  background: none;
  border: none;
  color: var(--text-secondary);
  padding: 8px 16px;
  font-size: 13.5px;
  font-weight: 600;
  cursor: pointer;
  border-radius: 6px;
  transition: all 0.2s ease;
}
.tab-link:hover, .tab-link.active {
  background: rgba(255, 255, 255, 0.03);
  color: var(--text-primary);
}
.tab-link.active {
  box-shadow: 0 0 10px rgba(99, 102, 241, 0.15);
  border: 1px solid rgba(99, 102, 241, 0.2);
}
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
}
.toggle-row {
  display: flex;
  align-items: center;
  gap: 10px;
  justify-content: flex-start;
}
.toggle-row input {
  width: 18px;
  height: 18px;
  accent-color: var(--primary);
}
.mt-4 { margin-top: 20px; }
</style>
