using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-DEV, tabela rh_dev_evento). Fidelidade campo a campo.</summary>
    public partial class DevEvento : EntidadeSaaSBase
    {
        public string? Titulo { get; private set; }
        public string? Descricao { get; private set; }
        public Guid? TipoEventoId { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public TimeSpan? HoraInicio { get; private set; }
        public TimeSpan? HoraFim { get; private set; }
        public string? Local { get; private set; }
        public string? Status { get; private set; }
        public Guid? AprovadoPor { get; private set; }

        protected DevEvento() { } // EF Core

        public DevEvento(
            string? titulo,
            string? descricao,
            Guid? tipoEventoId,
            DateTime? dataInicio,
            DateTime? dataFim,
            TimeSpan? horaInicio,
            TimeSpan? horaFim,
            string? local,
            string? status,
            Guid? aprovadoPor,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Titulo = titulo;
            Descricao = descricao;
            TipoEventoId = tipoEventoId;
            DataInicio = dataInicio;
            DataFim = dataFim;
            HoraInicio = horaInicio;
            HoraFim = horaFim;
            Local = local;
            Status = status;
            AprovadoPor = aprovadoPor;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<DevEvento>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
