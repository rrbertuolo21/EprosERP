<script setup lang="ts">
/**
 * Nova promoção — RH / Desenvolvimento.
 * Fonte: POST /rh/desenvolvimento/promocoes. Criação apenas.
 * FKs de filial/departamento/cargo (anterior e atual) não têm endpoint de listagem no
 * digest — UUID manual. colaboradorId usa o select de colaboradores.
 */
import { reactive, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { useOpcoesRh } from '~/components/rh-comum/useOpcoesRh'

definePageMeta({ layout: 'default' })

interface PromocaoForm {
  colaboradorId: string
  filialAnteriorId: string | null
  departamentoAnteriorId: string | null
  cargoAnteriorId: string | null
  filialAtualId: string | null
  departamentoAtualId: string | null
  cargoAtualId: string | null
  dataEfetiva: string | null
  motivo: string | null
  documento: string | null
}

const router = useRouter()
const toast = useToast()
const { colaboradores, carregarColaboradores } = useOpcoesRh()

const salvando = ref(false)
const form = reactive<PromocaoForm>({
  colaboradorId: '',
  filialAnteriorId: null,
  departamentoAnteriorId: null,
  cargoAnteriorId: null,
  filialAtualId: null,
  departamentoAtualId: null,
  cargoAtualId: null,
  dataEfetiva: null,
  motivo: null,
  documento: null
})

const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.colaboradorId) erros.colaboradorId = 'Colaborador é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/desenvolvimento/promocoes', { method: 'POST', body: form })
    toast.success('Promoção criada com sucesso!')
    router.push('/erp/rh/desenvolvimento/promocoes')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
function cancelar() {
  router.push('/erp/rh/desenvolvimento/promocoes')
}
onMounted(() => {
  void carregarColaboradores()
})
</script>

<template>
  <div>
    <PageToolbar title="Nova promoção">
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
          <SelectField v-model="form.colaboradorId" label="Colaborador" required :options="colaboradores" :error="erros.colaboradorId" />
          <DateTimeField v-model="form.dataEfetiva" label="Data efetiva" />
          <!-- Situação anterior / atual — sem endpoints de listagem no digest, UUID manual. -->
          <TextField v-model="form.filialAnteriorId" label="Filial anterior (UUID)" placeholder="UUID" />
          <TextField v-model="form.departamentoAnteriorId" label="Departamento anterior (UUID)" placeholder="UUID" />
          <TextField v-model="form.cargoAnteriorId" label="Cargo anterior (UUID)" placeholder="UUID" />
          <TextField v-model="form.filialAtualId" label="Filial atual (UUID)" placeholder="UUID" />
          <TextField v-model="form.departamentoAtualId" label="Departamento atual (UUID)" placeholder="UUID" />
          <TextField v-model="form.cargoAtualId" label="Cargo atual (UUID)" placeholder="UUID" />
          <TextField v-model="form.motivo" label="Motivo" maxlength="200" />
          <TextField v-model="form.documento" label="Documento" maxlength="150" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
