using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Framework (estrutura versionada) de divulgacao ESG (EF RELATORIOS_ESG 11.3). Entidade mestre.</summary>
    public class FrameworkRel : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Versao { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public DateTime? InicioVigencia { get; private set; }
        public DateTime? FimVigencia { get; private set; }
        public bool Ativo { get; private set; }

        protected FrameworkRel() { } // EF Core

        public FrameworkRel(
            string codigo,
            string versao,
            string descricao,
            DateTime? inicioVigencia,
            DateTime? fimVigencia,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Versao = versao;
            Descricao = descricao;
            InicioVigencia = inicioVigencia?.Date;
            FimVigencia = fimVigencia?.Date;
            Ativo = true;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<FrameworkRel>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo do framework e obrigatorio. [Origem: FrameworkRel]")
                .IsNotNullOrEmpty(Versao, nameof(Versao), "A versao do framework e obrigatoria. [Origem: FrameworkRel]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao e obrigatoria. [Origem: FrameworkRel]"));
        }
    }
}
