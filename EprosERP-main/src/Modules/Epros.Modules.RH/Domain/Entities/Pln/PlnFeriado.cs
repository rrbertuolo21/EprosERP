using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PLN, tabela rh_pln_feriado). Fidelidade campo a campo.</summary>
    public partial class PlnFeriado : EntidadeSaaSBase
    {
        public string? Nome { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public Guid? TipoFeriadoId { get; private set; }
        public string? Descricao { get; private set; }
        public bool? Remunerado { get; private set; }
        public bool? SincronizarCalendarioGoogle { get; private set; }
        public bool? SincronizarCalendarioOutlook { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }

        protected PlnFeriado() { } // EF Core

        public PlnFeriado(
            string? nome,
            DateTime? dataInicio,
            DateTime? dataFim,
            Guid? tipoFeriadoId,
            string? descricao,
            bool? remunerado,
            bool? sincronizarCalendarioGoogle,
            bool? sincronizarCalendarioOutlook,
            Guid? criadoPorId,
            Guid? ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            DataInicio = dataInicio;
            DataFim = dataFim;
            TipoFeriadoId = tipoFeriadoId;
            Descricao = descricao;
            Remunerado = remunerado;
            SincronizarCalendarioGoogle = sincronizarCalendarioGoogle;
            SincronizarCalendarioOutlook = sincronizarCalendarioOutlook;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PlnFeriado>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
