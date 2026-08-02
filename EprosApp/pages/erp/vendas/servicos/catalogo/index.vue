<script setup lang="ts">
/**
 * Gestão de Serviços — Catálogo de serviços comerciais.
 * Contrato real: base `/vendas/servicos`.
 *   GET/POST catalogo · DELETE catalogo/{id} · POST catalogo/{id}/reativar.
 * Lista + criação via RecursoLista; excluir/reativar pelo slot de ações. Apresentação — sem regra nova.
 */
import { ref } from 'vue'
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import { formatMoeda } from '~/components/concessionarias-shared/formatadores'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'

definePageMeta({ layout: 'default', middleware: 'auth' })

interface Servico {
  id: string
  nome?: string | null
  descricao?: string | null
  valor?: number | null
  taxa?: number | null
  ativo?: boolean | null
}

const toast = useToast()
const listaRef = ref<InstanceType<typeof RecursoLista>>()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const colunas: DataTableColumn<Servico>[] = [
  { key: 'nome', label: 'Serviço' },
  { key: 'descricao', label: 'Descrição' },
  { key: 'valor', label: 'Valor', align: 'right', formatter: formatMoeda },
  { key: 'taxa', label: 'Taxa', align: 'right', formatter: formatMoeda },
  { key: 'ativo', label: 'Situação', formatter: (v) => (v ? 'Ativo' : 'Inativo') }
]

const campos: CampoForm[] = [
  { key: 'nome', label: 'Nome', tipo: 'text', obrigatorio: true },
  { key: 'descricao', label: 'Descrição', tipo: 'textarea' },
  { key: 'valor', label: 'Valor', tipo: 'money' },
  { key: 'taxa', label: 'Taxa', tipo: 'money' },
  { key: 'permiteCamposCustomizados', label: 'Permite campos customizados', tipo: 'boolean' }
]

async function excluir(s: Servico) {
  const ok = await confirmRef.value!.open('Excluir serviço', `Excluir o serviço "${s.nome ?? ''}"?`)
  if (!ok) return
  try {
    await useApi('/vendas/servicos/catalogo/{id}', { method: 'DELETE', params: { id: s.id } })
    toast.success('Serviço excluído.')
    await listaRef.value?.recarregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}
async function reativar(s: Servico) {
  try {
    await useApi('/vendas/servicos/catalogo/{id}/reativar', { method: 'POST', params: { id: s.id } })
    toast.success('Serviço reativado.')
    await listaRef.value?.recarregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}
</script>

<template>
  <div>
    <RecursoLista
      ref="listaRef"
      title="Catálogo de serviços"
      subtitle="Gestão de Serviços — serviços comerciais"
      path="/vendas/servicos/catalogo"
      :columns="colunas"
      :campos="campos"
      rotulo-novo="+ Novo serviço"
      :com-busca="true"
      :tem-acoes="true"
    >
      <template #acoes-linha="{ row }">
        <button v-if="row.ativo !== false" type="button" class="btn btn-ghost btn-sm btn-danger-action" @click.stop="excluir(row)">Excluir</button>
        <button v-else type="button" class="btn btn-ghost btn-sm" @click.stop="reativar(row)">Reativar</button>
      </template>
    </RecursoLista>
    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
