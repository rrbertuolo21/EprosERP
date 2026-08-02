<script setup lang="ts">
/**
 * Vendas — hub Comercial. Ponto de entrada dos submódulos comerciais de Vendas
 * (o menu lateral ainda não lista estas telas — ver relatório). Só apresentação.
 */
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default', middleware: 'auth' })

const grupos = [
  {
    titulo: 'CRM Comercial',
    cards: [{ to: '/erp/vendas/crm', titulo: 'CRM', desc: 'Leads, oportunidades, campanhas, tickets, Pix.' }]
  },
  {
    titulo: 'Operação comercial',
    cards: [
      { to: '/erp/vendas/servicos/catalogo', titulo: 'Serviços — catálogo', desc: 'Serviços comerciais.' },
      { to: '/erp/vendas/servicos/faturas', titulo: 'Serviços — faturas', desc: 'Faturamento de serviços.' },
      { to: '/erp/vendas/contratos', titulo: 'Contratos de venda', desc: 'Ciclo de vida dos contratos.' },
      { to: '/erp/vendas/logistica-saida', titulo: 'Logística de saída', desc: 'Expedições e entregas.' },
      { to: '/erp/vendas/demanda', titulo: 'Planejamento de demanda', desc: 'Previsões de venda.' }
    ]
  },
  {
    titulo: 'Canais e pós-venda',
    cards: [
      { to: '/erp/vendas/portal/usuarios', titulo: 'Portal — usuários', desc: 'Acessos dos clientes.' },
      { to: '/erp/vendas/portal/solicitacoes', titulo: 'Portal — solicitações', desc: 'Atendimento do portal.' },
      { to: '/erp/vendas/ecommerce', titulo: 'Comércio eletrônico', desc: 'Loja, clientes e cupons.' },
      { to: '/erp/vendas/garantias/politicas', titulo: 'Garantias — políticas', desc: 'Políticas de RMA.' },
      { to: '/erp/vendas/garantias/coberturas', titulo: 'Garantias — coberturas', desc: 'Coberturas aplicadas.' }
    ]
  },
  {
    titulo: 'Internacional',
    cards: [
      { to: '/erp/vendas/faturamento-internacional/documentos', titulo: 'FCI — documentos', desc: 'Faturamento internacional.' },
      { to: '/erp/vendas/faturamento-internacional/impostos', titulo: 'FCI — impostos', desc: 'Impostos aplicáveis.' }
    ]
  }
]
</script>

<template>
  <div>
    <PageToolbar title="Vendas — Comercial" subtitle="Submódulos comerciais, canais e pós-venda" />
    <section v-for="g in grupos" :key="g.titulo" class="grupo">
      <h3 class="grupo-titulo">{{ g.titulo }}</h3>
      <div class="hub-grid">
        <NuxtLink v-for="c in g.cards" :key="c.to" :to="c.to" class="glass-panel hub-card">
          <strong class="hub-titulo">{{ c.titulo }}</strong>
          <span class="hub-desc">{{ c.desc }}</span>
        </NuxtLink>
      </div>
    </section>
  </div>
</template>

<style scoped>
.grupo { margin-top: 18px; }
.grupo-titulo { font-size: 14px; text-transform: uppercase; letter-spacing: 0.03em; color: var(--text-secondary); margin-bottom: 10px; }
.hub-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 12px; }
.hub-card { padding: 18px; display: flex; flex-direction: column; gap: 6px; text-decoration: none; color: inherit; transition: border-color 0.15s ease, transform 0.15s ease; }
.hub-card:hover { border-color: var(--primary); transform: translateY(-2px); }
.hub-titulo { font-size: 16px; }
.hub-desc { font-size: 13px; color: var(--text-secondary); }
</style>
