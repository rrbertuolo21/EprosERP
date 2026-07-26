<script setup lang="ts">
/**
 * Formulário de Exposição Cambial — Câmbio/Risco.
 * A API expõe apenas POST /cambio-risco/exposicoes (criar). Sem edição.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'
import { carregarOpcoesDe } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface ExposicaoForm {
  moedaId: string | null
  valorExposto: number | null
  dataReferencia: string | null
  origemExposicao: string | null
  entidadeOrigemTipo: string | null
  entidadeOrigemId: string | null
  taxaReferenciaId: string | null
  valorMoedaBase: number | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const isEdit = computed(() => (route.params.id as string) !== 'novo')
const salvando = ref(false)
const opcoesMoeda = ref<SelectOption[]>([])

const form = reactive<ExposicaoForm>({
  moedaId: null,
  valorExposto: null,
  dataReferencia: null,
  origemExposicao: null,
  entidadeOrigemTipo: null,
  entidadeOrigemId: null,
  taxaReferenciaId: null,
  valorMoedaBase: null
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.moedaId) erros.moedaId = 'Moeda é obrigatória.'
  if (form.valorExposto == null) erros.valorExposto = 'Valor exposto é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/cambio-risco/exposicoes', { method: 'POST', body: { ...form } })
    toast.success('Exposição registrada com sucesso!')
    router.push('/erp/financeiro/cambio-risco/exposicoes')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/cambio-risco/exposicoes')
}

onMounted(async () => {
  if (isEdit.value) {
    toast.error('Edição de exposição não é suportada pela API.')
    router.replace('/erp/financeiro/cambio-risco/exposicoes/novo')
  }
  opcoesMoeda.value = await carregarOpcoesDe('/cambio-risco/moedas', ['nome', 'codigoIso'])
})
</script>

<template>
  <div>
    <PageToolbar title="Nova exposição cambial" :loading="salvando">
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
          <SelectField v-model="form.moedaId" label="Moeda" required :options="opcoesMoeda" :error="erros.moedaId" />
          <MoneyInput v-model="form.valorExposto" label="Valor exposto" :error="erros.valorExposto" />
          <MoneyInput v-model="form.valorMoedaBase" label="Valor na moeda base" />
          <DateTimeField v-model="form.dataReferencia" label="Data de referência" mode="datetime" />
          <TextField v-model="form.origemExposicao" label="Origem da exposição" maxlength="80" />
          <TextField v-model="form.entidadeOrigemTipo" label="Tipo da entidade de origem" maxlength="60" />
          <!-- TODO: entidadeOrigemId e taxaReferenciaId são UUID sem endpoint de listagem próprio no digest. -->
          <TextField v-model="form.entidadeOrigemId" label="ID da entidade de origem" hint="UUID" />
          <TextField v-model="form.taxaReferenciaId" label="ID da taxa de referência" hint="UUID" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
