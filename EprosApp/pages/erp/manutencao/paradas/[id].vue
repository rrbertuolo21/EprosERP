<script setup lang="ts">
/**
 * Parada (nova/detalhe) — Manutenção / Paradas.
 * - novo: POST /manutencao/paradas
 * - edição: GET /manutencao/paradas/{id} (somente leitura dos dados de abertura; a finalização
 *   é feita na listagem via ação Finalizar).
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import type { SelectOption } from '~/composables/useEnum'
import { tipoParadaOpcoes, carregarEquipamentoOpcoes } from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface ParadaForm {
  codigo: string
  descricao: string
  responsavelId: string
  tipoParada: number
  dataHoraInicio: string | null
  usuarioRegistroId: string
  equipamentoId: string | null
  linhaId: string | null
  celulaId: string | null
  motivoParadaId: string | null
  observacao: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const erros = reactive<Record<string, string>>({})
const equipamentoOpcoes = ref<SelectOption[]>([])

const form = reactive<ParadaForm>({
  codigo: '',
  descricao: '',
  responsavelId: '',
  tipoParada: 0,
  dataHoraInicio: null,
  usuarioRegistroId: '',
  equipamentoId: null,
  linhaId: null,
  celulaId: null,
  motivoParadaId: null,
  observacao: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.descricao) erros.descricao = 'Descrição é obrigatória.'
  if (!form.responsavelId) erros.responsavelId = 'Responsável é obrigatório.'
  if (!form.dataHoraInicio) erros.dataHoraInicio = 'Data/hora de início é obrigatória.'
  if (!form.usuarioRegistroId) erros.usuarioRegistroId = 'Usuário de registro é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/manutencao/paradas/${idParam}`)
    const dados = extrairDados<Partial<ParadaForm>>(resposta)
    if (dados) Object.assign(form, dados)
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/manutencao/paradas', { method: 'POST', body: form })
    toast.success('Parada registrada com sucesso!')
    router.push('/erp/manutencao/paradas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/manutencao/paradas')
}

const usarSelectEquip = computed(() => equipamentoOpcoes.value.length > 0)

onMounted(async () => {
  equipamentoOpcoes.value = await carregarEquipamentoOpcoes()
  await carregar()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? `Parada ${form.codigo}` : 'Nova parada'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <button v-if="!isEdit" type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" maxlength="30" :disabled="isEdit" />
          <TextField v-model="form.descricao" label="Descrição" required maxlength="200" :error="erros.descricao" :disabled="isEdit" />
          <SelectField v-model="form.tipoParada" label="Tipo de parada" required :options="tipoParadaOpcoes" :clearable="false" :disabled="isEdit" />
          <DateTimeField v-model="form.dataHoraInicio" label="Início" mode="datetime" required :error="erros.dataHoraInicio" :disabled="isEdit" />
          <SelectField
            v-if="usarSelectEquip"
            v-model="form.equipamentoId"
            label="Equipamento"
            :options="equipamentoOpcoes"
            :disabled="isEdit"
          />
          <TextField v-else v-model="form.equipamentoId" label="Equipamento (ID)" placeholder="UUID" :disabled="isEdit" />
          <!-- TODO: uuids sem endpoint de listagem no módulo — texto até integração. -->
          <TextField v-model="form.responsavelId" label="Responsável (ID)" required placeholder="UUID" :error="erros.responsavelId" :disabled="isEdit" />
          <TextField v-model="form.usuarioRegistroId" label="Usuário registro (ID)" required placeholder="UUID" :error="erros.usuarioRegistroId" :disabled="isEdit" />
          <TextField v-model="form.linhaId" label="Linha (ID)" placeholder="UUID" :disabled="isEdit" />
          <TextField v-model="form.celulaId" label="Célula (ID)" placeholder="UUID" :disabled="isEdit" />
          <TextField v-model="form.motivoParadaId" label="Motivo (ID)" placeholder="UUID" :disabled="isEdit" />
          <TextField v-model="form.observacao" label="Observação" maxlength="500" :disabled="isEdit" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
