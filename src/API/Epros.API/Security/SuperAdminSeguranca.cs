using System;
using System.Collections.Generic;

namespace Epros.API.Security
{
    /// <summary>
    /// 1.11 (OPERACAO_SUPER_ADMIN) — Regras de fronteira do super-admin/landlord.
    ///
    /// Duas responsabilidades:
    ///   (fix #1) Definir QUAIS recursos ABAC são exclusivos do operador interno real da Siser
    ///            (token com tenant="system" E claim perfilId="interno"). Para esses recursos o
    ///            fallback legado "Cargo==Administrador" NÃO pode autorizar — só o operador interno.
    ///   (decisão #5) Classificar cada recurso super-admin numa <see cref="FaixaSuporteSiser"/>
    ///            (Técnico/Negócio/Ambos) e decidir, por menor privilégio, se um operador interno
    ///            com determinado perfil de suporte (ou PrimaryAdmin = tudo) pode acessá-lo.
    ///
    /// O mapa é code-level (não há schema novo): reusa a convenção de recurso do catálogo de
    /// capacidades (1.09/1.10). PrimaryAdmin (UsuarioInterno) é o "Lord pleno" e passa em tudo.
    /// </summary>
    public enum FaixaSuporteSiser
    {
        /// <summary>Infra/instalação/atualização/logs/diagnóstico/impersonação técnica.</summary>
        Tecnico,
        /// <summary>Planos/faturas/cobrança/gateways/dados comerciais do cliente.</summary>
        Negocio,
        /// <summary>Acessível a qualquer perfil de suporte (Técnico OU Negócio).</summary>
        Ambos
    }

    public static class SuperAdminSeguranca
    {
        /// <summary>Recurso base do landlord (dashboard/config/CMS/comunicação/geografia…): faixa Ambos.</summary>
        public const string RecursoBaseSuperAdmin = "SuperAdmin";

        /// <summary>Sub-recurso técnico: instalação/atualização/governança de versão/infra (faixa Técnico).</summary>
        public const string RecursoSuporteInfra = "SuporteInfra";

        /// <summary>Sub-recurso de negócio: planos/faturas/gateways/cobrança/catálogo SaaS (faixa Negócio).</summary>
        public const string RecursoSuporteComercial = "SuporteComercial";

        /// <summary>
        /// Recursos que SÓ um operador interno real da Siser pode autorizar. Para eles, o AbacFilter
        /// nega qualquer identidade não-interna (incluindo o fallback legado de cargo do tenant).
        /// </summary>
        private static readonly HashSet<string> RecursosSomenteInterno = new(StringComparer.OrdinalIgnoreCase)
        {
            RecursoBaseSuperAdmin,
            RecursoSuporteInfra,
            RecursoSuporteComercial
        };

        /// <summary>True se o recurso for exclusivo do operador interno (super-admin/landlord).</summary>
        public static bool ExigeOperadorInterno(string? recurso)
            => !string.IsNullOrEmpty(recurso) && RecursosSomenteInterno.Contains(recurso);

        /// <summary>Faixa de suporte exigida por um recurso super-admin (default Ambos).</summary>
        public static FaixaSuporteSiser FaixaDe(string? recurso)
        {
            if (string.Equals(recurso, RecursoSuporteInfra, StringComparison.OrdinalIgnoreCase))
                return FaixaSuporteSiser.Tecnico;
            if (string.Equals(recurso, RecursoSuporteComercial, StringComparison.OrdinalIgnoreCase))
                return FaixaSuporteSiser.Negocio;
            return FaixaSuporteSiser.Ambos;
        }

        /// <summary>
        /// Decide se o operador interno pode acessar a faixa exigida. PrimaryAdmin passa em tudo;
        /// SuporteTecnico cobre {Técnico, Ambos}; SuporteNegocio cobre {Negócio, Ambos}; Nenhum
        /// (não-primário) não tem escopo de suporte e é negado (menor privilégio — decisão #5).
        /// </summary>
        public static bool OperadorInternoAutorizado(bool primaryAdmin, string? perfilSuporteClaim, FaixaSuporteSiser faixaExigida)
        {
            if (primaryAdmin) return true;

            var tecnico = string.Equals(perfilSuporteClaim, "SuporteTecnico", StringComparison.OrdinalIgnoreCase);
            var negocio = string.Equals(perfilSuporteClaim, "SuporteNegocio", StringComparison.OrdinalIgnoreCase);

            return faixaExigida switch
            {
                FaixaSuporteSiser.Ambos => tecnico || negocio,
                FaixaSuporteSiser.Tecnico => tecnico,
                FaixaSuporteSiser.Negocio => negocio,
                _ => false
            };
        }

        /// <summary>Nome da claim que carrega o perfil de suporte do operador interno no token.</summary>
        public const string ClaimPerfilSuporte = "perfilSuporte";

        /// <summary>Nome da claim que marca o operador interno como PrimaryAdmin (Lord pleno).</summary>
        public const string ClaimPrimaryAdmin = "primaryAdmin";
    }
}
