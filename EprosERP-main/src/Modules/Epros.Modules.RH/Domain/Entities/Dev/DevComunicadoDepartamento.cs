using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-DEV, tabela rh_dev_comunicado_departamento). Fidelidade campo a campo.</summary>
    public partial class DevComunicadoDepartamento : EntidadeSaaSBase
    {
        public Guid ComunicadoId { get; private set; }
        public Guid DepartamentoId { get; private set; }

        protected DevComunicadoDepartamento() { } // EF Core

        public DevComunicadoDepartamento(
            Guid comunicadoId,
            Guid departamentoId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ComunicadoId = comunicadoId;
            DepartamentoId = departamentoId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<DevComunicadoDepartamento>().Requires();
            contract.AreNotEquals(ComunicadoId, Guid.Empty, nameof(ComunicadoId), "O campo ComunicadoId e obrigatorio.");
            contract.AreNotEquals(DepartamentoId, Guid.Empty, nameof(DepartamentoId), "O campo DepartamentoId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
