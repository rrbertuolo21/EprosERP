<template>
  <div class="dashboard-content">
    <header class="page-header">
      <h1 class="glow-text">{{ isNew ? 'Nova Empresa' : 'Editar Empresa' }}</h1>
      <p class="tagline">
        {{ isNew
          ? 'Cadastre uma nova empresa com dados básicos, regimes fiscais e inscrições.'
          : 'Gerencie os dados cadastrais, regimes, inscrições e o e-mail transacional da empresa.' }}
      </p>
      <div class="header-actions">
        <NuxtLink to="/plataforma/admin/empresas" class="btn btn-secondary btn-back">
          ← Voltar para Lista
        </NuxtLink>
        <span class="status-pill" :class="{ 'offline': !apiOnline }">
          <span class="status-dot"></span>
          {{ apiOnline ? 'Conectado à API Gateway' : 'Sem conexão com a API' }}
        </span>
      </div>
    </header>

    <div class="form-focused-layout">
      <section class="admin-section form-card glass-panel">
        <form @submit.prevent="salvarEmpresa" class="vertical-form mt-2">
          <!-- DADOS BÁSICOS -->
          <h4 class="form-section-title">Dados básicos</h4>
          <div class="form-row">
            <div class="form-group col-6">
              <label for="e-rs">Razão Social *</label>
              <input type="text" id="e-rs" v-model="empresa.razaoSocial" placeholder="Empresa Ltda" required />
            </div>
            <div class="form-group col-6">
              <label for="e-nf">Nome Fantasia</label>
              <input type="text" id="e-nf" v-model="empresa.nomeFantasia" placeholder="Nome fantasia" />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group col-6">
              <label for="e-cnpj">CNPJ *</label>
              <input type="text" id="e-cnpj" v-model="empresa.cnpj" placeholder="00.000.000/0000-00" required />
            </div>
            <div class="form-group col-6">
              <label for="e-grupo-trib">Grupo Tributário (Id)</label>
              <!-- PlanoGrupoId do Blazor legado não existe no backend novo; o agrupamento disponível é o TributarioGrupoId. -->
              <input type="text" id="e-grupo-trib" v-model="empresa.tributarioGrupoId" placeholder="GUID do grupo tributário (opcional)" />
              <!-- TODO: substituir por <select> quando existir endpoint de grupos tributários. -->
            </div>
          </div>

          <!-- REGIMES -->
          <h4 class="form-section-title">Regimes</h4>
          <div class="form-row">
            <div class="form-group col-6">
              <label for="e-reg-apur">Regime de Apuração *</label>
              <select id="e-reg-apur" v-model.number="empresa.regimeApuracao" required>
                <option v-for="op in regimesApuracao" :key="op.valor" :value="op.valor">{{ op.rotulo }}</option>
              </select>
            </div>
            <div class="form-group col-6">
              <label for="e-reg-trib">Regime Tributário *</label>
              <select id="e-reg-trib" v-model.number="empresa.regimeTributario" required>
                <option v-for="op in regimesTributarios" :key="op.valor" :value="op.valor">{{ op.rotulo }}</option>
              </select>
            </div>
          </div>

          <!-- INSCRIÇÕES -->
          <h4 class="form-section-title">Inscrições</h4>
          <div class="form-row">
            <div class="form-group col-6">
              <label for="e-im">Inscrição Municipal</label>
              <input type="text" id="e-im" v-model="empresa.inscricaoMunicipal" placeholder="Insc. municipal" />
            </div>
            <div class="form-group col-6">
              <label for="e-ie">Inscrição Estadual</label>
              <input type="text" id="e-ie" v-model="empresa.inscricaoEstadual" placeholder="Insc. estadual" />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group col-6">
              <label for="e-suframa">Inscrição Suframa</label>
              <input type="text" id="e-suframa" v-model="empresa.inscricaoSuframa" placeholder="Suframa" />
            </div>
            <div class="form-group col-6">
              <label for="e-cnae">CNAE</label>
              <input type="text" id="e-cnae" v-model="empresa.cnae" placeholder="0000-0/00" />
            </div>
          </div>

          <!-- ENDEREÇO (obrigatório pelo comando de empresa) -->
          <h4 class="form-section-title">Endereço</h4>
          <div class="form-row">
            <div class="form-group col-3">
              <label for="e-cep">CEP *</label>
              <input type="text" id="e-cep" v-model="empresa.endereco.cep" placeholder="00000-000" @blur="buscarCep" required />
            </div>
            <div class="form-group col-9">
              <label for="e-logradouro">Logradouro *</label>
              <input type="text" id="e-logradouro" v-model="empresa.endereco.logradouro" placeholder="Rua / Av..." required />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group col-3">
              <label for="e-numero">Número *</label>
              <input type="text" id="e-numero" v-model="empresa.endereco.numero" placeholder="123" required />
            </div>
            <div class="form-group col-3">
              <label for="e-compl">Complemento</label>
              <input type="text" id="e-compl" v-model="empresa.endereco.complemento" placeholder="Sala, Apto..." />
            </div>
            <div class="form-group col-6">
              <label for="e-bairro">Bairro *</label>
              <input type="text" id="e-bairro" v-model="empresa.endereco.bairro" placeholder="Bairro" required />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group col-9">
              <label for="e-cidade">Cidade *</label>
              <input type="text" id="e-cidade" v-model="empresa.endereco.cidade" placeholder="Cidade" required />
            </div>
            <div class="form-group col-3">
              <label for="e-uf">UF *</label>
              <input type="text" id="e-uf" v-model="empresa.endereco.estado" placeholder="SP" maxlength="2" required />
            </div>
          </div>

          <!-- LOGO -->
          <h4 class="form-section-title">Identidade visual</h4>
          <div class="form-row">
            <div class="form-group col-9">
              <label for="e-logo">Logo (URL ou data URI base64)</label>
              <input type="text" id="e-logo" v-model="empresa.logo" placeholder="https://... ou data:image/png;base64,..." />
            </div>
            <div class="form-group col-3" v-if="!isNew">
              <label>&nbsp;</label>
              <button type="button" class="btn btn-secondary btn-block" :disabled="salvandoLogo" @click="alterarLogo">
                {{ salvandoLogo ? 'Enviando...' : 'Atualizar logo' }}
              </button>
            </div>
          </div>

          <!-- E-MAIL TRANSACIONAL (apenas edição) -->
          <template v-if="!isNew">
            <h4 class="form-section-title">E-mail transacional</h4>
            <div class="form-row">
              <div class="form-group col-6">
                <button type="button" class="btn btn-secondary btn-block" :disabled="testandoEmail" @click="testarEmail">
                  {{ testandoEmail ? 'Testando...' : 'Testar envio de e-mail (SMTP)' }}
                </button>
              </div>
            </div>
            <!-- TODO: gestão de certificados digitais (GET/POST/DELETE /cadastros/empresas/{id}/certificados) — tela dedicada. -->
          </template>

          <footer class="form-footer mt-4">
            <button type="submit" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Gravando...' : (isNew ? 'Criar Empresa' : 'Salvar Alterações') }}
            </button>
            <NuxtLink to="/plataforma/admin/empresas" class="btn btn-secondary">Cancelar</NuxtLink>
          </footer>
        </form>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, type CommandResult } from '~/composables/useApi'

const msgErro = (e: unknown) => (e instanceof Error ? e.message : String(e))

interface EnderecoApi {
  logradouro?: string
  numero?: string
  complemento?: string | null
  bairro?: string
  cep?: string
  cidade?: string
  estado?: string
}

interface EmpresaApi {
  id?: string
  razaoSocial?: string
  nomeFantasia?: string | null
  cnpj?: string
  inscricaoEstadual?: string | null
  inscricaoMunicipal?: string | null
  inscricaoSuframa?: string | null
  cnae?: string | null
  regimeTributario?: number
  regimeApuracao?: number
  tributarioGrupoId?: string | null
  logo?: string | null
  endereco?: EnderecoApi | null
}

interface CriarEmpresaResposta {
  empresaId?: string
  EmpresaId?: string
}

interface EnderecoForm {
  logradouro: string
  numero: string
  complemento: string
  bairro: string
  cep: string
  cidade: string
  estado: string
}

interface EmpresaForm {
  id: string
  razaoSocial: string
  nomeFantasia: string
  cnpj: string
  inscricaoEstadual: string
  inscricaoMunicipal: string
  inscricaoSuframa: string
  cnae: string
  regimeTributario: number
  regimeApuracao: number
  tributarioGrupoId: string
  logo: string
  endereco: EnderecoForm
}

// Área landlord: usa o shell administrativo (sidebar + header) fornecido pelo layout `admin`.
definePageMeta({ layout: 'admin' })

const route = useRoute()
const router = useRouter()

const isNew = ref(route.params.id === 'nova' || route.params.id === 'novo')
const apiOnline = ref(true)
const saving = ref(false)
const salvandoLogo = ref(false)
const testandoEmail = ref(false)

// Valores conforme enums do backend (RegimeApuracao / RegimeTributario).
const regimesApuracao = [
  { valor: 1, rotulo: 'Cumulativo' },
  { valor: 2, rotulo: 'Não Cumulativo' },
  { valor: 3, rotulo: 'Misto' }
]
const regimesTributarios = [
  { valor: 1, rotulo: 'Simples Nacional' },
  { valor: 2, rotulo: 'Lucro Presumido' },
  { valor: 3, rotulo: 'Lucro Real' }
]

const empresa = reactive<EmpresaForm>({
  id: '',
  razaoSocial: '',
  nomeFantasia: '',
  cnpj: '',
  inscricaoEstadual: '',
  inscricaoMunicipal: '',
  inscricaoSuframa: '',
  cnae: '',
  regimeTributario: 1,
  regimeApuracao: 1,
  tributarioGrupoId: '',
  logo: '',
  endereco: {
    logradouro: '',
    numero: '',
    complemento: '',
    bairro: '',
    cep: '',
    cidade: '',
    estado: ''
  }
})

onMounted(async () => {
  if (!isNew.value) {
    await carregarEmpresa()
  }
})

const carregarEmpresa = async () => {
  try {
    const res = await useApi<CommandResult<EmpresaApi>>('/cadastros/empresas/{id}', {
      params: { id: String(route.params.id) }
    })
    const dados: EmpresaApi | undefined = res?.dados ?? res?.data
    if (!dados) return
    apiOnline.value = true
    Object.assign(empresa, {
      id: dados.id ?? '',
      razaoSocial: dados.razaoSocial ?? '',
      nomeFantasia: dados.nomeFantasia ?? '',
      cnpj: dados.cnpj ?? '',
      inscricaoEstadual: dados.inscricaoEstadual ?? '',
      inscricaoMunicipal: dados.inscricaoMunicipal ?? '',
      inscricaoSuframa: dados.inscricaoSuframa ?? '',
      cnae: dados.cnae ?? '',
      regimeTributario: dados.regimeTributario ?? 1,
      regimeApuracao: dados.regimeApuracao ?? 1,
      tributarioGrupoId: dados.tributarioGrupoId ?? '',
      logo: dados.logo ?? ''
    })
    if (dados.endereco) {
      Object.assign(empresa.endereco, {
        logradouro: dados.endereco.logradouro ?? '',
        numero: dados.endereco.numero ?? '',
        complemento: dados.endereco.complemento ?? '',
        bairro: dados.endereco.bairro ?? '',
        cep: dados.endereco.cep ?? '',
        cidade: dados.endereco.cidade ?? '',
        estado: dados.endereco.estado ?? ''
      })
    }
  } catch (e) {
    apiOnline.value = false
  }
}

const montarComando = () => ({
  Id: empresa.id,
  RazaoSocial: empresa.razaoSocial,
  NomeFantasia: empresa.nomeFantasia || null,
  Cnpj: empresa.cnpj.replace(/\D/g, ''),
  InscricaoEstadual: empresa.inscricaoEstadual || null,
  InscricaoMunicipal: empresa.inscricaoMunicipal || null,
  InscricaoSuframa: empresa.inscricaoSuframa || null,
  Cnae: empresa.cnae || null,
  RegimeTributario: empresa.regimeTributario,
  RegimeApuracao: empresa.regimeApuracao,
  TributarioGrupoId: empresa.tributarioGrupoId || null,
  Logo: empresa.logo || null,
  Endereco: {
    Logradouro: empresa.endereco.logradouro,
    Numero: empresa.endereco.numero,
    Complemento: empresa.endereco.complemento || null,
    Bairro: empresa.endereco.bairro,
    Cep: empresa.endereco.cep.replace(/\D/g, ''),
    Cidade: empresa.endereco.cidade,
    Estado: empresa.endereco.estado
  }
})

const salvarEmpresa = async () => {
  saving.value = true
  try {
    if (isNew.value) {
      const body = montarComando()
      delete (body as Record<string, unknown>).Id
      const res = await useApi<CommandResult<CriarEmpresaResposta>>('/cadastros/empresas', { method: 'POST', body })
      if (res?.sucesso === false) {
        alert(`Falha ao criar empresa: ${res.mensagem ?? 'erro desconhecido'}`)
        return
      }
      const novoId = res?.dados?.empresaId ?? res?.dados?.EmpresaId
      if (novoId) {
        router.push(`/plataforma/admin/empresas/${novoId}`)
      } else {
        router.push('/plataforma/admin/empresas')
      }
    } else {
      const res = await useApi<CommandResult>('/cadastros/empresas', { method: 'PUT', body: montarComando() })
      if (res?.sucesso === false) {
        alert(`Erro ao salvar: ${res.mensagem ?? 'erro desconhecido'}`)
        return
      }
      alert('Empresa atualizada com sucesso!')
      router.push('/plataforma/admin/empresas')
    }
  } catch (e) {
    alert(`Erro na requisição: ${msgErro(e)}`)
  } finally {
    saving.value = false
  }
}

const alterarLogo = async () => {
  salvandoLogo.value = true
  try {
    const res = await useApi<CommandResult>('/cadastros/empresas/alterar-logo', {
      method: 'PUT',
      body: { EmpresaId: empresa.id, Logo: empresa.logo || null }
    })
    if (res?.sucesso === false) {
      alert(`Não foi possível atualizar a logo: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    alert('Logo atualizada com sucesso!')
  } catch (e) {
    alert(`Erro ao atualizar logo: ${msgErro(e)}`)
  } finally {
    salvandoLogo.value = false
  }
}

const testarEmail = async () => {
  testandoEmail.value = true
  try {
    const res = await useApi<CommandResult>('/cadastros/empresas/{id}/testar-email', {
      method: 'POST',
      params: { id: empresa.id }
    })
    if (res?.sucesso === false) {
      alert(`Falha no teste de e-mail: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    alert('Teste de e-mail disparado com sucesso!')
  } catch (e) {
    alert(`Erro ao testar e-mail: ${msgErro(e)}`)
  } finally {
    testandoEmail.value = false
  }
}

const buscarCep = async () => {
  const clean = empresa.endereco.cep.replace(/\D/g, '')
  if (clean.length !== 8) return
  try {
    // Serviço externo de CEP (não é a nossa API): usa fetch nativo do navegador.
    const resp = await fetch(`https://viacep.com.br/ws/${clean}/json/`)
    const res = await resp.json()
    if (!res.erro) {
      empresa.endereco.logradouro = res.logradouro || empresa.endereco.logradouro
      empresa.endereco.bairro = res.bairro || empresa.endereco.bairro
      empresa.endereco.cidade = res.localidade || empresa.endereco.cidade
      empresa.endereco.estado = res.uf || empresa.endereco.estado
    }
  } catch {
    /* silencioso: preenchimento manual permanece disponível */
  }
}
</script>

<style scoped>
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
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
}
.btn-back:hover {
  background: rgba(255, 255, 255, 0.06);
  color: var(--text-primary);
}
.form-focused-layout {
  max-width: 900px;
  margin: 0 auto;
}
.form-section-title {
  margin: 20px 0 8px;
  font-size: 13px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--text-secondary);
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 6px;
}
.form-row {
  display: flex;
  gap: 16px;
  margin-bottom: 12px;
}
.col-6 { flex: 0 0 calc(50% - 8px); }
.col-3 { flex: 0 0 calc(25% - 12px); }
.col-9 { flex: 0 0 calc(75% - 4px); }

@media (max-width: 600px) {
  .form-row { flex-direction: column; gap: 12px; }
  .col-6, .col-3, .col-9 { flex: 0 0 100%; }
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
