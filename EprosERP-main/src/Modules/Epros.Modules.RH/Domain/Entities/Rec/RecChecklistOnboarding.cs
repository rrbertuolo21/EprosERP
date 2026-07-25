using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-REC). Fidelidade campo a campo.</summary>
    public partial class RecChecklistOnboarding : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public bool Padrao { get; private set; }
        public int Status { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected RecChecklistOnboarding() { } // EF Core

        public RecChecklistOnboarding(
            string nome,
            string? descricao,
            bool padrao,
            int status,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Descricao = descricao;
            Padrao = padrao;
            Status = status;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<RecChecklistOnboarding>().Requires();
            contract.IsNotNullOrEmpty(Nome, nameof(Nome), "O campo Nome e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
