<script setup lang="ts">
/**
 * CRM Comercial — Atendimento (tickets).
 * Contrato real: base `/vendas/crm`.
 *   GET  tickets?statusId=&clienteId=&incluirArquivados=&pagina=&tamanhoPagina=
 *   POST tickets/{id}/responder   (ResponderCrmTicketCommand: { ticketId, texto, tipo })
 * A criação de ticket exige StatusId (status configurável sem endpoint de listagem) — ver relatório.
 * Lista + resposta. Apresentação — sem regra nova.
 */
import { onMounted, reactive, ref } from 'vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { formatDataHora } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default', middleware: 'auth' })

const TIPO_RESPOSTA = [
  { value: 0, label: 'Resposta' },
  { value: 1, label: 'Nota interna' }
]

interface Ticket {
  id: string
  titulo?: string | null
  descricao?: string | null
  prioridade?: number | null
  criadoEm?: string | null
}

const toast = useToast()
const lista = useApiList<Ticket>('/vendas/crm/tickets', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<Ticket>[] = [
  { key: 'titulo', label: 'Título' },
  { key: 'descricao', label: 'Descrição' },
  { key: 'criadoEm', label: 'Aberto em', formatter: formatDataHora }
]

// Responder
const dlg = ref(false)
const salvando = ref(false)
const alvo = ref<Ticket | null>(null)
const resp = reactive({ texto: '', tipo: 0 as number })

function abrirResponder(t: Ticket) {
  alvo.value = t
  resp.texto = ''
  resp.tipo = 0
  dlg.value = true
}
async function confirmar() {
  if (!alvo.value || !resp.texto) {
    toast.warning('Informe o texto da resposta.')
    return
  }
  salvando.value = true
  try {
    await useApi('/vendas/crm/tickets/{id}/responder', {
      method: 'POST',
      params: { id: alvo.value.id },
      body: { ticketId: alvo.value.id, texto: resp.texto, tipo: resp.tipo }
    })
    toast.success('Resposta registrada.')
    dlg.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

onMounted(() => void lista.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Atendimento" subtitle="CRM Comercial — tickets de suporte" :loading="lista.carregando.value" />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhum ticket encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrirResponder(row)">Responder</button>
      </template>
    </DataTable>

    <AppDialog v-model="dlg" title="Responder ticket" width="560px" persistent>
      <div class="form-grid">
        <SelectField v-model="resp.tipo" label="Tipo" :options="TIPO_RESPOSTA" :clearable="false" />
        <TextField v-model="resp.texto" label="Texto" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="dlg = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="confirmar">
          <span v-if="salvando" class="spinner"></span><span v-else>Enviar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
