using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Workflow
{
    /// <summary>
    /// wf_historico — trilha imutável de transições e alterações (usuário, data/hora, IP, antes/depois
    /// e payload). Imutável para usuários operacionais. [Origem: EF WORKFLOW 10.9]
    /// </summary>
    public class WfHistorico : EntidadeSaaSBase
    {
        public Guid? InstanciaId { get; private set; }
        public string EntidadeTipo { get; private set; } = string.Empty;
        public string EntidadeIdReferencia { get; private set; } = string.Empty;
        public string Acao { get; private set; } = string.Empty;
        public string? EstadoAnterior { get; private set; }
        public string? EstadoNovo { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public string? IpOrigem { get; private set; }
        public string? PayloadJson { get; private set; }

        protected WfHistorico() { } // EF Core

        public WfHistorico(
            Guid? instanciaId,
            string entidadeTipo,
            string entidadeIdReferencia,
            string acao,
            string? estadoAnterior,
            string? estadoNovo,
            Guid? usuarioId,
            string? ipOrigem,
            string? payloadJson,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            InstanciaId = instanciaId;
            EntidadeTipo = entidadeTipo;
            EntidadeIdReferencia = entidadeIdReferencia;
            Acao = acao;
            EstadoAnterior = estadoAnterior;
            EstadoNovo = estadoNovo;
            UsuarioId = usuarioId;
            IpOrigem = ipOrigem;
            PayloadJson = payloadJson;

            AddNotifications(new Contract<WfHistorico>()
                .Requires()
                .IsNotNullOrEmpty(entidadeTipo, nameof(EntidadeTipo), "O tipo da entidade auditada é obrigatório [Origem: WfHistorico]")
                .IsNotNullOrEmpty(entidadeIdReferencia, nameof(EntidadeIdReferencia), "O identificador auditado é obrigatório [Origem: WfHistorico]")
                .IsNotNullOrEmpty(acao, nameof(Acao), "A ação/transição é obrigatória [Origem: WfHistorico]"));
        }
    }
}
