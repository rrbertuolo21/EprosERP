<script setup lang="ts">
/**
 * Cadastro/edição de Produto (erp/estoque/produtos/[id]).
 * Consome o catálogo do módulo Estoque — `GET/POST/PUT/DELETE /estoque/produtos`
 * (EstoqueController, `CriarProdutoCommand`/`AtualizarProdutoCommand`).
 * Guid como identificador; campos espelham exatamente o command do backend.
 *
 * Endpoint: estoque/produtos (GET/POST/PUT/DELETE por id, Guid).
 */
import { computed, onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

interface ProdutoDto {
  id?: string
  categoriaId?: string | null
  marcaProdutoId?: string | null
  unidadeMedidaComercialId?: string | null
  codigo?: string
  descricao?: string
  ean?: string | null
  pesoLiquido?: number
  pesoBruto?: number
  valorVenda?: number
  valorVendaPrazo?: number
  valorCompra?: number
  tipoProduto?: number | null
  ativo?: boolean
  imagem?: string
  cestId?: string | null
  codigoAnpId?: string | null
  balancaId?: string | null
  utilizaBalanca?: boolean
  codigoProdutoBalanca?: string | null
  ncmId?: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = computed(() => route.params.id as string)
const ehNovo = computed(() => idParam.value === 'novo')

const pageTitle = computed(() => (ehNovo.value ? 'Novo produto' : 'Editar produto'))

const carregando = ref(false)
const salvando = ref(false)
const erros = ref<Record<string, string>>({})

const form = ref({
  codigo: '',
  descricao: '',
  ean: '',
  pesoLiquido: 0 as number | null,
  pesoBruto: 0 as number | null,
  valorVenda: 0 as number | null,
  valorVendaPrazo: 0 as number | null,
  valorCompra: 0 as number | null,
  ativo: true,
  utilizaBalanca: false,
  codigoProdutoBalanca: ''
})

function aplicarDto(dto: ProdutoDto) {
  form.value = {
    codigo: dto.codigo ?? '',
    descricao: dto.descricao ?? '',
    ean: dto.ean ?? '',
    pesoLiquido: dto.pesoLiquido ?? 0,
    pesoBruto: dto.pesoBruto ?? 0,
    valorVenda: dto.valorVenda ?? 0,
    valorVendaPrazo: dto.valorVendaPrazo ?? 0,
    valorCompra: dto.valorCompra ?? 0,
    ativo: dto.ativo ?? true,
    utilizaBalanca: dto.utilizaBalanca ?? false,
    codigoProdutoBalanca: dto.codigoProdutoBalanca ?? ''
  }
}

async function carregarPorId(id: string) {
  carregando.value = true
  try {
    const resp = await useApi(`/estoque/produtos/{id}`, { params: { id } })
    const dto = extrairDados<ProdutoDto>(resp)
    if (dto) aplicarDto(dto)
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

function validar(): boolean {
  const novosErros: Record<string, string> = {}
  if (!form.value.codigo.trim()) novosErros.codigo = 'Informe o código do produto'
  if (!form.value.descricao.trim()) novosErros.descricao = 'Informe a descrição do produto'
  erros.value = novosErros
  return Object.keys(novosErros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação')
    return
  }

  salvando.value = true
  try {
    const payloadBase = {
      categoriaId: null,
      marcaProdutoId: null,
      unidadeMedidaComercialId: null,
      codigo: form.value.codigo.trim(),
      descricao: form.value.descricao.trim(),
      ean: form.value.ean.trim() || 'SEM GTIN',
      pesoLiquido: Number(form.value.pesoLiquido ?? 0),
      pesoBruto: Number(form.value.pesoBruto ?? 0),
      valorVenda: Number(form.value.valorVenda ?? 0),
      valorVendaPrazo: Number(form.value.valorVendaPrazo ?? 0),
      valorCompra: Number(form.value.valorCompra ?? 0),
      tipoProduto: null,
      ativo: form.value.ativo,
      cestId: null,
      codigoAnpId: null,
      balancaId: null,
      utilizaBalanca: form.value.utilizaBalanca,
      codigoProdutoBalanca: form.value.codigoProdutoBalanca || null,
      ncmId: null
    }

    if (!ehNovo.value) {
      await useApi(`/estoque/produtos/{id}`, {
        method: 'PUT',
        params: { id: idParam.value },
        body: { id: idParam.value, ...payloadBase }
      })
    } else {
      await useApi('/estoque/produtos', { method: 'POST', body: { ...payloadBase, imagem: '' } })
    }

    toast.success(ehNovo.value ? 'Produto criado com sucesso' : 'Produto atualizado com sucesso')
    await router.push('/erp/estoque/produtos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/estoque/produtos')
}

onMounted(async () => {
  if (!ehNovo.value) {
    await carregarPorId(idParam.value)
  }
})
</script>

<template>
  <div>
    <PageToolbar :title="pageTitle" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando || carregando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div class="form-grid">
        <div class="col-3">
          <TextField v-model="form.codigo" label="Código" required :error="erros.codigo" />
        </div>
        <div class="col-6">
          <TextField v-model="form.descricao" label="Descrição" required :error="erros.descricao" />
        </div>
        <div class="col-3">
          <TextField v-model="form.ean" label="EAN" hint="Se deixado em branco, será salvo como 'SEM GTIN'" />
        </div>

        <div class="col-3">
          <MoneyInput v-model="form.valorVenda" label="Valor de venda" />
        </div>
        <div class="col-3">
          <MoneyInput v-model="form.valorVendaPrazo" label="Valor de venda a prazo" />
        </div>
        <div class="col-3">
          <MoneyInput v-model="form.valorCompra" label="Valor de compra" />
        </div>
        <div class="col-3">
          <label class="field-label">Situação</label>
          <select v-model="form.ativo" class="input">
            <option :value="true">Ativo</option>
            <option :value="false">Inativo</option>
          </select>
        </div>

        <div class="col-3">
          <QuantityInput v-model="form.pesoLiquido" label="Peso líquido" :decimais="3" />
        </div>
        <div class="col-3">
          <QuantityInput v-model="form.pesoBruto" label="Peso bruto" :decimais="3" />
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; }
</style>
