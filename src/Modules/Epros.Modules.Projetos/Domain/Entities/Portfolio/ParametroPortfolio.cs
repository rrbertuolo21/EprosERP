using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Portfolio
{
    /// <summary>
    /// Parametro de portfolio por tenant. Origem: EF PRJ-PRT 14.5 (prj_portfolio_parametro).
    /// PRJ-PRT-RN-024: parametros por tenant devem ser auditados. Chave unica por tenant.
    /// </summary>
    public class ParametroPortfolio : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string ValorJson { get; private set; } = string.Empty;
        public bool Ativo { get; private set; } = true;

        protected ParametroPortfolio() { } // EF Core

        public ParametroPortfolio(
            string chave,
            string valorJson,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ParametroPortfolio>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parametro e obrigatoria. [Origem: ParametroPortfolio]")
                .IsNotNullOrEmpty(valorJson, nameof(ValorJson), "O valor do parametro e obrigatorio. [Origem: ParametroPortfolio]"));

            Chave = chave ?? string.Empty;
            ValorJson = valorJson ?? string.Empty;
            Ativo = true;
        }

        public void Alterar(string valorJson, bool ativo, string usuario)
        {
            if (string.IsNullOrWhiteSpace(valorJson))
            {
                AddNotification(nameof(ValorJson), "O valor do parametro e obrigatorio. [Origem: ParametroPortfolio]");
                return;
            }
            ValorJson = valorJson;
            Ativo = ativo;
            MarcarAlterado(usuario);
        }
    }
}
