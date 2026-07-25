using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Definicao
{
    /// <summary>
    /// Vinculo entre projeto e membro/gestor da equipe. Origem: EF PRJ-DEF 11.3 (prj_def_projeto_membro).
    /// RN-DEF-004/006/007: equipe minima, gestor e membros permitidos.
    /// </summary>
    public class ProjetoMembro : EntidadeSaaSBase
    {
        public Guid ProjetoId { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string Papel { get; private set; } = string.Empty; // Membro, Gestor, Responsavel

        protected ProjetoMembro() { } // EF Core

        public ProjetoMembro(Guid projetoId, Guid usuarioId, string papel, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ProjetoMembro>()
                .Requires()
                .AreNotEquals(projetoId, Guid.Empty, nameof(ProjetoId), "O projeto e obrigatorio. [Origem: ProjetoMembro]")
                .AreNotEquals(usuarioId, Guid.Empty, nameof(UsuarioId), "O usuario e obrigatorio. [Origem: ProjetoMembro]"));

            ProjetoId = projetoId;
            UsuarioId = usuarioId;
            Papel = string.IsNullOrWhiteSpace(papel) ? "Membro" : papel;
        }
    }
}
