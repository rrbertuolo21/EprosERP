import { defineNuxtPlugin, useRuntimeConfig, navigateTo } from '#app'
import { ofetch } from 'ofetch'

/**
 * Plugin de cliente HTTP único do EprosERP.
 *
 * Sobrescreve o `$fetch` global para:
 *  1. Aplicar a `baseURL` vinda do runtimeConfig (`public.apiBaseUrl`) — sem localhost hardcodado.
 *  2. Injetar `Authorization: Bearer <token>` e `X-Tenant-Id` a partir do localStorage.
 *  3. Tratar 401: limpar a sessão e redirecionar para o login.
 *
 * As telas NUNCA devem usar `$fetch`/`ofetch` diretamente — devem usar `useApi`/`useApiList`,
 * que já embutem o prefixo `/api/v1` e a tipagem do CommandResult.
 */
export default defineNuxtPlugin(() => {
  if (import.meta.client) {
    const config = useRuntimeConfig()
    const baseURL = config.public.apiBaseUrl as string

    // Cast: ofetch.create devolve um $Fetch genérico; o global do Nuxt é tipado com
    // NitroFetchRequest. O runtime é o mesmo — apenas alinhamos os tipos.
    globalThis.$fetch = ofetch.create({
      baseURL,
      onRequest({ options }) {
        const token = localStorage.getItem('epros_token')
        const storedUser = localStorage.getItem('epros_user')

        const headers = new Headers(options.headers as HeadersInit)

        // 1. Injeta Token JWT Bearer para autorização
        if (token) {
          headers.set('Authorization', `Bearer ${token}`)
        }

        // 2. Injeta ID de Tenant ativo do usuário logado
        if (storedUser) {
          try {
            const user = JSON.parse(storedUser)
            // Aceita tanto tenantId quanto a empresa ativa selecionada
            const tenantId = user?.tenantId
            if (tenantId) {
              headers.set('X-Tenant-Id', String(tenantId))
            }
          } catch (e) {
            console.error('Erro ao ler dados do inquilino para cabeçalho da API:', e)
          }
        }

        options.headers = headers
      },
      onResponseError({ response }) {
        // 3. Sessão expirada/inválida: limpa e volta ao login.
        if (response?.status === 401) {
          localStorage.removeItem('epros_token')
          localStorage.removeItem('epros_user')
          localStorage.removeItem('epros_empresa')
          // Evita loop se já estiver no login
          if (typeof window !== 'undefined' && window.location.pathname !== '/') {
            navigateTo('/')
          }
        }
      }
    }) as typeof globalThis.$fetch
  }
})
