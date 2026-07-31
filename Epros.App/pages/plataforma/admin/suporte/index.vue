<script setup lang="ts">
/**
 * Área Landlord — Suporte a Clientes (1.04 Pass 4).
 *
 * Só a Siser (operador interno com perfil de suporte) acessa. Formaliza a antiga
 * impersonação como "acesso de suporte" com auditoria obrigatória:
 *
 *  - Abrir sessão de suporte num tenant cliente: `POST /landlord/suporte/acessar-cliente`
 *    (tenant-alvo + usuário-alvo + MOTIVO obrigatório). O backend nega abrir suporte para o
 *    usuário ADMIN do cliente — a mensagem de negação é exibida no banner de erro.
 *  - Encerrar a sessão ativa: `POST /landlord/suporte/encerrar`.
 *  - Gerir o perfil de suporte dos usuários internos: `PUT .../usuarios-internos/{id}/perfil-suporte`
 *    (Suporte Técnico / Suporte Negócio / Nenhum). O perfil define o TIPO de sessão emitida.
 *
 * Endpoints protegidos por ABAC (SuperAdmin:Configurar) — token "system" da Siser.
 */
import { ref, reactive, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'

definePageMeta({ layout: 'admin' })

const SESSAO_KEY = 'epros_suporte_sessao'

interface SessaoSuporte {
  sessaoSuporteId: string
  tenantAlvo: string
  nomeAlvo: string
  perfilSuporte: string
  token: string
}

interface UsuarioInterno {
  id: string
  nome: string
  email: string
  timezone: string
  primaryAdmin: boolean
}

/** PerfilSuporteSiser no backend (enum serializado por inteiro). */
const PERFIS = [
  { valor: 0, label: 'Nenhum' },
  { valor: 1, label: 'Suporte Técnico' },
  { valor: 2, label: 'Suporte Negócio' }
] as const

const toast = useToast()

const sessaoAtiva = ref<SessaoSuporte | null>(null)
const errorAbrir = ref<string>('')
const abrindo = ref(false)
const encerrando = ref(false)

const form = reactive({ TenantAlvo: '', UsuarioAlvoId: '', EmpresaId: '', Motivo: '' })

const usuarios = ref<UsuarioInterno[]>([])
const carregandoUsuarios = ref(true)
const errorUsuarios = ref<string>('')
// Perfil escolhido por usuário (id -> valor do enum) para o PUT.
const perfilEscolhido = reactive<Record<string, number>>({})
const salvandoPerfil = ref<string>('')

/** Extrai a mensagem de erro (erros/mensagem) do envelope CommandResult devolvido em 422. */
function extrairErro(e: unknown, fallback: string): string {
  const err = e as { data?: { erros?: string[]; mensagem?: string }; message?: string }
  const erros = err?.data?.erros
  if (erros && erros.length) return erros.join(' ')
  return err?.data?.mensagem || err?.message || fallback
}

function carregarSessao() {
  if (!import.meta.client) return
  const raw = localStorage.getItem(SESSAO_KEY)
  if (!raw) {
    sessaoAtiva.value = null
    return
  }
  try {
    sessaoAtiva.value = JSON.parse(raw) as SessaoSuporte
  } catch {
    sessaoAtiva.value = null
  }
}

async function abrirSessao() {
  errorAbrir.value = ''
  if (!form.Motivo.trim()) {
    errorAbrir.value = 'O motivo do acesso de suporte é obrigatório.'
    return
  }
  abrindo.value = true
  try {
    const body: Record<string, unknown> = {
      TenantAlvo: form.TenantAlvo.trim(),
      UsuarioAlvoId: form.UsuarioAlvoId.trim(),
      Motivo: form.Motivo.trim()
    }
    if (form.EmpresaId.trim()) body.EmpresaId = form.EmpresaId.trim()

    const resp = await useApi<{ sucesso?: boolean; dados?: any; erros?: string[]; mensagem?: string }>(
      '/landlord/suporte/acessar-cliente',
      { method: 'POST', body }
    )
    const dados = resp?.dados ?? resp
    if (resp?.sucesso === false || !dados?.token) {
      errorAbrir.value = resp?.erros?.join(' ') || resp?.mensagem || 'Não foi possível abrir a sessão de suporte.'
      return
    }

    const sessao: SessaoSuporte = {
      sessaoSuporteId: dados.sessaoSuporteId,
      tenantAlvo: dados.tenantAlvo,
      nomeAlvo: dados.nomeAlvo,
      perfilSuporte: dados.perfilSuporte,
      token: dados.token
    }
    localStorage.setItem(SESSAO_KEY, JSON.stringify(sessao))
    sessaoAtiva.value = sessao
    toast.success('Sessão de suporte iniciada.')
    form.TenantAlvo = ''
    form.UsuarioAlvoId = ''
    form.EmpresaId = ''
    form.Motivo = ''
  } catch (e) {
    // Inclui a salvaguarda de bloqueio: "Acesso Proibido: não é permitido abrir suporte para um usuário ADMIN do tenant."
    errorAbrir.value = extrairErro(e, 'Falha ao abrir a sessão de suporte.')
  } finally {
    abrindo.value = false
  }
}

async function encerrarSessao() {
  if (!sessaoAtiva.value) return
  encerrando.value = true
  try {
    await useApi('/landlord/suporte/encerrar', {
      method: 'POST',
      body: { SessaoImpersonacaoId: sessaoAtiva.value.sessaoSuporteId }
    })
    localStorage.removeItem(SESSAO_KEY)
    sessaoAtiva.value = null
    toast.success('Sessão de suporte encerrada.')
  } catch (e) {
    toast.error(extrairErro(e, 'Falha ao encerrar a sessão de suporte.'))
  } finally {
    encerrando.value = false
  }
}

async function carregarUsuarios() {
  carregandoUsuarios.value = true
  errorUsuarios.value = ''
  try {
    const data = await useApi<UsuarioInterno[]>('/plataforma/superadmin/usuarios-internos')
    usuarios.value = Array.isArray(data) ? data : []
    for (const u of usuarios.value) {
      if (perfilEscolhido[u.id] === undefined) perfilEscolhido[u.id] = 0
    }
  } catch (e) {
    errorUsuarios.value = extrairErro(e, 'Falha ao carregar os usuários internos.')
  } finally {
    carregandoUsuarios.value = false
  }
}

async function salvarPerfil(u: UsuarioInterno) {
  salvandoPerfil.value = u.id
  try {
    await useApi(`/landlord/suporte/usuarios-internos/${u.id}/perfil-suporte`, {
      method: 'PUT',
      body: { UsuarioInternoId: u.id, PerfilSuporte: perfilEscolhido[u.id] ?? 0 }
    })
    const label = PERFIS.find((p) => p.valor === (perfilEscolhido[u.id] ?? 0))?.label ?? 'Nenhum'
    toast.success(`Perfil de ${u.nome} definido como "${label}".`)
  } catch (e) {
    toast.error(extrairErro(e, 'Falha ao definir o perfil de suporte.'))
  } finally {
    salvandoPerfil.value = ''
  }
}

onMounted(() => {
  carregarSessao()
  carregarUsuarios()
})
</script>

<template>
  <div class="admin-page">
    <header class="admin-page-header">
      <div>
        <h1 class="admin-page-title">Suporte a Clientes</h1>
        <p class="admin-page-sub">Acesso de suporte da Siser a um tenant cliente, com auditoria obrigatória.</p>
      </div>
    </header>

    <!-- Banner de sessão de suporte ativa -->
    <section v-if="sessaoAtiva" class="suporte-banner glass-panel">
      <div class="suporte-banner-info">
        <span class="suporte-dot" aria-hidden="true"></span>
        <div>
          <strong class="suporte-banner-title">Sessão de suporte ativa</strong>
          <p class="suporte-banner-sub">
            Tenant <code>{{ sessaoAtiva.tenantAlvo }}</code> · alvo
            <strong>{{ sessaoAtiva.nomeAlvo }}</strong> · perfil {{ sessaoAtiva.perfilSuporte }}
          </p>
        </div>
      </div>
      <button type="button" class="btn btn-danger" :disabled="encerrando" @click="encerrarSessao">
        <span v-if="!encerrando">Encerrar sessão</span>
        <span v-else class="spinner"></span>
      </button>
    </section>

    <div class="grid-2">
      <!-- Abrir sessão de suporte -->
      <section class="admin-section glass-panel">
        <header class="section-header"><h3>Abrir sessão de suporte</h3></header>
        <p class="section-hint">
          O tipo da sessão (Técnico / Negócio) é definido pelo <strong>seu perfil de suporte</strong>.
          Não é permitido abrir suporte para o usuário ADMIN do tenant cliente.
        </p>

        <p v-if="errorAbrir" class="admin-alert-error">{{ errorAbrir }}</p>

        <form class="vertical-form" @submit.prevent="abrirSessao">
          <div class="form-group">
            <label>Tenant do cliente (alvo)</label>
            <input v-model="form.TenantAlvo" type="text" placeholder="ex.: empresa_teste" required />
          </div>
          <div class="form-group">
            <label>ID do usuário-alvo</label>
            <input v-model="form.UsuarioAlvoId" type="text" placeholder="GUID do usuário no cliente" required />
          </div>
          <div class="form-group">
            <label>ID da empresa (opcional)</label>
            <input v-model="form.EmpresaId" type="text" placeholder="GUID da empresa (opcional)" />
          </div>
          <div class="form-group">
            <label>Motivo <span class="req">*</span></label>
            <textarea v-model="form.Motivo" rows="3" placeholder="Descreva o motivo do acesso de suporte" required></textarea>
          </div>
          <button type="submit" class="btn btn-primary btn-block" :disabled="abrindo">
            <span v-if="!abrindo">Abrir sessão de suporte</span>
            <span v-else class="spinner"></span>
          </button>
        </form>
      </section>

      <!-- Perfis de suporte da equipe interna -->
      <section class="admin-section glass-panel">
        <header class="section-header"><h3>Perfis de suporte da equipe</h3></header>
        <p class="section-hint">
          Define quem, na Siser, pode abrir sessões de suporte e com qual escopo.
        </p>

        <p v-if="errorUsuarios" class="admin-alert-error">{{ errorUsuarios }}</p>

        <div class="table-container">
          <table class="admin-table">
            <thead>
              <tr>
                <th>Usuário interno</th>
                <th>Perfil de suporte</th>
                <th class="align-right">Ação</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="carregandoUsuarios"><td colspan="3" class="td-empty">Carregando…</td></tr>
              <tr v-else-if="usuarios.length === 0"><td colspan="3" class="td-empty">Nenhum usuário interno.</td></tr>
              <tr v-for="u in usuarios" :key="u.id">
                <td>
                  <div class="cell-strong">{{ u.nome }}</div>
                  <div class="cell-muted">{{ u.email }}</div>
                </td>
                <td>
                  <select v-model.number="perfilEscolhido[u.id]" class="perfil-select">
                    <option v-for="p in PERFIS" :key="p.valor" :value="p.valor">{{ p.label }}</option>
                  </select>
                </td>
                <td class="align-right">
                  <button
                    type="button"
                    class="btn btn-secondary btn-sm"
                    :disabled="salvandoPerfil === u.id"
                    @click="salvarPerfil(u)"
                  >
                    <span v-if="salvandoPerfil !== u.id">Salvar</span>
                    <span v-else class="spinner"></span>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
.admin-page { display: flex; flex-direction: column; gap: 20px; }
.admin-page-header { display: flex; justify-content: space-between; align-items: flex-end; gap: 12px; flex-wrap: wrap; }
.admin-page-title { font-size: 24px; font-weight: 800; letter-spacing: -0.5px; color: var(--text-primary); }
.admin-page-sub { font-size: 13px; color: var(--text-secondary); margin-top: 2px; }
.admin-alert-error { background: rgba(239,68,68,0.08); border: 1px solid rgba(239,68,68,0.25); color: var(--danger); padding: 10px 14px; border-radius: 8px; font-size: 13px; }

.suporte-banner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 16px 20px;
  border: 1px solid var(--warning, #f59e0b);
  background: rgba(245, 158, 11, 0.08);
  flex-wrap: wrap;
}
.suporte-banner-info { display: flex; align-items: center; gap: 12px; }
.suporte-dot {
  width: 10px; height: 10px; border-radius: 50%;
  background: var(--warning, #f59e0b);
  box-shadow: 0 0 0 4px rgba(245, 158, 11, 0.2);
  flex-shrink: 0;
}
.suporte-banner-title { font-size: 14px; font-weight: 700; color: var(--text-primary); }
.suporte-banner-sub { font-size: 12.5px; color: var(--text-secondary); margin-top: 2px; }
.suporte-banner-sub code { font-family: monospace; background: rgba(255,255,255,0.06); padding: 1px 5px; border-radius: 4px; }
.btn-danger { background: var(--danger); color: #fff; border: none; padding: 8px 18px; }

.grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; align-items: start; }
@media (max-width: 950px) { .grid-2 { grid-template-columns: 1fr; } }
.admin-section { padding: 20px; display: flex; flex-direction: column; gap: 14px; }
.section-header h3 { font-size: 15px; font-weight: 750; color: var(--text-primary); border-bottom: 1px solid rgba(255,255,255,0.05); padding-bottom: 10px; }
.section-hint { font-size: 12px; color: var(--text-secondary); line-height: 1.5; }

.vertical-form { display: flex; flex-direction: column; gap: 12px; }
.form-group { display: flex; flex-direction: column; gap: 6px; }
.form-group label { font-size: 10px; font-weight: 600; text-transform: uppercase; color: var(--text-secondary); }
.form-group .req { color: var(--danger); }
.form-group input, .form-group textarea, .perfil-select {
  padding: 8px 12px; background: rgba(255,255,255,0.02); border: 1px solid var(--border-color);
  border-radius: 8px; color: var(--text-primary); font-size: 12.5px; width: 100%; font-family: inherit;
}
.form-group input:focus, .form-group textarea:focus, .perfil-select:focus {
  outline: none; border-color: var(--primary); background: rgba(99,102,241,0.04);
}
.btn-block { width: 100%; padding: 10px; }
.btn-sm { padding: 4px 12px; font-size: 11px; }

.table-container { overflow-x: auto; }
.admin-table { width: 100%; border-collapse: collapse; }
.admin-table th { padding: 10px 12px; font-size: 10.5px; font-weight: 700; text-transform: uppercase; color: var(--text-secondary); border-bottom: 1px solid var(--border-color); text-align: left; }
.admin-table td { padding: 12px; font-size: 13px; border-bottom: 1px solid rgba(255,255,255,0.02); vertical-align: middle; }
.td-empty { text-align: center; color: var(--text-muted); font-style: italic; }
.cell-strong { font-weight: 600; color: var(--text-primary); }
.cell-muted { font-size: 11.5px; color: var(--text-muted); margin-top: 2px; }
.align-right { text-align: right; }
.perfil-select { min-width: 150px; }
</style>
