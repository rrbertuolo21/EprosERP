using System;
using Epros.Modules.ESG.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Incidente ambiental ou ocupacional (EF GESTAO_AMBIENTAL_EHS 11.6).</summary>
    public class Incidente : EntidadeSaaSBase
    {
        public Guid RegistroEhsId { get; private set; }
        public string Tipo { get; private set; } = string.Empty;
        public DateTime DataHora { get; private set; }
        public Guid? LocalId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public string Gravidade { get; private set; } = string.Empty;
        public string? Impacto { get; private set; }
        public Guid? PessoaId { get; private set; }
        public EStatusWorkflowEsg Status { get; private set; }
        public string? NumeroCat { get; private set; }

        protected Incidente() { } // EF Core

        public Incidente(
            Guid registroEhsId,
            string tipo,
            DateTime dataHora,
            Guid? localId,
            string descricao,
            string gravidade,
            string? impacto,
            Guid? pessoaId,
            string? numeroCat,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            RegistroEhsId = registroEhsId;
            Tipo = tipo;
            DataHora = dataHora;
            LocalId = localId;
            Descricao = descricao;
            Gravidade = gravidade;
            Impacto = impacto;
            PessoaId = pessoaId;
            NumeroCat = numeroCat;
            Status = EStatusWorkflowEsg.Rascunho;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Incidente>()
                .Requires()
                .AreNotEquals(RegistroEhsId, Guid.Empty, nameof(RegistroEhsId), "O registro EHS e obrigatorio. [Origem: Incidente]")
                .IsNotNullOrEmpty(Tipo, nameof(Tipo), "O tipo do incidente e obrigatorio. [Origem: Incidente]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao e obrigatoria. [Origem: Incidente]")
                .IsNotNullOrEmpty(Gravidade, nameof(Gravidade), "A gravidade e obrigatoria. [Origem: Incidente]"));
        }
    }
}
