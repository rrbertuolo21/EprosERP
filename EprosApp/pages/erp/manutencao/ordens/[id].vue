<script setup lang="ts">
/**
 * Nova Ordem de Manutenção — Manutenção / Ordens.
 * POST /manutencao/ordens (equipamentoId, tipo, descricaoProblema).
 * A API não expõe GET/{id} nem PUT/DELETE — esta tela cobre apenas a abertura.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'
import { carregarEquipamentoOpcoes } from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface OrdemForm {
  equipamentoId: string
  tipo: string
  descricaoProblema: string
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const equipamentoOpcoes = ref<SelectOption[]>([])
const erros = reactive<Record<string, string>>({})

const tipoOpcoes: SelectOption[] = [
  { value: 'Corretiva', label: 'Corretiva' },
  { value: 'Preventiva', label: 'Preventiva' },
  { value: 'Preditiva', label: 'Preditiva' }
]

const form = reactive<OrdemForm>({
  equipamentoId: '',
  tipo: 'Corretiva',
  descricaoProblema: ''
})

const usarSelectEquipamento = computed(() => equipamentoOpcoes.value.length > 0)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.equipamentoId) erros.equipamentoId = 'Equipamento é obrigatório.'
  if (!form.tipo) erros.tipo = 'Tipo é obrigatório.'
  if (!form.descricaoProblema) erros.descricaoProblema = 'Descrição do problema é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/manutencao/ordens', { method: 'POST', body: form })
    toast.success('Ordem aberta com sucesso!')
    router.push('/erp/manutencao/ordens')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/manutencao/ordens')
}

onMounted(async () => {
  equipamentoOpcoes.value = await carregarEquipamentoOpcoes()
})
</script>

<template>
  <div>
    <PageToolbar title="Nova ordem de manutenção">
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
            v-if="usarSelectEquipamento"
            v-model="form.equipamentoId"
            label="Equipamento"
            required
            :options="equipamentoOpcoes"
            :error="erros.equipamentoId"
          />
          <TextField v-else v-model="form.equipamentoId" label="Equipamento (ID)" required placeholder="UUID" :error="erros.equipamentoId" />
          <SelectField v-model="form.tipo" label="Tipo" required :options="tipoOpcoes" :clearable="false" :error="erros.tipo" />
          <TextField v-model="form.descricaoProblema" label="Descrição do problema" required maxlength="500" :error="erros.descricaoProblema" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
}
</style>
