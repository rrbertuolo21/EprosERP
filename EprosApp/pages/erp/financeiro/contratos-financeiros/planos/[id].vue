<script setup lang="ts">
/**
 * Plano de Contrato — novo/edição.
 * POST /contratos-financeiros/planos · PUT /contratos-financeiros/planos/{id}.
 * Sem GET/{id}: na edição os dados vêm da listagem.
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
import { OPCOES_PERIODICIDADE } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface PlanoForm {
  id?: string
  descricao: string | null
  valor: number | null
  periodicidade: number | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const form = reactive<PlanoForm>({ descricao: null, valor: null, periodicidade: 0 })
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.descricao) erros.descricao = 'Descrição é obrigatória.'
  if (form.valor == null) erros.valor = 'Valor é obrigatório.'
  if (form.periodicidade == null) erros.periodicidade = 'Periodicidade é obrigatória.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi('/contratos-financeiros/planos', { query: { pagina: 1, tamanhoPagina: 500 } })
    const bruto = extrairDados<unknown>(resposta)
    const itens = (Array.isArray(bruto) ? bruto : (bruto as { itens?: PlanoForm[] })?.itens) ?? []
    const encontrado = (itens as PlanoForm[]).find((p) => String(p.id) === idParam)
    if (encontrado) Object.assign(form, encontrado)
    else toast.error('Plano não encontrado.')
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
      await useApi('/contratos-financeiros/planos/{id}', {
        method: 'PUT',
        params: { id: idParam },
        body: { id: idParam, descricao: form.descricao, valor: form.valor, periodicidade: form.periodicidade }
      })
    } else {
      await useApi('/contratos-financeiros/planos', {
        method: 'POST',
        body: { descricao: form.descricao, valor: form.valor, periodicidade: form.periodicidade }
      })
    }
    toast.success('Registro salvo com sucesso!')
    router.push('/erp/financeiro/contratos-financeiros/planos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/contratos-financeiros/planos')
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar plano' : 'Novo plano'" :loading="carregando">
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
          <TextField v-model="form.descricao" label="Descrição" required maxlength="120" :error="erros.descricao" />
          <MoneyInput v-model="form.valor" label="Valor" :error="erros.valor" />
          <SelectField v-model="form.periodicidade" label="Periodicidade" required :options="OPCOES_PERIODICIDADE" :clearable="false" :error="erros.periodicidade" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
