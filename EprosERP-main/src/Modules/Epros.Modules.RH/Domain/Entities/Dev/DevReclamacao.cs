using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-DEV, tabela rh_dev_reclamacao). Fidelidade campo a campo.</summary>
    public partial class DevReclamacao : EntidadeSaaSBase
    {
        public Guid? ColaboradorId { get; private set; }
        public Guid? ContraColaboradorId { get; private set; }
        public Guid? TipoReclamacaoId { get; private set; }
        public string? Assunto { get; private set; }
        public string? Descricao { get; private set; }
        public DateTime? DataReclamacao { get; private set; }
        public string? Status { get; private set; }
        public string? Documento { get; private set; }
        public Guid? ResolvidoPor { get; private set; }
        public DateTime? DataResolucao { get; private set; }

        protected DevReclamacao() { } // EF Core

        public DevReclamacao(
            Guid? colaboradorId,
            Guid? contraColaboradorId,
            Guid? tipoReclamacaoId,
            string? assunto,
            string? descricao,
            DateTime? dataReclamacao,
            string? status,
            string? documento,
            Guid? resolvidoPor,
            DateTime? dataResolucao,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            ContraColaboradorId = contraColaboradorId;
            TipoReclamacaoId = tipoReclamacaoId;
            Assunto = assunto;
            Descricao = descricao;
            DataReclamacao = dataReclamacao;
            Status = status;
            Documento = documento;
            ResolvidoPor = resolvidoPor;
            DataResolucao = dataResolucao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<DevReclamacao>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
