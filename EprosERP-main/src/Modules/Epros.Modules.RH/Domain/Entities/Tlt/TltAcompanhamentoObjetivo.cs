using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_acompanhamento_objetivo). Fidelidade campo a campo.</summary>
    public partial class TltAcompanhamentoObjetivo : EntidadeSaaSBase
    {
        public Guid? ObjetivoId { get; private set; }
        public DateTime? DataAcompanhamento { get; private set; }
        public decimal? ValorAnterior { get; private set; }
        public decimal? ValorContribuicao { get; private set; }
        public decimal? ValorAtual { get; private set; }
        public decimal? PercentualProgresso { get; private set; }
        public int? DiasRestantes { get; private set; }
        public DateTime? DataConclusaoProjetada { get; private set; }
        public string? StatusAndamento { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }

        protected TltAcompanhamentoObjetivo() { } // EF Core

        public TltAcompanhamentoObjetivo(
            Guid? objetivoId,
            DateTime? dataAcompanhamento,
            decimal? valorAnterior,
            decimal? valorContribuicao,
            decimal? valorAtual,
            decimal? percentualProgresso,
            int? diasRestantes,
            DateTime? dataConclusaoProjetada,
            string? statusAndamento,
            Guid? criadoPorId,
            Guid? ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ObjetivoId = objetivoId;
            DataAcompanhamento = dataAcompanhamento;
            ValorAnterior = valorAnterior;
            ValorContribuicao = valorContribuicao;
            ValorAtual = valorAtual;
            PercentualProgresso = percentualProgresso;
            DiasRestantes = diasRestantes;
            DataConclusaoProjetada = dataConclusaoProjetada;
            StatusAndamento = statusAndamento;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltAcompanhamentoObjetivo>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
