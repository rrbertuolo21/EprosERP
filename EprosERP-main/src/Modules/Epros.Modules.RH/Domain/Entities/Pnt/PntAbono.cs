using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PNT). Fidelidade campo a campo.</summary>
    public partial class PntAbono : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public int? Quantidade { get; private set; }
        public int? Utilizado { get; private set; }
        public int? Saldo { get; private set; }
        public DateTime? DataCadastro { get; private set; }
        public DateTime? InicioUtilizacao { get; private set; }
        public string Observacao { get; private set; } = string.Empty;

        protected PntAbono() { } // EF Core

        public PntAbono(
            Guid colaboradorId,
            int? quantidade,
            int? utilizado,
            int? saldo,
            DateTime? dataCadastro,
            DateTime? inicioUtilizacao,
            string observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            Quantidade = quantidade;
            Utilizado = utilizado;
            Saldo = saldo;
            DataCadastro = dataCadastro;
            InicioUtilizacao = inicioUtilizacao;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PntAbono>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Observacao, nameof(Observacao), "O campo Observacao e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
