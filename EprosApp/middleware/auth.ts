// Named middleware 'auth' — alias no-op. A autenticação real é aplicada
// globalmente por auth.global.ts; este named middleware existe para que
// páginas possam declarar `definePageMeta({ middleware: 'auth' })` de forma
// explícita/tipada sem duplicar a lógica de guarda.
export default defineNuxtRouteMiddleware(() => {})
