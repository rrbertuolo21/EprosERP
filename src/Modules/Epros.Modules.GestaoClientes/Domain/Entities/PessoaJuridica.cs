using System;
using Epros.Shared.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PessoaJuridica : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public Cnpj Cnpj { get; private set; } = null!;
        public string RazaoSocial { get; private set; } = string.Empty;
        public string? NomeFantasia { get; private set; }
        public string? InscricaoEstadual { get; private set; }
        public string? InscricaoMunicipal { get; private set; }
        public string? Cnae { get; private set; }

        protected PessoaJuridica() { } // EF Core

        public PessoaJuridica(
            Guid pessoaId,
            Cnpj cnpj,
            string razaoSocial,
            string? nomeFantasia,
            string? inscricaoEstadual,
            string? inscricaoMunicipal,
            string? cnae,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PessoaJuridica>()
                .Requires()
                .IsNotNull(cnpj, nameof(Cnpj), "CNPJ inválido")
                .IsNotNullOrEmpty(razaoSocial, nameof(RazaoSocial), "O campo RazaoSocial é obrigatório.")
                .HasMaxLen(razaoSocial, 250, nameof(RazaoSocial), "O campo RazaoSocial deve ter no máximo 250 caracteres [Origem: PessoaJuridica]")
                .HasMaxLen(nomeFantasia ?? string.Empty, 250, nameof(NomeFantasia), "O campo NomeFantasia deve ter no máximo 250 caracteres [Origem: PessoaJuridica]")
                .HasMaxLen(inscricaoEstadual ?? string.Empty, 14, nameof(InscricaoEstadual), "O campo InscricaoEstadual deve ter no máximo 14 caracteres [Origem: PessoaJuridica]")
                .HasMaxLen(inscricaoMunicipal ?? string.Empty, 15, nameof(InscricaoMunicipal), "O campo InscricaoMunicipal deve ter no máximo 15 caracteres [Origem: PessoaJuridica]")
                .HasMaxLen(cnae ?? string.Empty, 7, nameof(Cnae), "O campo Cnae deve ter no máximo 7 caracteres [Origem: PessoaJuridica]")
            );

            if (cnpj != null)
            {
                AddNotifications(cnpj.Notifications);
            }

            PessoaId = pessoaId;
            Cnpj = cnpj!;
            RazaoSocial = razaoSocial;
            NomeFantasia = nomeFantasia;
            InscricaoEstadual = inscricaoEstadual;
            InscricaoMunicipal = inscricaoMunicipal;
            Cnae = cnae;
        }
    }
}
