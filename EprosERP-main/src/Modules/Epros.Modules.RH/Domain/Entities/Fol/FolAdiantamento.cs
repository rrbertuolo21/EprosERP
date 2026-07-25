using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolAdiantamento : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public string NumeroComprovante { get; private set; } = string.Empty;
        public DateTime DataComprovante { get; private set; }
        public decimal Valor { get; private set; }
        public string Competencia { get; private set; } = string.Empty;
        public int? Serial { get; private set; }
        public string? Narrativa { get; private set; }

        protected FolAdiantamento() { } // EF Core

        public FolAdiantamento(
            Guid colaboradorId,
            string numeroComprovante,
            DateTime dataComprovante,
            decimal valor,
            string competencia,
            int? serial,
            string? narrativa,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            NumeroComprovante = numeroComprovante;
            DataComprovante = dataComprovante;
            Valor = valor;
            Competencia = competencia;
            Serial = serial;
            Narrativa = narrativa;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolAdiantamento>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(NumeroComprovante, nameof(NumeroComprovante), "O campo NumeroComprovante e obrigatorio.");
            contract.IsNotNullOrEmpty(Competencia, nameof(Competencia), "O campo Competencia e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
