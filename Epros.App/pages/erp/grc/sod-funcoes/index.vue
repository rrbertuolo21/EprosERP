<script setup lang="ts">
/**
 * Cadastro de Função SoD — GRC / Segregação de Funções.
 *
 * Fonte: POST /api/v1/grc/sod/funcoes. O backend não expõe GET de funções,
 * portanto esta é uma tela de cadastro (sem listagem) — anotado no relatório como lacuna.
 */
import { reactive, ref } from 'vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface FuncaoForm {
  codigo: string | null
  nome: string | null
  descricao: string | null
}

const toast = useToast()

const form = reactive<FuncaoForm>({ codigo: null, nome: null, descricao: null })
const erros = reactive<Record<string, string>>({})
const salvando = ref(false)

function limparForm() {
  form.codigo = null
  form.nome = null
  form.descricao = null
}

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
    await useApi('/grc/sod/funcoes', { method: 'POST', body: form })
    toast.success('Função SoD cadastrada com sucesso!')
    limparForm()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
</script>

<template>
  <div>
    <PageToolbar title="Funções SoD" subtitle="Cadastro de funções para regras de segregação (o backend não expõe listagem)">
      <template #actions>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" maxlength="50" />
          <TextField v-model="form.nome" label="Nome" required :error="erros.nome" maxlength="200" />
          <div class="span-2">
            <TextField v-model="form.descricao" label="Descrição" maxlength="1000" />
          </div>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.span-2 { grid-column: 1 / -1; }
</style>
