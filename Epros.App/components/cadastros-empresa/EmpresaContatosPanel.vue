<script setup lang="ts">
/**
 * EmpresaContatosPanel — aba "Contato" do formulário de empresa.
 *
 * Mantém uma lista local de contatos (nome/telefone/e-mail) e emite `update:modelValue`
 * a cada alteração. A persistência real (POST/PUT/DELETE em
 * `cadastros/empresas/{id}/contatos`) fica a cargo da página `[id].vue`, que decide
 * quando sincronizar (a API separa Contatos da Empresa em sub-recurso próprio).
 */
import { reactive } from 'vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import type { EmpresaContato } from './types'

const props = defineProps<{
  modelValue: EmpresaContato[]
}>()

const emit = defineEmits<{
  'update:modelValue': [value: EmpresaContato[]]
}>()

const tipoTelefoneOpcoes = [
  { label: 'Celular', value: 1 },
  { label: 'Fixo', value: 2 },
  { label: 'WhatsApp', value: 3 }
]

const novoContato = reactive<EmpresaContato>({
  nome: '',
  telefone: '',
  email: '',
  tipoTelefone: 2
})

const colunas: DataTableColumn<EmpresaContato>[] = [
  { key: 'nome', label: 'Nome' },
  { key: 'telefone', label: 'Número' },
  { key: 'email', label: 'E-mail' }
]

function adicionar() {
  if (!novoContato.nome) return
  emit('update:modelValue', [...props.modelValue, { ...novoContato }])
  novoContato.nome = ''
  novoContato.telefone = ''
  novoContato.email = ''
  novoContato.tipoTelefone = 2
}

function remover(item: EmpresaContato) {
  emit(
    'update:modelValue',
    props.modelValue.filter((c) => c !== item)
  )
}
</script>

<template>
  <div>
    <div class="form-grid">
      <div class="col-6">
        <TextField v-model="novoContato.nome" label="Nome" />
      </div>
      <div class="col-6">
        <TextField v-model="novoContato.email" label="E-mail" type="email" />
      </div>
      <div class="col-6">
        <SelectField v-model="novoContato.tipoTelefone" label="Tipo de contato telefônico" :options="tipoTelefoneOpcoes" />
      </div>
      <div class="col-4">
        <TextField v-model="novoContato.telefone" label="Telefone" placeholder="(00) 00000-0000" />
      </div>
      <div class="col-2 contatos-add">
        <button type="button" class="btn btn-primary" @click="adicionar">+ Adicionar</button>
      </div>
    </div>

    <DataTable
      :items="modelValue"
      :columns="colunas"
      :total="modelValue.length"
      :page="1"
      :page-size="modelValue.length || 1"
      empty-text="Nenhum contato adicionado."
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click="remover(row)">Remover</button>
      </template>
    </DataTable>
  </div>
</template>

<style scoped>
.contatos-add {
  display: flex;
  align-items: flex-end;
  padding-bottom: 4px;
}
</style>
