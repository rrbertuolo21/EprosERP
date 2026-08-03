<script setup lang="ts">
/**
 * Conta Emissora — novo/edição — Serviços Financeiros.
 * POST /servicos-financeiros/contas-emissoras · PUT /{id}. Sem GET/{id}: edição via listagem.
 * `nossoNumeroAtual` só é enviado na criação (não faz parte do PUT).
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'
import { carregarOpcoesDe } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface ContaEmissoraForm {
  id?: string
  bancoId: string | null
  configuracaoCedenteId: string | null
  nomeBanco: string | null
  carteira: string | null
  agencia: string | null
  digitoAgencia: string | null
  conta: string | null
  digitoConta: string | null
  especie: string | null
  nossoNumeroAtual: number | null
  tipoCobranca: string | null
  convenio: string | null
  contrato: string | null
  tipoCarteira: string | null
  incrementoNossoNumero: number | null
  tipoRemessa: string | null
  codigoCliente: string | null
  posto: string | null
  ativa: boolean
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const opcoesBanco = ref<SelectOption[]>([])
const form = reactive<ContaEmissoraForm>({
  bancoId: null,
  configuracaoCedenteId: null,
  nomeBanco: null,
  carteira: null,
  agencia: null,
  digitoAgencia: null,
  conta: null,
  digitoConta: null,
  especie: null,
  nossoNumeroAtual: 0,
  tipoCobranca: null,
  convenio: null,
  contrato: null,
  tipoCarteira: null,
  incrementoNossoNumero: 1,
  tipoRemessa: null,
  codigoCliente: null,
  posto: null,
  ativa: true
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.bancoId) erros.bancoId = 'Banco é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi('/servicos-financeiros/contas-emissoras', { query: { pagina: 1, tamanhoPagina: 500 } })
    const bruto = extrairDados<unknown>(resposta)
    const itens = (Array.isArray(bruto) ? bruto : (bruto as { itens?: ContaEmissoraForm[] })?.itens) ?? []
    const encontrada = (itens as ContaEmissoraForm[]).find((c) => String(c.id) === idParam)
    if (encontrada) Object.assign(form, encontrada)
    else toast.error('Conta emissora não encontrada.')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    if (isEdit.value) {
      const { nossoNumeroAtual: _omit, ...corpo } = form
      void _omit
      await useApi('/servicos-financeiros/contas-emissoras/{id}', { method: 'PUT', params: { id: idParam }, body: { id: idParam, ...corpo } })
    } else {
      await useApi('/servicos-financeiros/contas-emissoras', { method: 'POST', body: { ...form } })
    }
    toast.success('Registro salvo com sucesso!')
    router.push('/erp/financeiro/servicos-financeiros/contas-emissoras')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/servicos-financeiros/contas-emissoras')
}

onMounted(async () => {
  opcoesBanco.value = await carregarOpcoesDe('/bancos', ['nome', 'descricao', 'codigo'])
  await carregar()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar conta emissora' : 'Nova conta emissora'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <SelectField v-model="form.bancoId" label="Banco" required :options="opcoesBanco" :error="erros.bancoId" />
          <TextField v-model="form.nomeBanco" label="Nome do banco" maxlength="60" />
          <TextField v-model="form.agencia" label="Agência" maxlength="10" />
          <TextField v-model="form.digitoAgencia" label="Dígito da agência" maxlength="2" />
          <TextField v-model="form.conta" label="Conta" maxlength="15" />
          <TextField v-model="form.digitoConta" label="Dígito da conta" maxlength="2" />
          <TextField v-model="form.carteira" label="Carteira" maxlength="10" />
          <TextField v-model="form.tipoCarteira" label="Tipo de carteira" maxlength="20" />
          <TextField v-model="form.especie" label="Espécie" maxlength="10" />
          <TextField v-model="form.tipoCobranca" label="Tipo de cobrança" maxlength="20" />
          <TextField v-model="form.convenio" label="Convênio" maxlength="30" />
          <TextField v-model="form.contrato" label="Contrato" maxlength="30" />
          <TextField v-model="form.tipoRemessa" label="Tipo de remessa" maxlength="20" />
          <TextField v-model="form.codigoCliente" label="Código do cliente" maxlength="30" />
          <TextField v-model="form.posto" label="Posto" maxlength="10" />
          <TextField
            v-if="!isEdit"
            :model-value="form.nossoNumeroAtual"
            label="Nosso número atual"
            type="number"
            @update:model-value="(v) => (form.nossoNumeroAtual = v === '' ? null : Number(v))"
          />
          <TextField
            :model-value="form.incrementoNossoNumero"
            label="Incremento do nosso número"
            type="number"
            @update:model-value="(v) => (form.incrementoNossoNumero = v === '' ? null : Number(v))"
          />
          <!-- TODO: configuracaoCedenteId é UUID sem endpoint de listagem no digest (GET de cedentes não existe). -->
          <TextField v-model="form.configuracaoCedenteId" label="ID da config. de cedente" hint="UUID (opcional)" />
          <label class="field toggle-row">
            <span class="field-label">{{ form.ativa ? 'Ativa' : 'Inativa' }}</span>
            <input v-model="form.ativa" type="checkbox" />
          </label>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.toggle-row { display: flex; align-items: center; gap: 10px; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
</style>
