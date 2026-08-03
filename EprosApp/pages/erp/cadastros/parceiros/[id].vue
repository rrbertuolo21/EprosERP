<script setup lang="ts">
/**
 * Cadastro/edição de Parceiro. Porta `cadastros/parceiro/[id].vue` do legado:
 * toolbar com Cancelar/Salvar + `ParceiroForm` (rota `/erp/cadastros/parceiros/novo` para criação).
 */
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ParceiroForm from '~/components/cadastros-parceiro/ParceiroForm.vue'
import { useToast } from '~/composables/useToast'

definePageMeta({ layout: 'default' })

const route = useRoute()
const router = useRouter()
const toast = useToast()

const id = computed(() => String(route.params.id))
const ehNovo = computed(() => id.value === 'novo')
const titulo = computed(() => (ehNovo.value ? 'Novo parceiro' : 'Editar parceiro'))

const formRef = ref<InstanceType<typeof ParceiroForm> | null>(null)
const salvando = ref(false)

function cancelar() {
  router.push('/erp/cadastros/parceiros')
}

async function salvar() {
  if (!formRef.value) return
  salvando.value = true
  try {
    const ok = await formRef.value.salvar()
    if (ok) {
      router.push('/erp/cadastros/parceiros')
    }
  } catch (e) {
    toast.error('Não foi possível salvar o cadastro. Tente novamente.')
    console.error('[parceiros/[id].salvar]', e)
  } finally {
    salvando.value = false
  }
}
</script>

<template>
  <div>
    <PageToolbar :title="titulo">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <ParceiroForm :id="id" ref="formRef" />
  </div>
</template>
