/**
 * Definição da navegação do ERP (menu lateral por módulo).
 *
 * Usada por `AppSidebar`/`AppSidebarGroup`/`AppSidebarItem`. As rotas apontam para
 * as páginas sob `pages/erp/*` que os agentes de tela irão construir. Manter esta lista
 * alinhada ao inventário de rotas da seção 1 do MAPA_FRONTEND.md.
 *
 * QUARENTENA pós-cutover: módulos novos (não existem no legado) — reexpor depois.
 * Produção, Qualidade, RH, Manutenção, Projetos, DMS, GRC e ESG existem apenas como
 * módulos de API no backend (Epros.Modules.*) e NÃO possuem — e não devem ganhar —
 * entradas neste menu nem páginas em `pages/erp/*` antes do cutover. Ver MAPA_FRONTEND.md
 * linha 32/287. Não adicionar itens para esses módulos aqui sem antes remover esta nota.
 */

import type { AppIconName } from './AppIcon.vue'

export interface MenuItem {
  label: string
  to: string
  icon?: AppIconName
}

export interface MenuGroup {
  label: string
  icon?: AppIconName
  itens: MenuItem[]
}

export const erpMenu: MenuGroup[] = [
  {
    label: 'Cadastros',
    icon: 'address-book',
    itens: [
      { label: 'Parceiros', to: '/erp/cadastros/parceiros' },
      { label: 'Produtos', to: '/erp/cadastros/produtos' },
      { label: 'Empresas', to: '/erp/cadastros/empresas' },
      { label: 'Contadores', to: '/erp/cadastros/contadores' },
      { label: 'Serviços', to: '/erp/cadastros/servicos' }
    ]
  },
  {
    label: 'Vendas / Emissão',
    icon: 'file-invoice',
    itens: [
      { label: 'NF-e', to: '/erp/vendas/emissao/nfe' },
      { label: 'NF-e Simplificada', to: '/erp/vendas/emissao/nfe-simplificada' },
      { label: 'NFC-e', to: '/erp/vendas/emissao/nfce' },
      { label: 'Transmissões', to: '/erp/vendas/transmissoes' },
      { label: 'Inutilização', to: '/erp/vendas/inutilizacao-numeracao' }
    ]
  },
  {
    label: 'PDV',
    icon: 'pos',
    itens: [{ label: 'Caixa', to: '/erp/pdv' }]
  },
  {
    label: 'Compras',
    icon: 'shopping-cart',
    itens: [
      { label: 'Lista de Compras', to: '/erp/compras' },
      { label: 'Entrada de Mercadorias', to: '/erp/compras/entrada-mercadorias' },
      { label: 'Importar XML', to: '/erp/integracao/importar-xml' }
    ]
  },
  {
    label: 'Estoque',
    icon: 'package',
    itens: [
      { label: 'Produtos', to: '/erp/estoque/produtos' },
      { label: 'Movimento Manual', to: '/erp/estoque/movimento-manual' }
    ]
  },
  {
    label: 'Financeiro',
    icon: 'cash',
    itens: [
      { label: 'Contas a Receber', to: '/erp/financeiro/contas-a-receber' },
      { label: 'Contas a Pagar', to: '/erp/financeiro/contas-a-pagar' },
      { label: 'Bancos', to: '/erp/financeiro/bancos' },
      { label: 'Contas Bancárias', to: '/erp/financeiro/conta-bancaria' },
      { label: 'Natureza Financeira', to: '/erp/financeiro/natureza-financeira' },
      { label: 'Plano de Contas', to: '/erp/financeiro/plano-de-contas' }
    ]
  },
  {
    label: 'Fiscal',
    icon: 'chart',
    itens: [
      { label: 'CFOP', to: '/erp/fiscal/cfop' },
      { label: 'Tipo de Operação', to: '/erp/fiscal/tipo-operacao-fiscal' },
      { label: 'NCM', to: '/erp/fiscal/ncm' },
      { label: 'NCM Tributação', to: '/erp/fiscal/ncm-tributacao' },
      { label: 'ICMS Interestadual', to: '/erp/fiscal/icms-interestadual' },
      { label: 'Benefício Fiscal', to: '/erp/fiscal/codigo-beneficio-fiscal' },
      { label: 'Observações NF-e', to: '/erp/fiscal/observacoes-nfe' },
      { label: 'XML Contador', to: '/erp/fiscal/xml-contador' }
    ]
  },
  {
    label: 'Relatórios',
    icon: 'report',
    itens: [{ label: 'Vendas Simplificado', to: '/erp/relatorios/vendas/simplificado01' }]
  },
  {
    label: 'Configurações',
    icon: 'settings',
    itens: [
      { label: 'Certificado', to: '/erp/configuracoes/certificado' },
      { label: 'Usuários', to: '/erp/configuracoes/permissoes/usuarios' },
      { label: 'Perfis de Acesso', to: '/erp/configuracoes/permissoes/perfis' }
    ]
  }
]
