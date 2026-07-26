<script setup lang="ts">
/**
 * Formulário de Não Conformidade (QLD-NCR) — criação.
 *
 * Fonte: POST /qualidade/ncr. O backend NÃO expõe GET detalhe nem PUT, logo esta
 * tela é somente criação (rota `/novo`). Ao abrir um id existente, avisamos que a
 * edição não está disponível.
 */
import { ref, computed, reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { NCR_ORIGEM_OPCOES, NCR_PRIORIDADE_OPCOES } from '~/components/qualidade-shared/enums'

definePageMeta({ layout: 'default' })

interface NcrForm {
  codigo: string | null
  titulo: string | null
  descricao: string | null
  origemPrincipal: number | null
  prioridade: number | null
  responsavelId: string | null
  severidade: string | null
  dataOcorrencia: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isNovo = computed(() => idParam === 'novo')

const salvando = ref(false)

const form = reactive<NcrForm>({
  codigo: null,
  titulo: null,
  descricao: null,
  origemPrincipal: null,
  prioridade: null,
  responsavelId: null,
  severidade: null,
  dataOcorrencia: null
})

const erros = reactive<Record<string, string>>({})

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()
  if (!form.codigo) erros.codigo = 'O código é obrigatório.'
  if (!form.titulo) erros.titulo = 'O título é obrigatório.'
  if (!form.descricao) erros.descricao = 'A descrição é obrigatória.'
  if (form.origemPrincipal == null) erros.origemPrincipal = 'A origem principal é obrigatória.'
  if (form.prioridade == null) erros.prioridade = 'A prioridade é obrigatória.'
  if (!form.responsavelId) erros.responsavelId = 'O responsável é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/qualidade/ncr', { method: 'POST', body: form })
    toast.success('NCR criada com sucesso!')
    router.push('/erp/qualidade/ncr')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/qualidade/ncr')
}
</script>

<template>
  <div>
    <PageToolbar title="Nova NCR">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando || !isNovo" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div v-if="!isNovo" class="glass-panel form-panel">
      <p>A edição de NCRs não está disponível (o backend só expõe criação). Volte para a
        <a href="/erp/qualidade/ncr">listagem</a>.</p>
    </div>

    <div v-else class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" required maxlength="30" :error="erros.codigo" />
          <TextField v-model="form.titulo" label="Título" required maxlength="255" :error="erros.titulo" />
          <SelectField v-model="form.origemPrincipal" label="Origem Principal" required :options="NCR_ORIGEM_OPCOES" :error="erros.origemPrincipal" />
          <SelectField v-model="form.prioridade" label="Prioridade" required :options="NCR_PRIORIDADE_OPCOES" :error="erros.prioridade" />
          <TextField v-model="form.severidade" label="Severidade" maxlength="60" hint="Campo livre (domínio não definido no backend)." />
          <DateTimeField v-model="form.dataOcorrencia" label="Data da Ocorrência" mode="datetime" />
          <!-- responsavelId é uuid; sem endpoint de responsáveis no digest → input de texto. TODO: trocar por SelectField quando houver rota de usuários. -->
          <TextField v-model="form.responsavelId" label="Responsável (ID)" required placeholder="UUID do responsável" :error="erros.responsavelId" />
        </div>
        <div class="form-full">
          <label class="field-label">Descrição<span class="required">*</span></label>
          <textarea v-model="form.descricao" class="input textarea" rows="4" :class="{ 'is-invalid': !!erros.descricao }"></textarea>
          <span v-if="erros.descricao" class="field-error">{{ erros.descricao }}</span>
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
.form-full { margin-top: 16px; display: flex; flex-direction: column; gap: 6px; }
.textarea { min-height: 96px; resize: vertical; font-family: inherit; }
</style>
