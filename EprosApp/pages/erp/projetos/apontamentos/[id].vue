<script setup lang="ts">
/**
 * Apontamento — formulário de criação (o contrato só expõe POST; sem GET/{id} nem PUT).
 * POST /projetos/recursos/apontamentos.
 */
import { ref, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { TIMESHEET_TIPO_OPCOES } from '~/components/projetos-shared/statusWorkflow'

definePageMeta({ layout: 'default' })

const route = useRoute()
const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const form = reactive({
  usuarioId: '', projetoId: '', tarefaId: '',
  data: null as string | null, horas: 0, minutos: 0, notas: '', tipo: 1 as number | null
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.data) erros.data = 'Data é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) { toast.error('Formulário possui erros de validação.'); return }
  salvando.value = true
  try {
    await useApi('/projetos/recursos/apontamentos', {
      method: 'POST',
      body: {
        usuarioId: form.usuarioId || null, projetoId: form.projetoId || null, tarefaId: form.tarefaId || null,
        data: form.data, horas: form.horas, minutos: form.minutos, notas: form.notas || null, tipo: form.tipo
      }
    })
    toast.success('Apontamento registrado com sucesso!')
    router.push('/erp/projetos/apontamentos')
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { salvando.value = false }
}
function voltar() { router.push('/erp/projetos/apontamentos') }

onMounted(() => {
  // Não há endpoint de edição de apontamento; qualquer id diferente de "novo" volta à lista.
  if ((route.params.id as string) !== 'novo') voltar()
})
</script>

<template>
  <div>
    <PageToolbar title="Novo apontamento">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="voltar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span><span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>
    <div class="glass-panel form-panel">
      <div class="form-grid">
        <DateTimeField v-model="form.data" label="Data" mode="datetime" required :error="erros.data" />
        <SelectField v-model="form.tipo" label="Tipo" :options="TIMESHEET_TIPO_OPCOES" :clearable="false" />
        <QuantityInput v-model="form.horas" label="Horas" :decimais="0" />
        <QuantityInput v-model="form.minutos" label="Minutos" :decimais="0" />
        <TextField v-model="form.projetoId" label="Projeto (ID)" hint="UUID (opcional)" />
        <TextField v-model="form.tarefaId" label="Tarefa (ID)" hint="UUID (opcional)" />
        <TextField v-model="form.usuarioId" label="Usuário (ID)" hint="UUID (opcional)" />
        <TextField v-model="form.notas" label="Notas" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
