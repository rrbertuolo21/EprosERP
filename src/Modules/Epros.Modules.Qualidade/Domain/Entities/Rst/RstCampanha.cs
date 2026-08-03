using System;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    /// <summary>
    /// qld_rst_campanha — Campanha de rastreabilidade/recall.
    /// Fluxo Investigacao->Escopo->Contencao->Comunicacao->Recolhimento->Disposicao->Encerramento (D6).
    /// Recall != devolucao/garantia (RN-RST-005/006). Vinculo bidirecional com NCR (RN-RST-020).
    /// </summary>
    public class RstCampanha : EntidadeSaaSBase
    {
        public long? SequenciaExibicao { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Titulo { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public ERstGravidade Gravidade { get; private set; }
        public ERstEtapaCampanha Etapa { get; private set; }
        public EStatusRegistroQualidade Status { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public Guid? ProdutoId { get; private set; }
        public Guid? NcrId { get; private set; }
        public decimal? QuantidadeMercado { get; private set; } // ⚠️ valida (regulatorio D16)
        public string? Conclusao { get; private set; }
        public string? MotivoCancelamento { get; private set; }
        public DateTime DataAbertura { get; private set; }
        public DateTime? DataEncerramento { get; private set; }
        public int Versao { get; private set; }

        protected RstCampanha() { }

        public RstCampanha(string codigo, string titulo, ERstGravidade gravidade, Guid responsavelId,
            string? descricao, Guid? produtoId, Guid? ncrId, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Titulo = titulo;
            Gravidade = gravidade;
            ResponsavelId = responsavelId;
            Descricao = descricao;
            ProdutoId = produtoId;
            NcrId = ncrId;
            Etapa = ERstEtapaCampanha.Investigacao;
            Status = EStatusRegistroQualidade.EmAnalise;
            DataAbertura = DateTime.UtcNow;
            Versao = 1;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<RstCampanha>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo da campanha e obrigatorio [Origem: RstCampanha]")
                .IsLowerOrEqualsThan(Codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo deve ter no maximo 30 caracteres [Origem: RstCampanha]")
                .IsNotNullOrEmpty(Titulo, nameof(Titulo), "O titulo da campanha e obrigatorio [Origem: RstCampanha]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio [Origem: RstCampanha]"));
        }

        /// <summary>Avanca a etapa do fluxo. Nao avanca campanha encerrada/cancelada.</summary>
        public void AvancarEtapa(ERstEtapaCampanha novaEtapa, string usuario)
        {
            if (Status == EStatusRegistroQualidade.Encerrado || Status == EStatusRegistroQualidade.Inativo)
            {
                AddNotification(nameof(Status), "Campanha encerrada/cancelada nao muda de etapa [Origem: RstCampanha]");
                return;
            }
            Etapa = novaEtapa;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void DefinirQuantidadeMercado(decimal quantidade, string usuario)
        {
            QuantidadeMercado = quantidade;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string conclusao, string usuario)
        {
            AddNotifications(new Contract<RstCampanha>()
                .Requires()
                .IsNotNullOrEmpty(conclusao, nameof(Conclusao), "A conclusao e obrigatoria no encerramento [Origem: RstCampanha]"));
            if (!IsValid) return;
            Conclusao = conclusao;
            Etapa = ERstEtapaCampanha.Encerramento;
            Status = EStatusRegistroQualidade.Encerrado;
            DataEncerramento = DateTime.UtcNow;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string motivo, string usuario)
        {
            AddNotifications(new Contract<RstCampanha>()
                .Requires()
                .IsNotNullOrWhiteSpace(motivo, nameof(MotivoCancelamento), "O motivo do cancelamento e obrigatorio [Origem: RstCampanha]"));
            if (!IsValid) return;
            MotivoCancelamento = motivo;
            Etapa = ERstEtapaCampanha.Cancelada;
            Status = EStatusRegistroQualidade.Inativo;
            Versao++;
            MarcarAlterado(usuario);
        }
    }
}
