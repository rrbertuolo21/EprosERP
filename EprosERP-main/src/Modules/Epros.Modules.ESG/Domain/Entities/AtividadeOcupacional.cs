using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Atividade ocupacional com periodo e descricao (EF GESTAO_AMBIENTAL_EHS 11.1). RN-EHS-008/009.</summary>
    public class AtividadeOcupacional : EntidadeSaaSBase
    {
        public Guid? RegistroEhsId { get; private set; }
        public Guid? IdFolhaPpp { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public string Descricao { get; private set; } = string.Empty;

        protected AtividadeOcupacional() { } // EF Core

        public AtividadeOcupacional(
            Guid? registroEhsId,
            Guid? idFolhaPpp,
            DateTime? dataInicio,
            DateTime? dataFim,
            string descricao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            RegistroEhsId = registroEhsId;
            IdFolhaPpp = idFolhaPpp;
            DataInicio = dataInicio?.Date;
            DataFim = dataFim?.Date;
            Descricao = descricao;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<AtividadeOcupacional>()
                .Requires()
                // RN-EHS-009: atividade exige descricao.
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A atividade exige descricao. [Origem: AtividadeOcupacional]")
                // RN-EHS-008: periodo com inicio menor ou igual ao fim.
                .IsFalse(DataInicio.HasValue && DataFim.HasValue && DataFim.Value < DataInicio.Value,
                    nameof(DataFim), "O fim da atividade deve ser igual ou posterior ao inicio. [Origem: AtividadeOcupacional]"));
        }
    }
}
