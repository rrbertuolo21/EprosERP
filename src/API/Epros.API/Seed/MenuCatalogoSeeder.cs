using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Epros.API.Seed
{
    /// <summary>
    /// 1.10 (PERMISSOES_DE_MENU) — semeador do CATÁLOGO DE MENU real com <c>CapacidadeRequerida</c> preenchida.
    ///
    /// A 1.10 deixou o mecanismo pronto (o menu é PROJEÇÃO das capacidades efetivas), mas sem itens semeados com
    /// capacidade o <c>GET /api/v1/menu</c> volta vazio e o front cai no fallback estático (LC-3). Este seeder
    /// materializa a árvore de navegação do ERP (espelho do <c>erpMenu</c> do front) como <see cref="Menu"/> +
    /// <see cref="MenuItemNivel1"/>, e amarra cada folha à capacidade <c>recurso:acao</c> do seu módulo — a MESMA
    /// chave que o <c>AbacFilter</c> cobra —, de modo que o menu dinâmico passe a refletir o RBAC (menu ⇔ gate).
    ///
    /// Resolução de capacidade: o recurso de cada folha é resolvido contra o catálogo AUTORITATIVO descoberto por
    /// <see cref="CapacidadeCatalogoSeeder.DescobrirCapacidades"/> (os <c>[AbacAuthorize]</c> dos controllers),
    /// escolhendo a ação de leitura existente (ler/consultar/visualizar/listar/acessar). Assim nunca semeamos uma
    /// capacidade que o gate não conheça. Onde não há capacidade óbvia (ex.: telas de CRUD legadas que ainda não
    /// adotaram <c>[AbacAuthorize]</c>), a folha fica com <c>CapacidadeRequerida = null</c> — item de plataforma
    /// sem gate PRÓPRIO: pela projeção "nega por padrão" (REG-002), só aparece se tiver filho visível. Esses casos
    /// estão documentados abaixo (Recurso: null) e ficam pendentes de o controller correspondente ganhar o gate.
    ///
    /// Idempotente: Menu é chaveado por Descrição; item nível 1 por (MenuId, Descrição). Rodar N vezes não duplica;
    /// se um item já existe com capacidade nula e agora há capacidade resolvível, ela é preenchida (backfill).
    /// Módulo (entitlement de plano) fica nulo no catálogo semeado: a visibilidade por plano continua a cargo do
    /// gate/ModuloTenantMiddleware; o filtro de módulo no menu é refinamento futuro.
    /// </summary>
    public static class MenuCatalogoSeeder
    {
        // Ações de leitura, em ordem de preferência, para resolver a capacidade "de ver" um recurso.
        private static readonly string[] AcoesLeitura = { "Ler", "Consultar", "Visualizar", "Listar", "Acessar" };

        private sealed record LeafDef(string Descricao, string To, string? Recurso);
        private sealed record GroupDef(string Descricao, string Icon, LeafDef[] Itens);

        /// <summary>
        /// Semeia o catálogo de menu com capacidades resolvidas. Idempotente. Retorna quantas folhas ficaram com
        /// capacidade preenchida (para observabilidade). <paramref name="controllersAssembly"/> default = a API.
        /// </summary>
        public static async Task<int> SeedAsync(ContextGestaoClientes ctx, Assembly? controllersAssembly = null, CancellationToken ct = default)
        {
            // 1) Catálogo autoritativo de capacidades (recurso:acao) descoberto dos controllers.
            var descobertas = CapacidadeCatalogoSeeder.DescobrirCapacidades(controllersAssembly);
            var nomesCapacidades = new HashSet<string>(
                descobertas.Select(p => CapacidadeCatalogoSeeder.NomeCapacidade(p.Recurso, p.Acao)),
                StringComparer.OrdinalIgnoreCase);

            string? ResolverCap(string? recurso)
            {
                if (string.IsNullOrWhiteSpace(recurso)) return null;
                foreach (var acao in AcoesLeitura)
                {
                    var nome = CapacidadeCatalogoSeeder.NomeCapacidade(recurso, acao);
                    if (nomesCapacidades.Contains(nome)) return nome;
                }
                return null; // recurso sem capacidade de leitura no catálogo → folha sem gate próprio (documentado).
            }

            // 2) Estado atual do catálogo (Menu é IGlobalEntity, sem filtro de tenant; sem soft-delete).
            var menusExistentes = await ctx.Menus
                .Include(m => m.ItensNivel1)
                .ToListAsync(ct);
            var menusPorDescricao = menusExistentes.ToDictionary(m => m.Descricao, StringComparer.OrdinalIgnoreCase);

            var capacidadesPreenchidas = 0;
            var ordemGrupo = 0;

            foreach (var grupo in ArvoreMenu)
            {
                ordemGrupo++;

                if (!menusPorDescricao.TryGetValue(grupo.Descricao, out var menu))
                {
                    menu = new Menu(grupo.Descricao, grupo.Icon, to: null, ordem: ordemGrupo, modulo: null, capacidadeRequerida: null);
                    ctx.Menus.Add(menu);
                    await ctx.SaveChangesAsync(ct); // materializa o Id para ligar os itens nível 1.
                    menusPorDescricao[grupo.Descricao] = menu;
                }

                var itensExistentes = menu.ItensNivel1
                    .GroupBy(i => i.Descricao, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var ordemItem = 0;
                foreach (var leaf in grupo.Itens)
                {
                    ordemItem++;
                    var cap = ResolverCap(leaf.Recurso);
                    if (cap != null) capacidadesPreenchidas++;

                    if (!itensExistentes.TryGetValue(leaf.Descricao, out var item))
                    {
                        var novo = new MenuItemNivel1(menu.Id, leaf.Descricao, icon: null, to: leaf.To, ordem: ordemItem, modulo: null, capacidadeRequerida: cap);
                        ctx.MenusItensNivel1.Add(novo);
                    }
                    else if (item.CapacidadeRequerida == null && cap != null)
                    {
                        // Backfill: item pré-existente sem capacidade agora recebe a capacidade resolvida.
                        item.Alterar(item.Descricao, item.Icon, item.To ?? leaf.To, item.Ordem, item.Modulo, cap);
                    }
                }
            }

            await ctx.SaveChangesAsync(ct);
            Log.Information("[SeedMenu] Catálogo de menu semeado ({Grupos} grupos, {Caps} folhas com capacidade).",
                ArvoreMenu.Count, capacidadesPreenchidas);
            return capacidadesPreenchidas;
        }

        /// <summary>
        /// Árvore de navegação (espelho de <c>EprosApp/components/menu.ts</c>). Cada folha declara o RECURSO do
        /// seu módulo; a AÇÃO de leitura é resolvida em runtime contra o catálogo descoberto. Recurso null =
        /// tela de plataforma/legada sem capacidade própria ainda (documentado, fica sem gate próprio no menu).
        /// </summary>
        private static readonly IReadOnlyList<GroupDef> ArvoreMenu = new List<GroupDef>
        {
            new("Cadastros", "address-book", new[]
            {
                new LeafDef("Parceiros", "/erp/cadastros/parceiros", null),          // Pessoa/Parceiro: CRUD legado sem [AbacAuthorize].
                new LeafDef("Produtos", "/erp/cadastros/produtos", null),             // Produto: CRUD legado.
                new LeafDef("Empresas", "/erp/cadastros/empresas", "Empresa"),
                new LeafDef("Contadores", "/erp/cadastros/contadores", null),         // Contador: CRUD legado.
                new LeafDef("Serviços", "/erp/cadastros/servicos", null),             // Serviço: CRUD legado.
            }),
            new("Vendas / Emissão", "file-invoice", new[]
            {
                new LeafDef("NF-e", "/erp/vendas/emissao/nfe", "DocumentosFiscais"),
                new LeafDef("NF-e Simplificada", "/erp/vendas/emissao/nfe-simplificada", "DocumentosFiscais"),
                new LeafDef("NFC-e", "/erp/vendas/emissao/nfce", "DocumentosFiscais"),
                new LeafDef("Transmissões", "/erp/vendas/transmissoes", "DocumentosFiscais"),
                new LeafDef("Inutilização", "/erp/vendas/inutilizacao-numeracao", "DocumentosFiscais"),
            }),
            new("PDV", "pos", new[]
            {
                new LeafDef("Caixa", "/erp/pdv", null),                               // PDV/Caixa: sem recurso ABAC dedicado ainda.
            }),
            new("Compras", "shopping-cart", new[]
            {
                new LeafDef("Lista de Compras", "/erp/compras", null),
                new LeafDef("Entrada de Mercadorias", "/erp/compras/entrada-mercadorias", null),
                new LeafDef("Importar XML", "/erp/integracao/importar-xml", null),
            }),
            new("Estoque", "package", new[]
            {
                new LeafDef("Produtos", "/erp/estoque/produtos", "EstoqueSaldo"),
                new LeafDef("Movimento Manual", "/erp/estoque/movimento-manual", "EstoqueMovimentacao"),
            }),
            new("Financeiro", "cash", new[]
            {
                new LeafDef("Contas a Receber", "/erp/financeiro/contas-a-receber", "ContasReceber"),
                new LeafDef("Contas a Pagar", "/erp/financeiro/contas-a-pagar", "ContasPagar"),
                new LeafDef("Bancos", "/erp/financeiro/bancos", null),
                new LeafDef("Contas Bancárias", "/erp/financeiro/conta-bancaria", null),
                new LeafDef("Natureza Financeira", "/erp/financeiro/natureza-financeira", null),
                new LeafDef("Plano de Contas", "/erp/financeiro/plano-de-contas", null),
            }),
            new("Fiscal", "chart", new[]
            {
                new LeafDef("CFOP", "/erp/fiscal/cfop", null),
                new LeafDef("Tipo de Operação", "/erp/fiscal/tipo-operacao-fiscal", null),
                new LeafDef("NCM", "/erp/fiscal/ncm", null),
                new LeafDef("NCM Tributação", "/erp/fiscal/ncm-tributacao", null),
                new LeafDef("ICMS Interestadual", "/erp/fiscal/icms-interestadual", null),
                new LeafDef("Benefício Fiscal", "/erp/fiscal/codigo-beneficio-fiscal", null),
                new LeafDef("Observações NF-e", "/erp/fiscal/observacoes-nfe", null),
                new LeafDef("XML Contador", "/erp/fiscal/xml-contador", null),
            }),
            new("Relatórios", "report", new[]
            {
                new LeafDef("Vendas Simplificado", "/erp/relatorios/vendas/simplificado01", null),
            }),
            new("Configurações", "settings", new[]
            {
                new LeafDef("Certificado", "/erp/configuracoes/certificado", null),
                new LeafDef("Usuários", "/erp/configuracoes/permissoes/usuarios", null), // gestão de usuários internos: gate próprio ainda ausente.
                new LeafDef("Perfis de Acesso", "/erp/configuracoes/permissoes/perfis", "PerfilAcesso"),
            }),

            // ===== Módulos avançados =====
            new("Financeiro Avançado", "cash", new[]
            {
                new LeafDef("Tesouraria — Contas", "/erp/financeiro/tesouraria/contas", "Tesouraria"),
                new LeafDef("Tesouraria — Caixas", "/erp/financeiro/tesouraria/caixas", "Tesouraria"),
                new LeafDef("Tesouraria — Movimentos", "/erp/financeiro/tesouraria/movimentos", "Tesouraria"),
                new LeafDef("Tesouraria — Cheques", "/erp/financeiro/tesouraria/cheques", "Tesouraria"),
                new LeafDef("Tesouraria — Transferências", "/erp/financeiro/tesouraria/transferencias", "Tesouraria"),
                new LeafDef("Cartões", "/erp/financeiro/cartoes", null),
                new LeafDef("Serviços Fin. — Faturas", "/erp/financeiro/servicos-financeiros/faturas", "ServicosFinanceiros"),
                new LeafDef("Serviços Fin. — Sacados", "/erp/financeiro/servicos-financeiros/sacados", "ServicosFinanceiros"),
                new LeafDef("Serviços Fin. — Boletos", "/erp/financeiro/servicos-financeiros/boletos", "ServicosFinanceiros"),
                new LeafDef("Serviços Fin. — Remessas", "/erp/financeiro/servicos-financeiros/remessas", "ServicosFinanceiros"),
                new LeafDef("Câmbio — Moedas", "/erp/financeiro/cambio-risco/moedas", "CambioRisco"),
                new LeafDef("Câmbio — Exposições", "/erp/financeiro/cambio-risco/exposicoes", "CambioRisco"),
                new LeafDef("Contratos — Planos", "/erp/financeiro/contratos-financeiros/planos", "ContratosFinanceiros"),
                new LeafDef("Contratos — Contratos", "/erp/financeiro/contratos-financeiros/contratos", "ContratosFinanceiros"),
                new LeafDef("Orçamento — Versões", "/erp/financeiro/planejamento-orcamento/versoes", "PlanejamentoOrcamento"),
                new LeafDef("Orçamento — Metas", "/erp/financeiro/planejamento-orcamento/metas", "PlanejamentoOrcamento"),
                new LeafDef("Subsídios & Fundos", "/erp/financeiro/subsidios-fundos/programas", "SubsidiosFundos"),
            }),
            new("Contabilidade", "chart", new[]
            {
                new LeafDef("Plano de Contas", "/erp/contabilidade/contas", "ContabilidadeGeral"),
                new LeafDef("Lançamentos", "/erp/contabilidade/lancamentos", "ContabilidadeGeral"),
                new LeafDef("Períodos", "/erp/contabilidade/periodos", "ContabilidadeGeral"),
                new LeafDef("Saldos de Abertura", "/erp/contabilidade/saldos-abertura", "ContabilidadeGeral"),
                new LeafDef("Centros de Custo", "/erp/contabilidade/centros-custo", "ContabilidadeGerencial"),
                new LeafDef("Dimensões", "/erp/contabilidade/dimensoes", "ContabilidadeGerencial"),
                new LeafDef("Alocações", "/erp/contabilidade/alocacoes", "ContabilidadeGerencial"),
                new LeafDef("Consolidação", "/erp/contabilidade/consolidacao", "Consolidacao"),
            }),
            new("RH", "users", new[]
            {
                new LeafDef("Colaboradores", "/erp/rh/colaboradores", "RhForcaTrabalho"),
                new LeafDef("Força de Trabalho", "/erp/rh/forca-trabalho/colaboradores", "RhForcaTrabalho"),
                new LeafDef("Folhas", "/erp/rh/folhas", "RhFolha"),
                new LeafDef("Folha — Competências", "/erp/rh/folha/competencias", "RhFolha"),
                new LeafDef("Folha — Rubricas", "/erp/rh/folha/rubricas", "RhFolha"),
                new LeafDef("Ponto — Marcações", "/erp/rh/ponto/marcacoes", "RhPonto"),
                new LeafDef("Ponto — Períodos", "/erp/rh/ponto/periodos", "RhPonto"),
                new LeafDef("Recrutamento — Vagas", "/erp/rh/recrutamento/vagas", "RhRecrutamento"),
                new LeafDef("Recrutamento — Candidatos", "/erp/rh/recrutamento/candidatos", "RhRecrutamento"),
                new LeafDef("Treinamentos", "/erp/rh/treinamento/treinamentos", "RhTreinamento"),
                new LeafDef("Talentos — Metas", "/erp/rh/talentos/metas", "RhTalentos"),
                new LeafDef("Talentos — Licenças", "/erp/rh/talentos/licencas", "RhTalentos"),
                new LeafDef("Desenvolvimento — Advertências", "/erp/rh/desenvolvimento/advertencias", "RhDesenvolvimento"),
                new LeafDef("Desenvolvimento — Promoções", "/erp/rh/desenvolvimento/promocoes", "RhDesenvolvimento"),
                new LeafDef("Planejamento — Turnos", "/erp/rh/planejamento/turnos", "RhPlanejamento"),
                new LeafDef("Planejamento — Headcount", "/erp/rh/planejamento/headcount", "RhPlanejamento"),
                new LeafDef("SSO — PPP", "/erp/rh/sso/ppp", "RhSaudeSeguranca"),
                new LeafDef("Timesheets", "/erp/rh/timesheets", "RhForcaTrabalho"),
            }),
            new("Produção", "package", new[]
            {
                new LeafDef("Ordens de Produção", "/erp/producao/ordens", "ProducaoOrdensServico"),
                new LeafDef("Ordens MES", "/erp/producao/ordens-mes", "ProducaoMes"),
                new LeafDef("Fichas Técnicas", "/erp/producao/fichas", "ProducaoBom"),
                new LeafDef("Estrutura (BOM)", "/erp/producao/bom-estruturas", "ProducaoBom"),
                new LeafDef("BOM por SKU", "/erp/producao/bom", "ProducaoBom"),
                new LeafDef("MRP", "/erp/producao/mrp", "ProducaoMrp"),
                new LeafDef("Planejamentos", "/erp/producao/planejamentos", "ProducaoPlanejamento"),
                new LeafDef("Programações", "/erp/producao/programacoes", "ProducaoPlanejamento"),
                new LeafDef("Estimativas", "/erp/producao/estimativas", "ProducaoEstimativa"),
                new LeafDef("Custos", "/erp/producao/custos", "ProducaoCustos"),
            }),
            new("Manutenção", "settings", new[]
            {
                new LeafDef("Equipamentos", "/erp/manutencao/equipamentos", "ManutencaoEquipamentoConfig"),
                new LeafDef("Ordens de Manutenção", "/erp/manutencao/ordens", "ManutencaoOrdemServico"),
                new LeafDef("Ordens de Serviço", "/erp/manutencao/ordens-servico", "ManutencaoOrdemServico"),
                new LeafDef("Paradas", "/erp/manutencao/paradas", "ManutencaoParadas"),
                new LeafDef("Peças de Reposição", "/erp/manutencao/pecas-reposicao", "ManutencaoPecas"),
                new LeafDef("Preventiva — Planos", "/erp/manutencao/preventiva/planos", "ManutencaoPreventiva"),
                new LeafDef("Preditiva — Monitoramentos", "/erp/manutencao/preditiva/monitoramentos", "ManutencaoPreditiva"),
                new LeafDef("Preditiva — Alarmes", "/erp/manutencao/preditiva/alarmes", "ManutencaoPreditiva"),
                new LeafDef("Confiabilidade — Revisões", "/erp/manutencao/confiabilidade/revisoes", "ManutencaoConfiabilidade"),
                new LeafDef("Config — Induções", "/erp/manutencao/equipamentos-config/inducoes", "ManutencaoEquipamentoConfig"),
            }),
            new("Qualidade", "report", new[]
            {
                new LeafDef("Não Conformidades (NCR)", "/erp/qualidade/ncr", "QualidadeNaoConformidades"),
                new LeafDef("Não Conformidades Ativas", "/erp/qualidade/nao-conformidades", "QualidadeNaoConformidades"),
                new LeafDef("Planos de Inspeção", "/erp/qualidade/planos-inspecao", "QualidadePlanosInspecao"),
                new LeafDef("Inspeções", "/erp/qualidade/inspecoes", "QualidadePlanosInspecao"),
                new LeafDef("Aceitação/Rejeição", "/erp/qualidade/aceitacao-rejeicao", "QualidadeAceitacaoRejeicao"),
                new LeafDef("Atributos", "/erp/qualidade/atributos", "QualidadeAtributos"),
                new LeafDef("Administração", "/erp/qualidade/administracao", "QualidadeAdministracao"),
            }),
            new("Projetos", "chart", new[]
            {
                new LeafDef("Projetos", "/erp/projetos", "ProjetosPortfolio"),
                new LeafDef("Portfólio", "/erp/projetos/portfolio", "ProjetosPortfolio"),
                new LeafDef("Orçamento", "/erp/projetos/orcamento", "ProjetosOrcamento"),
                new LeafDef("Tarefas", "/erp/projetos/tarefas", "ProjetosRastreamento"),
                new LeafDef("Riscos", "/erp/projetos/riscos", "ProjetosRiscos"),
                new LeafDef("Alocações", "/erp/projetos/alocacoes", "ProjetosRecursos"),
                new LeafDef("Apontamentos", "/erp/projetos/apontamentos", "ProjetosRastreamento"),
                new LeafDef("Faturamento", "/erp/projetos/faturamento", "ProjetosFaturamento"),
                new LeafDef("Encerramento", "/erp/projetos/encerramento", "ProjetosEncerramento"),
            }),
            new("GRC", "report", new[]
            {
                new LeafDef("Riscos", "/erp/grc/riscos", "RiscosCorporativos"),
                new LeafDef("Controles", "/erp/grc/controles", "ControlesAuditoria"),
                new LeafDef("Incidentes", "/erp/grc/incidentes", "RiscosCorporativos"),
                new LeafDef("Políticas", "/erp/grc/politicas", "PoliticasGRC"),
                new LeafDef("Denúncias", "/erp/grc/denuncias", "InvestigacoesDenuncias"),
                new LeafDef("Denúncias — Categorias", "/erp/grc/denuncia-categorias", "InvestigacoesDenuncias"),
                new LeafDef("Compliance — Registros", "/erp/grc/compliance-registros", "ComplianceRegulatorio"),
                new LeafDef("Compliance — Certificados", "/erp/grc/compliance-certificados", "ComplianceRegulatorio"),
                new LeafDef("Auditoria — Planos", "/erp/grc/auditoria-planos", "ControlesAuditoria"),
                new LeafDef("Auditoria — Achados", "/erp/grc/auditoria-achados", "ControlesAuditoria"),
                new LeafDef("SoD — Regras", "/erp/grc/sod-regras", "SegregacaoFuncoes"),
                new LeafDef("SoD — Violações", "/erp/grc/sod-violacoes", "SegregacaoFuncoes"),
            }),
            new("ESG", "chart", new[]
            {
                new LeafDef("Emissões", "/erp/esg/emissoes", "EsgPegadaCarbono"),
                new LeafDef("GHG — Inventários", "/erp/esg/ghg/inventarios", "EsgPegadaCarbono"),
                new LeafDef("GHG — Fatores", "/erp/esg/ghg/fatores", "EsgPegadaCarbono"),
                new LeafDef("EHS — Registros", "/erp/esg/ehs/registros", "EsgGestaoAmbiental"),
                new LeafDef("EHS — Incidentes", "/erp/esg/ehs/incidentes", "EsgGestaoAmbiental"),
                new LeafDef("EHS — Licenças", "/erp/esg/ehs/licencas", "EsgGestaoAmbiental"),
                new LeafDef("Eco — Fluxos", "/erp/esg/eco/fluxos", "EsgEconomiaCircular"),
                new LeafDef("Eco — Devoluções", "/erp/esg/eco/devolucoes", "EsgEconomiaCircular"),
                new LeafDef("Relatórios", "/erp/esg/relatorios", "EsgRelatorios"),
                new LeafDef("Relatórios — Frameworks", "/erp/esg/relatorios/frameworks", "EsgRelatorios"),
            }),
            new("Concessionárias", "building", new[]
            {
                new LeafDef("Vendas — Estoque", "/erp/concessionarias/vendas/estoque", "ConcessionariasVendas"),
                new LeafDef("Vendas — Propostas", "/erp/concessionarias/vendas/propostas", "ConcessionariasVendas"),
                new LeafDef("Vendas — Reservas", "/erp/concessionarias/vendas/reservas", "ConcessionariasVendas"),
                new LeafDef("CRM — Oportunidades", "/erp/concessionarias/crm/oportunidades", "ConcessionariasCrm"),
                new LeafDef("CRM — Prospects", "/erp/concessionarias/crm/prospects", "ConcessionariasCrm"),
                new LeafDef("CRM — Test-drives", "/erp/concessionarias/crm/test-drives", "ConcessionariasCrm"),
                new LeafDef("Oficina — Ordens de Serviço", "/erp/concessionarias/manutencao/ordens-servico", "ConcessionariasManutencao"),
                new LeafDef("Oficina — Orçamentos", "/erp/concessionarias/manutencao/orcamentos", "ConcessionariasManutencao"),
                new LeafDef("Peças", "/erp/concessionarias/pecas/pecas", "ConcessionariasPecas"),
                new LeafDef("Garantias — Solicitações", "/erp/concessionarias/garantias/solicitacoes", "ConcessionariasGarantias"),
                new LeafDef("Finanças — Simulações", "/erp/concessionarias/financas/simulacoes", "ConcessionariasFinancas"),
                new LeafDef("Veículos", "/erp/concessionarias/veiculos", "ConcessionariasVendas"),
                new LeafDef("DMS — Vendas", "/erp/concessionarias/dms/vendas", "ConcessionariasVendas"),
                new LeafDef("DMS — Ordens de Serviço", "/erp/concessionarias/dms/ordens-servico", "ConcessionariasServicos"),
            }),
            new("Imobiliária", "building", new[]
            {
                new LeafDef("Imóveis", "/erp/imobiliaria/imoveis", "ImobiliariaGestao"),
                new LeafDef("Locações", "/erp/imobiliaria/locacoes", "ImobiliariaGestao"),
                new LeafDef("Contratos de Serviço", "/erp/imobiliaria/contratos-servico", "ImobiliariaGestao"),
            }),
        };
    }
}
