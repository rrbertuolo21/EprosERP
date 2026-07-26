using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_sucessao). Fidelidade campo a campo.</summary>
    public partial class TltSucessao : EntidadeSaaSBase
    {
        public Guid? PosicaoId { get; private set; }
        public Guid? ColaboradorAtualId { get; private set; }
        public Guid? SucessorId { get; private set; }
        public int? ProntidaoMeses { get; private set; }
        public string? RiscoPerda { get; private set; }
        public string? Observacao { get; private set; }

        protected TltSucessao() { } // EF Core

        public TltSucessao(
            Guid? posicaoId,
            Guid? colaboradorAtualId,
            Guid? sucessorId,
            int? prontidaoMeses,
            string? riscoPerda,
            string? observacao,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            PosicaoId = posicaoId;
            ColaboradorAtualId = colaboradorAtualId;
            SucessorId = sucessorId;
            ProntidaoMeses = prontidaoMeses;
            RiscoPerda = riscoPerda;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltSucessao>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
