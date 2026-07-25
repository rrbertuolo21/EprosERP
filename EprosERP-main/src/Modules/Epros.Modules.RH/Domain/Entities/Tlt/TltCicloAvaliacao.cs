using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_ciclo_avaliacao). Fidelidade campo a campo.</summary>
    public partial class TltCicloAvaliacao : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Frequencia { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public Guid? CriadoPorId { get; private set; }
        public Guid OwnerId { get; private set; }

        protected TltCicloAvaliacao() { } // EF Core

        public TltCicloAvaliacao(
            string nome,
            string frequencia,
            string? descricao,
            string status,
            Guid? criadoPorId,
            Guid ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Frequencia = frequencia;
            Descricao = descricao;
            Status = status;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltCicloAvaliacao>().Requires();
            contract.IsNotNullOrEmpty(Nome, nameof(Nome), "O campo Nome e obrigatorio.");
            contract.IsNotNullOrEmpty(Frequencia, nameof(Frequencia), "O campo Frequencia e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            contract.AreNotEquals(OwnerId, Guid.Empty, nameof(OwnerId), "O campo OwnerId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
