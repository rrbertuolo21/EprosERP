using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_tipo_licenca). Fidelidade campo a campo.</summary>
    public partial class TltTipoLicenca : EntidadeSaaSBase
    {
        public string? Nome { get; private set; }
        public string? Descricao { get; private set; }
        public int? DiasMaximosAno { get; private set; }
        public bool? Remunerada { get; private set; }
        public string? Cor { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }

        protected TltTipoLicenca() { } // EF Core

        public TltTipoLicenca(
            string? nome,
            string? descricao,
            int? diasMaximosAno,
            bool? remunerada,
            string? cor,
            Guid? criadoPorId,
            Guid? ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Descricao = descricao;
            DiasMaximosAno = diasMaximosAno;
            Remunerada = remunerada;
            Cor = cor;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltTipoLicenca>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
