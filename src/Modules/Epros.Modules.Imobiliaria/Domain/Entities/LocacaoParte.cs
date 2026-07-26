using System;
using Epros.Modules.Imobiliaria.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Parte vinculada a locacao: locatario ou fiador (EF GESTAO_IMOBILIARIA 11.8/11.9,
    /// tabelas imo_locacao_locatario / imo_locacao_fiador). Relacao N:N com pessoas (RN-013).
    /// </summary>
    public class LocacaoParte : EntidadeSaaSBase
    {
        public Guid LocacaoId { get; private set; }
        public Guid PessoaId { get; private set; }
        public EPapelParteLocacao Papel { get; private set; }

        protected LocacaoParte() { } // EF Core

        public LocacaoParte(Guid pessoaId, EPapelParteLocacao papel, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PessoaId = pessoaId;
            Papel = papel;
            Validar();
        }

        internal void VincularALocacao(Guid locacaoId) => LocacaoId = locacaoId;

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LocacaoParte>()
                .Requires()
                .AreNotEquals(PessoaId, Guid.Empty, nameof(PessoaId),
                    "A pessoa da parte da locacao e obrigatoria. [Origem: LocacaoParte]"));
        }
    }
}
