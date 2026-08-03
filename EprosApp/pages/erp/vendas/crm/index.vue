<script setup lang="ts">
/**
 * CRM Comercial — hub de navegação dos submódulos (`/vendas/crm`).
 * Só apresentação; cada card leva a uma tela que consome o contrato real.
 */
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default', middleware: 'auth' })

const cards = [
  { to: '/erp/vendas/crm/leads', titulo: 'Leads', desc: 'Funil de captação e conversão.' },
  { to: '/erp/vendas/crm/oportunidades', titulo: 'Oportunidades', desc: 'Pipeline de vendas (ganhar/perder).' },
  { to: '/erp/vendas/crm/campanhas', titulo: 'Campanhas', desc: 'Campanhas de marketing.' },
  { to: '/erp/vendas/crm/tickets', titulo: 'Atendimento', desc: 'Tickets e respostas.' },
  { to: '/erp/vendas/crm/atividades', titulo: 'Atividades', desc: 'Agenda, tarefas e chamadas.' },
  { to: '/erp/vendas/crm/webforms', titulo: 'Webforms', desc: 'Formulários de captação.' },
  { to: '/erp/vendas/crm/fidelizacao', titulo: 'Fidelização', desc: 'Clientes fidelizados e pontos.' },
  { to: '/erp/vendas/crm/pix', titulo: 'Pix relacional', desc: 'Configuração e cobranças Pix.' }
]
</script>

<template>
  <div>
    <PageToolbar title="CRM Comercial" subtitle="Gestão de relacionamento e funil de vendas" />
    <div class="hub-grid">
      <NuxtLink v-for="c in cards" :key="c.to" :to="c.to" class="glass-panel hub-card">
        <strong class="hub-titulo">{{ c.titulo }}</strong>
        <span class="hub-desc">{{ c.desc }}</span>
      </NuxtLink>
    </div>
  </div>
</template>

<style scoped>
.hub-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 12px; margin-top: 8px; }
.hub-card { padding: 18px; display: flex; flex-direction: column; gap: 6px; text-decoration: none; color: inherit; transition: border-color 0.15s ease, transform 0.15s ease; }
.hub-card:hover { border-color: var(--primary); transform: translateY(-2px); }
.hub-titulo { font-size: 16px; }
.hub-desc { font-size: 13px; color: var(--text-secondary); }
</style>
