<script setup lang="ts">
/**
 * Formulário de Tipo de Operação Fiscal (novo/edição) — Fiscal / Tipo Operação Fiscal.
 *
 * Porta o comportamento de `fiscal/tipo-operacao-fiscal/[id].vue` do legado: descrição,
 * finalidade/atendimento/frete/movimento, CFOP de NF-e e NFC-e (filtrados conforme
 * finalidade/movimento, replicando as regras do legado) e flag "sobrescreve tributação NCM".
 * Endpoints: `tipos-operacoes-fiscais`, `cfops` (para os selects de CFOP NF-e/NFC-e).
 */
import { ref, reactive, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'
import {
  opcoesFinalidade,
  opcoesAtendimento,
  opcoesTipoFrete,
  opcoesTipoMovimento,
  FINALIDADE_DEVOLUCAO,
  TIPO_MOVIMENTO_ENTRADA,
  TIPO_MOVIMENTO_SAIDA
} from './_enums'

definePageMeta({ layout: 'default' })

interface TipoOperacaoFiscalForm {
  id?: string
  tributarioGrupoId: string | null
  descricao: string
  finalidade: number | null
  atendimento: number | null
  tipoFrete: number | null
  tipoMovimento: number | null
  cfopNfeId: string | null
  cfopNfceId: string | null
  sobescreveTributacaoNcm: boolean
}

interface CfopOpcao {
  id: string
  cfopCodigo: number
  descricao: string
  indicadorNfe: boolean
  indicadorNfce: boolean
  indicadorDevolucao: boolean
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')
const tipoOperacaoId = computed(() => (isEdit.value ? idParam : undefined))

const carregando = ref(false)
const carregandoCfops = ref(false)
const salvando = ref(false)
const erros = reactive<Record<string, string>>({})
const cfops = ref<CfopOpcao[]>([])

const form = reactive<TipoOperacaoFiscalForm>({
  tributarioGrupoId: null,
  descricao: '',
  finalidade: null,
  atendimento: null,
  tipoFrete: null,
  tipoMovimento: null,
  cfopNfeId: null,
  cfopNfceId: null,
  sobescreveTributacaoNcm: false
})

function formatarCfop(c: CfopOpcao): SelectOption {
  return { label: `${c.cfopCodigo} - ${c.descricao}`, value: c.id }
}

/** Lista de CFOPs de NF-e disponíveis, filtrada conforme finalidade/movimento (regra do legado). */
const opcoesCfopNfe = computed<SelectOption[]>(() => {
  let filtrados = cfops.value.filter((c) => c.indicadorNfe)
  if (form.finalidade === FINALIDADE_DEVOLUCAO) {
    filtrados = filtrados.filter((c) => c.indicadorDevolucao)
  } else if (form.tipoMovimento === TIPO_MOVIMENTO_ENTRADA) {
    filtrados = filtrados.filter((c) => String(c.cfopCodigo).startsWith('1') || String(c.cfopCodigo).startsWith('2') || String(c.cfopCodigo).startsWith('3'))
  } else if (form.tipoMovimento === TIPO_MOVIMENTO_SAIDA) {
    filtrados = filtrados.filter((c) => String(c.cfopCodigo).startsWith('5') || String(c.cfopCodigo).startsWith('6') || String(c.cfopCodigo).startsWith('7'))
  }
  return filtrados.map(formatarCfop)
})

const opcoesCfopNfce = computed<SelectOption[]>(() => cfops.value.filter((c) => c.indicadorNfce).map(formatarCfop))

/** Exibe o CFOP NFC-e apenas quando não for entrada e não for finalidade de devolução (regra do legado). */
const exibirCfopNfce = computed(() => form.tipoMovimento !== TIPO_MOVIMENTO_ENTRADA && form.finalidade !== FINALIDADE_DEVOLUCAO)

watch(
  () => [form.finalidade, form.tipoMovimento],
  () => {
    if (form.cfopNfeId && !opcoesCfopNfe.value.some((o) => o.value === form.cfopNfeId)) {
      form.cfopNfeId = null
    }
  }
)

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()
  if (!form.descricao?.trim()) erros.descricao = 'A descrição é obrigatória.'
  else if (form.descricao.length > 150) erros.descricao = 'A descrição deve ter no máximo 150 caracteres.'
  if (form.finalidade === null) erros.finalidade = 'A finalidade é obrigatória.'
  if (form.atendimento === null) erros.atendimento = 'O atendimento é obrigatório.'
  if (form.tipoFrete === null) erros.tipoFrete = 'O tipo de frete é obrigatório.'
  if (form.tipoMovimento === null) erros.tipoMovimento = 'O tipo de movimento é obrigatório.'
  if (!form.tributarioGrupoId) erros.tributarioGrupoId = 'O grupo tributário é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregarCfops() {
  carregandoCfops.value = true
  try {
    const resposta = await useApi('/cfops', { query: { tamanhoPagina: 500 } })
    cfops.value = extrairDados<CfopOpcao[]>(resposta) ?? []
  } catch (e) {
    console.error('[tipo-operacao-fiscal/[id]] cfops', e)
    cfops.value = []
  } finally {
    carregandoCfops.value = false
  }
}

async function carregarTipoOperacao() {
  if (!isEdit.value || !tipoOperacaoId.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/tipos-operacoes-fiscais/${tipoOperacaoId.value}`)
    const dados = extrairDados<Partial<TipoOperacaoFiscalForm>>(resposta)
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
    if (isEdit.value) {
      await useApi(`/tipos-operacoes-fiscais/${tipoOperacaoId.value}`, { method: 'PUT', body: { id: tipoOperacaoId.value, ...form } })
    } else {
      await useApi('/tipos-operacoes-fiscais', { method: 'POST', body: form })
    }
    toast.success('Tipo de operação fiscal salvo com sucesso!')
    router.push('/erp/fiscal/tipo-operacao-fiscal')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/fiscal/tipo-operacao-fiscal')
}

onMounted(async () => {
  await carregarCfops()
  await carregarTipoOperacao()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar tipo de operação fiscal' : 'Novo tipo de operação fiscal'" :loading="carregando">
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
        <div class="form-grid form-grid-1">
          <TextField v-model="form.descricao" label="Descrição" required maxlength="150" :error="erros.descricao" />
        </div>
        <div class="form-grid">
          <SelectField v-model.number="form.finalidade" label="Finalidade" required :options="opcoesFinalidade" :error="erros.finalidade" />
          <SelectField v-model.number="form.atendimento" label="Atendimento" required :options="opcoesAtendimento" :error="erros.atendimento" />
          <SelectField v-model.number="form.tipoFrete" label="Tipo Frete" required :options="opcoesTipoFrete" :error="erros.tipoFrete" />
          <SelectField v-model.number="form.tipoMovimento" label="Tipo Movimento" required :options="opcoesTipoMovimento" :error="erros.tipoMovimento" />
          <SelectField
            v-model="form.cfopNfeId"
            label="CFOP NF-e"
            required
            :options="opcoesCfopNfe"
            :disabled="carregandoCfops"
            :error="erros.cfopNfeId"
          />
          <SelectField
            v-if="exibirCfopNfce"
            v-model="form.cfopNfceId"
            label="CFOP NFC-e"
            required
            :options="opcoesCfopNfce"
            :disabled="carregandoCfops"
            :error="erros.cfopNfceId"
          />
          <label class="field-checkbox toggle-row">
            <input v-model="form.sobescreveTributacaoNcm" type="checkbox" /> Sobrescreve Tributação NCM
          </label>
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
  margin-bottom: 12px;
}
.form-grid-1 { grid-template-columns: 1fr; }
.field-checkbox {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13.5px;
  color: var(--text-secondary);
}
.field-checkbox input { width: 16px; height: 16px; accent-color: var(--primary); }
.toggle-row { padding-top: 22px; }
</style>
