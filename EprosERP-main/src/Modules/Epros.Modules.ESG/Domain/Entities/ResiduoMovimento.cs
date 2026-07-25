using System;
using Epros.Modules.ESG.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Geracao e destino de residuo (EF GESTAO_AMBIENTAL_EHS 11.6).</summary>
    public class ResiduoMovimento : EntidadeSaaSBase
    {
        public Guid RegistroEhsId { get; private set; }
        public string TipoResiduo { get; private set; } = string.Empty;
        public string Classificacao { get; private set; } = string.Empty;
        public string Origem { get; private set; } = string.Empty;
        public decimal Quantidade { get; private set; }
        public string Unidade { get; private set; } = string.Empty;
        public DateTime Data { get; private set; }
        public Guid? LocalId { get; private set; }
        public string TipoMovimento { get; private set; } = string.Empty;
        public Guid? DestinoId { get; private set; }
        public Guid? EvidenciaArquivoId { get; private set; }
        public EStatusWorkflowEsg Status { get; private set; }

        protected ResiduoMovimento() { } // EF Core

        public ResiduoMovimento(
            Guid registroEhsId,
            string tipoResiduo,
            string classificacao,
            string origem,
            decimal quantidade,
            string unidade,
            DateTime data,
            Guid? localId,
            string tipoMovimento,
            Guid? destinoId,
            Guid? evidenciaArquivoId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            RegistroEhsId = registroEhsId;
            TipoResiduo = tipoResiduo;
            Classificacao = classificacao;
            Origem = origem;
            Quantidade = quantidade;
            Unidade = unidade;
            Data = data.Date;
            LocalId = localId;
            TipoMovimento = tipoMovimento;
            DestinoId = destinoId;
            EvidenciaArquivoId = evidenciaArquivoId;
            Status = EStatusWorkflowEsg.Rascunho;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ResiduoMovimento>()
                .Requires()
                .AreNotEquals(RegistroEhsId, Guid.Empty, nameof(RegistroEhsId), "O registro EHS e obrigatorio. [Origem: ResiduoMovimento]")
                .IsNotNullOrEmpty(TipoResiduo, nameof(TipoResiduo), "O tipo de residuo e obrigatorio. [Origem: ResiduoMovimento]")
                .IsNotNullOrEmpty(Unidade, nameof(Unidade), "A unidade e obrigatoria. [Origem: ResiduoMovimento]")
                .IsGreaterThan(Quantidade, 0, nameof(Quantidade), "A quantidade deve ser maior que zero. [Origem: ResiduoMovimento]"));
        }
    }
}
