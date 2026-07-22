using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Consentimento/base legal de tratamento de dados de um titular (CAD-PEM 12.15, LGPD).</summary>
    public class ConsentimentoTitular : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public EFinalidadeDados Finalidade { get; private set; }
        public EBaseLegalPrivacidade BaseLegal { get; private set; }
        public DateTime? DataConsentimento { get; private set; }
        public DateTime? DataRevogacao { get; private set; }
        public string? Canal { get; private set; }

        protected ConsentimentoTitular() { } // EF Core

        public ConsentimentoTitular(
            Guid pessoaId,
            EFinalidadeDados finalidade,
            EBaseLegalPrivacidade baseLegal,
            DateTime? dataConsentimento,
            string? canal,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PessoaId = pessoaId;
            Finalidade = finalidade;
            BaseLegal = baseLegal;
            DataConsentimento = dataConsentimento ?? DateTime.UtcNow;
            Canal = canal;
            Validar();
        }

        public void Revogar(string alteradoPor)
        {
            DataRevogacao = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ConsentimentoTitular>()
                .Requires()
                .AreNotEquals(PessoaId, Guid.Empty, nameof(PessoaId), "PessoaId é obrigatório [Origem: ConsentimentoTitular]")
                .IsTrue(Enum.IsDefined(typeof(EFinalidadeDados), Finalidade), nameof(Finalidade), "Finalidade não consta na lista [Origem: ConsentimentoTitular]")
                .IsTrue(Enum.IsDefined(typeof(EBaseLegalPrivacidade), BaseLegal), nameof(BaseLegal), "BaseLegal não consta na lista [Origem: ConsentimentoTitular]")
                .HasMaxLen(Canal ?? string.Empty, 100, nameof(Canal), "Canal deve ter no máximo 100 caracteres [Origem: ConsentimentoTitular]")
            );
        }
    }
}
