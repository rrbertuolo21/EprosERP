using System;
using Epros.Modules.ESG.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Licenca ambiental com vigencia e autoridade (EF GESTAO_AMBIENTAL_EHS 11.5).</summary>
    public class LicencaAmbiental : EntidadeSaaSBase
    {
        public Guid RegistroEhsId { get; private set; }
        public string Tipo { get; private set; } = string.Empty;
        public string Numero { get; private set; } = string.Empty;
        public string Autoridade { get; private set; } = string.Empty;
        public DateTime DataEmissao { get; private set; }
        public DateTime DataValidade { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public EStatusWorkflowEsg Status { get; private set; }
        public Guid? ArquivoId { get; private set; }

        protected LicencaAmbiental() { } // EF Core

        public LicencaAmbiental(
            Guid registroEhsId,
            string tipo,
            string numero,
            string autoridade,
            DateTime dataEmissao,
            DateTime dataValidade,
            Guid responsavelId,
            Guid? arquivoId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            RegistroEhsId = registroEhsId;
            Tipo = tipo;
            Numero = numero;
            Autoridade = autoridade;
            DataEmissao = dataEmissao.Date;
            DataValidade = dataValidade.Date;
            ResponsavelId = responsavelId;
            ArquivoId = arquivoId;
            Status = EStatusWorkflowEsg.Rascunho;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LicencaAmbiental>()
                .Requires()
                .AreNotEquals(RegistroEhsId, Guid.Empty, nameof(RegistroEhsId), "O registro EHS e obrigatorio. [Origem: LicencaAmbiental]")
                .IsNotNullOrEmpty(Numero, nameof(Numero), "O numero da licenca e obrigatorio. [Origem: LicencaAmbiental]")
                .IsNotNullOrEmpty(Autoridade, nameof(Autoridade), "A autoridade e obrigatoria. [Origem: LicencaAmbiental]")
                .IsFalse(DataValidade < DataEmissao, nameof(DataValidade), "A validade deve ser posterior a emissao. [Origem: LicencaAmbiental]"));
        }
    }
}
