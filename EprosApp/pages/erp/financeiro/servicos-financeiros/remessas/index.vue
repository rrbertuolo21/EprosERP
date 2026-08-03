<script setup lang="ts">
/**
 * Remessa CNAB — Serviços Financeiros.
 * A API expõe apenas POST /servicos-financeiros/remessas (não há listagem). Tela de geração.
 * Lacuna: POST /remessas/{id}/boletos (vincular boleto à remessa) não tem tela — depende do
 * id da remessa recém-criada, não retornado de forma padronizada pelo digest.
 */
import { ref, reactive, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'
import { OPCOES_LAYOUT_CNAB, carregarOpcoesDe } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface RemessaForm {
  nomeArquivo: string | null
  dataGeracao: string | null
  grupo: number | null
  layout: number | null
  contaEmissoraId: string | null
}

const toast = useToast()
const salvando = ref(false)
const opcoesConta = ref<SelectOption[]>([])

const form = reactive<RemessaForm>({
  nomeArquivo: null,
  dataGeracao: null,
  grupo: 0,
  layout: 240,
  contaEmissoraId: null
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.dataGeracao) erros.dataGeracao = 'Data de geração é obrigatória.'
  if (form.layout == null) erros.layout = 'Layout é obrigatório.'
  if (!form.contaEmissoraId) erros.contaEmissoraId = 'Conta emissora é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/servicos-financeiros/remessas', { method: 'POST', body: { ...form } })
    toast.success('Remessa gerada com sucesso!')
    form.nomeArquivo = null
    form.dataGeracao = null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

onMounted(async () => {
  opcoesConta.value = await carregarOpcoesDe('/servicos-financeiros/contas-emissoras', ['nomeBanco', 'conta'])
})
</script>

<template>
  <div>
    <PageToolbar title="Gerar Remessa CNAB" subtitle="Arquivo de remessa bancária" :loading="salvando">
      <template #actions>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Gerar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <SelectField v-model="form.contaEmissoraId" label="Conta emissora" required :options="opcoesConta" :error="erros.contaEmissoraId" />
          <SelectField v-model="form.layout" label="Layout CNAB" required :options="OPCOES_LAYOUT_CNAB" :clearable="false" :error="erros.layout" />
          <TextField v-model="form.nomeArquivo" label="Nome do arquivo" maxlength="60" />
          <DateTimeField v-model="form.dataGeracao" label="Data de geração" mode="datetime" required :error="erros.dataGeracao" />
          <TextField
            :model-value="form.grupo"
            label="Grupo"
            type="number"
            @update:model-value="(v) => (form.grupo = v === '' ? null : Number(v))"
          />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
