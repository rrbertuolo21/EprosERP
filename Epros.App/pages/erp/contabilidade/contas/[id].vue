<script setup lang="ts">
/**
 * Formulário de Conta Contábil (novo/edição) — Contabilidade Geral / Contas.
 *
 * Contrato:
 *   GET  /contabilidade-geral/contas/{id}
 *   POST /contabilidade-geral/contas
 *   PUT  /contabilidade-geral/contas/{id}
 * Body (POST/PUT): codigoConta, nomeConta, contaPaiId?, nivel, tipoConta (enum),
 *   aceitaLancamento, participaContabilidadeGeral, participaOrcamento, participaDepreciacao.
 */
import { computed, reactive, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import type { SelectOption } from '~/composables/useEnum'
import { tiposContaContabil } from '~/components/contabilidade-contas/enums'

definePageMeta({ layout: 'default' })

interface ContaContabilForm {
  id?: string
  codigoConta: string
  nomeConta: string
  contaPaiId: string | null
  nivel: number
  tipoConta: number
  aceitaLancamento: boolean
  participaContabilidadeGeral: boolean
  participaOrcamento: boolean
  participaDepreciacao: boolean
}

interface ContaOpcao {
  id: string
  codigoConta?: string | null
  nomeConta?: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const contasPai = ref<ContaOpcao[]>([])

const form = reactive<ContaContabilForm>({
  id: isEdit.value ? idParam : undefined,
  codigoConta: '',
  nomeConta: '',
  contaPaiId: null,
  nivel: 1,
  tipoConta: 0,
  aceitaLancamento: true,
  participaContabilidadeGeral: true,
  participaOrcamento: false,
  participaDepreciacao: false
})

const erros = reactive<Record<string, string>>({})

const opcoesContaPai = computed<SelectOption[]>(() =>
  contasPai.value
    .filter((c) => c.id !== form.id)
    .map((c) => ({ label: `${c.codigoConta ?? ''} — ${c.nomeConta ?? ''}`.trim(), value: c.id }))
)

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()
  if (!form.codigoConta?.trim()) erros.codigoConta = 'Código da conta é obrigatório.'
  if (!form.nomeConta?.trim()) erros.nomeConta = 'Nome da conta é obrigatório.'
  if (form.nivel == null || form.nivel < 1) erros.nivel = 'Nível deve ser maior ou igual a 1.'
  if (form.tipoConta == null) erros.tipoConta = 'Tipo da conta é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregarContasPai() {
  try {
    const resposta = await useApi('/contabilidade-geral/contas', { query: { tamanhoPagina: 100 } })
    const dados = extrairDados<{ itens?: ContaOpcao[] } | ContaOpcao[]>(resposta)
    contasPai.value = Array.isArray(dados) ? dados : dados?.itens ?? []
  } catch (e) {
    console.error('[contabilidade/contas/[id]] contas-pai', e)
    contasPai.value = []
  }
}

async function carregarConta() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/contabilidade-geral/contas/{id}`, { params: { id: idParam } })
    const dados = extrairDados<Partial<ContaContabilForm>>(resposta)
    if (dados) Object.assign(form, dados)
  } catch (e) {
    toast.error(obterMensagemErro(e))
    await router.push('/erp/contabilidade/contas')
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
      await useApi(`/contabilidade-geral/contas/{id}`, { method: 'PUT', params: { id: idParam }, body: { ...form, id: idParam } })
      toast.success('Conta contábil atualizada com sucesso!')
    } else {
      await useApi('/contabilidade-geral/contas', { method: 'POST', body: form })
      toast.success('Conta contábil criada com sucesso!')
    }
    await router.push('/erp/contabilidade/contas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/contabilidade/contas')
}

onMounted(async () => {
  await carregarContasPai()
  await carregarConta()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar conta contábil' : 'Nova conta contábil'" :loading="carregando || salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div class="form-grid">
        <TextField v-model="form.codigoConta" label="Código da Conta" required maxlength="30" :error="erros.codigoConta" />
        <TextField v-model="form.nomeConta" label="Nome da Conta" required maxlength="150" :error="erros.nomeConta" />
        <SelectField
          v-model="form.contaPaiId"
          label="Conta Pai"
          :options="opcoesContaPai"
          placeholder="Nenhuma (conta raiz)"
        />
        <TextField v-model.number="form.nivel" type="number" label="Nível" required :error="erros.nivel" />
        <SelectField v-model="form.tipoConta" label="Tipo da Conta" required :options="tiposContaContabil" :clearable="false" :error="erros.tipoConta" />
      </div>

      <div class="flags-grid">
        <label class="field toggle-row">
          <input v-model="form.aceitaLancamento" type="checkbox" />
          <span class="field-label">Aceita lançamento</span>
        </label>
        <label class="field toggle-row">
          <input v-model="form.participaContabilidadeGeral" type="checkbox" />
          <span class="field-label">Participa da contabilidade geral</span>
        </label>
        <label class="field toggle-row">
          <input v-model="form.participaOrcamento" type="checkbox" />
          <span class="field-label">Participa do orçamento</span>
        </label>
        <label class="field toggle-row">
          <input v-model="form.participaDepreciacao" type="checkbox" />
          <span class="field-label">Participa da depreciação</span>
        </label>
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
.flags-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 12px;
  margin-top: 20px;
  padding-top: 16px;
  border-top: 1px solid var(--border-color);
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
</style>
