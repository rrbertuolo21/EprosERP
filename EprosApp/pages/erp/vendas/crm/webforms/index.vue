<script setup lang="ts">
/**
 * CRM Comercial — Webforms (formulários de captação de leads).
 * Contrato real: POST `/vendas/crm/webforms` (CriarCrmWebformCommand). Sem GET de listagem.
 * Tela de criação. Apresentação — sem regra nova.
 */
import { reactive, ref } from 'vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'

definePageMeta({ layout: 'default', middleware: 'auth' })

const toast = useToast()
const salvando = ref(false)
const form = reactive({
  identificadorUnico: '',
  titulo: '',
  estruturaJson: '[]',
  mensagemAgradecimento: '',
  textoBotao: 'Enviar',
  leadTituloPadrao: '',
  usarCaptcha: false,
  notificarAdministrador: false
})
const erros = reactive<Record<string, string>>({})

async function salvar() {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.identificadorUnico) erros.identificadorUnico = 'Identificador é obrigatório.'
  if (!form.titulo) erros.titulo = 'Título é obrigatório.'
  if (!form.estruturaJson) erros.estruturaJson = 'Estrutura (JSON) é obrigatória.'
  if (Object.keys(erros).length) return
  salvando.value = true
  try {
    await useApi('/vendas/crm/webforms', {
      method: 'POST',
      body: {
        identificadorUnico: form.identificadorUnico,
        titulo: form.titulo,
        estruturaJson: form.estruturaJson,
        mensagemAgradecimento: form.mensagemAgradecimento || null,
        textoBotao: form.textoBotao || null,
        leadTituloPadrao: form.leadTituloPadrao || null,
        usarCaptcha: form.usarCaptcha,
        notificarAdministrador: form.notificarAdministrador
      }
    })
    toast.success('Webform criado.')
    form.identificadorUnico = ''
    form.titulo = ''
    form.estruturaJson = '[]'
    form.mensagemAgradecimento = ''
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
</script>

<template>
  <div>
    <PageToolbar title="Webforms" subtitle="CRM Comercial — formulários de captação" />
    <div class="glass-panel form-panel">
      <div class="form-grid">
        <TextField v-model="form.identificadorUnico" label="Identificador único" required :error="erros.identificadorUnico" hint="Slug usado na URL pública do formulário." />
        <TextField v-model="form.titulo" label="Título" required :error="erros.titulo" />
        <TextField v-model="form.textoBotao" label="Texto do botão" />
        <TextField v-model="form.leadTituloPadrao" label="Título padrão do lead" />
        <TextField v-model="form.estruturaJson" label="Estrutura (JSON)" required :error="erros.estruturaJson" hint="Definição dos campos do formulário em JSON." />
        <TextField v-model="form.mensagemAgradecimento" label="Mensagem de agradecimento" />
        <label class="field toggle-row">
          <span class="field-label">Usar captcha</span>
          <input v-model="form.usarCaptcha" type="checkbox" />
        </label>
        <label class="field toggle-row">
          <span class="field-label">Notificar administrador</span>
          <input v-model="form.notificarAdministrador" type="checkbox" />
        </label>
      </div>
      <div class="acoes">
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span><span v-else>Criar webform</span>
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.acoes { display: flex; justify-content: flex-end; margin-top: 16px; }
.toggle-row { display: flex; align-items: center; gap: 10px; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
</style>
