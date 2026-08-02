import { ref } from 'vue'
import { useRuntimeConfig } from '#app'
import { useAuth } from './useAuth'

/**
 * Assinatura mínima de uma conexão SignalR (evita depender do tipo do pacote no scaffolding).
 * Quando `@microsoft/signalr` estiver instalado, o objeto real implementa esta interface.
 */
interface RealtimeConnection {
  start(): Promise<void>
  stop(): Promise<void>
  on(event: string, handler: (...args: unknown[]) => void): void
  off(event: string, handler?: (...args: unknown[]) => void): void
  invoke<T = unknown>(method: string, ...args: unknown[]): Promise<T>
  readonly state: string
}

export type RealtimeHandlers = Record<string, (...args: unknown[]) => void>

/** Forma mínima do módulo @microsoft/signalr usada aqui (evita depender do tipo do pacote). */
interface SignalRBuilder {
  withUrl(url: string, options: { accessTokenFactory: () => string; transport: number }): SignalRBuilder
  withAutomaticReconnect(delays: number[]): SignalRBuilder
  build(): unknown
}
interface SignalRModule {
  HubConnectionBuilder: new () => SignalRBuilder
  HttpTransportType: { WebSockets: number }
}

/**
 * Composable de tempo real (SignalR) para emissão de documentos fiscais.
 *
 * Encapsula a criação/gerência da conexão com o hub. A URL vem de `runtimeConfig.public.realtimeUrl`
 * e o token da sessão (`useAuth`). O pacote `@microsoft/signalr` é importado dinamicamente — se não
 * estiver instalado, a conexão falha de forma controlada (log + estado desconectado), sem quebrar a UI.
 *
 * Contrato:
 *   const { conectar, on, invoke, desconectar, conectado } = useRealtime('/hubs/vendas')
 *   await conectar({ NfTransmissionCompleted: (res) => { ... } })
 */
export function useRealtime(hubPath: string) {
  const config = useRuntimeConfig()
  const { getToken } = useAuth()

  const conexao = ref<RealtimeConnection | null>(null)
  const conectando = ref(false)
  const conectado = ref(false)
  const erro = ref<unknown>(null)

  const baseUrl = (config.public.realtimeUrl as string) || (config.public.apiBaseUrl as string)

  async function conectar(handlers: RealtimeHandlers = {}): Promise<void> {
    if (conectando.value || conectado.value) return
    conectando.value = true
    erro.value = null
    try {
      // Import dinâmico via variável para não exigir o módulo em tempo de type-check
      // (o pacote pode não estar instalado no scaffolding inicial). Tipado como SignalRModule.
      const nomeModulo = '@microsoft/signalr'
      const signalR = (await import(/* @vite-ignore */ nomeModulo).catch(() => null)) as SignalRModule | null
      if (!signalR) {
        throw new Error('Pacote @microsoft/signalr não instalado.')
      }

      const url = `${baseUrl.replace(/\/$/, '')}${hubPath.startsWith('/') ? hubPath : `/${hubPath}`}`
      const builder = new signalR.HubConnectionBuilder()
        .withUrl(url, {
          accessTokenFactory: () => getToken() ?? '',
          transport: signalR.HttpTransportType.WebSockets
        })
        .withAutomaticReconnect([0, 2000, 10000, 30000])

      const conn = builder.build() as unknown as RealtimeConnection

      for (const [evento, handler] of Object.entries(handlers)) {
        conn.on(evento, handler)
      }

      await conn.start()
      conexao.value = conn
      conectado.value = true
    } catch (e) {
      erro.value = e
      conectado.value = false
      console.error(`[useRealtime:${hubPath}]`, e)
    } finally {
      conectando.value = false
    }
  }

  function on(evento: string, handler: (...args: unknown[]) => void): void {
    conexao.value?.on(evento, handler)
  }

  function off(evento: string, handler?: (...args: unknown[]) => void): void {
    conexao.value?.off(evento, handler)
  }

  async function invoke<T = unknown>(metodo: string, ...args: unknown[]): Promise<T | undefined> {
    if (!conexao.value) return undefined
    return await conexao.value.invoke<T>(metodo, ...args)
  }

  async function desconectar(): Promise<void> {
    try {
      await conexao.value?.stop()
    } finally {
      conexao.value = null
      conectado.value = false
    }
  }

  return {
    conexao,
    conectando,
    conectado,
    erro,
    conectar,
    on,
    off,
    invoke,
    desconectar
  }
}
