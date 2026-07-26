using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PNT). Fidelidade campo a campo.</summary>
    public partial class PntMarcacao : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid RelogioId { get; private set; }
        public int? Nsr { get; private set; }
        public DateTime? DataMarcacao { get; private set; }
        public TimeSpan HoraMarcacao { get; private set; }
        public string TipoMarcacao { get; private set; } = string.Empty;
        public string TipoRegistro { get; private set; } = string.Empty;
        public string ParEntradaSaida { get; private set; } = string.Empty;
        public string Justificativa { get; private set; } = string.Empty;
        public string? Origem { get; private set; }
        public string? Status { get; private set; }

        protected PntMarcacao() { } // EF Core

        public PntMarcacao(
            Guid colaboradorId,
            Guid relogioId,
            int? nsr,
            DateTime? dataMarcacao,
            TimeSpan horaMarcacao,
            string tipoMarcacao,
            string tipoRegistro,
            string parEntradaSaida,
            string justificativa,
            string? origem,
            string? status,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            RelogioId = relogioId;
            Nsr = nsr;
            DataMarcacao = dataMarcacao;
            HoraMarcacao = horaMarcacao;
            TipoMarcacao = tipoMarcacao;
            TipoRegistro = tipoRegistro;
            ParEntradaSaida = parEntradaSaida;
            Justificativa = justificativa;
            Origem = origem;
            Status = status;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PntMarcacao>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.AreNotEquals(RelogioId, Guid.Empty, nameof(RelogioId), "O campo RelogioId e obrigatorio.");
            contract.IsNotNullOrEmpty(TipoMarcacao, nameof(TipoMarcacao), "O campo TipoMarcacao e obrigatorio.");
            contract.IsNotNullOrEmpty(TipoRegistro, nameof(TipoRegistro), "O campo TipoRegistro e obrigatorio.");
            contract.IsNotNullOrEmpty(ParEntradaSaida, nameof(ParEntradaSaida), "O campo ParEntradaSaida e obrigatorio.");
            contract.IsNotNullOrEmpty(Justificativa, nameof(Justificativa), "O campo Justificativa e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
