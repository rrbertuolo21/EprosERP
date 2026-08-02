<script setup lang="ts">
/**
 * Portal do Cliente — Usuários de acesso.
 * Contrato real: GET/POST `/vendas/portal/usuarios` (CriarPortalUsuarioClienteCommand).
 * Lista + criação via RecursoLista. Apresentação — sem regra nova.
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'

definePageMeta({ layout: 'default', middleware: 'auth' })

interface Usuario {
  id: string
  nome?: string | null
  email?: string | null
  administradorCliente?: boolean | null
  status?: number | string | null
}

const colunas: DataTableColumn<Usuario>[] = [
  { key: 'nome', label: 'Nome' },
  { key: 'email', label: 'E-mail' },
  { key: 'administradorCliente', label: 'Admin', formatter: (v) => (v ? 'Sim' : 'Não') },
  { key: 'status', label: 'Status' }
]

const campos: CampoForm[] = [
  { key: 'clienteId', label: 'Cliente', tipo: 'fk', fkPath: '/cadastros/pessoas', obrigatorio: true },
  { key: 'nome', label: 'Nome', tipo: 'text', obrigatorio: true },
  { key: 'email', label: 'E-mail', tipo: 'text', obrigatorio: true },
  { key: 'telefone', label: 'Telefone', tipo: 'text' },
  { key: 'administradorCliente', label: 'Administrador do cliente', tipo: 'boolean' }
]
</script>

<template>
  <RecursoLista
    title="Portal do cliente — usuários"
    subtitle="Acessos dos clientes ao portal"
    path="/vendas/portal/usuarios"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Novo usuário"
    :com-busca="false"
  />
</template>
