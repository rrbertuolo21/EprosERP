using System;
using Epros.Shared.Domain.Entities;
using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaFaturaDuplicata. FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaFaturaDuplicata : EntidadeSaaSBase
    {
        public Guid VendaFaturaId { get; private set; }
        public string NumeroDuplicata { get; private set; } = string.Empty;
        public DateTime DataVencimento { get; private set; }
        public decimal ValorDuplicata { get; private set; }

        // Navegação intra-módulo
        public VendaFatura VendaFatura { get; private set; } = null!;

        protected VendaFaturaDuplicata() { } // EF Core

        public VendaFaturaDuplicata(Guid vendaFaturaId, string numeroDuplicata, DateTime dataVencimento, decimal valorDuplicata,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaFaturaId = vendaFaturaId;
            NumeroDuplicata = numeroDuplicata;
            DataVencimento = dataVencimento;
            ValorDuplicata = valorDuplicata;
            Validar();
        }

        /// <summary>Porte fiel de VendaFaturaDuplicata.Validar.</summary>
        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
                .Requires()
                .IsLowerOrEqualsThan(60, (NumeroDuplicata ?? "").Length, "NumeroDuplicata", "O Numero da Duplicata deve ter no máximo 60 caracteres")
                .IsLowerOrEqualsThan(ValorDuplicata, decimal.Zero, "ValorDuplicata", "Valor da Duplicata deve ser maior que Zero")
            );
        }

        public void Alterar(string numeroDuplicata, DateTime dataVencimento, decimal valorDuplicata, string alteradoPor)
        {
            NumeroDuplicata = numeroDuplicata;
            DataVencimento = dataVencimento;
            ValorDuplicata = valorDuplicata;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        /// <summary>Porte fiel de VendaFaturaDuplicata.Duplicar (novo Id/FK).</summary>
        public VendaFaturaDuplicata Duplicar(Guid novaFaturaId, string criadoPor)
            => new(novaFaturaId, NumeroDuplicata, DataVencimento, ValorDuplicata, TenantId, criadoPor);
    }
}
