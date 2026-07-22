using System;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Workflow
{
    /// <summary>
    /// wf_transicao — evento, estado de origem, estado de destino e permissão exigida. [Origem: EF WORKFLOW 10.5]
    /// </summary>
    public class WfTransicao : EntidadeSaaSBase
    {
        public Guid DefinicaoId { get; private set; }
        public Guid EstadoOrigemId { get; private set; }
        public Guid EstadoDestinoId { get; private set; }
        public EWfEvento Evento { get; private set; }
        public EWfPermissao PermissaoRequerida { get; private set; }
        public bool ExigeComentario { get; private set; }
        public bool PublicaEvento { get; private set; }

        protected WfTransicao() { } // EF Core

        public WfTransicao(
            Guid definicaoId,
            Guid estadoOrigemId,
            Guid estadoDestinoId,
            EWfEvento evento,
            EWfPermissao permissaoRequerida,
            bool exigeComentario,
            bool publicaEvento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            DefinicaoId = definicaoId;
            EstadoOrigemId = estadoOrigemId;
            EstadoDestinoId = estadoDestinoId;
            Evento = evento;
            PermissaoRequerida = permissaoRequerida;
            ExigeComentario = exigeComentario;
            PublicaEvento = publicaEvento;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<WfTransicao>()
                .Requires()
                .AreNotEquals(DefinicaoId, Guid.Empty, nameof(DefinicaoId), "A definição da transição é obrigatória [Origem: WfTransicao]")
                .AreNotEquals(EstadoOrigemId, Guid.Empty, nameof(EstadoOrigemId), "O estado de origem é obrigatório [Origem: WfTransicao]")
                .AreNotEquals(EstadoDestinoId, Guid.Empty, nameof(EstadoDestinoId), "O estado de destino é obrigatório [Origem: WfTransicao]"));
        }
    }
}
