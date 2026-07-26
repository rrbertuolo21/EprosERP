<script setup lang="ts">
/**
 * Ordem de Produção (simples) — só-criação.
 * POST /producao/ordens (codigo, produtoAcabadoSku, quantidadePlanejada). Não há GET/{id}
 * nem PUT no backend, então esta tela atende apenas a rota 'novo'. As ações de workflow
 * (iniciar/apontar/encerrar) ficam na listagem, por linha.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isNovo = computed(() => idParam === 'novo')

const salvando = ref(false)
const form = reactive<{ codigo: string | null; produtoAcabadoSku: string | null; quantidadePlanejada: number | null }>({
  codigo: null,
  produtoAcabadoSku: null,
  quantidadePlanejada: 0
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.produtoAcabadoSku) erros.produtoAcabadoSku = 'SKU do produto é obrigatório.'
  if (!form.quantidadePlanejada || form.quantidadePlanejada <= 0) erros.quantidadePlanejada = 'Quantidade planejada deve ser maior que zero.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) { toast.error('Formulário possui erros de validação.'); return }
  salvando.value = true
  try {
    await useApi('/producao/ordens', { method: 'POST', body: form })
    toast.success('Ordem criada com sucesso!')
    router.push('/erp/producao/ordens')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() { router.push('/erp/producao/ordens') }

onMounted(() => {
  if (!isNovo.value) {
    // Não há endpoint de detalhe/edição para ordens simples.
    toast.error('Ordens de produção não possuem edição. Use a listagem para iniciar/apontar/encerrar.')
    router.replace('/erp/producao/ordens')
  }
})
</script>

<template>
  <div>
    <PageToolbar title="Nova ordem de produção" :loading="salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" maxlength="60" hint="Opcional" />
          <TextField v-model="form.produtoAcabadoSku" label="SKU do produto acabado" required :error="erros.produtoAcabadoSku" />
          <QuantityInput v-model="form.quantidadePlanejada" label="Quantidade planejada" :error="erros.quantidadePlanejada" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
