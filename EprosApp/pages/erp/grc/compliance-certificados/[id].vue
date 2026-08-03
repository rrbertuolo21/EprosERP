<script setup lang="ts">
/**
 * Formulário de Certificado Digital (novo) — GRC / Compliance Regulatório.
 * Fonte: POST /api/v1/grc/compliance/certificados. Apenas criação.
 */
import { reactive, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useMask } from '~/composables/useMask'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface CertificadoForm {
  empresaId: string | null
  cnpj: string | null
  serial: string | null
  tipo: string | null
  origem: string | null
  dataValidade: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { maskCpfCnpj } = useMask()

const tipos: SelectOption[] = ['A1', 'A3'].map((t) => ({ label: t, value: t }))
const origens: SelectOption[] = [
  { label: 'Empresa', value: 'Empresa' },
  { label: 'Escritório Contador', value: 'EscritorioContador' }
]

const form = reactive<CertificadoForm>({ empresaId: null, cnpj: null, serial: null, tipo: 'A1', origem: 'Empresa', dataValidade: null })
const erros = reactive<Record<string, string>>({})
const salvando = ref(false)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.empresaId) erros.empresaId = 'Empresa (empresaId) é obrigatória.'
  if (!form.dataValidade) erros.dataValidade = 'Data de validade é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/grc/compliance/certificados', { method: 'POST', body: form })
    toast.success('Certificado cadastrado com sucesso!')
    router.push('/erp/grc/compliance-certificados')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/grc/compliance-certificados')
}

onMounted(() => {
  if ((route.params.id as string) !== 'novo') {
    toast.error('Edição não disponível neste módulo. Apenas cadastro de novos certificados.')
    router.replace('/erp/grc/compliance-certificados')
  }
})
</script>

<template>
  <div>
    <PageToolbar title="Novo certificado digital">
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
          <!-- empresaId é UUID; sem endpoint de listagem de empresas exposto no módulo GRC. -->
          <TextField v-model="form.empresaId" label="Empresa (empresaId)" required :error="erros.empresaId" hint="UUID da empresa" />
          <TextField
            v-model="form.cnpj"
            label="CNPJ"
            @update:model-value="(v) => (form.cnpj = maskCpfCnpj(v as string))"
          />
          <TextField v-model="form.serial" label="Serial" maxlength="100" />
          <SelectField v-model="form.tipo" label="Tipo" :options="tipos" />
          <SelectField v-model="form.origem" label="Origem" :options="origens" />
          <DateTimeField v-model="form.dataValidade" label="Data de validade" mode="datetime" required :error="erros.dataValidade" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
