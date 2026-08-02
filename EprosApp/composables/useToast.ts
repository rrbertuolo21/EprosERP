import { useState } from '#app'

export type ToastType = 'success' | 'error' | 'warning' | 'info'

export interface ToastItem {
  id: number
  type: ToastType
  message: string
  /** Duração em ms antes de auto-fechar. 0 = não fecha automaticamente. */
  duration: number
}

let seq = 0

/**
 * Composable de notificações (toasts).
 *
 * Estado global compartilhado (via `useState`) consumido pelo `<ToastHost />` no layout.
 * As telas chamam `toast.success('...')` etc. para dar feedback de ações.
 *
 * Contrato:
 *   const toast = useToast()
 *   toast.success('Salvo com sucesso')
 *   toast.error('Falha ao salvar')
 */
export function useToast() {
  const toasts = useState<ToastItem[]>('epros-toasts', () => [])

  function push(type: ToastType, message: string, duration = 4000): number {
    const id = ++seq
    toasts.value = [...toasts.value, { id, type, message, duration }]
    if (duration > 0) {
      setTimeout(() => remove(id), duration)
    }
    return id
  }

  function remove(id: number): void {
    toasts.value = toasts.value.filter((t) => t.id !== id)
  }

  return {
    toasts,
    remove,
    success: (msg: string, duration?: number) => push('success', msg, duration),
    error: (msg: string, duration?: number) => push('error', msg, duration ?? 6000),
    warning: (msg: string, duration?: number) => push('warning', msg, duration),
    info: (msg: string, duration?: number) => push('info', msg, duration)
  }
}
