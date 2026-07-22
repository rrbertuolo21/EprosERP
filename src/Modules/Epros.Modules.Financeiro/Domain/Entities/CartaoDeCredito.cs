using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    public class CartaoDeCredito : EntidadeSaaSBase
    {
        /// <summary>Sequência legada (SequenciaTenantId) — somente exibição/UX. Não substitui o Id (Guid).</summary>
        public long? SequenciaExibicao { get; private set; }

        public Guid ContaBancariaId { get; private set; }
        public string Apelido { get; private set; } = string.Empty;
        public string Titular { get; private set; } = string.Empty;
        public EBandeiraCartao BandeiraCartao { get; private set; }
        public string? Observacao { get; private set; }

        // Navigation
        public ContaBancaria ContaBancaria { get; private set; } = null!;
        public ICollection<CartaoDeCreditoFatura> CartaoDeCreditoFaturas { get; private set; } = new List<CartaoDeCreditoFatura>();

        protected CartaoDeCredito() { } // EF Core

        public CartaoDeCredito(
            Guid contaBancariaId,
            string apelido,
            string titular,
            EBandeiraCartao bandeiraCartao,
            string? observacao,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            ContaBancariaId = contaBancariaId;
            Apelido = apelido;
            Titular = titular;
            BandeiraCartao = bandeiraCartao;
            Observacao = observacao;
            Validar();
        }

        public void Alterar(
            Guid contaBancariaId,
            string apelido,
            string titular,
            EBandeiraCartao bandeiraCartao,
            string? observacao,
            string alteradoPor)
        {
            ContaBancariaId = contaBancariaId;
            Apelido = apelido;
            Titular = titular;
            BandeiraCartao = bandeiraCartao;
            Observacao = observacao;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<CartaoDeCredito>()
                .Requires()
                .AreNotEquals(ContaBancariaId, Guid.Empty, nameof(ContaBancariaId), "A conta bancária é obrigatória")
                .IsNotNullOrEmpty(Apelido, nameof(Apelido), "O apelido é obrigatório")
                .IsLowerOrEqualsThan(Apelido ?? string.Empty, 150, nameof(Apelido), "O apelido deve ter no máximo 150 caracteres")
                .IsNotNullOrEmpty(Titular, nameof(Titular), "O titular é obrigatório")
                .IsLowerOrEqualsThan(Titular ?? string.Empty, 200, nameof(Titular), "O titular deve ter no máximo 200 caracteres")
                .IsLowerOrEqualsThan(Observacao ?? string.Empty, 200, nameof(Observacao), "A observação deve ter no máximo 200 caracteres")
            );
        }
    }
}
