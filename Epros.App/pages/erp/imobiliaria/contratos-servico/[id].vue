<script setup lang="ts">
/**
 * Formulário de Contrato de Serviço — Imobiliária / Contratos de Serviço.
 *
 * Novo: POST /imobiliaria/contratos-servico (CriarContratoServicoCommand). Campos do corpo:
 *   proprietarioId (obrigatório), imovelId, descricao, vigenciaInicio, vigenciaFim, remuneracao.
 *
 * A API não expõe detalhe (GET /{id}) nem atualização (PUT): esta tela só cria.
 * Ao abrir com um id existente, redireciona para a listagem.
 */
import { ref, computed, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface ContratoForm {
  proprietarioId: string | null
  imovelId: string | null
  descricao: string
  vigenciaInicio: string | null
  vigenciaFim: string | null
  remuneracao: number | null
}

interface Pessoa {
  id: string
  nome?: string | null
  razaoSocial?: string | null
}
interface Imovel {
  id: string
  descricao?: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const salvando = ref(false)
const pessoas = ref<Pessoa[]>([])
const imoveis = ref<Imovel[]>([])

const form = reactive<ContratoForm>({
  proprietarioId: null,
  imovelId: null,
  descricao: '',
  vigenciaInicio: null,
  vigenciaFim: null,
  remuneracao: null
})

const erros = reactive<Record<string, string>>({})

const opcoesPessoas = computed<SelectOption[]>(() =>
  pessoas.value.map((p) => ({ label: p.razaoSocial ?? p.nome ?? p.id, value: p.id }))
)
const opcoesImoveis = computed<SelectOption[]>(() =>
  imoveis.value.map((i) => ({ label: i.descricao ?? i.id, value: i.id }))
)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.proprietarioId) erros.proprietarioId = 'O contrato de serviço exige um proprietário.'
  if (form.vigenciaInicio && form.vigenciaFim && form.vigenciaFim < form.vigenciaInicio) {
    erros.vigenciaFim = 'O fim da vigência deve ser igual ou posterior ao início.'
  }
  return Object.keys(erros).length === 0
}

async function carregarPessoas() {
  try {
    const resposta = await useApi('/cadastros/pessoas', { query: { tamanhoPagina: 500 } })
    pessoas.value = extrairDados<Pessoa[]>(resposta) ?? []
  } catch (e) {
    console.error('[contratos-servico/[id]] pessoas', e)
  }
}

async function carregarImoveis() {
  try {
    const resposta = await useApi('/imobiliaria/imoveis')
    imoveis.value = extrairDados<Imovel[]>(resposta) ?? []
  } catch (e) {
    console.error('[contratos-servico/[id]] imoveis', e)
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
      proprietarioId: form.proprietarioId,
      imovelId: form.imovelId,
      descricao: form.descricao || null,
      vigenciaInicio: form.vigenciaInicio,
      vigenciaFim: form.vigenciaFim,
      remuneracao: form.remuneracao
    }
    await useApi('/imobiliaria/contratos-servico', { method: 'POST', body: payload })
    toast.success('Contrato de serviço criado com sucesso!')
    router.push('/erp/imobiliaria/contratos-servico')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/imobiliaria/contratos-servico')
}

onMounted(async () => {
  if (isEdit.value) {
    // Sem GET de detalhe nem PUT: edição não é suportada pela API.
    toast.error('A API não permite editar contratos de serviço. Redirecionando para a lista.')
    router.replace('/erp/imobiliaria/contratos-servico')
    return
  }
  await Promise.all([carregarPessoas(), carregarImoveis()])
})
</script>

<template>
  <div>
    <PageToolbar title="Novo contrato de serviço">
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
          <SelectField
            v-model="form.proprietarioId"
            label="Proprietário"
            required
            :options="opcoesPessoas"
            :error="erros.proprietarioId"
          />
          <SelectField
            v-model="form.imovelId"
            label="Imóvel"
            :options="opcoesImoveis"
            placeholder="Selecione (opcional)..."
          />
          <TextField v-model="form.descricao" label="Descrição" maxlength="200" />
          <DateTimeField v-model="form.vigenciaInicio" label="Início da vigência" />
          <DateTimeField v-model="form.vigenciaFim" label="Fim da vigência" :error="erros.vigenciaFim" />
          <MoneyInput v-model="form.remuneracao" label="Remuneração" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
