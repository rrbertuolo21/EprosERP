using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PNT). Fidelidade campo a campo.</summary>
    public partial class PntPresencaDiaria : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid? TurnoId { get; private set; }
        public DateTime Data { get; private set; }
        public DateTime? Entrada { get; private set; }
        public DateTime? Saida { get; private set; }
        public string? HoraIntervalo { get; private set; }
        public string? TotalHoras { get; private set; }
        public string? HorasExtras { get; private set; }
        public decimal? ValorHoraExtra { get; private set; }
        public string? Status { get; private set; }
        public string? Observacao { get; private set; }

        protected PntPresencaDiaria() { } // EF Core

        public PntPresencaDiaria(
            Guid colaboradorId,
            Guid? turnoId,
            DateTime data,
            DateTime? entrada,
            DateTime? saida,
            string? horaIntervalo,
            string? totalHoras,
            string? horasExtras,
            decimal? valorHoraExtra,
            string? status,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            TurnoId = turnoId;
            Data = data;
            Entrada = entrada;
            Saida = saida;
            HoraIntervalo = horaIntervalo;
            TotalHoras = totalHoras;
            HorasExtras = horasExtras;
            ValorHoraExtra = valorHoraExtra;
            Status = status;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PntPresencaDiaria>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
