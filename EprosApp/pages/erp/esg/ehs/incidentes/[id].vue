<script setup lang="ts">
/**
 * Formulário de Incidente EHS (novo) — ESG / EHS / Incidentes.
 * A API expõe só `POST /esg/ehs/incidentes` (create-only). O `registroEhsId` é
 * carregado do endpoint de listagem de registros EHS.
 */
import { reactive, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApi, extrairLista } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface IncidenteForm {
  registroEhsId: string | null
  tipo: string | null
  dataHora: string | null
  localId: string | null
  descricao: string | null
  gravidade: string | null
  impacto: string | null
  pessoaId: string | null
  numeroCat: string | null
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const erros = reactive<Record<string, string>>({})
const opcoesRegistros = ref<SelectOption[]>([])

const form = reactive<IncidenteForm>({
  registroEhsId: null,
  tipo: null,
  dataHora: null,
  localId: null,
  descricao: null,
  gravidade: null,
  impacto: null,
  pessoaId: null,
  numeroCat: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.registroEhsId) erros.registroEhsId = 'Registro EHS é obrigatório.'
  if (!form.dataHora) erros.dataHora = 'Data/hora é obrigatória.'
  return Object.keys(erros).length === 0
}

async function carregarRegistros() {
  try {
    const resp = await useApi('/esg/ehs/registros')
    const arr = extrairLista<{ id: string; codigo?: string; descricao?: string }>(resp) ?? []
    opcoesRegistros.value = arr.map((r) => ({ label: r.codigo ?? r.descricao ?? r.id, value: r.id }))
  } catch (e) {
    console.error('[incidentes/[id]] registros', e)
  }
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/esg/ehs/incidentes', { method: 'POST', body: form })
    toast.success('Incidente registrado com sucesso!')
    router.push('/erp/esg/ehs/incidentes')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/esg/ehs/incidentes')
}

onMounted(() => {
  void carregarRegistros()
})
</script>

<template>
  <div>
    <PageToolbar title="Novo incidente EHS">
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
          <TextField v-model="form.tipo" label="Tipo" placeholder="Ex.: Acidente, Quase-acidente" />
          <DateTimeField v-model="form.dataHora" label="Data/hora" mode="datetime" required :error="erros.dataHora" />
          <TextField v-model="form.gravidade" label="Gravidade" placeholder="Ex.: Leve, Grave" />
          <TextField v-model="form.impacto" label="Impacto" />
          <TextField v-model="form.descricao" label="Descrição" />
          <TextField v-model="form.numeroCat" label="Número CAT" />
          <!-- TODO: localId / pessoaId são uuid; sem endpoint de listagem no módulo, mantidos como texto. -->
          <TextField v-model="form.localId" label="Local (UUID)" />
          <TextField v-model="form.pessoaId" label="Pessoa (UUID)" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 16px; }
</style>
