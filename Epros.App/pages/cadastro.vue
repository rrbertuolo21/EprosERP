<template>
  <div class="register-wrapper">
    <main class="register-card glass-panel" :class="{ 'loading-mode': loading }">
      <!-- Header -->
      <header class="card-header">
        <div class="logo-container">
          <AppLogo :size="30" full />
        </div>
        <p class="tagline">Cadastro & Onboarding de Inquilinos</p>
      </header>

      <!-- Steps Progress Tracker -->
      <section class="steps-tracker" v-if="!loading">
        <div :class="['step-indicator', { 'active': currentStep >= 1, 'completed': currentStep > 1 }]">
          <span class="step-num">1</span>
          <span class="step-label">Plano</span>
        </div>
        <div class="step-line" :class="{ 'filled': currentStep > 1 }"></div>
        <div :class="['step-indicator', { 'active': currentStep >= 2, 'completed': currentStep > 2 }]">
          <span class="step-num">2</span>
          <span class="step-label">Empresa</span>
        </div>
        <div class="step-line" :class="{ 'filled': currentStep > 2 }"></div>
        <div :class="['step-indicator', { 'active': currentStep >= 3 }]">
          <span class="step-num">3</span>
          <span class="step-label">Administrador</span>
        </div>
      </section>

      <!-- Main Form / Wizard Steps -->
      <div v-if="!loading" class="wizard-content">
        <!-- STEP 1: Plan Selection -->
        <div v-if="currentStep === 1" class="step-panel animate-fade">
          <h2 class="step-title">Escolha o seu plano</h2>
          <p class="step-subtitle">Selecione o plano ideal para a escala atual da sua empresa.</p>

          <div class="plans-selection-grid">
            <article 
              v-for="plan in plans" 
              :key="plan.id" 
              :class="['plan-select-card', { 'selected': form.selectedPlan?.id === plan.id, 'popular': plan.isPopular }]"
              @click="selectPlan(plan)"
            >
              <div v-if="plan.isPopular" class="popular-tag">★ Mais Escolhido</div>
              <h3 class="plan-name">{{ plan.name }}</h3>
              <p class="plan-desc">{{ plan.description }}</p>
              <div class="plan-price">
                <span class="currency">R$</span>
                <span class="price">{{ plan.price.toFixed(2).replace('.', ',') }}</span>
                <span class="period">/mês</span>
              </div>
              <ul class="features-list-mini">
                <li v-for="feat in plan.features.slice(0, 3)" :key="feat">
                  <span class="check-icon">✓</span> {{ feat }}
                </li>
              </ul>
            </article>
          </div>
        </div>

        <!-- STEP 2: Corporate Data -->
        <div v-if="currentStep === 2" class="step-panel animate-fade">
          <h2 class="step-title">Dados da sua Empresa</h2>
          <p class="step-subtitle">Insira as informações de faturamento e regime tributário.</p>

          <div class="form-grid">
            <div class="input-group">
              <label for="razaoSocial">Razão Social *</label>
              <input 
                type="text" 
                id="razaoSocial" 
                v-model="form.razaoSocial" 
                placeholder="Ex: Minha Empresa S.A."
                required
              />
            </div>

            <div class="input-group">
              <label for="cnpj">CNPJ *</label>
              <input 
                type="text" 
                id="cnpj" 
                v-model="form.cnpj" 
                placeholder="00.000.000/0000-00"
                @input="handleCnpjInput"
                maxlength="18"
                required
              />
            </div>

            <div class="input-group">
              <label for="nomeFantasia">Nome Fantasia (Será seu Tenant ID) *</label>
              <input 
                type="text" 
                id="nomeFantasia" 
                v-model="form.nomeFantasia" 
                placeholder="Ex: minhaempresa"
                required
              />
              <span class="input-hint" v-if="form.nomeFantasia">
                Identificador único no sistema: <code>{{ slugifiedTenantId }}</code>
              </span>
            </div>

            <div class="input-group">
              <label for="regimeTributario">Regime Tributário *</label>
              <select id="regimeTributario" v-model="form.regimeTributario" required>
                <option value="" disabled>Selecione um regime...</option>
                <option value="Simples Nacional">Simples Nacional</option>
                <option value="Lucro Presumido">Lucro Presumido</option>
                <option value="Lucro Real">Lucro Real</option>
              </select>
            </div>
          </div>
        </div>

        <!-- STEP 3: Admin Credentials -->
        <div v-if="currentStep === 3" class="step-panel animate-fade">
          <h2 class="step-title">Conta do Administrador</h2>
          <p class="step-subtitle">Crie suas credenciais para gerenciar a plataforma ERP.</p>

          <div class="form-grid">
            <div class="input-group">
              <label for="adminName">Nome Completo *</label>
              <input 
                type="text" 
                id="adminName" 
                v-model="form.adminName" 
                placeholder="Seu nome completo"
                required
              />
            </div>

            <div class="input-group">
              <label for="adminEmail">E-mail Corporativo *</label>
              <input 
                type="email" 
                id="adminEmail" 
                v-model="form.adminEmail" 
                placeholder="seu@email.com"
                required
              />
            </div>

            <div class="input-group col-span-2">
              <label for="adminPassword">Senha de Acesso *</label>
              <input 
                type="password" 
                id="adminPassword" 
                v-model="form.adminPassword" 
                placeholder="Mínimo 6 caracteres"
                required
              />
            </div>
          </div>
        </div>

        <!-- Validation Error Message -->
        <div v-if="errorMessage" class="error-box">
          <span class="error-icon">⚠️</span>
          <p class="error-msg">{{ errorMessage }}</p>
        </div>

        <!-- Wizard Action Buttons -->
        <footer class="wizard-actions">
          <button 
            type="button" 
            class="btn btn-secondary" 
            @click="prevStep" 
            :disabled="currentStep === 1"
          >
            Voltar
          </button>
          
          <button 
            v-if="currentStep < 3" 
            type="button" 
            class="btn btn-primary" 
            @click="nextStep"
            :disabled="!isCurrentStepValid"
          >
            Avançar
          </button>
          
          <button 
            v-else 
            type="button" 
            class="btn btn-success" 
            @click="handleRegister"
            :disabled="!isCurrentStepValid"
          >
            Concluir Cadastro
          </button>
        </footer>
      </div>

      <!-- Spinner/Loading Overlay for Auto-Provisioning -->
      <div v-else class="loading-overlay-wizard animate-fade">
        <span class="provision-icon">⚙️</span>
        <h3>Provisionando Ambiente...</h3>
        <p>Estamos configurando seu banco de dados isolado, tabelas de vendas, estoque e integrando o faturamento no plano selecionado.</p>
        <div class="progress-bar-container">
          <div class="progress-bar-fill"></div>
        </div>
        <span class="spinner"></span>
      </div>

      <footer class="card-footer-wizard" v-if="!loading">
        <p>Já possui uma conta? <NuxtLink to="/">Fazer Login</NuxtLink></p>
      </footer>
    </main>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from '#app'
import AppLogo from '~/components/AppLogo.vue'

definePageMeta({ layout: 'guest' })

const router = useRouter()
const currentStep = ref(1)
const loading = ref(false)
const errorMessage = ref('')

const plans = ref([
  {
    id: 'plano-silver',
    name: 'Plano Silver',
    price: 299.90,
    description: 'Ideal para pequenas empresas iniciando no controle de gestão empresarial.',
    features: [
      'Até 5 usuários ativos',
      'Até 1.000 faturas de clientes/mês',
      'Módulos de Vendas e Estoque'
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
      'Emissão ilimitada de NF-e e NFC-e'
    ],
    isPopular: true
  },
  {
    id: 'plano-platinum',
    name: 'Plano Platinum',
    price: 899.90,
    description: 'Completo para médias e grandes corporações filiais.',
    features: [
      'Usuários ativos ilimitados',
      'Faturas e notas ilimitadas',
      'Todos os módulos inclusos + Fiscal',
      'Suporte prioritário 24/7'
    ],
    isPopular: false
  }
])

const form = reactive({
  selectedPlan: null,
  razaoSocial: '',
  cnpj: '',
  nomeFantasia: '',
  regimeTributario: '',
  adminName: '',
  adminEmail: '',
  adminPassword: ''
})

onMounted(async () => {
  // Pre-seleciona o plano Gold (Popular) por padrão
  form.selectedPlan = plans.value.find(p => p.isPopular) || plans.value[0]

  let apiOnline = false
  try {
    await $fetch('http://localhost:5000/api/v1/plataforma/clientes').then(() => {
      apiOnline = true
    }).catch(() => {
      apiOnline = false
    })
  } catch (e) {
    apiOnline = false
  }

  if (apiOnline) {
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
        form.selectedPlan = plans.value.find(p => p.isPopular) || plans.value[0]
      }
    } catch (err) {
      console.warn('Erro ao carregar planos da API pública no wizard.', err)
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

const slugifiedTenantId = computed(() => {
  if (!form.nomeFantasia) return ''
  return form.nomeFantasia.toLowerCase()
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/[^a-z0-9]/g, "_")
    .replace(/^_+|_+$/g, "")
})

const handleCnpjInput = (event) => {
  let value = event.target.value
  let cleaned = value.replace(/\D/g, '')
  if (cleaned.length > 14) cleaned = cleaned.slice(0, 14)
  
  let formatted = cleaned
  if (cleaned.length > 2) {
    formatted = cleaned.slice(0, 2) + '.' + cleaned.slice(2)
  }
  if (cleaned.length > 5) {
    formatted = formatted.slice(0, 6) + '.' + formatted.slice(6)
  }
  if (cleaned.length > 8) {
    formatted = formatted.slice(0, 10) + '/' + formatted.slice(10)
  }
  if (cleaned.length > 12) {
    formatted = formatted.slice(0, 15) + '-' + formatted.slice(15)
  }
  form.cnpj = formatted
}

const selectPlan = (plan) => {
  form.selectedPlan = plan
}

const isCurrentStepValid = computed(() => {
  if (currentStep.value === 1) {
    return !!form.selectedPlan
  }
  if (currentStep.value === 2) {
    return form.razaoSocial.trim().length > 0 &&
      form.cnpj.replace(/\D/g, '').length === 14 &&
      form.nomeFantasia.trim().length > 0 &&
      form.regimeTributario.trim().length > 0
  }
  if (currentStep.value === 3) {
    return form.adminName.trim().length > 0 &&
      isValidEmail(form.adminEmail) &&
      form.adminPassword.length >= 6
  }
  return false
})

const isValidEmail = (email) => {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
}

const nextStep = () => {
  errorMessage.value = ''
  if (isCurrentStepValid.value && currentStep.value < 3) {
    currentStep.value++
  }
}

const prevStep = () => {
  errorMessage.value = ''
  if (currentStep.value > 1) {
    currentStep.value--
  }
}

const handleRegister = async () => {
  if (!isCurrentStepValid.value) return
  
  loading.value = true
  errorMessage.value = ''
  const tenantId = slugifiedTenantId.value

  const payload = {
    RazaoSocial: form.razaoSocial,
    Cnpj: form.cnpj.replace(/\D/g, ''),
    Email: form.adminEmail,
    PlanoId: form.selectedPlan.id,
    NomeContato: form.adminName,
    Telefone: ''
  }

  // Tenta enviar POST à API real
  try {
    await $fetch('http://localhost:5000/api/v1/plataforma/clientes', {
      method: 'POST',
      body: payload
    }).catch(err => {
      console.warn('API real offline/indisponível. Provisionando localmente no localStorage.', err)
    })
  } catch (error) {
    // Ignora falhas de CORS ou indisponibilidade
  }

  // Simula o delay de provisionamento com a barra de progresso
  setTimeout(() => {
    let tenants = []
    const storedTenants = localStorage.getItem('epros_tenants')
    if (storedTenants) {
      tenants = JSON.parse(storedTenants)
    } else {
      tenants = [
        { id: 'empresa_teste', name: 'Empresa Teste Ltda', plan: 'Plano Silver', status: 'Ativo', email: 'contato@teste.com' },
        { id: 'gold_inovacao', name: 'Gold Inovação Ltda', plan: 'Plano Gold', status: 'Ativo', email: 'contato@gold.com' },
        { id: 'platinum_corp', name: 'Platinum Corp', plan: 'Plano Platinum', status: 'Ativo', email: 'ceo@platinum.com' },
        { id: 'bloqueado', name: 'Inadimplência S.A.', plan: 'Plano Bronze', status: 'Atrasado', email: 'financeiro@inadimplente.com' }
      ]
    }

    if (tenants.some(t => t.id === tenantId)) {
      loading.value = false
      errorMessage.value = `O identificador único de inquilino '${tenantId}' já está em uso. Escolha outro Nome Fantasia.`
      return
    }

    // Registra inquilino no localStorage
    const newTenant = {
      id: tenantId,
      name: form.razaoSocial,
      plan: form.selectedPlan.name,
      status: 'Ativo',
      email: form.adminEmail,
      cnpj: form.cnpj,
      fantasia: form.nomeFantasia,
      regime: form.regimeTributario
    }
    tenants.push(newTenant)
    localStorage.setItem('epros_tenants', JSON.stringify(tenants))

    // Grava login ativo do usuário
    const userData = {
      email: form.adminEmail,
      tenantId: tenantId,
      tenantName: form.razaoSocial,
      planName: form.selectedPlan.name,
      status: 'Ativo'
    }
    localStorage.setItem('epros_user', JSON.stringify(userData))

    loading.value = false
    router.push('/dashboard?new=true')
  }, 1800)
}
</script>

<style scoped>
.register-wrapper {
  width: 100%;
  min-height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 40px 20px;
  z-index: 10;
}

.register-card {
  width: 680px;
  max-width: 100%;
  padding: 40px;
  transition: all 0.5s cubic-bezier(0.16, 1, 0.3, 1);
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.register-card.loading-mode {
  width: 500px;
}

.card-header {
  text-align: center;
}

.logo-container {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  margin-bottom: 8px;
}

.tagline {
  font-size: 13px;
  color: var(--text-secondary);
}

/* Progress Tracker */
.steps-tracker {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin: 10px 0 20px;
  padding: 0 10px;
}

.step-indicator {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  position: relative;
  z-index: 2;
  transition: all 0.3s ease;
}

.step-num {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  color: var(--text-muted);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 13px;
  font-weight: 700;
  transition: all 0.3s ease;
}

.step-label {
  font-size: 11px;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  transition: all 0.3s ease;
}

.step-indicator.active .step-num {
  background: var(--bg-color);
  border-color: var(--primary);
  color: var(--text-primary);
  box-shadow: 0 0 15px rgba(99, 102, 241, 0.3);
}

.step-indicator.active .step-label {
  color: var(--text-primary);
}

.step-indicator.completed .step-num {
  background: var(--success);
  border-color: var(--success);
  color: #fff;
  box-shadow: 0 0 15px rgba(16, 185, 129, 0.3);
}

.step-indicator.completed .step-label {
  color: var(--success);
}

.step-line {
  flex: 1;
  height: 2px;
  background: var(--border-color);
  margin-top: -18px; /* alinhar com o circulo */
  position: relative;
  z-index: 1;
}

.step-line.filled {
  background: linear-gradient(90deg, var(--success), var(--primary));
}

/* Step panel animations */
.animate-fade {
  animation: fade-in 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes fade-in {
  from { opacity: 0; transform: translateY(12px); }
  to { opacity: 1; transform: translateY(0); }
}

.step-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.step-title {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-primary);
  text-align: center;
}

.step-subtitle {
  font-size: 13.5px;
  color: var(--text-secondary);
  text-align: center;
  margin-bottom: 12px;
}

/* Plans Grid selection */
.plans-selection-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 16px;
}

.plan-select-card {
  background: rgba(255, 255, 255, 0.01);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 24px 16px;
  cursor: pointer;
  position: relative;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

.plan-select-card:hover {
  transform: translateY(-4px);
  border-color: rgba(255, 255, 255, 0.15);
  background: rgba(255, 255, 255, 0.02);
}

.plan-select-card.selected {
  border-color: var(--primary);
  background: rgba(99, 102, 241, 0.03);
  box-shadow: 0 8px 25px rgba(99, 102, 241, 0.12);
}

.plan-select-card.selected::after {
  content: '✓';
  position: absolute;
  top: 10px;
  right: 10px;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: var(--primary);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 700;
}

.plan-select-card.popular {
  border-color: rgba(99, 102, 241, 0.25);
}

.plan-select-card.popular.selected {
  border-color: var(--primary);
}

.popular-tag {
  position: absolute;
  top: -10px;
  background: linear-gradient(135deg, var(--primary), var(--accent-purple));
  color: white;
  font-size: 8px;
  font-weight: 800;
  text-transform: uppercase;
  padding: 3px 8px;
  border-radius: 8px;
  letter-spacing: 0.5px;
}

.plan-name {
  font-size: 15px;
  font-weight: 700;
  margin-bottom: 6px;
}

.plan-desc {
  font-size: 11px;
  color: var(--text-secondary);
  line-height: 1.4;
  min-height: 44px;
  margin-bottom: 12px;
}

.plan-price {
  margin-bottom: 16px;
  display: flex;
  align-items: baseline;
  justify-content: center;
}

.plan-price .currency {
  font-size: 11px;
  color: var(--text-secondary);
  font-weight: 600;
}

.plan-price .price {
  font-size: 24px;
  font-weight: 800;
  color: var(--text-primary);
}

.plan-price .period {
  font-size: 10px;
  color: var(--text-muted);
}

.features-list-mini {
  list-style: none;
  font-size: 10.5px;
  color: var(--text-muted);
  display: flex;
  flex-direction: column;
  gap: 6px;
  border-top: 1px solid rgba(255, 255, 255, 0.04);
  padding-top: 12px;
  width: 100%;
}

.features-list-mini li {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
}

.features-list-mini .check-icon {
  color: var(--success);
}

/* Forms Grid */
.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.col-span-2 {
  grid-column: span 2;
}

@media (max-width: 600px) {
  .form-grid {
    grid-template-columns: 1fr;
  }
  .col-span-2 {
    grid-column: span 1;
  }
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

.input-group input, .input-group select {
  width: 100%;
  padding: 12px 16px;
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid var(--border-color);
  border-radius: 10px;
  color: var(--text-primary);
  font-size: 14px;
  transition: all 0.3s ease;
}

.input-group input:focus, .input-group select:focus {
  outline: none;
  border-color: var(--primary);
  background: rgba(99, 102, 241, 0.04);
  box-shadow: 0 0 15px rgba(99, 102, 241, 0.15);
}

.input-group select option {
  background: #18181b;
  color: var(--text-primary);
}

.input-hint {
  font-size: 11px;
  color: var(--text-muted);
}

.input-hint code {
  color: var(--primary-hover);
  background: rgba(255, 255, 255, 0.03);
  padding: 2px 4px;
  border-radius: 4px;
}

/* Action Buttons */
.wizard-actions {
  display: flex;
  justify-content: space-between;
  margin-top: 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.04);
  padding-top: 20px;
  gap: 12px;
}

.wizard-actions button {
  flex: 1;
}

/* Error Box */
.error-box {
  background: rgba(239, 68, 68, 0.06);
  border: 1px solid rgba(239, 68, 68, 0.2);
  border-radius: 10px;
  padding: 12px 16px;
  display: flex;
  align-items: center;
  gap: 12px;
}

.error-icon {
  font-size: 16px;
}

.error-msg {
  font-size: 12.5px;
  color: #fca5a5;
  font-weight: 500;
  line-height: 1.4;
}

/* Loading Overlay inside Card */
.loading-overlay-wizard {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 40px 20px;
  gap: 16px;
}

.provision-icon {
  font-size: 52px;
  animation: spin 3s linear infinite;
  display: inline-block;
}

.loading-overlay-wizard h3 {
  font-size: 22px;
  font-weight: 800;
  background: linear-gradient(to right, #ffffff, #c7d2fe);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.loading-overlay-wizard p {
  font-size: 13.5px;
  color: var(--text-secondary);
  line-height: 1.6;
  max-width: 380px;
}

/* Progress bar inside wizard loading */
.progress-bar-container {
  width: 100%;
  height: 6px;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 3px;
  overflow: hidden;
  margin: 16px 0;
}

.progress-bar-fill {
  height: 100%;
  width: 0%;
  background: linear-gradient(90deg, var(--primary), var(--accent-purple));
  animation: fillProgress 1.6s ease-in-out forwards;
}

@keyframes fillProgress {
  0% { width: 0%; }
  100% { width: 100%; }
}

.card-footer-wizard {
  text-align: center;
  font-size: 13px;
  color: var(--text-secondary);
  border-top: 1px solid rgba(255, 255, 255, 0.04);
  padding-top: 16px;
  margin-top: 12px;
}

.card-footer-wizard a {
  color: var(--primary);
  text-decoration: none;
  font-weight: 600;
}

.card-footer-wizard a:hover {
  text-decoration: underline;
}
</style>
