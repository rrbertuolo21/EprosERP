using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Vinculo de pessoa proprietaria ao imovel (EF GESTAO_IMOBILIARIA 11.2, tabela imo_imovel_proprietario).
    /// Relacao 1:N a partir do imovel; ao menos um proprietario e obrigatorio (RN-001).
    /// </summary>
    public class ImovelProprietario : EntidadeSaaSBase
    {
        public Guid ImovelId { get; private set; }
        public Guid PessoaId { get; private set; }

        protected ImovelProprietario() { } // EF Core

        public ImovelProprietario(Guid pessoaId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PessoaId = pessoaId;
            Validar();
        }

        internal void VincularAoImovel(Guid imovelId) => ImovelId = imovelId;

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ImovelProprietario>()
                .Requires()
                .AreNotEquals(PessoaId, Guid.Empty, nameof(PessoaId),
                    "O proprietario e obrigatorio. [Origem: ImovelProprietario]"));
        }
    }
}
