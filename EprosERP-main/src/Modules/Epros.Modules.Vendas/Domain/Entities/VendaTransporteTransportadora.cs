using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaTransporteTransportadora. FK long -> Guid; VOs CNPJ/CPF -> string?;
    /// PessoaId (transportadora) referencia pessoa de outro módulo por Guid FK. Herda EntidadeSaaSBase.
    /// </summary>
    public class VendaTransporteTransportadora : EntidadeSaaSBase
    {
        public Guid VendaTransporteId { get; private set; }
        public Guid? PessoaId { get; private set; }
        public string? Cnpj { get; private set; }
        public string? Cpf { get; private set; }
        public string? RazaoSocial { get; private set; }
        public string? InscricaoEstadual { get; private set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Municipio { get; set; }
        public EEstado Uf { get; private set; }

        // Navegação intra-módulo
        public VendaTransporte VendaTransporte { get; private set; } = null!;

        protected VendaTransporteTransportadora() { } // EF Core

        public VendaTransporteTransportadora(Guid vendaTransporteId, Guid? pessoaId, string? cnpj, string? cpf, string? razaoSocial, string? inscricaoEstadual, string? logradouro, string? numero, string? complemento, string? bairro, string? municipio, EEstado uf,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaTransporteId = vendaTransporteId;
            PessoaId = pessoaId;
            Cnpj = cnpj;
            Cpf = cpf;
            RazaoSocial = razaoSocial;
            InscricaoEstadual = inscricaoEstadual;
            Logradouro = (logradouro ?? "")[..Math.Min((logradouro ?? "").Length, 60)];
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Municipio = municipio;
            Uf = uf;
            Validar();
        }

        public void Alterar(Guid? pessoaId, string? cnpj, string? cpf, string? razaoSocial, string? inscricaoEstadual, string? logradouro, string? numero, string? complemento, string? bairro, string? municipio, EEstado uf, string alteradoPor)
        {
            PessoaId = pessoaId;
            Cnpj = cnpj;
            Cpf = cpf;
            RazaoSocial = razaoSocial;
            InscricaoEstadual = inscricaoEstadual;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Municipio = municipio;
            Uf = uf;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        /// <summary>Porte fiel de VendaTransporteTransportadora.Validar.</summary>
        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
                .Requires()
                .IsBetween((RazaoSocial ?? "").Length, 2, 60, "RazaoSocial", "Razão Social do emitente, deve conter entre 2 e 60 caractes")
                .IsLowerOrEqualsThan(20, (InscricaoEstadual ?? "").Length, "InscricaoEstadual", "Inscrição Estadual pode conter no max 20 caracteres ")
                .IsLowerOrEqualsThan(60, (Logradouro ?? "").Length, "Logradouro", "Logradouro informado na venda transporte pode conter no max 60 caracteres")
                .IsLowerOrEqualsThan(60, (Numero ?? "").Length, "Numero", "Número informado na venda transporte pode conter no max 60 caracteres")
                .IsLowerOrEqualsThan(60, (Complemento ?? "").Length, "Complemento", "Complemento informado na venda transporte pode conter no max 60 caracteres")
                .IsLowerOrEqualsThan(60, (Bairro ?? "").Length, "Bairro", "Bairro informado na venda transporte pode conter no max 60 caracteres")
                .IsLowerOrEqualsThan(60, (Municipio ?? "").Length, "Municipio", "Nome Municipio informado na venda transporte pode conter no max 60 caracteres")
            );
        }

        /// <summary>Porte fiel de VendaTransporteTransportadora.ObterEnderecoCom60Caracteres.</summary>
        public string ObterEnderecoCom60Caracteres()
        {
            var enderecoCompleto = $"{Logradouro}, {Numero}";
            if (!string.IsNullOrEmpty(Bairro))
                enderecoCompleto += $", {Bairro}";
            if (!string.IsNullOrEmpty(Complemento))
                enderecoCompleto += $", {Complemento}";

            return enderecoCompleto.Length > 60 ? enderecoCompleto[..60] : enderecoCompleto;
        }
    }
}
