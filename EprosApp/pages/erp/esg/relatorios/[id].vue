<script setup lang="ts">
/**
 * Formulário de Relatório ESG (novo) — ESG / Relatórios.
 * A API expõe só `POST /esg/relatorios` (create-only). Rota /novo.
 */
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface RelatorioForm {
  anoFiscal: number | string | null
  nomeRelatorio: string | null
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const erros = reactive<Record<string, string>>({})

const form = reactive<RelatorioForm>({
  anoFiscal: new Date().getFullYear(),
  nomeRelatorio: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (form.anoFiscal == null) erros.anoFiscal = 'Ano fiscal é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/esg/relatorios', { method: 'POST', body: { ...form, anoFiscal: Number(form.anoFiscal) } })
    toast.success('Relatório criado com sucesso!')
    router.push('/erp/esg/relatorios')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/esg/relatorios')
}
</script>

<template>
  <div>
    <PageToolbar title="Novo relatório ESG">
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
          <TextField v-model="form.anoFiscal" label="Ano fiscal" type="number" required :error="erros.anoFiscal" />
          <TextField v-model="form.nomeRelatorio" label="Nome do relatório" placeholder="Ex.: Relatório de Sustentabilidade 2026" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 16px; }
</style>
