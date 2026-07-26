using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Vistoria do imovel: registra local e descricao (EF GESTAO_IMOBILIARIA 11.5, tabela imo_imovel_vistoria, RN-023).
    /// </summary>
    public class ImovelVistoria : EntidadeSaaSBase
    {
        public Guid ImovelId { get; private set; }
        public string Local { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public DateTime? DataVistoria { get; private set; }

        protected ImovelVistoria() { } // EF Core

        public ImovelVistoria(string local, string descricao, DateTime? dataVistoria, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Local = local;
            Descricao = descricao;
            DataVistoria = dataVistoria?.Date;
            Validar();
        }

        internal void VincularAoImovel(Guid imovelId) => ImovelId = imovelId;

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ImovelVistoria>()
                .Requires()
                .IsNotNullOrEmpty(Local, nameof(Local),
                    "O local da vistoria e obrigatorio. [Origem: ImovelVistoria]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao),
                    "A descricao da vistoria e obrigatoria. [Origem: ImovelVistoria]"));
        }
    }
}
