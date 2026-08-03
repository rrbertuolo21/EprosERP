<script setup lang="ts">
/**
 * Equipamento (novo/configuração) — Manutenção / Equipamentos.
 *
 * - novo: POST /manutencao/equipamentos (dados básicos de criação).
 * - edição: PUT /manutencao/equipamentos-config/equipamentos/{id}/configuracao (configuração
 *   técnica do equipamento). Observação: a API não expõe GET /equipamentos/{id}, portanto o
 *   formulário de configuração não é pré-preenchido — apenas grava a configuração informada.
 */
import { ref, computed, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { criticidadeOpcoes } from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface EquipamentoNovoForm {
  nome: string
  codigo: string
  setor: string
  dataAquisicao: string | null
  criticidade: string
}

interface EquipamentoConfigForm {
  equipamentoId: string
  descricao: string | null
  tipoEquipamentoId: string | null
  marcaId: string | null
  numeroSerie: string | null
  funcaoOperacional: string | null
  estadoConservacaoId: string | null
  responsavelId: string | null
  localId: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')
const salvando = ref(false)
const erros = reactive<Record<string, string>>({})

const formNovo = reactive<EquipamentoNovoForm>({
  nome: '',
  codigo: '',
  setor: '',
  dataAquisicao: null,
  criticidade: 'Media'
})

const formConfig = reactive<EquipamentoConfigForm>({
  equipamentoId: isEdit.value ? idParam : '',
  descricao: null,
  tipoEquipamentoId: null,
  marcaId: null,
  numeroSerie: null,
  funcaoOperacional: null,
  estadoConservacaoId: null,
  responsavelId: null,
  localId: null
})

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()
  if (!isEdit.value) {
    if (!formNovo.nome) erros.nome = 'Nome é obrigatório.'
    if (!formNovo.codigo) erros.codigo = 'Código é obrigatório.'
    if (!formNovo.dataAquisicao) erros.dataAquisicao = 'Data de aquisição é obrigatória.'
  }
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    if (isEdit.value) {
      await useApi(`/manutencao/equipamentos-config/equipamentos/${idParam}/configuracao`, {
        method: 'PUT',
        body: formConfig
      })
    } else {
      await useApi('/manutencao/equipamentos', { method: 'POST', body: formNovo })
    }
    toast.success('Registro salvo com sucesso!')
    router.push('/erp/manutencao/equipamentos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/manutencao/equipamentos')
}

onMounted(() => {
  // API não expõe GET /equipamentos/{id}; nada a pré-carregar.
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Configuração do equipamento' : 'Novo equipamento'">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <p v-if="isEdit" class="aviso">
        Configuração técnica do equipamento. Os dados básicos (nome, código) são definidos na criação.
      </p>
      <form class="vertical-form" @submit.prevent="salvar">
        <div v-if="!isEdit" class="form-grid">
          <TextField v-model="formNovo.codigo" label="Código" required maxlength="30" :error="erros.codigo" />
          <TextField v-model="formNovo.nome" label="Nome" required maxlength="120" :error="erros.nome" />
          <TextField v-model="formNovo.setor" label="Setor" maxlength="80" />
          <DateTimeField v-model="formNovo.dataAquisicao" label="Data de aquisição" required :error="erros.dataAquisicao" />
          <SelectField v-model="formNovo.criticidade" label="Criticidade" :options="criticidadeOpcoes" :clearable="false" />
        </div>

        <div v-else class="form-grid">
          <TextField v-model="formConfig.descricao" label="Descrição" maxlength="200" />
          <TextField v-model="formConfig.numeroSerie" label="Número de série" maxlength="60" />
          <TextField v-model="formConfig.funcaoOperacional" label="Função operacional" maxlength="120" />
          <!-- TODO: uuids sem endpoint de listagem no módulo — entrada por texto até integração. -->
          <TextField v-model="formConfig.tipoEquipamentoId" label="Tipo de equipamento (ID)" placeholder="UUID" hint="Identificador do tipo de equipamento" />
          <TextField v-model="formConfig.marcaId" label="Marca (ID)" placeholder="UUID" />
          <TextField v-model="formConfig.estadoConservacaoId" label="Estado de conservação (ID)" placeholder="UUID" />
          <TextField v-model="formConfig.responsavelId" label="Responsável (ID)" placeholder="UUID" />
          <TextField v-model="formConfig.localId" label="Local (ID)" placeholder="UUID" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.aviso { color: var(--text-secondary); font-size: 13px; margin-bottom: 16px; }
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
}
</style>
