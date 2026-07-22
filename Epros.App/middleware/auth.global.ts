import { defineNuxtRouteMiddleware, navigateTo } from '#app'

/**
 * Guard global de rota.
 *
 * Regras:
 *  - Sem sessão: só permite Login (`/`), Cadastro (`/cadastro`) e Recuperar Senha (`/recuperar-senha`).
 *  - Super Admin (`status === 'Admin'`): forçado ao painel `/plataforma/admin`, longe da área do cliente.
 *  - Inquilino inadimplente (`status === 'Atrasado'`): preso na página de faturas até regularizar.
 *  - Inquilino adimplente: acesso liberado ao ERP (`/erp/*`), dashboard e área do cliente.
 */
export default defineNuxtRouteMiddleware((to) => {
  if (!import.meta.client) return

  const storedUser = localStorage.getItem('epros_user')

  // Rotas públicas (sem autenticação).
  const rotasPublicas = ['/', '/cadastro', '/recuperar-senha']

  // 1. Sem usuário logado.
  if (!storedUser) {
    if (!rotasPublicas.includes(to.path)) {
      return navigateTo('/')
    }
    return
  }

  // 2. Usuário logado.
  let user: { status?: string } = {}
  try {
    user = JSON.parse(storedUser)
  } catch {
    localStorage.removeItem('epros_user')
    localStorage.removeItem('epros_token')
    return navigateTo('/')
  }

  // Super Admin: mantém no painel da plataforma.
  if (user.status === 'Admin') {
    if (to.path === '/' || to.path.startsWith('/area-cliente/')) {
      return navigateTo('/plataforma/admin')
    }
    return
  }

  // Inquilino normal chegando no login ou no painel admin: redireciona conforme adimplência.
  if (to.path === '/' || to.path === '/plataforma/admin') {
    return navigateTo(user.status === 'Atrasado' ? '/area-cliente/minhas-faturas' : '/dashboard')
  }

  // Restrição de inadimplência: bloqueia tudo (inclusive /erp/*) exceto a página de faturas.
  if (user.status === 'Atrasado') {
    if (to.path !== '/area-cliente/minhas-faturas') {
      return navigateTo('/area-cliente/minhas-faturas')
    }
    return
  }

  // Inquilino adimplente: acesso liberado ao ERP (/erp/*) e demais rotas autenticadas.
})
