using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    public class Timesheet : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public DateTime Data { get; private set; }
        public decimal HorasTrabalhadas { get; private set; }
        public string DescricaoAtividade { get; private set; } = string.Empty;

        protected Timesheet() { } // EF Core

        public Timesheet(
            Guid colaboradorId,
            DateTime data,
            decimal horasTrabalhadas,
            string descricaoAtividade,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Timesheet>()
                .Requires()
                .AreNotEquals(colaboradorId, Guid.Empty, nameof(ColaboradorId), "O ID do colaborador é obrigatório.")
                .IsGreaterThan(horasTrabalhadas, 0, nameof(HorasTrabalhadas), "As horas trabalhadas devem ser maiores que zero.")
                .IsLowerOrEqualsThan(horasTrabalhadas, 24, nameof(HorasTrabalhadas), "As horas trabalhadas não podem ser maiores que 24 horas por dia.")
                .IsNotNullOrEmpty(descricaoAtividade, nameof(DescricaoAtividade), "A descrição da atividade é obrigatória.")
            );

            ColaboradorId = colaboradorId;
            Data = data.Date;
            HorasTrabalhadas = horasTrabalhadas;
            DescricaoAtividade = descricaoAtividade;
        }
    }
}
