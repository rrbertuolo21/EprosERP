<script setup lang="ts">
/**
 * Formulário de Atributo (QLD-ATR) — criação.
 *
 * Fonte: POST /qualidade/atributos. Sem GET detalhe/PUT → somente criação.
 */
import { ref, computed, reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import {
  TIPO_ATRIBUTO_OPCOES,
  TIPO_DADO_ATRIBUTO_OPCOES,
  ESCOPO_ATRIBUTO_OPCOES,
  TIPO_CARACTERISTICA_OPCOES
} from '~/components/qualidade-shared/enums'

definePageMeta({ layout: 'default' })

interface AtributoForm {
  codigo: string | null
  nomeInterno: string | null
  rotulo: string | null
  tipoAtributo: number | null
  tipoDado: number | null
  escopo: number | null
  exibirFormularioPadrao: boolean
  obrigatorio: boolean
  tipoCaracteristica: number | null
  sensivelLgpd: boolean
  posicao: number | null
  responsavelId: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isNovo = computed(() => idParam === 'novo')
const salvando = ref(false)

const form = reactive<AtributoForm>({
  codigo: null,
  nomeInterno: null,
  rotulo: null,
  tipoAtributo: null,
  tipoDado: null,
  escopo: null,
  exibirFormularioPadrao: false,
  obrigatorio: false,
  tipoCaracteristica: null,
  sensivelLgpd: false,
  posicao: null,
  responsavelId: null
})

const erros = reactive<Record<string, string>>({})

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()
  if (!form.codigo) erros.codigo = 'O código é obrigatório.'
  if (!form.nomeInterno) erros.nomeInterno = 'O nome interno é obrigatório.'
  if (!form.rotulo) erros.rotulo = 'O rótulo é obrigatório.'
  if (form.tipoAtributo == null) erros.tipoAtributo = 'O tipo de atributo é obrigatório.'
  if (form.tipoDado == null) erros.tipoDado = 'O tipo de dado é obrigatório.'
  if (form.escopo == null) erros.escopo = 'O escopo é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    const posicaoTexto = form.posicao === null ? '' : String(form.posicao)
    const body = {
      ...form,
      posicao: posicaoTexto.trim() === '' ? null : Number(posicaoTexto)
    }
    await useApi('/qualidade/atributos', { method: 'POST', body })
    toast.success('Atributo criado com sucesso!')
    router.push('/erp/qualidade/atributos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/qualidade/atributos')
}
</script>

<template>
  <div>
    <PageToolbar title="Novo atributo">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando || !isNovo" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div v-if="!isNovo" class="glass-panel form-panel">
      <p>A edição não está disponível (o backend só expõe criação). Volte para a
        <a href="/erp/qualidade/atributos">listagem</a>.</p>
    </div>

    <div v-else class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" required maxlength="30" :error="erros.codigo" />
          <TextField v-model="form.nomeInterno" label="Nome interno" required maxlength="100" :error="erros.nomeInterno" />
          <TextField v-model="form.rotulo" label="Rótulo" required maxlength="255" :error="erros.rotulo" />
          <SelectField v-model="form.tipoAtributo" label="Tipo de atributo" required :options="TIPO_ATRIBUTO_OPCOES" :error="erros.tipoAtributo" />
          <SelectField v-model="form.tipoDado" label="Tipo de dado" required :options="TIPO_DADO_ATRIBUTO_OPCOES" :error="erros.tipoDado" />
          <SelectField v-model="form.escopo" label="Escopo" required :options="ESCOPO_ATRIBUTO_OPCOES" :error="erros.escopo" />
          <SelectField v-model="form.tipoCaracteristica" label="Tipo de característica" :options="TIPO_CARACTERISTICA_OPCOES" />
          <TextField v-model="form.posicao" label="Posição" type="number" placeholder="Ordem de exibição" />
          <!-- responsavelId é uuid; sem endpoint no digest → input de texto. TODO: SelectField quando houver rota. -->
          <TextField v-model="form.responsavelId" label="Responsável (ID)" placeholder="UUID do responsável (opcional)" />
        </div>

        <div class="toggles">
          <label class="toggle-row">
            <input v-model="form.exibirFormularioPadrao" type="checkbox" />
            <span class="field-label">Exibir no formulário padrão</span>
          </label>
          <label class="toggle-row">
            <input v-model="form.obrigatorio" type="checkbox" />
            <span class="field-label">Obrigatório</span>
          </label>
          <label class="toggle-row">
            <input v-model="form.sensivelLgpd" type="checkbox" />
            <span class="field-label">Sensível (LGPD)</span>
          </label>
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
.toggles { margin-top: 20px; display: flex; flex-wrap: wrap; gap: 24px; }
.toggle-row { display: flex; align-items: center; gap: 10px; cursor: pointer; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
</style>
