<script setup lang="ts">
/**
 * Sobre — informações de versão e ambiente da Administração SaaS.
 */
import { computed } from 'vue'
import { useRuntimeConfig } from '#app'

definePageMeta({ layout: 'admin' })

const config = useRuntimeConfig()

const apiBase = computed(() => ((config.public.apiBaseUrl as string) || '').replace(/\/$/, '') || 'mesma origem (proxy)')

const infos = computed(() => [
  { label: 'Produto', valor: 'Epros — Administração SaaS (Landlord)' },
  { label: 'Versão do painel', valor: '1.0.0' },
  { label: 'Base da API', valor: apiBase.value },
  { label: 'Prefixo de rotas', valor: '/api/v1' }
])
</script>

<template>
  <div class="admin-page">
    <header class="admin-page-header">
      <div>
        <h1 class="admin-page-title">Sobre</h1>
        <p class="admin-page-sub">Informações de versão e ambiente da plataforma.</p>
      </div>
    </header>

    <section class="admin-section glass-panel about-card">
      <div class="about-brand">
        <span class="about-sym">▲</span>
        <div>
          <h3>Epros Administração</h3>
          <p>Painel de gestão da plataforma (Dono da Plataforma).</p>
        </div>
      </div>
      <dl class="about-list">
        <div v-for="i in infos" :key="i.label" class="about-row">
          <dt>{{ i.label }}</dt>
          <dd>{{ i.valor }}</dd>
        </div>
      </dl>
    </section>
  </div>
</template>

<style scoped>
.admin-page { display: flex; flex-direction: column; gap: 20px; }
.admin-page-header { display: flex; justify-content: space-between; align-items: flex-end; gap: 12px; flex-wrap: wrap; }
.admin-page-title { font-size: 24px; font-weight: 800; letter-spacing: -0.5px; color: var(--text-primary); }
.admin-page-sub { font-size: 13px; color: var(--text-secondary); margin-top: 2px; }
.admin-section { padding: 24px; display: flex; flex-direction: column; gap: 18px; max-width: 640px; }
.about-brand { display: flex; align-items: center; gap: 14px; border-bottom: 1px solid rgba(255,255,255,0.05); padding-bottom: 16px; }
.about-sym { font-size: 30px; background: linear-gradient(135deg, var(--primary), var(--accent-purple)); -webkit-background-clip: text; -webkit-text-fill-color: transparent; }
.about-brand h3 { font-size: 17px; font-weight: 700; color: var(--text-primary); }
.about-brand p { font-size: 12.5px; color: var(--text-secondary); }
.about-list { display: flex; flex-direction: column; gap: 10px; }
.about-row { display: flex; justify-content: space-between; gap: 16px; font-size: 13px; }
.about-row dt { color: var(--text-secondary); font-weight: 600; }
.about-row dd { color: var(--text-primary); text-align: right; }
</style>
