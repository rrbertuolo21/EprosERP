using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Utilização financeira de um programa/fundo por despesa elegível (EF FIN-SBF §11.2 UtilizacaoSubsidio).
    /// Programa intra-módulo (navegação); título a pagar cross-module por Guid FK.
    /// </summary>
    public class UtilizacaoSubsidio : EntidadeSaaSBase
    {
        public Guid ProgramaSubsidioId { get; private set; }
        public Guid TituloPagarId { get; private set; }
        public decimal ValorElegivel { get; private set; }

        public ProgramaSubsidio ProgramaSubsidio { get; private set; } = null!;

        protected UtilizacaoSubsidio() { } // EF Core

        public UtilizacaoSubsidio(Guid programaSubsidioId, Guid tituloPagarId, decimal valorElegivel, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ProgramaSubsidioId = programaSubsidioId;
            TituloPagarId = tituloPagarId;
            ValorElegivel = valorElegivel;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<UtilizacaoSubsidio>()
                .Requires()
                .IsNotEmpty(ProgramaSubsidioId, nameof(ProgramaSubsidioId), "O programa é obrigatório [Origem: UtilizacaoSubsidio]")
                .IsNotEmpty(TituloPagarId, nameof(TituloPagarId), "O título a pagar é obrigatório [Origem: UtilizacaoSubsidio]")
                .IsGreaterThan(ValorElegivel, 0, nameof(ValorElegivel), "O valor elegível deve ser maior que zero [Origem: UtilizacaoSubsidio]")
            );
        }
    }
}
