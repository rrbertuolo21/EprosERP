<script setup lang="ts">
/**
 * Formulário de Política (novo) — GRC / Gestão de Políticas.
 * Fonte: POST /api/v1/grc/politicas. Apenas criação (sem GET/{id} nem PUT no backend).
 */
import { reactive, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface PoliticaForm {
  codigo: string | null
  titulo: string | null
  descricao: string | null
  categoria: string | null
  ownerId: string | null
  moduloAplicavel: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const form = reactive<PoliticaForm>({
  codigo: null,
  titulo: null,
  descricao: null,
  categoria: null,
  ownerId: null,
  moduloAplicavel: null
})
const erros = reactive<Record<string, string>>({})
const salvando = ref(false)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.titulo) erros.titulo = 'Título é obrigatório.'
  if (!form.codigo) erros.codigo = 'Código é obrigatório.'
  if (!form.ownerId) erros.ownerId = 'Responsável (ownerId) é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/grc/politicas', { method: 'POST', body: form })
    toast.success('Política cadastrada com sucesso!')
    router.push('/erp/grc/politicas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/grc/politicas')
}

onMounted(() => {
  if ((route.params.id as string) !== 'novo') {
    toast.error('Edição não disponível neste módulo. Apenas cadastro de novas políticas.')
    router.replace('/erp/grc/politicas')
  }
})
</script>

<template>
  <div>
    <PageToolbar title="Nova política">
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
          <TextField v-model="form.codigo" label="Código" required :error="erros.codigo" maxlength="50" />
          <TextField v-model="form.titulo" label="Título" required :error="erros.titulo" maxlength="200" />
          <TextField v-model="form.categoria" label="Categoria" placeholder="Conduta, Preço, Privacidade..." maxlength="100" />
          <TextField v-model="form.moduloAplicavel" label="Módulo aplicável" placeholder="RH, Vendas, Financeiro..." maxlength="100" />
          <!-- ownerId é UUID; não há endpoint de listagem de usuários exposto no módulo GRC. -->
          <TextField v-model="form.ownerId" label="Responsável (ownerId)" required :error="erros.ownerId" hint="UUID do usuário responsável" />
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
