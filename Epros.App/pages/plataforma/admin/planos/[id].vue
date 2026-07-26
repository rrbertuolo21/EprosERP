<template>
  <div class="dashboard-layout">
    <!-- Conteúdo Principal (o shell/header vêm do layout `admin`) -->
    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">{{ isNew ? 'Novo Plano' : 'Editar Plano' }}</h1>
        <p class="tagline">{{ isNew ? 'Cadastre um novo plano comercial e monte a lista de módulos inclusos.' : 'Ajuste valores, limites, status e os módulos que compõem o plano.' }}</p>
        <div class="header-actions">
          <NuxtLink to="/plataforma/admin/planos" class="btn btn-secondary btn-back">
            ← Voltar para Lista
          </NuxtLink>
          <span class="status-pill" :class="{ 'offline': !apiOnline }">
            <span class="status-dot"></span>
            {{ apiOnline ? 'Conectado à API Gateway' : 'Modo Simulação Offline' }}
          </span>
        </div>
      </header>

      <div class="admin-grid-layout form-focused-layout">
        <section class="admin-section form-card glass-panel col-span-2">
          <form @submit.prevent="salvarPlano" class="vertical-form mt-2">
            <!-- DADOS GERAIS -->
            <div class="form-tab-content">
              <div class="form-row">
                <div class="form-group col-6">
                  <label for="p-nome">Nome do Plano *</label>
                  <input type="text" id="p-nome" v-model="plano.nome" placeholder="Ex: Plano Gold" required />
                </div>
                <div class="form-group col-6">
                  <label for="p-grupo">Grupo de Planos</label>
                  <select id="p-grupo" v-model="plano.grupoPlanoId">
                    <option :value="null">Nenhum</option>
                    <option v-for="g in grupos" :key="g.id" :value="g.id">
                      {{ g.descricao ?? g.nome }}
                    </option>
                  </select>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group col-4">
                  <label for="p-limite-usuarios">Limite de Usuários</label>
                  <input type="number" id="p-limite-usuarios" v-model.number="plano.limiteUsuarios" min="0" placeholder="0 = ilimitado" />
                </div>
                <div class="form-group col-4">
                  <label for="p-limite-empresas">Limite de Empresas</label>
                  <input type="number" id="p-limite-empresas" v-model.number="plano.limiteEmpresas" min="0" placeholder="0 = ilimitado" />
                </div>
                <div class="form-group col-4">
                  <label for="p-valor">Valor Mensal (R$) *</label>
                  <input type="number" id="p-valor" v-model.number="plano.valor" step="0.01" min="0" required />
                  <small v-if="totalModulos > 0" class="hint-sugestao" @click="aplicarSugestao">
                    Sugestão (Σ módulos): {{ formatMoney(totalModulos) }} — clique para aplicar
                  </small>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group col-12">
                  <label for="p-recursos">Recursos Inclusos</label>
                  <textarea id="p-recursos" v-model="plano.recursosInclusos" rows="3" placeholder="Descreva os recursos inclusos no plano (um por linha ou texto livre)"></textarea>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group toggle-row col-6">
                  <label for="p-ativo">Plano Ativo</label>
                  <input type="checkbox" id="p-ativo" v-model="plano.ativo" />
                </div>
              </div>
            </div>

            <!-- SUB-LISTA DE MÓDULOS -->
            <div class="form-tab-content mt-4">
              <header class="tab-section-header">
                <h4>Módulos do Plano</h4>
                <div class="add-modulo-bar">
                  <select v-model="moduloSelecionadoId" class="add-modulo-select">
                    <option value="">Selecione um módulo do catálogo...</option>
                    <option v-for="m in catalogoDisponivel" :key="m.id" :value="m.id">
                      {{ m.nomeModulo }}{{ m.precoMensal ? ' — ' + formatMoney(m.precoMensal) : '' }}
                    </option>
                  </select>
                  <button type="button" class="btn btn-secondary btn-sm" :disabled="!moduloSelecionadoId" @click="adicionarModulo">
                    + Adicionar
                  </button>
                </div>
              </header>

              <div class="table-container mt-2">
                <table class="admin-table">
                  <thead>
                    <tr>
                      <th>Módulo</th>
                      <th>Descrição</th>
                      <th style="width: 140px">Valor (R$)</th>
                      <th style="width: 90px">Ativo</th>
                      <th class="align-right">Ações</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-if="plano.modulos.length === 0">
                      <td colspan="5" class="empty-cell">Nenhum módulo adicionado ao plano.</td>
                    </tr>
                    <tr v-else v-for="(mod, idx) in plano.modulos" :key="mod.moduloGeralId || idx">
                      <td><span class="tenant-name-txt">{{ mod.nome }}</span></td>
                      <td>
                        <input type="text" v-model="mod.descricao" class="inline-input" placeholder="Descrição" />
                      </td>
                      <td>
                        <input type="number" v-model.number="mod.valor" step="0.01" min="0" class="inline-input" />
                      </td>
                      <td class="align-center">
                        <input type="checkbox" v-model="mod.ativo" />
                      </td>
                      <td class="align-right">
                        <button type="button" @click="removerModulo(idx)" class="btn btn-secondary btn-table-action btn-danger-action">Remover</button>
                      </td>
                    </tr>
                    <tr v-if="plano.modulos.length > 0">
                      <td colspan="2" class="align-right total-label">Total dos módulos</td>
                      <td class="total-valor">{{ formatMoney(totalModulos) }}</td>
                      <td colspan="2"></td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <!-- AÇÕES -->
            <footer class="form-footer mt-4">
              <button type="submit" class="btn btn-primary" :disabled="saving">
                {{ saving ? 'Gravando...' : (isNew ? 'Criar Plano' : 'Salvar Alterações') }}
              </button>
              <NuxtLink to="/plataforma/admin/planos" class="btn btn-secondary">
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
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

// Área landlord: shell administrativo (sidebar + header) via layout `admin`.
definePageMeta({ layout: 'admin' })

const route = useRoute()
const router = useRouter()

const isNew = ref(route.params.id === 'novo')
const apiOnline = ref(true)
const saving = ref(false)

const grupos = ref([])
const catalogo = ref([])
const moduloSelecionadoId = ref('')

const plano = reactive({
  id: '',
  nome: '',
  grupoPlanoId: null,
  valor: 0,
  limiteUsuarios: 0,
  limiteEmpresas: 0,
  recursosInclusos: '',
  ativo: true,
  modulos: []
})

// Soma dos valores dos módulos (sugestão do valor do plano).
const totalModulos = computed(() =>
  plano.modulos.reduce((acc, m) => acc + Number(m.valor || 0), 0)
)

// Catálogo ainda não adicionado ao plano.
const catalogoDisponivel = computed(() => {
  const usados = new Set(plano.modulos.map(m => m.moduloGeralId))
  return catalogo.value.filter(m => !usados.has(m.id))
})

onMounted(async () => {
  await checkApiConnection()
  await Promise.all([carregarGrupos(), carregarCatalogo()])
  if (!isNew.value) {
    await carregarPlano()
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

const extrairLista = (res) => {
  if (Array.isArray(res)) return res
  return res?.items ?? res?.dados?.items ?? res?.dados ?? res?.data ?? []
}

const carregarGrupos = async () => {
  try {
    const res = await useApi('/plataforma/grupos-plano', { query: { tamanhoPagina: 200 } })
    grupos.value = extrairLista(res)
  } catch (e) {
    grupos.value = []
  }
}

const carregarCatalogo = async () => {
  try {
    const res = await useApi('/plataforma/add-ons', { query: { apenasHabilitados: true } })
    catalogo.value = extrairLista(res)
  } catch (e) {
    catalogo.value = []
  }
}

const carregarPlano = async () => {
  try {
    const res = await useApi(`/plataforma/planos/${route.params.id}`)
    const dados = res?.dados ?? res
    Object.assign(plano, {
      id: dados.id,
      nome: dados.nome ?? '',
      grupoPlanoId: dados.grupoPlanoId ?? null,
      valor: Number(dados.valor ?? 0),
      limiteUsuarios: dados.limiteUsuarios ?? 0,
      limiteEmpresas: dados.limiteEmpresas ?? 0,
      recursosInclusos: dados.recursosInclusos ?? '',
      ativo: dados.ativo ?? true,
      modulos: (dados.modulos ?? []).map(m => ({
        moduloGeralId: m.moduloGeralId ?? m.id,
        nome: m.nome ?? '',
        descricao: m.descricao ?? '',
        valor: Number(m.valor ?? 0),
        ativo: m.ativo ?? true
      }))
    })
  } catch (e) {
    apiOnline.value = false
  }
}

const adicionarModulo = () => {
  const item = catalogo.value.find(m => m.id === moduloSelecionadoId.value)
  if (!item) return
  plano.modulos.push({
    moduloGeralId: item.id,
    nome: item.nomeModulo ?? item.alias ?? 'Módulo',
    descricao: item.alias ?? '',
    valor: Number(item.precoMensal ?? 0),
    ativo: true
  })
  moduloSelecionadoId.value = ''
}

const removerModulo = (idx) => {
  plano.modulos.splice(idx, 1)
}

const aplicarSugestao = () => {
  plano.valor = Number(totalModulos.value.toFixed(2))
}

const salvarPlano = async () => {
  saving.value = true
  try {
    const body = {
      Nome: plano.nome,
      GrupoPlanoId: plano.grupoPlanoId || null,
      Valor: plano.valor,
      LimiteUsuarios: plano.limiteUsuarios,
      LimiteEmpresas: plano.limiteEmpresas,
      RecursosInclusos: plano.recursosInclusos,
      Ativo: plano.ativo,
      Modulos: plano.modulos.map(m => ({
        ModuloGeralId: m.moduloGeralId,
        Nome: m.nome,
        Descricao: m.descricao,
        Valor: m.valor,
        Ativo: m.ativo
      }))
    }

    let res
    if (isNew.value) {
      res = await useApi('/plataforma/planos', { method: 'POST', body })
    } else {
      res = await useApi(`/plataforma/planos/${plano.id}`, {
        method: 'PUT',
        body: { Id: plano.id, ...body }
      })
    }

    if (res?.sucesso === false) {
      alert(`Falha ao salvar: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    alert(isNew.value ? 'Plano criado com sucesso!' : 'Plano atualizado com sucesso!')
    router.push('/plataforma/admin/planos')
  } catch (e) {
    alert(`Erro na requisição: ${e.message}`)
  } finally {
    saving.value = false
  }
}

const formatMoney = (v) => {
  const n = Number(v ?? 0)
  return n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
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
.col-6 { flex: 0 0 calc(50% - 8px); }
.col-4 { flex: 0 0 calc(33.33% - 10.6px); }

@media (max-width: 600px) {
  .form-row { flex-direction: column; gap: 12px; }
  .col-6, .col-4, .col-12 { flex: 0 0 100%; }
}

.hint-sugestao {
  display: inline-block;
  margin-top: 6px;
  font-size: 11.5px;
  color: var(--primary);
  cursor: pointer;
}
.hint-sugestao:hover {
  text-decoration: underline;
}
.tab-section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 16px;
  margin-bottom: 12px;
  gap: 16px;
  flex-wrap: wrap;
}
.add-modulo-bar {
  display: flex;
  align-items: center;
  gap: 8px;
}
.add-modulo-select {
  min-width: 260px;
  padding: 6px 10px;
  background: rgba(0, 0, 0, 0.2);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  color: var(--text-primary);
  font-size: 13px;
}
.inline-input {
  width: 100%;
  padding: 6px 8px;
  background: rgba(0, 0, 0, 0.2);
  border: 1px solid var(--border-color);
  border-radius: 6px;
  color: var(--text-primary);
  font-size: 13px;
}
.btn-sm {
  padding: 6px 12px;
  font-size: 12px;
}
.align-center {
  text-align: center;
}
.total-label {
  font-weight: 600;
  color: var(--text-secondary);
}
.total-valor {
  font-weight: 700;
  color: var(--text-primary);
}
.empty-cell {
  text-align: center;
  padding: 32px !important;
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
