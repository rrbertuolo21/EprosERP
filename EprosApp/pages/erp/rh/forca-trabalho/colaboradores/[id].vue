<script setup lang="ts">
/**
 * Novo vínculo (força de trabalho) — RH.
 *
 * Fonte: POST /rh/forca-trabalho/colaboradores. Criação apenas (sem GET/{id}/PUT na API).
 * FKs sem endpoint de listagem (pessoaId, cargoId, departamentoId, filialId) ficam como
 * campo de UUID; turnoId usa o select de turnos.
 */
import { reactive, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { useOpcoesRh } from '~/components/rh-comum/useOpcoesRh'

definePageMeta({ layout: 'default' })

interface VinculoForm {
  pessoaId: string
  matricula: string | null
  cargoId: string
  departamentoId: string
  filialId: string | null
  turnoId: string | null
  dataAdmissao: string | null
  salarioBase: number | null
  tipoRemuneracao: string | null
}

const router = useRouter()
const toast = useToast()
const { turnos, carregarTurnos } = useOpcoesRh()

const salvando = ref(false)
const form = reactive<VinculoForm>({
  pessoaId: '',
  matricula: null,
  cargoId: '',
  departamentoId: '',
  filialId: null,
  turnoId: null,
  dataAdmissao: null,
  salarioBase: null,
  tipoRemuneracao: null
})

const erros = reactive<Record<string, string>>({})
function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}
function validar(): boolean {
  limparErros()
  // pessoaId, cargoId e departamentoId são obrigatórios no DTO (uuid).
  if (!form.pessoaId) erros.pessoaId = 'Pessoa (UUID) é obrigatória.'
  if (!form.cargoId) erros.cargoId = 'Cargo (UUID) é obrigatório.'
  if (!form.departamentoId) erros.departamentoId = 'Departamento (UUID) é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/forca-trabalho/colaboradores', { method: 'POST', body: form })
    toast.success('Vínculo criado com sucesso!')
    router.push('/erp/rh/forca-trabalho/colaboradores')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/rh/forca-trabalho/colaboradores')
}

onMounted(() => {
  void carregarTurnos()
})
</script>

<template>
  <div>
    <PageToolbar title="Novo vínculo">
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
          <!-- TODO: sem endpoint de listagem para Pessoa no digest — UUID manual. -->
          <TextField v-model="form.pessoaId" label="Pessoa (UUID)" required :error="erros.pessoaId" placeholder="00000000-0000-0000-0000-000000000000" />
          <TextField v-model="form.matricula" label="Matrícula" maxlength="30" />
          <!-- TODO: sem endpoint de listagem para Cargo no digest — UUID manual. -->
          <TextField v-model="form.cargoId" label="Cargo (UUID)" required :error="erros.cargoId" placeholder="UUID" />
          <!-- TODO: sem endpoint de listagem para Departamento no digest — UUID manual. -->
          <TextField v-model="form.departamentoId" label="Departamento (UUID)" required :error="erros.departamentoId" placeholder="UUID" />
          <!-- TODO: sem endpoint de listagem para Filial no digest — UUID manual. -->
          <TextField v-model="form.filialId" label="Filial (UUID)" placeholder="UUID" />
          <SelectField v-model="form.turnoId" label="Turno" :options="turnos" />
          <DateTimeField v-model="form.dataAdmissao" label="Data de admissão" />
          <MoneyInput v-model="form.salarioBase" label="Salário base" />
          <TextField v-model="form.tipoRemuneracao" label="Tipo de remuneração" maxlength="40" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
