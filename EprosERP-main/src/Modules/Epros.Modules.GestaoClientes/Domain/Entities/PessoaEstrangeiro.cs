using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PessoaEstrangeiro : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string IdentificacaoEstrangeiro { get; private set; } = string.Empty;

        protected PessoaEstrangeiro() { } // EF Core

        public PessoaEstrangeiro(
            Guid pessoaId,
            string nome,
            string identificacaoEstrangeiro,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PessoaEstrangeiro>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O campo Nome é obrigatório.")
                .HasMaxLen(nome, 60, nameof(Nome), "O campo Nome deve ter no máximo 60 caracteres [Origem: PessoaEstrangeiro]")
                .IsNotNullOrEmpty(identificacaoEstrangeiro, nameof(IdentificacaoEstrangeiro), "O campo IdentificacaoEstrangeiro é obrigatório.")
                .HasMaxLen(identificacaoEstrangeiro, 20, nameof(IdentificacaoEstrangeiro), "O campo Identificacao Estrangeiro deve ter no máximo 20 caracteres [Origem: PessoaEstrangeiro]")
            );

            PessoaId = pessoaId;
            Nome = nome;
            IdentificacaoEstrangeiro = identificacaoEstrangeiro;
        }
    }
}
