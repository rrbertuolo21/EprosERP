using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC — Base de parametrizacao por tenant (D-TEC-04). Ate agora so existia
    /// <see cref="DenunciaParametro"/> (grc_den_parametro); os outros 5 submodulos nao
    /// persistiam SLA, janelas de alerta, escalas, modo de bloqueio e flags de workflow.
    /// Cada submodulo ganha a sua tabela propria (mesmo shape do DEN), evitando um catalogo
    /// unico acoplado. Chave por (TenantId, Chave). ValorJson guarda o payload do parametro.
    /// </summary>
    public abstract class ParametroGrcBase : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string ValorJson { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }
        public DateTime DataAlteracao { get; private set; }

        protected ParametroGrcBase() { } // EF Core

        protected ParametroGrcBase(string chave, string valorJson, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ParametroGrcBase>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parametro e obrigatoria.")
            );

            Chave = chave;
            ValorJson = valorJson ?? string.Empty;
            Ativo = true;
            DataAlteracao = DateTime.UtcNow;
        }

        public void Alterar(string valorJson, bool ativo, string usuario)
        {
            ValorJson = valorJson ?? string.Empty;
            Ativo = ativo;
            DataAlteracao = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>GRC-POL — parametro por tenant (grc_pol_parametro). Ex.: POL_REACEITE.</summary>
    public class PoliticaParametro : ParametroGrcBase
    {
        protected PoliticaParametro() { }
        public PoliticaParametro(string chave, string valorJson, string tenantId, string criadoPor)
            : base(chave, valorJson, tenantId, criadoPor) { }
    }

    /// <summary>GRC-REG — parametro por tenant (grc_reg_parametro). Ex.: REG_ALERTA=[30,15,7].</summary>
    public class ComplianceParametro : ParametroGrcBase
    {
        protected ComplianceParametro() { }
        public ComplianceParametro(string chave, string valorJson, string tenantId, string criadoPor)
            : base(chave, valorJson, tenantId, criadoPor) { }
    }

    /// <summary>GRC-RIS — parametro por tenant (grc_ris_parametro). Ex.: RIS_ESCALA / RIS_FORMULA.</summary>
    public class RiscoParametro : ParametroGrcBase
    {
        protected RiscoParametro() { }
        public RiscoParametro(string chave, string valorJson, string tenantId, string criadoPor)
            : base(chave, valorJson, tenantId, criadoPor) { }
    }

    /// <summary>GRC-CIA — parametro por tenant (grc_cia_parametro). Ex.: severidade/SLA.</summary>
    public class ControleParametro : ParametroGrcBase
    {
        protected ControleParametro() { }
        public ControleParametro(string chave, string valorJson, string tenantId, string criadoPor)
            : base(chave, valorJson, tenantId, criadoPor) { }
    }

    /// <summary>GRC-SOD — parametro por tenant (grc_sod_parametro). Ex.: SOD_MODO_BLOQUEIO, prazo maximo de excecao.</summary>
    public class SegregacaoParametro : ParametroGrcBase
    {
        protected SegregacaoParametro() { }
        public SegregacaoParametro(string chave, string valorJson, string tenantId, string criadoPor)
            : base(chave, valorJson, tenantId, criadoPor) { }
    }
}
