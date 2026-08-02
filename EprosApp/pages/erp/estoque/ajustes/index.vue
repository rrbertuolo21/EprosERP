<script setup lang="ts">
/**
 * Ajustes e Avarias de Estoque (erp/estoque/ajustes).
 * Lançamento de movimentações controladas (EST-MVM-001):
 *   - Ajuste (POST /estoque/ajustes → CriarAjusteEstoqueCommand): cabeçalho + N linhas.
 *     Quantidade positiva = entrada; negativa = saída controlada (EF §7.8). Motivo obrigatório.
 *   - Avaria (POST /estoque/avarias → CriarAvariaEstoqueCommand): saída controlada por perda/avaria.
 *
 * Estas operações não expõem GET de listagem própria (geram fichas/movimentos); o histórico
 * consolidado aparece em Movimento Manual e no Saldo de Estoque. Tela de lançamento (form).
 */
import { computed, reactive, ref } from 'vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useEstoqueEnums } from '~/composables/useEstoqueEnums'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

const toast = useToast()
const { tipoAjuste } = useEstoqueEnums()

type Aba = 'ajuste' | 'avaria'
const aba = ref<Aba>('ajuste')
const salvando = ref(false)

// ---------------- Ajuste ----------------
interface AjusteItem {
  produtoId: string
  quantidade: number | null
  valorUnitario: number | null
  lote: string
}

const ajuste = reactive({
  empresaId: '',
  localId: '',
  dataAjuste: new Date().toISOString().slice(0, 10),
  tipoAjuste: 0 as number,
  valorRecuperado: null as number | null,
  observacao: '',
  itens: [{ produtoId: '', quantidade: null, valorUnitario: null, lote: '' }] as AjusteItem[]
})

const errosAjuste = reactive<Record<string, string>>({})

function addLinhaAjuste() {
  ajuste.itens.push({ produtoId: '', quantidade: null, valorUnitario: null, lote: '' })
}
function removerLinhaAjuste(i: number) {
  ajuste.itens.splice(i, 1)
  if (ajuste.itens.length === 0) addLinhaAjuste()
}

const totalAjuste = computed(() =>
  ajuste.itens.reduce((acc, it) => acc + (it.quantidade ?? 0) * (it.valorUnitario ?? 0), 0)
)

function validarAjuste(): boolean {
  for (const k of Object.keys(errosAjuste)) delete errosAjuste[k]
  if (!ajuste.empresaId.trim()) errosAjuste.empresaId = 'Empresa é obrigatória.'
  if (!ajuste.observacao.trim()) errosAjuste.observacao = 'O motivo/observação é obrigatório.'
  const linhasValidas = ajuste.itens.filter((i) => i.produtoId.trim() && i.quantidade != null && i.quantidade !== 0)
  if (linhasValidas.length === 0) errosAjuste.itens = 'Informe ao menos uma linha com produto e quantidade (≠ 0).'
  return Object.keys(errosAjuste).length === 0
}

async function salvarAjuste() {
  if (!validarAjuste()) {
    toast.error('Formulário de ajuste possui erros.')
    return
  }
  salvando.value = true
  try {
    await useApi('/estoque/ajustes', {
      method: 'POST',
      body: {
        empresaId: ajuste.empresaId.trim(),
        localId: ajuste.localId.trim() || null,
        dataAjuste: ajuste.dataAjuste,
        tipoAjuste: ajuste.tipoAjuste,
        valorRecuperado: ajuste.valorRecuperado,
        observacao: ajuste.observacao.trim(),
        itens: ajuste.itens
          .filter((i) => i.produtoId.trim() && i.quantidade != null && i.quantidade !== 0)
          .map((i) => ({
            produtoId: i.produtoId.trim(),
            quantidade: Number(i.quantidade ?? 0),
            valorUnitario: Number(i.valorUnitario ?? 0),
            lote: i.lote.trim() || null
          }))
      }
    })
    toast.success('Ajuste aplicado com sucesso!')
    ajuste.observacao = ''
    ajuste.valorRecuperado = null
    ajuste.itens = [{ produtoId: '', quantidade: null, valorUnitario: null, lote: '' }]
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

// ---------------- Avaria ----------------
const avaria = reactive({
  empresaId: '',
  produtoId: '',
  codigo: '',
  nome: '',
  categoriaId: '',
  precoCompra: null as number | null,
  quantidade: null as number | null,
  dataAvaria: new Date().toISOString().slice(0, 10),
  nota: '',
  referencia: ''
})
const errosAvaria = reactive<Record<string, string>>({})

function validarAvaria(): boolean {
  for (const k of Object.keys(errosAvaria)) delete errosAvaria[k]
  if (!avaria.empresaId.trim()) errosAvaria.empresaId = 'Empresa é obrigatória.'
  if (!avaria.produtoId.trim()) errosAvaria.produtoId = 'Produto é obrigatório.'
  if (!avaria.nome.trim()) errosAvaria.nome = 'Nome do produto é obrigatório.'
  if (!avaria.categoriaId.trim()) errosAvaria.categoriaId = 'Categoria é obrigatória.'
  if (avaria.quantidade == null || avaria.quantidade <= 0) errosAvaria.quantidade = 'Quantidade deve ser maior que zero.'
  return Object.keys(errosAvaria).length === 0
}

async function salvarAvaria() {
  if (!validarAvaria()) {
    toast.error('Formulário de avaria possui erros.')
    return
  }
  salvando.value = true
  try {
    await useApi('/estoque/avarias', {
      method: 'POST',
      body: {
        empresaId: avaria.empresaId.trim(),
        produtoId: avaria.produtoId.trim(),
        codigo: avaria.codigo.trim() || null,
        nome: avaria.nome.trim(),
        categoriaId: avaria.categoriaId.trim(),
        precoCompra: Number(avaria.precoCompra ?? 0),
        quantidade: Number(avaria.quantidade ?? 0),
        dataAvaria: avaria.dataAvaria,
        nota: avaria.nota.trim() || null,
        referencia: avaria.referencia.trim() || null
      }
    })
    toast.success('Avaria registrada com sucesso!')
    avaria.quantidade = null
    avaria.nota = ''
    avaria.referencia = ''
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
</script>

<template>
  <div>
    <PageToolbar title="Ajustes e Avarias" subtitle="Lançamento de movimentações controladas de estoque" :loading="salvando" />

    <div class="tabs">
      <button type="button" class="tab" :class="{ ativo: aba === 'ajuste' }" @click="aba = 'ajuste'">Ajuste de estoque</button>
      <button type="button" class="tab" :class="{ ativo: aba === 'avaria' }" @click="aba = 'avaria'">Avaria / perda</button>
    </div>

    <!-- Ajuste -->
    <div v-show="aba === 'ajuste'" class="glass-panel form-panel">
      <div class="form-grid">
        <TextField v-model="ajuste.empresaId" label="Empresa (ID)" required :error="errosAjuste.empresaId" />
        <TextField v-model="ajuste.localId" label="Local (ID)" hint="Opcional" />
        <DateTimeField v-model="ajuste.dataAjuste" label="Data do ajuste" required />
        <SelectField v-model="ajuste.tipoAjuste" label="Tipo de ajuste" :options="tipoAjuste.opcoes" :clearable="false" />
        <MoneyInput v-model="ajuste.valorRecuperado" label="Valor recuperado" />
      </div>

      <TextField v-model="ajuste.observacao" label="Motivo / observação" required :error="errosAjuste.observacao" />

      <h3 class="secao-titulo">Itens do ajuste</h3>
      <p v-if="errosAjuste.itens" class="erro-inline">{{ errosAjuste.itens }}</p>
      <div class="tabela-itens">
        <div class="linha-item cabecalho">
          <span>Produto (ID)</span><span>Quantidade (+/−)</span><span>Valor unitário</span><span>Lote</span><span></span>
        </div>
        <div v-for="(item, i) in ajuste.itens" :key="i" class="linha-item">
          <TextField v-model="item.produtoId" placeholder="GUID do produto" />
          <QuantityInput v-model="item.quantidade" :min="-999999" :decimais="4" />
          <MoneyInput v-model="item.valorUnitario" />
          <TextField v-model="item.lote" placeholder="Opcional" />
          <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Remover" @click="removerLinhaAjuste(i)">✕</button>
        </div>
      </div>
      <div class="itens-rodape">
        <button type="button" class="btn btn-secondary btn-sm" @click="addLinhaAjuste">+ Adicionar item</button>
        <span class="total">Total estimado: <strong>{{ totalAjuste.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }) }}</strong></span>
      </div>

      <div class="acoes-form">
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvarAjuste">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Aplicar ajuste</span>
        </button>
      </div>
    </div>

    <!-- Avaria -->
    <div v-show="aba === 'avaria'" class="glass-panel form-panel">
      <div class="form-grid">
        <TextField v-model="avaria.empresaId" label="Empresa (ID)" required :error="errosAvaria.empresaId" />
        <TextField v-model="avaria.produtoId" label="Produto (ID)" required :error="errosAvaria.produtoId" />
        <TextField v-model="avaria.codigo" label="Código" />
        <TextField v-model="avaria.nome" label="Nome do produto" required :error="errosAvaria.nome" />
        <TextField v-model="avaria.categoriaId" label="Categoria (ID)" required :error="errosAvaria.categoriaId" />
        <MoneyInput v-model="avaria.precoCompra" label="Preço de compra" />
        <QuantityInput v-model="avaria.quantidade" label="Quantidade avariada" :decimais="4" :error="errosAvaria.quantidade" />
        <DateTimeField v-model="avaria.dataAvaria" label="Data da avaria" required />
        <TextField v-model="avaria.referencia" label="Referência" />
      </div>
      <TextField v-model="avaria.nota" label="Nota / observação" />

      <div class="acoes-form">
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvarAvaria">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Registrar avaria</span>
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.tabs { display: flex; gap: 4px; margin: 8px 0 12px; }
.tab { padding: 8px 16px; border: 1px solid var(--border-color); background: transparent; color: var(--text-secondary); border-radius: 8px; cursor: pointer; font-size: 13px; }
.tab.ativo { background: var(--primary); color: #fff; border-color: var(--primary); }
.form-panel { padding: 20px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; margin-bottom: 16px; }
.secao-titulo { font-size: 14px; margin: 18px 0 10px; }
.tabela-itens { display: flex; flex-direction: column; gap: 8px; }
.linha-item { display: grid; grid-template-columns: 2fr 1.2fr 1.2fr 1fr 40px; gap: 10px; align-items: end; }
.linha-item.cabecalho { font-size: 12px; color: var(--text-muted); align-items: center; }
.itens-rodape { display: flex; justify-content: space-between; align-items: center; margin-top: 12px; }
.total { font-size: 13px; color: var(--text-secondary); }
.acoes-form { display: flex; justify-content: flex-end; margin-top: 20px; }
.erro-inline { color: var(--danger, #dc3545); font-size: 13px; margin: 4px 0; }
</style>
