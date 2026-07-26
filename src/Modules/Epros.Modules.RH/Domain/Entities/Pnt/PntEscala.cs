using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PNT). Fidelidade campo a campo.</summary>
    public partial class PntEscala : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string DescontoHoraDia { get; private set; } = string.Empty;
        public string DescontoDsr { get; private set; } = string.Empty;
        public string CodigoHorarioDomingo { get; private set; } = string.Empty;
        public string CodigoHorarioSegunda { get; private set; } = string.Empty;
        public string CodigoHorarioTerca { get; private set; } = string.Empty;
        public string CodigoHorarioQuarta { get; private set; } = string.Empty;
        public string CodigoHorarioQuinta { get; private set; } = string.Empty;
        public string CodigoHorarioSexta { get; private set; } = string.Empty;
        public string CodigoHorarioSabado { get; private set; } = string.Empty;

        protected PntEscala() { } // EF Core

        public PntEscala(
            Guid empresaId,
            string nome,
            string descontoHoraDia,
            string descontoDsr,
            string codigoHorarioDomingo,
            string codigoHorarioSegunda,
            string codigoHorarioTerca,
            string codigoHorarioQuarta,
            string codigoHorarioQuinta,
            string codigoHorarioSexta,
            string codigoHorarioSabado,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Nome = nome;
            DescontoHoraDia = descontoHoraDia;
            DescontoDsr = descontoDsr;
            CodigoHorarioDomingo = codigoHorarioDomingo;
            CodigoHorarioSegunda = codigoHorarioSegunda;
            CodigoHorarioTerca = codigoHorarioTerca;
            CodigoHorarioQuarta = codigoHorarioQuarta;
            CodigoHorarioQuinta = codigoHorarioQuinta;
            CodigoHorarioSexta = codigoHorarioSexta;
            CodigoHorarioSabado = codigoHorarioSabado;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PntEscala>().Requires();
            contract.AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "O campo EmpresaId e obrigatorio.");
            contract.IsNotNullOrEmpty(Nome, nameof(Nome), "O campo Nome e obrigatorio.");
            contract.IsNotNullOrEmpty(DescontoHoraDia, nameof(DescontoHoraDia), "O campo DescontoHoraDia e obrigatorio.");
            contract.IsNotNullOrEmpty(DescontoDsr, nameof(DescontoDsr), "O campo DescontoDsr e obrigatorio.");
            contract.IsNotNullOrEmpty(CodigoHorarioDomingo, nameof(CodigoHorarioDomingo), "O campo CodigoHorarioDomingo e obrigatorio.");
            contract.IsNotNullOrEmpty(CodigoHorarioSegunda, nameof(CodigoHorarioSegunda), "O campo CodigoHorarioSegunda e obrigatorio.");
            contract.IsNotNullOrEmpty(CodigoHorarioTerca, nameof(CodigoHorarioTerca), "O campo CodigoHorarioTerca e obrigatorio.");
            contract.IsNotNullOrEmpty(CodigoHorarioQuarta, nameof(CodigoHorarioQuarta), "O campo CodigoHorarioQuarta e obrigatorio.");
            contract.IsNotNullOrEmpty(CodigoHorarioQuinta, nameof(CodigoHorarioQuinta), "O campo CodigoHorarioQuinta e obrigatorio.");
            contract.IsNotNullOrEmpty(CodigoHorarioSexta, nameof(CodigoHorarioSexta), "O campo CodigoHorarioSexta e obrigatorio.");
            contract.IsNotNullOrEmpty(CodigoHorarioSabado, nameof(CodigoHorarioSabado), "O campo CodigoHorarioSabado e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
