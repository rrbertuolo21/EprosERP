<script setup lang="ts">
/**
 * Formulário de Imóvel — Imobiliária / Imóveis.
 *
 * Novo: POST /imobiliaria/imoveis (CriarImovelCommand). Campos do corpo:
 *   descricao, municipioId, cep, logradouro, numero, complemento, bairro,
 *   proprietarios[] { pessoaId }, vistorias[] { local, descricao, dataVistoria },
 *   custos[] { descricao, valor, competencia }.
 *
 * A API NÃO expõe atualização (não há PUT). Para um imóvel existente carregamos
 * GET /imobiliaria/imoveis/{id} e exibimos em modo somente leitura.
 *
 * O campo `uf` é apenas auxiliar de UI para filtrar os municípios (não é enviado);
 * o corpo do POST guarda somente `municipioId`.
 */
import { ref, computed, reactive, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useMask } from '~/composables/useMask'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface ProprietarioInput {
  pessoaId: string | null
}
interface VistoriaInput {
  local: string
  descricao: string
  dataVistoria: string | null
}
interface CustoImovelInput {
  descricao: string
  valor: number | null
  competencia: string | null
}
interface ImovelForm {
  descricao: string
  municipioId: string | null
  cep: string
  logradouro: string
  numero: string
  complemento: string
  bairro: string
  proprietarios: ProprietarioInput[]
  vistorias: VistoriaInput[]
  custos: CustoImovelInput[]
}

interface Pessoa {
  id: string
  nome?: string | null
  razaoSocial?: string | null
}
interface Municipio {
  id: string
  nome: string
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { maskCEP, somenteDigitos } = useMask()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const abaAtiva = ref<'dados' | 'proprietarios' | 'vistorias' | 'custos'>('dados')

const pessoas = ref<Pessoa[]>([])
const municipios = ref<Municipio[]>([])
// Auxiliar de UI: seleciona a UF para carregar os municípios (não integra o corpo do POST).
const uf = ref<string | null>(null)

const ufsBrasil: SelectOption[] = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
].map((u) => ({ label: u, value: u }))

const form = reactive<ImovelForm>({
  descricao: '',
  municipioId: null,
  cep: '',
  logradouro: '',
  numero: '',
  complemento: '',
  bairro: '',
  proprietarios: [{ pessoaId: null }],
  vistorias: [],
  custos: []
})

const erros = reactive<Record<string, string>>({})

const opcoesPessoas = computed<SelectOption[]>(() =>
  pessoas.value.map((p) => ({ label: p.razaoSocial ?? p.nome ?? p.id, value: p.id }))
)
const opcoesMunicipios = computed<SelectOption[]>(() =>
  municipios.value.map((m) => ({ label: m.nome, value: m.id }))
)

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()
  if (!form.descricao) erros.descricao = 'A descrição do imóvel é obrigatória.'
  const proprietariosValidos = form.proprietarios.filter((p) => p.pessoaId)
  if (proprietariosValidos.length === 0) erros.proprietarios = 'O imóvel exige ao menos um proprietário.'
  return Object.keys(erros).length === 0
}

// ---- Proprietários ----
function adicionarProprietario() {
  form.proprietarios.push({ pessoaId: null })
}
function removerProprietario(i: number) {
  form.proprietarios.splice(i, 1)
}

// ---- Vistorias ----
function adicionarVistoria() {
  form.vistorias.push({ local: '', descricao: '', dataVistoria: null })
}
function removerVistoria(i: number) {
  form.vistorias.splice(i, 1)
}

// ---- Custos ----
function adicionarCusto() {
  form.custos.push({ descricao: '', valor: 0, competencia: null })
}
function removerCusto(i: number) {
  form.custos.splice(i, 1)
}

async function carregarPessoas() {
  try {
    const resposta = await useApi('/cadastros/pessoas', { query: { tamanhoPagina: 500 } })
    pessoas.value = extrairLista<Pessoa>(resposta) ?? []
  } catch (e) {
    console.error('[imoveis/[id]] pessoas', e)
    pessoas.value = []
  }
}

async function carregarMunicipiosPorUf(sigla: string) {
  if (!sigla) {
    municipios.value = []
    return
  }
  try {
    const resposta = await useApi(`/cadastros/geografia/municipios/obter-por-uf/${sigla}`)
    municipios.value = extrairLista<Municipio>(resposta) ?? (Array.isArray(resposta) ? (resposta as Municipio[]) : [])
  } catch (e) {
    console.error('[imoveis/[id]] municipios', e)
    municipios.value = []
  }
}

let ufAnterior = ''
watch(uf, async (novaUf) => {
  if (!novaUf || novaUf === ufAnterior) return
  ufAnterior = novaUf
  await carregarMunicipiosPorUf(novaUf)
})

let cepAnterior = ''
watch(
  () => form.cep,
  async (cep) => {
    const limpo = somenteDigitos(cep ?? '')
    if (limpo === cepAnterior || limpo.length !== 8) return
    cepAnterior = limpo
    try {
      const resposta = await useApi(`/cadastros/geografia/cep/${limpo}`)
      const dados = extrairDados<{ logradouro?: string; bairro?: string; uf?: string }>(resposta)
      if (dados) {
        form.logradouro = dados.logradouro ?? form.logradouro
        form.bairro = dados.bairro ?? form.bairro
        if (dados.uf) uf.value = dados.uf
      }
    } catch (e) {
      console.error('[imoveis/[id]] cep', e)
    }
  }
)

async function carregarImovel() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/imobiliaria/imoveis/${idParam}`)
    const dados = extrairDados<Partial<ImovelForm> & {
      proprietarios?: Array<{ pessoaId: string }>
      vistorias?: VistoriaInput[]
      custos?: CustoImovelInput[]
    }>(resposta)
    if (dados) {
      form.descricao = dados.descricao ?? ''
      form.municipioId = dados.municipioId ?? null
      form.cep = dados.cep ?? ''
      form.logradouro = dados.logradouro ?? ''
      form.numero = dados.numero ?? ''
      form.complemento = dados.complemento ?? ''
      form.bairro = dados.bairro ?? ''
      form.proprietarios = (dados.proprietarios ?? []).map((p) => ({ pessoaId: p.pessoaId }))
      form.vistorias = (dados.vistorias ?? []).map((v) => ({
        local: v.local ?? '',
        descricao: v.descricao ?? '',
        dataVistoria: v.dataVistoria ? String(v.dataVistoria).slice(0, 10) : null
      }))
      form.custos = (dados.custos ?? []).map((c) => ({
        descricao: c.descricao ?? '',
        valor: c.valor ?? 0,
        competencia: c.competencia ? String(c.competencia).slice(0, 10) : null
      }))
    }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function salvar() {
  if (isEdit.value) return // sem PUT: modo somente leitura
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    if (erros.proprietarios) abaAtiva.value = 'proprietarios'
    return
  }
  salvando.value = true
  try {
    const payload = {
      descricao: form.descricao,
      municipioId: form.municipioId,
      cep: form.cep || null,
      logradouro: form.logradouro || null,
      numero: form.numero || null,
      complemento: form.complemento || null,
      bairro: form.bairro || null,
      proprietarios: form.proprietarios.filter((p) => p.pessoaId).map((p) => ({ pessoaId: p.pessoaId })),
      vistorias: form.vistorias.map((v) => ({
        local: v.local,
        descricao: v.descricao,
        dataVistoria: v.dataVistoria
      })),
      custos: form.custos.map((c) => ({
        descricao: c.descricao,
        valor: c.valor ?? 0,
        competencia: c.competencia
      }))
    }
    await useApi('/imobiliaria/imoveis', { method: 'POST', body: payload })
    toast.success('Imóvel cadastrado com sucesso!')
    router.push('/erp/imobiliaria/imoveis')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/imobiliaria/imoveis')
}

onMounted(async () => {
  await carregarPessoas()
  await carregarImovel()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Imóvel (visualização)' : 'Novo imóvel'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">{{ isEdit ? 'Voltar' : 'Cancelar' }}</button>
        <button v-if="!isEdit" type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div v-if="isEdit" class="readonly-banner">
      Modo somente leitura: a API não expõe atualização de imóvel (sem PUT).
    </div>

    <div class="glass-panel form-panel">
      <nav class="form-tabs">
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'dados' }" @click="abaAtiva = 'dados'">Dados & Endereço</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'proprietarios' }" @click="abaAtiva = 'proprietarios'">Proprietários</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'vistorias' }" @click="abaAtiva = 'vistorias'">Vistorias</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'custos' }" @click="abaAtiva = 'custos'">Custos</button>
      </nav>

      <form class="vertical-form mt-4" @submit.prevent="salvar">
        <!-- Dados & Endereço -->
        <div v-show="abaAtiva === 'dados'" class="form-tab-content">
          <div class="form-grid">
            <TextField v-model="form.descricao" label="Descrição" required maxlength="200" :disabled="isEdit" :error="erros.descricao" />
            <TextField
              v-model="form.cep"
              label="CEP"
              :disabled="isEdit"
              @update:model-value="(v) => (form.cep = maskCEP(v as string))"
            />
            <TextField v-model="form.logradouro" label="Logradouro" :disabled="isEdit" />
            <TextField v-model="form.numero" label="Número" :disabled="isEdit" />
            <TextField v-model="form.complemento" label="Complemento" :disabled="isEdit" />
            <TextField v-model="form.bairro" label="Bairro" :disabled="isEdit" />
            <SelectField
              v-model="uf"
              label="UF (para filtrar municípios)"
              :options="ufsBrasil"
              :disabled="isEdit"
              placeholder="Selecione a UF..."
            />
            <SelectField
              v-model="form.municipioId"
              label="Município"
              :options="opcoesMunicipios"
              :disabled="isEdit"
              :placeholder="uf ? 'Selecione...' : 'Selecione a UF primeiro'"
            />
          </div>
        </div>

        <!-- Proprietários -->
        <div v-show="abaAtiva === 'proprietarios'" class="form-tab-content">
          <p v-if="erros.proprietarios" class="field-error mb-2">{{ erros.proprietarios }}</p>
          <div v-for="(prop, i) in form.proprietarios" :key="`prop-${i}`" class="repeater-row">
            <SelectField
              v-model="prop.pessoaId"
              label="Proprietário (pessoa)"
              :options="opcoesPessoas"
              :disabled="isEdit"
              class="repeater-grow"
            />
            <button v-if="!isEdit" type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="removerProprietario(i)">Remover</button>
          </div>
          <button v-if="!isEdit" type="button" class="btn btn-secondary btn-sm mt-2" @click="adicionarProprietario">+ Adicionar proprietário</button>
        </div>

        <!-- Vistorias -->
        <div v-show="abaAtiva === 'vistorias'" class="form-tab-content">
          <div v-for="(vist, i) in form.vistorias" :key="`vist-${i}`" class="repeater-block">
            <div class="form-grid">
              <TextField v-model="vist.local" label="Local" :disabled="isEdit" />
              <TextField v-model="vist.descricao" label="Descrição" :disabled="isEdit" />
              <DateTimeField v-model="vist.dataVistoria" label="Data da vistoria" :disabled="isEdit" />
            </div>
            <button v-if="!isEdit" type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="removerVistoria(i)">Remover vistoria</button>
          </div>
          <p v-if="!form.vistorias.length" class="text-muted">Nenhuma vistoria adicionada.</p>
          <button v-if="!isEdit" type="button" class="btn btn-secondary btn-sm mt-2" @click="adicionarVistoria">+ Adicionar vistoria</button>
        </div>

        <!-- Custos -->
        <div v-show="abaAtiva === 'custos'" class="form-tab-content">
          <div v-for="(custo, i) in form.custos" :key="`custo-${i}`" class="repeater-block">
            <div class="form-grid">
              <TextField v-model="custo.descricao" label="Descrição" :disabled="isEdit" />
              <MoneyInput v-model="custo.valor" label="Valor" :readonly="isEdit" />
              <DateTimeField v-model="custo.competencia" label="Competência" :disabled="isEdit" />
            </div>
            <button v-if="!isEdit" type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="removerCusto(i)">Remover custo</button>
          </div>
          <p v-if="!form.custos.length" class="text-muted">Nenhum custo adicionado.</p>
          <button v-if="!isEdit" type="button" class="btn btn-secondary btn-sm mt-2" @click="adicionarCusto">+ Adicionar custo</button>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.readonly-banner {
  margin-top: 8px;
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 13px;
  color: var(--text-secondary);
  background: rgba(234, 179, 8, 0.08);
  border: 1px solid rgba(234, 179, 8, 0.25);
}
.form-tabs { display: flex; gap: 8px; border-bottom: 1px solid var(--border-color); padding-bottom: 12px; flex-wrap: wrap; }
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
.tab-link:hover, .tab-link.active { background: rgba(255, 255, 255, 0.03); color: var(--text-primary); }
.tab-link.active { box-shadow: 0 0 10px rgba(99, 102, 241, 0.15); border: 1px solid rgba(99, 102, 241, 0.2); }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.repeater-row { display: flex; align-items: flex-end; gap: 12px; margin-bottom: 12px; }
.repeater-grow { flex: 1; }
.repeater-block { padding: 14px; border: 1px solid var(--border-color); border-radius: 8px; margin-bottom: 14px; }
.repeater-block .btn { margin-top: 10px; }
.text-muted { color: var(--text-secondary); font-size: 13px; }
.mt-2 { margin-top: 10px; }
.mt-4 { margin-top: 20px; }
.mb-2 { margin-bottom: 10px; }
</style>
