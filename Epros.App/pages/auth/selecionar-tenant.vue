<script setup lang="ts">
/**
 * Seleção de tenant (1.04 PASS 2) — etapa após o login quando a identidade global tem
 * acesso a mais de um tenant (ex.: um contador parceiro que atende várias empresas).
 *
 * O login já emitiu um token básico + a lista de `Tenants`; aqui o usuário escolhe UM,
 * chamamos `POST /auth/selecionar-tenant` e recebemos o token escopado ao tenant. Segue,
 * então, o MESMO desfecho do login single-tenant: bloqueio de inadimplência, seleção de
 * empresa (quando houver várias) e entrada no ERP. Usuário com 1 tenant nunca chega aqui
 * (o backend resolve direto no login).
 *
 * Análoga à seleção de empresa: lê a sessão parcial de `epros_user`, não reinventa store.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from '#app'
import { useApi } from '~/composables/useApi'
import ThemeToggle from '~/components/ThemeToggle.vue'
import AppLogo from '~/components/AppLogo.vue'

definePageMeta({ layout: 'guest' })

interface TenantDisponivel {
  tenantId: string
  nome: string
  papel?: string
}

interface SessaoParcial {
  email?: string
  usuarioId?: string
  tenantId?: string
  tenants?: TenantDisponivel[]
}

const router = useRouter()
const tenants = ref<TenantDisponivel[]>([])
const usuarioId = ref<string>('')
const emailUsuario = ref<string>('')
const carregando = ref(true)
const selecionando = ref<string>('')
const errorMessage = ref<string>('')

/** Extrai a mensagem de erro do envelope CommandResult devolvido pelo ofetch (422). */
function extrairErro(e: unknown, fallback: string): string {
  const err = e as { data?: { erros?: string[]; mensagem?: string }; message?: string }
  const erros = err?.data?.erros
  if (erros && erros.length) return erros.join(' ')
  return err?.data?.mensagem || err?.message || fallback
}

async function carregar() {
  carregando.value = true
  errorMessage.value = ''
  const raw = import.meta.client ? localStorage.getItem('epros_user') : null
  if (!raw) {
    router.replace('/')
    return
  }
  let sessao: SessaoParcial = {}
  try {
    sessao = JSON.parse(raw) as SessaoParcial
  } catch {
    router.replace('/')
    return
  }

  usuarioId.value = String(sessao.usuarioId ?? '')
  emailUsuario.value = sessao.email ?? ''

  // Preferência: lista já entregue pelo login. Fallback: recarrega do backend.
  if (sessao.tenants && sessao.tenants.length) {
    tenants.value = sessao.tenants
  } else if (usuarioId.value) {
    try {
      const data = await useApi<TenantDisponivel[]>('/auth/tenants-disponiveis', {
        query: { usuarioId: usuarioId.value }
      })
      tenants.value = Array.isArray(data) ? data : []
    } catch (e) {
      errorMessage.value = extrairErro(e, 'Falha ao carregar os tenants disponíveis.')
    }
  }

  // Sem tenants ou sessão inválida: volta ao login.
  if (!usuarioId.value || tenants.value.length === 0) {
    router.replace('/')
    return
  }

  carregando.value = false
}

async function selecionar(tenant: TenantDisponivel) {
  if (selecionando.value) return
  selecionando.value = tenant.tenantId
  errorMessage.value = ''
  try {
    const resp = await useApi<{ sucesso?: boolean; dados?: any; erros?: string[]; mensagem?: string }>(
      '/auth/selecionar-tenant',
      { method: 'POST', body: { UsuarioId: usuarioId.value, TenantId: tenant.tenantId } }
    )
    const dados = resp?.dados ?? resp
    const token = dados?.token
    if (resp?.sucesso === false || !token) {
      errorMessage.value = resp?.erros?.join(' ') || resp?.mensagem || 'Não foi possível selecionar o tenant.'
      selecionando.value = ''
      return
    }

    // Token escopado ao tenant selecionado.
    localStorage.setItem('epros_token', token)
    const userData = {
      email: dados.email ?? emailUsuario.value,
      usuarioId: dados.usuarioId ?? usuarioId.value,
      tenantId: dados.tenantId ?? tenant.tenantId,
      tenantName: tenant.nome,
      planName: 'Plano Demo',
      status: dados.block ? 'Atrasado' : 'Ativo',
      empresas: dados.empresas ?? []
    }
    localStorage.setItem('epros_user', JSON.stringify(userData))
    // Não sobrou lixo da escolha anterior de empresa (outro tenant).
    localStorage.removeItem('epros_empresa')

    // Empresa única já resolvida no token → grava a ativa (mesmo padrão do login).
    if (!dados.exigeSelecaoEmpresa && dados.empresas?.length) {
      const e0 = dados.empresas[0]
      localStorage.setItem('epros_empresa', JSON.stringify({ id: e0.empresaId, nome: e0.razaoSocial }))
    }

    if (userData.status === 'Atrasado') {
      router.push('/area-cliente/minhas-faturas')
    } else {
      router.push('/erp/acesso-rapido')
    }
  } catch (e) {
    errorMessage.value = extrairErro(e, 'Falha ao selecionar o tenant.')
    selecionando.value = ''
  }
}

function voltarLogin() {
  localStorage.removeItem('epros_token')
  localStorage.removeItem('epros_user')
  router.push('/')
}

onMounted(carregar)
</script>

<template>
  <div class="tenant-page">
    <div class="theme-corner">
      <ThemeToggle />
    </div>

    <section class="tenant-column">
      <div class="tenant-column-inner">
        <div class="form-brand">
          <AppLogo :size="30" full />
        </div>

        <main class="tenant-card glass-panel">
          <header class="tenant-card-head">
            <h2 class="tenant-title">Selecione o ambiente</h2>
            <p class="tenant-subtitle">
              <template v-if="emailUsuario">{{ emailUsuario }} —</template>
              você tem acesso a mais de um tenant. Escolha para continuar.
            </p>
          </header>

          <div v-if="errorMessage" class="status-banner error-banner">
            <span>❌ {{ errorMessage }}</span>
          </div>

          <p v-if="carregando" class="tenant-loading">Carregando tenants…</p>

          <ul v-else class="tenant-list">
            <li v-for="t in tenants" :key="t.tenantId">
              <button
                type="button"
                class="tenant-item"
                :disabled="!!selecionando"
                @click="selecionar(t)"
              >
                <span class="tenant-item-avatar">{{ (t.nome || t.tenantId).charAt(0).toUpperCase() }}</span>
                <span class="tenant-item-body">
                  <span class="tenant-item-name">{{ t.nome || t.tenantId }}</span>
                  <span v-if="t.papel" class="tenant-item-role">{{ t.papel }}</span>
                </span>
                <span v-if="selecionando === t.tenantId" class="spinner"></span>
                <span v-else class="tenant-item-arrow" aria-hidden="true">→</span>
              </button>
            </li>
          </ul>

          <button type="button" class="tenant-back" @click="voltarLogin">Voltar ao login</button>
        </main>
      </div>
    </section>
  </div>
</template>

<style scoped>
.tenant-page {
  position: relative;
  display: flex;
  width: 100%;
  min-height: 100vh;
  background: var(--bg-color);
}
.theme-corner {
  position: absolute;
  top: 20px;
  right: 20px;
  z-index: 20;
}
.tenant-column {
  flex: 1 1 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 32px 24px;
}
.tenant-column-inner {
  width: 100%;
  max-width: 448px;
  display: flex;
  flex-direction: column;
  align-items: center;
  animation: fade-in 0.5s cubic-bezier(0.16, 1, 0.3, 1);
}
@keyframes fade-in {
  from { opacity: 0; transform: translateY(14px); }
  to { opacity: 1; transform: translateY(0); }
}
.form-brand {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 28px;
}
.tenant-card {
  width: 100%;
  padding: 32px;
  border-radius: 20px;
}
.tenant-card-head {
  text-align: center;
  margin-bottom: 22px;
}
.tenant-title {
  font-size: 22px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 6px;
}
.tenant-subtitle {
  font-size: 13px;
  color: var(--text-secondary);
  line-height: 1.5;
}
.status-banner {
  padding: 12px 16px;
  border-radius: 10px;
  font-size: 13px;
  margin-bottom: 18px;
  display: flex;
  align-items: center;
  gap: 10px;
}
.error-banner {
  background: var(--danger-glow);
  border: 1px solid var(--danger);
  color: var(--danger);
}
.tenant-loading {
  text-align: center;
  color: var(--text-muted);
  font-size: 13px;
  padding: 20px 0;
}
.tenant-list {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin: 0;
  padding: 0;
}
.tenant-item {
  display: flex;
  align-items: center;
  gap: 14px;
  width: 100%;
  padding: 14px 16px;
  border-radius: 12px;
  background: var(--surface-raised);
  border: 1px solid var(--border-color);
  color: var(--text-primary);
  cursor: pointer;
  text-align: left;
  transition: transform 0.15s ease, border-color 0.15s ease;
}
.tenant-item:hover:not(:disabled) {
  transform: translateY(-1px);
  border-color: var(--primary);
}
.tenant-item:disabled {
  opacity: 0.6;
  cursor: default;
}
.tenant-item-avatar {
  width: 38px;
  height: 38px;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 10px;
  background: var(--primary);
  color: #fff;
  font-weight: 700;
  font-size: 16px;
}
.tenant-item-body {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
  min-width: 0;
}
.tenant-item-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
}
.tenant-item-role {
  font-size: 12px;
  color: var(--text-secondary);
}
.tenant-item-arrow {
  color: var(--text-muted);
  font-size: 16px;
}
.tenant-back {
  margin-top: 20px;
  width: 100%;
  background: transparent;
  border: none;
  color: var(--text-secondary);
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  padding: 6px;
}
.tenant-back:hover {
  color: var(--primary);
  text-decoration: underline;
}
</style>
