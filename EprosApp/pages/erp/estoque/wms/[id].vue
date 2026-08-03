<script setup lang="ts">
/**
 * WMS — Armazém (cadastro/edição) (erp/estoque/wms/[id]).
 * `id === 'novo'` → POST /estoque-wms-armazens (CriarWmsArmazemCommand).
 * `id === guid`   → GET /estoque-wms-armazens/{id} + PUT /estoque-wms-armazens/{id} (AtualizarWmsArmazemCommand).
 */
import { computed, onMounted, reactive, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

interface ArmazemDto {
  id?: string
  nome?: string
  endereco?: string
  cidade?: string
  cep?: string
  telefone?: string | null
  email?: string | null
  ativo?: boolean
  usuarioDonoId?: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = computed(() => route.params.id as string)
const ehNovo = computed(() => idParam.value === 'novo')
const pageTitle = computed(() => (ehNovo.value ? 'Novo armazém' : 'Editar armazém'))

const carregando = ref(false)
const salvando = ref(false)
const erros = reactive<Record<string, string>>({})

const form = reactive({
  nome: '',
  endereco: '',
  cidade: '',
  cep: '',
  telefone: '',
  email: '',
  ativo: true,
  usuarioDonoId: ''
})

async function carregar() {
  if (ehNovo.value) return
  carregando.value = true
  try {
    const resp = await useApi('/estoque-wms-armazens/{id}', { params: { id: idParam.value } })
    const dto = extrairDados<ArmazemDto>(resp)
    if (dto) {
      form.nome = dto.nome ?? ''
      form.endereco = dto.endereco ?? ''
      form.cidade = dto.cidade ?? ''
      form.cep = dto.cep ?? ''
      form.telefone = dto.telefone ?? ''
      form.email = dto.email ?? ''
      form.ativo = dto.ativo ?? true
      form.usuarioDonoId = dto.usuarioDonoId ?? ''
    }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.nome.trim()) erros.nome = 'Nome é obrigatório.'
  if (!form.endereco.trim()) erros.endereco = 'Endereço é obrigatório.'
  if (!form.cidade.trim()) erros.cidade = 'Cidade é obrigatória.'
  if (!form.cep.trim()) erros.cep = 'CEP é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    if (ehNovo.value) {
      await useApi('/estoque-wms-armazens', {
        method: 'POST',
        body: {
          nome: form.nome.trim(),
          endereco: form.endereco.trim(),
          cidade: form.cidade.trim(),
          cep: form.cep.trim(),
          telefone: form.telefone.trim() || null,
          email: form.email.trim() || null,
          ativo: form.ativo,
          usuarioDonoId: form.usuarioDonoId.trim() || null
        }
      })
    } else {
      await useApi('/estoque-wms-armazens/{id}', {
        method: 'PUT',
        params: { id: idParam.value },
        body: {
          id: idParam.value,
          nome: form.nome.trim(),
          endereco: form.endereco.trim(),
          cidade: form.cidade.trim(),
          cep: form.cep.trim(),
          telefone: form.telefone.trim() || null,
          email: form.email.trim() || null,
          ativo: form.ativo
        }
      })
    }
    toast.success(ehNovo.value ? 'Armazém criado com sucesso!' : 'Armazém atualizado!')
    await router.push('/erp/estoque/wms')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() { router.push('/erp/estoque/wms') }

onMounted(() => void carregar())
</script>

<template>
  <div>
    <PageToolbar :title="pageTitle" subtitle="Cadastro de armazém WMS" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando || carregando" @click="salvar">
          <span v-if="salvando" class="spinner"></span><span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div class="form-grid">
        <TextField v-model="form.nome" label="Nome" required :error="erros.nome" />
        <TextField v-model="form.endereco" label="Endereço" required :error="erros.endereco" />
        <TextField v-model="form.cidade" label="Cidade" required :error="erros.cidade" />
        <TextField v-model="form.cep" label="CEP" required :error="erros.cep" />
        <TextField v-model="form.telefone" label="Telefone" />
        <TextField v-model="form.email" label="E-mail" type="email" />
        <div class="field">
          <label class="field-label">Situação</label>
          <select v-model="form.ativo" class="select">
            <option :value="true">Ativo</option>
            <option :value="false">Inativo</option>
          </select>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
