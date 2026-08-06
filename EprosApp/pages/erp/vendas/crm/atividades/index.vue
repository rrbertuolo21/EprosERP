<script setup lang="ts">
/**
 * CRM Comercial — Atividades / Agenda.
 * Contrato real: base `/vendas/crm` — o backend expõe apenas comandos (sem GET de listagem):
 *   POST atividades                 (CriarCrmAtividadeCommand)
 *   POST atividades/{id}/concluir    (ConcluirCrmAtividadeCommand: { id, resultado })
 * Por isso a tela é operacional (registrar / concluir), não uma listagem. Apresentação — sem regra nova.
 */
import { reactive, ref } from 'vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useAuth } from '~/composables/useAuth'

definePageMeta({ layout: 'default', middleware: 'auth' })

const ENTIDADE = [
  { value: 0, label: 'Lead' },
  { value: 1, label: 'Oportunidade' },
  { value: 2, label: 'Ticket' },
  { value: 3, label: 'Cliente' },
  { value: 4, label: 'Campanha' }
]
const TIPO = [
  { value: 0, label: 'Tarefa' },
  { value: 1, label: 'Chamada' },
  { value: 2, label: 'Reunião' },
  { value: 3, label: 'E-mail' },
  { value: 4, label: 'Nota' }
]
const PRIORIDADE = [
  { value: 0, label: 'Baixa' },
  { value: 1, label: 'Média' },
  { value: 2, label: 'Alta' },
  { value: 4, label: 'Urgente' }
]

const toast = useToast()
const { getUserId } = useAuth()

const salvando = ref(false)
const form = reactive({
  entidadeTipo: 0 as number,
  tipoAtividade: 0 as number,
  nome: '',
  assunto: '',
  data: null as string | null,
  hora: '',
  prioridade: 1 as number,
  descricao: '',
  entidadeId: ''
})
const erros = reactive<Record<string, string>>({})

async function registrar() {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.nome && !form.assunto) erros.nome = 'Informe nome ou assunto.'
  if (Object.keys(erros).length) return
  salvando.value = true
  try {
    const id = form.entidadeId || null
    await useApi('/vendas/crm/atividades', {
      method: 'POST',
      body: {
        entidadeTipo: form.entidadeTipo,
        tipoAtividade: form.tipoAtividade,
        leadId: form.entidadeTipo === 0 ? id : null,
        oportunidadeId: form.entidadeTipo === 1 ? id : null,
        ticketId: form.entidadeTipo === 2 ? id : null,
        campanhaId: form.entidadeTipo === 4 ? id : null,
        nome: form.nome || null,
        assunto: form.assunto || null,
        data: form.data,
        hora: form.hora || null,
        prioridade: form.prioridade,
        descricao: form.descricao || null,
        usuarioId: getUserId()
      }
    })
    toast.success('Atividade registrada.')
    form.nome = ''
    form.assunto = ''
    form.descricao = ''
    form.entidadeId = ''
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

// Concluir por id
const concluir = reactive({ id: '', resultado: '' })
const concluindo = ref(false)
async function concluirAtividade() {
  if (!concluir.id) {
    toast.warning('Informe o id da atividade.')
    return
  }
  concluindo.value = true
  try {
    await useApi('/vendas/crm/atividades/{id}/concluir', {
      method: 'POST',
      params: { id: concluir.id },
      body: { id: concluir.id, resultado: concluir.resultado || null }
    })
    toast.success('Atividade concluída.')
    concluir.id = ''
    concluir.resultado = ''
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    concluindo.value = false
  }
}
</script>

<template>
  <div>
    <PageToolbar title="Atividades" subtitle="CRM Comercial — agenda e tarefas" />

    <div class="glass-panel form-panel">
      <h3 class="secao-titulo">Nova atividade</h3>
      <div class="form-grid">
        <SelectField v-model="form.entidadeTipo" label="Vínculo" :options="ENTIDADE" :clearable="false" />
        <TextField v-model="form.entidadeId" label="ID do vínculo" hint="Id do lead/oportunidade/ticket/campanha (opcional)." />
        <SelectField v-model="form.tipoAtividade" label="Tipo" :options="TIPO" :clearable="false" />
        <TextField v-model="form.nome" label="Nome" :error="erros.nome" />
        <TextField v-model="form.assunto" label="Assunto" />
        <DateTimeField v-model="form.data" label="Data" mode="date" />
        <TextField v-model="form.hora" label="Hora" placeholder="HH:mm" />
        <SelectField v-model="form.prioridade" label="Prioridade" :options="PRIORIDADE" :clearable="false" />
        <TextField v-model="form.descricao" label="Descrição" />
      </div>
      <div class="acoes">
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="registrar">
          <span v-if="salvando" class="spinner"></span><span v-else>Registrar atividade</span>
        </button>
      </div>
    </div>

    <div class="glass-panel form-panel">
      <h3 class="secao-titulo">Concluir atividade</h3>
      <div class="form-grid">
        <TextField v-model="concluir.id" label="ID da atividade" />
        <TextField v-model="concluir.resultado" label="Resultado" />
      </div>
      <div class="acoes">
        <button type="button" class="btn btn-secondary" :disabled="concluindo" @click="concluirAtividade">
          <span v-if="concluindo" class="spinner"></span><span v-else>Concluir</span>
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.secao-titulo { font-size: 15px; margin-bottom: 14px; }
.acoes { display: flex; justify-content: flex-end; margin-top: 16px; }
</style>
