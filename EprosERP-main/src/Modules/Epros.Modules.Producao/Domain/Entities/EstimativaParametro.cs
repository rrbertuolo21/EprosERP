using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-EST — Parâmetro de estimativa por tenant (prd_est_parametro).</summary>
    public class EstimativaParametro : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string? Valor { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected EstimativaParametro() { } // EF Core

        public EstimativaParametro(string chave, string? valor, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Chave = chave;
            Valor = valor;
            Ativo = true;

            AddNotifications(new Contract<EstimativaParametro>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parâmetro é obrigatória [Origem: EstimativaParametro].")
            );
        }

        public void Alterar(string? valor, bool ativo, string alteradoPor)
        {
            Valor = valor;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }
    }
}
