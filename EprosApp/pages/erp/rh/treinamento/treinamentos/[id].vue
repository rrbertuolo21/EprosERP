<script setup lang="ts">
/**
 * Novo treinamento — RH / Treinamento.
 * Fonte: POST /rh/treinamento/treinamentos. Criação apenas.
 * Todos os FKs (tipoTreinamentoId, treinadorId, filialId, departamentoId, criadoPorUsuarioId,
 * donoFuncionalId) são UUID manual — sem endpoint de listagem no digest.
 * horaInicio/horaFim são TimeSpan (HH:mm:ss) — texto.
 */
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface TreinamentoForm {
  titulo: string | null
  descricao: string | null
  tipoTreinamentoId: string
  treinadorId: string
  filialId: string
  departamentoId: string
  dataInicio: string | null
  dataFim: string | null
  horaInicio: string
  horaFim: string
  local: string | null
  capacidadeMaxima: number | null
  custo: number | null
  criadoPorUsuarioId: string
  donoFuncionalId: string
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const form = reactive<TreinamentoForm>({
  titulo: null,
  descricao: null,
  tipoTreinamentoId: '',
  treinadorId: '',
  filialId: '',
  departamentoId: '',
  dataInicio: null,
  dataFim: null,
  horaInicio: '',
  horaFim: '',
  local: null,
  capacidadeMaxima: null,
  custo: null,
  criadoPorUsuarioId: '',
  donoFuncionalId: ''
})

const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.titulo) erros.titulo = 'Título é obrigatório.'
  if (!form.tipoTreinamentoId) erros.tipoTreinamentoId = 'Tipo de treinamento (UUID) é obrigatório.'
  if (!form.treinadorId) erros.treinadorId = 'Treinador (UUID) é obrigatório.'
  if (!form.dataInicio) erros.dataInicio = 'Data início é obrigatória.'
  if (!form.dataFim) erros.dataFim = 'Data fim é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/treinamento/treinamentos', {
      method: 'POST',
      body: { ...form, capacidadeMaxima: form.capacidadeMaxima != null ? Number(form.capacidadeMaxima) : null }
    })
    toast.success('Treinamento criado com sucesso!')
    router.push('/erp/rh/treinamento/treinamentos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
function cancelar() {
  router.push('/erp/rh/treinamento/treinamentos')
}
</script>

<template>
  <div>
    <PageToolbar title="Novo treinamento">
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
          <TextField v-model="form.titulo" label="Título" required maxlength="150" :error="erros.titulo" />
          <TextField v-model="form.local" label="Local" maxlength="120" />
          <!-- TODO: sem endpoints de listagem no digest — UUID manual. -->
          <TextField v-model="form.tipoTreinamentoId" label="Tipo de treinamento (UUID)" required :error="erros.tipoTreinamentoId" placeholder="UUID" />
          <TextField v-model="form.treinadorId" label="Treinador (UUID)" required :error="erros.treinadorId" placeholder="UUID" />
          <TextField v-model="form.filialId" label="Filial (UUID)" placeholder="UUID" />
          <TextField v-model="form.departamentoId" label="Departamento (UUID)" placeholder="UUID" />
          <DateTimeField v-model="form.dataInicio" label="Data início" required :error="erros.dataInicio" />
          <DateTimeField v-model="form.dataFim" label="Data fim" required :error="erros.dataFim" />
          <TextField v-model="form.horaInicio" label="Hora início" placeholder="HH:mm:ss" />
          <TextField v-model="form.horaFim" label="Hora fim" placeholder="HH:mm:ss" />
          <TextField v-model="form.capacidadeMaxima" label="Capacidade máxima" type="number" />
          <MoneyInput v-model="form.custo" label="Custo" />
          <TextField v-model="form.criadoPorUsuarioId" label="Criado por (UUID)" placeholder="UUID" />
          <TextField v-model="form.donoFuncionalId" label="Dono funcional (UUID)" placeholder="UUID" />
          <TextField v-model="form.descricao" label="Descrição" maxlength="500" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
