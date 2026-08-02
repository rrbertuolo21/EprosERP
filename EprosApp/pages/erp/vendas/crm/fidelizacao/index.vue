<script setup lang="ts">
/**
 * CRM Comercial — Fidelização (clientes fidelizados + pontuação).
 * Contrato real: base `/vendas/crm` (somente comandos):
 *   POST fidelizacao/clientes    (CriarCrmClienteFidelizadoCommand)
 *   POST fidelizacao/pontuacoes  (RegistrarPontuacaoFidelizacaoCommand)
 * Apresentação — sem regra nova.
 */
import { reactive, ref } from 'vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'

definePageMeta({ layout: 'default', middleware: 'auth' })

const toast = useToast()

// Novo cliente fidelizado
const cliente = reactive({ nome: '', email: '', celular: '', dataAniversario: null as string | null })
const salvandoCliente = ref(false)
async function salvarCliente() {
  if (!cliente.nome) {
    toast.warning('Nome é obrigatório.')
    return
  }
  salvandoCliente.value = true
  try {
    await useApi('/vendas/crm/fidelizacao/clientes', {
      method: 'POST',
      body: {
        nome: cliente.nome,
        email: cliente.email || null,
        celular: cliente.celular || null,
        dataAniversario: cliente.dataAniversario
      }
    })
    toast.success('Cliente fidelizado criado.')
    cliente.nome = ''
    cliente.email = ''
    cliente.celular = ''
    cliente.dataAniversario = null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoCliente.value = false
  }
}

// Pontuação
const ponto = reactive({ clienteFidelizadoId: '', pontos: 0 as number | null, data: null as string | null, observacao: '' })
const salvandoPonto = ref(false)
async function salvarPontuacao() {
  if (!ponto.clienteFidelizadoId || !ponto.pontos) {
    toast.warning('Informe o cliente e a quantidade de pontos.')
    return
  }
  salvandoPonto.value = true
  try {
    await useApi('/vendas/crm/fidelizacao/pontuacoes', {
      method: 'POST',
      body: {
        clienteFidelizadoId: ponto.clienteFidelizadoId,
        pontos: ponto.pontos,
        data: ponto.data,
        observacao: ponto.observacao || null
      }
    })
    toast.success('Pontuação registrada.')
    ponto.pontos = 0
    ponto.observacao = ''
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoPonto.value = false
  }
}
</script>

<template>
  <div>
    <PageToolbar title="Fidelização" subtitle="CRM Comercial — clientes fidelizados e pontos" />

    <div class="glass-panel form-panel">
      <h3 class="secao-titulo">Novo cliente fidelizado</h3>
      <div class="form-grid">
        <TextField v-model="cliente.nome" label="Nome" required />
        <TextField v-model="cliente.email" label="E-mail" type="email" />
        <TextField v-model="cliente.celular" label="Celular" />
        <DateTimeField v-model="cliente.dataAniversario" label="Aniversário" mode="date" />
      </div>
      <div class="acoes">
        <button type="button" class="btn btn-primary" :disabled="salvandoCliente" @click="salvarCliente">
          <span v-if="salvandoCliente" class="spinner"></span><span v-else>Cadastrar</span>
        </button>
      </div>
    </div>

    <div class="glass-panel form-panel">
      <h3 class="secao-titulo">Registrar pontuação</h3>
      <div class="form-grid">
        <TextField v-model="ponto.clienteFidelizadoId" label="ID do cliente fidelizado" required />
        <QuantityInput v-model="ponto.pontos" label="Pontos" />
        <DateTimeField v-model="ponto.data" label="Data" mode="date" />
        <TextField v-model="ponto.observacao" label="Observação" />
      </div>
      <div class="acoes">
        <button type="button" class="btn btn-secondary" :disabled="salvandoPonto" @click="salvarPontuacao">
          <span v-if="salvandoPonto" class="spinner"></span><span v-else>Registrar pontos</span>
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
</style>
