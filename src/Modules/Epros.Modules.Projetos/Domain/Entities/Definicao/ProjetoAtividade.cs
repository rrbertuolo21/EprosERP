using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Definicao
{
    /// <summary>
    /// Evento funcional do projeto (ActivityLog). Origem: EF PRJ-DEF 11.5 (prj_def_projeto_atividade).
    /// RN-DEF-019: atividade registra tipo, usuario, projeto e observacao.
    /// </summary>
    public class ProjetoAtividade : EntidadeSaaSBase
    {
        public Guid ProjetoId { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public string? TipoUsuario { get; private set; }
        public string TipoAtividade { get; private set; } = string.Empty;
        public string? Observacao { get; private set; }

        protected ProjetoAtividade() { } // EF Core

        public ProjetoAtividade(Guid projetoId, Guid? usuarioId, string? tipoUsuario, string tipoAtividade, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ProjetoAtividade>()
                .Requires()
                .AreNotEquals(projetoId, Guid.Empty, nameof(ProjetoId), "O projeto e obrigatorio. [Origem: ProjetoAtividade]")
                .IsNotNullOrEmpty(tipoAtividade, nameof(TipoAtividade), "O tipo da atividade e obrigatorio. [Origem: ProjetoAtividade]"));

            ProjetoId = projetoId;
            UsuarioId = usuarioId;
            TipoUsuario = tipoUsuario;
            TipoAtividade = tipoAtividade;
            Observacao = observacao;
        }
    }
}
