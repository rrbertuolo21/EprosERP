<template>
  <div class="dashboard-layout">
    <!-- Conteúdo Principal (o shell/header vem do layout `admin`) -->
    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">{{ isNew ? 'Novo Inquilino' : 'Editar Inquilino' }}</h1>
        <p class="tagline">{{ isNew ? 'Cadastre um novo cliente SaaS na plataforma e defina o plano inicial.' : 'Gerencie as informações detalhadas, endereços e composições de faturamento do inquilino.' }}</p>
        <div class="header-actions">
          <NuxtLink to="/plataforma/admin/clientes" class="btn btn-secondary btn-back">
            ← Voltar para Lista
          </NuxtLink>
          <button
            v-if="!isNew"
            type="button"
            class="btn btn-primary"
            @click="openAprovarModal"
          >
            Aprovar assinatura manual
          </button>
          <span class="status-pill" :class="{ 'offline': !apiOnline }">
            <span class="status-dot"></span>
            {{ apiOnline ? 'Conectado à API Gateway' : 'Modo Simulação Offline' }}
          </span>
        </div>
      </header>

      <div class="admin-grid-layout form-focused-layout">
        <!-- Card Principal -->
        <section class="admin-section form-card glass-panel col-span-2">
          <!-- Abas para o Modo Edição -->
          <nav class="form-tabs" v-if="!isNew">
            <button 
              type="button" 
              v-for="tab in tabs" 
              :key="tab.id" 
              :class="['tab-link', { 'active': currentTab === tab.id }]"
              @click="currentTab = tab.id"
            >
              {{ tab.name }}
            </button>
          </nav>

          <form @submit.prevent="saveClient" class="vertical-form mt-4">
            <!-- ABA 1: DADOS GERAIS -->
            <div v-show="currentTab === 'dados'" class="form-tab-content">
              <div class="form-row">
                <div class="form-group col-6">
                  <label for="c-rs">Razão Social *</label>
                  <input type="text" id="c-rs" v-model="cliente.razaoSocial" placeholder="Empresa Ltda" required />
                </div>
                <div class="form-group col-6">
                  <label for="c-cnpj">CNPJ *</label>
                  <input type="text" id="c-cnpj" v-model="cliente.cnpj" placeholder="00.000.000/0000-00" required />
                </div>
              </div>

              <div class="form-row">
                <div class="form-group col-6">
                  <label for="c-email">E-mail Comercial *</label>
                  <input type="email" id="c-email" v-model="cliente.email" placeholder="financeiro@empresa.com" required />
                </div>
                <div class="form-group col-6">
                  <label for="c-tel">Telefone</label>
                  <input type="text" id="c-tel" v-model="cliente.telefone" placeholder="(11) 99999-8888" />
                </div>
              </div>

              <div class="form-row">
                <div class="form-group col-6">
                  <label for="c-contato">Nome do Contato Principal</label>
                  <input type="text" id="c-contato" v-model="cliente.nomeContato" placeholder="Ex: Roberto Ramos" />
                </div>
                <div class="form-group col-6">
                  <label for="c-plano">Plano de Assinatura *</label>
                  <select id="c-plano" v-model="cliente.planoId" required>
                    <option value="" disabled>Selecione um plano...</option>
                    <option v-for="plan in plans" :key="plan.id" :value="plan.id">
                      {{ plan.name }} - R$ {{ plan.price.toFixed(2) }}
                    </option>
                  </select>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group col-4">
                  <label for="c-revenda">Revenda Canal</label>
                  <select id="c-revenda" v-model="cliente.revendaId">
                    <option :value="null">Nenhuma (Direto)</option>
                    <option v-for="rev in revendas" :key="rev.id" :value="rev.id">
                      {{ rev.nome }}
                    </option>
                  </select>
                </div>
                <div class="form-group col-4">
                  <label for="c-vendedor">Vendedor Responsável</label>
                  <select id="c-vendedor" v-model="cliente.vendedorId">
                    <option :value="null">Nenhum</option>
                    <option v-for="vend in vendedores" :key="vend.id" :value="vend.id">
                      {{ vend.nome }}
                    </option>
                  </select>
                </div>
                <div class="form-group col-4">
                  <label for="c-vcto">Dia de Vencimento (1 a 28) *</label>
                  <input type="number" id="c-vcto" v-model.number="cliente.diaVencimento" min="1" max="28" required />
                </div>
              </div>

              <div class="form-row">
                <div class="form-group col-12">
                  <label>Cota contratada (override do plano) <small>— deixe vazio para usar o limite do plano</small></label>
                  <div class="cota-grid">
                    <div>
                      <label for="c-cota-usu" class="cota-lbl">Usuários</label>
                      <input type="number" id="c-cota-usu" min="0" placeholder="(plano)" v-model.number="cliente.cotaUsuarios" />
                    </div>
                    <div>
                      <label for="c-cota-emp" class="cota-lbl">Empresas</label>
                      <input type="number" id="c-cota-emp" min="0" placeholder="(plano)" v-model.number="cliente.cotaEmpresas" />
                    </div>
                    <div>
                      <label for="c-cota-perm" class="cota-lbl">Permissões</label>
                      <input type="number" id="c-cota-perm" min="0" placeholder="(plano)" v-model.number="cliente.cotaPermissoes" />
                    </div>
                  </div>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group toggle-row col-6">
                  <label for="c-demo">Modo Demonstração (Demo)</label>
                  <input type="checkbox" id="c-demo" v-model="cliente.isDemo" />
                </div>
                <div class="form-group toggle-row col-6" v-if="!isNew">
                  <label for="c-ativo">Cadastro Ativo</label>
                  <input type="checkbox" id="c-ativo" v-model="cliente.ativo" />
                </div>
              </div>
            </div>

            <!-- ABA 2: ENDEREÇOS (APENAS EM EDIÇÃO) -->
            <div v-show="currentTab === 'enderecos'" class="form-tab-content">
              <header class="tab-section-header">
                <h4>Endereços de Faturamento / Operação</h4>
                <button type="button" @click="openEnderecoModal()" class="btn btn-secondary btn-sm">
                  + Adicionar Endereço
                </button>
              </header>

              <div class="table-container mt-2">
                <table class="admin-table">
                  <thead>
                    <tr>
                      <th>Tipo</th>
                      <th>Logradouro</th>
                      <th>Bairro</th>
                      <th>CEP</th>
                      <th>Cidade/UF</th>
                      <th class="align-right">Ações</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-if="cliente.enderecos.length === 0">
                      <td colspan="6" class="empty-cell">Nenhum endereço cadastrado.</td>
                    </tr>
                    <tr v-else v-for="(end, idx) in cliente.enderecos" :key="end.id || idx">
                      <td>
                        <span class="badge">{{ getTipoEnderecoLabel(end.tipoEndereco) }}</span>
                        <span v-if="end.principal" class="badge badge-principal" title="Endereço principal">★ Principal</span>
                      </td>
                      <td>{{ end.logradouro }}, {{ end.numero }} {{ end.complemento ? '· ' + end.complemento : '' }}</td>
                      <td>{{ end.bairro }}</td>
                      <td>{{ end.cep }}</td>
                      <td>{{ end.uf }}</td>
                      <td class="align-right">
                        <button type="button" @click="openEnderecoModal(end, idx)" class="btn btn-secondary btn-table-action">Editar</button>
                        <button type="button" @click="removeEndereco(idx)" class="btn btn-secondary btn-table-action btn-danger-action">Remover</button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <!-- ABA 3: COMPOSIÇÕES DE FATURAMENTO (APENAS EM EDIÇÃO) -->
            <div v-show="currentTab === 'composicoes'" class="form-tab-content">
              <header class="tab-section-header">
                <h4>Composições Mensais de Faturamento</h4>
                <button type="button" @click="openComposicaoModal()" class="btn btn-secondary btn-sm">
                  + Adicionar Item
                </button>
              </header>

              <div class="table-container mt-2">
                <table class="admin-table">
                  <thead>
                    <tr>
                      <th>Descrição</th>
                      <th>Data Inicial</th>
                      <th>Data Final</th>
                      <th>Valor Mensal</th>
                      <th>Reajustável?</th>
                      <th class="align-right">Ações</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-if="cliente.composicoes.length === 0">
                      <td colspan="6" class="empty-cell">Nenhuma composição financeira cadastrada.</td>
                    </tr>
                    <tr v-else v-for="(comp, idx) in cliente.composicoes" :key="comp.id || idx">
                      <td><span class="tenant-name-txt">{{ comp.descricao }}</span></td>
                      <td>{{ formatDate(comp.dataInicial) }}</td>
                      <td>{{ comp.dataFinal ? formatDate(comp.dataFinal) : 'Vigência Indeterminada' }}</td>
                      <td>R$ {{ comp.valor.toFixed(2) }}</td>
                      <td>
                        <span :class="['badge', comp.podeReajustar ? 'badge-success' : 'badge-danger']">
                          {{ comp.podeReajustar ? 'Sim' : 'Não' }}
                        </span>
                      </td>
                      <td class="align-right">
                        <button type="button" @click="openComposicaoModal(comp, idx)" class="btn btn-secondary btn-table-action">Editar</button>
                        <button type="button" @click="removeComposicao(idx)" class="btn btn-secondary btn-table-action btn-danger-action">Remover</button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <!-- Botões de Ação do Formulário -->
            <footer class="form-footer mt-4">
              <button type="submit" class="btn btn-primary" :disabled="saving">
                {{ saving ? 'Gravando...' : (isNew ? 'Criar Inquilino' : 'Salvar Informações') }}
              </button>
              <NuxtLink to="/plataforma/admin/clientes" class="btn btn-secondary">
                Cancelar
              </NuxtLink>
            </footer>
          </form>
        </section>
      </div>
    </main>

    <!-- MODAL: ENDEREÇO -->
    <div class="modal-backdrop" v-if="enderecoModal.open">
      <div class="modal-card glass-panel">
        <header class="modal-header">
          <h3>{{ enderecoModal.index === -1 ? 'Novo Endereço' : 'Editar Endereço' }}</h3>
          <button type="button" @click="enderecoModal.open = false" class="btn-close">×</button>
        </header>
        <form @submit.prevent="saveEnderecoModal" class="vertical-form">
          <div class="form-row">
            <div class="form-group col-6">
              <label>Tipo de Endereço *</label>
              <select v-model="enderecoModal.form.tipoEndereco" required>
                <option :value="1">Principal</option>
                <option :value="2">Cobrança</option>
                <option :value="3">Entrega</option>
                <option :value="4">Outros</option>
              </select>
            </div>
            <div class="form-group col-6">
              <label>CEP *</label>
              <input type="text" v-model="enderecoModal.form.cep" placeholder="00000-000" @blur="fetchCep" required />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group col-9">
              <label>Logradouro *</label>
              <input type="text" v-model="enderecoModal.form.logradouro" placeholder="Rua / Av..." required />
            </div>
            <div class="form-group col-3">
              <label>Número *</label>
              <input type="text" v-model="enderecoModal.form.numero" placeholder="123" required />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group col-6">
              <label>Complemento</label>
              <input type="text" v-model="enderecoModal.form.complemento" placeholder="Apto, Sala..." />
            </div>
            <div class="form-group col-6">
              <label>Bairro *</label>
              <input type="text" v-model="enderecoModal.form.bairro" placeholder="Bairro" required />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group col-6">
              <label>Estado (UF) *</label>
              <input type="text" v-model="enderecoModal.form.uf" placeholder="SP" maxlength="2" required />
            </div>
            <div class="form-group col-6">
              <label>Referência</label>
              <input type="text" v-model="enderecoModal.form.referencia" placeholder="Ponto de referência" />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group toggle-row col-12">
              <label>Endereço principal (deste tipo)</label>
              <input type="checkbox" v-model="enderecoModal.form.principal" />
            </div>
          </div>
          <button type="submit" class="btn btn-primary btn-block mt-4">Confirmar Endereço</button>
        </form>
      </div>
    </div>

    <!-- MODAL: COMPOSIÇÃO -->
    <div class="modal-backdrop" v-if="composicaoModal.open">
      <div class="modal-card glass-panel">
        <header class="modal-header">
          <h3>{{ composicaoModal.index === -1 ? 'Novo Item Faturamento' : 'Editar Item' }}</h3>
          <button type="button" @click="composicaoModal.open = false" class="btn-close">×</button>
        </header>
        <form @submit.prevent="saveComposicaoModal" class="vertical-form">
          <div class="form-group">
            <label>Descrição do Item *</label>
            <input type="text" v-model="composicaoModal.form.descricao" placeholder="Mensalidade ERP, Adicional Licença..." required />
          </div>
          <div class="form-row">
            <div class="form-group col-6">
              <label>Valor Mensal (R$) *</label>
              <input type="number" v-model.number="composicaoModal.form.valor" step="0.01" min="1" required />
            </div>
            <div class="form-group toggle-row col-6">
              <label>Permitir Reajuste Automático</label>
              <input type="checkbox" v-model="composicaoModal.form.podeReajustar" />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group col-6">
              <label>Data de Início *</label>
              <input type="date" v-model="composicaoModal.form.dataInicial" required />
            </div>
            <div class="form-group col-6">
              <label>Data Final (Opcional)</label>
              <input type="date" v-model="composicaoModal.form.dataFinal" />
            </div>
          </div>
          <button type="submit" class="btn btn-primary btn-block mt-4">Confirmar Item</button>
        </form>
      </div>
    </div>

    <!-- MODAL: APROVAR ASSINATURA MANUAL -->
    <div class="modal-backdrop" v-if="aprovarModal.open">
      <div class="modal-card glass-panel">
        <header class="modal-header">
          <h3>Aprovar Assinatura Manual</h3>
          <button type="button" @click="aprovarModal.open = false" class="btn-close">×</button>
        </header>
        <form @submit.prevent="aprovarAssinaturaManual" class="vertical-form">
          <div class="form-row">
            <div class="form-group col-6">
              <label>Data de Início *</label>
              <input type="date" v-model="aprovarModal.form.dataInicio" required />
            </div>
            <div class="form-group col-6">
              <label>Data de Fim (Opcional)</label>
              <input type="date" v-model="aprovarModal.form.dataFim" />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group col-6">
              <label>Dia de Vencimento (1 a 28) *</label>
              <input type="number" v-model.number="aprovarModal.form.diaVencimento" min="1" max="28" required />
            </div>
            <div class="form-group col-6">
              <label>Valor Recorrente (R$) *</label>
              <input type="number" v-model.number="aprovarModal.form.valorRecorrente" step="0.01" min="0.01" required />
            </div>
          </div>
          <div class="form-group">
            <label>Operador Responsável *</label>
            <input type="text" v-model="aprovarModal.form.operador" placeholder="Nome do operador" required />
          </div>
          <div class="form-group">
            <label>Justificativa (mín. 10 caracteres) *</label>
            <textarea v-model="aprovarModal.form.justificativa" rows="3" minlength="10" placeholder="Motivo da aprovação manual da assinatura" required></textarea>
          </div>
          <button type="submit" class="btn btn-primary btn-block mt-4" :disabled="aprovando">
            {{ aprovando ? 'Enviando...' : 'Confirmar Aprovação' }}
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

// Área landlord: usa o shell administrativo (sidebar + header) fornecido pelo layout `admin`.
definePageMeta({ layout: 'admin' })

const route = useRoute()
const router = useRouter()

const isNew = ref(route.params.id === 'novo')
const apiOnline = ref(true)
const saving = ref(false)
const currentTab = ref('dados')

const tabs = [
  { id: 'dados', name: 'Dados Gerais' },
  { id: 'enderecos', name: 'Endereços' },
  { id: 'composicoes', name: 'Faturamento Mensal' }
]

const plans = ref([])
const revendas = ref([])
const vendedores = ref([])

const cliente = reactive({
  id: '',
  razaoSocial: '',
  cnpj: '',
  email: '',
  planoId: '',
  revendaId: null,
  vendedorId: null,
  diaVencimento: 10,
  ativo: true,
  telefone: '',
  nomeContato: '',
  isDemo: false,
  tokenAcesso: '',
  cotaUsuarios: null,
  cotaEmpresas: null,
  cotaPermissoes: null,
  enderecos: [],
  composicoes: []
})

// Modais States
const enderecoModal = reactive({
  open: false,
  index: -1,
  form: {
    id: '',
    tipoEndereco: 1,
    paisId: 'a3d07384-d113-4956-aab9-e58f00030000', // Default Brasil Guid
    municipioId: 'b3d07384-d113-4956-aab9-e58f00030000', // Default SP Guid
    uf: '',
    cep: '',
    logradouro: '',
    numero: '',
    complemento: '',
    bairro: '',
    referencia: '',
    principal: false
  }
})

const composicaoModal = reactive({
  open: false,
  index: -1,
  form: {
    id: '',
    descricao: '',
    valor: 150.00,
    dataInicial: new Date().toISOString().split('T')[0],
    dataFinal: '',
    podeReajustar: true
  }
})

// Estado da aprovação manual de assinatura.
const aprovando = ref(false)
const aprovarModal = reactive({
  open: false,
  form: {
    dataInicio: new Date().toISOString().split('T')[0],
    dataFim: '',
    diaVencimento: 10,
    valorRecorrente: 0,
    operador: '',
    justificativa: ''
  }
})

onMounted(async () => {
  await checkApiConnection()
  await loadMetadataSelects()
  if (!isNew.value) {
    await loadClienteDetalhado()
  }
})

const checkApiConnection = async () => {
  try {
    await useApi('/plataforma/superadmin/dashboard')
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
  }
}

const loadMetadataSelects = async () => {
  if (apiOnline.value) {
    try {
      // Carrega planos: tenta o endpoint landlord `/plataforma/planos` quando existir;
      // se não existir/estiver indisponível, cai para a lista pública (fallback).
      await carregarPlanos()

      // Carrega revendas
      const revRes = await useApi('/plataforma/revendas', { query: { tamanhoPagina: 100 } })
      revendas.value = revRes.items

      // Carrega vendedores
      const vendRes = await useApi('/plataforma/vendedores', { query: { tamanhoPagina: 100 } })
      vendedores.value = vendRes.items
    } catch (e) {
      loadMetadataSimulado()
    }
  } else {
    loadMetadataSimulado()
  }
}

const loadMetadataSimulado = () => {
  plans.value = [
    { id: 'plano-silver', name: 'Plano Silver', price: 299.90 },
    { id: 'plano-gold', name: 'Plano Gold', price: 499.90 },
    { id: 'plano-platinum', name: 'Plano Platinum', price: 899.90 }
  ]
  revendas.value = [
    { id: '1b10b651-789a-41f2-95b7-a3a8080c0001', nome: 'Revenda São Paulo TI' },
    { id: '2b10b651-789a-41f2-95b7-a3a8080c0002', nome: 'Revenda Nordeste Sistemas' }
  ]
  vendedores.value = [
    { id: '1a10b651-789a-41f2-95b7-a3a8080c0001', nome: 'Roberto Silva' },
    { id: '2a10b651-789a-41f2-95b7-a3a8080c0002', nome: 'Ana Souza' }
  ]
}

const loadClienteDetalhado = async () => {
  if (apiOnline.value) {
    try {
      const res = await useApi(`/plataforma/clientes/${route.params.id}`)
      Object.assign(cliente, {
        id: res.id,
        razaoSocial: res.razaoSocial,
        cnpj: res.cnpj,
        email: res.email,
        planoId: res.planoId,
        revendaId: res.revendaId,
        vendedorId: res.vendedorId,
        diaVencimento: res.diaVencimento,
        ativo: res.ativo,
        telefone: res.telefone || '',
        nomeContato: res.nomeContato || '',
        isDemo: res.isDemo,
        tokenAcesso: res.tokenAcesso || '',
        cotaUsuarios: res.cotaUsuarios ?? null,
        cotaEmpresas: res.cotaEmpresas ?? null,
        cotaPermissoes: res.cotaPermissoes ?? null,
        enderecos: res.enderecos || [],
        composicoes: res.composicoes || []
      })
    } catch (e) {
      loadClienteSimulado()
    }
  } else {
    loadClienteSimulado()
  }
}

const loadClienteSimulado = () => {
  const stored = localStorage.getItem('epros_tenants')
  if (stored) {
    const list = JSON.parse(stored)
    const item = list.find(c => c.id === route.params.id)
    if (item) {
      Object.assign(cliente, {
        id: item.id,
        razaoSocial: item.name,
        cnpj: item.cnpj || '',
        email: item.email,
        planoId: item.planId || 'plano-silver',
        revendaId: item.revendaId || null,
        vendedorId: item.vendedorId || null,
        diaVencimento: item.diaVencimento || 10,
        ativo: item.status === 'Ativo',
        telefone: item.telefone || '',
        nomeContato: item.nomeContato || '',
        isDemo: item.isDemo || false,
        tokenAcesso: '',
        enderecos: item.enderecos || [],
        composicoes: item.composicoes || []
      })
    }
  }
}

// Carrega planos para o select de PlanoId. Prioriza o endpoint landlord `/plataforma/planos`
// (quando existir) e faz fallback para a lista pública. Se ambos falharem, o form ainda funciona
// (o valor de PlanoId persiste e pode ser mantido/salvo como texto/GUID).
const carregarPlanos = async () => {
  try {
    const res = await useApi('/plataforma/planos', { query: { tamanhoPagina: 100 } })
    const lista = res?.items ?? res?.dados ?? res?.data ?? res
    if (Array.isArray(lista) && lista.length) {
      plans.value = lista.map(p => ({
        id: p.id,
        name: p.nome ?? p.name ?? p.descricao ?? 'Plano',
        price: Number(p.preco ?? p.valor ?? p.price ?? 0)
      }))
      return
    }
  } catch (e) {
    // endpoint landlord ainda não existe (GAP-1) — segue para o fallback público
  }
  try {
    const planRes = await useApi('/public/AreaPublica/planos')
    plans.value = planRes.map(p => ({ id: p.id, name: p.nome, price: p.preco }))
  } catch (e) {
    // sem planos disponíveis — mantém o que já houver; PlanoId continua editável
  }
}

const openAprovarModal = () => {
  aprovarModal.form.dataInicio = new Date().toISOString().split('T')[0]
  aprovarModal.form.dataFim = ''
  aprovarModal.form.diaVencimento = cliente.diaVencimento || 10
  aprovarModal.form.valorRecorrente = 0
  aprovarModal.form.operador = ''
  aprovarModal.form.justificativa = ''
  aprovarModal.open = true
}

const aprovarAssinaturaManual = async () => {
  if (aprovarModal.form.justificativa.trim().length < 10) {
    alert('A justificativa deve ter no mínimo 10 caracteres.')
    return
  }
  aprovando.value = true
  try {
    // Rota real no backend novo: SuperAdminController → api/v1/plataforma/superadmin/clientes/{id}/aprovar-assinatura-manual
    const res = await useApi(`/plataforma/superadmin/clientes/${cliente.id}/aprovar-assinatura-manual`, {
      method: 'POST',
      body: {
        ClienteId: cliente.id,
        DataInicio: aprovarModal.form.dataInicio,
        DataFim: aprovarModal.form.dataFim || null,
        DiaVencimento: aprovarModal.form.diaVencimento,
        ValorRecorrente: aprovarModal.form.valorRecorrente,
        FaturaPendenteIdParaBaixar: null,
        Operador: aprovarModal.form.operador,
        Justificativa: aprovarModal.form.justificativa
      }
    })
    if (res?.sucesso === false) {
      alert(`Falha ao aprovar assinatura: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    alert('Assinatura manual aprovada com sucesso!')
    aprovarModal.open = false
    await loadClienteDetalhado()
  } catch (e) {
    alert(`Erro ao aprovar assinatura: ${e.message}`)
  } finally {
    aprovando.value = false
  }
}

const saveClient = async () => {
  saving.value = true
  if (apiOnline.value) {
    try {
      if (isNew.value) {
        // Envia POST de criação básica
        const res = await useApi('/plataforma/clientes', {
          method: 'POST',
          body: {
            RazaoSocial: cliente.razaoSocial,
            Cnpj: cliente.cnpj.replace(/\D/g, ''),
            Email: cliente.email,
            PlanoId: cliente.planoId,
            Telefone: cliente.telefone,
            NomeContato: cliente.nomeContato,
            IsDemo: cliente.isDemo
          }
        })
        if (res.sucesso) {
          // Redireciona para configurar detalhes (edit)
          router.push(`/plataforma/admin/clientes/${res.dados.clienteId}`)
        } else {
          alert(`Falha no cadastro: ${res.mensagem}`)
        }
      } else {
        // Envia PUT com o detalhado completo
        const res = await useApi(`/plataforma/clientes/${cliente.id}`, {
          method: 'PUT',
          body: {
            Id: cliente.id,
            RazaoSocial: cliente.razaoSocial,
            Cnpj: cliente.cnpj.replace(/\D/g, ''),
            Email: cliente.email,
            PlanoId: cliente.planoId,
            RevendaId: cliente.revendaId,
            VendedorId: cliente.vendedorId,
            DiaVencimento: cliente.diaVencimento,
            Ativo: cliente.ativo,
            Telefone: cliente.telefone,
            NomeContato: cliente.nomeContato,
            IsDemo: cliente.isDemo,
            TokenAcesso: cliente.tokenAcesso,
            CotaUsuarios: cliente.cotaUsuarios,
            CotaEmpresas: cliente.cotaEmpresas,
            CotaPermissoes: cliente.cotaPermissoes,
            Enderecos: cliente.enderecos.map(e => ({
              Id: e.id || '00000000-0000-0000-0000-000000000000',
              TipoEndereco: e.tipoEndereco,
              PaisId: e.paisId,
              MunicipioId: e.municipioId,
              SubdivisaoId: e.subdivisaoId,
              Uf: e.uf,
              Cep: e.cep,
              Logradouro: e.logradouro,
              Numero: e.numero,
              Complemento: e.complemento,
              Bairro: e.bairro,
              Referencia: e.referencia,
              Principal: e.principal || false
            })),
            Composicoes: cliente.composicoes.map(c => ({
              Id: c.id || '00000000-0000-0000-0000-000000000000',
              Descricao: c.descricao,
              Valor: c.valor,
              DataInicial: c.dataInicial,
              DataFinal: c.dataFinal || null,
              PodeReajustar: c.podeReajustar
            }))
          }
        })
        if (res.sucesso) {
          alert('Informações atualizadas com sucesso!')
          router.push('/plataforma/admin/clientes')
        } else {
          alert(`Erro ao salvar: ${res.mensagem}`)
        }
      }
    } catch (e) {
      alert(`Erro na requisição: ${e.message}`)
    }
  } else {
    // Simulado local
    let list = JSON.parse(localStorage.getItem('epros_tenants') || '[]')
    if (isNew.value) {
      const newId = cliente.razaoSocial.toLowerCase().replace(/[^a-z0-9]/g, '_')
      const planName = plans.value.find(p => p.id === cliente.planoId)?.name || 'Plano Silver'
      const newTenant = {
        id: newId,
        tenantId: newId,
        name: cliente.razaoSocial,
        email: cliente.email,
        cnpj: cliente.cnpj,
        plan: planName,
        planId: cliente.planoId,
        status: 'Ativo',
        diaVencimento: cliente.diaVencimento,
        telefone: cliente.telefone,
        nomeContato: cliente.nomeContato,
        isDemo: cliente.isDemo,
        enderecos: [],
        composicoes: []
      }
      list.push(newTenant)
      localStorage.setItem('epros_tenants', JSON.stringify(list))
      router.push(`/plataforma/admin/clientes/${newId}`)
    } else {
      const idx = list.findIndex(c => c.id === cliente.id)
      if (idx !== -1) {
        list[idx].name = cliente.razaoSocial
        list[idx].email = cliente.email
        list[idx].cnpj = cliente.cnpj
        list[idx].planId = cliente.planoId
        list[idx].plan = plans.value.find(p => p.id === cliente.planoId)?.name || list[idx].plan
        list[idx].status = cliente.ativo ? 'Ativo' : 'Suspenso'
        list[idx].diaVencimento = cliente.diaVencimento
        list[idx].telefone = cliente.telefone
        list[idx].nomeContato = cliente.nomeContato
        list[idx].isDemo = cliente.isDemo
        list[idx].enderecos = cliente.enderecos
        list[idx].composicoes = cliente.composicoes
        localStorage.setItem('epros_tenants', JSON.stringify(list))
        alert('[Simulado] Cliente salvo com sucesso!')
        router.push('/plataforma/admin/clientes')
      }
    }
  }
  saving.value = false
}

// Endereços Helpers
const getTipoEnderecoLabel = (tipo) => {
  switch (tipo) {
    case 1: return 'Principal'
    case 2: return 'Cobrança'
    case 3: return 'Entrega'
    default: return 'Outros'
  }
}

const openEnderecoModal = (end = null, index = -1) => {
  enderecoModal.index = index
  if (end) {
    Object.assign(enderecoModal.form, { ...end })
  } else {
    Object.assign(enderecoModal.form, {
      id: '',
      tipoEndereco: 1,
      paisId: 'a3d07384-d113-4956-aab9-e58f00030000',
      municipioId: 'b3d07384-d113-4956-aab9-e58f00030000',
      uf: '',
      cep: '',
      logradouro: '',
      numero: '',
      complemento: '',
      bairro: '',
      referencia: ''
    })
  }
  enderecoModal.open = true
}

const saveEnderecoModal = () => {
  if (enderecoModal.index === -1) {
    cliente.enderecos.push({ ...enderecoModal.form })
  } else {
    cliente.enderecos[enderecoModal.index] = { ...enderecoModal.form }
  }
  enderecoModal.open = false
}

const removeEndereco = (index) => {
  if (confirm('Remover este endereço?')) {
    cliente.enderecos.splice(index, 1)
  }
}

const fetchCep = async () => {
  const clean = enderecoModal.form.cep.replace(/\D/g, '')
  if (clean.length === 8) {
    try {
      // Serviço externo de CEP (não é a nossa API): usa fetch nativo do navegador.
      const resp = await fetch(`https://viacep.com.br/ws/${clean}/json/`)
      const res = await resp.json()
      if (!res.erro) {
        enderecoModal.form.logradouro = res.logradouro
        enderecoModal.form.bairro = res.bairro
        enderecoModal.form.uf = res.uf
      }
    } catch (e) {}
  }
}

// Composições Helpers
const openComposicaoModal = (comp = null, index = -1) => {
  composicaoModal.index = index
  if (comp) {
    Object.assign(composicaoModal.form, {
      ...comp,
      dataInicial: comp.dataInicial.split('T')[0],
      dataFinal: comp.dataFinal ? comp.dataFinal.split('T')[0] : ''
    })
  } else {
    Object.assign(composicaoModal.form, {
      id: '',
      descricao: '',
      valor: 150.00,
      dataInicial: new Date().toISOString().split('T')[0],
      dataFinal: '',
      podeReajustar: true
    })
  }
  composicaoModal.open = true
}

const saveComposicaoModal = () => {
  if (composicaoModal.index === -1) {
    cliente.composicoes.push({ ...composicaoModal.form })
  } else {
    cliente.composicoes[composicaoModal.index] = { ...composicaoModal.form }
  }
  composicaoModal.open = false
}

const removeComposicao = (index) => {
  if (confirm('Remover este item de faturamento?')) {
    cliente.composicoes.splice(index, 1)
  }
}

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return d.toLocaleDateString('pt-BR')
}
</script>

<style scoped>
.form-focused-layout {
  max-width: 900px;
  margin: 0 auto;
}
.form-tabs {
  display: flex;
  gap: 8px;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 12px;
}
.tab-link {
  background: none;
  border: none;
  color: var(--text-secondary);
  padding: 8px 16px;
  font-size: 13.5px;
  font-weight: 600;
  cursor: pointer;
  border-radius: 6px;
  transition: all 0.2s ease;
}
.tab-link:hover, .tab-link.active {
  background: rgba(255,255,255,0.03);
  color: var(--text-primary);
}
.tab-link.active {
  box-shadow: 0 0 10px rgba(99, 102, 241, 0.15);
  border: 1px solid rgba(99, 102, 241, 0.2);
}
.form-row {
  display: flex;
  gap: 16px;
  margin-bottom: 12px;
}
.col-6 { flex: 0 0 calc(50% - 8px); }
.col-4 { flex: 0 0 calc(33.33% - 10.6px); }
.col-3 { flex: 0 0 calc(25% - 12px); }
.col-9 { flex: 0 0 calc(75% - 4px); }

@media (max-width: 600px) {
  .form-row { flex-direction: column; gap: 12px; }
  .col-6, .col-4, .col-3, .col-9 { flex: 0 0 100%; }
}

.tab-section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 16px;
  margin-bottom: 12px;
}
.btn-sm {
  padding: 6px 12px;
  font-size: 12px;
}
.form-footer {
  display: flex;
  gap: 12px;
  border-top: 1px solid var(--border-color);
  padding-top: 16px;
}
.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.65);
  backdrop-filter: blur(8px);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}
.modal-card {
  width: 520px;
  max-width: 95%;
  padding: 24px;
  border: 1px solid rgba(255,255,255,0.1);
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.5);
}
.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 8px;
}
.btn-close {
  background: none;
  border: none;
  color: var(--text-secondary);
  font-size: 24px;
  cursor: pointer;
}
.btn-close:hover {
  color: var(--text-primary);
}
.mt-2 {
  margin-top: 12px;
}
/* 1.01 front — cota (override do plano) + endereço principal */
.cota-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; margin-top: 6px; }
.cota-lbl { font-size: 12px; color: #64748b; display: block; margin-bottom: 2px; }
.badge-principal { background: #fef9c3; color: #a16207; margin-left: 6px; }
</style>
