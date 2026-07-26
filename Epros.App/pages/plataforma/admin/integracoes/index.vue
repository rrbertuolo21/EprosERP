<template>
  <div class="dashboard-layout">
    <!-- Conteúdo Principal (cabeçalho/sidebar vêm do shell `admin`) -->
    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">Gateways de Pagamento</h1>
        <p class="tagline">Configure os provedores de pagamento (Mercado Pago) usados para gerar cobranças Pix nas faturas da plataforma.</p>
        <div class="header-actions">
          <NuxtLink to="/plataforma/admin" class="btn btn-secondary btn-back">
            ← Voltar ao Painel
          </NuxtLink>
          <NuxtLink to="/plataforma/admin/integracoes/novo" class="btn btn-primary">
            + Novo Gateway
          </NuxtLink>
          <span class="status-pill" :class="{ 'offline': !apiOnline }">
            <span class="status-dot"></span>
            {{ apiOnline ? 'Conectado à API Gateway' : 'Modo Simulação Offline' }}
          </span>
        </div>
      </header>

      <section class="admin-section glass-panel mt-4">
        <header class="section-header">
          <h3>Gateways Configurados</h3>
        </header>

        <div class="table-container">
          <table class="admin-table">
            <thead>
              <tr>
                <th>Provedor</th>
                <th>Ambiente</th>
                <th>Moeda</th>
                <th>Access Token</th>
                <th>Escopo</th>
                <th>Status</th>
                <th class="align-right">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="7" class="loading-cell">Carregando gateways...</td>
              </tr>
              <tr v-else-if="gateways.length === 0">
                <td colspan="7" class="empty-cell">Nenhum gateway configurado.</td>
              </tr>
              <tr v-else v-for="gw in gateways" :key="gw.id">
                <td><span class="tenant-name-txt">{{ nomeProvedor(gw.provedor) }}</span></td>
                <td>
                  <span :class="['badge', gw.ambiente === 'Producao' ? 'badge-warning' : 'badge-info']">
                    {{ gw.ambiente === 'Producao' ? 'Produção' : 'Sandbox' }}
                  </span>
                </td>
                <td>{{ gw.moeda || 'BRL' }}</td>
                <td><code class="token-mask">{{ gw.accessToken || '—' }}</code></td>
                <td>
                  <span class="tenant-id-badge">{{ gw.tenantId ? 'Tenant específico' : 'Plataforma (global)' }}</span>
                </td>
                <td>
                  <span :class="['badge', gw.ativo ? 'badge-success' : 'badge-danger']">
                    {{ gw.ativo ? 'Ativo' : 'Inativo' }}
                  </span>
                </td>
                <td class="align-right">
                  <button
                    type="button"
                    class="btn btn-secondary btn-table-action"
                    :disabled="testando === gw.id"
                    @click="testarConexao(gw)"
                  >
                    {{ testando === gw.id ? 'Testando...' : 'Testar conexão' }}
                  </button>
                  <NuxtLink
                    :to="`/plataforma/admin/integracoes/${gw.id}`"
                    class="btn btn-secondary btn-table-action"
                  >
                    Editar
                  </NuxtLink>
                  <button
                    type="button"
                    class="btn btn-secondary btn-table-action btn-danger-action"
                    @click="excluirGateway(gw)"
                  >
                    Excluir
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

// Área landlord: usa o shell administrativo (sidebar + header) do layout `admin`.
definePageMeta({ layout: 'admin' })

const apiOnline = ref(true)
const loading = ref(false)
const gateways = ref([])
const testando = ref(null)

onMounted(async () => {
  await carregarGateways()
})

// Normaliza respostas paginadas ou envelopadas em CommandResult.
const extrairLista = (res) => {
  if (Array.isArray(res)) return res
  return res?.items ?? res?.dados?.items ?? res?.dados ?? res?.data ?? []
}

const carregarGateways = async () => {
  loading.value = true
  try {
    const res = await useApi('/plataforma/gateways-pagamento')
    gateways.value = extrairLista(res)
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
    gateways.value = []
  }
  loading.value = false
}

const testarConexao = async (gw) => {
  testando.value = gw.id
  try {
    const res = await useApi(`/plataforma/gateways-pagamento/${gw.id}/testar-conexao`, { method: 'POST' })
    if (res?.sucesso === false) {
      alert(`Falha na conexão: ${res.mensagem ?? 'não foi possível conectar ao provedor.'}`)
      return
    }
    alert(res?.mensagem ?? 'Conexão estabelecida com sucesso!')
  } catch (e) {
    alert(`Erro ao testar conexão: ${e.message}`)
  } finally {
    testando.value = null
  }
}

const excluirGateway = async (gw) => {
  if (!confirm(`Deseja realmente excluir o gateway ${nomeProvedor(gw.provedor)} (${gw.ambiente === 'Producao' ? 'Produção' : 'Sandbox'})?`)) return
  try {
    const res = await useApi(`/plataforma/gateways-pagamento/${gw.id}`, { method: 'DELETE' })
    if (res?.sucesso === false) {
      alert(`Erro ao excluir: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    await carregarGateways()
  } catch (e) {
    alert(`Erro de comunicação com a API: ${e.message}`)
  }
}

const nomeProvedor = (provedor) => {
  const mapa = { MercadoPago: 'Mercado Pago' }
  return mapa[provedor] ?? provedor ?? '—'
}
</script>

<style scoped>
.header-actions {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-top: 8px;
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
.loading-cell, .empty-cell {
  text-align: center;
  padding: 48px !important;
  color: var(--text-secondary);
}
.token-mask {
  font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
  font-size: 12px;
  color: var(--text-secondary);
  letter-spacing: 0.5px;
}
.badge-success {
  background: rgba(16, 185, 129, 0.1);
  color: var(--success);
  border: 1px solid rgba(16, 185, 129, 0.2);
}
.badge-danger {
  background: rgba(239, 68, 68, 0.1);
  color: var(--danger);
  border: 1px solid rgba(239, 68, 68, 0.2);
}
.badge-warning {
  background: rgba(245, 158, 11, 0.1);
  color: #fbbf24;
  border: 1px solid rgba(245, 158, 11, 0.2);
}
.badge-info {
  background: rgba(59, 130, 246, 0.1);
  color: #60a5fa;
  border: 1px solid rgba(59, 130, 246, 0.2);
}
.mt-4 {
  margin-top: 24px;
}
</style>
