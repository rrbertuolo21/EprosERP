<script setup lang="ts">
/**
 * Nova Indução de Equipamento — Manutenção / Configuração / Induções.
 * POST /manutencao/equipamentos-config/inducoes. A API não expõe GET/{id}; edição não é suportada.
 */
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import type { SelectOption } from '~/composables/useEnum'
import { carregarEquipamentoOpcoes } from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface InducaoForm {
  equipamentoId: string
  dataInicio: string | null
  responsavelId: string | null
  checklistJson: string | null
  observacao: string | null
}

const router = useRouter()
const toast = useToast()
const salvando = ref(false)
const erros = reactive<Record<string, string>>({})
const equipamentoOpcoes = ref<SelectOption[]>([])

const form = reactive<InducaoForm>({
  equipamentoId: '',
  dataInicio: null,
  responsavelId: null,
  checklistJson: null,
  observacao: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.equipamentoId) erros.equipamentoId = 'Equipamento é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/manutencao/equipamentos-config/inducoes', { method: 'POST', body: form })
    toast.success('Indução registrada com sucesso!')
    router.push('/erp/manutencao/equipamentos-config/inducoes')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/manutencao/equipamentos-config/inducoes')
}

onMounted(async () => {
  equipamentoOpcoes.value = await carregarEquipamentoOpcoes()
})
</script>

<template>
  <div>
    <PageToolbar title="Nova indução de equipamento">
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
          <SelectField
            v-if="equipamentoOpcoes.length > 0"
            v-model="form.equipamentoId"
            label="Equipamento"
            required
            :options="equipamentoOpcoes"
            :error="erros.equipamentoId"
          />
          <TextField v-else v-model="form.equipamentoId" label="Equipamento (ID)" required placeholder="UUID" :error="erros.equipamentoId" />
          <DateTimeField v-model="form.dataInicio" label="Data de início" mode="datetime" />
          <!-- TODO: responsavelId sem endpoint de listagem — texto até integração. -->
          <TextField v-model="form.responsavelId" label="Responsável (ID)" placeholder="UUID" />
          <TextField v-model="form.observacao" label="Observação" maxlength="500" />
          <TextField v-model="form.checklistJson" label="Checklist (JSON)" placeholder='{"itens":[]}' />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
