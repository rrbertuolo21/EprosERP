<template>
  <div class="dashboard-layout">
    <!-- Conteúdo Principal (o shell/header vêm do layout `admin`) -->
    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">{{ isNew ? 'Novo Gateway de Pagamento' : 'Editar Gateway de Pagamento' }}</h1>
        <p class="tagline">{{ isNew ? 'Configure as credenciais do provedor de pagamento para gerar cobranças na plataforma.' : 'Ajuste credenciais, ambiente e escopo do gateway de pagamento.' }}</p>
        <div class="header-actions">
          <NuxtLink to="/plataforma/admin/integracoes" class="btn btn-secondary btn-back">
            ← Voltar para Lista
          </NuxtLink>
          <button
            v-if="!isNew"
            type="button"
            class="btn btn-secondary"
            :disabled="testando"
            @click="testarConexao"
          >
            {{ testando ? 'Testando...' : 'Testar conexão' }}
          </button>
          <span class="status-pill" :class="{ 'offline': !apiOnline }">
            <span class="status-dot"></span>
            {{ apiOnline ? 'Conectado à API Gateway' : 'Modo Simulação Offline' }}
          </span>
        </div>
      </header>

      <div class="admin-grid-layout form-focused-layout">
        <section class="admin-section form-card glass-panel col-span-2">
          <form @submit.prevent="salvarGateway" class="vertical-form mt-2">
            <div class="form-tab-content">
              <div class="form-row">
                <div class="form-group col-6">
                  <label for="g-provedor">Provedor *</label>
                  <select id="g-provedor" v-model="gateway.provedor" required>
                    <option value="MercadoPago">Mercado Pago</option>
                  </select>
                </div>
                <div class="form-group col-6">
                  <label for="g-ambiente">Ambiente *</label>
                  <select id="g-ambiente" v-model="gateway.ambiente" required>
                    <option value="Sandbox">Sandbox</option>
                    <option value="Producao">Produção</option>
                  </select>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group col-12">
                  <label for="g-access-token">Access Token *</label>
                  <input
                    type="password"
                    id="g-access-token"
                    v-model="gateway.accessToken"
                    autocomplete="new-password"
                    :placeholder="isNew ? 'Cole aqui o Access Token do provedor' : 'Deixe em branco para manter o token atual'"
                    :required="isNew"
                  />
                  <small class="field-help">Segredo do provedor. Nunca é exibido em texto claro depois de salvo.</small>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group col-6">
                  <label for="g-public-key">Public Key</label>
                  <input
                    type="text"
                    id="g-public-key"
                    v-model="gateway.publicKey"
                    autocomplete="off"
                    placeholder="Chave pública do provedor"
                  />
                </div>
                <div class="form-group col-6">
                  <label for="g-webhook-secret">Webhook Secret</label>
                  <input
                    type="password"
                    id="g-webhook-secret"
                    v-model="gateway.webhookSecret"
                    autocomplete="new-password"
                    :placeholder="isNew ? 'Segredo de validação do webhook' : 'Em branco = manter o atual'"
                  />
                </div>
              </div>

              <div class="form-row">
                <div class="form-group col-4">
                  <label for="g-moeda">Moeda *</label>
                  <input
                    type="text"
                    id="g-moeda"
                    v-model="gateway.moeda"
                    maxlength="3"
                    placeholder="BRL"
                    required
                  />
                </div>
                <div class="form-group col-8">
                  <label for="g-notification-url">Notification URL</label>
                  <input
                    type="url"
                    id="g-notification-url"
                    v-model="gateway.notificationUrl"
                    placeholder="https://..."
                  />
                  <small class="field-help">URL que o provedor chamará para notificar mudanças de status (webhook).</small>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group col-12">
                  <label for="g-tenant">Tenant (opcional)</label>
                  <input
                    type="text"
                    id="g-tenant"
                    v-model="gateway.tenantId"
                    placeholder="Deixe vazio para escopo de plataforma"
                  />
                  <small class="field-help">Vazio = vale para toda a plataforma. Informe um Tenant para restringir este gateway a um inquilino específico.</small>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group toggle-row col-6">
                  <label for="g-ativo">Gateway Ativo</label>
                  <input type="checkbox" id="g-ativo" v-model="gateway.ativo" />
                </div>
              </div>
            </div>

            <!-- AÇÕES -->
            <footer class="form-footer mt-4">
              <button type="submit" class="btn btn-primary" :disabled="saving">
                {{ saving ? 'Gravando...' : (isNew ? 'Criar Gateway' : 'Salvar Alterações') }}
              </button>
              <NuxtLink to="/plataforma/admin/integracoes" class="btn btn-secondary">
                Cancelar
              </NuxtLink>
            </footer>
          </form>
        </section>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

// Área landlord: shell administrativo (sidebar + header) via layout `admin`.
definePageMeta({ layout: 'admin' })

const route = useRoute()
const router = useRouter()

const isNew = ref(route.params.id === 'novo')
const apiOnline = ref(true)
const saving = ref(false)
const testando = ref(false)

const gateway = reactive({
  id: '',
  provedor: 'MercadoPago',
  ambiente: 'Sandbox',
  accessToken: '',
  publicKey: '',
  webhookSecret: '',
  moeda: 'BRL',
  notificationUrl: '',
  tenantId: '',
  ativo: true
})

onMounted(async () => {
  if (!isNew.value) {
    await carregarGateway()
  }
})

const carregarGateway = async () => {
  try {
    const res = await useApi(`/plataforma/gateways-pagamento/${route.params.id}`)
    const dados = res?.dados ?? res
    Object.assign(gateway, {
      id: dados.id,
      provedor: dados.provedor ?? 'MercadoPago',
      ambiente: dados.ambiente ?? 'Sandbox',
      // Token vem mascarado no GET — mantemos o campo vazio para não reenviar a máscara.
      accessToken: '',
      publicKey: dados.publicKey ?? '',
      webhookSecret: '',
      moeda: dados.moeda ?? 'BRL',
      notificationUrl: dados.notificationUrl ?? '',
      tenantId: dados.tenantId ?? '',
      ativo: dados.ativo ?? true
    })
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
  }
}

const salvarGateway = async () => {
  saving.value = true
  try {
    const body = {
      Provedor: gateway.provedor,
      Ambiente: gateway.ambiente,
      PublicKey: gateway.publicKey || null,
      Moeda: gateway.moeda || 'BRL',
      NotificationUrl: gateway.notificationUrl || null,
      TenantId: gateway.tenantId || null,
      Ativo: gateway.ativo
    }

    // Só envia o segredo quando o usuário digitou algo (evita sobrescrever com máscara/vazio).
    if (gateway.accessToken) body.AccessToken = gateway.accessToken
    if (gateway.webhookSecret) body.WebhookSecret = gateway.webhookSecret

    let res
    if (isNew.value) {
      res = await useApi('/plataforma/gateways-pagamento', { method: 'POST', body })
    } else {
      res = await useApi(`/plataforma/gateways-pagamento/${gateway.id}`, {
        method: 'PUT',
        body: { Id: gateway.id, ...body }
      })
    }

    if (res?.sucesso === false) {
      alert(`Falha ao salvar: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    alert(isNew.value ? 'Gateway criado com sucesso!' : 'Gateway atualizado com sucesso!')
    router.push('/plataforma/admin/integracoes')
  } catch (e) {
    alert(`Erro na requisição: ${e.message}`)
  } finally {
    saving.value = false
  }
}

const testarConexao = async () => {
  testando.value = true
  try {
    const res = await useApi(`/plataforma/gateways-pagamento/${gateway.id}/testar-conexao`, { method: 'POST' })
    if (res?.sucesso === false) {
      alert(`Falha na conexão: ${res.mensagem ?? 'não foi possível conectar ao provedor.'}`)
      return
    }
    alert(res?.mensagem ?? 'Conexão estabelecida com sucesso!')
  } catch (e) {
    alert(`Erro ao testar conexão: ${e.message}`)
  } finally {
    testando.value = false
  }
}
</script>

<style scoped>
.form-focused-layout {
  max-width: 900px;
  margin: 0 auto;
}
.header-actions {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-top: 8px;
  flex-wrap: wrap;
}
.btn-back {
  padding: 8px 16px;
  font-size: 13px;
  background: rgba(255,255,255,0.02);
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
}
.btn-back:hover {
  background: rgba(255,255,255,0.06);
  color: var(--text-primary);
}
.form-row {
  display: flex;
  gap: 16px;
  margin-bottom: 12px;
}
.col-12 { flex: 0 0 100%; }
.col-8 { flex: 0 0 calc(66.66% - 5.4px); }
.col-6 { flex: 0 0 calc(50% - 8px); }
.col-4 { flex: 0 0 calc(33.33% - 10.6px); }

@media (max-width: 600px) {
  .form-row { flex-direction: column; gap: 12px; }
  .col-6, .col-4, .col-8, .col-12 { flex: 0 0 100%; }
}

.field-help {
  display: block;
  margin-top: 6px;
  font-size: 11.5px;
  color: var(--text-secondary);
}
.form-footer {
  display: flex;
  gap: 12px;
  border-top: 1px solid var(--border-color);
  padding-top: 16px;
}
.mt-2 { margin-top: 12px; }
.mt-4 { margin-top: 24px; }
</style>
