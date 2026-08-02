using System.Collections.Generic;
using System.Linq;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// TRANSVERSAL T2 — CATÁLOGO ÚNICO DE EVENTOS DE INTEGRAÇÃO.
    /// Registro central e versionado dos tipos de evento publicados via Outbox pós-commit
    /// (<see cref="OutboxMessage.EventType"/>). Substitui as "magic strings" espalhadas por
    /// módulo: código novo referencia estas constantes, e consumidores/testes validam o tipo
    /// contra <see cref="EhEventoConhecido"/>.
    ///
    /// É a casa da homologação dos ~20 *EventNotification.cs + eventos dotados (EST-*, COM-*, FIN-*).
    /// Ver DECISOES-TRANSVERSAIS.md · T2 e DECISOES_IMPLANTACAO_V1.md · TC-01/TC-02.
    /// </summary>
    public static class CatalogoEventosIntegracao
    {
        /// <summary>Versão do envelope/catálogo (semver simplificado). Muda quando o schema evolui.</summary>
        public const string Versao = "1";

        /// <summary>Eventos de assinatura/SaaS e ciclo de faturamento (COM-*/FIN- do Landlord).</summary>
        public static class Assinatura
        {
            public const string AssinaturaCancelada = "AssinaturaCanceladaEvent";
            public const string AssinaturaReativada = "AssinaturaReativadaEvent";
            public const string PlanoAlterado = "PlanoAlteradoEvent";
            public const string TrialEncerrado = "TrialEncerradoEvent";
            public const string ComissaoApurada = "ComissaoApuradaEvent";
            public const string FaturaAlertaCobranca = "FaturaAlertaCobrancaEvent";
            public const string PagamentoEstornado = "PagamentoEstornadoEvent";
            public const string ReciboEmitido = "ReciboEmitidoEvent";
        }

        /// <summary>Aprovações/workflow transversal.</summary>
        public static class Workflow
        {
            public const string AprovacaoSolicitada = "AprovacaoSolicitada";
            public const string AprovacaoConcluida = "AprovacaoConcluida";
        }

        /// <summary>Cadastro/governança de pessoa (LGPD — T1).</summary>
        public static class Pessoa
        {
            public const string Criada = "pessoa.criada";
            public const string Atualizada = "pessoa.atualizada";
            public const string Inativada = "pessoa.inativada";
            public const string Mesclada = "pessoa.mesclada";
            public const string Anonimizada = "pessoa.anonimizada";
        }

        /// <summary>Vendas / comercial (COM-*).</summary>
        public static class Vendas
        {
            public const string VendaFaturada = "VendaFaturada";
            public const string VendaCancelada = "VendaCancelada";
            public const string PedidoEcommerceParaVenda = "PedidoEcommerceParaVenda";
            public const string DemandaPlanejadaPublicada = "DemandaPlanejadaPublicada";
            public const string ProjetoFaturado = "ProjetoFaturado";
        }

        /// <summary>Compras / entradas.</summary>
        public static class Compras
        {
            public const string CompraLancada = "CompraLancada";
            public const string CompraCancelada = "CompraCancelada";
            public const string CompraFiscalLancada = "CompraFiscalLancada";
            public const string CompraEntradaPropriaLancada = "CompraEntradaPropriaLancada";
            public const string CompraEntradaFornecedorLancada = "CompraEntradaFornecedorLancada";
            // Devolução de Compra (CD4 / EF DEVOLUCAO_DE_COMPRA). Confirmação publica saída de estoque
            // (motor único D1) + estorno financeiro idempotente por compra (DEV-006). Cancelamento compensa.
            public const string DevolucaoCompraConfirmada = "com.devolucao.confirmada";
            /// <summary>Saída de estoque da devolução — consumida pelo motor único de saldo (D1). Sentido/CFOP = valida-contador (NF-06).</summary>
            public const string DevolucaoCompraSaidaEstoque = "com.devolucao.saida_estoque";
            /// <summary>Estorno/redução do passivo — fato gerador financeiro único, idempotente por devolução/compra (DEV-006).</summary>
            public const string DevolucaoCompraEstornoFinanceiro = "com.devolucao.estorno_financeiro";
            public const string DevolucaoCompraCancelada = "com.devolucao.cancelada";
            // Comércio Exterior / Importação (CD1 / EF COMERCIO_EXTERIOR). Nacionalização gera entrada no
            // Estoque (motor único D1) com custo landed (quando ligado) + títulos financeiros de tributos/frete.
            /// <summary>Entrada nacionalizada da importação — consumida pelo motor único de saldo (D1), com custo (landed quando ligado).</summary>
            public const string ImportacaoNacionalizada = "com.importacao.nacionalizada";
            /// <summary>Títulos financeiros de tributos/frete de importação — fato gerador único, idempotente por compra.</summary>
            public const string ImportacaoTitulosFinanceiros = "com.importacao.titulos_financeiros";
        }

        /// <summary>Fiscal (documento eletrônico).</summary>
        public static class Fiscal
        {
            public const string DocumentoFiscalAutorizado = "DocumentoFiscalAutorizado";
            public const string DocumentoFiscalCancelado = "DocumentoFiscalCancelado";
        }

        /// <summary>Estoque (EST-*): LDE, WMS, GCC, SUB, TMS, SC.</summary>
        public static class Estoque
        {
            public const string LdeEntradaCriada = "est.lde.entrada_criada";
            public const string LdeEntradaConfirmada = "est.lde.entrada_confirmada";
            public const string LdeEntradaCancelada = "est.lde.entrada_cancelada";
            public const string LdeEntradaEstornada = "est.lde.entrada_estornada";
            public const string LdeDocumentoVinculado = "est.lde.documento_vinculado";
            public const string LdeLocalEntregaAlterado = "est.lde.local_entrega_alterado";
            /// <summary>Mercadoria fisicamente recebida/conferida (LDE). Consumidores: Qualidade (inspecao) e Financeiro (contas a pagar).</summary>
            public const string MercadoriaRecebida = "est.lde.mercadoria_recebida";
            public const string ScPedidoCompraCriado = "est.sc.pedido_compra_criado";
            /// <summary>Cotação decidida (CD2): vencedor escolhido após o mapa comparativo; apta a originar pedido.</summary>
            public const string ScCotacaoDecidida = "est.sc.cotacao_decidida";
            public const string TmsAlterado = "est.tms.001.alterado";
            /// <summary>Frete de entrada rateado sobre os itens da compra (NF-04): compõe o custo (motor de custeio D1).</summary>
            public const string TmsFreteRateado = "est.tms.frete_rateado";
            public const string WmsArmazemCriado = "estoque.wms.armazem_criado";
            public const string WmsArmazemAlterado = "estoque.wms.armazem_alterado";
            public const string WmsArmazemExclusaoSolicitada = "estoque.wms.armazem_exclusao_solicitada";
            public const string GccContratoCriado = "estoque.gcc.contrato_criado";
            public const string GccContratoEnviadoAprovacao = "estoque.gcc.contrato_enviado_aprovacao";
            public const string GccContratoAprovado = "estoque.gcc.contrato_aprovado";
            public const string GccConsumoRegistrado = "estoque.gcc.consumo_registrado";
            /// <summary>Aditivo contratual aplicado (CD5): preço/quantidade/vigência/condições de contrato aprovado.</summary>
            public const string GccAditivoRegistrado = "estoque.gcc.aditivo_registrado";
            public const string SubEnvioRegistrado = "estoque.sub.envio_registrado";
            public const string SubRetornoRegistrado = "estoque.sub.retorno_registrado";
            /// <summary>Serviço de beneficiamento cobrado (SUB-009): gera a compra do serviço + contas a pagar (via evento).</summary>
            public const string SubServicoCobrado = "estoque.sub.servico_cobrado";
            /// <summary>Documento fiscal de remessa/retorno da subcontratação com CFOP parametrizado (valida-contador).</summary>
            public const string SubDocumentoFiscalRegistrado = "estoque.sub.documento_fiscal_registrado";
            // Inventário Físico e Contagem Cíclica (EST-INV) — EF §13.
            public const string InventarioCriado = "estoque.inv.inventario_criado";
            public const string InventarioItemContado = "estoque.inv.item_contado";
            public const string InventarioDivergenciaCalculada = "estoque.inv.divergencia_calculada";
            public const string InventarioAprovado = "estoque.inv.inventario_aprovado";
            public const string InventarioAjusteGerado = "estoque.inv.ajuste_gerado";
            // Rastreabilidade de Lote e Serialização (EST-RLT).
            public const string LoteCriado = "estoque.rlt.lote_criado";
            public const string LoteBloqueado = "estoque.rlt.lote_bloqueado";
            public const string LoteDesbloqueado = "estoque.rlt.lote_desbloqueado";
            public const string SerialRegistrado = "estoque.rlt.serial_registrado";
            public const string RecallAberto = "estoque.rlt.recall_aberto";
            public const string RecallEncerrado = "estoque.rlt.recall_encerrado";
            // Análise e Planejamento de Estoque (EST-APE).
            public const string AnaliseParametrosAlterados = "estoque.analise.parametros_alterados";
            public const string AnaliseAlertaReposicao = "estoque.analise.alerta_reposicao";
            public const string AnaliseExcessoMaximo = "estoque.analise.excesso_maximo";
            // Portal do Fornecedor (EST-PFO) — EF §13.
            public const string PfoConviteEnviado = "est.pfo.convite_enviado";
            public const string PfoAcessoAtivado = "est.pfo.acesso_ativado";
            public const string PfoCotacaoRespondida = "est.pfo.cotacao_respondida";
            public const string PfoPreAvisoEnviado = "est.pfo.pre_aviso_enviado";
            public const string PfoDocumentoEnviado = "est.pfo.documento_enviado";
        }

        /// <summary>Qualidade (QLD-*): INS, ACR, NCR. Decide e SOLICITA o efeito ao dono (Estoque/NCR) — nunca movimenta saldo.</summary>
        public static class Qualidade
        {
            /// <summary>ACR decidiu rejeitar: solicita bloqueio do lote ao Estoque.</summary>
            public const string AcrLoteBloqueado = "qld.acr.lote_bloqueado";
            /// <summary>ACR aceitou: solicita liberação do lote ao Estoque.</summary>
            public const string AcrLoteLiberado = "qld.acr.lote_liberado";
            /// <summary>ACR colocou em quarentena: solicita quarentena do lote ao Estoque.</summary>
            public const string AcrLoteQuarentena = "qld.acr.lote_quarentena";
            /// <summary>ACR sugere abertura de NCR por gatilho (severidade/recorrência) — D13, default seguro (sugere, não cria à revelia).</summary>
            public const string AcrNcrSolicitada = "qld.acr.ncr_solicitada";
            /// <summary>ACR sinaliza intenção de devolução fiscal ao módulo Fiscal — ⚠️ valida-contador (D15).</summary>
            public const string AcrDevolucaoSolicitada = "qld.acr.devolucao_solicitada";
            /// <summary>INS concluiu execução com resultado técnico (alimenta ACR).</summary>
            public const string InsInspecaoConcluida = "qld.ins.inspecao_concluida";
            /// <summary>NCR aberta.</summary>
            public const string NcrAberta = "qld.ncr.aberta";
            /// <summary>NCR encerrada.</summary>
            public const string NcrEncerrada = "qld.ncr.encerrada";
            /// <summary>Recall aberto (RST).</summary>
            public const string RstRecallAberto = "qld.rst.recall_aberto";
            /// <summary>Recall encerrado (RST).</summary>
            public const string RstRecallEncerrado = "qld.rst.recall_encerrado";
            /// <summary>RST solicita contencao/bloqueio de lote/serie ao Estoque (nao movimenta saldo — D6/D24).</summary>
            public const string RstBloqueioSolicitado = "qld.rst.bloqueio_solicitado";
        }

        /// <summary>
        /// Imobiliaria (IMO-*). ⚠️ ADIÇÃO MÍNIMA pelo módulo IMOBILIARIA (worktree wt/imobiliaria) —
        /// homologar nomes/consumidores na revisão do catálogo. Baixa de aluguel integra o
        /// CONTAS_RECEBER por evento (não recria o recebível): a IMOBILIARIA publica a cobrança
        /// gerada e reflete a baixa/estorno vinda do FINANCEIRO.
        /// </summary>
        public static class Imobiliaria
        {
            public const string ImovelDisponibilizado = "imo.imovel.disponibilizado";
            public const string ImovelInativado = "imo.imovel.inativado";
            public const string LocacaoFormalizada = "imo.locacao.formalizada";
            public const string LocacaoEncerrada = "imo.locacao.encerrada";
            public const string LocacaoCancelada = "imo.locacao.cancelada";
            public const string LocacaoReajustada = "imo.locacao.reajustada";
            public const string LocacaoRescindida = "imo.locacao.rescindida";
            /// <summary>Cobrança recorrente por competência — CONSUMIDA pelo CONTAS_RECEBER (origina o título).</summary>
            public const string AluguelCobrancaGerada = "imo.aluguel.cobranca_gerada";
            /// <summary>Baixa refletida do FINANCEIRO (não governa juros/multa/desconto — NF-01).</summary>
            public const string AluguelBaixaRefletida = "imo.aluguel.baixa_refletida";
            public const string AluguelBaixaEstornada = "imo.aluguel.baixa_estornada";
            public const string ReciboEmitido = "imo.recibo.emitido";
            public const string PropostaConvertida = "imo.proposta.convertida";
            public const string GarantiaRegistrada = "imo.garantia.registrada";
        }

        /// <summary>Operações (produção/manutenção/qualidade/RH/GRC).</summary>
        public static class Operacoes
        {
            public const string OrdemProducaoEncerrada = "OrdemProducaoEncerrada";
            public const string OrdemManutencaoConcluida = "OrdemManutencaoConcluida";
            public const string InspecaoReprovada = "InspecaoReprovada";
            public const string FolhaProcessada = "FolhaProcessada";
            public const string DenunciaProcedente = "DenunciaProcedente";
        }

        private static readonly HashSet<string> _todos = new(new[]
        {
            Assinatura.AssinaturaCancelada, Assinatura.AssinaturaReativada, Assinatura.PlanoAlterado,
            Assinatura.TrialEncerrado, Assinatura.ComissaoApurada, Assinatura.FaturaAlertaCobranca,
            Assinatura.PagamentoEstornado, Assinatura.ReciboEmitido,
            Workflow.AprovacaoSolicitada, Workflow.AprovacaoConcluida,
            Pessoa.Criada, Pessoa.Atualizada, Pessoa.Inativada, Pessoa.Mesclada, Pessoa.Anonimizada,
            Vendas.VendaFaturada, Vendas.VendaCancelada, Vendas.PedidoEcommerceParaVenda,
            Vendas.DemandaPlanejadaPublicada, Vendas.ProjetoFaturado,
            Compras.CompraLancada, Compras.CompraCancelada, Compras.CompraFiscalLancada,
            Compras.CompraEntradaPropriaLancada, Compras.CompraEntradaFornecedorLancada,
            Compras.DevolucaoCompraConfirmada, Compras.DevolucaoCompraSaidaEstoque,
            Compras.DevolucaoCompraEstornoFinanceiro, Compras.DevolucaoCompraCancelada,
            Compras.ImportacaoNacionalizada, Compras.ImportacaoTitulosFinanceiros,
            Fiscal.DocumentoFiscalAutorizado, Fiscal.DocumentoFiscalCancelado,
            Estoque.LdeEntradaCriada, Estoque.LdeEntradaConfirmada, Estoque.LdeEntradaCancelada,
            Estoque.LdeEntradaEstornada, Estoque.LdeDocumentoVinculado, Estoque.LdeLocalEntregaAlterado,
            Estoque.MercadoriaRecebida,
            Estoque.ScPedidoCompraCriado, Estoque.ScCotacaoDecidida, Estoque.TmsAlterado, Estoque.TmsFreteRateado,
            Estoque.WmsArmazemCriado, Estoque.WmsArmazemAlterado, Estoque.WmsArmazemExclusaoSolicitada,
            Estoque.GccContratoCriado, Estoque.GccContratoEnviadoAprovacao, Estoque.GccContratoAprovado,
            Estoque.GccConsumoRegistrado, Estoque.GccAditivoRegistrado, Estoque.SubEnvioRegistrado, Estoque.SubRetornoRegistrado,
            Estoque.SubServicoCobrado, Estoque.SubDocumentoFiscalRegistrado,
            Estoque.InventarioCriado, Estoque.InventarioItemContado, Estoque.InventarioDivergenciaCalculada,
            Estoque.InventarioAprovado, Estoque.InventarioAjusteGerado,
            Estoque.LoteCriado, Estoque.LoteBloqueado, Estoque.LoteDesbloqueado,
            Estoque.SerialRegistrado, Estoque.RecallAberto, Estoque.RecallEncerrado,
            Estoque.AnaliseParametrosAlterados, Estoque.AnaliseAlertaReposicao, Estoque.AnaliseExcessoMaximo,
            Estoque.PfoConviteEnviado, Estoque.PfoAcessoAtivado, Estoque.PfoCotacaoRespondida,
            Estoque.PfoPreAvisoEnviado, Estoque.PfoDocumentoEnviado,
            Operacoes.OrdemProducaoEncerrada, Operacoes.OrdemManutencaoConcluida, Operacoes.InspecaoReprovada,
            Operacoes.FolhaProcessada, Operacoes.DenunciaProcedente,
            Qualidade.AcrLoteBloqueado, Qualidade.AcrLoteLiberado, Qualidade.AcrLoteQuarentena,
            Qualidade.AcrNcrSolicitada, Qualidade.AcrDevolucaoSolicitada, Qualidade.InsInspecaoConcluida,
            Qualidade.NcrAberta, Qualidade.NcrEncerrada,
            Qualidade.RstRecallAberto, Qualidade.RstRecallEncerrado, Qualidade.RstBloqueioSolicitado,
            Imobiliaria.ImovelDisponibilizado, Imobiliaria.ImovelInativado,
            Imobiliaria.LocacaoFormalizada, Imobiliaria.LocacaoEncerrada, Imobiliaria.LocacaoCancelada,
            Imobiliaria.LocacaoReajustada, Imobiliaria.LocacaoRescindida,
            Imobiliaria.AluguelCobrancaGerada, Imobiliaria.AluguelBaixaRefletida, Imobiliaria.AluguelBaixaEstornada,
            Imobiliaria.ReciboEmitido, Imobiliaria.PropostaConvertida, Imobiliaria.GarantiaRegistrada
        }, System.StringComparer.Ordinal);

        /// <summary>Todos os tipos de evento homologados no catálogo.</summary>
        public static IReadOnlyCollection<string> Todos => _todos;

        /// <summary>True se o tipo de evento está registrado no catálogo central.</summary>
        public static bool EhEventoConhecido(string eventType) =>
            !string.IsNullOrWhiteSpace(eventType) && _todos.Contains(eventType);
    }
}
