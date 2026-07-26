<script setup lang="ts">
/**
 * Formulário de Inventário GEE (novo) — ESG / GHG / Inventários.
 * A API expõe só `POST /esg/ghg/inventarios` (create-only). O ciclo de vida
 * (submeter/aprovar/consolidar) é feito por ações na listagem.
 */
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface InventarioForm {
  codigo: string | null
  versao: number | string | null
  periodoInicio: string | null
  periodoFim: string | null
  criterioFronteira: string | null
  metodologia: string | null
  responsavelId: string | null
  observacao: string | null
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const erros = reactive<Record<string, string>>({})

const form = reactive<InventarioForm>({
  codigo: null,
  versao: 1,
  periodoInicio: null,
  periodoFim: null,
  criterioFronteira: null,
  metodologia: null,
  responsavelId: null,
  observacao: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.codigo) erros.codigo = 'Código é obrigatório.'
  if (form.versao == null) erros.versao = 'Versão é obrigatória.'
  if (!form.periodoInicio) erros.periodoInicio = 'Início do período é obrigatório.'
  if (!form.periodoFim) erros.periodoFim = 'Fim do período é obrigatório.'
  if (!form.responsavelId) erros.responsavelId = 'Responsável é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/esg/ghg/inventarios', { method: 'POST', body: { ...form, versao: Number(form.versao) } })
    toast.success('Inventário criado com sucesso!')
    router.push('/erp/esg/ghg/inventarios')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/esg/ghg/inventarios')
}
</script>

<template>
  <div>
    <PageToolbar title="Novo inventário GEE">
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
          <TextField v-model="form.codigo" label="Código" required :error="erros.codigo" />
          <TextField v-model="form.versao" label="Versão" type="number" required :error="erros.versao" />
          <DateTimeField v-model="form.periodoInicio" label="Início do período" required :error="erros.periodoInicio" />
          <DateTimeField v-model="form.periodoFim" label="Fim do período" required :error="erros.periodoFim" />
          <TextField v-model="form.criterioFronteira" label="Critério de fronteira" placeholder="Ex.: Controle operacional" />
          <TextField v-model="form.metodologia" label="Metodologia" placeholder="Ex.: GHG Protocol" />
          <!-- TODO: responsavelId é um uuid de usuário; sem endpoint de listagem no módulo ESG, mantido como texto. -->
          <TextField v-model="form.responsavelId" label="Responsável (UUID)" required :error="erros.responsavelId" placeholder="UUID do responsável" />
          <TextField v-model="form.observacao" label="Observação" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 16px; }
</style>
