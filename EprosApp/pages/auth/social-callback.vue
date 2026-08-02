<script setup lang="ts">
/**
 * Callback do login social (1.04 PASS 3).
 *
 * O provedor (Google/Microsoft) redireciona o navegador para cá com `code` + `state`.
 * Esta página lê esses parâmetros, chama `GET /auth/social/{provedor}/callback` e trata
 * os três desfechos possíveis do backend:
 *   1. `PrecisaCompletarCadastro` → conta social nova: encaminha ao onboarding pré-preenchido
 *      (social não cria tenant fiscal sozinho — REG-036).
 *   2. `ExigeSelecaoTenant` → identidade com vários tenants: vai à seleção de tenant.
 *   3. `token` (empresa única/resolvida) → entra direto no ERP (ou faturas se inadimplente).
 *
 * O provedor não trafega no `state`; guardamos `epros_social_provedor` no `/start` para
 * saber qual rota de callback chamar aqui (também aceitamos `?provedor=` na URL).
 */
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from '#app'
import { useApi } from '~/composables/useApi'
import AppLogo from '~/components/AppLogo.vue'

definePageMeta({ layout: 'guest' })

const route = useRoute()
const router = useRouter()
const status = ref<'processando' | 'erro'>('processando')
const errorMessage = ref<string>('')

function extrairErro(e: unknown, fallback: string): string {
  const err = e as { data?: { erros?: string[]; mensagem?: string }; message?: string }
  const erros = err?.data?.erros
  if (erros && erros.length) return erros.join(' ')
  return err?.data?.mensagem || err?.message || fallback
}

function query(nome: string): string {
  const v = route.query[nome]
  return typeof v === 'string' ? v : ''
}

async function processar() {
  status.value = 'processando'
  errorMessage.value = ''

  // Sessão anterior não deve poluir o cabeçalho da chamada anônima de callback.
  localStorage.removeItem('epros_token')
  localStorage.removeItem('epros_user')
  localStorage.removeItem('epros_empresa')

  const provedor =
    query('provedor') ||
    (import.meta.client ? localStorage.getItem('epros_social_provedor') : '') ||
    'Google'
  const code = query('code')
  const state = query('state')
  const erroProvedor = query('error')

  if (erroProvedor) {
    status.value = 'erro'
    errorMessage.value = 'Autenticação social não concluída no provedor.'
    return
  }
  if (!code || !state) {
    status.value = 'erro'
    errorMessage.value = 'Parâmetros do callback social ausentes (code/state).'
    return
  }

  try {
    const resp = await useApi<{ sucesso?: boolean; dados?: any; erros?: string[]; mensagem?: string }>(
      '/auth/social/{provedor}/callback',
      { method: 'GET', params: { provedor }, query: { code, state } }
    )
    const dados = resp?.dados ?? resp

    if (resp?.sucesso === false || !dados) {
      status.value = 'erro'
      errorMessage.value = resp?.erros?.join(' ') || resp?.mensagem || 'Falha na autenticação social.'
      return
    }

    // 1. Conta social nova → onboarding pré-preenchido com os dados verificados do provedor.
    if (dados.precisaCompletarCadastro) {
      const prefill = {
        social: true,
        provedor: dados.provedor ?? provedor,
        subjectId: dados.subjectId,
        email: dados.email ?? '',
        nome: dados.nome ?? ''
      }
      localStorage.setItem('epros_social_onboarding', JSON.stringify(prefill))
      router.replace({
        path: '/cadastro',
        query: { social: '1', provedor: prefill.provedor, email: prefill.email, nome: prefill.nome }
      })
      return
    }

    const token = dados.token
    if (!token) {
      status.value = 'erro'
      errorMessage.value = 'Não foi possível autenticar com o provedor social.'
      return
    }
    localStorage.setItem('epros_token', token)

    // 2. Vários tenants → seleção de tenant (mesma etapa do login por senha).
    if (dados.exigeSelecaoTenant) {
      const partial = {
        email: dados.email,
        usuarioId: dados.usuarioId,
        tenantId: dados.tenantId,
        tenantName: 'Selecione o tenant',
        planName: 'Plano Demo',
        status: 'Ativo',
        tenants: dados.tenants ?? []
      }
      localStorage.setItem('epros_user', JSON.stringify(partial))
      router.replace('/auth/selecionar-tenant')
      return
    }

    // 2b. Sem acesso a nenhuma empresa → mesma tela de seleção no estado vazio (onboarding/contato).
    if (dados.semAcesso) {
      const partial = {
        email: dados.email,
        usuarioId: dados.usuarioId,
        tenantId: dados.tenantId,
        tenants: [],
        semAcesso: true
      }
      localStorage.setItem('epros_user', JSON.stringify(partial))
      router.replace('/auth/selecionar-tenant')
      return
    }

    // 3. Tenant único → entra direto (grava empresa ativa como no login por senha).
    const userData = {
      email: dados.email,
      usuarioId: dados.usuarioId,
      tenantId: dados.tenantId,
      tenantName: dados.nome ?? 'Empresa',
      planName: 'Plano Demo',
      status: dados.block ? 'Atrasado' : 'Ativo',
      empresas: dados.empresas ?? []
    }
    localStorage.setItem('epros_user', JSON.stringify(userData))
    if (!dados.exigeSelecaoEmpresa && dados.empresas?.length) {
      const e0 = dados.empresas[0]
      localStorage.setItem('epros_empresa', JSON.stringify({ id: e0.empresaId, nome: e0.razaoSocial }))
    }

    if (userData.status === 'Atrasado') {
      router.replace('/area-cliente/minhas-faturas')
    } else {
      router.replace('/erp/acesso-rapido')
    }
  } catch (e) {
    status.value = 'erro'
    errorMessage.value = extrairErro(e, 'Falha na autenticação social.')
  }
}

onMounted(processar)
</script>

<template>
  <div class="callback-page">
    <div class="callback-card glass-panel">
      <div class="form-brand">
        <AppLogo :size="30" full />
      </div>

      <template v-if="status === 'processando'">
        <span class="spinner spinner-lg"></span>
        <h2 class="callback-title">Concluindo o login…</h2>
        <p class="callback-sub">Validando sua autenticação com o provedor.</p>
      </template>

      <template v-else>
        <div class="callback-error-icon">❌</div>
        <h2 class="callback-title">Não foi possível entrar</h2>
        <p class="callback-sub">{{ errorMessage }}</p>
        <NuxtLink to="/" class="btn btn-primary callback-btn">Voltar ao login</NuxtLink>
      </template>
    </div>
  </div>
</template>

<style scoped>
.callback-page {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  min-height: 100vh;
  padding: 24px;
  background: var(--bg-color);
}
.callback-card {
  width: 100%;
  max-width: 420px;
  padding: 40px 32px;
  border-radius: 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 14px;
}
.form-brand {
  margin-bottom: 10px;
}
.callback-title {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-primary);
}
.callback-sub {
  font-size: 13px;
  color: var(--text-secondary);
  line-height: 1.5;
}
.callback-error-icon {
  font-size: 32px;
}
.spinner-lg {
  width: 34px;
  height: 34px;
  border-width: 3px;
}
.callback-btn {
  margin-top: 10px;
  padding: 11px 24px;
  text-decoration: none;
}
</style>
