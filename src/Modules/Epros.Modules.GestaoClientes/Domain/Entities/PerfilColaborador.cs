using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    // Perfil operacional do colaborador (cargo, departamento, limite de desconto).
    // NÃO confundir com PerfilAcesso (RBAC / menu dinâmico do tenant).
    public class PerfilColaborador : EntidadeSaaSBase
    {
        public string UserId { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Cargo { get; private set; } = string.Empty;
        public string Departamento { get; private set; } = string.Empty;
        public decimal LimiteDesconto { get; private set; }
        public bool Ativo { get; private set; }
        public List<UsuarioPermissao> Permissoes { get; private set; } = new();

        protected PerfilColaborador() { } // EF Core

        public PerfilColaborador(
            string userId,
            string nome,
            string email,
            string cargo,
            string departamento,
            decimal limiteDesconto,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PerfilColaborador>()
                .Requires()
                .IsNotNullOrEmpty(userId, nameof(UserId), "UserId é obrigatório")
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome é obrigatório")
                .IsEmail(email, nameof(Email), "E-mail inválido")
                .IsNotNullOrEmpty(cargo, nameof(Cargo), "Cargo é obrigatório")
                .IsNotNullOrEmpty(departamento, nameof(Departamento), "Departamento é obrigatório")
                .IsGreaterThan(limiteDesconto, -1, nameof(LimiteDesconto), "Limite de desconto deve ser igual ou maior que zero")
            );

            UserId = userId;
            Nome = nome;
            Email = email;
            Cargo = cargo;
            Departamento = departamento;
            LimiteDesconto = limiteDesconto;
            Ativo = true;
        }

        public PerfilColaborador(
            Guid id,
            string userId,
            string nome,
            string email,
            string cargo,
            string departamento,
            decimal limiteDesconto,
            string tenantId,
            string criadoPor)
            : base(id, Guid.NewGuid(), tenantId, criadoPor, DateTime.UtcNow)
        {
            AddNotifications(new Contract<PerfilColaborador>()
                .Requires()
                .IsNotNullOrEmpty(userId, nameof(UserId), "UserId é obrigatório")
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome é obrigatório")
                .IsEmail(email, nameof(Email), "E-mail inválido")
                .IsNotNullOrEmpty(cargo, nameof(Cargo), "Cargo é obrigatório")
                .IsNotNullOrEmpty(departamento, nameof(Departamento), "Departamento é obrigatório")
                .IsGreaterThan(limiteDesconto, -1, nameof(LimiteDesconto), "Limite de desconto deve ser igual ou maior que zero")
            );

            UserId = userId;
            Nome = nome;
            Email = email;
            Cargo = cargo;
            Departamento = departamento;
            LimiteDesconto = limiteDesconto;
            Ativo = true;
        }

        public void Atualizar(string nome, string cargo, string departamento, decimal limiteDesconto, string alteradoPor)
        {
            AddNotifications(new Contract<PerfilColaborador>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome é obrigatório")
                .IsNotNullOrEmpty(cargo, nameof(Cargo), "Cargo é obrigatório")
                .IsNotNullOrEmpty(departamento, nameof(Departamento), "Departamento é obrigatório")
                .IsGreaterThan(limiteDesconto, -1, nameof(LimiteDesconto), "Limite de desconto deve ser igual ou maior que zero")
            );

            if (IsValid)
            {
                Nome = nome;
                Cargo = cargo;
                Departamento = departamento;
                LimiteDesconto = limiteDesconto;
                MarcarAlterado(alteradoPor);
            }
        }

        public void AdicionarPermissao(string recurso, string acao, bool permitido, string criadoPor)
        {
            var permissao = new UsuarioPermissao(Id, recurso, acao, permitido, TenantId, criadoPor);
            if (permissao.IsValid)
            {
                Permissoes.Add(permissao);
            }
            else
            {
                AddNotifications(permissao.Notifications);
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
    }
}
