using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>Registro de reajuste de valor de um contrato financeiro (EF FIN-GCF §13.4 gcf_reajuste).</summary>
    public class ReajusteContrato : EntidadeSaaSBase
    {
        public Guid ContratoId { get; private set; }
        public DateTime? DataReajuste { get; private set; }
        public decimal? ValorAnterior { get; private set; }
        public decimal? ValorNovo { get; private set; }
        public string? Motivo { get; private set; }
        public Guid? AprovacaoId { get; private set; }

        protected ReajusteContrato() { } // EF Core

        public ReajusteContrato(Guid contratoId, DateTime? dataReajuste, decimal? valorAnterior, decimal? valorNovo,
            string? motivo, Guid? aprovacaoId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ContratoId = contratoId;
            DataReajuste = dataReajuste;
            ValorAnterior = valorAnterior;
            ValorNovo = valorNovo;
            Motivo = motivo;
            AprovacaoId = aprovacaoId;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ReajusteContrato>()
                .Requires()
                .IsNotEmpty(ContratoId, nameof(ContratoId), "O contrato é obrigatório [Origem: ReajusteContrato]")
            );
        }
    }
}
