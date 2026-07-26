<script setup lang="ts">
/**
 * Formulário de Plano de Inspeção (QLD-INS) — criação.
 *
 * Fonte: POST /qualidade/planos-inspecao. Sem GET detalhe/PUT no backend → tela
 * somente de criação (rota `/novo`).
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
import { CONTEXTO_PLANO_OPCOES } from '~/components/qualidade-shared/enums'

definePageMeta({ layout: 'default' })

interface PlanoForm {
  codigo: string | null
  descricao: string | null
  contexto: number | null
  responsavelId: string | null
  produtoId: string | null
  processoId: string | null
  etapaId: string | null
  dataInicioVigencia: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isNovo = computed(() => idParam === 'novo')
const salvando = ref(false)

const form = reactive<PlanoForm>({
  codigo: null,
  descricao: null,
  contexto: null,
  responsavelId: null,
  produtoId: null,
  processoId: null,
  etapaId: null,
  dataInicioVigencia: null
})

const erros = reactive<Record<string, string>>({})

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()
  if (!form.codigo) erros.codigo = 'O código é obrigatório.'
  if (!form.descricao) erros.descricao = 'A descrição é obrigatória.'
  if (form.contexto == null) erros.contexto = 'O contexto é obrigatório.'
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
    await useApi('/qualidade/planos-inspecao', { method: 'POST', body: form })
    toast.success('Plano de inspeção criado com sucesso!')
    router.push('/erp/qualidade/planos-inspecao')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/qualidade/planos-inspecao')
}
</script>

<template>
  <div>
    <PageToolbar title="Novo plano de inspeção">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando || !isNovo" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div v-if="!isNovo" class="glass-panel form-panel">
      <p>A edição de planos não está disponível (o backend só expõe criação). Volte para a
        <a href="/erp/qualidade/planos-inspecao">listagem</a>.</p>
    </div>

    <div v-else class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" required maxlength="30" :error="erros.codigo" />
          <SelectField v-model="form.contexto" label="Contexto" required :options="CONTEXTO_PLANO_OPCOES" :error="erros.contexto" />
          <DateTimeField v-model="form.dataInicioVigencia" label="Início da Vigência" mode="datetime" />
          <!-- FKs uuid; sem endpoints no digest → inputs de texto. TODO: SelectField quando houver rotas. -->
          <TextField v-model="form.responsavelId" label="Responsável (ID)" required placeholder="UUID do responsável" :error="erros.responsavelId" />
          <TextField v-model="form.produtoId" label="Produto (ID)" placeholder="UUID do produto (opcional)" />
          <TextField v-model="form.processoId" label="Processo (ID)" placeholder="UUID do processo (opcional)" />
          <TextField v-model="form.etapaId" label="Etapa (ID)" placeholder="UUID da etapa (opcional)" />
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
