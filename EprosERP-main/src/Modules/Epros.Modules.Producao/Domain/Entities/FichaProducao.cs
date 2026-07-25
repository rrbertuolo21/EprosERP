using System;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-GOS — Ficha/ordem de serviço de produção customizada vinculada à venda (prd_gos_ficha_producao).
    /// Fiel ao EF GESTAO_DE_ORDENS_DE_SERVICO §11.1. FKs legadas long → Guid. Situação: 1→2→3.
    /// </summary>
    public class FichaProducao : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
        public Guid ItemVendaId { get; private set; }
        public Guid PessoaId { get; private set; }
        public ESituacaoFichaProducao Situacao { get; private set; } = ESituacaoFichaProducao.AguardandoPagamento;
        public DateTime Entrada { get; private set; }
        public DateTime? Saida { get; private set; }
        public ELogomarcaFichaProducao Logomarca { get; private set; } = ELogomarcaFichaProducao.SemLogo;
        public int LateraisPorta { get; private set; }
        public int ApoioCabeca { get; private set; }
        public string? Transportadora { get; private set; }
        public string? AnoModelo { get; private set; }
        public string? CorCouro { get; private set; }
        public string? Costura { get; private set; }
        public string? TipoAcento { get; private set; }
        public string? TipoEncosto { get; private set; }
        public string? Abd { get; private set; }
        public string? Abt { get; private set; }
        public string? Observacao { get; private set; }

        protected FichaProducao() { } // EF Core

        public FichaProducao(
            Guid vendaId,
            Guid itemVendaId,
            Guid pessoaId,
            ELogomarcaFichaProducao logomarca,
            int lateraisPorta,
            int apoioCabeca,
            string tenantId,
            string criadoPor,
            DateTime? entrada = null,
            DateTime? saida = null,
            string? transportadora = null,
            string? anoModelo = null,
            string? corCouro = null,
            string? costura = null,
            string? tipoAcento = null,
            string? tipoEncosto = null,
            string? abd = null,
            string? abt = null,
            string? observacao = null)
            : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            ItemVendaId = itemVendaId;
            PessoaId = pessoaId;
            Situacao = ESituacaoFichaProducao.AguardandoPagamento;
            Entrada = entrada ?? DateTime.UtcNow;
            Saida = saida;
            Logomarca = logomarca;
            LateraisPorta = lateraisPorta;
            ApoioCabeca = apoioCabeca;
            Transportadora = transportadora;
            AnoModelo = anoModelo;
            CorCouro = corCouro;
            Costura = costura;
            TipoAcento = tipoAcento;
            TipoEncosto = tipoEncosto;
            Abd = abd;
            Abt = abt;
            Observacao = observacao;

            AddNotifications(new Contract<FichaProducao>()
                .Requires()
                .AreNotEquals(vendaId, Guid.Empty, nameof(VendaId), "A venda é obrigatória [Origem: FichaProducao].")
                .AreNotEquals(itemVendaId, Guid.Empty, nameof(ItemVendaId), "O item da venda é obrigatório [Origem: FichaProducao].")
                .AreNotEquals(pessoaId, Guid.Empty, nameof(PessoaId), "A pessoa é obrigatória [Origem: FichaProducao].")
            );
        }

        /// <summary>GOS §12.1: transição operacional AguardandoPagamento → EmProducao → Concluido.</summary>
        public void IniciarProducao(string alteradoPor)
        {
            if (Situacao != ESituacaoFichaProducao.AguardandoPagamento)
            {
                AddNotification(nameof(Situacao), "Só é possível iniciar a produção de ficha Aguardando pagamento.");
                return;
            }
            Situacao = ESituacaoFichaProducao.EmProducao;
            MarcarAlterado(alteradoPor);
        }

        public void Concluir(string alteradoPor)
        {
            if (Situacao != ESituacaoFichaProducao.EmProducao)
            {
                AddNotification(nameof(Situacao), "Só é possível concluir uma ficha Em produção.");
                return;
            }
            Situacao = ESituacaoFichaProducao.Concluido;
            Saida = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void AlterarConfiguracao(
            ELogomarcaFichaProducao logomarca,
            int lateraisPorta,
            int apoioCabeca,
            string alteradoPor,
            string? transportadora = null,
            string? anoModelo = null,
            string? corCouro = null,
            string? costura = null,
            string? tipoAcento = null,
            string? tipoEncosto = null,
            string? abd = null,
            string? abt = null,
            string? observacao = null)
        {
            Logomarca = logomarca;
            LateraisPorta = lateraisPorta;
            ApoioCabeca = apoioCabeca;
            Transportadora = transportadora;
            AnoModelo = anoModelo;
            CorCouro = corCouro;
            Costura = costura;
            TipoAcento = tipoAcento;
            TipoEncosto = tipoEncosto;
            Abd = abd;
            Abt = abt;
            Observacao = observacao;
            MarcarAlterado(alteradoPor);
        }
    }
}
