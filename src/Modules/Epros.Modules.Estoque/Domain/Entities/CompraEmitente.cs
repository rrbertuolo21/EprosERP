using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Emitente (fornecedor ou empresa própria) da compra. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraEmitente. Os ValueObjects CNPJ/CPF foram achatados para
    /// strings. EmpresaId/PessoaId são FKs Guid para outros módulos (sem navegação cruzada).
    /// </summary>
    public class CompraEmitente : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }
        public Guid? EmpresaId { get; private set; }
        public Guid? PessoaId { get; private set; } // Fornecedor
        public string? Cnpj { get; private set; }
        public string? Cpf { get; private set; }
        public string RazaoSocial { get; private set; } = string.Empty;
        public string? NomeFantasia { get; private set; }
        public string? Telefone { get; private set; }
        public string InscricaoEstadual { get; private set; } = string.Empty;
        public string? InscricaoEstadualST { get; private set; }
        public string? InscricaoMunicipal { get; private set; }
        public int Cnae { get; private set; }
        public ERegimeTributario RegimeTributario { get; private set; }

        // Navegação intra-módulo
        public Compra? Compra { get; private set; }
        public CompraEmitenteEndereco? Endereco { get; private set; }

        protected CompraEmitente() { } // EF Core

        public CompraEmitente(Guid compraId, Guid? empresaId, Guid? pessoaId, string? cnpj, string? cpf, string razaoSocial, string? nomeFantasia, string? telefone, string inscricaoEstadual, string? inscricaoEstadualST, string? inscricaoMunicipal, int cnae, ERegimeTributario regimeTributario, CompraEmitenteEndereco? endereco, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
            EmpresaId = empresaId;
            PessoaId = pessoaId;
            Cnpj = cnpj;
            Cpf = cpf;
            RazaoSocial = razaoSocial ?? string.Empty;
            NomeFantasia = nomeFantasia;
            Telefone = telefone;
            InscricaoEstadual = inscricaoEstadual ?? string.Empty;
            InscricaoEstadualST = inscricaoEstadualST;
            InscricaoMunicipal = inscricaoMunicipal;
            Cnae = cnae;
            RegimeTributario = regimeTributario;
            Endereco = endereco;
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<CompraEmitente>()
                .Requires()
                .IsBetween((RazaoSocial ?? "").Length, 2, 60, nameof(RazaoSocial), "Razão Social do emitente, deve conter entre 2 e 60 caractes")
                .IsLowerOrEqualsThan((NomeFantasia ?? "").Length, 60, nameof(NomeFantasia), "Nome Fantasia emitente pode conter no max 60 caracteres")
                .IsLowerOrEqualsThan((InscricaoEstadual ?? "").Length, 20, nameof(InscricaoEstadual), "Inscrição Estadual pode conter no max 20 caracteres")
                .IsLowerOrEqualsThan((InscricaoEstadualST ?? "").Length, 14, nameof(InscricaoEstadualST), "Inscrição Estadual ST do emitente pode conter entre 2-14 caracteres")
                .IsLowerOrEqualsThan((InscricaoMunicipal ?? "").Length, 15, nameof(InscricaoMunicipal), "Inscrição Municipal do emitente pode conter no max 15 caracteres")
                .IsLowerOrEqualsThan((Telefone ?? "").Length, 14, nameof(Telefone), "Telefone do emitente pode conter no max 14 caracteres")
                .IsLowerOrEqualsThan(Cnae.ToString().Length, 7, nameof(Cnae), "Cnae do emitente pode conter no max 7 caracteres")
            );

            if (string.IsNullOrWhiteSpace(Cnpj) && string.IsNullOrWhiteSpace(Cpf))
                AddNotification("CPF/CNPJ", "CPF/CNPJ pelo menos um dos dois devem ser informados");
        }

        public void Alterar(string? telefone, string inscricaoEstadual, string? inscricaoEstadualST, string? inscricaoMunicipal, int cnae, ERegimeTributario regimeTributario, CompraEmitenteEndereco? endereco, string usuario)
        {
            Telefone = telefone;
            InscricaoEstadual = inscricaoEstadual ?? string.Empty;
            InscricaoEstadualST = inscricaoEstadualST;
            InscricaoMunicipal = inscricaoMunicipal;
            Cnae = cnae;
            RegimeTributario = regimeTributario;
            Endereco = endereco;
            MarcarAlterado(usuario);
            Validar();
        }
    }
}
