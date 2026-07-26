using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Encerramento
{
    /// <summary>
    /// Parametro de encerramento por tenant. Origem: EF PRJ-ENC 11.5 (prj_enc_parametro).
    /// RN: chave unica por tenant (secao 10.4). ValorJson estruturado.
    /// </summary>
    public class ParametroEncerramento : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string ValorJson { get; private set; } = string.Empty;

        protected ParametroEncerramento() { } // EF Core

        public ParametroEncerramento(
            string chave,
            string valorJson,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ParametroEncerramento>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parametro e obrigatoria. [Origem: ParametroEncerramento]")
                .IsNotNullOrEmpty(valorJson, nameof(ValorJson), "O valor do parametro e obrigatorio. [Origem: ParametroEncerramento]"));

            Chave = chave ?? string.Empty;
            ValorJson = valorJson ?? string.Empty;
        }

        public void Alterar(string valorJson, string usuario)
        {
            if (string.IsNullOrWhiteSpace(valorJson))
            {
                AddNotification(nameof(ValorJson), "O valor do parametro e obrigatorio. [Origem: ParametroEncerramento]");
                return;
            }
            ValorJson = valorJson;
            MarcarAlterado(usuario);
        }
    }
}
