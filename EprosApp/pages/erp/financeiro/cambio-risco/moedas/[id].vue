<script setup lang="ts">
/**
 * Formulário de Moeda (novo/edição) — Câmbio/Risco.
 * POST /cambio-risco/moedas · PUT /cambio-risco/moedas/{id}.
 * A API não expõe GET /{id}; na edição os dados são carregados da listagem.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface MoedaForm {
  id?: string
  codigoIso: string | null
  simbolo: string | null
  nome: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const form = reactive<MoedaForm>({ id: undefined, codigoIso: null, simbolo: null, nome: null })
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.nome) erros.nome = 'Nome é obrigatório.'
  if (!form.codigoIso) erros.codigoIso = 'Código ISO é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi('/cambio-risco/moedas', { query: { pagina: 1, tamanhoPagina: 500 } })
    const bruto = extrairDados<unknown>(resposta)
    const itens = (Array.isArray(bruto) ? bruto : (bruto as { itens?: MoedaForm[] })?.itens) ?? []
    const encontrada = (itens as MoedaForm[]).find((m) => String(m.id) === idParam)
    if (encontrada) Object.assign(form, encontrada)
    else toast.error('Moeda não encontrada.')
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
      await useApi('/cambio-risco/moedas/{id}', {
        method: 'PUT',
        params: { id: idParam },
        body: { id: idParam, codigoIso: form.codigoIso, simbolo: form.simbolo, nome: form.nome }
      })
    } else {
      await useApi('/cambio-risco/moedas', {
        method: 'POST',
        body: { codigoIso: form.codigoIso, simbolo: form.simbolo, nome: form.nome }
      })
    }
    toast.success('Registro salvo com sucesso!')
    router.push('/erp/financeiro/cambio-risco/moedas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/cambio-risco/moedas')
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar moeda' : 'Nova moeda'" :loading="carregando">
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
          <TextField v-model="form.codigoIso" label="Código ISO" required maxlength="3" placeholder="Ex.: USD" :error="erros.codigoIso" />
          <TextField v-model="form.simbolo" label="Símbolo" maxlength="5" placeholder="Ex.: $" />
          <TextField v-model="form.nome" label="Nome" required maxlength="80" :error="erros.nome" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
