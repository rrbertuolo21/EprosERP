<script setup lang="ts">
/**
 * Novo turno — RH / Planejamento.
 * Fonte: POST /rh/planejamento/turnos. Criação apenas.
 * As horas (horaInicio/horaFim/intervalo*) são TimeSpan (HH:mm:ss) — enviadas como texto;
 * não há componente de hora compartilhado. criadoPorId/ownerId são UUID manual.
 */
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface TurnoForm {
  nome: string | null
  horaInicio: string | null
  horaFim: string | null
  intervaloInicio: string | null
  intervaloFim: string | null
  turnoNoturno: boolean
  criadoPorId: string | null
  ownerId: string | null
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const form = reactive<TurnoForm>({
  nome: null,
  horaInicio: null,
  horaFim: null,
  intervaloInicio: null,
  intervaloFim: null,
  turnoNoturno: false,
  criadoPorId: null,
  ownerId: null
})

const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.nome) erros.nome = 'Nome é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/planejamento/turnos', { method: 'POST', body: form })
    toast.success('Turno criado com sucesso!')
    router.push('/erp/rh/planejamento/turnos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
function cancelar() {
  router.push('/erp/rh/planejamento/turnos')
}
</script>

<template>
  <div>
    <PageToolbar title="Novo turno">
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
          <TextField v-model="form.nome" label="Nome" required maxlength="80" :error="erros.nome" />
          <TextField v-model="form.horaInicio" label="Hora início" placeholder="HH:mm:ss" />
          <TextField v-model="form.horaFim" label="Hora fim" placeholder="HH:mm:ss" />
          <TextField v-model="form.intervaloInicio" label="Intervalo início" placeholder="HH:mm:ss" />
          <TextField v-model="form.intervaloFim" label="Intervalo fim" placeholder="HH:mm:ss" />
          <label class="field toggle-row">
            <span class="field-label">{{ form.turnoNoturno ? 'Turno noturno' : 'Turno diurno' }}</span>
            <input v-model="form.turnoNoturno" type="checkbox" />
          </label>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.toggle-row { display: flex; align-items: center; gap: 10px; justify-content: flex-start; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
</style>
