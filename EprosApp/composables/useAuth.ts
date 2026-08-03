import { ref } from 'vue'
import { navigateTo } from '#app'
import { useApi } from './useApi'

/** Payload decodificado do JWT emitido pela API EprosERP. */
export interface JwtPayload {
  tenantId?: string
  id?: string
  empresaId?: string
  pessoaGrupoId?: string
  produtoGrupoId?: string
  login?: string
  acesso?: string
  isAdmin?: string
  exp?: number
  [key: string]: unknown
}

/** Empresa acessível pelo usuário (retornada no login). */
export interface Empresa {
  id: number
  nome: string
  cnpj?: string
  razaoSocial?: string
}

/** Usuário de sessão persistido em localStorage (`epros_user`). */
export interface UsuarioSessao {
  id?: number | string
  email?: string
  tenantId?: string
  tenantName?: string
  planName?: string
  /** Status SaaS: 'Admin' | 'Atrasado' | 'Ativo' | ... (controla o middleware). */
  status?: string
  isAdmin?: boolean
  empresas?: Empresa[]
  [key: string]: unknown
}

/** Resposta esperada do endpoint de login. */
export interface LoginResponse {
  token: string
  tenantId?: string
  empresas?: Empresa[]
  usuario?: UsuarioSessao
  [key: string]: unknown
}

const TOKEN_KEY = 'epros_token'
const USER_KEY = 'epros_user'
const EMPRESA_KEY = 'epros_empresa'

/**
 * Decodifica o payload de um JWT sem validar assinatura (só leitura no cliente).
 */
export function decodeJWT(token?: string | null): JwtPayload | null {
  if (!token) return null
  try {
    const base64Url = token.split('.')[1]
    const base64 = base64Url?.replace(/-/g, '+').replace(/_/g, '/')
    const jsonPayload = decodeURIComponent(
      window.atob(base64 ?? '')
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    )
    return JSON.parse(jsonPayload) as JwtPayload
  } catch {
    return null
  }
}

/**
 * Composable de autenticação/sessão.
 *
 * Responsável por: login, logout, `me`, leitura/escrita de `epros_token`/`epros_user`,
 * decodificação de JWT e verificação de expiração. É a única fonte de verdade da sessão.
 */
export function useAuth() {
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  function getToken(): string | null {
    if (import.meta.server) return null
    return localStorage.getItem(TOKEN_KEY)
  }

  function getUser(): UsuarioSessao | null {
    if (import.meta.server) return null
    const raw = localStorage.getItem(USER_KEY)
    if (!raw) return null
    try {
      return JSON.parse(raw) as UsuarioSessao
    } catch {
      return null
    }
  }

  function setSession(token: string, usuario: UsuarioSessao): void {
    localStorage.setItem(TOKEN_KEY, token)
    localStorage.setItem(USER_KEY, JSON.stringify(usuario))
  }

  function clearSession(): void {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
    localStorage.removeItem(EMPRESA_KEY)
  }

  /** Verifica se há token e se ele não está expirado (campo `exp` do JWT). */
  function isAuthenticated(): boolean {
    const token = getToken()
    if (!token) return false
    const payload = decodeJWT(token)
    if (payload?.exp) {
      return payload.exp * 1000 > Date.now()
    }
    // Sem exp legível: considera autenticado se há usuário armazenado.
    return !!getUser()
  }

  const isAdmin = () => getUser()?.status === 'Admin' || getUser()?.isAdmin === true

  /**
   * Efetua login. Persiste token + usuário e devolve o usuário de sessão.
   * @param rota rota de login da API (default `/account/login`).
   */
  async function login(
    email: string,
    senha: string,
    rota = '/account/login'
  ): Promise<UsuarioSessao | null> {
    carregando.value = true
    erro.value = null
    try {
      const resp = await useApi.post<LoginResponse>(rota, { email, senha })
      const token = resp?.token
      if (!token) throw new Error('Token não retornado pela API.')

      const payload = decodeJWT(token)
      const usuario: UsuarioSessao = {
        ...(resp.usuario ?? {}),
        email: resp.usuario?.email ?? email,
        tenantId: resp.tenantId ?? payload?.tenantId,
        isAdmin: payload?.isAdmin?.toLowerCase() === 'true',
        empresas: resp.empresas ?? resp.usuario?.empresas
      }
      setSession(token, usuario)
      return usuario
    } catch (e) {
      erro.value = e instanceof Error ? e.message : 'Falha ao autenticar.'
      console.error('[useAuth.login]', e)
      return null
    } finally {
      carregando.value = false
    }
  }

  /** Recarrega os dados do usuário logado a partir da API. */
  async function me(rota = '/account/me'): Promise<UsuarioSessao | null> {
    try {
      const usuario = await useApi.get<UsuarioSessao>(rota)
      if (usuario) {
        const atual = getUser() ?? {}
        setSession(getToken() ?? '', { ...atual, ...usuario })
      }
      return usuario
    } catch (e) {
      console.error('[useAuth.me]', e)
      return null
    }
  }

  /** Encerra a sessão e redireciona para o login. */
  async function logout(): Promise<void> {
    clearSession()
    await navigateTo('/')
  }

  return {
    carregando,
    erro,
    getToken,
    getUser,
    setSession,
    clearSession,
    isAuthenticated,
    isAdmin,
    decodeJWT,
    login,
    me,
    logout
  }
}
