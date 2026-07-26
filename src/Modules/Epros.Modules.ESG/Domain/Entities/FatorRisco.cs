using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Fator de risco ocupacional com exposicao, intensidade e controles (EF GESTAO_AMBIENTAL_EHS 11.3). RN-EHS-010.</summary>
    public class FatorRisco : EntidadeSaaSBase
    {
        public Guid? AtividadeId { get; private set; }
        public Guid? IdFolhaPpp { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public string Tipo { get; private set; } = string.Empty;
        public string FatorRiscoDescricao { get; private set; } = string.Empty;
        public string Intensidade { get; private set; } = string.Empty;
        public string TecnicaUtilizada { get; private set; } = string.Empty;
        public string EpcEficaz { get; private set; } = string.Empty;
        public string EpiEficaz { get; private set; } = string.Empty;
        public string? CaEpi { get; private set; }
        public string AtendimentoNr061 { get; private set; } = string.Empty;
        public string AtendimentoNr062 { get; private set; } = string.Empty;
        public string AtendimentoNr063 { get; private set; } = string.Empty;
        public string AtendimentoNr064 { get; private set; } = string.Empty;
        public string AtendimentoNr065 { get; private set; } = string.Empty;

        protected FatorRisco() { } // EF Core

        public FatorRisco(
            Guid? atividadeId,
            Guid? idFolhaPpp,
            DateTime? dataInicio,
            DateTime? dataFim,
            string tipo,
            string fatorRiscoDescricao,
            string intensidade,
            string tecnicaUtilizada,
            string epcEficaz,
            string epiEficaz,
            string? caEpi,
            string atendimentoNr061,
            string atendimentoNr062,
            string atendimentoNr063,
            string atendimentoNr064,
            string atendimentoNr065,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AtividadeId = atividadeId;
            IdFolhaPpp = idFolhaPpp;
            DataInicio = dataInicio?.Date;
            DataFim = dataFim?.Date;
            Tipo = tipo;
            FatorRiscoDescricao = fatorRiscoDescricao;
            Intensidade = intensidade;
            TecnicaUtilizada = tecnicaUtilizada;
            EpcEficaz = epcEficaz;
            EpiEficaz = epiEficaz;
            CaEpi = caEpi;
            AtendimentoNr061 = atendimentoNr061;
            AtendimentoNr062 = atendimentoNr062;
            AtendimentoNr063 = atendimentoNr063;
            AtendimentoNr064 = atendimentoNr064;
            AtendimentoNr065 = atendimentoNr065;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<FatorRisco>()
                .Requires()
                // RN-EHS-010: exige Tipo, FatorRisco, Intensidade e TecnicaUtilizada.
                .IsNotNullOrEmpty(Tipo, nameof(Tipo), "O tipo do fator de risco e obrigatorio. [Origem: FatorRisco]")
                .IsNotNullOrEmpty(FatorRiscoDescricao, nameof(FatorRiscoDescricao), "O fator de risco e obrigatorio. [Origem: FatorRisco]")
                .IsNotNullOrEmpty(Intensidade, nameof(Intensidade), "A intensidade e obrigatoria. [Origem: FatorRisco]")
                .IsNotNullOrEmpty(TecnicaUtilizada, nameof(TecnicaUtilizada), "A tecnica utilizada e obrigatoria. [Origem: FatorRisco]")
                // RN-EHS-008: periodo valido.
                .IsFalse(DataInicio.HasValue && DataFim.HasValue && DataFim.Value < DataInicio.Value,
                    nameof(DataFim), "O fim deve ser igual ou posterior ao inicio. [Origem: FatorRisco]"));
        }
    }
}
