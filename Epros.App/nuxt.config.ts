// https://nuxt.com/docs/api/configuration/nuxt-config
const isElectronBuild = process.env.NUXT_ELECTRON === 'true'

export default defineNuxtConfig({
  ssr: false,
  devtools: { enabled: process.env.NODE_ENV !== 'production' },
  css: ['~/assets/css/main.css'],
  app: {
    // baseURL relativa é obrigatória para Electron (file:///).
    // Deploy web em produção usa '/' para assets absolutos via Caddy.
    baseURL: isElectronBuild ? './' : '/'
  },
  // Proxy dev: evita CORS entre frontend e API.
  // `nitro.devProxy` reescrevia o mesmo caminho `/api/v1`, então o proxy do Vite
  // abaixo é a única fonte de verdade em dev (evita conflito e erro de tipagem).
  vite: {
    server: {
      proxy: {
        '/api/v1': {
          target: 'http://127.0.0.1:5000',
          changeOrigin: true
        }
      }
    }
  },
  runtimeConfig: {
    public: {
      // Vazio em dev = mesma origem via proxy (/api/v1/...)
      apiBaseUrl: process.env.NUXT_PUBLIC_API_BASE_URL || '',
      realtimeUrl: process.env.NUXT_PUBLIC_REALTIME_URL || 'http://127.0.0.1:5000',
      storageUri: process.env.NUXT_PUBLIC_STORAGE_URI || ''
    }
  }
})
