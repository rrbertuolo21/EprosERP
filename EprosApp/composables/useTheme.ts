import { ref, readonly } from 'vue'

/** Temas suportados pelo app. */
export type Tema = 'light' | 'dark'

const STORAGE_KEY = 'epros_tema'
const DEFAULT_TEMA: Tema = 'light'

/**
 * Estado reativo do tema, compartilhado entre todos os componentes.
 * Fica em escopo de módulo para ser um singleton (uma única fonte de verdade).
 */
const tema = ref<Tema>(DEFAULT_TEMA)
let inicializado = false

/** Lê o tema salvo em localStorage; se não houver, usa prefers-color-scheme. */
function resolverTemaInicial(): Tema {
  if (import.meta.server) return DEFAULT_TEMA
  try {
    const salvo = localStorage.getItem(STORAGE_KEY)
    if (salvo === 'light' || salvo === 'dark') return salvo
  } catch {
    /* localStorage indisponível — segue para o fallback */
  }
  // Sem preferência salva: respeita o SO, mas mantém 'dark' como padrão histórico.
  if (window.matchMedia?.('(prefers-color-scheme: light)').matches) return 'light'
  return DEFAULT_TEMA
}

/** Aplica o tema no <html> e persiste a escolha. */
function aplicar(novo: Tema, persistir = true): void {
  tema.value = novo
  if (import.meta.server) return
  document.documentElement.setAttribute('data-theme', novo)
  if (persistir) {
    try {
      localStorage.setItem(STORAGE_KEY, novo)
    } catch {
      /* ignora falha de escrita (modo privado, etc.) */
    }
  }
}

/**
 * Composable de tema (claro/escuro).
 *
 * - `tema`: estado reativo somente-leitura ('light' | 'dark').
 * - `definirTema(t)`: força um tema específico.
 * - `alternarTema()`: alterna entre claro e escuro.
 * - `inicializarTema()`: aplica o tema salvo/preferido (chamado no boot, sem flash).
 */
export function useTheme() {
  function inicializarTema(): void {
    if (inicializado) {
      // Já resolvido neste ciclo — só garante o atributo no DOM.
      aplicar(tema.value, false)
      return
    }
    inicializado = true
    aplicar(resolverTemaInicial(), false)
  }

  function definirTema(novo: Tema): void {
    aplicar(novo)
  }

  function alternarTema(): void {
    aplicar(tema.value === 'dark' ? 'light' : 'dark')
  }

  return {
    tema: readonly(tema),
    inicializarTema,
    definirTema,
    alternarTema
  }
}
