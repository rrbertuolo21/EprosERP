<script setup lang="ts">
/**
 * Formulário de Usuário (criar/editar) — porta `configuracoes/permissoes/usuarios/[id].vue` do legado.
 *
 * Consome `aplicativo/usuarios` (GET/POST/PUT/DELETE + `nova-senha`) e `plataforma/perfis-acesso`
 * para a lista de perfis disponíveis nos vínculos empresa/perfil.
 *
 * Rota `novo` cria; qualquer outro valor de `id` edita o usuário correspondente.
 */
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados, type CommandResult } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useTenant } from '~/composables/useTenant'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import UsuarioEmpresaTable, { type UsuarioEmpresaVinculo } from '~/components/config/UsuarioEmpresaTable.vue'

definePageMeta({
  middleware: 'auth'
})

interface Usuario {
  id?: string
  login: string
  email: string
  senha?: string
  ativo: boolean
  usuarioEmpresa: UsuarioEmpresaVinculo[]
  [key: string]: unknown
}

interface PerfilOpcao {
  id: string
  descricao: string
}

/** Envelope paginado de `GET plataforma/perfis-acesso` (PascalCase: Itens/Total/Pagina). */
interface RespostaPerfisAcesso {
  Itens?: PerfilOpcao[]
  itens?: PerfilOpcao[]
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { empresas } = useTenant()

const estadoInicial = (): Usuario => ({
  login: '',
  email: '',
  senha: '',
  ativo: true,
  usuarioEmpresa: []
})

const usuario = reactive<Usuario>(estadoInicial())
const carregando = ref(false)
const salvando = ref(false)
const perfis = ref<PerfilOpcao[]>([])

const dialogSenhaVisivel = ref(false)
const novaSenha = ref('')
const salvandoSenha = ref(false)

const vinculoEmpresaId = ref<number | null>(null)
const vinculoPerfilId = ref<string | null>(null)
const vinculoAdmin = ref(false)
const indiceEmEdicao = ref<number | null>(null)

const isEditing = computed(() => route.params.id !== 'novo')

const empresasOpcoes = computed(() => empresas.value.map((e) => ({ label: e.nome, value: e.id })))
const perfisOpcoes = computed(() => perfis.value.map((p) => ({ label: p.descricao, value: p.id })))

function nomeEmpresa(id: number | null): string {
  return empresas.value.find((e) => e.id === id)?.nome ?? '—'
}

function nomePerfil(id: string | null): string {
  return perfis.value.find((p) => p.id === id)?.descricao ?? '—'
}

/** Carrega a lista de perfis de acesso da empresa (`plataforma/perfis-acesso`, dono RBAC = GestaoClientes). */
async function carregarPerfis(): Promise<void> {
  try {
    const resposta = await useApi<CommandResult<RespostaPerfisAcesso> | RespostaPerfisAcesso>('/plataforma/perfis-acesso', {
      query: { ativo: true, tamanhoPagina: 200 }
    })
    const dados = extrairDados<RespostaPerfisAcesso>(resposta)
    perfis.value = dados?.Itens ?? dados?.itens ?? []
  } catch (e) {
    perfis.value = []
  }
}

async function carregarUsuario(id: string): Promise<void> {
  carregando.value = true
  try {
    const resposta = await useApi<CommandResult<Usuario> | Usuario>('/aplicativo/usuarios/{id}', {
      params: { id }
    })
    const dados = extrairDados<Usuario>(resposta)
    if (dados) {
      Object.assign(usuario, { ...estadoInicial(), ...dados, usuarioEmpresa: dados.usuarioEmpresa ?? [] })
    }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

function adicionarVinculo(): void {
  if (!vinculoAdmin.value && vinculoEmpresaId.value === null) {
    toast.error('É necessário selecionar uma empresa.')
    return
  }
  if (!vinculoAdmin.value && vinculoPerfilId.value === null) {
    toast.error('Selecione um perfil.')
    return
  }

  const item: UsuarioEmpresaVinculo = {
    empresaId: vinculoEmpresaId.value,
    perfilUsuarioId: vinculoAdmin.value ? null : vinculoPerfilId.value,
    isAdmin: vinculoAdmin.value
  }

  if (indiceEmEdicao.value !== null) {
    usuario.usuarioEmpresa.splice(indiceEmEdicao.value, 1, item)
    indiceEmEdicao.value = null
  } else {
    usuario.usuarioEmpresa.push(item)
  }

  vinculoEmpresaId.value = null
  vinculoPerfilId.value = null
  vinculoAdmin.value = false
}

function editarVinculo(index: number): void {
  const item = usuario.usuarioEmpresa[index]
  vinculoEmpresaId.value = item.empresaId
  vinculoPerfilId.value = item.perfilUsuarioId
  vinculoAdmin.value = item.isAdmin
  indiceEmEdicao.value = index
  usuario.usuarioEmpresa.splice(index, 1)
}

function removerVinculo(index: number): void {
  usuario.usuarioEmpresa.splice(index, 1)
}

function validar(): string | null {
  if (!usuario.login.trim()) return 'Informe o login.'
  if (!usuario.email.trim()) return 'Informe o e-mail.'
  if (!isEditing.value && !usuario.senha) return 'Informe a senha.'
  if (usuario.usuarioEmpresa.length === 0) return 'É necessário vincular ao menos uma empresa.'
  return null
}

async function salvar(): Promise<void> {
  const mensagemErro = validar()
  if (mensagemErro) {
    toast.error(mensagemErro)
    return
  }

  salvando.value = true
  try {
    if (isEditing.value) {
      await useApi('/aplicativo/usuarios', { method: 'PUT', body: usuario })
    } else {
      await useApi('/aplicativo/usuarios', { method: 'POST', body: usuario })
    }
    toast.success('Usuário salvo com sucesso.')
    router.push('/erp/configuracoes/permissoes/usuarios')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar(): void {
  router.push('/erp/configuracoes/permissoes/usuarios')
}

function abrirDialogSenha(): void {
  novaSenha.value = ''
  dialogSenhaVisivel.value = true
}

async function salvarNovaSenha(): Promise<void> {
  if (!novaSenha.value) {
    toast.error('Informe a nova senha.')
    return
  }
  salvandoSenha.value = true
  try {
    await useApi('/aplicativo/usuarios/nova-senha', {
      method: 'PUT',
      body: { id: usuario.id, senha: novaSenha.value }
    })
    toast.success('Nova senha gerada com sucesso.')
    dialogSenhaVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoSenha.value = false
  }
}

onMounted(() => {
  void carregarPerfis()
  const id = route.params.id as string
  if (id && id !== 'novo') {
    void carregarUsuario(id)
  }
})

watch(
  () => route.params.id,
  (id) => {
    if (id === 'novo') {
      Object.assign(usuario, estadoInicial())
    } else if (id) {
      void carregarUsuario(id as string)
    }
  }
)
</script>

<template>
  <div>
    <PageToolbar :title="isEditing ? 'Editar usuário' : 'Novo usuário'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="cancelar">Cancelar</button>
        <button
          v-if="isEditing"
          type="button"
          class="btn btn-secondary"
          :disabled="salvando"
          @click="abrirDialogSenha"
        >
          Nova senha
        </button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <section class="glass-panel section">
      <div class="form-grid">
        <div class="col-4">
          <TextField v-model="usuario.login" label="Login" required />
        </div>
        <div class="col-4">
          <TextField v-model="usuario.email" type="email" label="E-mail" required />
        </div>
        <div v-if="!isEditing" class="col-3">
          <TextField v-model="usuario.senha" type="password" label="Senha" required />
        </div>
        <div class="col-1">
          <label class="field-label">Ativo</label>
          <label class="switch">
            <input v-model="usuario.ativo" type="checkbox" />
            <span class="switch-track"><span class="switch-thumb"></span></span>
          </label>
        </div>
      </div>
    </section>

    <section class="glass-panel section">
      <h3 class="section-title">Empresas vinculadas</h3>
      <div class="form-grid">
        <div class="col-5">
          <SelectField v-model="vinculoEmpresaId" label="Empresa" :options="empresasOpcoes" />
        </div>
        <div class="col-1">
          <label class="field-label">Admin</label>
          <label class="switch">
            <input v-model="vinculoAdmin" type="checkbox" />
            <span class="switch-track"><span class="switch-thumb"></span></span>
          </label>
        </div>
        <div class="col-5">
          <SelectField
            v-model="vinculoPerfilId"
            label="Perfil"
            :options="perfisOpcoes"
            :disabled="vinculoAdmin"
          />
        </div>
        <div class="col-1 add-btn-col">
          <button type="button" class="btn btn-primary btn-sm" @click="adicionarVinculo">+ Add</button>
        </div>
      </div>

      <UsuarioEmpresaTable
        :items="usuario.usuarioEmpresa"
        :nome-empresa="nomeEmpresa"
        :nome-perfil="nomePerfil"
        @edit="editarVinculo"
        @remove="removerVinculo"
      />
    </section>

    <AppDialog v-model="dialogSenhaVisivel" title="Nova senha" width="420px">
      <TextField v-model="novaSenha" type="password" label="Nova senha" required />
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="dialogSenhaVisivel = false">Fechar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoSenha" @click="salvarNovaSenha">
          <span v-if="salvandoSenha" class="spinner"></span>
          <span v-else>Alterar senha</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.section { margin-top: 16px; padding: 20px; }
.section-title { margin: 0 0 16px; font-size: 15px; }
.add-btn-col { display: flex; align-items: flex-end; padding-bottom: 2px; }
.switch { position: relative; display: inline-block; width: 40px; height: 22px; cursor: pointer; }
.switch input { opacity: 0; width: 0; height: 0; }
.switch-track {
  position: absolute; inset: 0; background: rgba(255, 255, 255, 0.12); border-radius: 999px; transition: 0.2s;
}
.switch-thumb {
  position: absolute; top: 2px; left: 2px; width: 18px; height: 18px; background: #fff; border-radius: 50%; transition: 0.2s;
}
.switch input:checked + .switch-track { background: var(--primary); }
.switch input:checked + .switch-track .switch-thumb { transform: translateX(18px); }
</style>
