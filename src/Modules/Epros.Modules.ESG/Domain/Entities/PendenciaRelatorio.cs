using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Tratamento de excecao/pendencia de um relatorio (EF RELATORIOS_ESG 11.5).</summary>
    public class PendenciaRelatorio : EntidadeSaaSBase
    {
        public Guid RelatorioId { get; private set; }
        public Guid? ItemId { get; private set; }
        public string Criticidade { get; private set; } = string.Empty; // Bloqueante, Alerta
        public Guid ResponsavelId { get; private set; }
        public DateTime? Prazo { get; private set; }
        public string Status { get; private set; } = "Aberta"; // Aberta, EmTratamento, Resolvida
        public string? Resolucao { get; private set; }

        protected PendenciaRelatorio() { } // EF Core

        public PendenciaRelatorio(
            Guid relatorioId,
            Guid? itemId,
            string criticidade,
            Guid responsavelId,
            DateTime? prazo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            RelatorioId = relatorioId;
            ItemId = itemId;
            Criticidade = criticidade;
            ResponsavelId = responsavelId;
            Prazo = prazo;
            Status = "Aberta";
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PendenciaRelatorio>()
                .Requires()
                .AreNotEquals(RelatorioId, Guid.Empty, nameof(RelatorioId), "O relatorio e obrigatorio. [Origem: PendenciaRelatorio]")
                .IsNotNullOrEmpty(Criticidade, nameof(Criticidade), "A criticidade e obrigatoria. [Origem: PendenciaRelatorio]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio. [Origem: PendenciaRelatorio]"));
        }

        public void Resolver(string resolucao, string usuario)
        {
            if (string.IsNullOrWhiteSpace(resolucao))
            {
                AddNotification(nameof(Resolucao), "A resolucao e obrigatoria ao resolver a pendencia.");
                return;
            }
            Status = "Resolvida";
            Resolucao = resolucao;
            MarcarAlterado(usuario);
        }
    }
}
