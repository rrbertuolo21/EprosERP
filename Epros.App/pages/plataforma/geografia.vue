<template>
  <div class="dashboard-layout">
    <!-- Cabeçalho Compartilhado -->
    <AppHeader />

    <!-- Conteúdo Principal -->
    <main class="dashboard-content">
      <header class="page-header animate-fade-in">
        <h1 class="glow-text">Gestão de Geografia e Localização</h1>
        <p class="tagline">Gerencie a base de países, estados, municípios, histórico de buscas de CEP e configure as zonas de entrega logísticas.</p>
        <span class="status-pill" :class="{ 'offline': !apiOnline }">
          <span class="status-dot animate-pulse"></span>
          {{ apiOnline ? 'Conectado à API Gateway (C#)' : 'Modo Simulação Offline' }}
        </span>
      </header>

      <!-- Menu de Abas -->
      <nav class="admin-tabs glass-panel animate-slide-up">
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

      <!-- Seção: Países e Estados -->
      <div v-if="currentTab === 'paises'" class="tab-content-wrapper animate-fade-in">
        <div class="admin-grid-layout">
          <!-- Form Novo País -->
          <section class="admin-section glass-panel">
            <header class="section-header">
              <h3>Cadastrar Novo País</h3>
            </header>
            <form @submit.prevent="handleCreatePais" class="vertical-form">
              <div class="form-group">
                <label>Nome do País</label>
                <input type="text" v-model="newPais.Nome" placeholder="Ex: Argentina" required />
              </div>
              <div class="form-row-2">
                <div class="form-group">
                  <label>ISO Alpha-2</label>
                  <input type="text" v-model="newPais.CodigoIsoAlpha2" placeholder="Ex: AR" maxlength="2" required />
                </div>
                <div class="form-group">
                  <label>ISO Alpha-3</label>
                  <input type="text" v-model="newPais.CodigoIsoAlpha3" placeholder="Ex: ARG" maxlength="3" required />
                </div>
              </div>
              <div class="form-row-2">
                <div class="form-group">
                  <label>Código Numérico</label>
                  <input type="text" v-model="newPais.CodigoNumerico" placeholder="Ex: 032" maxlength="3" required />
                </div>
                <div class="form-group">
                  <label>DDI (Código Discagem)</label>
                  <input type="text" v-model="newPais.CodigoDiscagem" placeholder="Ex: +54" />
                </div>
              </div>
              <div class="form-group">
                <label>Capital</label>
                <input type="text" v-model="newPais.Capital" placeholder="Ex: Buenos Aires" />
              </div>
              <button type="submit" class="btn btn-primary btn-block">Salvar País</button>
            </form>
          </section>

          <!-- Listagem de Países -->
          <section class="admin-section glass-panel flex-grow-2">
            <header class="section-header flex-row-between">
              <h3>Países Cadastrados</h3>
              <button @click="sincronizarGeografiaBase" class="btn btn-secondary btn-small">
                ⚡ Sincronizar Base BR (IBGE)
              </button>
            </header>
            <div class="table-container">
              <table class="admin-table">
                <thead>
                  <tr>
                    <th>País</th>
                    <th>ISO Alpha-2</th>
                    <th>ISO Alpha-3</th>
                    <th>Código Numérico</th>
                    <th>DDI</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="pais in paises" :key="pais.id" class="table-row-hover">
                    <td class="font-bold text-primary">{{ pais.nome }}</td>
                    <td><span class="tz-badge">{{ pais.codigoIsoAlpha2 }}</span></td>
                    <td><span class="tz-badge">{{ pais.codigoIsoAlpha3 }}</span></td>
                    <td>{{ pais.codigoNumerico }}</td>
                    <td>{{ pais.codigoDiscagem || 'N/A' }}</td>
                    <td>
                      <span class="status-badge" :class="pais.ativo ? 'active' : 'inactive'">
                        {{ pais.ativo ? 'Ativo' : 'Inativo' }}
                      </span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>
        </div>
      </div>

      <!-- Seção: Municípios -->
      <div v-if="currentTab === 'municipios'" class="tab-content-wrapper animate-fade-in">
        <section class="admin-section glass-panel">
          <header class="section-header flex-row-between">
            <h3>Municípios e Cidades</h3>
            <div class="filter-controls">
              <input type="text" v-model="filterMunName" placeholder="🔍 Filtrar por nome..." class="search-input" />
              <select v-model="selectedUf" class="select-input">
                <option value="">Todas as UFs</option>
                <option v-for="uf in ufsList" :key="uf" :value="uf">{{ uf }}</option>
              </select>
            </div>
          </header>
          <div class="table-container">
            <table class="admin-table">
              <thead>
                <tr>
                  <th>Código IBGE</th>
                  <th>Município</th>
                  <th>Subdivisão / UF</th>
                  <th>País</th>
                  <th>Coordenadas</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="mun in filteredMunicipios" :key="mun.id" class="table-row-hover">
                  <td><span class="setting-key-badge">{{ mun.codigoIbge }}</span></td>
                  <td class="font-bold text-primary">{{ mun.nome }}</td>
                  <td>{{ mun.uf }}</td>
                  <td>{{ mun.paisNome }}</td>
                  <td>{{ (mun.latitude && mun.longitude) ? `${mun.latitude}, ${mun.longitude}` : 'N/A' }}</td>
                  <td>
                    <span class="status-badge active">Ativo</span>
                  </td>
                </tr>
                <tr v-if="filteredMunicipios.length === 0">
                  <td colspan="6" class="text-center text-muted">Nenhum município encontrado. Clique em "Sincronizar Base BR" para carregar dados de teste.</td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>
      </div>

      <!-- Seção: Cache de CEP & Falhas -->
      <div v-if="currentTab === 'cep'" class="tab-content-wrapper animate-fade-in">
        <div class="admin-grid-layout">
          <!-- Simulador de Consulta -->
          <section class="admin-section glass-panel">
            <header class="section-header">
              <h3>Testar Consulta de CEP</h3>
            </header>
            <div class="vertical-form">
              <div class="form-group">
                <label>Digitar CEP</label>
                <div class="search-row">
                  <input type="text" v-model="searchCep" placeholder="Ex: 01001-000" class="flex-grow" />
                  <button @click="triggerCepQuery" class="btn btn-primary">Buscar</button>
                </div>
              </div>
              
              <!-- Resultado da busca -->
              <div v-if="cepSearchResult" class="cep-result-card glass-panel animate-scale-in">
                <header class="card-header flex-row-between">
                  <span class="cep-badge">{{ cepSearchResult.cep }}</span>
                  <span class="status-badge active">Sucesso</span>
                </header>
                <div class="cep-details">
                  <p><strong>Logradouro:</strong> {{ cepSearchResult.logradouro || 'N/A' }}</p>
                  <p><strong>Bairro:</strong> {{ cepSearchResult.bairro || 'N/A' }}</p>
                  <p><strong>Localidade/UF:</strong> {{ cepSearchResult.localidade }} / {{ cepSearchResult.uf }}</p>
                  <p><strong>IBGE:</strong> {{ cepSearchResult.ibge }}</p>
                </div>
              </div>
              <div v-else-if="cepSearchError" class="cep-result-card glass-panel error animate-scale-in">
                <span class="status-badge inactive">Falha</span>
                <p class="error-msg">Não foi possível resolver o CEP. Entrada manual permitida no cadastro.</p>
                <button @click="openManualRegister" class="btn btn-secondary btn-small btn-block" style="margin-top: 10px;">
                  📝 Registrar CEP Manualmente
                </button>
              </div>

              <!-- Formulário de Registro Manual de CEP -->
              <div v-if="showManualForm" class="cep-result-card glass-panel animate-scale-in" style="border-left: 4px solid var(--primary-color); margin-top: 14px;">
                <header class="card-header flex-row-between" style="margin-bottom: 10px;">
                  <span class="font-bold">📝 Registrar CEP Manualmente</span>
                  <button @click="showManualForm = false" class="btn btn-secondary btn-small">Cancelar</button>
                </header>
                <form @submit.prevent="handleCreateManualCep" class="vertical-form">
                  <div class="form-row-2">
                    <div class="form-group">
                      <label>CEP</label>
                      <input type="text" :value="formatCep(manualCepData.Cep)" disabled />
                    </div>
                    <div class="form-group">
                      <label>UF</label>
                      <input type="text" v-model="manualCepData.Uf" disabled placeholder="Selecione o Município" />
                    </div>
                  </div>
                  <div class="form-group">
                    <label>Município</label>
                    <select v-model="manualCepData.MunicipioId" @change="onMunicipioChange" class="select-input btn-block" required>
                      <option value="">Selecione o Município...</option>
                      <option v-for="m in municipios" :key="m.id" :value="m.id">{{ m.nome }} ({{ m.uf }})</option>
                    </select>
                  </div>
                  <div class="form-group">
                    <label>Logradouro</label>
                    <input type="text" v-model="manualCepData.Logradouro" placeholder="Ex: Rua das Laranjeiras" required />
                  </div>
                  <div class="form-group">
                    <label>Bairro</label>
                    <input type="text" v-model="manualCepData.Bairro" placeholder="Ex: Centro" required />
                  </div>
                  <div class="form-group">
                    <label>Justificativa do Registro</label>
                    <textarea v-model="manualCepData.Justificativa" rows="2" placeholder="Descreva o motivo de registrar este CEP de forma manual..." required></textarea>
                  </div>
                  <button type="submit" class="btn btn-primary btn-block">Salvar CEP Manual</button>
                </form>
              </div>
            </div>
          </section>

          <!-- Histórico de Cache / Falhas -->
          <section class="admin-section glass-panel flex-grow-2">
            <header class="section-header">
              <h3>Fila de CEPs em Cache & Falhas de Integração</h3>
            </header>
            <div class="table-container">
              <table class="admin-table">
                <thead>
                  <tr>
                    <th>CEP</th>
                    <th>Localidade / UF</th>
                    <th>Logradouro / Bairro</th>
                    <th>Integração (Provedor)</th>
                    <th>Resultado</th>
                    <th>Consultado Em</th>
                    <th>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="cache in postCodesCache" :key="cache.id" class="table-row-hover">
                    <td><span class="setting-key-badge">{{ formatCep(cache.codigoPostal) }}</span></td>
                    <td>{{ cache.falhou ? 'N/A' : `${cache.uf}` }}</td>
                    <td>
                      <div v-if="!cache.falhou">
                        <span class="text-primary">{{ cache.logradouro }}</span>
                        <span class="text-muted block text-xs">{{ cache.bairro }}</span>
                      </div>
                      <div v-else class="text-danger text-xs">
                        ⚠ {{ cache.motivoFalha }}
                      </div>
                    </td>
                    <td>{{ cache.provedor }}</td>
                    <td>
                      <span class="status-badge" :class="cache.falhou ? 'inactive' : 'active'">
                        {{ cache.falhou ? 'Falha' : 'Sucesso' }}
                      </span>
                    </td>
                    <td>{{ formatDateTime(cache.consultadoEm) }}</td>
                    <td>
                      <button 
                        v-if="cache.falhou" 
                        @click="reprocessarCep(cache.codigoPostal)" 
                        class="btn btn-secondary btn-small"
                        style="padding: 2px 6px; font-size: 11px;"
                        title="Tentar buscar novamente do provedor oficial"
                      >
                        🔄 Reprocessar
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>
        </div>
      </div>

      <!-- Seção: Zonas de Entrega -->
      <div v-if="currentTab === 'zonas'" class="tab-content-wrapper animate-fade-in">
        <div class="admin-grid-layout">
          <!-- Nova Zona de Entrega -->
          <section class="admin-section glass-panel">
            <header class="section-header">
              <h3>Criar Zona de Entrega</h3>
            </header>
            <form @submit.prevent="handleCreateZona" class="vertical-form">
              <div class="form-group">
                <label>Nome da Zona</label>
                <input type="text" v-model="newZona.Nome" placeholder="Ex: São Paulo Metropolitana" required />
              </div>
              <div class="form-row-2">
                <div class="form-group">
                  <label>CEP Início</label>
                  <input type="text" v-model="newZona.CepInicio" placeholder="Ex: 01000000" maxlength="8" required />
                </div>
                <div class="form-group">
                  <label>CEP Fim</label>
                  <input type="text" v-model="newZona.CepFim" placeholder="Ex: 09999999" maxlength="8" required />
                </div>
              </div>
              <button type="submit" class="btn btn-primary btn-block">Salvar Zona</button>
            </form>
          </section>

          <!-- Listagem Zonas de Entrega -->
          <section class="admin-section glass-panel flex-grow-2">
            <header class="section-header">
              <h3>Zonas de Entrega Logísticas (Tenant Isolated)</h3>
            </header>
            <div class="table-container">
              <table class="admin-table">
                <thead>
                  <tr>
                    <th>Nome da Zona</th>
                    <th>Faixa de CEPs</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="zona in zonasEntrega" :key="zona.id" class="table-row-hover">
                    <td class="font-bold text-primary">{{ zona.nome }}</td>
                    <td>
                      <span class="setting-key-badge">{{ formatCep(zona.cepInicio) }}</span>
                      <span class="arrow-separator">➔</span>
                      <span class="setting-key-badge">{{ formatCep(zona.cepFim) }}</span>
                    </td>
                    <td>
                      <span class="status-badge" :class="zona.ativo ? 'active' : 'inactive'">
                        {{ zona.ativo ? 'Ativo' : 'Inativo' }}
                      </span>
                    </td>
                  </tr>
                  <tr v-if="zonasEntrega.length === 0">
                    <td colspan="3" class="text-center text-muted">Nenhuma zona de entrega logística cadastrada.</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>
        </div>
      </div>

      <!-- Console de Logs da Operação -->
      <section class="logs-console-section glass-panel animate-slide-up">
        <header class="logs-header">
          <div class="logs-header-title">
            <h3>Console de Logs e Integrações</h3>
            <p>Monitore requisições de consulta de CEP e sincronizações do sistema em tempo real.</p>
          </div>
          <div class="simulator-actions">
            <button @click="clearLogs" class="btn btn-log-clear">Limpar Console</button>
          </div>
        </header>
        <div class="terminal-box" ref="terminalBox">
          <div 
            v-for="(log, idx) in logs" 
            :key="idx" 
            :class="['log-line', log.type]"
          >
            <span class="log-time">[{{ log.time }}]</span>
            <span class="log-msg">{{ log.message }}</span>
          </div>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, nextTick, watch } from 'vue'

// Página autocontida (já renderiza seu próprio AppHeader) — sem layout do shell ERP.
definePageMeta({ layout: false })

const apiOnline = ref(false)
const currentTab = ref('paises')
const logs = ref([])
const terminalBox = ref(null)

// Abas
const tabItems = [
  { id: 'paises', name: 'Países & Estados', icon: '🌍' },
  { id: 'municipios', name: 'Municípios', icon: '🏙️' },
  { id: 'cep', name: 'CEP & Falhas', icon: '📬' },
  { id: 'zonas', name: 'Zonas de Entrega', icon: '🚚' }
]

// Data
const paises = ref([])
const municipios = ref([])
const postCodesCache = ref([])
const zonasEntrega = ref([])

// Filters
const filterMunName = ref('')
const selectedUf = ref('')
const ufsList = ['SP', 'RJ', 'MG', 'BA', 'DF', 'PR', 'RS', 'PE', 'CE', 'AM', 'PA', 'GO', 'SC']

// Forms
const newPais = reactive({
  Nome: '',
  CodigoIsoAlpha2: '',
  CodigoIsoAlpha3: '',
  CodigoNumerico: '',
  Capital: '',
  CodigoDiscagem: ''
})

const newZona = reactive({
  Nome: '',
  CepInicio: '',
  CepFim: ''
})

const searchCep = ref('')
const cepSearchResult = ref(null)
const cepSearchError = ref(false)

// Suporte a CEP Manual e Reprocessamento
const showManualForm = ref(false)
const manualCepData = reactive({
  Cep: '',
  Logradouro: '',
  Bairro: '',
  MunicipioId: '',
  Uf: '',
  Justificativa: ''
})

const openManualRegister = () => {
  showManualForm.value = true
  manualCepData.Cep = searchCep.value.replace(/\D/g, '')
  manualCepData.Logradouro = ''
  manualCepData.Bairro = ''
  manualCepData.MunicipioId = ''
  manualCepData.Uf = ''
  manualCepData.Justificativa = ''
}

const onMunicipioChange = () => {
  const mun = municipios.value.find(m => m.id === manualCepData.MunicipioId)
  if (mun) {
    manualCepData.Uf = mun.uf
  }
}

// Watch dinâmico para buscar municípios de acordo com o filtro de estado (online)
watch(selectedUf, async (newUf) => {
  if (apiOnline.value && newUf) {
    try {
      const data = await $fetch(`http://localhost:5000/api/v1/cadastros/geografia/municipios/obter-por-uf/${newUf}`)
      municipios.value = data
      addLog(`Carregou municípios do estado ${newUf} via API.`, 'success')
    } catch (e) {
      addLog(`Erro ao carregar municípios da UF: ${e.message}`, 'error')
    }
  }
})

// Lifecycle
onMounted(async () => {
  await checkApiOnline()
  await loadData()
  addLog('Módulo de Geografia e Localização inicializado.', 'info')
})

const checkApiOnline = async () => {
  try {
    await $fetch('http://localhost:5000/api/v1/cadastros/geografia/paises')
    apiOnline.value = true
  } catch (e) {
    if (e.status === 401 || e.status === 403) {
      apiOnline.value = true
    } else {
      apiOnline.value = false
      console.warn('API Gateway C# offline. Ativando modo simulação.', e)
    }
  }
}

const loadData = async () => {
  await loadPaises()
  await loadMunicipios()
  await loadCepCache()
  await loadZonasEntrega()
}

// Log utility
const addLog = (message, type = 'info') => {
  const time = new Date().toLocaleTimeString()
  logs.value.push({ time, message, type })
  nextTick(() => {
    if (terminalBox.value) {
      terminalBox.value.scrollTop = terminalBox.value.scrollHeight
    }
  })
}

const clearLogs = () => {
  logs.value = []
}

const changeTab = (tabId) => {
  currentTab.value = tabId
  addLog(`Navegou para aba: ${tabItems.find(t => t.id === tabId).name}`, 'info')
}

// FORMAT UTIL
const formatDateTime = (val) => {
  if (!val) return ''
  return new Date(val).toLocaleString()
}
const formatCep = (val) => {
  if (!val) return ''
  const clean = val.replace(/\D/g, '')
  if (clean.length === 8) {
    return `${clean.substring(0, 5)}-${clean.substring(5)}`
  }
  return val
}

// ------------------------------------------------------------
// API / SIMULATION HANDLERS
// ------------------------------------------------------------

const loadPaises = async () => {
  if (apiOnline.value) {
    try {
      const data = await $fetch('http://localhost:5000/api/v1/cadastros/geografia/paises')
      paises.value = data
    } catch (e) {
      addLog(`Erro ao carregar países da API: ${e.message}`, 'error')
      loadPaisesSimulado()
    }
  } else {
    loadPaisesSimulado()
  }
}

const loadPaisesSimulado = () => {
  const stored = localStorage.getItem('epros_paises')
  if (stored) {
    paises.value = JSON.parse(stored)
  } else {
    paises.value = [
      { id: '1', nome: 'Brasil', codigoIsoAlpha2: 'BR', codigoIsoAlpha3: 'BRA', codigoNumerico: '076', capital: 'Brasília', codigoDiscagem: '+55', ativo: true }
    ]
    localStorage.setItem('epros_paises', JSON.stringify(paises.value))
  }
}

const handleCreatePais = async () => {
  if (apiOnline.value) {
    try {
      const res = await $fetch('http://localhost:5000/api/v1/cadastros/geografia/paises', {
        method: 'POST', // Espera command record
        body: newPais
      })
      if (res.sucesso) {
        addLog(`País '${newPais.Nome}' cadastrado com sucesso!`, 'success')
        resetPaisForm()
        await loadPaises()
      } else {
        addLog(`Falha ao cadastrar país: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Erro na requisição: ${e.message}`, 'error')
    }
  } else {
    const isDuplicate = paises.value.some(p => p.codigoIsoAlpha2.toUpperCase() === newPais.CodigoIsoAlpha2.toUpperCase())
    if (isDuplicate) {
      addLog(`Erro: Código ISO Alpha-2 '${newPais.CodigoIsoAlpha2}' já cadastrado.`, 'error')
      return
    }

    paises.value.push({
      id: Math.random().toString(36).substring(7),
      nome: newPais.Nome,
      codigoIsoAlpha2: newPais.CodigoIsoAlpha2.toUpperCase(),
      codigoIsoAlpha3: newPais.CodigoIsoAlpha3.toUpperCase(),
      codigoNumerico: newPais.CodigoNumerico,
      capital: newPais.Capital,
      codigoDiscagem: newPais.CodigoDiscagem,
      ativo: true
    })
    localStorage.setItem('epros_paises', JSON.stringify(paises.value))
    addLog(`[Simulado] País '${newPais.Nome}' criado com sucesso localmente.`, 'success')
    resetPaisForm()
  }
}

const resetPaisForm = () => {
  newPais.Nome = ''
  newPais.CodigoIsoAlpha2 = ''
  newPais.CodigoIsoAlpha3 = ''
  newPais.CodigoNumerico = ''
  newPais.Capital = ''
  newPais.CodigoDiscagem = ''
}

const sincronizarGeografiaBase = async () => {
  addLog('Iniciando sincronização de geografia base (IBGE BR)...', 'info')
  if (apiOnline.value) {
    try {
      const res = await $fetch('http://localhost:5000/api/v1/cadastros/geografia/municipios/sincronizar', { method: 'POST' })
      if (res.sucesso) {
        addLog('Sincronização com a base do IBGE concluída no servidor.', 'success')
        await loadData()
      } else {
        addLog(`Erro na sincronização: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Erro na requisição: ${e.message}`, 'error')
    }
  } else {
    // Simulado
    addLog('[Simulação] Criando subdivisões de estados e principais capitais brasileiras...', 'info')
    loadMunicipiosSimulado(true)
    addLog('[Simulação] Sincronização concluída com sucesso.', 'success')
  }
}

const loadMunicipios = async () => {
  if (apiOnline.value) {
    try {
      // Buscar municípios de São Paulo por padrão para demonstração rápida
      const data = await $fetch('http://localhost:5000/api/v1/cadastros/geografia/municipios/obter-por-uf/SP')
      municipios.value = data
    } catch (e) {
      addLog(`Erro ao buscar municípios da API: ${e.message}`, 'error')
      loadMunicipiosSimulado()
    }
  } else {
    loadMunicipiosSimulado()
  }
}

const loadMunicipiosSimulado = (forceReset = false) => {
  const stored = localStorage.getItem('epros_municipios')
  if (stored && !forceReset) {
    municipios.value = JSON.parse(stored)
  } else {
    municipios.value = [
      { id: 'm1', paisNome: 'Brasil', uf: 'SP', nome: 'São Paulo', codigoIbge: 3550308, latitude: -23.5505, longitude: -46.6333 },
      { id: 'm2', paisNome: 'Brasil', uf: 'SP', nome: 'Campinas', codigoIbge: 3509502, latitude: -22.9099, longitude: -47.0626 },
      { id: 'm3', paisNome: 'Brasil', uf: 'RJ', nome: 'Rio de Janeiro', codigoIbge: 3304557, latitude: -22.9068, longitude: -43.1729 },
      { id: 'm4', paisNome: 'Brasil', uf: 'MG', nome: 'Belo Horizonte', codigoIbge: 3106200, latitude: -19.9191, longitude: -43.9378 }
    ]
    localStorage.setItem('epros_municipios', JSON.stringify(municipios.value))
  }
}

const filteredMunicipios = computed(() => {
  return municipios.value.filter(m => {
    const matchesName = m.nome.toLowerCase().includes(filterMunName.value.toLowerCase())
    const matchesUf = selectedUf.value === '' || m.uf.toUpperCase() === selectedUf.value.toUpperCase()
    return matchesName && matchesUf
  })
})

const loadCepCache = async () => {
  if (apiOnline.value) {
    try {
      const data = await $fetch('http://localhost:5000/api/v1/cadastros/geografia/cep/cache')
      postCodesCache.value = data
    } catch (e) {
      addLog(`Erro ao carregar cache de CEPs da API: ${e.message}`, 'error')
      loadCepCacheSimulado()
    }
  } else {
    loadCepCacheSimulado()
  }
}

const loadCepCacheSimulado = () => {
  const stored = localStorage.getItem('epros_cep_cache')
  if (stored) {
    postCodesCache.value = JSON.parse(stored)
  } else {
    postCodesCache.value = [
      { id: 'c1', codigoPostal: '01310100', uf: 'SP', logradouro: 'Avenida Paulista', bairro: 'Bela Vista', provedor: 'ViaCEP', falhou: false, consultadoEm: new Date().toISOString() },
      { id: 'c2', codigoPostal: '99999999', uf: '', logradouro: '', bairro: '', provedor: 'ViaCEP', falhou: true, motivoFalha: 'CEP não existente na base nacional', consultadoEm: new Date().toISOString() }
    ]
    localStorage.setItem('epros_cep_cache', JSON.stringify(postCodesCache.value))
  }
}

const triggerCepQuery = async () => {
  cepSearchResult.value = null
  cepSearchError.value = false
  const cleanCep = searchCep.value.replace(/\D/g, '')
  if (cleanCep.length !== 8) {
    addLog('CEP inválido! O CEP deve conter 8 dígitos.', 'warn')
    return
  }

  addLog(`Iniciando busca de CEP: ${searchCep.value}...`, 'info')

  if (apiOnline.value) {
    try {
      const data = await $fetch(`http://localhost:5000/api/v1/cadastros/geografia/cep/${cleanCep}`)
      if (data) {
        cepSearchResult.value = data
        addLog(`CEP ${searchCep.value} resolvido com sucesso via API.`, 'success')
        await loadCepCache()
      } else {
        cepSearchError.value = true
        addLog(`Falha ao resolver CEP ${searchCep.value}. Modo manual habilitado.`, 'warn')
        await loadCepCache()
      }
    } catch (e) {
      cepSearchError.value = true
      addLog(`Erro na requisição de CEP: ${e.message}`, 'error')
      await loadCepCache()
    }
  } else {
    // Simulado
    if (cleanCep === '99999999') {
      cepSearchError.value = true
      postCodesCache.value.unshift({
        id: Math.random().toString(36).substring(7),
        codigoPostal: cleanCep,
        uf: '',
        logradouro: '',
        bairro: '',
        provedor: 'ViaCEP (Mock)',
        falhou: true,
        motivoFalha: 'CEP não encontrado na simulação',
        consultadoEm: new Date().toISOString()
      })
      localStorage.setItem('epros_cep_cache', JSON.stringify(postCodesCache.value))
      addLog(`[Simulação] CEP ${searchCep.value} falhou. Permissão para preenchimento manual concedida.`, 'warn')
    } else {
      const mockResult = {
        cep: formatCep(cleanCep),
        logradouro: 'Rua Simulação das Laranjeiras',
        bairro: 'Jardim das Flores',
        localidade: 'Campinas',
        uf: 'SP',
        ibge: '3509502',
        municipioId: 'm2',
        paisId: '1'
      }
      cepSearchResult.value = mockResult
      postCodesCache.value.unshift({
        id: Math.random().toString(36).substring(7),
        codigoPostal: cleanCep,
        uf: mockResult.uf,
        logradouro: mockResult.logradouro,
        bairro: mockResult.bairro,
        provedor: 'ViaCEP (Mock)',
        falhou: false,
        consultadoEm: new Date().toISOString()
      })
      localStorage.setItem('epros_cep_cache', JSON.stringify(postCodesCache.value))
      addLog(`[Simulação] CEP ${searchCep.value} resolvido e gravado no cache com sucesso.`, 'success')
    }
  }
}

const loadZonasEntrega = async () => {
  if (apiOnline.value) {
    try {
      const data = await $fetch('http://localhost:5000/api/v1/cadastros/geografia/zonas-entrega')
      zonasEntrega.value = data
    } catch (e) {
      addLog(`Erro ao carregar zonas de entrega da API: ${e.message}`, 'error')
      loadZonasSimulado()
    }
  } else {
    loadZonasSimulado()
  }
}

const loadZonasSimulado = () => {
  const stored = localStorage.getItem('epros_zonas_entrega')
  if (stored) {
    zonasEntrega.value = JSON.parse(stored)
  } else {
    zonasEntrega.value = [
      { id: 'z1', nome: 'Zona Metropolitana de São Paulo', cepInicio: '01000000', cepFim: '09999999', ativo: true }
    ]
    localStorage.setItem('epros_zonas_entrega', JSON.stringify(zonasEntrega.value))
  }
}

const handleCreateZona = async () => {
  if (apiOnline.value) {
    try {
      const res = await $fetch('http://localhost:5000/api/v1/cadastros/geografia/zonas-entrega', {
        method: 'POST',
        body: newZona
      })
      if (res.sucesso) {
        addLog(`Zona de entrega '${newZona.Nome}' salva com sucesso!`, 'success')
        resetZonaForm()
        await loadZonasEntrega()
      } else {
        addLog(`Falha ao cadastrar zona: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Erro na requisição: ${e.message}`, 'error')
    }
  } else {
    zonasEntrega.value.push({
      id: Math.random().toString(36).substring(7),
      nome: newZona.Nome,
      cepInicio: newZona.CepInicio,
      cepFim: newZona.CepFim,
      ativo: true
    })
    localStorage.setItem('epros_zonas_entrega', JSON.stringify(zonasEntrega.value))
    addLog(`[Simulado] Zona de entrega '${newZona.Nome}' salva com sucesso.`, 'success')
    resetZonaForm()
  }
}

const resetZonaForm = () => {
  newZona.Nome = ''
  newZona.CepInicio = ''
  newZona.CepFim = ''
}

const handleCreateManualCep = async () => {
  addLog(`Registrando CEP ${manualCepData.Cep} manualmente...`, 'info')
  if (apiOnline.value) {
    try {
      const res = await $fetch('http://localhost:5000/api/v1/cadastros/geografia/cep/manual', {
        method: 'PUT',
        body: manualCepData
      })
      if (res.sucesso) {
        addLog(`CEP ${manualCepData.Cep} registrado manualmente com sucesso via API!`, 'success')
        showManualForm.value = false
        cepSearchResult.value = res.dados
        cepSearchError.value = false
        await loadCepCache()
      } else {
        addLog(`Falha ao registrar CEP manual: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Erro ao registrar CEP manual: ${e.message}`, 'error')
    }
  } else {
    // Modo simulação
    const mun = municipios.value.find(m => m.id === manualCepData.MunicipioId)
    const newEntry = {
      id: Math.random().toString(36).substring(7),
      codigoPostal: manualCepData.Cep,
      logradouro: manualCepData.Logradouro,
      bairro: manualCepData.Bairro,
      uf: manualCepData.Uf,
      provedor: 'Manual',
      falhou: false,
      motivoFalha: manualCepData.Justificativa,
      consultadoEm: new Date().toISOString()
    }
    postCodesCache.value.unshift(newEntry)
    localStorage.setItem('epros_cep_cache', JSON.stringify(postCodesCache.value))
    addLog(`[Simulação] CEP ${manualCepData.Cep} registrado manualmente localmente.`, 'success')
    
    cepSearchResult.value = {
      cep: formatCep(manualCepData.Cep),
      logradouro: manualCepData.Logradouro,
      bairro: manualCepData.Bairro,
      localidade: mun ? mun.nome : '',
      uf: manualCepData.Uf,
      ibge: mun ? mun.codigoIbge.toString() : '',
      municipioId: manualCepData.MunicipioId,
      paisId: '1'
    }
    cepSearchError.value = false
    showManualForm.value = false
  }
}

const reprocessarCep = async (cep) => {
  addLog(`Solicitando reprocessamento para o CEP ${cep}...`, 'info')
  if (apiOnline.value) {
    try {
      const res = await $fetch(`http://localhost:5000/api/v1/cadastros/geografia/cep/${cep}/reprocessar`, {
        method: 'POST'
      })
      if (res.sucesso) {
        addLog(`CEP ${cep} reprocessado com sucesso!`, 'success')
        await loadCepCache()
      } else {
        addLog(`Falha ao reprocessar CEP: ${res.mensagem}`, 'error')
      }
    } catch (e) {
      addLog(`Erro ao reprocessar CEP: ${e.message}`, 'error')
    }
  } else {
    // Modo simulação
    const entry = postCodesCache.value.find(c => c.codigoPostal === cep)
    if (entry) {
      entry.falhou = false
      entry.logradouro = 'Rua Reprocessada com Sucesso (Simulado)'
      entry.bairro = 'Bairro Simulado'
      entry.uf = 'SP'
      localStorage.setItem('epros_cep_cache', JSON.stringify(postCodesCache.value))
      addLog(`[Simulação] CEP ${cep} reprocessado localmente.`, 'success')
    }
  }
}
</script>

<style scoped>
/* Adota a mesma identidade do painel admin.vue */
.form-row-2 {
  display: flex;
  gap: 12px;
}
.form-row-2 > .form-group {
  flex: 1;
}

.search-row {
  display: flex;
  gap: 10px;
}

.flex-grow {
  flex-grow: 1;
}
.flex-grow-2 {
  flex: 2;
}

.filter-controls {
  display: flex;
  gap: 10px;
  align-items: center;
}

.search-input {
  padding: 6px 12px;
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid var(--border-color);
  border-radius: 6px;
  color: var(--text-primary);
  font-size: 12.5px;
}

.select-input {
  padding: 6px 12px;
  background: rgba(0, 0, 0, 0.3);
  border: 1px solid var(--border-color);
  border-radius: 6px;
  color: var(--text-primary);
  font-size: 12.5px;
}

.cep-result-card {
  margin-top: 14px;
  padding: 16px;
  border-left: 4px solid var(--success);
}

.cep-result-card.error {
  border-left: 4px solid var(--danger);
}

.cep-badge {
  font-size: 15px;
  font-weight: 700;
  font-family: monospace;
  background: rgba(255, 255, 255, 0.05);
  padding: 4px 8px;
  border-radius: 6px;
  border: 1px solid var(--border-color);
  color: var(--primary-light);
}

.cep-details p {
  margin: 6px 0;
  font-size: 13px;
  color: var(--text-secondary);
}

.arrow-separator {
  color: var(--text-muted);
  margin: 0 8px;
  font-size: 12px;
}

.text-danger {
  color: var(--danger);
  font-size: 12.5px;
}

.block {
  display: block;
}
.text-xs {
  font-size: 11px;
}
</style>
