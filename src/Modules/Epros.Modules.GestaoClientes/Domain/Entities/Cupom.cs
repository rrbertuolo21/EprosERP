using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Cupom : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Codigo { get; private set; } = string.Empty;
        public string Tipo { get; private set; } = "Fixo"; // Fixo, Percentual
        public decimal ValorDesconto { get; private set; }
        public int? LimiteUso { get; private set; }
        public int QuantidadeUsos { get; private set; }
        public bool Ativo { get; private set; }
        public DateTime? ValidoAte { get; private set; }

        protected Cupom() { } // EF Core

        public Cupom(
            string codigo,
            string tipo,
            decimal valorDesconto,
            int? limiteUso,
            DateTime? validoAte,
            string tenantId,
            string criadoPor,
            string nome = "")
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Cupom>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "Código do cupom é obrigatório")
                .IsNotNullOrEmpty(tipo, nameof(Tipo), "Tipo de desconto é obrigatório")
                .IsGreaterThan(valorDesconto, 0, nameof(ValorDesconto), "Valor do desconto deve ser maior que zero")
            );

            if (tipo != "Fixo" && tipo != "Percentual")
            {
                AddNotification(nameof(Tipo), "Tipo de desconto inválido. Escolha 'Fixo' ou 'Percentual'.");
            }

            if (tipo == "Percentual" && valorDesconto > 100)
            {
                AddNotification(nameof(ValorDesconto), "Desconto percentual não pode ser maior que 100%.");
            }

            Nome = string.IsNullOrWhiteSpace(nome) ? codigo.ToUpperInvariant() : nome;
            Codigo = codigo.ToUpperInvariant();
            Tipo = tipo;
            ValorDesconto = valorDesconto;
            LimiteUso = limiteUso;
            QuantidadeUsos = 0;
            Ativo = true;
            ValidoAte = validoAte;
        }

        public bool Validar()
        {
            if (!Ativo) return false;
            if (ValidoAte.HasValue && ValidoAte.Value < DateTime.UtcNow) return false;
            if (LimiteUso.HasValue && QuantidadeUsos >= LimiteUso.Value) return false;
            return true;
        }

        public void IncrementarUso(string alteradoPor)
        {
            QuantidadeUsos++;
            MarcarAlterado(alteradoPor);
        }

        public void DecrementarUso(string alteradoPor)
        {
            if (QuantidadeUsos > 0)
            {
                QuantidadeUsos--;
                MarcarAlterado(alteradoPor);
            }
        }

        public void Inativar(string alteradoPor)
        {
            Ativo = false;
            MarcarAlterado(alteradoPor);
        }

        public void Ativar(string alteradoPor)
        {
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }

        public void Atualizar(string nome, string tipo, decimal valorDesconto, int? limiteUso, DateTime? validoAte, string alteradoPor)
        {
            if (tipo != "Fixo" && tipo != "Percentual")
            {
                AddNotification(nameof(Tipo), "Tipo de desconto inválido. Escolha 'Fixo' ou 'Percentual'.");
                return;
            }
            if (tipo == "Percentual" && valorDesconto > 100)
            {
                AddNotification(nameof(ValorDesconto), "Desconto percentual não pode ser maior que 100%.");
                return;
            }
            if (!string.IsNullOrWhiteSpace(nome)) Nome = nome;
            Tipo = tipo;
            ValorDesconto = valorDesconto;
            LimiteUso = limiteUso;
            ValidoAte = validoAte;
            MarcarAlterado(alteradoPor);
        }

        public decimal CalcularDesconto(decimal valorOriginal)
        {
            if (Tipo == "Fixo")
            {
                return Math.Min(ValorDesconto, valorOriginal);
            }
            else // Percentual
            {
                return Math.Round(valorOriginal * (ValorDesconto / 100m), 2);
            }
        }
    }
}
