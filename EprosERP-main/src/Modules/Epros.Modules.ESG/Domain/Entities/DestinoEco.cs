using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Resultado e quantidade destinada de uma triagem (EF ECONOMIA_CIRCULAR 11.5).</summary>
    public class DestinoEco : EntidadeSaaSBase
    {
        public Guid TriagemId { get; private set; }
        public string TipoDestino { get; private set; } = string.Empty; // Reuso, Reparo, Reciclagem...
        public decimal Quantidade { get; private set; }
        public string Unidade { get; private set; } = string.Empty;
        public DateTime DataExecucao { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public Guid? EvidenciaArquivoId { get; private set; }
        public string? Observacao { get; private set; }

        protected DestinoEco() { } // EF Core

        public DestinoEco(
            Guid triagemId,
            string tipoDestino,
            decimal quantidade,
            string unidade,
            DateTime dataExecucao,
            Guid responsavelId,
            Guid? evidenciaArquivoId,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            TriagemId = triagemId;
            TipoDestino = tipoDestino;
            Quantidade = quantidade;
            Unidade = unidade;
            DataExecucao = dataExecucao;
            ResponsavelId = responsavelId;
            EvidenciaArquivoId = evidenciaArquivoId;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<DestinoEco>()
                .Requires()
                .AreNotEquals(TriagemId, Guid.Empty, nameof(TriagemId), "A triagem e obrigatoria. [Origem: DestinoEco]")
                .IsNotNullOrEmpty(TipoDestino, nameof(TipoDestino), "O tipo de destino e obrigatorio. [Origem: DestinoEco]")
                // RN-ECO-004 constraint: quantidade maior que zero.
                .IsGreaterThan(Quantidade, 0, nameof(Quantidade), "A quantidade destinada deve ser maior que zero. [Origem: DestinoEco]")
                .IsNotNullOrEmpty(Unidade, nameof(Unidade), "A unidade e obrigatoria. [Origem: DestinoEco]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio. [Origem: DestinoEco]"));
        }
    }
}
