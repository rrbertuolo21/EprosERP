<script setup lang="ts">
/**
 * Meios de pagamento salvos — Área do Cliente (portal SaaS) — 1.08B.
 *
 * Lista/adiciona/remove cartões-on-file e define o cartão padrão (débito automático).
 *
 * ⛔ PCI (obrigatório): o dado de cartão CRU (número/CVV) NUNCA passa por este app nem pelo backend.
 * O cartão é tokenizado NO NAVEGADOR pela lib do Mercado Pago (SDK MercadoPago.js / Bricks): o campo
 * de cartão é um iframe seguro do gateway e o resultado é apenas um TOKEN opaco. É esse token que enviamos
 * a `POST /aplicativo/assinaturas/meios-pagamento`. A criação do MercadoPago.js Brick é um ponto de
 * integração de ambiente (depende da public key do gateway) e está sinalizada abaixo como TODO.
 *
 * Endpoints:
 *   GET    /aplicativo/assinaturas/meios-pagamento
 *   POST   /aplicativo/assinaturas/meios-pagamento           { cardToken }
 *   DELETE /aplicativo/assinaturas/meios-pagamento/{id}
 *   POST   /aplicativo/assinaturas/meios-pagamento/{id}/padrao
 */
import { ref, onMounted } from 'vue'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'

definePageMeta({ layout: 'default' })

interface MeioPagamento {
  id: string
  tipo: string
  bandeira?: string | null
  ultimosQuatro?: string | null
  validadeMes?: number | null
  validadeAno?: number | null
  padrao: boolean
}

const toast = useToast()

const meios = ref<MeioPagamento[]>([])
const carregando = ref(false)
const salvando = ref(false)

const colunas: DataTableColumn<MeioPagamento>[] = [
  { key: 'bandeira', label: 'Bandeira', align: 'left' },
  { key: 'ultimosQuatro', label: 'Final', align: 'center', formatter: (v) => (v ? `•••• ${v}` : '—') },
  {
    key: 'validadeMes',
    label: 'Validade',
    align: 'center',
    formatter: (_v, row) => (row.validadeMes ? `${row.validadeMes}/${row.validadeAno}` : '—')
  },
  { key: 'padrao', label: 'Padrão', align: 'center', formatter: (v) => (v ? 'Sim' : '') }
]

async function carregar() {
  carregando.value = true
  try {
    const resposta = await useApi<{ dados?: MeioPagamento[]; data?: MeioPagamento[] }>(
      '/aplicativo/assinaturas/meios-pagamento'
    )
    meios.value = extrairLista<MeioPagamento>(resposta) ?? []
  } catch (e) {
    toast.error(obterMensagemErro(e))
    meios.value = []
  } finally {
    carregando.value = false
  }
}

/**
 * Tokeniza o cartão no navegador (MercadoPago.js) e envia SÓ o token ao backend.
 * TODO (integração de ambiente): instanciar o Brick de cartão do Mercado Pago com a public key do gateway
 * e obter `cardToken` a partir do callback `onSubmit` do Brick (o número/CVV ficam no iframe do MP).
 */
async function adicionarCartao(cardToken: string) {
  if (!cardToken) {
    toast.error('Token do cartão não gerado. Verifique os dados no formulário seguro do gateway.')
    return
  }
  salvando.value = true
  try {
    const resposta = await useApi<{ mensagem?: string }>(
      '/aplicativo/assinaturas/meios-pagamento',
      { method: 'POST', body: { cardToken } }
    )
    void resposta
    toast.success('Cartão salvo com sucesso.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function definirPadrao(meio: MeioPagamento) {
  try {
    await useApi('/aplicativo/assinaturas/meios-pagamento/{id}/padrao', {
      method: 'POST',
      params: { id: meio.id }
    })
    toast.success('Cartão padrão atualizado.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

async function remover(meio: MeioPagamento) {
  try {
    await useApi('/aplicativo/assinaturas/meios-pagamento/{id}', {
      method: 'DELETE',
      params: { id: meio.id }
    })
    toast.success('Cartão removido.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

onMounted(() => {
  void carregar()
})

// Exposto para o container do Brick do Mercado Pago (callback onSubmit → adicionarCartao(token)).
defineExpose({ adicionarCartao })
</script>

<template>
  <div class="meios-wrapper">
    <PageToolbar title="Meios de pagamento" subtitle="Cartões salvos para débito automático da assinatura" :loading="carregando" />

    <div class="alert-banner glass-panel">
      <div class="alert-text">
        <p>
          Seus dados de cartão são processados diretamente pelo gateway de pagamento (Mercado Pago) em um
          formulário seguro. O número e o CVV do cartão <strong>nunca</strong> são armazenados pela Epros —
          guardamos apenas a bandeira, os últimos 4 dígitos e a validade.
        </p>
      </div>
    </div>

    <!-- Ponto de montagem do Brick de cartão do Mercado Pago (integração de ambiente). -->
    <div id="mp-card-brick-container"></div>

    <DataTable
      :items="meios"
      :columns="colunas"
      :total="meios.length"
      :loading="carregando"
      empty-text="Nenhum cartão salvo."
    >
      <template #actions="{ row }">
        <button v-if="!row.padrao" type="button" class="btn btn-secondary btn-sm" @click.stop="definirPadrao(row)">
          Tornar padrão
        </button>
        <button type="button" class="btn btn-danger btn-sm" @click.stop="remover(row)">
          Remover
        </button>
      </template>
    </DataTable>
  </div>
</template>

<style scoped>
.meios-wrapper {
  width: 100%;
  max-width: 960px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 24px 20px;
}
.alert-banner {
  padding: 20px 24px;
  border-left: 4px solid var(--primary);
}
.alert-text p { font-size: 13px; line-height: 1.6; color: var(--text-secondary); }
.alert-text strong { color: var(--text-primary); }
#mp-card-brick-container { min-height: 0; }
</style>
