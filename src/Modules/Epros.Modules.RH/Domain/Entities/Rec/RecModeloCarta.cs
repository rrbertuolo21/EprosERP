using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-REC). Fidelidade campo a campo.</summary>
    public partial class RecModeloCarta : EntidadeSaaSBase
    {
        public string Idioma { get; private set; } = string.Empty;
        public string Conteudo { get; private set; } = string.Empty;
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected RecModeloCarta() { } // EF Core

        public RecModeloCarta(
            string idioma,
            string conteudo,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Idioma = idioma;
            Conteudo = conteudo;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<RecModeloCarta>().Requires();
            contract.IsNotNullOrEmpty(Idioma, nameof(Idioma), "O campo Idioma e obrigatorio.");
            contract.IsNotNullOrEmpty(Conteudo, nameof(Conteudo), "O campo Conteudo e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
