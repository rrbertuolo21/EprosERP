<script setup lang="ts">
/**
 * Formulário de Denúncia (nova) — GRC / Investigações e Denúncias.
 * Fonte: POST /api/v1/grc/denuncias/detalhada (variante rica: título, categoria, prioridade).
 * Categorias carregadas de GET /api/v1/grc/denuncias/categorias. Apenas criação.
 */
import { reactive, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface DenunciaForm {
  titulo: string | null
  relato: string | null
  categoriaId: string | null
  prioridade: string | null
  anonima: boolean
}

interface Categoria {
  id: string
  nome?: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const prioridades: SelectOption[] = ['Baixa', 'Media', 'Alta', 'Critica'].map((p) => ({ label: p, value: p }))
const categorias = ref<SelectOption[]>([])

const form = reactive<DenunciaForm>({ titulo: null, relato: null, categoriaId: null, prioridade: 'Media', anonima: false })
const erros = reactive<Record<string, string>>({})
const salvando = ref(false)

async function carregarCategorias() {
  try {
    const resposta = await useApi('/grc/denuncias/categorias')
    const dados = extrairDados<Categoria[]>(resposta) ?? []
    categorias.value = dados.map((c) => ({ label: c.nome ?? c.id, value: c.id }))
  } catch (e) {
    console.error('[grc/denuncias/[id]] categorias', e)
    categorias.value = []
  }
}

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.relato) erros.relato = 'Relato é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/grc/denuncias/detalhada', { method: 'POST', body: form })
    toast.success('Denúncia registrada com sucesso!')
    router.push('/erp/grc/denuncias')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/grc/denuncias')
}

onMounted(() => {
  if ((route.params.id as string) !== 'novo') {
    toast.error('Edição não disponível neste módulo. Apenas registro de novas denúncias.')
    router.replace('/erp/grc/denuncias')
    return
  }
  void carregarCategorias()
})
</script>

<template>
  <div>
    <PageToolbar title="Nova denúncia">
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
          <TextField v-model="form.titulo" label="Título" maxlength="200" />
          <SelectField v-model="form.categoriaId" label="Categoria" :options="categorias" placeholder="Selecione..." />
          <SelectField v-model="form.prioridade" label="Prioridade" :options="prioridades" />
          <label class="field toggle-row">
            <span class="field-label">{{ form.anonima ? 'Anônima' : 'Identificada' }}</span>
            <input v-model="form.anonima" type="checkbox" />
          </label>
          <div class="span-2">
            <TextField v-model="form.relato" label="Relato" required :error="erros.relato" maxlength="2000" />
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
.toggle-row { display: flex; align-items: center; gap: 10px; justify-content: flex-start; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
</style>
