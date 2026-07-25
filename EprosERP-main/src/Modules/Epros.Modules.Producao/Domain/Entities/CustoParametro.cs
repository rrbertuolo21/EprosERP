using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-CST — Parâmetro de custo por tenant (prd_cst_parametro). Chave única parcial por tenant.</summary>
    public class CustoParametro : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string? Valor { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected CustoParametro() { } // EF Core

        public CustoParametro(string chave, string? valor, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Chave = chave;
            Valor = valor;
            Ativo = true;

            AddNotifications(new Contract<CustoParametro>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parâmetro é obrigatória [Origem: CustoParametro].")
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
