using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_conta_bancaria_colaborador). Fidelidade campo a campo.</summary>
    public partial class WfmContaBancariaColaborador : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid BancoId { get; private set; }
        public string? TituloConta { get; private set; }
        public string? NumeroConta { get; private set; }
        public string? CodigoBanco { get; private set; }
        public string? Agencia { get; private set; }
        public bool? Principal { get; private set; }

        protected WfmContaBancariaColaborador() { } // EF Core

        public WfmContaBancariaColaborador(
            Guid colaboradorId,
            Guid bancoId,
            string? tituloConta,
            string? numeroConta,
            string? codigoBanco,
            string? agencia,
            bool? principal,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            BancoId = bancoId;
            TituloConta = tituloConta;
            NumeroConta = numeroConta;
            CodigoBanco = codigoBanco;
            Agencia = agencia;
            Principal = principal;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmContaBancariaColaborador>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.AreNotEquals(BancoId, Guid.Empty, nameof(BancoId), "O campo BancoId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
