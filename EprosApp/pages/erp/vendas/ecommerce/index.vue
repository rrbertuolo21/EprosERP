<script setup lang="ts">
/**
 * Comércio Eletrônico — configuração da loja, clientes e cupons.
 * Contrato real: base `/vendas/ecommerce` (comandos, sem GET de listagem):
 *   POST configuracao (SalvarEcoConfiguracaoLojaCommand)
 *   POST clientes     (CriarEcoClienteCommand)
 *   POST cupons       (CriarEcoCupomCommand)
 * Pedidos/pagamentos/rastreio têm fluxo próprio (exigem itens) — ver relatório.
 * Apresentação — sem regra nova.
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

const TIPO_CUPOM = [
  { value: 0, label: 'Percentual' },
  { value: 1, label: 'Fixo' }
]

const toast = useToast()

// Configuração da loja
const cfg = reactive({
  nome: '', cidade: '', uf: '', cep: '', telefone: '', email: '', freteGratisValor: 0 as number | null, tokenLoja: ''
})
const salvandoCfg = ref(false)
async function salvarConfig() {
  salvandoCfg.value = true
  try {
    await useApi('/vendas/ecommerce/configuracao', {
      method: 'POST',
      body: {
        nome: cfg.nome || null,
        cidade: cfg.cidade || null,
        uf: cfg.uf || null,
        cep: cfg.cep || null,
        telefone: cfg.telefone || null,
        email: cfg.email || null,
        freteGratisValor: cfg.freteGratisValor ?? 0,
        tokenLoja: cfg.tokenLoja || null
      }
    })
    toast.success('Configuração da loja salva.')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoCfg.value = false
  }
}

// Novo cliente da loja
const cli = reactive({ nome: '', sobrenome: '', cpf: '', email: '', telefone: '', senha: '' })
const salvandoCli = ref(false)
async function salvarCliente() {
  if (!cli.nome || !cli.email || !cli.senha) {
    toast.warning('Nome, e-mail e senha são obrigatórios.')
    return
  }
  salvandoCli.value = true
  try {
    await useApi('/vendas/ecommerce/clientes', {
      method: 'POST',
      body: {
        nome: cli.nome,
        sobrenome: cli.sobrenome || null,
        cpf: cli.cpf || null,
        email: cli.email,
        telefone: cli.telefone || null,
        senhaHash: cli.senha
      }
    })
    toast.success('Cliente criado.')
    cli.nome = ''; cli.sobrenome = ''; cli.cpf = ''; cli.email = ''; cli.telefone = ''; cli.senha = ''
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoCli.value = false
  }
}

// Novo cupom
const cup = reactive({ codigo: '', valor: 0 as number | null, tipo: 0 as number })
const salvandoCup = ref(false)
async function salvarCupom() {
  if (!cup.codigo || !cup.valor) {
    toast.warning('Informe código e valor do cupom.')
    return
  }
  salvandoCup.value = true
  try {
    await useApi('/vendas/ecommerce/cupons', {
      method: 'POST',
      body: { codigo: cup.codigo, valor: cup.valor, tipo: cup.tipo }
    })
    toast.success('Cupom criado.')
    cup.codigo = ''; cup.valor = 0
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoCup.value = false
  }
}
</script>

<template>
  <div>
    <PageToolbar title="Comércio eletrônico" subtitle="Loja virtual — configuração, clientes e cupons" />

    <div class="glass-panel form-panel">
      <h3 class="secao-titulo">Configuração da loja</h3>
      <div class="form-grid">
        <TextField v-model="cfg.nome" label="Nome da loja" />
        <TextField v-model="cfg.email" label="E-mail" type="email" />
        <TextField v-model="cfg.telefone" label="Telefone" />
        <TextField v-model="cfg.cidade" label="Cidade" />
        <TextField v-model="cfg.uf" label="UF" maxlength="2" />
        <TextField v-model="cfg.cep" label="CEP" />
        <MoneyInput v-model="cfg.freteGratisValor" label="Frete grátis a partir de" />
        <TextField v-model="cfg.tokenLoja" label="Token da loja" />
      </div>
      <div class="acoes">
        <button type="button" class="btn btn-primary" :disabled="salvandoCfg" @click="salvarConfig">
          <span v-if="salvandoCfg" class="spinner"></span><span v-else>Salvar configuração</span>
        </button>
      </div>
    </div>

    <div class="glass-panel form-panel">
      <h3 class="secao-titulo">Novo cliente da loja</h3>
      <div class="form-grid">
        <TextField v-model="cli.nome" label="Nome" required />
        <TextField v-model="cli.sobrenome" label="Sobrenome" />
        <TextField v-model="cli.cpf" label="CPF" />
        <TextField v-model="cli.email" label="E-mail" type="email" required />
        <TextField v-model="cli.telefone" label="Telefone" />
        <TextField v-model="cli.senha" label="Senha" type="password" required />
      </div>
      <div class="acoes">
        <button type="button" class="btn btn-secondary" :disabled="salvandoCli" @click="salvarCliente">
          <span v-if="salvandoCli" class="spinner"></span><span v-else>Cadastrar cliente</span>
        </button>
      </div>
    </div>

    <div class="glass-panel form-panel">
      <h3 class="secao-titulo">Novo cupom</h3>
      <div class="form-grid">
        <TextField v-model="cup.codigo" label="Código" required />
        <MoneyInput v-model="cup.valor" label="Valor" />
        <SelectField v-model="cup.tipo" label="Tipo" :options="TIPO_CUPOM" :clearable="false" />
      </div>
      <div class="acoes">
        <button type="button" class="btn btn-secondary" :disabled="salvandoCup" @click="salvarCupom">
          <span v-if="salvandoCup" class="spinner"></span><span v-else>Criar cupom</span>
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
