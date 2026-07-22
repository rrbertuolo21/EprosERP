<script setup lang="ts">
/**
 * Perfis de acesso — listagem.
 *
 * Porta `configuracoes/permissoes/perfil-usuarios/index.vue` do legado.
 *
 * Contrato de rotas fixado (equalização): CRUD de perfis de acesso vive em
 * `api/v1/plataforma/perfis-acesso`.
 *
 * ATENÇÃO (gap de backend): o único controller disponível hoje é `PerfilController`
 * (`api/v1/plataforma/perfil`), que expõe apenas `GET menu` e `GET testar-desconto` —
 * não há listar/criar/editar/excluir perfil em `plataforma/perfis-acesso` ainda.
 * Esta tela já chama a rota do contrato; enquanto o backend não implementar o CRUD,
 * a listagem fica vazia com mensagem de erro amigável.
 */
import { onMounted, ref } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'

definePageMeta({
  middleware: 'auth'
})

interface PerfilLinha {
  id: number
  descricao: string
  [key: string]: unknown
}

const toast = useToast()

const lista = useApiList<PerfilLinha>('/plataforma/perfis-acesso', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<PerfilLinha>[] = [
  { key: 'descricao', label: 'Descrição', sortable: true }
]

const excluirVisivel = ref(false)
const excluirAlvo = ref<PerfilLinha | null>(null)
const excluindo = ref(false)

function novoPerfil() {
  navigateTo('/erp/configuracoes/permissoes/perfis/novo')
}

function editarPerfil(item: PerfilLinha) {
  navigateTo(`/erp/configuracoes/permissoes/perfis/${item.id}`)
}

function pedirExclusao(item: PerfilLinha) {
  excluirAlvo.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!excluirAlvo.value) return
  excluindo.value = true
  try {
    await useApi('/plataforma/perfis-acesso/{id}', { method: 'DELETE', params: { id: excluirAlvo.value.id } })
    toast.success('Perfil excluído com sucesso.')
    excluirVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Perfis de usuário" subtitle="Perfis de acesso e permissões" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novoPerfil">+ Novo perfil</button>
      </template>
    </PageToolbar>

    <div v-if="lista.erro.value" class="glass-panel aviso-erro">
      Não foi possível carregar os perfis: {{ lista.erro.value }}. O backend ainda não expõe o CRUD de
      listagem em <code>plataforma/perfis-acesso</code> — ver observação ao integrador.
    </div>

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      row-key="id"
      empty-text="Nenhum perfil encontrado."
      @update:page="(p) => lista.irParaPagina(p)"
      @update:page-size="(s) => lista.buscar({ tamanhoPagina: s, pagina: 1 })"
      @update:sort="(s) => lista.buscar({ ordenacao: s })"
      @row-click="editarPerfil"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editarPerfil(row)">✏️</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Excluir" @click.stop="pedirExclusao(row)">🗑️</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="excluirAlvo?.descricao"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>

<style scoped>
.aviso-erro { margin-bottom: 16px; padding: 14px 18px; font-size: 13px; color: var(--text-secondary); }
.aviso-erro code { background: rgba(255, 255, 255, 0.06); padding: 1px 5px; border-radius: 4px; }
</style>
