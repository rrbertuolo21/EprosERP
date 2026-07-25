using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PLN, tabela rh_pln_escala). Fidelidade campo a campo.</summary>
    public partial class PlnEscala : EntidadeSaaSBase
    {
        public string? Nome { get; private set; }
        public Guid? TurnoId { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public bool Ativo { get; private set; }
        public string? Observacao { get; private set; }

        protected PlnEscala() { } // EF Core

        public PlnEscala(
            string? nome,
            Guid? turnoId,
            DateTime? dataInicio,
            DateTime? dataFim,
            bool ativo,
            string? observacao,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            TurnoId = turnoId;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Ativo = ativo;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PlnEscala>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
