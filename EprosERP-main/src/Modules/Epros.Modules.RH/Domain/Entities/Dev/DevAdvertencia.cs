using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-DEV, tabela rh_dev_advertencia). Fidelidade campo a campo.</summary>
    public partial class DevAdvertencia : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid? TipoAdvertenciaId { get; private set; }
        public string? Assunto { get; private set; }
        public string? Severidade { get; private set; }
        public DateTime? DataAdvertencia { get; private set; }
        public string? Descricao { get; private set; }
        public string? Documento { get; private set; }
        public Guid? AdvertidoPor { get; private set; }
        public string? Status { get; private set; }
        public string? RespostaColaborador { get; private set; }

        protected DevAdvertencia() { } // EF Core

        public DevAdvertencia(
            Guid colaboradorId,
            Guid? tipoAdvertenciaId,
            string? assunto,
            string? severidade,
            DateTime? dataAdvertencia,
            string? descricao,
            string? documento,
            Guid? advertidoPor,
            string? status,
            string? respostaColaborador,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            TipoAdvertenciaId = tipoAdvertenciaId;
            Assunto = assunto;
            Severidade = severidade;
            DataAdvertencia = dataAdvertencia;
            Descricao = descricao;
            Documento = documento;
            AdvertidoPor = advertidoPor;
            Status = status;
            RespostaColaborador = respostaColaborador;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<DevAdvertencia>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
