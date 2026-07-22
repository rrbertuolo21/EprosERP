using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Duplicata (parcela) de uma CompraFatura. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraFaturaDuplicata.
    /// </summary>
    public class CompraFaturaDuplicata : EntidadeSaaSBase
    {
        public Guid CompraFaturaId { get; private set; }
        public string NumeroDuplicata { get; private set; } = string.Empty;
        public DateTime DataVencimento { get; private set; }
        public decimal ValorDuplicata { get; private set; }

        // Navegação intra-módulo
        public CompraFatura? CompraFatura { get; private set; }

        protected CompraFaturaDuplicata() { } // EF Core

        public CompraFaturaDuplicata(Guid compraFaturaId, string numeroDuplicata, DateTime dataVencimento, decimal valorDuplicata, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraFaturaId = compraFaturaId;
            NumeroDuplicata = numeroDuplicata;
            DataVencimento = dataVencimento;
            ValorDuplicata = valorDuplicata;
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<CompraFaturaDuplicata>()
                .Requires()
                .IsLowerOrEqualsThan((NumeroDuplicata ?? "").Length, 60, nameof(NumeroDuplicata), "O Numero da Duplicata deve ter no máximo 60 caracteres")
                .IsGreaterThan(ValorDuplicata, decimal.Zero, nameof(ValorDuplicata), "Valor da Duplicata deve ser maior que Zero")
            );
        }

        public void Alterar(string numeroDuplicata, DateTime dataVencimento, decimal valorDuplicata, string usuario)
        {
            NumeroDuplicata = numeroDuplicata;
            DataVencimento = dataVencimento;
            ValorDuplicata = valorDuplicata;
            MarcarAlterado(usuario);
            Validar();
        }
    }
}
