using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Fatura de uma compra, agregando as duplicatas. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraFatura.
    /// </summary>
    public class CompraFatura : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }
        public string? NumeroFatura { get; private set; }
        public decimal ValorOriginal { get; private set; }
        public decimal ValorDesconto { get; private set; }
        public decimal ValorLiquido { get; private set; }

        // Navegação intra-módulo
        public ICollection<CompraFaturaDuplicata> Duplicatas { get; private set; } = new List<CompraFaturaDuplicata>();
        public Compra? Compra { get; private set; }

        protected CompraFatura() { } // EF Core

        public CompraFatura(Guid compraId, string? numeroFatura, decimal valorOriginal, decimal valorDesconto, decimal valorLiquido, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
            NumeroFatura = numeroFatura;
            ValorOriginal = valorOriginal;
            ValorDesconto = valorDesconto;
            ValorLiquido = valorLiquido;
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<CompraFatura>()
                .Requires()
                .IsLowerOrEqualsThan((NumeroFatura ?? "").Length, 60, nameof(NumeroFatura), "O Numero da Fatura deve ter no máximo 60 caracteres")
                .IsGreaterThan(ValorOriginal, decimal.Zero, nameof(ValorOriginal), "Valor Original deve ser maior que Zero")
                .IsGreaterThan(ValorLiquido, decimal.Zero, nameof(ValorLiquido), "Valor Líquido deve ser maior que Zero")
            );
        }

        /// <summary>Porte fiel de CompraFatura.ValorTotalDuplicatasDiferente(): true se a soma das duplicatas difere do valor líquido.</summary>
        public bool ValorTotalDuplicatasDiferente()
        {
            if (Duplicatas != null)
            {
                var sumDuplicatas = Duplicatas.Select(d => d.ValorDuplicata).Sum();
                if (sumDuplicatas != ValorLiquido)
                {
                    return true;
                }
            }
            return false;
        }

        public void Alterar(string? numeroFatura, decimal valorOriginal, decimal valorDesconto, decimal valorLiquido, string usuario)
        {
            NumeroFatura = numeroFatura;
            ValorOriginal = valorOriginal;
            ValorDesconto = valorDesconto;
            ValorLiquido = valorLiquido;
            MarcarAlterado(usuario);
            Validar();
        }

        public void DeletarComDuplicatas(string usuario)
        {
            Deletar(usuario);
            foreach (var item in Duplicatas)
            {
                item.Deletar(usuario);
            }
        }

        public void IncluirDuplicata(string numeroDuplicata, DateTime dataVencimento, decimal valorDuplicata, string criadoPor)
        {
            var duplicata = new CompraFaturaDuplicata(Id, numeroDuplicata, dataVencimento, valorDuplicata, TenantId, criadoPor);

            Duplicatas.Add(duplicata);

            if (duplicata.Notifications.Any())
            {
                AddNotifications(duplicata.Notifications);
            }
        }

        public void AlterarDuplicata(Guid compraFaturaDuplicataId, string numeroDuplicata, DateTime dataVencimento, decimal valorDuplicata, string usuario)
        {
            var localizado = Duplicatas.FirstOrDefault(x => x.Id == compraFaturaDuplicataId);
            if (localizado != null)
            {
                localizado.Alterar(numeroDuplicata, dataVencimento, valorDuplicata, usuario);

                if (localizado.Notifications.Any())
                {
                    AddNotifications(localizado.Notifications);
                }
            }
        }

        public void DeletarDuplicata(Guid compraFaturaDuplicataId, string usuario)
        {
            var localizado = Duplicatas.FirstOrDefault(x => x.Id == compraFaturaDuplicataId);
            localizado?.Deletar(usuario);
        }
    }
}
