using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_solicitacao_licenca). Fidelidade campo a campo.</summary>
    public partial class TltSolicitacaoLicenca : EntidadeSaaSBase
    {
        public Guid? ColaboradorId { get; private set; }
        public Guid? TipoLicencaId { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public int? TotalDias { get; private set; }
        public string? Motivo { get; private set; }
        public string? Anexo { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public string? ComentarioAprovador { get; private set; }
        public DateTime? AprovadoEm { get; private set; }
        public Guid? AprovadoPorId { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }

        protected TltSolicitacaoLicenca() { } // EF Core

        public TltSolicitacaoLicenca(
            Guid? colaboradorId,
            Guid? tipoLicencaId,
            DateTime? dataInicio,
            DateTime? dataFim,
            int? totalDias,
            string? motivo,
            string? anexo,
            string status,
            string? comentarioAprovador,
            DateTime? aprovadoEm,
            Guid? aprovadoPorId,
            Guid? criadoPorId,
            Guid? ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            TipoLicencaId = tipoLicencaId;
            DataInicio = dataInicio;
            DataFim = dataFim;
            TotalDias = totalDias;
            Motivo = motivo;
            Anexo = anexo;
            Status = status;
            ComentarioAprovador = comentarioAprovador;
            AprovadoEm = aprovadoEm;
            AprovadoPorId = aprovadoPorId;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltSolicitacaoLicenca>().Requires();
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
