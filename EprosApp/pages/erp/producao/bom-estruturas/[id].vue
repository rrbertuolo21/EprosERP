<script setup lang="ts">
/**
 * Estrutura de Produto (BOM) — criação (novo) e detalhe/workflow (id existente).
 * POST /producao/bom-estruturas + workflow (submeter/aprovar/rejeitar/inativar/reativar/encerrar).
 * Sem PUT.
 * Campos com palpite: produtoId / variacaoId / subUnidadeId são uuid sem endpoint de listagem
 * próprio no módulo → TextField (uuid). `tipoCustoProducao` é string livre (enum não exposto).
 * Coleção `componentes` não é editável aqui (ver relatório).
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import WorkflowActions from '~/components/producao-shared/WorkflowActions.vue'
import { rotuloStatusWorkflow, classeBadgeStatus, formatarData, formatarMoeda, formatarQuantidade } from '~/components/producao-shared/producao'

definePageMeta({ layout: 'default' })

interface BomForm {
  produtoId: string
  variacaoId: string
  codigo: string | null
  percentualDesperdicio: number | null
  custoIngredientes: number | null
  custoExtra: number | null
  quantidadeTotal: number | null
  ingredientesJson: string | null
  instrucoes: string | null
  tipoCustoProducao: string | null
  precoFinal: number | null
  subUnidadeId: string | null
  versao: string | null
  inicioVigencia: string | null
  fimVigencia: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const acaoEmAndamento = ref(false)
const registro = ref<Record<string, unknown> | null>(null)

const form = reactive<BomForm>({
  produtoId: '',
  variacaoId: '',
  codigo: null,
  percentualDesperdicio: 0,
  custoIngredientes: 0,
  custoExtra: 0,
  quantidadeTotal: 0,
  ingredientesJson: null,
  instrucoes: null,
  tipoCustoProducao: null,
  precoFinal: null,
  subUnidadeId: null,
  versao: null,
  inicioVigencia: null,
  fimVigencia: null
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.produtoId) erros.produtoId = 'Produto é obrigatório.'
  if (!form.variacaoId) erros.variacaoId = 'Variação é obrigatória.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/producao/bom-estruturas/${idParam}`)
    registro.value = extrairDados<Record<string, unknown>>(resposta) ?? null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function salvar() {
  if (!validar()) { toast.error('Formulário possui erros de validação.'); return }
  salvando.value = true
  try {
    await useApi('/producao/bom-estruturas', { method: 'POST', body: form })
    toast.success('Estrutura criada com sucesso!')
    router.push('/erp/producao/bom-estruturas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function executarAcao(chave: string, motivo?: string) {
  acaoEmAndamento.value = true
  try {
    await useApi(`/producao/bom-estruturas/${idParam}/${chave}`, {
      method: 'POST',
      body: chave === 'rejeitar' ? (motivo ?? '') : undefined
    })
    toast.success('Ação executada com sucesso.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmAndamento.value = false
  }
}

function cancelar() { router.push('/erp/producao/bom-estruturas') }
const statusAtual = computed(() => registro.value?.status as number | string | undefined)
onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Estrutura de Produto (BOM)' : 'Nova estrutura'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <WorkflowActions v-if="isEdit" :status="statusAtual" :loading="acaoEmAndamento" @acao="executarAcao" />
        <button v-else type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div v-if="isEdit">
        <div v-if="registro" class="detail-grid">
          <div class="detail-item"><span class="detail-label">Código</span><span>{{ registro.codigo || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Status</span><span class="badge" :class="classeBadgeStatus(rotuloStatusWorkflow(statusAtual))">{{ rotuloStatusWorkflow(statusAtual) }}</span></div>
          <div class="detail-item"><span class="detail-label">Versão</span><span>{{ registro.versao || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Quantidade Total</span><span>{{ formatarQuantidade(registro.quantidadeTotal as number) }}</span></div>
          <div class="detail-item"><span class="detail-label">Custo Ingredientes</span><span>{{ formatarMoeda(registro.custoIngredientes as number) }}</span></div>
          <div class="detail-item"><span class="detail-label">Custo Extra</span><span>{{ formatarMoeda(registro.custoExtra as number) }}</span></div>
          <div class="detail-item"><span class="detail-label">Preço Final</span><span>{{ formatarMoeda(registro.precoFinal as number) }}</span></div>
          <div class="detail-item"><span class="detail-label">Produto (ID)</span><span>{{ registro.produtoId || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Variação (ID)</span><span>{{ registro.variacaoId || '—' }}</span></div>
          <div class="detail-item"><span class="detail-label">Início Vigência</span><span>{{ formatarData(registro.inicioVigencia as string) }}</span></div>
          <div class="detail-item"><span class="detail-label">Fim Vigência</span><span>{{ formatarData(registro.fimVigencia as string) }}</span></div>
          <div class="detail-item"><span class="detail-label">Criado em</span><span>{{ formatarData(registro.criadoEm as string, true) }}</span></div>
        </div>
        <p v-else-if="!carregando" class="empty-detail">Registro não encontrado.</p>
      </div>

      <form v-else class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.produtoId" label="Produto (ID)" required :error="erros.produtoId" hint="UUID do produto acabado" />
          <TextField v-model="form.variacaoId" label="Variação (ID)" required :error="erros.variacaoId" hint="UUID da variação" />
          <TextField v-model="form.codigo" label="Código" maxlength="60" hint="Opcional" />
          <TextField v-model="form.versao" label="Versão" maxlength="20" />
          <PercentInput v-model="form.percentualDesperdicio" label="Percentual de desperdício" />
          <QuantityInput v-model="form.quantidadeTotal" label="Quantidade total" />
          <MoneyInput v-model="form.custoIngredientes" label="Custo dos ingredientes" />
          <MoneyInput v-model="form.custoExtra" label="Custo extra" />
          <MoneyInput v-model="form.precoFinal" label="Preço final" />
          <TextField v-model="form.tipoCustoProducao" label="Tipo de custo de produção" hint="Texto livre" />
          <TextField v-model="form.subUnidadeId" label="Sub-unidade (ID)" hint="UUID (opcional)" />
          <DateTimeField v-model="form.inicioVigencia" label="Início da vigência" />
          <DateTimeField v-model="form.fimVigencia" label="Fim da vigência" />
          <TextField v-model="form.instrucoes" label="Instruções" hint="Texto livre" />
          <TextField v-model="form.ingredientesJson" label="Ingredientes (JSON)" hint="Estrutura JSON dos ingredientes (opcional)" />
        </div>
        <p class="form-note">Os componentes detalhados da estrutura são gerenciados após a criação (sem endpoint de manutenção de itens exposto).</p>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.detail-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 16px; }
.detail-item { display: flex; flex-direction: column; gap: 4px; }
.detail-label { font-size: 12px; color: var(--text-secondary); font-weight: 600; }
.form-note { margin-top: 16px; font-size: 12.5px; color: var(--text-secondary); }
.empty-detail { color: var(--text-secondary); }
</style>
