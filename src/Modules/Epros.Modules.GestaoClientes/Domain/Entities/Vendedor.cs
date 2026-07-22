using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Vendedor : EntidadeSaaSBase
    {
        public Guid? RevendaId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string? Telefone { get; private set; }
        public decimal PercentualComissao { get; private set; }
        public bool Ativo { get; private set; }

        protected Vendedor() { } // EF Core

        public Vendedor(
            Guid? revendaId,
            string nome,
            string email,
            string? telefone,
            decimal percentualComissao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Vendedor>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome do vendedor é obrigatório")
                .IsEmail(email, nameof(Email), "E-mail inválido")
                .IsGreaterThan(percentualComissao, -0.01m, nameof(PercentualComissao), "Percentual de comissão deve ser maior ou igual a zero")
            );

            RevendaId = revendaId;
            Nome = nome;
            Email = email;
            Telefone = telefone;
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

        public void VincularRevenda(Guid revendaId, string alteradoPor)
        {
            RevendaId = revendaId;
            MarcarAlterado(alteradoPor);
        }

        public void Alterar(string nome, string email, string? telefone, decimal percentualComissao, Guid? revendaId, bool ativo, string alteradoPor)
        {
            AddNotifications(new Contract<Vendedor>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome do vendedor é obrigatório")
                .IsEmail(email, nameof(Email), "E-mail inválido")
                .IsGreaterThan(percentualComissao, -0.01m, nameof(PercentualComissao), "Percentual de comissão deve ser maior ou igual a zero")
            );

            if (IsValid)
            {
                Nome = nome;
                Email = email;
                Telefone = telefone;
                PercentualComissao = percentualComissao;
                RevendaId = revendaId;
                Ativo = ativo;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
