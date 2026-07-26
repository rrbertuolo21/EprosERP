using System;
using Epros.Modules.ESG.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Medida bruta usada no calculo, vinculada a uma fonte (EF PEGADA_DE_CARBONO 11.2 Atividade).</summary>
    public class DadoAtividadeGee : EntidadeSaaSBase
    {
        public Guid FonteEmissaoId { get; private set; }
        public DateTime DataReferencia { get; private set; }
        public decimal Quantidade { get; private set; }
        public string Unidade { get; private set; } = string.Empty;
        public EOrigemDadoGhg OrigemDado { get; private set; }
        public string? ReferenciaOperacional { get; private set; }

        protected DadoAtividadeGee() { } // EF Core

        public DadoAtividadeGee(
            Guid fonteEmissaoId,
            DateTime dataReferencia,
            decimal quantidade,
            string unidade,
            EOrigemDadoGhg origemDado,
            string? referenciaOperacional,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            FonteEmissaoId = fonteEmissaoId;
            DataReferencia = dataReferencia.Date;
            Quantidade = quantidade;
            Unidade = unidade;
            OrigemDado = origemDado;
            ReferenciaOperacional = referenciaOperacional;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<DadoAtividadeGee>()
                .Requires()
                .AreNotEquals(FonteEmissaoId, Guid.Empty, nameof(FonteEmissaoId), "A fonte de emissao e obrigatoria. [Origem: DadoAtividadeGee]")
                .IsNotNullOrEmpty(Unidade, nameof(Unidade), "A unidade e obrigatoria. [Origem: DadoAtividadeGee]"));
        }
    }
}
