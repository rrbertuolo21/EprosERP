<script setup lang="ts">
/**
 * Hub da Gestão Imobiliária (IMO-001).
 *
 * Ponto de entrada dos cadastros e fluxos do submódulo: imóveis, contratos de serviço,
 * locações (com gestão de ciclo/financeiro dentro do contrato) e propostas. Só navegação.
 */
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

const links = [
  { titulo: 'Imóveis', descricao: 'Cadastro e ciclo de vida dos imóveis.', rota: '/erp/imobiliaria/imoveis', icone: '🏠' },
  { titulo: 'Locações', descricao: 'Contratos de aluguel: período, valor, partes e gestão do contrato.', rota: '/erp/imobiliaria/locacoes', icone: '📄' },
  { titulo: 'Propostas', descricao: 'Propostas de locação/aquisição: aprovar, rejeitar e converter.', rota: '/erp/imobiliaria/propostas', icone: '✍️' },
  { titulo: 'Contratos de serviço', descricao: 'Administração e serviços vinculados.', rota: '/erp/imobiliaria/contratos-servico', icone: '🧾' }
]
</script>

<template>
  <div>
    <PageToolbar title="Gestão Imobiliária" subtitle="Imóveis, locações, propostas e gestão de contratos" />
    <div class="cards-grid">
      <NuxtLink v-for="l in links" :key="l.rota" :to="l.rota" class="imo-card glass-panel">
        <span class="imo-icone">{{ l.icone }}</span>
        <span class="imo-titulo">{{ l.titulo }}</span>
        <span class="imo-desc">{{ l.descricao }}</span>
      </NuxtLink>
    </div>
  </div>
</template>

<style scoped>
.cards-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 16px; }
.imo-card {
  display: flex; flex-direction: column; gap: 6px; padding: 20px; text-decoration: none; color: inherit;
  border-left: 3px solid var(--primary); transition: transform 0.15s ease, box-shadow 0.15s ease;
}
.imo-card:hover { transform: translateY(-2px); box-shadow: 0 6px 20px rgba(0,0,0,0.12); }
.imo-icone { font-size: 24px; }
.imo-titulo { font-size: 15px; font-weight: 700; color: var(--text-primary); }
.imo-desc { font-size: 13px; color: var(--text-secondary); line-height: 1.4; }
</style>
