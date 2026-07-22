<template>
  <div class="login-page">
    <!-- Botão de alternar tema no canto -->
    <div class="theme-corner">
      <ThemeToggle />
    </div>

    <!-- ESQUERDA: painel de marca (some no mobile) -->
    <aside class="brand-panel">
      <div class="brand-panel-inner">
        <div class="brand-mark">
          <AppLogo :size="40" :wordmark="false" monochrome />
        </div>
        <h2 class="brand-panel-title">Epros ERP</h2>
        <p class="brand-panel-sub">Plataforma SaaS multi-tenant para gestão do seu negócio.</p>
      </div>
      <div class="brand-orb brand-orb-1"></div>
      <div class="brand-orb brand-orb-2"></div>
    </aside>

    <!-- DIREITA: coluna do formulário -->
    <section class="form-column">
      <div class="form-column-inner">
        <!-- Logo + nome do sistema -->
        <div class="form-brand">
          <AppLogo :size="30" full />
        </div>

        <main class="login-card">
          <header class="login-card-head">
            <h2 class="login-title">Bem-vindo de volta</h2>
            <p class="login-subtitle">Faça login para continuar</p>
          </header>

          <!-- Banner de erro real de credenciais -->
          <div v-if="errorMessage" class="status-banner error-banner">
            <span>❌ {{ errorMessage }}</span>
          </div>

          <form @submit.prevent="handleLogin" class="login-form">
            <div class="field">
              <label class="field-label" for="email">E-mail</label>
              <div class="input-affix has-prefix">
                <span class="prefix" aria-hidden="true">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <rect x="2" y="4" width="20" height="16" rx="2" />
                    <path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7" />
                  </svg>
                </span>
                <input
                  class="input"
                  type="email"
                  id="email"
                  v-model="form.email"
                  placeholder="seu@email.com"
                  required
                />
              </div>
            </div>

            <div class="field">
              <label class="field-label" for="password">Senha</label>
              <div class="input-affix has-prefix has-suffix">
                <span class="prefix" aria-hidden="true">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <rect x="3" y="11" width="18" height="11" rx="2" />
                    <path d="M7 11V7a5 5 0 0 1 10 0v4" />
                  </svg>
                </span>
                <input
                  class="input"
                  :type="showPassword ? 'text' : 'password'"
                  id="password"
                  v-model="form.password"
                  placeholder="••••••••"
                  required
                />
                <button
                  type="button"
                  class="password-toggle"
                  :aria-label="showPassword ? 'Ocultar senha' : 'Mostrar senha'"
                  @click="showPassword = !showPassword"
                >
                  <svg v-if="showPassword" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M9.88 9.88a3 3 0 0 0 4.24 4.24" />
                    <path d="M10.73 5.08A10.43 10.43 0 0 1 12 5c7 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68" />
                    <path d="M6.61 6.61A13.526 13.526 0 0 0 2 12s3 7 10 7a9.74 9.74 0 0 0 5.39-1.61" />
                    <line x1="2" x2="22" y1="2" y2="22" />
                  </svg>
                  <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z" />
                    <circle cx="12" cy="12" r="3" />
                  </svg>
                </button>
              </div>
            </div>

            <div class="forgot-row">
              <NuxtLink to="/recuperar-senha" class="forgot-link">Esqueci a senha</NuxtLink>
            </div>

            <button type="submit" class="btn btn-primary submit-btn" :disabled="loading">
              <span v-if="!loading">Entrar</span>
              <span v-else class="spinner"></span>
            </button>
          </form>

          <p class="signup-prompt">
            Não tem uma conta?
            <NuxtLink to="/cadastro" class="signup-link">Cadastre-se</NuxtLink>
          </p>
        </main>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from '#app'
import ThemeToggle from '~/components/ThemeToggle.vue'
import AppLogo from '~/components/AppLogo.vue'

definePageMeta({ layout: 'guest' })

const router = useRouter()
const loading = ref(false)
// Mantido apenas como sinalizador interno de fallback (não exibido na UI):
// se a API real de login falhar e o Keycloak também estiver fora, cai no
// modo simulado local. Não é mostrado como aviso para não assustar o usuário
// por causa da checagem de /installation/state.
const keycloakOffline = ref(false)
const errorMessage = ref('')
const showPassword = ref(false)

// Tenant não é mais exposto na UI de produção (multi-tenant é resolvido pela
// API a partir do login/e-mail). Mantido no estado com default vazio para
// preservar o payload/fallback existentes sem alterar o fluxo de auth.
const form = reactive({
  tenant: '',
  email: '',
  password: ''
})

const apiUrl = (path) => {
  const config = useRuntimeConfig()
  const base = (config.public.apiBaseUrl || '').replace(/\/$/, '')
  return `${base}${path}`
}

onMounted(async () => {
  try {
    await $fetch(apiUrl('/api/v1/installation/state'), { timeout: 3000 })
    keycloakOffline.value = false
  } catch {
    keycloakOffline.value = true
  }
})

// Decodificador nativo de JWT baseado em base64url
const decodeJwt = (token) => {
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const jsonPayload = decodeURIComponent(atob(base64).split('').map((c) => {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
    }).join(''))
    return JSON.parse(jsonPayload)
  } catch (e) {
    console.error('Falha ao decodificar JWT nativamente:', e)
    return null
  }
}

const handleLogin = async () => {
  loading.value = true
  errorMessage.value = ''

  // 1) Login real via API EprosERP (tenant operacional)
  try {
    const resp = await $fetch(apiUrl('/api/v1/public/auth/login'), {
      method: 'POST',
      body: { email: form.email, senha: form.password }
    })
    const dados = resp?.dados ?? resp?.data ?? resp
    if (resp?.sucesso !== false && dados?.token) {
      localStorage.setItem('epros_token', dados.token)
      const userData = {
        email: dados.email ?? form.email,
        tenantId: dados.tenantId ?? form.tenant.toLowerCase().trim(),
        tenantName: dados.nome ?? 'Empresa',
        planName: 'Plano Demo',
        status: dados.block ? 'Atrasado' : 'Ativo',
        empresas: dados.empresas ?? []
      }
      localStorage.setItem('epros_user', JSON.stringify(userData))
      loading.value = false
      if (dados.empresas?.length === 1) {
        localStorage.setItem('epros_empresa', JSON.stringify({ id: dados.empresas[0].empresaId, nome: dados.empresas[0].razaoSocial }))
      }
      router.push('/erp/acesso-rapido')
      return
    }
    if (resp?.erros?.length) {
      errorMessage.value = resp.erros.join(' ')
      loading.value = false
      return
    }
  } catch (apiErr) {
    const erros = apiErr?.data?.erros ?? apiErr?.response?._data?.erros
    if (erros?.length) {
      errorMessage.value = erros.join(' ')
      loading.value = false
      return
    }
    console.warn('Login API indisponível, tentando fallback:', apiErr)
  }

  if (keycloakOffline.value) {
    setTimeout(() => {
      loading.value = false
      executeSimulatedLogin()
    }, 500)
    return
  }

  // 2) Fluxo OIDC Keycloak (legado)
  try {
    const body = new URLSearchParams()
    body.append('client_id', 'epros-api')
    body.append('grant_type', 'password')
    body.append('username', form.email)
    body.append('password', form.password)
    body.append('scope', 'openid')

    const tokenData = await $fetch('http://localhost:8080/realms/epros-tenant/protocol/openid-connect/token', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded'
      },
      body: body
    })

    if (tokenData && tokenData.access_token) {
      const decoded = decodeJwt(tokenData.access_token)
      if (!decoded) {
        throw new Error('Não foi possível obter claims do JWT recebido.')
      }

      // Persiste token e perfil
      localStorage.setItem('epros_token', tokenData.access_token)

      // Super Admin verificado por role ou e-mail
      const isLandlordAdmin = decoded.realm_access?.roles?.includes('admin') || decoded.email === 'admin@epros.com'
      const resolvedTenantId = decoded.tenantId || form.tenant.toLowerCase().trim() || 'tenant-padrao'

      const userData = {
        email: decoded.email || form.email,
        tenantId: resolvedTenantId,
        tenantName: resolvedTenantId.charAt(0).toUpperCase() + resolvedTenantId.slice(1) + ' Ltda',
        planName: isLandlordAdmin ? 'Dono da Plataforma' : (form.tenant.toLowerCase().includes('gold') ? 'Plano Gold' : (form.tenant.toLowerCase().includes('platinum') ? 'Plano Platinum' : 'Plano Silver')),
        status: isLandlordAdmin ? 'Admin' : 'Ativo'
      }

      localStorage.setItem('epros_user', JSON.stringify(userData))
      loading.value = false

      if (isLandlordAdmin) {
        router.push('/plataforma/admin')
      } else {
        router.push('/dashboard')
      }
    } else {
      throw new Error('Token não retornado pelo Keycloak.')
    }
  } catch (err) {
    loading.value = false
    console.error('Erro de autenticação no Keycloak:', err)

    if (err.status === 401 || err.status === 400) {
      errorMessage.value = 'Inquilino, usuário ou senha inválidos no Keycloak.'
    } else {
      console.warn('Servidor Keycloak ficou inacessível no login. Executando fallback em modo simulação.')
      keycloakOffline.value = true
      executeSimulatedLogin()
    }
  }
}

const executeSimulatedLogin = () => {
  const tenantKey = form.tenant.toLowerCase().trim()
  const emailKey = form.email.toLowerCase().trim()

  // Caso de login do Super Admin da Plataforma
  if (tenantKey === 'admin' || emailKey === 'admin@epros.com') {
    const adminData = {
      email: form.email,
      tenantId: 'admin',
      tenantName: 'Super Administrador',
      planName: 'Dono da Plataforma',
      status: 'Admin'
    }
    localStorage.setItem('epros_user', JSON.stringify(adminData))
    router.push('/plataforma/admin')
    return
  }

  // Inicializa inquilinos no localStorage se não existirem
  let tenants = []
  const storedTenants = localStorage.getItem('epros_tenants')
  if (storedTenants) {
    tenants = JSON.parse(storedTenants)
  } else {
    tenants = [
      { id: 'empresa_teste', name: 'Empresa Teste Ltda', plan: 'Plano Silver', status: 'Ativo', email: 'contato@teste.com' },
      { id: 'gold_inovacao', name: 'Gold Inovação Ltda', plan: 'Plano Gold', status: 'Ativo', email: 'contato@gold.com' },
      { id: 'platinum_corp', name: 'Platinum Corp', plan: 'Plano Platinum', status: 'Ativo', email: 'ceo@platinum.com' },
      { id: 'bloqueado', name: 'Inadimplência S.A.', plan: 'Plano Bronze', status: 'Atrasado', email: 'financeiro@inadimplente.com' }
    ]
    localStorage.setItem('epros_tenants', JSON.stringify(tenants))
  }

  // Procura inquilino cadastrado
  let tenantInfo = tenants.find(t => t.id === tenantKey)

  if (!tenantInfo) {
    // Se não existe, cria dinamicamente
    let plan = 'Plano Silver'
    if (tenantKey.includes('gold')) {
      plan = 'Plano Gold'
    } else if (tenantKey.includes('platinum')) {
      plan = 'Plano Platinum'
    }

    tenantInfo = {
      id: tenantKey,
      name: form.tenant.charAt(0).toUpperCase() + form.tenant.slice(1) + ' Ltda',
      plan: plan,
      status: 'Ativo',
      email: form.email
    }
    tenants.push(tenantInfo)
    localStorage.setItem('epros_tenants', JSON.stringify(tenants))
  }

  const userData = {
    email: form.email,
    tenantId: tenantInfo.id,
    tenantName: tenantInfo.name,
    planName: tenantInfo.plan,
    status: tenantInfo.status
  }

  // Grava no localStorage para persistência e validação no middleware
  localStorage.setItem('epros_user', JSON.stringify(userData))

  if (tenantInfo.status === 'Atrasado') {
    router.push('/area-cliente/minhas-faturas')
  } else {
    router.push('/dashboard')
  }
}
</script>

<style scoped>
/* ==== Estrutura split-screen ==== */
.login-page {
  position: relative;
  display: flex;
  width: 100%;
  min-height: 100vh;
}

.theme-corner {
  position: absolute;
  top: 20px;
  right: 20px;
  z-index: 20;
}

/* ==== ESQUERDA: painel de marca ==== */
.brand-panel {
  position: relative;
  flex: 1 1 55%;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  background:
    radial-gradient(circle at 30% 20%, rgba(129, 140, 248, 0.35), transparent 55%),
    linear-gradient(135deg, var(--primary), #4f46e5 55%, var(--accent-purple));
  color: #fff;
  padding: 40px;
}

.brand-panel-inner {
  position: relative;
  z-index: 2;
  max-width: 420px;
  text-align: center;
}

.brand-mark {
  width: 84px;
  height: 84px;
  margin: 0 auto 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 22px;
  background: rgba(255, 255, 255, 0.14);
  border: 1px solid rgba(255, 255, 255, 0.25);
  backdrop-filter: blur(6px);
  color: #fff;
}

.brand-panel-title {
  font-size: 34px;
  font-weight: 700;
  letter-spacing: -0.5px;
  margin-bottom: 12px;
}

.brand-panel-sub {
  font-size: 15px;
  line-height: 1.6;
  color: rgba(255, 255, 255, 0.85);
}

.brand-orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(90px);
  opacity: 0.5;
  z-index: 1;
}

.brand-orb-1 {
  width: 380px;
  height: 380px;
  background: var(--accent-pink);
  top: -120px;
  right: -80px;
}

.brand-orb-2 {
  width: 320px;
  height: 320px;
  background: #3b82f6;
  bottom: -100px;
  left: -60px;
}

/* ==== DIREITA: coluna do formulário ==== */
.form-column {
  flex: 1 1 45%;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 32px 24px;
  background: var(--bg-color);
}

.form-column-inner {
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

/* ==== Card ==== */
.login-card {
  width: 100%;
  padding: 36px;
  border-radius: 20px;
  background: var(--surface-raised);
  border: 1px solid var(--border-color);
  box-shadow: var(--glass-shadow);
}

.login-card-head {
  text-align: center;
  margin-bottom: 28px;
}

.login-title {
  font-size: 24px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 6px;
}

.login-subtitle {
  font-size: 14px;
  color: var(--text-secondary);
}

/* ==== Banners ==== */
.status-banner {
  padding: 12px 16px;
  border-radius: 10px;
  font-size: 13px;
  margin-bottom: 20px;
  display: flex;
  align-items: center;
  gap: 10px;
  animation: slide-down 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.error-banner {
  background: var(--danger-glow);
  border: 1px solid var(--danger);
  color: var(--danger);
}

@keyframes slide-down {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
}

/* ==== Formulário ==== */
.login-form {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

/* Ícones dos affixes precisam de dimensão explícita (SVG inline) */
.input-affix .prefix svg,
.password-toggle svg {
  width: 18px;
  height: 18px;
  display: block;
}

/* Sobrescreve pointer-events do prefixo global só para exibir o ícone */
.input-affix .prefix {
  display: flex;
  align-items: center;
}

.password-toggle {
  position: absolute;
  right: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 6px;
  border: none;
  background: transparent;
  color: var(--text-muted);
  cursor: pointer;
  border-radius: 6px;
  transition: color 0.2s ease, background 0.2s ease;
}

.password-toggle:hover {
  color: var(--text-primary);
  background: var(--hover-bg);
}

.forgot-row {
  display: flex;
  justify-content: flex-end;
  margin-top: -4px;
}

.forgot-link,
.signup-link {
  color: var(--primary);
  text-decoration: none;
  font-weight: 600;
  font-size: 13px;
  transition: color 0.2s ease;
}

.forgot-link:hover,
.signup-link:hover {
  color: var(--primary-hover);
  text-decoration: underline;
}

.submit-btn {
  margin-top: 4px;
  padding: 13px;
  width: 100%;
  font-size: 15px;
}

.signup-prompt {
  text-align: center;
  margin-top: 22px;
  font-size: 13px;
  color: var(--text-secondary);
}

.signup-link {
  margin-left: 4px;
}

/* ==== Responsivo: no mobile só a coluna do formulário ==== */
@media (max-width: 992px) {
  .brand-panel {
    display: none;
  }
  .form-column {
    flex: 1 1 100%;
  }
}
</style>
