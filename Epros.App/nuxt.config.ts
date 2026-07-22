// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  ssr: false,
  devtools: { enabled: true },
  css: ['~/assets/css/main.css'],
  app: {
    // baseURL relativa é obrigatória para que o Electron consiga carregar
    // os assets estáticos a partir do sistema de arquivos local (file:///)
    baseURL: './'
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
      apiBaseUrl: '',
      realtimeUrl: 'http://127.0.0.1:5000',
      storageUri: ''
    }
  }
})
