<template>
  <div class="dashboard-layout">
    <!-- Cabeçalho Compartilhado -->
    <AppHeader />

    <!-- Conteúdo Principal -->
    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">Escolha o plano ideal para sua operação</h1>
        <p class="tagline">Explore recursos avançados e aumente a produtividade do seu inquilino de forma escalável.</p>
      </header>

      <!-- Grade de Planos -->
      <section class="plans-grid">
        <article 
          v-for="plan in plans" 
          :key="plan.id" 
          :class="['plan-card', 'glass-panel', { 'popular': plan.isPopular, 'current': user && user.planName === plan.name }]"
        >
          <!-- Badge Popular / Atual -->
          <div v-if="user && user.planName === plan.name" class="card-status-badge current-badge">
            ✓ Plano Ativo
          </div>
          <div v-else-if="plan.isPopular" class="card-status-badge popular-badge">
            ★ Mais Escolhido
          </div>

          <header class="plan-card-header">
            <h2 class="plan-name">{{ plan.name }}</h2>
            <p class="plan-desc">{{ plan.description }}</p>
            
            <div class="price-box">
              <span class="currency">R$</span>
              <span class="price-value">{{ plan.price.toFixed(2).split('.')[0] }}</span>
              <span class="price-cents">,{{ plan.price.toFixed(2).split('.')[1] }}</span>
              <span class="period">/mês</span>
            </div>
          </header>

          <hr class="card-divider" />

          <!-- Lista de Recursos -->
          <ul class="features-list">
            <li v-for="feat in plan.features" :key="feat">
              <span class="check-icon">✓</span>
              <span class="feat-text">{{ feat }}</span>
            </li>
          </ul>

          <!-- Botão de Ação -->
          <footer class="plan-card-footer">
            <button 
              v-if="user && user.planName === plan.name" 
              class="btn btn-secondary btn-plan-action active-plan-btn" 
              disabled
            >
              Plano Ativo
            </button>
            <button 
              v-else
              @click="handleUpgrade(plan)" 
              class="btn btn-primary btn-plan-action"
              :disabled="upgradingPlanId !== null"
            >
              <span v-if="upgradingPlanId === plan.id" class="spinner"></span>
              <span v-else>Fazer Upgrade</span>
            </button>
          </footer>
        </article>
      </section>
    </main>

    <!-- Overlay de Sucesso do Upgrade -->
    <div v-if="showUpgradeSuccess" class="success-overlay">
      <div class="success-card glass-panel animate-scale">
        <div class="success-check-icon">🎉</div>
        <h2>Upgrade Realizado!</h2>
        <p>Sua assinatura foi atualizada para o <strong>{{ upgradedPlanName }}</strong>.</p>
        <p class="sub-message">Os limites e recursos do novo plano já estão disponíveis para o seu inquilino.</p>
        <button @click="showUpgradeSuccess = false" class="btn btn-success btn-success-dismiss">
          Entendido
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from '#app'
import AppHeader from '~/components/AppHeader.vue'

// Página autocontida (já renderiza seu próprio AppHeader) — sem layout do shell ERP.
definePageMeta({ layout: false })

const router = useRouter()
const user = ref(null)
const upgradingPlanId = ref(null)
const showUpgradeSuccess = ref(false)
const upgradedPlanName = ref('')
const apiOnline = ref(false)

const plans = ref([
  {
    id: 'plano-silver',
    name: 'Plano Silver',
    price: 299.90,
    description: 'Ideal para pequenas empresas iniciando no controle de gestão empresarial.',
    features: [
      'Até 5 usuários ativos',
      'Até 1.000 faturas de clientes/mês',
      'Módulos de Vendas e Estoque',
      'Suporte técnico por E-mail',
      'Backups diários automatizados'
    ],
    isPopular: false
  },
  {
    id: 'plano-gold',
    name: 'Plano Gold',
    price: 499.90,
    description: 'Perfeito para empresas em crescimento com necessidades fiscais e financeiras.',
    features: [
      'Até 15 usuários ativos',
      'Até 5.000 faturas de clientes/mês',
      'Módulos de Vendas, Estoque e Financeiro',
      'Suporte por E-mail & Chat em Tempo Real',
      'Emissão ilimitada de NF-e e NFC-e',
      'Integração de pagamentos via PIX'
    ],
    isPopular: true
  },
  {
    id: 'plano-platinum',
    name: 'Plano Platinum',
    price: 899.90,
    description: 'Completo para médias e grandes corporações multi-empresas e filiais.',
    features: [
      'Usuários ativos ilimitados',
      'Faturas e notas ilimitadas',
      'Todos os módulos inclusos + Fiscal avançado',
      'Gerente de conta e suporte dedicado 24/7',
      'Acesso à API para integrações customizadas',
      'Suporte completo a Multi-Empresas'
    ],
    isPopular: false
  }
])

onMounted(async () => {
  const storedUser = localStorage.getItem('epros_user')
  if (!storedUser) {
    router.push('/')
    return
  }
  user.value = JSON.parse(storedUser)

  // Detecta conectividade com o Backend
  try {
    await $fetch('http://localhost:5000/api/v1/plataforma/clientes').then(() => {
      apiOnline.value = true
    }).catch(() => {
      apiOnline.value = false
    })
  } catch (e) {
    apiOnline.value = false
  }

  if (apiOnline.value) {
    try {
      const planRes = await $fetch('http://localhost:5000/api/v1/public/AreaPublica/planos')
      if (planRes && planRes.length > 0) {
        plans.value = planRes.map(p => ({
          id: p.id,
          name: p.nome,
          price: p.preco,
          description: p.nome === 'Plano Silver' ? 'Ideal para pequenas empresas iniciando no controle de gestão empresarial.' : (p.nome === 'Plano Gold' ? 'Perfeito para empresas em crescimento com necessidades fiscais e financeiras.' : 'Completo para médias e grandes corporações filiais.'),
          features: p.recursosInclusos ? p.recursosInclusos.split(';') : p.modulos,
          isPopular: p.nome === 'Plano Gold'
        }))
      }
    } catch (err) {
      console.warn('Erro ao carregar planos da API, usando local storage.', err)
      loadPlansFromLocalStorage()
    }
  } else {
    loadPlansFromLocalStorage()
  }
})

const loadPlansFromLocalStorage = () => {
  const storedPlans = localStorage.getItem('epros_plans')
  if (storedPlans) {
    const updatedPlans = JSON.parse(storedPlans)
    plans.value.forEach(p => {
      const match = updatedPlans.find(up => up.id === p.id || up.name === p.name)
      if (match) {
        p.price = match.price
      }
    })
  }
}

const handleUpgrade = (plan) => {
  if (!user.value || user.value.planName === plan.name) return
  
  upgradingPlanId.value = plan.id
  
  // Simula o delay de requisição premium
  setTimeout(async () => {
    try {
      if (apiOnline.value) {
        await $fetch('http://localhost:5000/api/v1/aplicativo/assinaturas/contratar', {
          method: 'POST',
          body: {
            PlanoId: plan.id,
            MetodoPagamento: 'PIX'
          }
        })
      } else {
        // Fallback offline
        await $fetch('http://localhost:5000/api/v1/plataforma/clientes/upgrade', {
          method: 'POST',
          body: {
            tenantId: user.value.tenantId,
            planName: plan.name
          }
        }).catch(() => {})
      }
    } catch (e) {
      console.warn('Erro ao realizar upgrade na API.', e)
    }

    // Atualiza localmente o plano do usuário
    user.value.planName = plan.name
    localStorage.setItem('epros_user', JSON.stringify(user.value))

    // Dispara a animação e o estado de sucesso
    upgradedPlanName.value = plan.name
    upgradingPlanId.value = null
    showUpgradeSuccess.value = true
    
    // Auto hide success overlay after 5 seconds
    setTimeout(() => {
      showUpgradeSuccess.value = false
    }, 5000)
  }, 1200)
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
  max-width: 1200px;
  margin: 0 auto;
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 40px;
}

.page-header {
  text-align: center;
  margin-top: 20px;
}

.glow-text {
  font-size: 36px;
  font-weight: 800;
  letter-spacing: -1px;
  margin-bottom: 12px;
  background: linear-gradient(135deg, #ffffff 0%, #a1a1aa 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.tagline {
  font-size: 16px;
  color: var(--text-secondary);
  max-width: 600px;
  margin: 0 auto;
  line-height: 1.6;
}

/* Grid de Planos */
.plans-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 30px;
  align-items: stretch;
}

.plan-card {
  position: relative;
  display: flex;
  flex-direction: column;
  padding: 40px 32px;
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
  height: 100%;
}

.plan-card:hover {
  transform: translateY(-8px);
  border-color: rgba(99, 102, 241, 0.25);
  box-shadow: 0 20px 40px rgba(99, 102, 241, 0.08);
}

/* Plano Destaque Popular */
.plan-card.popular {
  border-color: rgba(99, 102, 241, 0.35);
  box-shadow: 0 10px 30px rgba(99, 102, 241, 0.05);
}

.plan-card.popular::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  border-radius: 16px;
  border: 1px solid transparent;
  background: linear-gradient(135deg, var(--primary), var(--accent-purple)) border-box;
  -webkit-mask: linear-gradient(#fff 0 0) padding-box, linear-gradient(#fff 0 0);
  -webkit-mask-composite: destination-out;
  mask-composite: exclude;
  pointer-events: none;
}

/* Plano Atual */
.plan-card.current {
  border-color: rgba(16, 185, 129, 0.3);
  box-shadow: 0 10px 30px rgba(16, 185, 129, 0.03);
}

/* Badges superiores */
.card-status-badge {
  position: absolute;
  top: -14px;
  left: 50%;
  transform: translateX(-50%);
  padding: 6px 14px;
  border-radius: 20px;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.3);
}

.popular-badge {
  background: linear-gradient(135deg, var(--primary), var(--accent-purple));
  color: #fff;
}

.current-badge {
  background: linear-gradient(135deg, var(--success), #059669);
  color: #fff;
}

/* Cabeçalho do Card */
.plan-card-header {
  margin-bottom: 28px;
}

.plan-name {
  font-size: 22px;
  font-weight: 700;
  margin-bottom: 12px;
  color: var(--text-primary);
}

.plan-desc {
  font-size: 13px;
  color: var(--text-secondary);
  line-height: 1.5;
  min-height: 45px;
  margin-bottom: 24px;
}

.price-box {
  display: flex;
  align-items: baseline;
}

.currency {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-secondary);
  margin-right: 4px;
}

.price-value {
  font-size: 44px;
  font-weight: 800;
  line-height: 1;
  letter-spacing: -1px;
  color: var(--text-primary);
}

.price-cents {
  font-size: 18px;
  font-weight: 700;
  color: var(--text-primary);
}

.period {
  font-size: 13px;
  color: var(--text-muted);
  margin-left: 6px;
}

.card-divider {
  border: none;
  border-top: 1px solid rgba(255, 255, 255, 0.05);
  margin-bottom: 28px;
}

/* Lista de Recursos */
.features-list {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-bottom: 36px;
  flex-grow: 1;
}

.features-list li {
  display: flex;
  align-items: flex-start;
  gap: 12px;
}

.check-icon {
  color: var(--primary-hover);
  font-weight: 700;
  font-size: 14px;
  line-height: 1.4;
}

.feat-text {
  font-size: 13.5px;
  color: var(--text-secondary);
  line-height: 1.4;
}

/* Botão de Ação */
.plan-card-footer {
  margin-top: auto;
}

.btn-plan-action {
  width: 100%;
  padding: 14px;
  font-size: 14px;
}

.active-plan-btn {
  background: rgba(16, 185, 129, 0.08) !important;
  color: var(--success) !important;
  border: 1px solid rgba(16, 185, 129, 0.25) !important;
  cursor: default;
}

/* Success Overlay */
.success-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.7);
  backdrop-filter: blur(6px);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 200;
}

.success-card {
  width: 440px;
  padding: 40px;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
}

.animate-scale {
  animation: scale-up 0.4s cubic-bezier(0.34, 1.56, 0.64, 1);
}

@keyframes scale-up {
  from { transform: scale(0.85); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}

.success-check-icon {
  font-size: 48px;
  margin-bottom: 8px;
  animation: bounce 1s ease infinite alternate;
}

@keyframes bounce {
  from { transform: translateY(0); }
  to { transform: translateY(-10px); }
}

.success-card h2 {
  font-size: 22px;
  font-weight: 700;
}

.success-card p {
  font-size: 14px;
  color: var(--text-secondary);
  line-height: 1.6;
}

.sub-message {
  font-size: 12px !important;
  color: var(--text-muted) !important;
}

.btn-success-dismiss {
  width: 100%;
  margin-top: 16px;
  padding: 12px;
}
</style>
