<script setup lang="ts">
/**
 * Acesso Rápido — grade de atalhos para os módulos mais usados do ERP.
 *
 * Porta o comportamento de `acesso-rapido.vue` do legado: cartões de navegação rápida
 * para telas de emissão, cadastros, financeiro e fiscal. Puramente de navegação —
 * sem chamadas de API além da sessão já carregada pelo layout padrão.
 */
import { computed } from 'vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppIcon, { type AppIconName } from '~/components/AppIcon.vue'
import { useAuth } from '~/composables/useAuth'
import { useTenant } from '~/composables/useTenant'

definePageMeta({ layout: 'default' })

interface AtalhoAcessoRapido {
  titulo: string
  rota: string
  icone: AppIconName
}

const itensAcessoRapido: AtalhoAcessoRapido[] = [
  { titulo: 'Vendas NF-e Simplificada', rota: '/erp/vendas/emissao/nfe-simplificada', icone: 'invoice' },
  { titulo: 'Vendas NFC-e', rota: '/erp/vendas/emissao/nfce', icone: 'receipt' },
  { titulo: 'Compras', rota: '/erp/compras', icone: 'cart' },
  { titulo: 'Notas fiscais', rota: '/erp/vendas/transmissoes', icone: 'document' },
  { titulo: 'Produtos', rota: '/erp/cadastros/produtos', icone: 'box' },
  { titulo: 'Clientes', rota: '/erp/cadastros/parceiros?tipo=cliente', icone: 'users' },
  { titulo: 'Recebimentos', rota: '/erp/financeiro/contas-a-receber', icone: 'wallet-in' },
  { titulo: 'Pagamentos', rota: '/erp/financeiro/contas-a-pagar', icone: 'wallet-out' },
  { titulo: 'Empresas', rota: '/erp/cadastros/empresas', icone: 'building' },
  { titulo: 'Fornecedores', rota: '/erp/cadastros/parceiros?tipo=fornecedor', icone: 'truck' },
  { titulo: 'Estoque', rota: '/erp/estoque/produtos', icone: 'chart' },
  { titulo: 'PDV', rota: '/erp/pdv', icone: 'pos' }
]

// Saudação + contexto de sessão (usuário e empresa ativa), lidos dos composables
// existentes — sem inventar store nova.
const { getUser } = useAuth()
const { empresaAtiva } = useTenant()

const usuario = computed(() => getUser())

const saudacao = computed(() => {
  const hora = new Date().getHours()
  if (hora < 12) return 'Bom dia'
  if (hora < 18) return 'Boa tarde'
  return 'Boa noite'
})

const nomeUsuario = computed(() => {
  const u = usuario.value
  if (!u) return ''
  const nome = (u.nome as string | undefined) ?? u.email ?? ''
  return typeof nome === 'string' ? nome.split('@')[0] : ''
})
</script>

<template>
  <div>
    <PageToolbar title="Acesso rápido" subtitle="Atalhos para os módulos mais usados do sistema" />

    <section class="welcome-banner glass-panel">
      <div class="welcome-text">
        <h2 class="welcome-title">
          {{ saudacao }}<template v-if="nomeUsuario">, {{ nomeUsuario }}</template>
        </h2>
        <p class="welcome-sub">Aqui está o resumo rápido para continuar de onde parou.</p>
      </div>
      <div v-if="empresaAtiva" class="welcome-empresa">
        <AppIcon name="building" :size="18" />
        <span>{{ empresaAtiva.razaoSocial ?? empresaAtiva.nome }}</span>
      </div>
    </section>

    <div class="atalhos-grid">
      <NuxtLink v-for="item in itensAcessoRapido" :key="item.rota + item.titulo" :to="item.rota" class="atalho-card glass-panel">
        <span class="atalho-icone">
          <AppIcon :name="item.icone" :size="26" />
        </span>
        <span class="atalho-titulo">{{ item.titulo }}</span>
      </NuxtLink>
    </div>
  </div>
</template>

<style scoped>
.welcome-banner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 20px 24px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}
.welcome-title {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-primary);
}
.welcome-sub {
  margin-top: 4px;
  font-size: 13px;
  color: var(--text-secondary);
}
.welcome-empresa {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 14px;
  border-radius: 10px;
  background: var(--surface-raised);
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
  font-size: 13px;
  font-weight: 600;
}

.atalhos-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 16px;
}
.atalho-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 28px 16px;
  text-decoration: none;
  color: var(--text-primary);
  transition: transform 0.2s ease, border-color 0.2s ease;
  min-height: 120px;
}
.atalho-card:hover {
  transform: translateY(-2px);
  border-color: var(--primary);
}
.atalho-icone {
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--primary);
}
.atalho-titulo {
  font-size: 13px;
  font-weight: 600;
  text-align: center;
}
</style>
