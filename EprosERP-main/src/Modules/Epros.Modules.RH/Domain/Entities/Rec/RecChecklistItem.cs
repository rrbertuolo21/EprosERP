using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-REC). Fidelidade campo a campo.</summary>
    public partial class RecChecklistItem : EntidadeSaaSBase
    {
        public Guid ChecklistId { get; private set; }
        public string NomeTarefa { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public string? Categoria { get; private set; }
        public string? PapelResponsavel { get; private set; }
        public int? PrazoDias { get; private set; }
        public bool Obrigatorio { get; private set; }
        public int Status { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected RecChecklistItem() { } // EF Core

        public RecChecklistItem(
            Guid checklistId,
            string nomeTarefa,
            string? descricao,
            string? categoria,
            string? papelResponsavel,
            int? prazoDias,
            bool obrigatorio,
            int status,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ChecklistId = checklistId;
            NomeTarefa = nomeTarefa;
            Descricao = descricao;
            Categoria = categoria;
            PapelResponsavel = papelResponsavel;
            PrazoDias = prazoDias;
            Obrigatorio = obrigatorio;
            Status = status;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<RecChecklistItem>().Requires();
            contract.AreNotEquals(ChecklistId, Guid.Empty, nameof(ChecklistId), "O campo ChecklistId e obrigatorio.");
            contract.IsNotNullOrEmpty(NomeTarefa, nameof(NomeTarefa), "O campo NomeTarefa e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
