<template>
  <div class="perfis-acesso-module">
    <!-- Listagem Principal -->
    <div v-if="!showForm" class="animate-fade">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;">
        <div>
          <h4>🔑 Perfis de Acesso & Permissões de Menu</h4>
          <p style="font-size: 13px; color: var(--text-secondary);">
            Gerencie perfis operacionais e conceda permissões granulares de leitura, escrita e exclusão para cada menu do ERP.
          </p>
        </div>
        <button type="button" @click="openCreateForm" class="btn btn-primary">
          ➕ Novo Perfil
        </button>
      </div>

      <!-- Tabela de Perfis -->
      <div class="table-container glass-panel">
        <table class="workspace-table">
          <thead>
            <tr>
              <th>Perfil de Acesso</th>
              <th>Status</th>
              <th>Regras Ativas</th>
              <th style="text-align: right;">Ações</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in perfis" :key="p.id" class="table-row-hover">
              <td class="bold-text">{{ p.descricao }}</td>
              <td>
                <span :class="['badge', p.ativo ? 'badge-paga' : 'badge-cancelada']">
                  {{ p.ativo ? 'Ativo' : 'Inativo' }}
                </span>
              </td>
              <td>
                <span class="setting-key-badge">{{ p.quantidadeAcessos }} regras</span>
              </td>
              <td style="text-align: right;">
                <div style="display: flex; gap: 8px; justify-content: flex-end;">
                  <button type="button" @click="openEditForm(p.id)" class="btn-api-sub">✏️ Editar</button>
                  <button type="button" @click="deletePerfil(p.id, p.descricao)" class="btn-api-sub delete-btn">🗑️ Excluir</button>
                </div>
              </td>
            </tr>
            <tr v-if="perfis.length === 0 && !loading">
              <td colspan="4" style="text-align: center; color: var(--text-muted); padding: 30px;">
                Nenhum perfil de acesso cadastrado.
              </td>
            </tr>
            <tr v-if="loading">
              <td colspan="4" style="text-align: center; padding: 30px;">
                <span class="spinner"></span> Carregando...
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Formulário Cadastro/Edição -->
    <div v-else class="animate-fade">
      <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;">
        <div>
          <h4>{{ isEditing ? '🔄 Editar Perfil de Acesso' : '⚡ Cadastrar Novo Perfil de Acesso' }}</h4>
          <p style="font-size: 13px; color: var(--text-secondary);">
            Nomeie o perfil e configure as permissões de acesso aos recursos na matriz abaixo.
          </p>
        </div>
        <button type="button" @click="closeForm" class="btn btn-secondary">
          Voltar para Lista
        </button>
      </div>

      <div class="glass-panel" style="padding: 24px;">
        <form @submit.prevent="saveForm" class="config-form">
          <div class="form-group" style="max-width: 400px; margin-bottom: 24px;">
            <label for="perf-desc">Descrição do Perfil *</label>
            <input type="text" id="perf-desc" v-model="form.descricao" required placeholder="Ex: Faturista / Auxiliar Contábil" />
          </div>

          <!-- Matriz de Permissões -->
          <div class="matrix-container">
            <header class="matrix-header flex-row-between">
              <h5>Matriz de Permissões de Menu</h5>
              <div style="display: flex; gap: 8px;">
                <button type="button" @click="toggleAll(true)" class="btn btn-secondary btn-small" style="font-size: 11px;">Marcar Todos</button>
                <button type="button" @click="toggleAll(false)" class="btn btn-secondary btn-small" style="font-size: 11px;">Desmarcar Todos</button>
              </div>
            </header>

            <div class="permissions-tree-table">
              <div class="tree-table-header">
                <div class="col-name">Menu / Funcionalidade</div>
                <div class="col-check text-center">Ver (Leitura)</div>
                <div class="col-check text-center">Editar (Gravação)</div>
                <div class="col-check text-center">Excluir (Remoção)</div>
              </div>

              <!-- Loop Menus -->
              <div v-for="menu in menuCatalog" :key="menu.id" class="tree-menu-node">
                <!-- Nível 0: Menu Principal -->
                <div class="tree-row row-level-0">
                  <div class="col-name">
                    <span class="tree-icon">{{ menu.icon || '📁' }}</span>
                    <span class="menu-name">{{ menu.descricao }}</span>
                    <span v-if="menu.modulo" class="badge-module">{{ menu.modulo }}</span>
                  </div>
                  <div class="col-check text-center">
                    <input type="checkbox" v-model="matrixState[menu.id].ver" @change="onParentChange(menu, 0)" />
                  </div>
                  <div class="col-check text-center">
                    <input type="checkbox" v-model="matrixState[menu.id].editar" @change="onParentChange(menu, 0)" />
                  </div>
                  <div class="col-check text-center">
                    <input type="checkbox" v-model="matrixState[menu.id].excluir" @change="onParentChange(menu, 0)" />
                  </div>
                </div>

                <!-- Nível 1: Submenus Nível 1 -->
                <div v-for="item1 in menu.itensNivel1" :key="item1.id" class="tree-menu-node">
                  <div class="tree-row row-level-1">
                    <div class="col-name">
                      <span class="indent-guide"></span>
                      <span class="tree-icon">{{ item1.icon || '📄' }}</span>
                      <span class="menu-name">{{ item1.descricao }}</span>
                      <span v-if="item1.modulo" class="badge-module">{{ item1.modulo }}</span>
                    </div>
                    <div class="col-check text-center">
                      <input type="checkbox" v-model="matrixState[item1.id].ver" @change="onParentChange(item1, 1, menu.id)" />
                    </div>
                    <div class="col-check text-center">
                      <input type="checkbox" v-model="matrixState[item1.id].editar" @change="onParentChange(item1, 1, menu.id)" />
                    </div>
                    <div class="col-check text-center">
                      <input type="checkbox" v-model="matrixState[item1.id].excluir" @change="onParentChange(item1, 1, menu.id)" />
                    </div>
                  </div>

                  <!-- Nível 2: Submenus Nível 2 -->
                  <div v-for="item2 in item1.itensNivel2" :key="item2.id" class="tree-row row-level-2">
                    <div class="col-name">
                      <span class="indent-guide double"></span>
                      <span class="tree-icon">▪</span>
                      <span class="menu-name">{{ item2.descricao }}</span>
                    </div>
                    <div class="col-check text-center">
                      <input type="checkbox" v-model="matrixState[item2.id].ver" @change="onLeafChange(item2.id, item1.id, menu.id)" />
                    </div>
                    <div class="col-check text-center">
                      <input type="checkbox" v-model="matrixState[item2.id].editar" @change="onLeafChange(item2.id, item1.id, menu.id)" />
                    </div>
                    <div class="col-check text-center">
                      <input type="checkbox" v-model="matrixState[item2.id].excluir" @change="onLeafChange(item2.id, item1.id, menu.id)" />
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="form-actions" style="margin-top: 30px;">
            <button type="submit" class="btn btn-success" :disabled="saving">
              {{ saving ? 'Salvando...' : '💾 Gravar Perfil & Permissões' }}
            </button>
            <button type="button" @click="closeForm" class="btn btn-secondary">
              Cancelar
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const loading = ref(false)
const saving = ref(false)
const perfis = ref([])
const showForm = ref(false)
const isEditing = ref(false)

const menuCatalog = ref([])
const matrixState = ref({})

const form = ref({
  id: null,
  descricao: ''
})

onMounted(async () => {
  await loadPerfis()
  await loadMenuCatalog()
})

const loadPerfis = async () => {
  loading.value = true
  try {
    const data = await useApi('/plataforma/perfis-acesso')
    perfis.value = data.dados?.itens || []
  } catch (e) {
    console.error('Falha ao listar perfis de acesso.', e)
  } finally {
    loading.value = false
  }
}

const loadMenuCatalog = async () => {
  try {
    const result = await useApi('/plataforma/menus')
    menuCatalog.value = result.dados || []
    resetMatrixState()
  } catch (e) {
    console.error('Falha ao carregar catálogo de menus.', e)
  }
}

const resetMatrixState = () => {
  const state = {}
  
  // Recursivamente inicializa o estado de checkboxes para todos os níveis de menu
  menuCatalog.value.forEach(menu => {
    state[menu.id] = { ver: false, editar: false, excluir: false }
    menu.itensNivel1.forEach(item1 => {
      state[item1.id] = { ver: false, editar: false, excluir: false }
      item1.itensNivel2.forEach(item2 => {
        state[item2.id] = { ver: false, editar: false, excluir: false }
      })
    })
  })

  matrixState.value = state
}

const toggleAll = (checked) => {
  Object.keys(matrixState.value).forEach(key => {
    matrixState.value[key] = { ver: checked, editar: checked, excluir: checked }
  })
}

// Lógica de Toggles em Cascata do Menu
const onParentChange = (node, level, parentId = null) => {
  const nodeState = matrixState.value[node.id]
  
  // 1. Se marcarmos 'Editar' ou 'Excluir', precisamos marcar 'Ver' automaticamente
  if (nodeState.editar || nodeState.excluir) {
    nodeState.ver = true
  }

  // 2. Se desmarcarmos 'Ver', desmarcamos tudo abaixo e no próprio nível
  if (!nodeState.ver) {
    nodeState.editar = false
    nodeState.excluir = false
  }

  // Cascata para Baixo
  if (level === 0) {
    node.itensNivel1.forEach(item1 => {
      matrixState.value[item1.id].ver = nodeState.ver
      matrixState.value[item1.id].editar = nodeState.editar
      matrixState.value[item1.id].excluir = nodeState.excluir

      item1.itensNivel2.forEach(item2 => {
        matrixState.value[item2.id].ver = nodeState.ver
        matrixState.value[item2.id].editar = nodeState.editar
        matrixState.value[item2.id].excluir = nodeState.excluir
      })
    })
  } else if (level === 1) {
    node.itensNivel2.forEach(item2 => {
      matrixState.value[item2.id].ver = nodeState.ver
      matrixState.value[item2.id].editar = nodeState.editar
      matrixState.value[item2.id].excluir = nodeState.excluir
    })
    
    // Se marcar este nível como Ver, precisamos garantir que o menu pai (Nível 0) também esteja marcado
    if (nodeState.ver && parentId) {
      matrixState.value[parentId].ver = true
    }
  }
}

const onLeafChange = (id, item1Id, menuId) => {
  const leafState = matrixState.value[id]

  if (leafState.editar || leafState.excluir) {
    leafState.ver = true
  }
  if (!leafState.ver) {
    leafState.editar = false
    leafState.excluir = false
  }

  // Garantir que os pais sejam marcados se ativado 'Ver'
  if (leafState.ver) {
    matrixState.value[item1Id].ver = true
    matrixState.value[menuId].ver = true
  }
}

// Ações do Form
const openCreateForm = () => {
  form.value = { id: null, descricao: '' }
  resetMatrixState()
  isEditing.value = false
  showForm.value = true
}

const openEditForm = async (id) => {
  loading.value = true
  try {
    const data = await useApi(`/plataforma/perfis-acesso/${id}`)
    const p = data.dados
    form.value = { id: p.id, descricao: p.descricao }
    
    resetMatrixState()
    
    // Aplicar estados salvos
    p.acessos.forEach(acesso => {
      const idKey = acesso.menuItemNivel2Id || acesso.menuItemNivel1Id || acesso.menuId
      if (matrixState.value[idKey]) {
        matrixState.value[idKey] = {
          ver: acesso.ver,
          editar: acesso.editar,
          excluir: acesso.excluir
        }
      }
    })

    isEditing.value = true
    showForm.value = true
  } catch (e) {
    console.error(e)
    alert('Erro ao carregar dados do perfil.')
  } finally {
    loading.value = false
  }
}

const closeForm = () => {
  showForm.value = false
  form.value = { id: null, descricao: '' }
  resetMatrixState()
}

const saveForm = async () => {
  saving.value = true
  try {
    // Construir lista de acessos achatada baseada no matrixState e menuCatalog
    const acessos = []

    menuCatalog.value.forEach(menu => {
      const s0 = matrixState.value[menu.id]
      if (s0.ver || s0.editar || s0.excluir) {
        acessos.push({
          menuId: menu.id,
          menuItemNivel1Id: null,
          menuItemNivel2Id: null,
          ver: s0.ver,
          editar: s0.editar,
          excluir: s0.excluir
        })
      }

      menu.itensNivel1.forEach(item1 => {
        const s1 = matrixState.value[item1.id]
        if (s1.ver || s1.editar || s1.excluir) {
          acessos.push({
            menuId: menu.id,
            menuItemNivel1Id: item1.id,
            menuItemNivel2Id: null,
            ver: s1.ver,
            editar: s1.editar,
            excluir: s1.excluir
          })
        }

        item1.itensNivel2.forEach(item2 => {
          const s2 = matrixState.value[item2.id]
          if (s2.ver || s2.editar || s2.excluir) {
            acessos.push({
              menuId: menu.id,
              menuItemNivel1Id: item1.id,
              menuItemNivel2Id: item2.id,
              ver: s2.ver,
              editar: s2.editar,
              excluir: s2.excluir
            })
          }
        })
      })
    })

    const payload = {
      descricao: form.value.descricao,
      acessos
    }

    let rota = '/plataforma/perfis-acesso'
    let method = 'POST'

    if (isEditing.value) {
      rota = `/plataforma/perfis-acesso/${form.value.id}`
      method = 'PUT'
      payload.id = form.value.id
    }

    const result = await useApi(rota, {
      method,
      body: payload
    })

    if (result.sucesso) {
      alert(isEditing.value ? 'Perfil de acesso atualizado com sucesso!' : 'Perfil de acesso criado com sucesso!')
      closeForm()
      await loadPerfis()
    } else {
      const errMsg = result.erros ? result.erros.join('\n') : result.mensagem
      alert(`Erro ao salvar perfil:\n${errMsg}`)
    }
  } catch (e) {
    console.error(e)
    const errMsg = e.data?.erros ? e.data.erros.join('\n') : (e.data?.mensagem || e.message)
    alert(`Erro de conexão ao salvar perfil:\n${errMsg}`)
  } finally {
    saving.value = false
  }
}

const deletePerfil = async (id, desc) => {
  if (!confirm(`Deseja realmente excluir o perfil de acesso "${desc}"?`)) return

  try {
    await useApi(`/plataforma/perfis-acesso/${id}`, {
      method: 'DELETE'
    })
    alert('Perfil excluído com sucesso!')
    await loadPerfis()
  } catch (e) {
    console.error(e)
    alert('Erro ao tentar deletar o perfil de acesso.')
  }
}
</script>

<style scoped>
.perfis-acesso-module {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.delete-btn {
  color: var(--danger);
  border-color: rgba(239, 68, 68, 0.2);
}
.delete-btn:hover {
  background: rgba(239, 68, 68, 0.1) !important;
  border-color: var(--danger) !important;
}

/* Tree Grid permissions */
.matrix-container {
  border: 1px solid var(--border-color);
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.01);
  overflow: hidden;
}

.matrix-header {
  padding: 16px 20px;
  border-bottom: 1px solid var(--border-color);
  background: rgba(255,255,255,0.01);
}

.permissions-tree-table {
  display: flex;
  flex-direction: column;
}

.tree-table-header {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 1fr;
  padding: 12px 20px;
  border-bottom: 1px solid var(--border-color);
  background: rgba(255,255,255,0.03);
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--text-secondary);
}

.tree-row {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 1fr;
  padding: 12px 20px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.02);
  align-items: center;
  transition: background 0.15s ease;
}

.tree-row:hover {
  background: rgba(255, 255, 255, 0.01);
}

.row-level-0 {
  background: rgba(255, 255, 255, 0.02);
  font-weight: 700;
  border-bottom-color: rgba(255,255,255,0.05);
}

.row-level-1 {
  font-weight: 600;
}

.row-level-2 {
  color: var(--text-secondary);
  font-size: 13px;
}

.col-name {
  display: flex;
  align-items: center;
  gap: 8px;
}

.tree-icon {
  font-size: 14px;
}

.indent-guide {
  display: inline-block;
  width: 20px;
  height: 18px;
  border-left: 1px dashed rgba(255, 255, 255, 0.15);
  border-bottom: 1px dashed rgba(255, 255, 255, 0.15);
  margin-right: 6px;
}

.indent-guide.double {
  width: 40px;
  border-left-width: 1px;
  border-right: 1px dashed rgba(255, 255, 255, 0.15);
  margin-right: 8px;
}

.badge-module {
  font-size: 9px;
  font-weight: 700;
  padding: 1px 5px;
  border-radius: 4px;
  text-transform: uppercase;
  background: rgba(99, 102, 241, 0.1);
  color: #818cf8;
  border: 1px solid rgba(99, 102, 241, 0.2);
}

.col-check input[type="checkbox"] {
  width: 18px;
  height: 18px;
  cursor: pointer;
}

.text-center {
  text-align: center;
}
</style>
