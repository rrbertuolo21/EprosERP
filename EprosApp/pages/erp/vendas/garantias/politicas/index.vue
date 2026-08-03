<script setup lang="ts">
/**
 * Garantias (RMA) — Políticas de garantia.
 * Contrato real: base `/vendas/garantias`.
 *   GET/POST politicas · PUT politicas/{id} · POST politicas/{id}/inativar.
 * Lista + criação via RecursoLista; inativar pelo slot de ações. Apresentação — sem regra nova.
 */
import { ref } from 'vue'
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'

definePageMeta({ layout: 'default', middleware: 'auth' })

const TIPO_DURACAO = [
  { value: 0, label: 'Dias' },
  { value: 1, label: 'Meses' },
  { value: 2, label: 'Anos' }
]
const UNIDADE_USO = [
  { value: 0, label: 'Nenhuma' },
  { value: 1, label: 'Km' },
  { value: 2, label: 'Horas' }
]
function tipoDuracaoLabel(v: unknown) {
  return TIPO_DURACAO.find((t) => t.value === Number(v))?.label ?? ''
}

interface Politica {
  id: string
  nome?: string | null
  descricao?: string | null
  duracao?: number | null
  tipoDuracao?: number | null
  limiteUso?: number | null
  ativo?: boolean | null
}

const toast = useToast()
const listaRef = ref<InstanceType<typeof RecursoLista>>()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const colunas: DataTableColumn<Politica>[] = [
  { key: 'nome', label: 'Política' },
  { key: 'descricao', label: 'Descrição' },
  { key: 'duracao', label: 'Duração', align: 'right' },
  { key: 'tipoDuracao', label: 'Unidade', formatter: tipoDuracaoLabel },
  { key: 'ativo', label: 'Situação', formatter: (v) => (v ? 'Ativa' : 'Inativa') }
]

const campos: CampoForm[] = [
  { key: 'nome', label: 'Nome', tipo: 'text', obrigatorio: true },
  { key: 'descricao', label: 'Descrição', tipo: 'textarea' },
  { key: 'duracao', label: 'Duração', tipo: 'int', obrigatorio: true },
  { key: 'tipoDuracao', label: 'Unidade da duração', tipo: 'select', opcoes: TIPO_DURACAO, obrigatorio: true },
  { key: 'limiteUso', label: 'Limite de uso', tipo: 'money', hint: 'Km/horas máximos, se aplicável.' },
  { key: 'unidadeUso', label: 'Unidade de uso', tipo: 'select', opcoes: UNIDADE_USO }
]

async function inativar(p: Politica) {
  const ok = await confirmRef.value!.open('Inativar política', `Inativar a política "${p.nome ?? ''}"?`)
  if (!ok) return
  try {
    await useApi('/vendas/garantias/politicas/{id}/inativar', { method: 'POST', params: { id: p.id } })
    toast.success('Política inativada.')
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
      title="Garantias — políticas"
      subtitle="Políticas de garantia (RMA)"
      path="/vendas/garantias/politicas"
      :columns="colunas"
      :campos="campos"
      rotulo-novo="+ Nova política"
      :com-busca="false"
      :tem-acoes="true"
    >
      <template #acoes-linha="{ row }">
        <button v-if="row.ativo !== false" type="button" class="btn btn-ghost btn-sm btn-danger-action" @click.stop="inativar(row)">Inativar</button>
      </template>
    </RecursoLista>
    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
