<script setup lang="ts">
/**
 * Formulário de Categoria de Denúncia (nova) — GRC.
 * Fonte: POST /api/v1/grc/denuncias/categorias. Apenas criação.
 */
import { reactive, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface CategoriaForm {
  nome: string | null
  descricao: string | null
  cor: string | null
  criadorId: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const form = reactive<CategoriaForm>({ nome: null, descricao: null, cor: null, criadorId: null })
const erros = reactive<Record<string, string>>({})
const salvando = ref(false)

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
    await useApi('/grc/denuncias/categorias', { method: 'POST', body: form })
    toast.success('Categoria cadastrada com sucesso!')
    router.push('/erp/grc/denuncia-categorias')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/grc/denuncia-categorias')
}

onMounted(() => {
  if ((route.params.id as string) !== 'novo') {
    toast.error('Edição não disponível neste módulo. Apenas cadastro de novas categorias.')
    router.replace('/erp/grc/denuncia-categorias')
  }
})
</script>

<template>
  <div>
    <PageToolbar title="Nova categoria">
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
          <TextField v-model="form.nome" label="Nome" required :error="erros.nome" maxlength="100" />
          <TextField v-model="form.cor" label="Cor" placeholder="#RRGGBB" maxlength="20" />
          <!-- criadorId é UUID opcional; sem endpoint de listagem de usuários no módulo GRC. -->
          <TextField v-model="form.criadorId" label="Criador (criadorId)" hint="UUID do usuário criador (opcional)" />
          <div class="span-2">
            <TextField v-model="form.descricao" label="Descrição" maxlength="500" />
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
