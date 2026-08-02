<template>
  <div class="dashboard-layout">
    <AppHeader />

    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">Gerenciar assinatura</h1>
        <p class="tagline">
          Cancele sua assinatura quando quiser. Você mantém acesso somente-leitura por 30 dias para exportar
          seus dados e pode reativar dentro desse período.
        </p>
      </header>

      <section class="glass-panel manage-panel">
        <!-- Estado: assinatura ativa → oferece cancelamento -->
        <div v-if="!cancelada">
          <h2 class="panel-title">Cancelar assinatura</h2>
          <p class="panel-desc">
            Ao cancelar, o acesso operacional é bloqueado, mas você continua com <strong>leitura e exportação</strong>
            por 30 dias. Nada é apagado nesse período.
          </p>

          <label class="field-label" for="motivo">Motivo (opcional)</label>
          <textarea
            id="motivo"
            v-model="motivo"
            class="field-input"
            rows="3"
            placeholder="Conte pra gente por que está cancelando (ajuda a melhorar o produto)."
          ></textarea>

          <button
            class="btn btn-danger btn-block"
            :disabled="processando"
            @click="cancelar"
          >
            <span v-if="processando" class="spinner"></span>
            <span v-else>Cancelar minha assinatura</span>
          </button>
        </div>

        <!-- Estado: cancelada → oferece reativação dentro da janela -->
        <div v-else>
          <div class="status-badge cancelada-badge">Assinatura cancelada</div>
          <h2 class="panel-title">Reativar assinatura</h2>
          <p class="panel-desc">
            Sua assinatura está cancelada. Dentro da janela de 30 dias você pode reativá-la e voltar a operar
            normalmente, mantendo seus dados.
          </p>

          <button
            class="btn btn-success btn-block"
            :disabled="processando"
            @click="reativar"
          >
            <span v-if="processando" class="spinner"></span>
            <span v-else>Reativar minha assinatura</span>
          </button>
        </div>

        <p v-if="mensagem" :class="['result-message', erro ? 'is-error' : 'is-ok']">{{ mensagem }}</p>
      </section>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from '#app'
import AppHeader from '~/components/AppHeader.vue'
import { useApi } from '~/composables/useApi'

// Página autocontida (renderiza seu próprio AppHeader).
definePageMeta({ layout: false })

const router = useRouter()
const user = ref(null)
const motivo = ref('')
const processando = ref(false)
const mensagem = ref('')
const erro = ref(false)
const cancelada = ref(false)

onMounted(async () => {
  const storedUser = localStorage.getItem('epros_user')
  if (!storedUser) {
    router.push('/')
    return
  }
  user.value = JSON.parse(storedUser)

  // Detecta se a assinatura vigente já está cancelada (área do cliente).
  try {
    const vigente = await useApi('/aplicativo/assinaturas/vigente')
    if (vigente && vigente.status === 'Cancelada') {
      cancelada.value = true
    }
  } catch (e) {
    // Sem assinatura vigente ou API offline: mantém o fluxo de cancelamento por padrão.
  }
})

const cancelar = async () => {
  processando.value = true
  mensagem.value = ''
  erro.value = false
  try {
    const res = await useApi('/aplicativo/assinaturas/cancelar', {
      method: 'POST',
      body: { Motivo: motivo.value || null }
    })
    cancelada.value = true
    mensagem.value = (res && res.mensagem) ||
      'Assinatura cancelada. Você tem 30 dias de acesso somente-leitura para exportar seus dados.'
  } catch (e) {
    erro.value = true
    mensagem.value = 'Não foi possível cancelar agora. Tente novamente em instantes.'
  } finally {
    processando.value = false
  }
}

const reativar = async () => {
  processando.value = true
  mensagem.value = ''
  erro.value = false
  try {
    const res = await useApi('/aplicativo/assinaturas/reativar', { method: 'POST' })
    cancelada.value = false
    mensagem.value = (res && res.mensagem) || 'Assinatura reativada com sucesso.'
  } catch (e) {
    erro.value = true
    mensagem.value = 'Não foi possível reativar. A janela de 30 dias pode ter expirado — contrate um novo plano.'
  } finally {
    processando.value = false
  }
}
</script>

<style scoped>
.dashboard-layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  z-index: 10;
}

.dashboard-content {
  flex: 1;
  padding: 0 20px 60px;
  max-width: 720px;
  margin: 0 auto;
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.page-header {
  text-align: center;
  margin-top: 20px;
}

.glow-text {
  font-size: 32px;
  font-weight: 800;
  letter-spacing: -1px;
  margin-bottom: 12px;
  background: linear-gradient(135deg, #ffffff 0%, #a1a1aa 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.tagline {
  font-size: 15px;
  color: var(--text-secondary);
  max-width: 560px;
  margin: 0 auto;
  line-height: 1.6;
}

.manage-panel {
  padding: 36px 32px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.panel-title {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-primary);
}

.panel-desc {
  font-size: 14px;
  color: var(--text-secondary);
  line-height: 1.6;
}

.field-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-secondary);
}

.field-input {
  width: 100%;
  padding: 12px 14px;
  border-radius: 10px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: rgba(255, 255, 255, 0.03);
  color: var(--text-primary);
  font-size: 14px;
  resize: vertical;
}

.btn-block {
  width: 100%;
  padding: 14px;
  font-size: 14px;
  margin-top: 8px;
}

.btn-danger {
  background: linear-gradient(135deg, #ef4444, #b91c1c);
  color: #fff;
  border: none;
}

.status-badge {
  align-self: flex-start;
  padding: 6px 14px;
  border-radius: 20px;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.cancelada-badge {
  background: rgba(239, 68, 68, 0.12);
  color: #ef4444;
  border: 1px solid rgba(239, 68, 68, 0.25);
}

.result-message {
  font-size: 13.5px;
  line-height: 1.5;
  margin-top: 8px;
}

.is-ok {
  color: var(--success);
}

.is-error {
  color: #ef4444;
}
</style>
