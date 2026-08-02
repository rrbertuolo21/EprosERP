<script setup lang="ts">
/**
 * CRM Comercial — Pix relacional (configuração + cobranças).
 * Contrato real: base `/vendas/crm`:
 *   PUT  pix/configuracao            (SalvarCrmConfiguracaoPixRelacionalCommand)
 *   POST pix/pagamentos              (CriarCrmPagamentoPixRelacionalCommand)
 *   POST pix/pagamentos/{id}/status  (AtualizarStatusCrmPagamentoPixCommand)
 * Apresentação — sem regra nova. Não há GET; a tela é operacional.
 */
import { reactive, ref } from 'vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'

definePageMeta({ layout: 'default', middleware: 'auth' })

const STATUS_PIX = [
  { value: 0, label: 'Pendente' },
  { value: 1, label: 'Aprovado' },
  { value: 2, label: 'Rejeitado' },
  { value: 3, label: 'Outro' }
]

const toast = useToast()

// Configuração
const cfg = reactive({ linkApiAppVendas: '', tokenPix: '', cnpjEmpresa: '', ativo: true })
const salvandoCfg = ref(false)
async function salvarConfig() {
  salvandoCfg.value = true
  try {
    await useApi('/vendas/crm/pix/configuracao', {
      method: 'PUT',
      body: {
        linkApiAppVendas: cfg.linkApiAppVendas || null,
        tokenPix: cfg.tokenPix || null,
        cnpjEmpresa: cfg.cnpjEmpresa || null,
        ativo: cfg.ativo
      }
    })
    toast.success('Configuração Pix salva.')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoCfg.value = false
  }
}

// Nova cobrança
const pag = reactive({ valor: 0 as number | null, entidadeOrigemTipo: '', entidadeOrigemId: '', qrCode: '' })
const salvandoPag = ref(false)
async function criarCobranca() {
  if (!pag.valor || pag.valor <= 0) {
    toast.warning('Informe o valor da cobrança.')
    return
  }
  salvandoPag.value = true
  try {
    await useApi('/vendas/crm/pix/pagamentos', {
      method: 'POST',
      body: {
        valor: pag.valor,
        entidadeOrigemTipo: pag.entidadeOrigemTipo || null,
        entidadeOrigemId: pag.entidadeOrigemId || null,
        qrCode: pag.qrCode || null
      }
    })
    toast.success('Cobrança Pix criada.')
    pag.valor = 0
    pag.qrCode = ''
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoPag.value = false
  }
}

// Atualizar status
const st = reactive({ id: '', status: 1 as number })
const salvandoSt = ref(false)
async function atualizarStatus() {
  if (!st.id) {
    toast.warning('Informe o id da cobrança.')
    return
  }
  salvandoSt.value = true
  try {
    await useApi('/vendas/crm/pix/pagamentos/{id}/status', {
      method: 'POST',
      params: { id: st.id },
      body: { id: st.id, status: st.status }
    })
    toast.success('Status atualizado.')
    st.id = ''
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoSt.value = false
  }
}
</script>

<template>
  <div>
    <PageToolbar title="Pix relacional" subtitle="CRM Comercial — configuração e cobranças Pix" />

    <div class="glass-panel form-panel">
      <h3 class="secao-titulo">Configuração</h3>
      <div class="form-grid">
        <TextField v-model="cfg.linkApiAppVendas" label="Link da API (app vendas)" />
        <TextField v-model="cfg.tokenPix" label="Token Pix" />
        <TextField v-model="cfg.cnpjEmpresa" label="CNPJ da empresa" />
        <label class="field toggle-row">
          <span class="field-label">Ativo</span>
          <input v-model="cfg.ativo" type="checkbox" />
        </label>
      </div>
      <div class="acoes">
        <button type="button" class="btn btn-primary" :disabled="salvandoCfg" @click="salvarConfig">
          <span v-if="salvandoCfg" class="spinner"></span><span v-else>Salvar configuração</span>
        </button>
      </div>
    </div>

    <div class="glass-panel form-panel">
      <h3 class="secao-titulo">Nova cobrança</h3>
      <div class="form-grid">
        <MoneyInput v-model="pag.valor" label="Valor" />
        <TextField v-model="pag.entidadeOrigemTipo" label="Tipo de origem" hint="Ex.: Lead, Oportunidade." />
        <TextField v-model="pag.entidadeOrigemId" label="ID de origem" />
        <TextField v-model="pag.qrCode" label="QR Code (copia e cola)" />
      </div>
      <div class="acoes">
        <button type="button" class="btn btn-secondary" :disabled="salvandoPag" @click="criarCobranca">
          <span v-if="salvandoPag" class="spinner"></span><span v-else>Gerar cobrança</span>
        </button>
      </div>
    </div>

    <div class="glass-panel form-panel">
      <h3 class="secao-titulo">Atualizar status de cobrança</h3>
      <div class="form-grid">
        <TextField v-model="st.id" label="ID da cobrança" />
        <SelectField v-model="st.status" label="Status" :options="STATUS_PIX" :clearable="false" />
      </div>
      <div class="acoes">
        <button type="button" class="btn btn-secondary" :disabled="salvandoSt" @click="atualizarStatus">
          <span v-if="salvandoSt" class="spinner"></span><span v-else>Atualizar</span>
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.secao-titulo { font-size: 15px; margin-bottom: 14px; }
.acoes { display: flex; justify-content: flex-end; margin-top: 16px; }
.toggle-row { display: flex; align-items: center; gap: 10px; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
</style>
