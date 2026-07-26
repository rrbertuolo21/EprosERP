using System;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Orcamento
{
    /// <summary>
    /// Marco orcamentario com custo, datas, status e progresso. Origem: EF PRJ-ORC 11.2 (prj_orcamento_marco).
    /// RN-ORC-002 (titulo max 255), RN-ORC-003 (custo >= 0), RN-ORC-005 (fim >= inicio),
    /// RN-ORC-006 (status Incomplete/Complete), RN-ORC-007 (progresso 0..100).
    /// </summary>
    public class MarcoOrcamentario : EntidadeSaaSBase
    {
        public Guid OrcamentoProjetoId { get; private set; }
        public Guid ProjetoId { get; private set; }
        public string Titulo { get; private set; } = string.Empty;
        public decimal Custo { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public string? Resumo { get; private set; }
        public EMarcoStatus Status { get; private set; } = EMarcoStatus.Incomplete;
        public int Progresso { get; private set; }

        protected MarcoOrcamentario() { } // EF Core

        public MarcoOrcamentario(
            Guid orcamentoProjetoId,
            Guid projetoId,
            string titulo,
            decimal custo,
            DateTime dataInicio,
            DateTime dataFim,
            string? resumo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<MarcoOrcamentario>()
                .Requires()
                .AreNotEquals(projetoId, Guid.Empty, nameof(ProjetoId), "O projeto e obrigatorio. [Origem: MarcoOrcamentario]")
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O titulo do marco e obrigatorio. [Origem: MarcoOrcamentario]")
                .IsLowerOrEqualsThan(titulo?.Length ?? 0, 255, nameof(Titulo), "O titulo do marco deve ter no maximo 255 caracteres. [Origem: MarcoOrcamentario]"));

            if (custo < 0)
                AddNotification(nameof(Custo), "O custo do marco deve ser maior ou igual a zero. [Origem: MarcoOrcamentario]");

            if (dataFim < dataInicio)
                AddNotification(nameof(DataFim), "A data final nao pode ser anterior a data inicial. [Origem: MarcoOrcamentario]");

            OrcamentoProjetoId = orcamentoProjetoId;
            ProjetoId = projetoId;
            Titulo = titulo ?? string.Empty;
            Custo = custo;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Resumo = resumo;
            Status = EMarcoStatus.Incomplete;
            Progresso = 0;
        }

        /// <summary>RN-ORC-006/007/012: status e progresso do marco.</summary>
        public void AtualizarProgresso(int progresso, EMarcoStatus status, string usuario)
        {
            if (progresso < 0 || progresso > 100)
            {
                AddNotification(nameof(Progresso), "O progresso deve estar entre 0 e 100. [Origem: MarcoOrcamentario]");
                return;
            }

            if (status == EMarcoStatus.Complete && progresso != 100)
            {
                AddNotification(nameof(Status), "Marco concluido deve ter progresso 100. [Origem: MarcoOrcamentario]");
                return;
            }

            Progresso = progresso;
            Status = status;
            MarcarAlterado(usuario);
        }
    }
}
