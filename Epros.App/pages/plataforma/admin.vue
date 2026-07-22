<template>
  <div class="dashboard-layout">
    <!-- Cabeçalho Compartilhado -->
    <AppHeader />

    <!-- Conteúdo Principal -->
    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">Painel Landlord Backoffice</h1>
        <p class="tagline">Gerencie as assinaturas, inquilinos ativos, equipe interna, configurações globais e monitore a infraestrutura da plataforma.</p>
        <span class="status-pill" :class="{ 'offline': !apiOnline }">
          <span class="status-dot"></span>
          {{ apiOnline ? 'Conectado à API Gateway (C#)' : 'Modo Simulação Offline' }}
        </span>
      </header>

      <!-- Menu de Abas Lateral/Topo -->
      <nav class="admin-tabs glass-panel">
        <button 
          v-for="tab in tabItems" 
          :key="tab.id" 
          :class="['tab-btn', { 'tab-active': currentTab === tab.id }]"
          @click="changeTab(tab.id)"
        >
          <span class="tab-icon">{{ tab.icon }}</span>
          <span class="tab-label">{{ tab.name }}</span>
        </button>
      </nav>

      <!-- Aba 1: Inquilinos & Planos -->
      <div v-if="currentTab === 'inquilinos'" class="tab-content-wrapper">
        <!-- KPIs da Plataforma -->
        <section class="kpis-grid">
          <div class="kpi-card glass-panel">
            <span class="kpi-icon">💰</span>
            <div class="kpi-info">
              <span class="kpi-label">MRR (Recorrência Mensal)</span>
              <span class="kpi-value">{{ formatAmount(mrr) }}</span>
              <span class="kpi-trend success-text">↑ 12% este mês</span>
            </div>
          </div>

          <div class="kpi-card glass-panel">
            <span class="kpi-icon">🏢</span>
            <div class="kpi-info">
              <span class="kpi-label">Inquilinos Cadastrados</span>
              <span class="kpi-value">{{ tenants.length }}</span>
              <span class="kpi-sub">{{ activeTenantsCount }} ativos no momento</span>
            </div>
          </div>

          <div class="kpi-card glass-panel">
            <span class="kpi-icon">⚠️</span>
            <div class="kpi-info">
              <span class="kpi-label">Inadimplência Crítica</span>
              <span class="kpi-value">{{ overdueTenantsCount }}</span>
              <span class="kpi-trend" :class="overdueTenantsCount > 0 ? 'danger-text' : 'success-text'">
                {{ overdueTenantsCount > 0 ? 'Requer atenção imediata' : 'Tudo regularizado' }}
              </span>
            </div>
          </div>

          <div class="kpi-card glass-panel">
            <span class="kpi-icon">📈</span>
            <div class="kpi-info">
              <span class="kpi-label">Faturamento Total (Pago)</span>
              <span class="kpi-value">{{ formatAmount(receitaTotal) }}</span>
              <span class="kpi-sub">Pix + Boletos liquidados</span>
            </div>
          </div>
        </section>

        <div class="admin-grid-layout">
          <!-- Tabela de Inquilinos -->
          <section class="admin-section tenants-section glass-panel">
            <header class="section-header" style="display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 8px;">
              <h3>Gerenciamento de Inquilinos (Tenants)</h3>
              <div style="display: flex; gap: 8px;">
                <NuxtLink to="/plataforma/admin/clientes" class="btn btn-secondary btn-sm" style="font-size: 11.5px; padding: 6px 12px;">
                  📂 Avançado (Endereços/Contratos)
                </NuxtLink>
                <NuxtLink to="/plataforma/admin/revendas" class="btn btn-secondary btn-sm" style="font-size: 11.5px; padding: 6px 12px;">
                  🤝 Revendas
                </NuxtLink>
                <NuxtLink to="/plataforma/admin/vendedores" class="btn btn-secondary btn-sm" style="font-size: 11.5px; padding: 6px 12px;">
                  💼 Vendedores
                </NuxtLink>
              </div>
            </header>
            
            <div class="table-container">
              <table class="admin-table">
                <thead>
                  <tr>
                    <th>Inquilino (ID)</th>
                    <th>Razão Social / CNPJ</th>
                    <th>Plano</th>
                    <th>Status</th>
                    <th class="align-right">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="tenant in tenants" :key="tenant.id">
                    <td><span class="tenant-id-badge">{{ tenant.tenantId }}</span></td>
                    <td>
                      <div class="tenant-details">
                        <span class="tenant-name-txt">{{ tenant.name }}</span>
                        <span class="tenant-email-txt">{{ tenant.cnpj }} · {{ tenant.email }}</span>
                      </div>
                    </td>
                    <td>{{ tenant.plan }}</td>
                    <td>
                      <span :class="['badge', getStatusBadgeClass(tenant.status)]">
                        {{ tenant.status }}
                      </span>
                    </td>
                    <td class="align-right">
                      <button 
                        v-if="tenant.status === 'Ativo'" 
                        @click="updateTenantStatus(tenant.id, 'Suspenso')" 
                        class="btn btn-secondary btn-table-action btn-danger-action"
                      >
                        Suspender
                      </button>
                      <button 
                        v-else 
                        @click="updateTenantStatus(tenant.id, 'Ativo')" 
                        class="btn btn-primary btn-table-action btn-success-action"
                      >
                        Ativar
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <!-- Cadastros Rápidos -->
          <div class="forms-sidebar-column">
            <!-- Novo Inquilino -->
            <section class="admin-section form-card glass-panel">
              <header class="section-header">
                <h3>Cadastrar Inquilino</h3>
              </header>
              <form @submit.prevent="handleCreateClient" class="vertical-form">
                <div class="form-group">
                  <label for="new-rs">Razão Social</label>
                  <input type="text" id="new-rs" v-model="newClient.RazaoSocial" placeholder="Empresa Ltda" required />
                </div>
                <div class="form-group">
                  <label for="new-cnpj">CNPJ</label>
                  <input type="text" id="new-cnpj" v-model="newClient.Cnpj" placeholder="12.345.678/0001-99" required />
                </div>
                <div class="form-group">
                  <label for="new-email">E-mail Comercial</label>
                  <input type="email" id="new-email" v-model="newClient.Email" placeholder="contato@empresa.com" required />
                </div>
                <div class="form-group">
                  <label for="new-plano">Plano Comercial</label>
                  <select id="new-plano" v-model="newClient.PlanoId" required>
                    <option value="" disabled>Selecione um plano...</option>
                    <option v-for="plan in plans" :key="plan.id" :value="plan.id">
                      {{ plan.name }} - {{ formatAmount(plan.price) }}
                    </option>
                  </select>
                </div>
                <div class="form-group toggle-row">
                  <label for="new-demo">Modo Demonstração (Demo)</label>
                  <input type="checkbox" id="new-demo" v-model="newClient.IsDemo" />
                </div>
                <button type="submit" class="btn btn-primary btn-block">Salvar Inquilino</button>
              </form>
            </section>

            <!-- Novo Plano -->
            <section class="admin-section form-card glass-panel">
              <header class="section-header">
                <h3>Criar Plano Comercial</h3>
              </header>
              <form @submit.prevent="handleCreatePlan" class="vertical-form">
                <div class="form-group">
                  <label for="plan-name">Nome do Plano</label>
                  <input type="text" id="plan-name" v-model="newPlan.Nome" placeholder="Plano Platinum" required />
                </div>
                <div class="form-group">
                  <label for="plan-price">Preço Mensal (R$)</label>
                  <input type="number" id="plan-price" v-model.number="newPlan.Preco" step="0.01" required />
                </div>
                <div class="form-group">
                  <label for="plan-limit-invoice">Limite de Faturas</label>
                  <input type="number" id="plan-limit-invoice" v-model.number="newPlan.LimiteFaturas" required />
                </div>
                <div class="form-group">
                  <label for="plan-limit-cash">Limite de Caixas</label>
                  <input type="number" id="plan-limit-cash" v-model.number="newPlan.LimiteCaixas" required />
                </div>
                <div class="form-group checkbox-channels">
                  <label>Módulos Ativos no Plano *</label>
                  <div class="channels-row">
                    <label><input type="checkbox" value="Vendas" v-model="newPlan.Modulos" /> Vendas</label>
                    <label><input type="checkbox" value="Estoque" v-model="newPlan.Modulos" /> Estoque</label>
                    <label><input type="checkbox" value="Financeiro" v-model="newPlan.Modulos" /> Financeiro</label>
                    <label><input type="checkbox" value="Fiscal" v-model="newPlan.Modulos" /> Fiscal</label>
                  </div>
                </div>
                <button type="submit" class="btn btn-primary btn-block">Salvar Plano</button>
              </form>
            </section>
          </div>
        </div>
      </div>

      <!-- Aba 2: Equipe Siser -->
      <div v-else-if="currentTab === 'equipe'" class="tab-content-wrapper">
        <div class="admin-grid-layout">
          <!-- Tabela de Membros Siser -->
          <section class="admin-section glass-panel">
            <header class="section-header">
              <h3>Membros da Equipe Siser</h3>
            </header>
            <div class="table-container">
              <table class="admin-table">
                <thead>
                  <tr>
                    <th>Nome</th>
                    <th>E-mail</th>
                    <th>Fuso Horário</th>
                    <th>Permissão</th>
                    <th class="align-right">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="user in usuariosInternos" :key="user.id">
                    <td>
                      <span class="user-name-txt">{{ user.nome }}</span>
                    </td>
                    <td>{{ user.email }}</td>
                    <td><span class="tz-badge">{{ user.timezone }}</span></td>
                    <td>
                      <span :class="['badge', user.primaryAdmin ? 'badge-paga' : 'badge-cancelada']">
                        {{ user.primaryAdmin ? 'Administrador Principal' : 'Operador' }}
                      </span>
                    </td>
                    <td class="align-right actions-cell">
                      <button 
                        v-if="!user.primaryAdmin" 
                        @click="promoteToPrimary(user.id)" 
                        class="btn btn-primary btn-table-action btn-success-action"
                      >
                        Promover a Admin
                      </button>
                      <button 
                        @click="openResetPasswordModal(user)" 
                        class="btn btn-secondary btn-table-action"
                      >
                        Nova Senha
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <!-- Cadastrar Membro Siser -->
          <section class="admin-section form-card glass-panel">
            <header class="section-header">
              <h3>Adicionar à Equipe Siser</h3>
            </header>
            <form @submit.prevent="handleCreateInternalUser" class="vertical-form">
              <div class="form-group">
                <label for="user-name">Nome Completo</label>
                <input type="text" id="user-name" v-model="newUser.Nome" placeholder="Diretor Operações" required />
              </div>
              <div class="form-group">
                <label for="user-email">E-mail</label>
                <input type="email" id="user-email" v-model="newUser.Email" placeholder="admin2@epros.com" required />
              </div>
              <div class="form-group">
                <label for="user-password">Senha de Acesso</label>
                <input type="password" id="user-password" v-model="newUser.Senha" placeholder="••••••••" required />
              </div>
              <div class="form-group">
                <label for="user-tz">Fuso Horário</label>
                <select id="user-tz" v-model="newUser.Timezone">
                  <option value="America/Sao_Paulo">Brasília (America/Sao_Paulo)</option>
                  <option value="America/Manaus">Manaus (America/Manaus)</option>
                  <option value="UTC">UTC / GMT</option>
                </select>
              </div>
              <div class="form-group toggle-row">
                <label for="user-primary">Administrador Principal</label>
                <input type="checkbox" id="user-primary" v-model="newUser.PrimaryAdmin" />
              </div>
              <button type="submit" class="btn btn-primary btn-block">Adicionar Usuário</button>
            </form>
          </section>
        </div>
      </div>

      <!-- Aba 3: Configurações Globais -->
      <div v-else-if="currentTab === 'configuracoes'" class="tab-content-wrapper">
        <div class="admin-grid-layout">
          <!-- Tabela de Configurações -->
          <section class="admin-section glass-panel">
            <header class="section-header">
              <h3>Parâmetros Globais de Sistema</h3>
            </header>
            <div class="table-container">
              <table class="admin-table">
                <thead>
                  <tr>
                    <th>Chave</th>
                    <th>Valor</th>
                    <th>Escopo</th>
                    <th>Tipo</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="setting in systemSettings" :key="setting.id">
                    <td><span class="setting-key-badge">{{ setting.chave }}</span></td>
                    <td>
                      <span v-if="setting.isSecret" class="secret-value">•••••••• (Segredo Criptografado no Vault)</span>
                      <span v-else>{{ setting.valor }}</span>
                    </td>
                    <td><span class="scope-badge">{{ setting.escopo }}</span></td>
                    <td>
                      <span :class="['badge', setting.isSecret ? 'badge-cancelada' : 'badge-paga']">
                        {{ setting.isSecret ? 'Segredo' : 'Livre' }}
                      </span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <!-- Cadastrar/Atualizar Configuração -->
          <section class="admin-section form-card glass-panel">
            <header class="section-header">
              <h3>Definir Parâmetro</h3>
            </header>
            <form @submit.prevent="handleDefineSystemSetting" class="vertical-form">
              <div class="form-group">
                <label for="setting-key">Chave de Configuração</label>
                <input type="text" id="setting-key" v-model="newSetting.Chave" placeholder="smtp_host" required />
              </div>
              <div class="form-group">
                <label for="setting-val">Valor do Parâmetro</label>
                <input type="text" id="setting-val" v-model="newSetting.Valor" placeholder="smtp.gmail.com" required />
              </div>
              <div class="form-group">
                <label for="setting-scope">Escopo</label>
                <select id="setting-scope" v-model="newSetting.Escopo">
                  <option value="global">Global (Siser)</option>
                  <option value="tenant">Por Inquilino</option>
                </select>
              </div>
              <div class="form-group toggle-row">
                <label for="setting-secret">Criptografar valor no cofre (Vault)</label>
                <input type="checkbox" id="setting-secret" v-model="newSetting.EhSegredo" />
              </div>
              <button type="submit" class="btn btn-primary btn-block">Salvar Parâmetro</button>
            </form>
          </section>
        </div>
      </div>

      <!-- Aba 4: Execuções em Massa -->
      <div v-else-if="currentTab === 'execucoes'" class="tab-content-wrapper">
        <div class="admin-grid-layout">
          <!-- Tabela de Pedidos de Execuções -->
          <section class="admin-section glass-panel">
            <header class="section-header">
              <h3>Lista de Scripts & Processamentos em Lote (Maker-Checker)</h3>
            </header>
            <div class="table-container">
              <table class="admin-table">
                <thead>
                  <tr>
                    <th>Data/Hora</th>
                    <th>Descrição</th>
                    <th>Criador</th>
                    <th>Status</th>
                    <th class="align-right">Fluxo Maker-Checker</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="exec in execucoesMassa" :key="exec.id">
                    <td>{{ formatDate(exec.criadoEm) }}</td>
                    <td>
                      <div class="exec-details">
                        <span class="exec-desc-txt">{{ exec.descricao }}</span>
                        <span class="exec-payload-txt">Payload: {{ truncate(exec.actionPayload, 50) }}</span>
                      </div>
                    </td>
                    <td>{{ exec.criadoPor }}</td>
                    <td>
                      <span :class="['badge', getExecStatusClass(exec.status)]">
                        {{ exec.status }}
                      </span>
                    </td>
                    <td class="align-right actions-cell">
                      <button 
                        v-if="exec.status === 'Draft'" 
                        @click="simulateExec(exec.id)" 
                        class="btn btn-secondary btn-table-action"
                      >
                        ⚡ Simular (Sandbox)
                      </button>
                      <button 
                        v-if="exec.status === 'Simulado'" 
                        @click="approveExec(exec.id)" 
                        class="btn btn-primary btn-table-action btn-success-action"
                      >
                        ✓ Aprovar (Checker)
                      </button>
                      <button 
                        v-if="exec.status === 'Aprovado'" 
                        @click="runExec(exec.id)" 
                        class="btn btn-primary btn-table-action"
                      >
                        🚀 Executar Físico
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <!-- Cadastrar Solicitação de Execução em Massa -->
          <section class="admin-section form-card glass-panel">
            <header class="section-header">
              <h3>Criar Solicitação em Massa</h3>
            </header>
            <form @submit.prevent="handleCreateMassExecution" class="vertical-form">
              <div class="form-group">
                <label for="exec-desc">Descrição do Script</label>
                <input type="text" id="exec-desc" v-model="newExec.Descricao" placeholder="Reajustar preços IGP-M do Plano Gold" required />
              </div>
              <div class="form-group">
                <label for="exec-payload">Action/Payload (JSON de Comandos)</label>
                <textarea id="exec-payload" v-model="newExec.ActionPayload" rows="5" placeholder='{ "planoId": "gold", "indice": 1.05 }' required></textarea>
              </div>
              <button type="submit" class="btn btn-primary btn-block">Cadastrar Solicitação</button>
            </form>
          </section>
        </div>
      </div>

      <!-- Aba 5: Comunicador & Newsletter -->
      <div v-else-if="currentTab === 'mensagens'" class="tab-content-wrapper">
        <div class="admin-grid-layout">
          <!-- Central de Newsletter -->
          <section class="admin-section glass-panel">
            <header class="section-header">
              <h3>Inscrições da Newsletter (LGPD Compliance)</h3>
            </header>
            <div class="table-container">
              <table class="admin-table">
                <thead>
                  <tr>
                    <th>Data Inscrição</th>
                    <th>E-mail do Assinante</th>
                    <th>Consentimento</th>
                    <th class="align-right">Ação</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="sub in subscribers" :key="sub.id">
                    <td>{{ formatDate(sub.criadoEm) }}</td>
                    <td>{{ sub.email }}</td>
                    <td>
                      <span :class="['badge', sub.ativo ? 'badge-paga' : 'badge-cancelada']">
                        {{ sub.ativo ? 'Consentido' : 'Descadastrado' }}
                      </span>
                    </td>
                    <td class="align-right">
                      <button 
                        v-if="sub.ativo" 
                        @click="toggleSubscriber(sub.id, 'cancelar')" 
                        class="btn btn-secondary btn-table-action btn-danger-action"
                      >
                        Opt-out (Descadastrar)
                      </button>
                      <button 
                        v-else 
                        @click="toggleSubscriber(sub.id, 'reativar')" 
                        class="btn btn-primary btn-table-action btn-success-action"
                      >
                        Opt-in (Reativar)
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <!-- Comunicador Super Admin -->
          <section class="admin-section form-card glass-panel">
            <header class="section-header">
              <h3>Comunicador Global</h3>
            </header>
            <form @submit.prevent="handleSendCommunication" class="vertical-form">
              <div class="form-group">
                <label for="msg-title">Assunto / Título</label>
                <input type="text" id="msg-title" v-model="newMsg.Titulo" placeholder="Atualização de Termos e Políticas" required />
              </div>
              <div class="form-group">
                <label for="msg-body">Mensagem</label>
                <textarea id="msg-body" v-model="newMsg.Mensagem" rows="5" placeholder="Comunicamos que a partir de amanhã..." required></textarea>
              </div>
              <div class="form-group checkbox-channels">
                <label>Canais de Notificação</label>
                <div class="channels-row">
                  <label><input type="checkbox" value="Email" v-model="newMsg.Canais" /> E-mail</label>
                  <label><input type="checkbox" value="SMS" v-model="newMsg.Canais" /> SMS</label>
                  <label><input type="checkbox" value="WhatsApp" v-model="newMsg.Canais" /> WhatsApp</label>
                </div>
              </div>
              <button type="submit" class="btn btn-primary btn-block">Disparar Comunicado</button>
            </form>
          </section>
        </div>
      </div>

      <!-- Console de Logs Geral (Compartilhado no rodapé de todas as abas) -->
      <section class="admin-section logs-console-section glass-panel">
        <header class="section-header logs-header">
          <div class="logs-header-title">
            <h3>Console de Eventos de Infraestrutura & Jobs</h3>
            <p>Monitore logs de transações, auditorias RLS e simule workers em tempo real</p>
          </div>
          <div class="simulator-actions">
            <button @click="triggerFaturamentoJob" class="btn btn-secondary btn-log-sim">
              ⚙️ Faturamento Quartz
            </button>
            <button @click="triggerOutboxProcessor" class="btn btn-secondary btn-log-sim">
              🔄 Outbox Worker
            </button>
            <button @click="clearConsoleLogs" class="btn btn-secondary btn-log-clear">
              ✕ Limpar
            </button>
          </div>
        </header>

        <!-- Terminal de Logs -->
        <div class="terminal-box" ref="terminal">
          <div v-for="(log, idx) in logs" :key="idx" :class="['log-line', log.type]">
            <span class="log-time">[{{ log.time }}]</span>
            <span class="log-msg">{{ log.message }}</span>
          </div>
        </div>
      </section>
    </main>

    <!-- Modal de Alteração de Senha -->
    <div v-if="resetPasswordModal.open" class="modal-backdrop">
      <div class="modal-card glass-panel">
        <header class="modal-header">
          <h3>Alterar Senha do Usuário</h3>
          <button @click="closeResetPasswordModal" class="btn-close-modal">✕</button>
        </header>
        <p class="modal-desc">Defina a nova senha para o operador <strong>{{ resetPasswordModal.user.nome }}</strong> ({{ resetPasswordModal.user.email }}).</p>
        <form @submit.prevent="handleResetPassword" class="vertical-form">
          <div class="form-group">
            <label for="new-pass-val">Nova Senha</label>
            <input type="password" id="new-pass-val" v-model="resetPasswordModal.newPassword" placeholder="••••••••" required />
          </div>
          <div class="modal-actions-row">
            <button type="button" @click="closeResetPasswordModal" class="btn btn-secondary">Cancelar</button>
            <button type="submit" class="btn btn-primary">Salvar Nova Senha</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, reactive } from 'vue'
import { useRouter } from '#app'
import AppHeader from '~/components/AppHeader.vue'

// Página autocontida (já renderiza seu próprio AppHeader) — sem layout do shell ERP.
definePageMeta({ layout: false })

const router = useRouter()
const apiOnline = ref(false)

// Abas do Painel
const tabItems = [
  { id: 'inquilinos', name: 'Inquilinos & Planos', icon: '🏢' },
  { id: 'equipe', name: 'Equipe Siser', icon: '👥' },
  { id: 'configuracoes', name: 'Configurações Globais', icon: '⚙️' },
  { id: 'execucoes', name: 'Execuções em Massa', icon: '⚡' },
  { id: 'mensagens', name: 'Mensagens & Newsletter', icon: '✉️' }
]
const currentTab = ref('inquilinos')

// Banco de Dados em Memória e API data
const tenants = ref([])
const plans = ref([])
const usuariosInternos = ref([])
const systemSettings = ref([])
const execucoesMassa = ref([])
const subscribers = ref([])

const logs = ref([])
const terminal = ref(null)
const totalSimulatedPayments = ref(0)
const currentUserSession = ref(null)

// Formulários e Reatividade
const newClient = reactive({ RazaoSocial: '', Cnpj: '', Email: '', PlanoId: '', IsDemo: false })
const newPlan = reactive({ Nome: '', Preco: 299.90, LimiteFaturas: 1000, LimiteCaixas: 5, Modulos: ['Vendas', 'Estoque'] })
const newUser = reactive({ Nome: '', Email: '', Senha: '', Timezone: 'America/Sao_Paulo', PrimaryAdmin: false })
const newSetting = reactive({ Chave: '', Valor: '', Escopo: 'global', EhSegredo: false })
const newExec = reactive({ Descricao: '', ActionPayload: '' })
const newMsg = reactive({ Titulo: '', Mensagem: '', Canais: ['Email'] })

// Modal de alteração de senha
const resetPasswordModal = reactive({
  open: false,
  user: null,
  newPassword: ''
})

// KPIs Dinâmicos baseados no status da API ou localStorage
const mrr = computed(() => {
  return tenants.value.reduce((acc, tenant) => {
    if (tenant.status !== 'Ativo') return acc
    // Busca preço na lista de planos simulados ou reais
    const planObj = plans.value.find(p => p.name === tenant.plan || p.id === tenant.plan)
    const val = planObj ? planObj.price : (tenant.plan.includes('Gold') ? 499.90 : (tenant.plan.includes('Platinum') ? 899.90 : 299.90))
    return acc + val
  }, 0)
})

const activeTenantsCount = computed(() => {
  return tenants.value.filter(t => t.status === 'Ativo').length
})

const overdueTenantsCount = computed(() => {
  return tenants.value.filter(t => t.status === 'Atrasado').length
})

const receitaTotal = computed(() => {
  // Retorna receita consolidada + simulações locais
  return (tenants.value.length * 300) + totalSimulatedPayments.value
})

onMounted(async () => {
  // Guard de autenticação admin
  const storedUser = localStorage.getItem('epros_user')
  if (!storedUser) {
    router.push('/')
    return
  }
  const user = JSON.parse(storedUser)
  if (user.status !== 'Admin') {
    router.push('/area-cliente/minhas-faturas')
    return
  }
  currentUserSession.value = user

  // Detecta conectividade com o Backend
  await checkApiConnection()

  // Inicializa logs do Console
  initializeLogs()

  // Carrega dados da aba inicial
  await loadData()
})

const checkApiConnection = async () => {
  try {
    // Ping API Dashboard
    await $fetch('http://localhost:5000/api/v1/plataforma/superadmin/dashboard')
    apiOnline.value = true
  } catch (e) {
    console.warn('API Gateway ou Keycloak inacessível. Usando Modo de Simulação Local (localStorage).')
    apiOnline.value = false
  }
}

const changeTab = async (tabId) => {
  currentTab.value = tabId
  await loadData()
}

const loadData = async () => {
  if (currentTab.value === 'inquilinos') {
    await loadClientes()
  } else if (currentTab.value === 'equipe') {
    await loadUsuariosInternos()
  } else if (currentTab.value === 'configuracoes') {
    await loadSystemSettings()
  } else if (currentTab.value === 'execucoes') {
    await loadExecucoesMassa()
  } else if (currentTab.value === 'mensagens') {
    await loadSubscribers()
  }
}

// ------------------------------------------------------------
// ABA 1: GERENCIAMENTO DE INQUILINOS E PLANOS
// ------------------------------------------------------------

const loadClientes = async () => {
  if (apiOnline.value) {
    try {
      // 1. Carrega Planos do backend
      const planRes = await $fetch('http://localhost:5000/api/v1/public/AreaPublica/planos')
      plans.value = planRes.map(p => ({
        id: p.id,
        name: p.nome,
        price: p.preco,
        limit: p.limiteUsuarios
      }))

      // 2. Carrega Clientes/Inquilinos do backend via novo endpoint
      const clientRes = await $fetch('http://localhost:5000/api/v1/plataforma/superadmin/clientes')

      tenants.value = clientRes.map(c => ({
        id: c.id,
        tenantId: c.tenantId,
        name: c.razaoSocial,
        email: c.email,
        cnpj: c.cnpj,
        plan: c.planoNome,
        status: c.statusSaaS === 'Active' || c.ativo ? 'Ativo' : 'Suspenso'
      }))
    } catch (e) {
      console.error('Erro ao buscar inquilinos da API. Falling back para simulado.', e)
      loadClientesSimulado()
    }
  } else {
    loadClientesSimulado()
  }
}

const loadClientesSimulado = () => {
  const storedTenants = localStorage.getItem('epros_tenants')
  if (storedTenants) {
    tenants.value = JSON.parse(storedTenants)
  } else {
    tenants.value = [
      { id: 'empresa_teste', tenantId: 'empresa_teste', name: 'Empresa Teste Ltda', plan: 'Plano Silver', status: 'Ativo', email: 'contato@teste.com', cnpj: '12.345.678/0001-90' },
      { id: 'gold_inovacao', tenantId: 'gold_inovacao', name: 'Gold Inovação Ltda', plan: 'Plano Gold', status: 'Ativo', email: 'contato@gold.com', cnpj: '98.765.432/0001-01' },
      { id: 'platinum_corp', tenantId: 'platinum_corp', name: 'Platinum Corp', plan: 'Plano Platinum', status: 'Ativo', email: 'ceo@platinum.com', cnpj: '11.222.333/0001-44' },
      { id: 'bloqueado', tenantId: 'bloqueado', name: 'Inadimplência S.A.', plan: 'Plano Silver', status: 'Atrasado', email: 'financeiro@inadimplente.com', cnpj: '55.666.777/0001-55' }
    ]
    localStorage.setItem('epros_tenants', JSON.stringify(tenants.value))
  }

  const storedPlans = localStorage.getItem('epros_plans')
  if (storedPlans) {
    plans.value = JSON.parse(storedPlans)
  } else {
    plans.value = [
      { id: 'plano-silver', name: 'Plano Silver', price: 299.90, limit: 1000 },
      { id: 'plano-gold', name: 'Plano Gold', price: 499.90, limit: 5000 },
      { id: 'plano-platinum', name: 'Plano Platinum', price: 899.90, limit: 999999 }
    ]
    localStorage.setItem('epros_plans', JSON.stringify(plans.value))
  }
}

const updateTenantStatus = async (id, newStatus) => {
  if (apiOnline.value) {
    try {
      const endpoint = newStatus === 'Suspenso' ? 'suspender' : 'ativar'
      const res = await $fetch(`http://localhost:5000/api/v1/plataforma/clientes/${id}/${endpoint}`, {
        method: 'POST'
      })
      if (res.sucesso) {
        addLog(`Inquilino atualizado com sucesso para status '${newStatus}' no banco de dados.`, 'success')
        await loadClientes()
      } else {
        addLog(`Falha ao alterar status do inquilino: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Falha na requisição HTTP: ${e.message}`, 'error')
    }
  } else {
    const idx = tenants.value.findIndex(t => t.id === id)
    if (idx !== -1) {
      tenants.value[idx].status = newStatus
      localStorage.setItem('epros_tenants', JSON.stringify(tenants.value))
      addLog(`[Simulado] Inquilino '${id}' alterado para '${newStatus}'.`, 'success')
    }
  }
}

const handleCreateClient = async () => {
  if (apiOnline.value) {
    try {
      const res = await $fetch('http://localhost:5000/api/v1/plataforma/clientes', {
        method: 'POST',
        body: {
          RazaoSocial: newClient.RazaoSocial,
          Cnpj: newClient.Cnpj.replace(/[^\d]/g, ''),
          Email: newClient.Email,
          PlanoId: newClient.PlanoId,
          IsDemo: newClient.IsDemo
        }
      })
      if (res.sucesso) {
        addLog(`Inquilino '${newClient.RazaoSocial}' cadastrado com sucesso na API!`, 'success')
        resetNewClientForm()
        await loadClientes()
      } else {
        addLog(`Falha ao cadastrar inquilino: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Falha na requisição de cadastro: ${e.message}`, 'error')
    }
  } else {
    // Simulado
    const newId = newClient.RazaoSocial.toLowerCase().replace(/[^a-z0-9]/g, '_')
    const simulatedClient = {
      id: newId,
      tenantId: newId,
      name: newClient.RazaoSocial,
      email: newClient.Email,
      cnpj: newClient.Cnpj,
      plan: 'Plano Silver',
      status: 'Ativo'
    }
    tenants.value.push(simulatedClient)
    localStorage.setItem('epros_tenants', JSON.stringify(tenants.value))
    addLog(`[Simulado] Inquilino '${newClient.RazaoSocial}' cadastrado localmente no localStorage.`, 'success')
    resetNewClientForm()
  }
}

const resetNewClientForm = () => {
  newClient.RazaoSocial = ''
  newClient.Cnpj = ''
  newClient.Email = ''
  newClient.PlanoId = ''
  newClient.IsDemo = false
}

const handleCreatePlan = async () => {
  if (apiOnline.value) {
    try {
      const res = await $fetch('http://localhost:5000/api/v1/plataforma/clientes/planos', {
        method: 'POST',
        body: {
          Nome: newPlan.Nome,
          Preco: newPlan.Preco,
          Modulos: newPlan.Modulos
        }
      })
      if (res.sucesso) {
        addLog(`Plano comercial '${newPlan.Nome}' criado com sucesso no banco de dados!`, 'success')
        resetNewPlanForm()
      } else {
        addLog(`Falha ao criar plano: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Falha na requisição de plano: ${e.message}`, 'error')
    }
  } else {
    // Simulado
    const pId = newPlan.Nome.toLowerCase().replace(/ /g, '-')
    const simulatedPlan = {
      id: pId,
      name: newPlan.Nome,
      price: newPlan.Preco,
      limit: newPlan.LimiteFaturas,
      modules: newPlan.Modulos
    }
    plans.value.push(simulatedPlan)
    localStorage.setItem('epros_plans', JSON.stringify(plans.value))
    addLog(`[Simulado] Plano '${newPlan.Nome}' criado com sucesso localmente.`, 'success')
    resetNewPlanForm()
  }
}

const resetNewPlanForm = () => {
  newPlan.Nome = ''
  newPlan.Preco = 299.90
  newPlan.LimiteFaturas = 1000
  newPlan.LimiteCaixas = 5
  newPlan.Modulos = ['Vendas', 'Estoque']
}

// ------------------------------------------------------------
// ABA 2: EQUIPE SISER
// ------------------------------------------------------------

const loadUsuariosInternos = async () => {
  if (apiOnline.value) {
    try {
      const data = await $fetch('http://localhost:5000/api/v1/plataforma/superadmin/usuarios-internos')
      usuariosInternos.value = data
    } catch (e) {
      console.error('Erro ao buscar usuarios internos da API.', e)
      loadUsuariosInternosSimulado()
    }
  } else {
    loadUsuariosInternosSimulado()
  }
}

const loadUsuariosInternosSimulado = () => {
  const storedUsers = localStorage.getItem('epros_siser_users')
  if (storedUsers) {
    usuariosInternos.value = JSON.parse(storedUsers)
  } else {
    usuariosInternos.value = [
      { id: 'admin-siser-id', nome: 'Diretor Geral', email: 'admin@epros.com', timezone: 'America/Sao_Paulo', primaryAdmin: true },
      { id: 'op1-id', nome: 'Operador Suporte 1', email: 'suporte1@epros.com', timezone: 'America/Sao_Paulo', primaryAdmin: false },
      { id: 'op2-id', nome: 'Operador Suporte 2', email: 'suporte2@epros.com', timezone: 'America/Manaus', primaryAdmin: false }
    ]
    localStorage.setItem('epros_siser_users', JSON.stringify(usuariosInternos.value))
  }
}

const handleCreateInternalUser = async () => {
  if (apiOnline.value) {
    try {
      const res = await $fetch('http://localhost:5000/api/v1/plataforma/superadmin/usuarios-internos', {
        method: 'POST',
        body: newUser
      })
      if (res.sucesso) {
        addLog(`Usuário interno '${newUser.Nome}' adicionado à equipe com sucesso!`, 'success')
        resetNewUserForm()
        await loadUsuariosInternos()
      } else {
        addLog(`Falha ao criar usuário: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Falha na requisição de usuário interno: ${e.message}`, 'error')
    }
  } else {
    // Simulado
    const simulatedUser = {
      id: Math.random().toString(36).substring(7),
      nome: newUser.Nome,
      email: newUser.Email,
      timezone: newUser.Timezone,
      primaryAdmin: newUser.PrimaryAdmin
    }
    usuariosInternos.value.push(simulatedUser)
    localStorage.setItem('epros_siser_users', JSON.stringify(usuariosInternos.value))
    addLog(`[Simulado] Usuário interno '${newUser.Nome}' criado no localStorage.`, 'success')
    resetNewUserForm()
  }
}

const promoteToPrimary = async (id) => {
  if (apiOnline.value) {
    try {
      const res = await $fetch(`http://localhost:5000/api/v1/plataforma/superadmin/usuarios-internos/${id}/tornar-admin`, {
        method: 'POST'
      })
      if (res.sucesso) {
        addLog('Usuário promovido a administrador principal com sucesso no backend!', 'success')
        await loadUsuariosInternos()
      } else {
        addLog(`Erro ao promover usuário: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Falha na requisição: ${e.message}`, 'error')
    }
  } else {
    usuariosInternos.value = usuariosInternos.value.map(u => {
      if (u.id === id) {
        return { ...u, primaryAdmin: true }
      }
      return u
    })
    localStorage.setItem('epros_siser_users', JSON.stringify(usuariosInternos.value))
    addLog('[Simulado] Usuário promovido a admin principal.', 'success')
  }
}

const openResetPasswordModal = (user) => {
  resetPasswordModal.user = user
  resetPasswordModal.newPassword = ''
  resetPasswordModal.open = true
}

const closeResetPasswordModal = () => {
  resetPasswordModal.open = false
  resetPasswordModal.user = null
  resetPasswordModal.newPassword = ''
}

const handleResetPassword = async () => {
  const userId = resetPasswordModal.user.id
  if (apiOnline.value) {
    try {
      const res = await $fetch(`http://localhost:5000/api/v1/plataforma/superadmin/usuarios-internos/${userId}/alterar-senha`, {
        method: 'PUT',
        body: {
          UsuarioInternoId: userId,
          NovaSenha: resetPasswordModal.newPassword
        }
      })
      if (res.sucesso) {
        addLog(`Senha de '${resetPasswordModal.user.nome}' redefinida com sucesso!`, 'success')
        closeResetPasswordModal()
      } else {
        addLog(`Falha ao alterar senha: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Erro na requisição: ${e.message}`, 'error')
    }
  } else {
    addLog(`[Simulado] Senha do usuário '${resetPasswordModal.user.nome}' redefinida localmente.`, 'success')
    closeResetPasswordModal()
  }
}

const resetNewUserForm = () => {
  newUser.Nome = ''
  newUser.Email = ''
  newUser.Senha = ''
  newUser.Timezone = 'America/Sao_Paulo'
  newUser.PrimaryAdmin = false
}

// ------------------------------------------------------------
// ABA 3: CONFIGURAÇÕES GLOBAIS (SYSTEM SETTINGS)
// ------------------------------------------------------------

const loadSystemSettings = async () => {
  if (apiOnline.value) {
    try {
      const data = await $fetch('http://localhost:5000/api/v1/plataforma/superadmin/configuracoes')
      systemSettings.value = data
    } catch (e) {
      console.error('Erro ao buscar configurações do sistema.', e)
      loadSystemSettingsSimulado()
    }
  } else {
    loadSystemSettingsSimulado()
  }
}

const loadSystemSettingsSimulado = () => {
  const storedSettings = localStorage.getItem('epros_system_settings')
  if (storedSettings) {
    systemSettings.value = JSON.parse(storedSettings)
  } else {
    systemSettings.value = [
      { id: '1', chave: 'smtp_host', valor: 'smtp.epros.com.br', escopo: 'global', isSecret: false },
      { id: '2', chave: 'smtp_port', valor: '587', escopo: 'global', isSecret: false },
      { id: '3', chave: 'webhook_secret_mp', valor: 'secret_hash_code_2026', escopo: 'global', isSecret: true },
      { id: '4', chave: 'trial_days', valor: '15', escopo: 'global', isSecret: false }
    ]
    localStorage.setItem('epros_system_settings', JSON.stringify(systemSettings.value))
  }
}

const handleDefineSystemSetting = async () => {
  if (apiOnline.value) {
    try {
      const res = await $fetch('http://localhost:5000/api/v1/plataforma/superadmin/configuracoes', {
        method: 'POST',
        body: newSetting
      })
      if (res.sucesso) {
        addLog(`Parâmetro '${newSetting.Chave}' salvo com sucesso!`, 'success')
        resetNewSettingForm()
        await loadSystemSettings()
      } else {
        addLog(`Falha ao definir parâmetro: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Falha na requisição de configuração: ${e.message}`, 'error')
    }
  } else {
    // Simulado
    const existingIdx = systemSettings.value.findIndex(s => s.chave === newSetting.Chave)
    if (existingIdx !== -1) {
      systemSettings.value[existingIdx].valor = newSetting.Valor
      systemSettings.value[existingIdx].escopo = newSetting.Escopo
      systemSettings.value[existingIdx].isSecret = newSetting.EhSegredo
    } else {
      systemSettings.value.push({
        id: Math.random().toString(36).substring(7),
        chave: newSetting.Chave,
        valor: newSetting.Valor,
        escopo: newSetting.Escopo,
        isSecret: newSetting.EhSegredo
      })
    }
    localStorage.setItem('epros_system_settings', JSON.stringify(systemSettings.value))
    addLog(`[Simulado] Parâmetro '${newSetting.Chave}' salvo localmente.`, 'success')
    resetNewSettingForm()
  }
}

const resetNewSettingForm = () => {
  newSetting.Chave = ''
  newSetting.Valor = ''
  newSetting.Escopo = 'global'
  newSetting.EhSegredo = false
}

// ------------------------------------------------------------
// ABA 4: EXECUÇÕES EM MASSA (MAKER-CHECKER)
// ------------------------------------------------------------

const loadExecucoesMassa = async () => {
  if (apiOnline.value) {
    try {
      const data = await $fetch('http://localhost:5000/api/v1/plataforma/superadmin/execucoes-massa-global')
      execucoesMassa.value = data
    } catch (e) {
      console.error('Erro ao buscar execuções em massa.', e)
      loadExecucoesMassaSimulado()
    }
  } else {
    loadExecucoesMassaSimulado()
  }
}

const loadExecucoesMassaSimulado = () => {
  const storedExecs = localStorage.getItem('epros_mass_execs')
  if (storedExecs) {
    execucoesMassa.value = JSON.parse(storedExecs)
  } else {
    execucoesMassa.value = [
      { id: 'exec-1', descricao: 'Reajuste IGP-M Contratos', actionPayload: '{ "index": 1.055 }', status: 'Draft', criadoPor: 'admin@epros.com', criadoEm: new Date().toISOString() }
    ]
    localStorage.setItem('epros_mass_execs', JSON.stringify(execucoesMassa.value))
  }
}

const handleCreateMassExecution = async () => {
  if (apiOnline.value) {
    try {
      const res = await $fetch('http://localhost:5000/api/v1/plataforma/superadmin/execucoes-massa-global', {
        method: 'POST',
        body: newExec
      })
      if (res.sucesso) {
        addLog('Solicitação de execução em massa criada em rascunho (Draft) com sucesso!', 'success')
        newExec.Descricao = ''
        newExec.ActionPayload = ''
        await loadExecucoesMassa()
      } else {
        addLog(`Erro ao criar solicitação: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Falha na requisição: ${e.message}`, 'error')
    }
  } else {
    const simulatedExec = {
      id: Math.random().toString(36).substring(7),
      descricao: newExec.Descricao,
      actionPayload: newExec.ActionPayload,
      status: 'Draft',
      criadoPor: currentUserSession.value.email,
      criadoEm: new Date().toISOString()
    }
    execucoesMassa.value.push(simulatedExec)
    localStorage.setItem('epros_mass_execs', JSON.stringify(execucoesMassa.value))
    addLog(`[Simulado] Solicitação '${newExec.Descricao}' criada em rascunho (Draft).`, 'success')
    newExec.Descricao = ''
    newExec.ActionPayload = ''
  }
}

const simulateExec = async (id) => {
  addLog(`Iniciando simulação dry-run (Sandbox) para execução [${id}]...`, 'info')
  if (apiOnline.value) {
    try {
      const res = await $fetch(`http://localhost:5000/api/v1/plataforma/superadmin/execucoes-massa-global/${id}/simular`, {
        method: 'POST'
      })
      if (res.sucesso) {
        addLog(`Simulação concluída com sucesso! Sandbox de Rollback forçado executado.`, 'success')
        if (res.dados && res.dados.logs) {
          addLog(`Logs da Simulação: ${res.dados.logs}`, 'info')
        }
        await loadExecucoesMassa()
      } else {
        addLog(`Falha na simulação: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Falha na requisição de simulação: ${e.message}`, 'error')
    }
  } else {
    setTimeout(() => {
      execucoesMassa.value = execucoesMassa.value.map(e => {
        if (e.id === id) return { ...e, status: 'Simulado' }
        return e
      })
      localStorage.setItem('epros_mass_execs', JSON.stringify(execucoesMassa.value))
      addLog('[Simulado] Sandbox executado. Transação fisicamente abortada no rollback.', 'success')
      addLog('LOG SANDBOX: 4 contratos analisados. Preços incrementados em 5.5% para Plano Gold. Rollback aplicado.', 'info')
    }, 1000)
  }
}

const approveExec = async (id) => {
  if (apiOnline.value) {
    try {
      const res = await $fetch(`http://localhost:5000/api/v1/plataforma/superadmin/execucoes-massa-global/${id}/ativar`, {
        method: 'POST'
      })
      if (res.sucesso) {
        addLog('Execução em massa aprovada pelo Checker com sucesso!', 'success')
        await loadExecucoesMassa()
      } else {
        addLog(`Falha na aprovação (Maker-Checker bloqueia mesmo aprovador): ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Falha na aprovação: ${e.message}`, 'error')
    }
  } else {
    // Simulação Maker-Checker
    const item = execucoesMassa.value.find(e => e.id === id)
    if (item && item.criadoPor === currentUserSession.value.email) {
      addLog('BARRADO (Maker-Checker): Você não pode aprovar um script criado por você mesmo!', 'error')
      return
    }

    execucoesMassa.value = execucoesMassa.value.map(e => {
      if (e.id === id) return { ...e, status: 'Aprovado', aprovadoPor: currentUserSession.value.email }
      return e
    })
    localStorage.setItem('epros_mass_execs', JSON.stringify(execucoesMassa.value))
    addLog('[Simulado] Aprovado com sucesso pelo Checker.', 'success')
  }
}

const runExec = async (id) => {
  addLog(`Iniciando execução definitiva física no banco de dados para [${id}]...`, 'info')
  if (apiOnline.value) {
    try {
      const res = await $fetch(`http://localhost:5000/api/v1/plataforma/superadmin/execucoes-massa-global/${id}/concluir`, {
        method: 'POST'
      })
      if (res.sucesso) {
        addLog(`Execução concluída com sucesso absoluto! Banco gravado.`, 'success')
        await loadExecucoesMassa()
      } else {
        addLog(`Falha ao executar script: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Falha na requisição: ${e.message}`, 'error')
    }
  } else {
    setTimeout(() => {
      execucoesMassa.value = execucoesMassa.value.map(e => {
        if (e.id === id) return { ...e, status: 'Concluido' }
        return e
      })
      localStorage.setItem('epros_mass_execs', JSON.stringify(execucoesMassa.value))
      addLog('[Simulado] Execução concluída. Modificações persistidas no banco físico.', 'success')
    }, 1000)
  }
}

const getExecStatusClass = (status) => {
  if (status === 'Concluido') return 'badge-paga'
  if (status === 'Aprovado') return 'badge-paga'
  if (status === 'Simulado') return 'badge-pendente'
  if (status === 'Draft') return 'badge-pendente'
  return 'badge-cancelada'
}

// ------------------------------------------------------------
// ABA 5: MENSAGENS E NEWSLETTER
// ------------------------------------------------------------

const loadSubscribers = async () => {
  if (apiOnline.value) {
    try {
      const data = await $fetch('http://localhost:5000/api/v1/plataforma/superadmin/newsletter')
      subscribers.value = data
    } catch (e) {
      console.error('Erro ao buscar assinantes da newsletter.', e)
      loadSubscribersSimulado()
    }
  } else {
    loadSubscribersSimulado()
  }
}

const loadSubscribersSimulado = () => {
  const storedSubs = localStorage.getItem('epros_newsletter')
  if (storedSubs) {
    subscribers.value = JSON.parse(storedSubs)
  } else {
    subscribers.value = [
      { id: 'sub-1', email: 'usuario.novidade@gmail.com', ativo: true, criadoEm: new Date().toISOString() },
      { id: 'sub-2', email: 'parceiro.corp@outlook.com', ativo: true, criadoEm: new Date().toISOString() },
      { id: 'sub-3', email: 'ex.cliente@terra.com.br', ativo: false, criadoEm: new Date().toISOString() }
    ]
    localStorage.setItem('epros_newsletter', JSON.stringify(subscribers.value))
  }
}

const toggleSubscriber = async (id, action) => {
  if (apiOnline.value) {
    try {
      const res = await $fetch(`http://localhost:5000/api/v1/plataforma/superadmin/newsletter/${id}/${action}`, {
        method: 'POST'
      })
      if (res.sucesso) {
        addLog(`Inscrição de e-mail atualizada com sucesso para '${action}'!`, 'success')
        await loadSubscribers()
      } else {
        addLog(`Erro ao atualizar newsletter: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Falha na requisição: ${e.message}`, 'error')
    }
  } else {
    subscribers.value = subscribers.value.map(s => {
      if (s.id === id) {
        return { ...s, ativo: action === 'reativar' }
      }
      return s
    })
    localStorage.setItem('epros_newsletter', JSON.stringify(subscribers.value))
    addLog(`[Simulado] Inscrição da newsletter alterada para '${action}'.`, 'success')
  }
}

const handleSendCommunication = async () => {
  if (newMsg.Canais.length === 0) {
    addLog('Selecione pelo menos um canal de notificação (E-mail, SMS, WhatsApp)!', 'error')
    return
  }
  if (apiOnline.value) {
    try {
      const res = await $fetch('http://localhost:5000/api/v1/plataforma/superadmin/comunicacao', {
        method: 'POST',
        body: newMsg
      })
      if (res.sucesso) {
        addLog('Comunicado gravado no Outbox assíncrono com sucesso!', 'success')
        newMsg.Titulo = ''
        newMsg.Mensagem = ''
      } else {
        addLog(`Erro ao enviar comunicado: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Falha na requisição do Comunicador: ${e.message}`, 'error')
    }
  } else {
    addLog(`[Simulado] Comunicado '${newMsg.Titulo}' enfileirado para envio nos canais [${newMsg.Canais.join(', ')}].`, 'success')
    newMsg.Titulo = ''
    newMsg.Mensagem = ''
  }
}

// ------------------------------------------------------------
// UTILS E SIMULADORES DE INFRA / LOGS
// ------------------------------------------------------------

const initializeLogs = () => {
  logs.value = [
    { time: getTimestamp(), message: 'Serviço Landlord iniciado com sucesso.', type: 'info' },
    { time: getTimestamp(), message: 'Carregadas as tabelas e schemas do banco PostgreSQL.', type: 'success' },
    { time: getTimestamp(), message: 'Outbox Worker aguardando eventos na tabela de mensagens.', type: 'info' }
  ]
}

const addLog = (message, type = 'info') => {
  logs.value.push({
    time: getTimestamp(),
    message,
    type
  })
  
  // Scroll para o fim do terminal
  setTimeout(() => {
    if (terminal.value) {
      terminal.value.scrollTop = terminal.value.scrollHeight
    }
  }, 50)
}

const getTimestamp = () => {
  const now = new Date()
  return now.toLocaleTimeString('pt-BR')
}

const formatAmount = (val) => {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(val)
}

const getStatusBadgeClass = (status) => {
  if (status === 'Ativo') return 'badge-paga'
  if (status === 'Atrasado') return 'badge-atrasada'
  return 'badge-cancelada' // Suspenso
}

const formatDate = (dateStr) => {
  if (!dateStr) return '-'
  const d = new Date(dateStr)
  return d.toLocaleDateString('pt-BR') + ' ' + d.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
}

const truncate = (text, length) => {
  if (!text) return ''
  if (text.length <= length) return text
  return text.substring(0, length) + '...'
}

const triggerFaturamentoJob = () => {
  addLog('Quartz Job: Disparada varredura de faturamento e suspensão automática.', 'info')
  setTimeout(() => {
    addLog('Quartz Job: 0 faturas vencidas suspensas. Processamento finalizado com sucesso.', 'success')
  }, 600)
}

const triggerOutboxProcessor = () => {
  addLog('Outbox Worker: Iniciando processamento de eventos do Outbox...', 'info')
  setTimeout(() => {
    addLog('Outbox Worker: 0 mensagens pendentes processadas.', 'success')
  }, 400)
}

const clearConsoleLogs = () => {
  logs.value = []
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
  padding: 0 24px 40px;
  max-width: 1300px;
  margin: 0 auto;
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.page-header {
  text-align: center;
  margin-top: 10px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.glow-text {
  font-size: 32px;
  font-weight: 800;
  letter-spacing: -1px;
  background: linear-gradient(135deg, #ffffff 0%, #a1a1aa 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.tagline {
  font-size: 13.5px;
  color: var(--text-secondary);
  max-width: 700px;
}

.status-pill {
  font-size: 11px;
  font-weight: 600;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: rgba(16, 185, 129, 0.08);
  border: 1px solid rgba(16, 185, 129, 0.25);
  color: var(--success);
  padding: 4px 12px;
  border-radius: 20px;
}

.status-pill.offline {
  background: rgba(245, 158, 11, 0.08);
  border-color: rgba(245, 158, 11, 0.25);
  color: var(--warning);
}

.status-dot {
  width: 6px;
  height: 6px;
  background: var(--success);
  border-radius: 50%;
  box-shadow: 0 0 8px var(--success);
}

.status-pill.offline .status-dot {
  background: var(--warning);
  box-shadow: 0 0 8px var(--warning);
}

/* Abas de Navegação */
.admin-tabs {
  display: flex;
  gap: 8px;
  padding: 8px;
  border-radius: 12px;
  justify-content: center;
}

.tab-btn {
  background: transparent;
  border: 1px solid transparent;
  color: var(--text-secondary);
  padding: 10px 18px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.2s ease-in-out;
}

.tab-btn:hover {
  background: rgba(255, 255, 255, 0.03);
  color: var(--text-primary);
}

.tab-active {
  background: rgba(255, 255, 255, 0.06);
  border-color: var(--border-color);
  color: var(--text-primary);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

/* Tab Content */
.tab-content-wrapper {
  display: flex;
  flex-direction: column;
  gap: 20px;
  animation: fade-slide-in 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes fade-slide-in {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}

/* KPIs Grid */
.kpis-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 16px;
}

.kpi-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px;
}

.kpi-icon {
  font-size: 26px;
  padding: 10px;
  background: rgba(255, 255, 255, 0.02);
  border-radius: 10px;
  border: 1px solid var(--border-color);
}

.kpi-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.kpi-label {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--text-secondary);
}

.kpi-value {
  font-size: 22px;
  font-weight: 700;
  color: var(--text-primary);
}

.kpi-trend, .kpi-sub {
  font-size: 11px;
  color: var(--text-muted);
}

.success-text { color: var(--success); }
.danger-text { color: var(--danger); }

/* Layout Grid */
.admin-grid-layout {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 20px;
}

@media (max-width: 950px) {
  .admin-grid-layout {
    grid-template-columns: 1fr;
  }
}

.forms-sidebar-column {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.admin-section {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.section-header h3 {
  font-size: 15px;
  font-weight: 750;
  color: var(--text-primary);
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
  padding-bottom: 10px;
}

/* Tabelas */
.table-container {
  overflow-x: auto;
}

.admin-table {
  width: 100%;
  border-collapse: collapse;
}

.admin-table th {
  padding: 10px 14px;
  font-size: 10.5px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--text-secondary);
  border-bottom: 1px solid var(--border-color);
  text-align: left;
}

.admin-table td {
  padding: 12px 14px;
  font-size: 13px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.02);
}

.tenant-id-badge, .setting-key-badge, .tz-badge, .scope-badge {
  font-family: monospace;
  font-size: 11px;
  background: rgba(255, 255, 255, 0.03);
  padding: 2px 6px;
  border-radius: 4px;
  border: 1px solid var(--border-color);
}

.tenant-details, .exec-details {
  display: flex;
  flex-direction: column;
}

.tenant-name-txt, .user-name-txt, .exec-desc-txt {
  font-weight: 600;
  color: var(--text-primary);
}

.tenant-email-txt, .exec-payload-txt {
  font-size: 11px;
  color: var(--text-muted);
}

.actions-cell {
  display: flex;
  gap: 6px;
  justify-content: flex-end;
}

.align-right { text-align: right; }

.btn-table-action {
  padding: 4px 10px;
  font-size: 11px;
}

.btn-danger-action {
  background: rgba(239, 68, 68, 0.06) !important;
  color: var(--danger) !important;
  border: 1px solid rgba(239, 68, 68, 0.2) !important;
}
.btn-danger-action:hover { background: rgba(239, 68, 68, 0.12) !important; }

.btn-success-action {
  background: rgba(16, 185, 129, 0.06) !important;
  color: var(--success) !important;
  border: 1px solid rgba(16, 185, 129, 0.2) !important;
}
.btn-success-action:hover { background: rgba(16, 185, 129, 0.12) !important; }

/* Formulários */
.vertical-form {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-group label {
  font-size: 10px;
  font-weight: 600;
  text-transform: uppercase;
  color: var(--text-secondary);
}

.form-group input, .form-group select, .form-group textarea {
  padding: 8px 12px;
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  color: var(--text-primary);
  font-size: 12.5px;
  width: 100%;
}

.form-group input:focus, .form-group select:focus, .form-group textarea:focus {
  outline: none;
  border-color: var(--primary);
  background: rgba(99, 102, 241, 0.04);
}

.toggle-row {
  flex-direction: row;
  justify-content: space-between;
  align-items: center;
}

.toggle-row input[type="checkbox"] {
  width: auto;
}

.btn-block {
  width: 100%;
  padding: 10px;
}

.secret-value {
  color: var(--text-muted);
  font-family: monospace;
  font-size: 11px;
}

/* Canais checkbox */
.checkbox-channels {
  gap: 8px;
}
.channels-row {
  display: flex;
  gap: 16px;
}
.channels-row label {
  font-size: 12px;
  text-transform: none;
  color: var(--text-primary);
  display: flex;
  align-items: center;
  gap: 4px;
}
.channels-row input {
  width: auto;
}

/* Console de Logs */
.logs-console-section {
  margin-top: 20px;
}

.logs-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
  padding-bottom: 12px;
}

.logs-header-title h3 {
  font-size: 15px;
  font-weight: 700;
  margin-bottom: 2px;
}

.logs-header-title p {
  font-size: 11.5px;
  color: var(--text-secondary);
}

.simulator-actions {
  display: flex;
  gap: 6px;
}

.btn-log-sim {
  font-size: 11.5px;
  padding: 6px 12px;
}

.btn-log-clear {
  font-size: 11.5px;
  padding: 6px 12px;
  color: var(--text-muted);
}

.terminal-box {
  background: rgba(0, 0, 0, 0.45);
  border: 1px solid var(--border-color);
  border-radius: 10px;
  height: 180px;
  padding: 14px;
  font-family: monospace;
  font-size: 12px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.log-line { line-height: 1.4; }
.log-time { color: var(--text-muted); margin-right: 6px; }
.log-line.info .log-msg { color: #c7d2fe; }
.log-line.success .log-msg { color: var(--success); }
.log-line.warn .log-msg { color: var(--warning); }
.log-line.error .log-msg { color: var(--danger); }

/* Modal Backdrop */
.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.65);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 200;
  backdrop-filter: blur(4px);
}

.modal-card {
  width: 440px;
  padding: 24px;
  animation: modal-scale-in 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes modal-scale-in {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
  padding-bottom: 12px;
  margin-bottom: 12px;
}

.modal-header h3 {
  font-size: 16px;
  font-weight: 700;
}

.btn-close-modal {
  background: transparent;
  border: none;
  color: var(--text-muted);
  font-size: 16px;
  cursor: pointer;
}
.btn-close-modal:hover { color: var(--text-primary); }

.modal-desc {
  font-size: 12.5px;
  color: var(--text-secondary);
  line-height: 1.5;
  margin-bottom: 16px;
}

.modal-actions-row {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 16px;
}
</style>
