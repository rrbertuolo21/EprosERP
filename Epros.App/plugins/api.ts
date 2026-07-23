import { defineNuxtPlugin, useRuntimeConfig, navigateTo } from '#app'
import { ofetch } from 'ofetch'

/**
 * Cliente HTTP da API EprosERP (`$api`).
 *
 * Não substitui o `$fetch` global do Nuxt — o framework usa `$fetch` para assets
 * internos (`/_nuxt/builds/meta/...`). Apenas `useApi` / `useApiList` devem usar `$api`.
 */
export default defineNuxtPlugin(() => {
  const config = useRuntimeConfig()
  const baseURL = config.public.apiBaseUrl as string

  const api = ofetch.create({
    baseURL,
    onRequest({ options }) {
      if (import.meta.server) return

      const token = localStorage.getItem('epros_token')
      const storedUser = localStorage.getItem('epros_user')

      const headers = new Headers(options.headers as HeadersInit)

      if (token) {
        headers.set('Authorization', `Bearer ${token}`)
      }

      if (storedUser) {
        try {
          const user = JSON.parse(storedUser)
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
      if (import.meta.server) return

      if (response?.status === 401) {
        localStorage.removeItem('epros_token')
        localStorage.removeItem('epros_user')
        localStorage.removeItem('epros_empresa')
        if (typeof window !== 'undefined' && window.location.pathname !== '/') {
          navigateTo('/')
        }
      }
    }
  })

  return {
    provide: {
      api
    }
  }
})
