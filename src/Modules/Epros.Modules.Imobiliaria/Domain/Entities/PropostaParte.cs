using System;
using Epros.Modules.Imobiliaria.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Parte envolvida na proposta (ID2): proponente ou interessado. FK logica -> PESSOAS.
    /// </summary>
    public class PropostaParte : EntidadeSaaSBase
    {
        public Guid PropostaId { get; private set; }
        public Guid PessoaId { get; private set; }
        public EPapelParteProposta Papel { get; private set; }

        protected PropostaParte() { } // EF Core

        public PropostaParte(Guid pessoaId, EPapelParteProposta papel, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PessoaId = pessoaId;
            Papel = papel;
            Validar();
        }

        internal void VincularAProposta(Guid propostaId) => PropostaId = propostaId;

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PropostaParte>()
                .Requires()
                .AreNotEquals(PessoaId, Guid.Empty, nameof(PessoaId),
                    "A pessoa da parte da proposta e obrigatoria. [Origem: PropostaParte]"));
        }
    }
}
