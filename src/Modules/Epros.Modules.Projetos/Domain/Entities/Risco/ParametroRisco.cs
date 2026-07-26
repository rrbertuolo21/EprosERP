using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Risco
{
    /// <summary>
    /// Parametro de risco por tenant/projeto. Origem: EF PRJ-RSK 11.6 (prj_risco_parametro).
    /// Chave unica por tenant (secao 10.4). ValorJson estruturado; Ativo controla vigencia.
    /// </summary>
    public class ParametroRisco : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string ValorJson { get; private set; } = string.Empty;
        public bool Ativo { get; private set; } = true;

        protected ParametroRisco() { } // EF Core

        public ParametroRisco(
            string chave,
            string valorJson,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ParametroRisco>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parametro e obrigatoria. [Origem: ParametroRisco]")
                .IsNotNullOrEmpty(valorJson, nameof(ValorJson), "O valor do parametro e obrigatorio. [Origem: ParametroRisco]"));

            Chave = chave ?? string.Empty;
            ValorJson = valorJson ?? string.Empty;
            Ativo = true;
        }

        public void Alterar(string valorJson, bool ativo, string usuario)
        {
            if (string.IsNullOrWhiteSpace(valorJson))
            {
                AddNotification(nameof(ValorJson), "O valor do parametro e obrigatorio. [Origem: ParametroRisco]");
                return;
            }
            ValorJson = valorJson;
            Ativo = ativo;
            MarcarAlterado(usuario);
        }
    }
}
