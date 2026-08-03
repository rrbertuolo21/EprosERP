<script setup lang="ts">
/**
 * Upgrades de Versão (Landlord) — GovernancaVersaoController (rota `api/v1/super-admin/upgrades`).
 *
 * Fluxo de governança de versão do SaaS: solicitar → aprovar/rejeitar → executar → rollback.
 * Contrato:
 *   GET  /super-admin/upgrades
 *   POST /super-admin/upgrades/{id}/aprovar | /rejeitar | /executar | /rollback
 */
import { ref, onMounted } from 'vue'
import { useApi, extrairLista } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'

definePageMeta({ layout: 'admin' })

const toast = useToast()
const { formatarDataHora } = useHelper()

interface Upgrade {
  id: string
  versaoAtual?: string | null
  versaoAlvo?: string | null
  motivo?: string | null
  status?: number | string | null
  solicitadoPor?: string | null
  aprovadoPor?: string | null
  criadoEm?: string | null
}

const itens = ref<Upgrade[]>([])
const carregando = ref(false)
const processando = ref<string | null>(null)

function statusLabel(s: unknown): string {
  const map: Record<string, string> = { '0': 'Solicitado', '1': 'Aprovado', '2': 'Rejeitado', '3': 'Executado', '4': 'Rollback' }
  return map[String(s)] ?? String(s ?? '—')
}

async function carregar() {
  carregando.value = true
  try {
    const resp = await useApi('/super-admin/upgrades')
    itens.value = extrairLista<Upgrade>(resp)
  } catch (e) {
    toast.error(obterMensagemErro(e))
    itens.value = []
  } finally {
    carregando.value = false
  }
}

async function acao(u: Upgrade, verbo: 'aprovar' | 'rejeitar' | 'executar' | 'rollback') {
  processando.value = u.id
  try {
    await useApi(`/super-admin/upgrades/{id}/${verbo}`, { method: 'POST', params: { id: u.id }, body: {} })
    toast.success(`Upgrade ${verbo === 'aprovar' ? 'aprovado' : verbo === 'rejeitar' ? 'rejeitado' : verbo === 'executar' ? 'executado' : 'revertido'}.`)
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = null
  }
}

onMounted(carregar)
</script>

<template>
  <div>
    <header class="page-header">
      <h1 class="glow-text">Upgrades de Versão</h1>
      <p class="tagline">Governança de versão do SaaS: aprovação, execução e rollback dos upgrades.</p>
    </header>

    <section class="admin-section glass-panel mt-4">
      <header class="section-header">
        <h3>Solicitações</h3>
        <button class="btn btn-secondary btn-table-action" :disabled="carregando" @click="carregar">Atualizar</button>
      </header>
      <table class="data-table">
        <thead>
          <tr><th>Versão atual</th><th>Versão alvo</th><th>Motivo</th><th>Solicitado por</th><th>Criado</th><th>Status</th><th></th></tr>
        </thead>
        <tbody>
          <tr v-if="carregando"><td colspan="7" class="empty">Carregando…</td></tr>
          <tr v-else-if="!itens.length"><td colspan="7" class="empty">Nenhuma solicitação de upgrade.</td></tr>
          <tr v-for="u in itens" :key="u.id">
            <td><code>{{ u.versaoAtual || '—' }}</code></td>
            <td><code>{{ u.versaoAlvo || '—' }}</code></td>
            <td>{{ u.motivo || '—' }}</td>
            <td>{{ u.solicitadoPor || '—' }}</td>
            <td>{{ u.criadoEm ? formatarDataHora(u.criadoEm) : '—' }}</td>
            <td><span class="badge">{{ statusLabel(u.status) }}</span></td>
            <td class="align-right">
              <button class="btn btn-secondary btn-table-action" :disabled="processando === u.id" @click="acao(u, 'aprovar')">Aprovar</button>
              <button class="btn btn-secondary btn-table-action" :disabled="processando === u.id" @click="acao(u, 'rejeitar')">Rejeitar</button>
              <button class="btn btn-secondary btn-table-action" :disabled="processando === u.id" @click="acao(u, 'executar')">Executar</button>
              <button class="btn btn-secondary btn-table-action" :disabled="processando === u.id" @click="acao(u, 'rollback')">Rollback</button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>
  </div>
</template>
