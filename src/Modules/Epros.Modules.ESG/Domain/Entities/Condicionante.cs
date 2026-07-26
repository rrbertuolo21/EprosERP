using System;
using Epros.Modules.ESG.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Condicionante/requisito de uma licenca (EF GESTAO_AMBIENTAL_EHS 11.5).</summary>
    public class Condicionante : EntidadeSaaSBase
    {
        public Guid LicencaId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public DateTime? Prazo { get; private set; }
        public string? Periodicidade { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public EStatusWorkflowEsg Status { get; private set; }
        public Guid? EvidenciaArquivoId { get; private set; }

        protected Condicionante() { } // EF Core

        public Condicionante(
            Guid licencaId,
            string codigo,
            string descricao,
            DateTime? prazo,
            string? periodicidade,
            Guid responsavelId,
            Guid? evidenciaArquivoId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            LicencaId = licencaId;
            Codigo = codigo;
            Descricao = descricao;
            Prazo = prazo?.Date;
            Periodicidade = periodicidade;
            ResponsavelId = responsavelId;
            EvidenciaArquivoId = evidenciaArquivoId;
            Status = EStatusWorkflowEsg.Rascunho;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Condicionante>()
                .Requires()
                .AreNotEquals(LicencaId, Guid.Empty, nameof(LicencaId), "A licenca e obrigatoria. [Origem: Condicionante]")
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo da condicionante e obrigatorio. [Origem: Condicionante]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao e obrigatoria. [Origem: Condicionante]"));
        }
    }
}
