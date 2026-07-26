<script setup lang="ts">
/**
 * Listagem de Categorias de Denúncia — GRC.
 * Fonte: GET /api/v1/grc/denuncias/categorias, POST criação e
 * POST /api/v1/grc/denuncias/categorias/{categoriaId}/inativar (ação por linha).
 */
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Categoria {
  id: string
  nome?: string | null
  descricao?: string | null
  cor?: string | null
  ativa?: boolean
}

const router = useRouter()
const toast = useToast()
const inativandoId = ref<string | null>(null)

const lista = useApiList<Categoria>('/grc/denuncias/categorias', { tamanhoPaginaInicial: 50 })

const colunas: DataTableColumn<Categoria>[] = [
  { key: 'nome', label: 'Nome', sortable: true },
  { key: 'descricao', label: 'Descrição' },
  { key: 'cor', label: 'Cor', align: 'center', width: '110px' },
  { key: 'ativa', label: 'Status', align: 'center', width: '110px' }
]

async function inativar(item: Categoria) {
  inativandoId.value = item.id
  try {
    await useApi('/grc/denuncias/categorias/{categoriaId}/inativar', { method: 'POST', params: { categoriaId: item.id } })
    toast.success('Categoria inativada com sucesso.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    inativandoId.value = null
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Categorias de Denúncia" subtitle="Classificação usada na triagem de denúncias" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="router.push('/erp/grc/denuncia-categorias/novo')">+ Nova categoria</button>
      </template>
    </PageToolbar>

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhuma categoria cadastrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-cor="{ value }">
        <span v-if="value" class="cor-chip" :style="{ background: String(value) }"></span>
        <span v-if="value">{{ value }}</span>
      </template>
      <template #cell-ativa="{ value }">
        <span class="badge" :class="value ? 'badge-success' : 'badge-danger'">{{ value ? 'Ativa' : 'Inativa' }}</span>
      </template>
      <template #actions="{ row }">
        <button
          v-if="row.ativa"
          type="button"
          class="btn btn-ghost btn-sm btn-danger-action"
          :disabled="inativandoId === row.id"
          @click.stop="inativar(row)"
        >Inativar</button>
      </template>
    </DataTable>
  </div>
</template>

<style scoped>
.cor-chip { display: inline-block; width: 14px; height: 14px; border-radius: 3px; margin-right: 6px; vertical-align: middle; }
</style>
