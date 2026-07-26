<script setup lang="ts">
/**
 * Detalhe de Veículo (somente leitura) — GET /veiculos/{id}.
 * Como o digest não detalha o DTO, renderizamos os campos retornados de forma
 * genérica (rótulo/valor), sem inventar estrutura.
 */
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const carregando = ref(false)
const veiculo = ref<Record<string, unknown> | null>(null)

const camposExibicao = computed(() => {
  if (!veiculo.value) return [] as { chave: string; valor: string }[]
  return Object.entries(veiculo.value)
    .filter(([, v]) => v !== null && v !== undefined && typeof v !== 'object')
    .map(([chave, valor]) => ({ chave: humanizar(chave), valor: String(valor) }))
})

function humanizar(chave: string): string {
  const s = chave.replace(/([A-Z])/g, ' $1').replace(/_/g, ' ')
  return s.charAt(0).toUpperCase() + s.slice(1)
}

async function carregar() {
  carregando.value = true
  try {
    const resposta = await useApi('/veiculos/{id}', { params: { id: idParam } })
    veiculo.value = extrairDados<Record<string, unknown>>(resposta) ?? null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

function voltar() {
  router.push('/erp/concessionarias/veiculos')
}

onMounted(() => {
  void carregar()
})
</script>

<template>
  <div>
    <PageToolbar title="Detalhe do veículo" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
      </template>
    </PageToolbar>

    <div class="glass-panel detalhe-panel">
      <p v-if="!carregando && camposExibicao.length === 0" class="vazio">
        Nenhum dado disponível para este veículo.
      </p>
      <dl v-else class="detalhe-grid">
        <div v-for="campo in camposExibicao" :key="campo.chave" class="detalhe-item">
          <dt>{{ campo.chave }}</dt>
          <dd>{{ campo.valor }}</dd>
        </div>
      </dl>
    </div>
  </div>
</template>

<style scoped>
.detalhe-panel { padding: 20px; margin-top: 8px; }
.detalhe-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 16px;
}
.detalhe-item dt { font-size: 12px; color: var(--text-secondary); margin-bottom: 4px; }
.detalhe-item dd { font-size: 14px; color: var(--text-primary); margin: 0; }
.vazio { color: var(--text-secondary); font-size: 14px; }
</style>
