using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-LMS, tabela rh_lms_tarefa). Fidelidade campo a campo.</summary>
    public partial class LmsTarefa : EntidadeSaaSBase
    {
        public Guid TreinamentoId { get; private set; }
        public string Titulo { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public DateTime DataLimite { get; private set; }
        public Guid ResponsavelUsuarioId { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected LmsTarefa() { } // EF Core

        public LmsTarefa(
            Guid treinamentoId,
            string titulo,
            string? descricao,
            string status,
            DateTime dataLimite,
            Guid responsavelUsuarioId,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            TreinamentoId = treinamentoId;
            Titulo = titulo;
            Descricao = descricao;
            Status = status;
            DataLimite = dataLimite;
            ResponsavelUsuarioId = responsavelUsuarioId;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<LmsTarefa>().Requires();
            contract.AreNotEquals(TreinamentoId, Guid.Empty, nameof(TreinamentoId), "O campo TreinamentoId e obrigatorio.");
            contract.IsNotNullOrEmpty(Titulo, nameof(Titulo), "O campo Titulo e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            contract.AreNotEquals(ResponsavelUsuarioId, Guid.Empty, nameof(ResponsavelUsuarioId), "O campo ResponsavelUsuarioId e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
