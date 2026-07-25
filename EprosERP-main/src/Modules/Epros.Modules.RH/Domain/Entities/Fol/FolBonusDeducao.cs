using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolBonusDeducao : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public DateTime Data { get; private set; }
        public decimal? Valor { get; private set; }
        public string? Competencia { get; private set; }
        public string? Tipo { get; private set; }
        public string? Narrativa { get; private set; }

        protected FolBonusDeducao() { } // EF Core

        public FolBonusDeducao(
            Guid colaboradorId,
            DateTime data,
            decimal? valor,
            string? competencia,
            string? tipo,
            string? narrativa,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            Data = data;
            Valor = valor;
            Competencia = competencia;
            Tipo = tipo;
            Narrativa = narrativa;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolBonusDeducao>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
