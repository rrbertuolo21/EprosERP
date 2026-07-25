using System;
using System.Linq;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaDestinatarioEndereco. FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaDestinatarioEndereco : EntidadeSaaSBase
    {
        public Guid VendaDestinatarioId { get; private set; }
        public ETipoEndereco TipoEndereco { get; private set; }
        public EEstado Uf { get; private set; }
        public string? Logradouro { get; private set; }
        public string? Numero { get; private set; }
        public string? Complemento { get; private set; }
        public string? Bairro { get; private set; }
        public int MunicipioId { get; private set; }
        public string? MunicipioNome { get; private set; }
        public string? Cep { get; private set; }
        public int PaisId { get; private set; }
        public string? PaisNome { get; private set; }

        // Navegação intra-módulo
        public VendaDestinatario VendaDestinatario { get; private set; } = null!;

        protected VendaDestinatarioEndereco() { } // EF Core

        public VendaDestinatarioEndereco(Guid vendaDestinatarioId, ETipoEndereco tipoEndereco, EEstado uf, string logradouro, string numero, string complemento, string bairro, int municipioId, string municipioNome, string cep,
            string tenantId, string criadoPor, int paisId = 1058, string paisNome = "BRASIL") : base(tenantId, criadoPor)
        {
            VendaDestinatarioId = vendaDestinatarioId;
            TipoEndereco = tipoEndereco;
            Uf = uf;
            Logradouro = string.IsNullOrEmpty(logradouro) ? null : logradouro;
            Numero = string.IsNullOrEmpty(numero) ? null : numero;
            Complemento = string.IsNullOrEmpty(complemento) ? null : complemento;
            Bairro = string.IsNullOrEmpty(bairro) ? null : bairro;
            MunicipioId = municipioId;
            MunicipioNome = string.IsNullOrEmpty(municipioNome) ? null : municipioNome;
            Cep = string.IsNullOrEmpty(cep) ? null : RemoveMascaras(cep);
            PaisId = paisId == 0 ? 1058 : paisId;
            PaisNome = string.IsNullOrEmpty(paisNome) ? "BRASIL" : paisNome;
            Validar();
        }

        public void Alterar(EEstado uf, string logradouro, string numero, string complemento, string bairro, int municipioId, string municipioNome, string cep, string alteradoPor, int paisId = 1058, string paisNome = "BRASIL")
        {
            Uf = uf;
            Logradouro = string.IsNullOrEmpty(logradouro) ? null : logradouro;
            Numero = string.IsNullOrEmpty(numero) ? null : numero;
            Complemento = string.IsNullOrEmpty(complemento) ? null : complemento;
            Bairro = string.IsNullOrEmpty(bairro) ? null : bairro;
            MunicipioId = municipioId;
            MunicipioNome = string.IsNullOrEmpty(municipioNome) ? null : municipioNome;
            Cep = string.IsNullOrEmpty(cep) ? null : RemoveMascaras(cep);
            PaisId = paisId == 0 ? 1058 : paisId;
            PaisNome = string.IsNullOrEmpty(paisNome) ? "BRASIL" : paisNome;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        /// <summary>Porte fiel de VendaDestinatarioEndereco.Validar.</summary>
        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
                .Requires()
                .IsBetween((Logradouro ?? "").Length, 2, 60, "Logradouro", "Logradouro do endereço do destinatario, deve conter entre 2 e 60 caractes")
                .Requires()
                .IsBetween((Numero ?? "").Length, 1, 60, "Numero", "Numero do endereço do destinatario, deve conter entre 1 e 60 caractes")
                .IsTrue((Complemento ?? "").Length == 0 || (Complemento ?? "").Length >= 1 && (Complemento ?? "").Length <= 60, "Complemento", "Complemento do endereço do destinatario, deve conter entre 1 e 60 caractes")
                .Requires()
                .IsBetween((Bairro ?? "").Length, 2, 60, "Bairro", "Bairro do endereço do destinatario, deve conter entre 2 e 60 caractes")
                .Requires()
                .IsBetween((MunicipioNome ?? "").Length, 2, 60, "MunicipioNome", "Municipio do endereço do destinatario, deve conter entre 2 e 60 caractes")
                .Requires()
                .IsTrue((Cep ?? "").Length == 8, "Cep", "CEP do endereço do destinatario, deve conter 8 caractes")
                .Requires()
                .IsBetween(PaisId.ToString().Length, 1, 4, "PaisId", "Código do Pais do endereço do destinatario, deve conter entre 1 e 4 caractes")
                .Requires()
                .IsBetween((PaisNome ?? "").Length, 2, 60, "PaisNome", "Nome do Pais do endereço do destinatario, deve conter entre 2 e 60 caractes")
            );

            if (!Enum.IsDefined(typeof(EEstado), Uf))
                AddNotification("Uf", "UF do endereço do destinatario inválido");
        }

        /// <summary>Porte fiel de VendaDestinatarioEndereco.Duplicar (novo Id/FK).</summary>
        public VendaDestinatarioEndereco Duplicar(Guid novoDestinatarioId, string criadoPor)
            => new(novoDestinatarioId, TipoEndereco, Uf, Logradouro ?? string.Empty, Numero ?? string.Empty, Complemento ?? string.Empty,
                   Bairro ?? string.Empty, MunicipioId, MunicipioNome ?? string.Empty, Cep ?? string.Empty, TenantId, criadoPor, PaisId, PaisNome ?? "BRASIL");

        private static string RemoveMascaras(string valor)
            => new string((valor ?? string.Empty).Where(char.IsLetterOrDigit).ToArray());
    }
}
