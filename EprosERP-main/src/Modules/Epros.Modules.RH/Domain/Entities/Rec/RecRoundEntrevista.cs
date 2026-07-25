using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-REC). Fidelidade campo a campo.</summary>
    public partial class RecRoundEntrevista : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public int Sequencia { get; private set; }
        public string? Descricao { get; private set; }
        public int Status { get; private set; }
        public Guid VagaId { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected RecRoundEntrevista() { } // EF Core

        public RecRoundEntrevista(
            string nome,
            int sequencia,
            string? descricao,
            int status,
            Guid vagaId,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Sequencia = sequencia;
            Descricao = descricao;
            Status = status;
            VagaId = vagaId;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<RecRoundEntrevista>().Requires();
            contract.IsNotNullOrEmpty(Nome, nameof(Nome), "O campo Nome e obrigatorio.");
            contract.AreNotEquals(VagaId, Guid.Empty, nameof(VagaId), "O campo VagaId e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
