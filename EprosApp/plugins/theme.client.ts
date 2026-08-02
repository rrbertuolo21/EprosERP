import { useTheme } from '~/composables/useTheme'

/**
 * Aplica o tema salvo (ou o preferido pelo SO) o mais cedo possível no boot,
 * antes da montagem do app, evitando "flash" do tema errado.
 */
export default defineNuxtPlugin(() => {
  const { inicializarTema } = useTheme()
  inicializarTema()
})
