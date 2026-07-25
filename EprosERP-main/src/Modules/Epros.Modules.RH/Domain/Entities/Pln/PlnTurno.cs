using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PLN, tabela rh_pln_turno). Fidelidade campo a campo.</summary>
    public partial class PlnTurno : EntidadeSaaSBase
    {
        public string? Nome { get; private set; }
        public TimeSpan? HoraInicio { get; private set; }
        public TimeSpan? HoraFim { get; private set; }
        public TimeSpan? IntervaloInicio { get; private set; }
        public TimeSpan? IntervaloFim { get; private set; }
        public bool? TurnoNoturno { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }
        public bool Ativo { get; private set; }

        protected PlnTurno() { } // EF Core

        public PlnTurno(
            string? nome,
            TimeSpan? horaInicio,
            TimeSpan? horaFim,
            TimeSpan? intervaloInicio,
            TimeSpan? intervaloFim,
            bool? turnoNoturno,
            Guid? criadoPorId,
            Guid? ownerId,
            bool ativo,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            HoraInicio = horaInicio;
            HoraFim = horaFim;
            IntervaloInicio = intervaloInicio;
            IntervaloFim = intervaloFim;
            TurnoNoturno = turnoNoturno;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Ativo = ativo;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PlnTurno>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
