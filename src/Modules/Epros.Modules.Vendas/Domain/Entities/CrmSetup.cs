using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>Pipeline comercial do CRM (crm_pipeline). Fonte: EF §10.1. CRM-002.</summary>
    public class CrmPipeline : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public Guid CriadorUsuarioId { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected CrmPipeline() { }

        public CrmPipeline(string nome, Guid criadorUsuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            CriadorUsuarioId = criadorUsuarioId;
            Ativo = true;
            AddNotifications(new Contract<CrmPipeline>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do pipeline é obrigatório. [Origem: CrmPipeline]"));
        }

        public void Alterar(string nome, bool ativo, string alteradoPor)
        {
            Nome = nome;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Etapa de lead/oportunidade dentro do pipeline (crm_etapa). Fonte: EF §10.2. CRM-003.</summary>
    public class CrmEtapa : EntidadeSaaSBase
    {
        public Guid PipelineId { get; private set; }
        public ECrmTipoEtapa TipoEtapa { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public int Ordem { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected CrmEtapa() { }

        public CrmEtapa(Guid pipelineId, ECrmTipoEtapa tipoEtapa, string nome, int ordem, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PipelineId = pipelineId;
            TipoEtapa = tipoEtapa;
            Nome = nome;
            Ordem = ordem;
            Ativo = true;
            AddNotifications(new Contract<CrmEtapa>()
                .Requires()
                .AreNotEquals(pipelineId, Guid.Empty, nameof(PipelineId), "O pipeline da etapa é obrigatório. [Origem: CrmEtapa]")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome da etapa é obrigatório. [Origem: CrmEtapa]"));
        }

        public void Alterar(string nome, int ordem, bool ativo, string alteradoPor)
        {
            Nome = nome;
            Ordem = ordem;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Etiqueta comercial (crm_etiqueta). Fonte: EF §10.3. CRM-004.</summary>
    public class CrmEtiqueta : EntidadeSaaSBase
    {
        public Guid? PipelineId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Cor { get; private set; } = "#FF6B6B";
        public bool Ativo { get; private set; } = true;

        protected CrmEtiqueta() { }

        public CrmEtiqueta(Guid? pipelineId, string nome, string? cor, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PipelineId = pipelineId;
            Nome = nome;
            Cor = string.IsNullOrWhiteSpace(cor) ? "#FF6B6B" : cor; // EF §10.3: padrão #FF6B6B.
            Ativo = true;
            AddNotifications(new Contract<CrmEtiqueta>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome da etiqueta é obrigatório. [Origem: CrmEtiqueta]"));
        }

        public void Alterar(string nome, string? cor, bool ativo, string alteradoPor)
        {
            Nome = nome;
            Cor = string.IsNullOrWhiteSpace(cor) ? Cor : cor;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Origem comercial de lead/oportunidade (crm_origem). Fonte: EF §10.4.</summary>
    public class CrmOrigem : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public bool Ativo { get; private set; } = true;

        protected CrmOrigem() { }

        public CrmOrigem(string nome, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Ativo = true;
            AddNotifications(new Contract<CrmOrigem>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome da origem é obrigatório. [Origem: CrmOrigem]"));
        }

        public void Alterar(string nome, bool ativo, string alteradoPor)
        {
            Nome = nome;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }
    }
}
