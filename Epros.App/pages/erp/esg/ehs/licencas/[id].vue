<script setup lang="ts">
/**
 * Formulário de Licença Ambiental (novo) — ESG / EHS / Licenças.
 * A API expõe só `POST /esg/ehs/licencas` (create-only). O `registroEhsId` é
 * carregado do endpoint de listagem de registros EHS.
 */
import { reactive, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface LicencaForm {
  registroEhsId: string | null
  tipo: string | null
  numero: string | null
  autoridade: string | null
  dataEmissao: string | null
  dataValidade: string | null
  responsavelId: string | null
  arquivoId: string | null
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const erros = reactive<Record<string, string>>({})
const opcoesRegistros = ref<SelectOption[]>([])

const form = reactive<LicencaForm>({
  registroEhsId: null,
  tipo: null,
  numero: null,
  autoridade: null,
  dataEmissao: null,
  dataValidade: null,
  responsavelId: null,
  arquivoId: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.registroEhsId) erros.registroEhsId = 'Registro EHS é obrigatório.'
  if (!form.dataEmissao) erros.dataEmissao = 'Data de emissão é obrigatória.'
  if (!form.dataValidade) erros.dataValidade = 'Data de validade é obrigatória.'
  if (!form.responsavelId) erros.responsavelId = 'Responsável é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregarRegistros() {
  try {
    const resp = await useApi('/esg/ehs/registros')
    const arr = extrairDados<Array<{ id: string; codigo?: string; descricao?: string }>>(resp) ?? []
    opcoesRegistros.value = arr.map((r) => ({ label: r.codigo ?? r.descricao ?? r.id, value: r.id }))
  } catch (e) {
    console.error('[licencas/[id]] registros', e)
  }
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/esg/ehs/licencas', { method: 'POST', body: form })
    toast.success('Licença ambiental registrada com sucesso!')
    router.push('/erp/esg/ehs/licencas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/esg/ehs/licencas')
}

onMounted(() => {
  void carregarRegistros()
})
</script>

<template>
  <div>
    <PageToolbar title="Nova licença ambiental">
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
          <SelectField v-model="form.registroEhsId" label="Registro EHS" required :options="opcoesRegistros" :error="erros.registroEhsId" />
          <TextField v-model="form.tipo" label="Tipo" placeholder="Ex.: Operação, Instalação" />
          <TextField v-model="form.numero" label="Número" />
          <TextField v-model="form.autoridade" label="Autoridade emissora" placeholder="Ex.: IBAMA, CETESB" />
          <DateTimeField v-model="form.dataEmissao" label="Data de emissão" required :error="erros.dataEmissao" />
          <DateTimeField v-model="form.dataValidade" label="Data de validade" required :error="erros.dataValidade" />
          <!-- TODO: responsavelId / arquivoId são uuid; sem endpoint de listagem, mantidos como texto. -->
          <TextField v-model="form.responsavelId" label="Responsável (UUID)" required :error="erros.responsavelId" />
          <TextField v-model="form.arquivoId" label="Arquivo (UUID)" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 16px; }
</style>
