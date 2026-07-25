using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Revenda : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public decimal PercentualComissao { get; private set; }
        public bool Ativo { get; private set; }

        protected Revenda() { } // EF Core

        public Revenda(string nome, decimal percentualComissao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Revenda>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome da revenda é obrigatório")
                .IsGreaterThan(percentualComissao, -0.01m, nameof(PercentualComissao), "Percentual de comissão deve ser maior ou igual a zero")
            );

            Nome = nome;
            PercentualComissao = percentualComissao;
            Ativo = true;
        }

        public void Desativar(string alteradoPor)
        {
            Ativo = false;
            MarcarAlterado(alteradoPor);
        }

        public void Ativar(string alteradoPor)
        {
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }

        public void AtualizarPercentualComissao(decimal novoPercentual, string alteradoPor)
        {
            AddNotifications(new Contract<Revenda>()
                .Requires()
                .IsGreaterThan(novoPercentual, -0.01m, nameof(PercentualComissao), "Percentual de comissão deve ser maior ou igual a zero")
            );

            if (IsValid)
            {
                PercentualComissao = novoPercentual;
                MarcarAlterado(alteradoPor);
            }
        }

        public void Alterar(string nome, decimal percentualComissao, bool ativo, string alteradoPor)
        {
            AddNotifications(new Contract<Revenda>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome da revenda é obrigatório")
                .IsGreaterThan(percentualComissao, -0.01m, nameof(PercentualComissao), "Percentual de comissão deve ser maior ou igual a zero")
            );

            if (IsValid)
            {
                Nome = nome;
                PercentualComissao = percentualComissao;
                Ativo = ativo;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
