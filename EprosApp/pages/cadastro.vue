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
          <p class="step-subtitle">Pessoa jurídica (CNPJ) ou pessoa física (CPF: autônomo/produtor rural/MEI).</p>

          <!-- Toggle PF / PJ -->
          <div class="tipo-pessoa-toggle">
            <button type="button" :class="['toggle-btn', { active: form.tipoPessoa === 'PJ' }]" @click="form.tipoPessoa = 'PJ'">Pessoa Jurídica (CNPJ)</button>
            <button type="button" :class="['toggle-btn', { active: form.tipoPessoa === 'PF' }]" @click="form.tipoPessoa = 'PF'">Pessoa Física (CPF)</button>
          </div>

          <div class="form-grid">
            <div class="input-group col-span-2">
              <label for="razaoSocial">{{ form.tipoPessoa === 'PJ' ? 'Razão Social *' : 'Nome Completo *' }}</label>
              <input type="text" id="razaoSocial" v-model="form.razaoSocial" :placeholder="form.tipoPessoa === 'PJ' ? 'Ex: Minha Empresa S.A.' : 'Ex: José da Silva'" required />
            </div>

            <div v-if="form.tipoPessoa === 'PJ'" class="input-group">
              <label for="cnpj">CNPJ *</label>
              <input type="text" id="cnpj" v-model="form.cnpj" placeholder="00.000.000/0000-00" @input="handleCnpjInput" maxlength="18" required />
            </div>

            <div v-else class="input-group">
              <label for="cpf">CPF *</label>
              <input type="text" id="cpf" v-model="form.cpf" placeholder="000.000.000-00" @input="handleCpfInput" maxlength="14" required />
            </div>

            <div class="input-group">
              <label for="uf">UF *</label>
              <select id="uf" v-model="form.uf" @change="carregarMunicipios" required>
                <option value="" disabled>Selecione...</option>
                <option v-for="u in ufs" :key="u" :value="u">{{ u }}</option>
              </select>
            </div>

            <div class="input-group col-span-2">
              <label for="municipio">Município *</label>
              <select id="municipio" v-model="form.municipioIbge" :disabled="!form.uf || municipios.length === 0" required>
                <option value="" disabled>{{ form.uf ? (municipios.length ? 'Selecione o município...' : 'Carregando...') : 'Selecione a UF primeiro' }}</option>
                <option v-for="m in municipios" :key="m.codigoIbge" :value="m.codigoIbge">{{ m.nome }}</option>
              </select>
            </div>

            <div class="input-group">
              <label for="telefone">Telefone *</label>
              <input type="text" id="telefone" v-model="form.telefone" placeholder="(00) 00000-0000" required />
            </div>

            <div class="input-group">
              <label for="tipoTelefone">Tipo de Telefone *</label>
              <select id="tipoTelefone" v-model="form.tipoTelefone" required>
                <option value="Celular">Celular</option>
                <option value="Fixo">Fixo</option>
                <option value="Comercial">Comercial</option>
                <option value="Whatsapp">Whatsapp</option>
              </select>
            </div>

            <div class="input-group col-span-2">
              <label for="logradouro">Endereço (opcional)</label>
              <input type="text" id="logradouro" v-model="form.logradouro" placeholder="Logradouro" />
            </div>
            <div class="input-group">
              <label for="numero">Número</label>
              <input type="text" id="numero" v-model="form.numero" placeholder="Nº" />
            </div>
            <div class="input-group">
              <label for="bairro">Bairro</label>
              <input type="text" id="bairro" v-model="form.bairro" placeholder="Bairro" />
            </div>
            <div class="input-group">
              <label for="cep">CEP</label>
              <input type="text" id="cep" v-model="form.cep" placeholder="00000-000" maxlength="9" />
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
                placeholder="Mínimo 8 caracteres, com letra e número"
                required
              />
              <span class="input-hint">Mínimo 8 caracteres, contendo ao menos uma letra e um número.</span>
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
import { useApi } from '~/composables/useApi'

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

// 1.07 — UFs do Brasil (dado estático); municípios vêm do endpoint público por UF.
const ufs = ['AC','AL','AP','AM','BA','CE','DF','ES','GO','MA','MT','MS','MG','PA','PB','PR','PE','PI','RJ','RN','RS','RO','RR','SC','SP','SE','TO']
const municipios = ref([])

const form = reactive({
  selectedPlan: null,
  tipoPessoa: 'PJ',       // 'PJ' (CNPJ) | 'PF' (CPF)
  razaoSocial: '',
  cnpj: '',
  cpf: '',
  uf: '',
  municipioIbge: '',      // CodigoIbge (7 dígitos) exigido pelo backend
  telefone: '',
  tipoTelefone: 'Celular',
  logradouro: '',
  numero: '',
  bairro: '',
  cep: '',
  adminName: '',
  adminEmail: '',
  adminPassword: ''
})

onMounted(async () => {
  // Pre-seleciona o plano Gold (Popular) por padrão
  form.selectedPlan = plans.value.find(p => p.isPopular) || plans.value[0]

  // Carrega os planos reais (públicos). O id é um Guid do backend (PlanoId como Guid).
  try {
    const planRes = await useApi('/public/AreaPublica/planos')
    const lista = planRes?.dados ?? planRes
    if (Array.isArray(lista) && lista.length > 0) {
      plans.value = lista.map(p => ({
        id: p.id,
        name: p.nome,
        price: p.preco,
        description: p.recursosInclusos || 'Plano EprosERP.',
        features: p.recursosInclusos ? String(p.recursosInclusos).split(';') : (p.modulos || []),
        isPopular: /gold/i.test(p.nome)
      }))
      form.selectedPlan = plans.value.find(p => p.isPopular) || plans.value[0]
    }
  } catch (err) {
    console.warn('Não foi possível carregar os planos públicos. Mantendo lista padrão.', err)
  }
})

// Carrega os municípios da UF selecionada a partir do endpoint público (IBGE).
const carregarMunicipios = async () => {
  form.municipioIbge = ''
  municipios.value = []
  if (!form.uf) return
  try {
    const res = await useApi('/public/AreaPublica/municipios/{uf}', { params: { uf: form.uf } })
    const lista = res?.dados ?? res
    municipios.value = Array.isArray(lista) ? lista : []
  } catch (err) {
    console.warn('Falha ao carregar municípios da UF.', err)
    municipios.value = []
  }
}

const handleCnpjInput = (event) => {
  let cleaned = event.target.value.replace(/\D/g, '').slice(0, 14)
  let formatted = cleaned
  if (cleaned.length > 2) formatted = cleaned.slice(0, 2) + '.' + cleaned.slice(2)
  if (cleaned.length > 5) formatted = formatted.slice(0, 6) + '.' + formatted.slice(6)
  if (cleaned.length > 8) formatted = formatted.slice(0, 10) + '/' + formatted.slice(10)
  if (cleaned.length > 12) formatted = formatted.slice(0, 15) + '-' + formatted.slice(15)
  form.cnpj = formatted
}

const handleCpfInput = (event) => {
  let cleaned = event.target.value.replace(/\D/g, '').slice(0, 11)
  let formatted = cleaned
  if (cleaned.length > 3) formatted = cleaned.slice(0, 3) + '.' + cleaned.slice(3)
  if (cleaned.length > 6) formatted = formatted.slice(0, 7) + '.' + formatted.slice(7)
  if (cleaned.length > 9) formatted = formatted.slice(0, 11) + '-' + formatted.slice(11)
  form.cpf = formatted
}

const selectPlan = (plan) => {
  form.selectedPlan = plan
}

// Pol\u00edtica de senha alinhada ao backend (REG-032): m\u00ednimo 8 + ao menos uma letra e um n\u00famero.
const isSenhaValida = (senha) => senha.length >= 8 && /[A-Za-z]/.test(senha) && /[0-9]/.test(senha)

const isCurrentStepValid = computed(() => {
  if (currentStep.value === 1) {
    return !!form.selectedPlan
  }
  if (currentStep.value === 2) {
    const docOk = form.tipoPessoa === 'PJ'
      ? form.cnpj.replace(/\D/g, '').length === 14
      : form.cpf.replace(/\D/g, '').length === 11
    return form.razaoSocial.trim().length > 0 &&
      docOk &&
      String(form.municipioIbge).length === 7 &&
      form.telefone.replace(/\D/g, '').length >= 8 &&
      !!form.tipoTelefone
  }
  if (currentStep.value === 3) {
    return form.adminName.trim().length > 0 &&
      isValidEmail(form.adminEmail) &&
      isSenhaValida(form.adminPassword)
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

  // Payload real do self-register (POST /api/v1/public/auth/registrar-tenant → RegistrarNovoTenantCommand).
  // O tenant nasce em TRIAL automático (backend); o plano escolhido é referência para upgrade futuro.
  const payload = {
    NomeEmpresa: form.razaoSocial,
    Cnpj: form.tipoPessoa === 'PJ' ? form.cnpj.replace(/\D/g, '') : '',
    Cpf: form.tipoPessoa === 'PF' ? form.cpf.replace(/\D/g, '') : null,
    NomeAdmin: form.adminName,
    EmailAdmin: form.adminEmail,
    SenhaAdmin: form.adminPassword,
    CodigoIbgeMunicipio: Number(form.municipioIbge),
    Telefone: form.telefone.replace(/\D/g, ''),
    TipoTelefone: form.tipoTelefone,
    Logradouro: form.logradouro?.trim() || null,
    Numero: form.numero?.trim() || null,
    Bairro: form.bairro?.trim() || null,
    Cep: form.cep ? form.cep.replace(/\D/g, '') : null
  }

  try {
    const res = await useApi('/public/auth/registrar-tenant', { method: 'POST', body: payload })
    const ok = res?.sucesso ?? true
    if (!ok) {
      loading.value = false
      errorMessage.value = (res?.erros && res.erros.join(' ')) || res?.mensagem || 'Não foi possível concluir o cadastro.'
      return
    }
    // Sucesso: ambiente provisionado (trial). Encaminha ao login para o primeiro acesso.
    router.push('/?cadastro=ok')
  } catch (err) {
    loading.value = false
    const corpo = err?.data ?? err?.response?._data
    errorMessage.value = (corpo?.erros && corpo.erros.join(' ')) || corpo?.mensagem || 'Erro ao comunicar com o servidor. Tente novamente.'
  }
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

/* Toggle PF / PJ */
.tipo-pessoa-toggle {
  display: flex;
  gap: 8px;
  margin-bottom: 4px;
}

.toggle-btn {
  flex: 1;
  padding: 10px 12px;
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid var(--border-color);
  border-radius: 10px;
  color: var(--text-secondary);
  font-size: 12.5px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.25s ease;
}

.toggle-btn.active {
  border-color: var(--primary);
  background: rgba(99, 102, 241, 0.06);
  color: var(--text-primary);
  box-shadow: 0 0 12px rgba(99, 102, 241, 0.12);
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
