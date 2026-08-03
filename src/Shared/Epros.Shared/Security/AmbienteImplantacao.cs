using System;

namespace Epros.Shared.Security
{
    /// <summary>
    /// Critério ÚNICO de "fail-closed" de segurança para segredos com fallback de desenvolvimento
    /// (chave de assinatura JWT, token/KEK do cofre). Um segredo só pode cair no valor fixo de
    /// desenvolvimento quando o processo está rodando em <b>desenvolvimento local puro</b> — isto é,
    /// ambiente <c>Development</c> E fora de contêiner/CI. Qualquer ambiente deployado
    /// (Production/Staging/Testing, ou Development dentro de contêiner/pipeline) exige o segredo real
    /// vindo de env/secret; se faltar, a operação/startup falha em vez de operar com credencial insegura.
    ///
    /// Isso remove o antigo atalho onde "ASPNETCORE_ENVIRONMENT errado" transformava um binário
    /// deployado em bypass (o "gato").
    /// </summary>
    public static class AmbienteImplantacao
    {
        /// <summary>
        /// True se o processo está claramente rodando em contêiner ou em um pipeline de CI —
        /// nesses casos NUNCA se aceita o fallback de segredo de desenvolvimento.
        /// </summary>
        public static bool EstaEmContainerOuCI()
        {
            return EhVerdadeiro(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"))
                || EhVerdadeiro(Environment.GetEnvironmentVariable("CI"))
                || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST"))
                || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));
        }

        /// <summary>
        /// True somente em desenvolvimento local puro (Development E não em contêiner/CI).
        /// É a ÚNICA condição em que um segredo pode cair no valor fixo de desenvolvimento.
        /// </summary>
        public static bool EhDesenvolvimentoLocal(string? environmentName)
        {
            return string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase)
                && !EstaEmContainerOuCI();
        }

        private static bool EhVerdadeiro(string? valor)
        {
            return string.Equals(valor, "true", StringComparison.OrdinalIgnoreCase) || valor == "1";
        }
    }
}
