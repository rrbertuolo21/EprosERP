using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaFatura (fatura/cobrança da venda + duplicatas).
    /// FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaFatura : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
        public string? NumeroFatura { get; private set; }
        public decimal ValorOriginal { get; private set; }
        public decimal ValorDesconto { get; private set; }
        public decimal ValorLiquido { get; private set; }

        private readonly List<VendaFaturaDuplicata> _duplicatas = new();
        public IReadOnlyCollection<VendaFaturaDuplicata> Duplicatas => _duplicatas.AsReadOnly();

        // Navegação intra-módulo
        public Venda Venda { get; private set; } = null!;

        protected VendaFatura() { } // EF Core

        public VendaFatura(Guid vendaId, string? numeroFatura, decimal valorOriginal, decimal valorDesconto, decimal valorLiquido,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            NumeroFatura = numeroFatura;
            ValorOriginal = valorOriginal;
            ValorDesconto = valorDesconto;
            ValorLiquido = valorLiquido;
            Validar();
        }

        /// <summary>Porte fiel de VendaFatura.Validar.</summary>
        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
                .Requires()
                .IsLowerOrEqualsThan(60, (NumeroFatura ?? "").Length, "NumeroFatura", "O Numero da Fatura deve ter no máximo 60 caracteres")
                .IsLowerOrEqualsThan(ValorOriginal, decimal.Zero, "ValorOriginal", "Valor Original deve ser maior que Zero")
                .IsLowerOrEqualsThan(ValorLiquido, decimal.Zero, "ValorLiquido", "Valor Líquido deve ser maior que Zero")
            );
        }

        /// <summary>Porte fiel de VendaFatura.ValorTotalDuplicatasDiferente.</summary>
        public bool ValorTotalDuplicatasDiferente()
        {
            if (_duplicatas.Count > 0)
            {
                var sumDuplicatas = _duplicatas.Select(d => d.ValorDuplicata).Sum();
                if (sumDuplicatas != ValorLiquido)
                    return true;
            }
            return false;
        }

        public void Alterar(string? numeroFatura, decimal valorOriginal, decimal valorDesconto, decimal valorLiquido, string alteradoPor)
        {
            NumeroFatura = numeroFatura;
            ValorOriginal = valorOriginal;
            ValorDesconto = valorDesconto;
            ValorLiquido = valorLiquido;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        /// <summary>Porte fiel de VendaFatura.Deletar (soft delete em cascata das duplicatas).</summary>
        public void DeletarEmCascata(string userId)
        {
            Deletar(userId);
            foreach (var item in _duplicatas)
                item.Deletar(userId);
        }

        public void IncluirDuplicata(string numeroDuplicata, DateTime dataVencimento, decimal valorDuplicata, string criadoPor)
        {
            var duplicata = new VendaFaturaDuplicata(Id, numeroDuplicata, dataVencimento, valorDuplicata, TenantId, criadoPor);
            _duplicatas.Add(duplicata);
            if (duplicata.Notifications.Any())
                AddNotifications(duplicata.Notifications);
        }

        public void AlterarDuplicata(Guid vendaFaturaDuplicataId, string numeroDuplicata, DateTime dataVencimento, decimal valorDuplicata, string alteradoPor)
        {
            var localizado = _duplicatas.FirstOrDefault(x => x.Id == vendaFaturaDuplicataId);
            if (localizado != null)
            {
                localizado.Alterar(numeroDuplicata, dataVencimento, valorDuplicata, alteradoPor);
                if (localizado.Notifications.Any())
                    AddNotifications(localizado.Notifications);
            }
        }

        public void DeletarDuplicata(Guid vendaFaturaDuplicataId, string userId)
        {
            var localizado = _duplicatas.FirstOrDefault(x => x.Id == vendaFaturaDuplicataId);
            localizado?.Deletar(userId);
        }

        /// <summary>Porte fiel de VendaFatura.Duplicar (novo Id/FK, incl. duplicatas).</summary>
        public VendaFatura Duplicar(Guid novaVendaId, string criadoPor)
        {
            var clone = new VendaFatura(novaVendaId, NumeroFatura, ValorOriginal, ValorDesconto, ValorLiquido, TenantId, criadoPor);
            foreach (var duplicata in _duplicatas)
                clone._duplicatas.Add(duplicata.Duplicar(clone.Id, criadoPor));
            return clone;
        }
    }
}
