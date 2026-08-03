<script setup lang="ts">
/**
 * Recuperar Senha — tela pública de solicitação de nova senha por e-mail.
 *
 * Porta o comportamento de `recuperarSenha.vue` do legado: valida o e-mail e chama
 * o AccountController para disparar o envio da nova senha/link de redefinição.
 *
 * Endpoint: POST /account/gerar-nova-senha/{email}
 */
import { ref } from 'vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import AppLogo from '~/components/AppLogo.vue'

definePageMeta({ layout: 'guest' })

const email = ref('')
const enviando = ref(false)
const erroEmail = ref('')

const regexEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

const toast = useToast()

function validar(): boolean {
  erroEmail.value = ''
  if (!email.value) {
    erroEmail.value = 'Informe o e-mail.'
    return false
  }
  if (!regexEmail.test(email.value)) {
    erroEmail.value = 'Informe um e-mail válido.'
    return false
  }
  return true
}

async function recuperarSenha() {
  if (!validar()) {
    toast.error('Informe um e-mail válido.')
    return
  }

  enviando.value = true
  try {
    const resposta = await useApi<{ dados?: string; data?: string; mensagem?: string }>(
      '/account/gerar-nova-senha/{email}',
      { method: 'POST', params: { email: email.value } }
    )
    toast.success(resposta?.dados ?? resposta?.data ?? resposta?.mensagem ?? 'E-mail de recuperação enviado com sucesso.')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    enviando.value = false
  }
}
</script>

<template>
  <div class="recuperar-senha-wrapper">
    <main class="recuperar-senha-card glass-panel">
      <header class="card-header">
        <div class="logo-container">
          <AppLogo :size="30" full />
        </div>
        <h2 class="title">Recuperar senha</h2>
        <p class="tagline">Digite seu e-mail para recuperar sua senha</p>
      </header>

      <form class="recuperar-form" @submit.prevent="recuperarSenha">
        <div class="input-group">
          <label for="email">E-mail</label>
          <input
            id="email"
            v-model="email"
            type="email"
            class="input"
            :class="{ 'is-invalid': !!erroEmail }"
            placeholder="seu@email.com"
            required
          />
          <span v-if="erroEmail" class="field-error">{{ erroEmail }}</span>
        </div>

        <button type="submit" class="btn btn-primary submit-btn" :disabled="enviando">
          <span v-if="!enviando">Enviar</span>
          <span v-else class="spinner"></span>
        </button>

        <p class="back-prompt">
          Lembrou sua senha?
          <NuxtLink to="/" class="back-link">Voltar ao login</NuxtLink>
        </p>
      </form>
    </main>
  </div>
</template>

<style scoped>
.recuperar-senha-wrapper {
  width: 100%;
  min-height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
}

.recuperar-senha-card {
  width: 420px;
  padding: 40px;
}

.card-header {
  text-align: center;
  margin-bottom: 32px;
}

.logo-container {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  margin-bottom: 8px;
}

.title {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-primary);
  margin-top: 16px;
}

.tagline {
  font-size: 13px;
  color: var(--text-secondary);
  margin-top: 6px;
}

.recuperar-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.input-group label {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.8px;
  color: var(--text-secondary);
}

.submit-btn {
  margin-top: 8px;
  padding: 12px;
}

.back-prompt {
  text-align: center;
  font-size: 13px;
  color: var(--text-secondary);
}

.back-link {
  color: var(--primary);
  text-decoration: none;
  font-weight: 600;
  margin-left: 6px;
}

.back-link:hover {
  text-decoration: underline;
}
</style>
