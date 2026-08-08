/**
 * Definição da navegação do ERP (menu lateral por módulo).
 *
 * Usada por `AppSidebar`/`AppSidebarGroup`/`AppSidebarItem`. As rotas apontam para
 * as páginas sob `pages/erp/*` que os agentes de tela irão construir. Manter esta lista
 * alinhada ao inventário de rotas da seção 1 do MAPA_FRONTEND.md.
 *
 * Módulos avançados (RH, Produção, Manutenção, Qualidade, Projetos, GRC, ESG,
 * Concessionárias, Imobiliária, Contabilidade, Financeiro Avançado): telas construídas
 * pela fábrica a partir das EFs + OpenAPI e agora expostas no menu. Sobem DESABILITADOS
 * por ABAC (nega por padrão); a visibilidade efetiva deve ser amarrada à habilitação por
 * plano/cliente na hora de liberar cada módulo.
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
      { label: 'Serviços', to: '/erp/cadastros/servicos' },
      { label: 'Reajustes de Preço', to: '/erp/cadastros/produtos-reajustes' }
    ]
  },
  {
    label: 'Vendas / Emissão',
    icon: 'file-invoice',
    itens: [
      { label: 'NF-e', to: '/erp/vendas/emissao/nfe' },
      { label: 'NF-e Simplificada', to: '/erp/vendas/emissao/nfe-simplificada' },
      { label: 'NFC-e', to: '/erp/vendas/emissao/nfce' },
      { label: 'NF-e Devolução/Retorno', to: '/erp/vendas/emissao/devolucao-retorno/nfe' },
      { label: 'Transmissões', to: '/erp/vendas/transmissoes' },
      { label: 'Inutilização', to: '/erp/vendas/inutilizacao-numeracao' }
    ]
  },
  {
    label: 'Vendas / Comercial',
    icon: 'file-invoice',
    itens: [
      { label: 'Comercial (hub)', to: '/erp/vendas/comercial' },
      { label: 'Contratos de Venda', to: '/erp/vendas/contratos' },
      { label: 'Planejamento de Demanda', to: '/erp/vendas/demanda' },
      { label: 'Logística de Saída', to: '/erp/vendas/logistica-saida' },
      { label: 'E-commerce', to: '/erp/vendas/ecommerce' },
      { label: 'Serviços — Catálogo', to: '/erp/vendas/servicos/catalogo' },
      { label: 'Serviços — Faturas', to: '/erp/vendas/servicos/faturas' },
      { label: 'Garantias — Coberturas', to: '/erp/vendas/garantias/coberturas' },
      { label: 'Garantias — Políticas', to: '/erp/vendas/garantias/politicas' },
      { label: 'FCI — Documentos', to: '/erp/vendas/faturamento-internacional/documentos' },
      { label: 'FCI — Impostos', to: '/erp/vendas/faturamento-internacional/impostos' },
      { label: 'Portal — Solicitações', to: '/erp/vendas/portal/solicitacoes' },
      { label: 'Portal — Usuários', to: '/erp/vendas/portal/usuarios' }
    ]
  },
  {
    label: 'Vendas / CRM',
    icon: 'users',
    itens: [
      { label: 'CRM (hub)', to: '/erp/vendas/crm' },
      { label: 'Leads', to: '/erp/vendas/crm/leads' },
      { label: 'Oportunidades', to: '/erp/vendas/crm/oportunidades' },
      { label: 'Atividades / Agenda', to: '/erp/vendas/crm/atividades' },
      { label: 'Campanhas', to: '/erp/vendas/crm/campanhas' },
      { label: 'Fidelização', to: '/erp/vendas/crm/fidelizacao' },
      { label: 'Tickets', to: '/erp/vendas/crm/tickets' },
      { label: 'Webforms', to: '/erp/vendas/crm/webforms' },
      { label: 'Pix Relacional', to: '/erp/vendas/crm/pix' }
    ]
  },
  {
    label: 'PDV',
    icon: 'pos',
    itens: [
      { label: 'Caixa', to: '/erp/pdv' },
      { label: 'Gestão de Caixa', to: '/erp/pdv/caixa' }
    ]
  },
  {
    label: 'Compras',
    icon: 'shopping-cart',
    itens: [
      { label: 'Lista de Compras', to: '/erp/compras' },
      { label: 'Gestão de Compras', to: '/erp/compras/gestao' },
      { label: 'Aprovações & Alçadas', to: '/erp/compras/aprovacoes' },
      { label: 'Sourcing — Cotações', to: '/erp/compras/sourcing' },
      { label: 'Contratos GCC', to: '/erp/compras/contratos-gcc' },
      { label: 'Entrada de Mercadorias', to: '/erp/compras/entrada-mercadorias' },
      { label: 'NF-e de Entrada', to: '/erp/compras/emissao/nfe-entrada' },
      { label: 'NF-e Entrada — Devolução/Retorno', to: '/erp/compras/emissao/devolucao-retorno/nfe-entrada' },
      { label: 'Devolução de Compra', to: '/erp/compras/devolucao' },
      { label: 'Subcontratação', to: '/erp/compras/subcontratacao' },
      { label: 'TMS — Frete', to: '/erp/compras/tms' },
      { label: 'Comércio Exterior', to: '/erp/compras/comercio-exterior' },
      { label: 'Importar XML', to: '/erp/integracao/importar-xml' },
      { label: 'Relatórios', to: '/erp/compras/relatorios' }
    ]
  },
  {
    label: 'Estoque',
    icon: 'package',
    itens: [
      { label: 'Produtos', to: '/erp/estoque/produtos' },
      { label: 'Saldo', to: '/erp/estoque/saldo' },
      { label: 'Movimento Manual', to: '/erp/estoque/movimento-manual' },
      { label: 'Transferências', to: '/erp/estoque/transferencias' },
      { label: 'Ajustes e Avarias', to: '/erp/estoque/ajustes' },
      { label: 'Inventários', to: '/erp/estoque/inventarios' },
      { label: 'Rastreabilidade', to: '/erp/estoque/rastreabilidade' },
      { label: 'WMS — Armazéns', to: '/erp/estoque/wms' },
      { label: 'Análise & Planejamento', to: '/erp/estoque/analise' },
      { label: 'Portal do Fornecedor', to: '/erp/estoque/portal-fornecedor' },
      { label: 'Logística de Entrada', to: '/erp/estoque/logistica-entrada' }
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
    itens: [
      { label: 'Central de Relatórios', to: '/erp/relatorios' },
      { label: 'BI — Painel Gerencial', to: '/erp/relatorios/bi/painel' },
      { label: 'Vendas Simplificado', to: '/erp/relatorios/vendas/simplificado01' },
      { label: 'Operacional — Vendas', to: '/erp/relatorios/operacionais/vendas' },
      { label: 'Operacional — Posição de Estoque', to: '/erp/relatorios/operacionais/posicao-estoque' },
      { label: 'Operacional — Giro de Estoque', to: '/erp/relatorios/operacionais/giro-estoque' },
      { label: 'Operacional — Fluxo de Caixa', to: '/erp/relatorios/operacionais/fluxo-caixa' },
      { label: 'Operacional — Aging CP/CR', to: '/erp/relatorios/operacionais/aging' }
    ]
  },
  {
    label: 'Configurações',
    icon: 'settings',
    itens: [
      { label: 'Certificado', to: '/erp/configuracoes/certificado' },
      { label: 'Usuários', to: '/erp/configuracoes/permissoes/usuarios' },
      { label: 'Perfis de Acesso', to: '/erp/configuracoes/permissoes/perfis' },
      { label: 'Preferências', to: '/erp/configuracoes/preferencias' },
      { label: 'Importações', to: '/erp/configuracoes/importacoes' }
    ]
  },

  // ===== Módulos avançados (sobem desabilitados por ABAC; liberar por plano/cliente) =====

  {
    label: 'Financeiro Avançado',
    icon: 'cash',
    itens: [
      { label: 'Tesouraria — Contas', to: '/erp/financeiro/tesouraria/contas' },
      { label: 'Tesouraria — Caixas', to: '/erp/financeiro/tesouraria/caixas' },
      { label: 'Tesouraria — Movimentos', to: '/erp/financeiro/tesouraria/movimentos' },
      { label: 'Tesouraria — Cheques', to: '/erp/financeiro/tesouraria/cheques' },
      { label: 'Tesouraria — Transferências', to: '/erp/financeiro/tesouraria/transferencias' },
      { label: 'Cartões', to: '/erp/financeiro/cartoes' },
      { label: 'Serviços Fin. — Faturas', to: '/erp/financeiro/servicos-financeiros/faturas' },
      { label: 'Serviços Fin. — Sacados', to: '/erp/financeiro/servicos-financeiros/sacados' },
      { label: 'Serviços Fin. — Boletos', to: '/erp/financeiro/servicos-financeiros/boletos' },
      { label: 'Serviços Fin. — Remessas', to: '/erp/financeiro/servicos-financeiros/remessas' },
      { label: 'Câmbio — Moedas', to: '/erp/financeiro/cambio-risco/moedas' },
      { label: 'Câmbio — Exposições', to: '/erp/financeiro/cambio-risco/exposicoes' },
      { label: 'Contratos — Planos', to: '/erp/financeiro/contratos-financeiros/planos' },
      { label: 'Contratos — Contratos', to: '/erp/financeiro/contratos-financeiros/contratos' },
      { label: 'Orçamento — Versões', to: '/erp/financeiro/planejamento-orcamento/versoes' },
      { label: 'Orçamento — Metas', to: '/erp/financeiro/planejamento-orcamento/metas' },
      { label: 'Subsídios & Fundos', to: '/erp/financeiro/subsidios-fundos/programas' }
    ]
  },
  {
    label: 'Contabilidade',
    icon: 'chart',
    itens: [
      { label: 'Plano de Contas', to: '/erp/contabilidade/contas' },
      { label: 'Lançamentos', to: '/erp/contabilidade/lancamentos' },
      { label: 'Períodos', to: '/erp/contabilidade/periodos' },
      { label: 'Saldos de Abertura', to: '/erp/contabilidade/saldos-abertura' },
      { label: 'Centros de Custo', to: '/erp/contabilidade/centros-custo' },
      { label: 'Dimensões', to: '/erp/contabilidade/dimensoes' },
      { label: 'Alocações', to: '/erp/contabilidade/alocacoes' },
      { label: 'Consolidação', to: '/erp/contabilidade/consolidacao' },
      { label: 'Ativos Fixos', to: '/erp/contabilidade/ativos-fixos' }
    ]
  },
  {
    label: 'RH',
    icon: 'users',
    itens: [
      { label: 'Colaboradores', to: '/erp/rh/colaboradores' },
      { label: 'Força de Trabalho', to: '/erp/rh/forca-trabalho/colaboradores' },
      { label: 'Folhas', to: '/erp/rh/folhas' },
      { label: 'Folha — Competências', to: '/erp/rh/folha/competencias' },
      { label: 'Folha — Rubricas', to: '/erp/rh/folha/rubricas' },
      { label: 'Ponto — Marcações', to: '/erp/rh/ponto/marcacoes' },
      { label: 'Ponto — Períodos', to: '/erp/rh/ponto/periodos' },
      { label: 'Recrutamento — Vagas', to: '/erp/rh/recrutamento/vagas' },
      { label: 'Recrutamento — Candidatos', to: '/erp/rh/recrutamento/candidatos' },
      { label: 'Treinamentos', to: '/erp/rh/treinamento/treinamentos' },
      { label: 'Talentos — Metas', to: '/erp/rh/talentos/metas' },
      { label: 'Talentos — Licenças', to: '/erp/rh/talentos/licencas' },
      { label: 'Desenvolvimento — Advertências', to: '/erp/rh/desenvolvimento/advertencias' },
      { label: 'Desenvolvimento — Promoções', to: '/erp/rh/desenvolvimento/promocoes' },
      { label: 'Planejamento — Turnos', to: '/erp/rh/planejamento/turnos' },
      { label: 'Planejamento — Headcount', to: '/erp/rh/planejamento/headcount' },
      { label: 'SSO — PPP', to: '/erp/rh/sso/ppp' },
      { label: 'Timesheets', to: '/erp/rh/timesheets' }
    ]
  },
  {
    label: 'Produção',
    icon: 'package',
    itens: [
      { label: 'Ordens de Produção', to: '/erp/producao/ordens' },
      { label: 'Ordens MES', to: '/erp/producao/ordens-mes' },
      { label: 'Fichas Técnicas', to: '/erp/producao/fichas' },
      { label: 'Estrutura (BOM)', to: '/erp/producao/bom-estruturas' },
      { label: 'BOM por SKU', to: '/erp/producao/bom' },
      { label: 'MRP', to: '/erp/producao/mrp' },
      { label: 'Planejamentos', to: '/erp/producao/planejamentos' },
      { label: 'Programações', to: '/erp/producao/programacoes' },
      { label: 'Estimativas', to: '/erp/producao/estimativas' },
      { label: 'Custos', to: '/erp/producao/custos' }
    ]
  },
  {
    label: 'Manutenção',
    icon: 'settings',
    itens: [
      { label: 'Equipamentos', to: '/erp/manutencao/equipamentos' },
      { label: 'Ordens de Manutenção', to: '/erp/manutencao/ordens' },
      { label: 'Ordens de Serviço', to: '/erp/manutencao/ordens-servico' },
      { label: 'Paradas', to: '/erp/manutencao/paradas' },
      { label: 'Peças de Reposição', to: '/erp/manutencao/pecas-reposicao' },
      { label: 'Preventiva — Planos', to: '/erp/manutencao/preventiva/planos' },
      { label: 'Preditiva — Monitoramentos', to: '/erp/manutencao/preditiva/monitoramentos' },
      { label: 'Preditiva — Alarmes', to: '/erp/manutencao/preditiva/alarmes' },
      { label: 'Confiabilidade — Revisões', to: '/erp/manutencao/confiabilidade/revisoes' },
      { label: 'Config — Induções', to: '/erp/manutencao/equipamentos-config/inducoes' }
    ]
  },
  {
    label: 'Qualidade',
    icon: 'report',
    itens: [
      { label: 'Não Conformidades (NCR)', to: '/erp/qualidade/ncr' },
      { label: 'Não Conformidades Ativas', to: '/erp/qualidade/nao-conformidades' },
      { label: 'Planos de Inspeção', to: '/erp/qualidade/planos-inspecao' },
      { label: 'Inspeções', to: '/erp/qualidade/inspecoes' },
      { label: 'Aceitação/Rejeição', to: '/erp/qualidade/aceitacao-rejeicao' },
      { label: 'Atributos', to: '/erp/qualidade/atributos' },
      { label: 'Administração', to: '/erp/qualidade/administracao' }
    ]
  },
  {
    label: 'Projetos',
    icon: 'chart',
    itens: [
      { label: 'Projetos', to: '/erp/projetos' },
      { label: 'Portfólio', to: '/erp/projetos/portfolio' },
      { label: 'Orçamento', to: '/erp/projetos/orcamento' },
      { label: 'Tarefas', to: '/erp/projetos/tarefas' },
      { label: 'Riscos', to: '/erp/projetos/riscos' },
      { label: 'Alocações', to: '/erp/projetos/alocacoes' },
      { label: 'Apontamentos', to: '/erp/projetos/apontamentos' },
      { label: 'Faturamento', to: '/erp/projetos/faturamento' },
      { label: 'Encerramento', to: '/erp/projetos/encerramento' }
    ]
  },
  {
    label: 'GRC',
    icon: 'report',
    itens: [
      { label: 'Riscos', to: '/erp/grc/riscos' },
      { label: 'Controles', to: '/erp/grc/controles' },
      { label: 'Incidentes', to: '/erp/grc/incidentes' },
      { label: 'Políticas', to: '/erp/grc/politicas' },
      { label: 'Denúncias', to: '/erp/grc/denuncias' },
      { label: 'Denúncias — Categorias', to: '/erp/grc/denuncia-categorias' },
      { label: 'Compliance — Registros', to: '/erp/grc/compliance-registros' },
      { label: 'Compliance — Certificados', to: '/erp/grc/compliance-certificados' },
      { label: 'Auditoria — Planos', to: '/erp/grc/auditoria-planos' },
      { label: 'Auditoria — Achados', to: '/erp/grc/auditoria-achados' },
      { label: 'SoD — Regras', to: '/erp/grc/sod-regras' },
      { label: 'SoD — Violações', to: '/erp/grc/sod-violacoes' }
    ]
  },
  {
    label: 'ESG',
    icon: 'chart',
    itens: [
      { label: 'Emissões', to: '/erp/esg/emissoes' },
      { label: 'GHG — Inventários', to: '/erp/esg/ghg/inventarios' },
      { label: 'GHG — Fatores', to: '/erp/esg/ghg/fatores' },
      { label: 'EHS — Registros', to: '/erp/esg/ehs/registros' },
      { label: 'EHS — Incidentes', to: '/erp/esg/ehs/incidentes' },
      { label: 'EHS — Licenças', to: '/erp/esg/ehs/licencas' },
      { label: 'Eco — Fluxos', to: '/erp/esg/eco/fluxos' },
      { label: 'Eco — Devoluções', to: '/erp/esg/eco/devolucoes' },
      { label: 'Relatórios', to: '/erp/esg/relatorios' },
      { label: 'Relatórios — Frameworks', to: '/erp/esg/relatorios/frameworks' }
    ]
  },
  {
    label: 'Concessionárias',
    icon: 'building',
    itens: [
      { label: 'Vendas — Estoque', to: '/erp/concessionarias/vendas/estoque' },
      { label: 'Vendas — Propostas', to: '/erp/concessionarias/vendas/propostas' },
      { label: 'Vendas — Reservas', to: '/erp/concessionarias/vendas/reservas' },
      { label: 'CRM — Oportunidades', to: '/erp/concessionarias/crm/oportunidades' },
      { label: 'CRM — Prospects', to: '/erp/concessionarias/crm/prospects' },
      { label: 'CRM — Test-drives', to: '/erp/concessionarias/crm/test-drives' },
      { label: 'Oficina — Ordens de Serviço', to: '/erp/concessionarias/manutencao/ordens-servico' },
      { label: 'Oficina — Orçamentos', to: '/erp/concessionarias/manutencao/orcamentos' },
      { label: 'Peças', to: '/erp/concessionarias/pecas/pecas' },
      { label: 'Garantias — Solicitações', to: '/erp/concessionarias/garantias/solicitacoes' },
      { label: 'Finanças — Simulações', to: '/erp/concessionarias/financas/simulacoes' },
      { label: 'Veículos', to: '/erp/concessionarias/veiculos' },
      { label: 'DMS — Vendas', to: '/erp/concessionarias/dms/vendas' },
      { label: 'DMS — Ordens de Serviço', to: '/erp/concessionarias/dms/ordens-servico' }
    ]
  },
  {
    label: 'Imobiliária',
    icon: 'building',
    itens: [
      { label: 'Gestão Imobiliária', to: '/erp/imobiliaria' },
      { label: 'Imóveis', to: '/erp/imobiliaria/imoveis' },
      { label: 'Propostas', to: '/erp/imobiliaria/propostas' },
      { label: 'Locações', to: '/erp/imobiliaria/locacoes' },
      { label: 'Contratos de Serviço', to: '/erp/imobiliaria/contratos-servico' }
    ]
  }
]
