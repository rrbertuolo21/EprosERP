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
            public const string TmsAlterado = "est.tms.001.alterado";
            public const string WmsArmazemCriado = "estoque.wms.armazem_criado";
            public const string WmsArmazemAlterado = "estoque.wms.armazem_alterado";
            public const string WmsArmazemExclusaoSolicitada = "estoque.wms.armazem_exclusao_solicitada";
            public const string GccContratoCriado = "estoque.gcc.contrato_criado";
            public const string GccContratoEnviadoAprovacao = "estoque.gcc.contrato_enviado_aprovacao";
            public const string GccContratoAprovado = "estoque.gcc.contrato_aprovado";
            public const string GccConsumoRegistrado = "estoque.gcc.consumo_registrado";
            public const string SubEnvioRegistrado = "estoque.sub.envio_registrado";
            public const string SubRetornoRegistrado = "estoque.sub.retorno_registrado";
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
            Fiscal.DocumentoFiscalAutorizado, Fiscal.DocumentoFiscalCancelado,
            Estoque.LdeEntradaCriada, Estoque.LdeEntradaConfirmada, Estoque.LdeEntradaCancelada,
            Estoque.LdeEntradaEstornada, Estoque.LdeDocumentoVinculado, Estoque.LdeLocalEntregaAlterado,
            Estoque.MercadoriaRecebida,
            Estoque.ScPedidoCompraCriado, Estoque.TmsAlterado,
            Estoque.WmsArmazemCriado, Estoque.WmsArmazemAlterado, Estoque.WmsArmazemExclusaoSolicitada,
            Estoque.GccContratoCriado, Estoque.GccContratoEnviadoAprovacao, Estoque.GccContratoAprovado,
            Estoque.GccConsumoRegistrado, Estoque.SubEnvioRegistrado, Estoque.SubRetornoRegistrado,
            Estoque.InventarioCriado, Estoque.InventarioItemContado, Estoque.InventarioDivergenciaCalculada,
            Estoque.InventarioAprovado, Estoque.InventarioAjusteGerado,
            Estoque.LoteCriado, Estoque.LoteBloqueado, Estoque.LoteDesbloqueado,
            Estoque.SerialRegistrado, Estoque.RecallAberto, Estoque.RecallEncerrado,
            Estoque.AnaliseParametrosAlterados, Estoque.AnaliseAlertaReposicao, Estoque.AnaliseExcessoMaximo,
            Operacoes.OrdemProducaoEncerrada, Operacoes.OrdemManutencaoConcluida, Operacoes.InspecaoReprovada,
            Operacoes.FolhaProcessada, Operacoes.DenunciaProcedente
        }, System.StringComparer.Ordinal);

        /// <summary>Todos os tipos de evento homologados no catálogo.</summary>
        public static IReadOnlyCollection<string> Todos => _todos;

        /// <summary>True se o tipo de evento está registrado no catálogo central.</summary>
        public static bool EhEventoConhecido(string eventType) =>
            !string.IsNullOrWhiteSpace(eventType) && _todos.Contains(eventType);
    }
}
