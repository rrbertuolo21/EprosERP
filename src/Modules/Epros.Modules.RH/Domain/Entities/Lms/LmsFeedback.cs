using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-LMS, tabela rh_lms_feedback). Fidelidade campo a campo.</summary>
    public partial class LmsFeedback : EntidadeSaaSBase
    {
        public Guid TarefaId { get; private set; }
        public Guid UsuarioAlvoId { get; private set; }
        public int Nota { get; private set; }
        public string? Comentarios { get; private set; }
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected LmsFeedback() { } // EF Core

        public LmsFeedback(
            Guid tarefaId,
            Guid usuarioAlvoId,
            int nota,
            string? comentarios,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            TarefaId = tarefaId;
            UsuarioAlvoId = usuarioAlvoId;
            Nota = nota;
            Comentarios = comentarios;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<LmsFeedback>().Requires();
            contract.AreNotEquals(TarefaId, Guid.Empty, nameof(TarefaId), "O campo TarefaId e obrigatorio.");
            contract.AreNotEquals(UsuarioAlvoId, Guid.Empty, nameof(UsuarioAlvoId), "O campo UsuarioAlvoId e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
