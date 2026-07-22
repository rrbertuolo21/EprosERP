using System;
using Epros.Shared.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PessoaFisica : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public Cpf Cpf { get; private set; } = null!;
        public string Nome { get; private set; } = string.Empty;
        public string? Sobrenome { get; private set; }
        public string? RgNumero { get; private set; }
        public string? RgOrgaoEmissor { get; private set; }
        public ETipoGenero TipoGenero { get; private set; }
        public DateTime? DataNascimento { get; private set; }

        protected PessoaFisica() { } // EF Core

        public PessoaFisica(
            Guid pessoaId,
            Cpf cpf,
            string nome,
            string? sobrenome,
            string? rgNumero,
            string? rgOrgaoEmissor,
            ETipoGenero tipoGenero,
            DateTime? dataNascimento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PessoaFisica>()
                .Requires()
                .IsNotNull(cpf, nameof(Cpf), "CPF inválido")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O campo Nome é obrigatório.")
                .HasMaxLen(nome, 60, nameof(Nome), "O campo Nome deve ter no máximo 60 caracteres [Origem: PessoaFisica]")
                .HasMaxLen(sobrenome ?? string.Empty, 100, nameof(Sobrenome), "O campo Sobrenome deve ter no máximo 100 caracteres [Origem: PessoaFisica]")
                .HasMaxLen(rgNumero ?? string.Empty, 14, nameof(RgNumero), "O campo RgNumero deve ter no máximo 14 caracteres [Origem: PessoaFisica]")
                .HasMaxLen(rgOrgaoEmissor ?? string.Empty, 10, nameof(RgOrgaoEmissor), "O campo RgOrgaoEmissor deve ter no máximo 10 caracteres [Origem: PessoaFisica]")
                .IsTrue(Enum.IsDefined(typeof(ETipoGenero), tipoGenero), nameof(TipoGenero), "TipoGenero não consta na lista [Origem: PessoaFisica]")
            );

            if (cpf != null)
            {
                AddNotifications(cpf.Notifications);
            }

            PessoaId = pessoaId;
            Cpf = cpf!;
            Nome = nome;
            Sobrenome = sobrenome;
            RgNumero = rgNumero;
            RgOrgaoEmissor = rgOrgaoEmissor;
            TipoGenero = tipoGenero;
            DataNascimento = dataNascimento;
        }
    }
}
