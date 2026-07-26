<script setup lang="ts">
/**
 * Grupo de Recorrência — novo/edição — Serviços Financeiros.
 * POST /servicos-financeiros/grupos-recorrencia · PUT /{id}. Sem GET/{id}: edição via listagem.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface GrupoForm {
  id?: string
  descricao: string | null
  meses: number | null
  diaVencimento: number | null
  valor: number | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const form = reactive<GrupoForm>({ descricao: null, meses: 1, diaVencimento: 1, valor: null })
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.descricao) erros.descricao = 'Descrição é obrigatória.'
  if (form.meses == null || form.meses < 1) erros.meses = 'Meses deve ser ao menos 1.'
  if (form.diaVencimento == null || form.diaVencimento < 1 || form.diaVencimento > 31) erros.diaVencimento = 'Dia de vencimento entre 1 e 31.'
  if (form.valor == null) erros.valor = 'Valor é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi('/servicos-financeiros/grupos-recorrencia', { query: { pagina: 1, tamanhoPagina: 500 } })
    const bruto = extrairDados<unknown>(resposta)
    const itens = (Array.isArray(bruto) ? bruto : (bruto as { itens?: GrupoForm[] })?.itens) ?? []
    const encontrado = (itens as GrupoForm[]).find((g) => String(g.id) === idParam)
    if (encontrado) Object.assign(form, encontrado)
    else toast.error('Grupo não encontrado.')
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
    const body = { descricao: form.descricao, meses: form.meses, diaVencimento: form.diaVencimento, valor: form.valor }
    if (isEdit.value) {
      await useApi('/servicos-financeiros/grupos-recorrencia/{id}', { method: 'PUT', params: { id: idParam }, body: { id: idParam, ...body } })
    } else {
      await useApi('/servicos-financeiros/grupos-recorrencia', { method: 'POST', body })
    }
    toast.success('Registro salvo com sucesso!')
    router.push('/erp/financeiro/servicos-financeiros/grupos-recorrencia')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/servicos-financeiros/grupos-recorrencia')
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar grupo' : 'Novo grupo'" :loading="carregando">
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
          <TextField
            :model-value="form.meses"
            label="Meses"
            type="number"
            :error="erros.meses"
            @update:model-value="(v) => (form.meses = v === '' ? null : Number(v))"
          />
          <TextField
            :model-value="form.diaVencimento"
            label="Dia de vencimento"
            type="number"
            :error="erros.diaVencimento"
            @update:model-value="(v) => (form.diaVencimento = v === '' ? null : Number(v))"
          />
          <MoneyInput v-model="form.valor" label="Valor" :error="erros.valor" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
