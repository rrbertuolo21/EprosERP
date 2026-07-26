using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-LMS, tabela rh_lms_treinamento). Fidelidade campo a campo.</summary>
    public partial class LmsTreinamento : EntidadeSaaSBase
    {
        public string Titulo { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public Guid TipoTreinamentoId { get; private set; }
        public Guid TreinadorId { get; private set; }
        public Guid FilialId { get; private set; }
        public Guid DepartamentoId { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public TimeSpan HoraInicio { get; private set; }
        public TimeSpan HoraFim { get; private set; }
        public string? Local { get; private set; }
        public int? CapacidadeMaxima { get; private set; }
        public decimal? Custo { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected LmsTreinamento() { } // EF Core

        public LmsTreinamento(
            string titulo,
            string? descricao,
            Guid tipoTreinamentoId,
            Guid treinadorId,
            Guid filialId,
            Guid departamentoId,
            DateTime dataInicio,
            DateTime dataFim,
            TimeSpan horaInicio,
            TimeSpan horaFim,
            string? local,
            int? capacidadeMaxima,
            decimal? custo,
            string status,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Titulo = titulo;
            Descricao = descricao;
            TipoTreinamentoId = tipoTreinamentoId;
            TreinadorId = treinadorId;
            FilialId = filialId;
            DepartamentoId = departamentoId;
            DataInicio = dataInicio;
            DataFim = dataFim;
            HoraInicio = horaInicio;
            HoraFim = horaFim;
            Local = local;
            CapacidadeMaxima = capacidadeMaxima;
            Custo = custo;
            Status = status;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<LmsTreinamento>().Requires();
            contract.IsNotNullOrEmpty(Titulo, nameof(Titulo), "O campo Titulo e obrigatorio.");
            contract.AreNotEquals(TipoTreinamentoId, Guid.Empty, nameof(TipoTreinamentoId), "O campo TipoTreinamentoId e obrigatorio.");
            contract.AreNotEquals(TreinadorId, Guid.Empty, nameof(TreinadorId), "O campo TreinadorId e obrigatorio.");
            contract.AreNotEquals(FilialId, Guid.Empty, nameof(FilialId), "O campo FilialId e obrigatorio.");
            contract.AreNotEquals(DepartamentoId, Guid.Empty, nameof(DepartamentoId), "O campo DepartamentoId e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
