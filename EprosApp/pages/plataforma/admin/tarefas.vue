<script setup lang="ts">
/**
 * Tarefas Agendadas — launcher do Hangfire Dashboard.
 *
 * Não há endpoint REST de listagem no backend novo; assim como no Blazor, a tela
 * apenas abre o dashboard externo do Hangfire montado na API (`/hangfire`).
 */
import { computed } from 'vue'
import { useRuntimeConfig } from '#app'

definePageMeta({ layout: 'admin' })

const config = useRuntimeConfig()
const hangfireUrl = computed(() => {
  const base = ((config.public.apiBaseUrl as string) || '').replace(/\/$/, '')
  return `${base}/hangfire`
})
</script>

<template>
  <div class="admin-page">
    <header class="admin-page-header">
      <div>
        <h1 class="admin-page-title">Tarefas Agendadas</h1>
        <p class="admin-page-sub">Monitoramento de jobs recorrentes e em background.</p>
      </div>
    </header>

    <section class="launcher-card glass-panel">
      <span class="launcher-icon">⏱️</span>
      <div class="launcher-body">
        <h3>Hangfire Dashboard</h3>
        <p>
          O agendamento e o histórico de jobs (faturamento recorrente, outbox, workers) são
          administrados diretamente no painel do Hangfire, montado na API da plataforma.
        </p>
        <a :href="hangfireUrl" target="_blank" rel="noopener noreferrer" class="btn btn-primary">
          Abrir Hangfire ↗
        </a>
      </div>
    </section>
  </div>
</template>

<style scoped>
.admin-page { display: flex; flex-direction: column; gap: 20px; }
.admin-page-header { display: flex; justify-content: space-between; align-items: flex-end; gap: 12px; flex-wrap: wrap; }
.admin-page-title { font-size: 24px; font-weight: 800; letter-spacing: -0.5px; color: var(--text-primary); }
.admin-page-sub { font-size: 13px; color: var(--text-secondary); margin-top: 2px; }
.launcher-card { display: flex; gap: 20px; padding: 28px; align-items: flex-start; max-width: 640px; }
.launcher-icon { font-size: 40px; line-height: 1; }
.launcher-body { display: flex; flex-direction: column; gap: 10px; align-items: flex-start; }
.launcher-body h3 { font-size: 17px; font-weight: 700; color: var(--text-primary); }
.launcher-body p { font-size: 13px; color: var(--text-secondary); line-height: 1.55; }
.launcher-body .btn { margin-top: 6px; text-decoration: none; }
</style>
