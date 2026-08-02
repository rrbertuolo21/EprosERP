<script setup lang="ts">
/**
 * Hub de RELATÓRIOS — ponto de entrada dos módulos RPT.
 *
 * Agrupa os relatórios operacionais (RPT-OPB, só leitura) e o painel gerencial de BI
 * (RPT-ONM). Cada card leva à respectiva tela. Sem IO: apenas navegação.
 */
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface RelatorioLink {
  titulo: string
  descricao: string
  rota: string
  icone: string
}
interface Grupo {
  nome: string
  itens: RelatorioLink[]
}

const grupos: Grupo[] = [
  {
    nome: 'Estoque',
    itens: [
      { titulo: 'Posição de estoque', descricao: 'Saldo, mínimo, reservado e valor por produto.', rota: '/erp/relatorios/operacionais/posicao-estoque', icone: '📦' },
      { titulo: 'Giro de estoque', descricao: 'Saídas sobre saldo por produto no período.', rota: '/erp/relatorios/operacionais/giro-estoque', icone: '🔄' }
    ]
  },
  {
    nome: 'Vendas',
    itens: [
      { titulo: 'Vendas', descricao: 'Por período, por produto e por cliente.', rota: '/erp/relatorios/operacionais/vendas', icone: '🛒' },
      { titulo: 'Vendas simplificado (Excel)', descricao: 'Exportação de vendas do período em planilha.', rota: '/erp/relatorios/vendas/simplificado01', icone: '⬇️' }
    ]
  },
  {
    nome: 'Financeiro',
    itens: [
      { titulo: 'Aging CP/CR', descricao: 'Faixas de vencimento de contas a pagar e a receber.', rota: '/erp/relatorios/operacionais/aging', icone: '⏳' },
      { titulo: 'Fluxo de caixa', descricao: 'Entradas x saídas por dia no período.', rota: '/erp/relatorios/operacionais/fluxo-caixa', icone: '💧' }
    ]
  },
  {
    nome: 'BI / Analytics',
    itens: [
      { titulo: 'Painel gerencial', descricao: 'KPIs de faturamento, margem, inadimplência e ruptura + série e top produtos.', rota: '/erp/relatorios/bi/painel', icone: '📊' }
    ]
  }
]
</script>

<template>
  <div>
    <PageToolbar title="Relatórios" subtitle="Relatórios operacionais e painel gerencial (BI)" />

    <section v-for="grupo in grupos" :key="grupo.nome" class="grupo">
      <h3 class="grupo-titulo">{{ grupo.nome }}</h3>
      <div class="cards-grid">
        <NuxtLink v-for="item in grupo.itens" :key="item.rota" :to="item.rota" class="rel-card glass-panel">
          <span class="rel-icone">{{ item.icone }}</span>
          <span class="rel-titulo">{{ item.titulo }}</span>
          <span class="rel-desc">{{ item.descricao }}</span>
        </NuxtLink>
      </div>
    </section>
  </div>
</template>

<style scoped>
.grupo { margin-bottom: 26px; }
.grupo-titulo {
  font-size: 13px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-secondary);
  margin: 0 0 12px;
}
.cards-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 16px;
}
.rel-card {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 20px;
  text-decoration: none;
  color: inherit;
  border-left: 3px solid var(--primary);
  transition: transform 0.15s ease, box-shadow 0.15s ease;
}
.rel-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.12);
}
.rel-icone { font-size: 24px; }
.rel-titulo { font-size: 15px; font-weight: 700; color: var(--text-primary); }
.rel-desc { font-size: 13px; color: var(--text-secondary); line-height: 1.4; }
</style>
