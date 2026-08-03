<script setup lang="ts">
/**
 * Configurações DF-e / Certificado — parâmetros de emissão fiscal da empresa ativa.
 *
 * Porta o comportamento de `configuracoes/certificado.vue` do legado (cadastro de
 * certificado digital) adaptado aos endpoints reais disponíveis no backend novo:
 *   - `fiscal/configuracoes-dfe` (CRUD de configuração DF-e, filtrável por empresa)
 *   - `cadastros/empresas/{empresaId}/parametros-dfe` (série/número/CSC de NF-e e NFC-e)
 *
 * O backend novo NÃO possui endpoint de upload/gestão de arquivo de certificado digital
 * (`certificados-digitais/*` do legado não tem equivalente em api/v1) — ver aviso no rodapé
 * da tela e o resumo retornado ao integrador.
 */
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useApi, extrairDados, type CommandResult } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useTenant } from '~/composables/useTenant'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import EmpresaSelector from '~/components/shared/EmpresaSelector.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'

definePageMeta({
  layout: 'default',
  middleware: 'auth'
})

interface ConfiguracaoDFe {
  id?: string
  empresaId?: string
  ambiente?: number | string
  serieNfe?: string
  numeroInicialNfe?: number
  serieNfce?: string
  numeroInicialNfce?: number
  cscId?: string
  cscToken?: string
  observacoes?: string
  [key: string]: unknown
}

interface ParametrosDFe {
  empresaId?: string
  serieNfe?: string
  proximoNumeroNfe?: number
  serieNfce?: string
  proximoNumeroNfce?: number
  cscIdNfce?: string
  cscTokenNfce?: string
  [key: string]: unknown
}

const toast = useToast()
const { empresaAtiva, empresaId } = useTenant()

const carregando = ref(false)
const salvando = ref(false)

const opcoesAmbiente = [
  { label: 'Produção', value: 1 },
  { label: 'Homologação', value: 2 }
]

const configuracao = reactive<ConfiguracaoDFe>({
  ambiente: 2,
  serieNfe: '',
  numeroInicialNfe: 1,
  serieNfce: '',
  numeroInicialNfce: 1,
  cscId: '',
  cscToken: '',
  observacoes: ''
})

const parametros = reactive<ParametrosDFe>({
  serieNfe: '',
  proximoNumeroNfe: 1,
  serieNfce: '',
  proximoNumeroNfce: 1,
  cscIdNfce: '',
  cscTokenNfce: ''
})

const existeConfiguracao = ref(false)
const existeParametros = ref(false)
const erro = ref<string | null>(null)

/** Busca a configuração DF-e vinculada à empresa ativa (endpoint filtra por empresaId na query). */
async function carregarConfiguracaoDFe(): Promise<void> {
  if (!empresaId.value) return
  try {
    const resposta = await useApi<CommandResult<ConfiguracaoDFe> | ConfiguracaoDFe>(
      '/fiscal/configuracoes-dfe/empresa/{empresaId}',
      { params: { empresaId: empresaId.value } }
    )
    const dados = extrairDados<ConfiguracaoDFe>(resposta)
    if (dados) {
      Object.assign(configuracao, dados)
      existeConfiguracao.value = true
    } else {
      existeConfiguracao.value = false
    }
  } catch (e) {
    // 404 é esperado quando a empresa ainda não tem configuração cadastrada.
    existeConfiguracao.value = false
  }
}

/** Busca os parâmetros DF-e (série/número/CSC) da empresa ativa. */
async function carregarParametrosDFe(): Promise<void> {
  if (!empresaId.value) return
  try {
    const resposta = await useApi<CommandResult<ParametrosDFe> | ParametrosDFe>(
      '/cadastros/empresas/{empresaId}/parametros-dfe',
      { params: { empresaId: empresaId.value } }
    )
    const dados = extrairDados<ParametrosDFe>(resposta)
    if (dados) {
      Object.assign(parametros, dados)
      existeParametros.value = true
    } else {
      existeParametros.value = false
    }
  } catch (e) {
    existeParametros.value = false
  }
}

async function carregarTudo(): Promise<void> {
  if (!empresaId.value) return
  carregando.value = true
  erro.value = null
  try {
    await Promise.all([carregarConfiguracaoDFe(), carregarParametrosDFe()])
  } catch (e) {
    erro.value = obterMensagemErro(e)
  } finally {
    carregando.value = false
  }
}

/** Salva a configuração DF-e (cria se ainda não existir, atualiza caso contrário). */
async function salvarConfiguracaoDFe(): Promise<void> {
  if (!empresaId.value) {
    toast.error('Selecione uma empresa antes de salvar.')
    return
  }
  salvando.value = true
  try {
    const payload = { ...configuracao, empresaId: empresaId.value }
    if (existeConfiguracao.value && configuracao.id) {
      await useApi(`/fiscal/configuracoes-dfe/{id}`, {
        method: 'PUT',
        params: { id: configuracao.id },
        body: { ...payload, id: configuracao.id }
      })
    } else {
      const resposta = await useApi<CommandResult<ConfiguracaoDFe>>('/fiscal/configuracoes-dfe', {
        method: 'POST',
        body: payload
      })
      const criado = extrairDados<ConfiguracaoDFe>(resposta)
      if (criado?.id) configuracao.id = criado.id
      existeConfiguracao.value = true
    }
    toast.success('Configuração DF-e salva com sucesso.')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

/** Salva os parâmetros DF-e (série/número/CSC) — cria ou atualiza conforme já exista registro. */
async function salvarParametrosDFe(): Promise<void> {
  if (!empresaId.value) {
    toast.error('Selecione uma empresa antes de salvar.')
    return
  }
  salvando.value = true
  try {
    const payload = { ...parametros, empresaId: empresaId.value }
    if (existeParametros.value) {
      await useApi('/cadastros/empresas/{empresaId}/parametros-dfe', {
        method: 'PUT',
        params: { empresaId: empresaId.value },
        body: payload
      })
    } else {
      await useApi('/cadastros/empresas/{empresaId}/parametros-dfe', {
        method: 'POST',
        params: { empresaId: empresaId.value },
        body: payload
      })
      existeParametros.value = true
    }
    toast.success('Parâmetros DF-e salvos com sucesso.')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

const tituloEmpresa = computed(() => empresaAtiva.value?.nome ?? 'nenhuma empresa selecionada')

onMounted(() => {
  void carregarTudo()
})

watch(empresaId, () => {
  void carregarTudo()
})
</script>

<template>
  <div>
    <PageToolbar title="Configurações DF-e" :subtitle="`Empresa: ${tituloEmpresa}`" :loading="carregando">
      <template #actions>
        <EmpresaSelector />
      </template>
    </PageToolbar>

    <div v-if="!empresaId" class="glass-panel empty-state">
      Selecione uma empresa para configurar a emissão de documentos fiscais.
    </div>

    <template v-else>
      <section class="glass-panel section">
        <h3 class="section-title">Configuração DF-e</h3>
        <p class="section-hint">
          Parâmetros gerais de emissão (ambiente, séries, CSC) usados pela API fiscal ao transmitir NF-e/NFC-e.
        </p>
        <div class="form-grid">
          <div class="col-3">
            <SelectField v-model="configuracao.ambiente" label="Ambiente" :options="opcoesAmbiente" />
          </div>
          <div class="col-3">
            <TextField v-model="configuracao.serieNfe" label="Série NF-e" />
          </div>
          <div class="col-3">
            <TextField v-model.number="configuracao.numeroInicialNfe" type="number" label="Número inicial NF-e" />
          </div>
          <div class="col-3">
            <TextField v-model="configuracao.serieNfce" label="Série NFC-e" />
          </div>
          <div class="col-3">
            <TextField v-model.number="configuracao.numeroInicialNfce" type="number" label="Número inicial NFC-e" />
          </div>
          <div class="col-3">
            <TextField v-model="configuracao.cscId" label="CSC ID (NFC-e)" />
          </div>
          <div class="col-6">
            <TextField v-model="configuracao.cscToken" type="password" label="CSC Token (NFC-e)" />
          </div>
          <div class="col-12">
            <TextField v-model="configuracao.observacoes" label="Observações" />
          </div>
        </div>
        <div class="section-actions">
          <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvarConfiguracaoDFe">
            <span v-if="salvando" class="spinner"></span>
            <span v-else>Salvar configuração</span>
          </button>
        </div>
      </section>

      <section class="glass-panel section">
        <h3 class="section-title">Parâmetros DF-e da empresa</h3>
        <p class="section-hint">
          Numeração corrente (série/próximo número) e CSC específico da empresa, usados na emissão de NFC-e.
        </p>
        <div class="form-grid">
          <div class="col-3">
            <TextField v-model="parametros.serieNfe" label="Série NF-e" />
          </div>
          <div class="col-3">
            <TextField v-model.number="parametros.proximoNumeroNfe" type="number" label="Próximo número NF-e" />
          </div>
          <div class="col-3">
            <TextField v-model="parametros.serieNfce" label="Série NFC-e" />
          </div>
          <div class="col-3">
            <TextField v-model.number="parametros.proximoNumeroNfce" type="number" label="Próximo número NFC-e" />
          </div>
          <div class="col-6">
            <TextField v-model="parametros.cscIdNfce" label="CSC ID (NFC-e)" />
          </div>
          <div class="col-6">
            <TextField v-model="parametros.cscTokenNfce" type="password" label="CSC Token (NFC-e)" />
          </div>
        </div>
        <div class="section-actions">
          <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvarParametrosDFe">
            <span v-if="salvando" class="spinner"></span>
            <span v-else>Salvar parâmetros</span>
          </button>
        </div>
      </section>

      <div class="glass-panel section aviso">
        <strong>Certificado digital (A1/A3):</strong> o backend atual não expõe endpoint de upload/gestão de
        arquivo de certificado (equivalente a <code>certificados-digitais/*</code> no legado). Esta tela cobre
        apenas os parâmetros de emissão (<code>fiscal/configuracoes-dfe</code> e
        <code>cadastros/empresas/{empresaId}/parametros-dfe</code>). Ver observação ao integrador.
      </div>
    </template>
  </div>
</template>

<style scoped>
.section { margin-top: 16px; padding: 20px; }
.section-title { margin: 0 0 4px; font-size: 15px; }
.section-hint { margin: 0 0 16px; font-size: 13px; color: var(--text-secondary); }
.section-actions { margin-top: 16px; display: flex; justify-content: flex-end; }
.empty-state { padding: 32px; text-align: center; color: var(--text-secondary); }
.aviso { font-size: 13px; color: var(--text-secondary); line-height: 1.6; }
.aviso code { background: rgba(255, 255, 255, 0.06); padding: 1px 5px; border-radius: 4px; }
</style>
