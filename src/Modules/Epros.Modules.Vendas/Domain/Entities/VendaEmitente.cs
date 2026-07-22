using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaEmitente. FK long -> Guid; VOs CNPJ/CPF -> string?; herda EntidadeSaaSBase.
    /// EmpresaId referencia empresa em outro módulo (plataforma) por Guid FK.
    /// </summary>
    public class VendaEmitente : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
        public Guid EmpresaId { get; private set; }
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

        // Navegações intra-módulo
        public Venda Venda { get; private set; } = null!;
        public VendaEmitenteEndereco? Endereco { get; private set; }

        protected VendaEmitente() { } // EF Core

        public VendaEmitente(Guid vendaId, Guid empresaId, string? cnpj, string? cpf, string razaoSocial, string? nomeFantasia, string? telefone, string inscricaoEstadual, string? inscricaoEstadualST, string? inscricaoMunicipal, int cnae, ERegimeTributario regimeTributario, VendaEmitenteEndereco? endereco,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            EmpresaId = empresaId;
            Cnpj = cnpj;
            Cpf = cpf;
            RazaoSocial = (razaoSocial ?? "").Trim();
            NomeFantasia = (nomeFantasia ?? "").Trim();
            Telefone = telefone;
            InscricaoEstadual = inscricaoEstadual;
            InscricaoEstadualST = inscricaoEstadualST;
            InscricaoMunicipal = inscricaoMunicipal;
            Cnae = cnae;
            RegimeTributario = regimeTributario;
            Endereco = endereco;
            Validar();
        }

        public void Alterar(string? telefone, string inscricaoEstadual, string? inscricaoEstadualST, string? inscricaoMunicipal, int cnae, ERegimeTributario regimeTributario, VendaEmitenteEndereco? endereco, string alteradoPor)
        {
            Telefone = telefone;
            InscricaoEstadual = inscricaoEstadual;
            InscricaoEstadualST = inscricaoEstadualST;
            InscricaoMunicipal = inscricaoMunicipal;
            Cnae = cnae;
            RegimeTributario = regimeTributario;
            Endereco = endereco;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        /// <summary>Associa o endereço do emitente (parte do agregado do emitente).</summary>
        public void DefinirEndereco(VendaEmitenteEndereco endereco)
        {
            Endereco = endereco;
            AddNotifications(endereco.Notifications);
        }

        /// <summary>Porte fiel de VendaEmitente.Duplicar (novo Id/FK, incl. endereço).</summary>
        public VendaEmitente Duplicar(Guid novaVendaId, string criadoPor)
        {
            var clone = new VendaEmitente(novaVendaId, EmpresaId, Cnpj, Cpf, RazaoSocial, NomeFantasia, Telefone,
                InscricaoEstadual, InscricaoEstadualST, InscricaoMunicipal, Cnae, RegimeTributario, null, TenantId, criadoPor);
            if (Endereco is not null)
                clone.Endereco = Endereco.Duplicar(clone.Id, criadoPor);
            return clone;
        }

        /// <summary>Porte fiel de VendaEmitente.Validar.</summary>
        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
                .Requires()
                .IsBetween((RazaoSocial ?? "").Length, 2, 60, "RazaoSocial", "Razão Social do emitente, deve conter entre 2 e 60 caractes")
                .IsLowerOrEqualsThan(60, (NomeFantasia ?? "").Length, "NomeFantasia", "Nome Fantasia emitente pode conter no max 60 caracteres")
                .IsLowerOrEqualsThan(20, (InscricaoEstadual ?? "").Length, "InscricaoEstadual", "Inscrição Estadual pode conter no max 20 caracteres ")
                .IsLowerOrEqualsThan(14, (InscricaoEstadualST ?? "").Length, "InscricaoEstadualST", "Inscrição Estadual ST do emitente pode conter entre 2-14 caracteres ")
                .IsLowerOrEqualsThan(15, (InscricaoMunicipal ?? "").Length, "InscricaoMunicipal", "Inscrição Municipal do emitente pode conter no max 15 caracteres ")
                .IsLowerOrEqualsThan(14, (Telefone ?? "").Length, "Telefone", "Telefone do emitente pode conter no max 14 caracteres ")
                .IsLowerOrEqualsThan(7, Cnae.ToString().Length, "Cnae", "Cnae do emitente pode conter no max 7 caracteres ")
            );

            if (!Enum.IsDefined(typeof(ERegimeTributario), RegimeTributario))
                AddNotification("RegimeTributario", "Regime tributário informado inválido ");

            if (string.IsNullOrWhiteSpace(Cnpj) && string.IsNullOrWhiteSpace(Cpf))
                AddNotification("CPF/CNPJ", "CPF/CNPJ pelo menos um dos dois devem ser informados ");
        }
    }
}
