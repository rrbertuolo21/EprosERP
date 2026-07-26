using System;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaEmitenteEndereco. Era owned-type (Notifiable) no legado;
    /// portado como entidade própria (FK Guid para VendaEmitente) herdando EntidadeSaaSBase.
    /// </summary>
    public class VendaEmitenteEndereco : EntidadeSaaSBase
    {
        public Guid VendaEmitenteId { get; private set; }
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
        public VendaEmitente VendaEmitente { get; private set; } = null!;

        protected VendaEmitenteEndereco() { } // EF Core

        public VendaEmitenteEndereco(Guid vendaEmitenteId, EEstado uf, string logradouro, string numero, string complemento, string bairro, int codMunicipioIbge, string nomeMunicipio, string cep,
            string tenantId, string criadoPor, int paisId = 1058, string paisNome = "BRASIL") : base(tenantId, criadoPor)
        {
            VendaEmitenteId = vendaEmitenteId;
            Uf = uf;
            Logradouro = string.IsNullOrEmpty(logradouro) ? null : logradouro;
            Numero = string.IsNullOrEmpty(numero) ? null : numero;
            Complemento = string.IsNullOrEmpty(complemento) ? null : complemento;
            Bairro = string.IsNullOrEmpty(bairro) ? null : bairro;
            MunicipioId = codMunicipioIbge;
            MunicipioNome = string.IsNullOrEmpty(nomeMunicipio) ? null : nomeMunicipio;
            Cep = string.IsNullOrEmpty(cep) ? null : RemoveMascaras(cep);
            PaisId = paisId == 0 ? 1058 : paisId;
            PaisNome = string.IsNullOrEmpty(paisNome) ? "BRASIL" : paisNome;
            Validar();
        }

        public void Alterar(EEstado uf, string logradouro, string numero, string complemento, string bairro, int codMunicipioIbge, string nomeMunicipio, string cep, string alteradoPor, int paisId = 1058, string paisNome = "BRASIL")
        {
            Uf = uf;
            Logradouro = string.IsNullOrEmpty(logradouro) ? null : logradouro;
            Numero = string.IsNullOrEmpty(numero) ? null : numero;
            Complemento = string.IsNullOrEmpty(complemento) ? null : complemento;
            Bairro = string.IsNullOrEmpty(bairro) ? null : bairro;
            MunicipioId = codMunicipioIbge;
            MunicipioNome = string.IsNullOrEmpty(nomeMunicipio) ? null : nomeMunicipio;
            Cep = string.IsNullOrEmpty(cep) ? null : RemoveMascaras(cep);
            PaisId = paisId == 0 ? 1058 : paisId;
            PaisNome = string.IsNullOrEmpty(paisNome) ? "BRASIL" : paisNome;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        /// <summary>Porte fiel de VendaEmitenteEndereco.Validar.</summary>
        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
                .Requires()
                .IsBetween((Logradouro ?? "").Length, 2, 60, "Logradouro", "Logradouro do endereço do emitente, deve conter entre 2 e 60 caractes")
                .Requires()
                .IsBetween((Numero ?? "").Length, 1, 60, "Numero", "Numero do endereço do emitente, deve conter entre 1 e 60 caractes")
                .IsTrue((Complemento ?? "").Length == 0 || (Complemento ?? "").Length >= 1 && (Complemento ?? "").Length <= 60, "Complemento", "Complemento do endereço do emitente, deve conter entre 1 e 60 caractes")
                .Requires()
                .IsBetween((Bairro ?? "").Length, 2, 60, "Bairro", "Bairro do endereço do emitente, deve conter entre 2 e 60 caractes")
                .Requires()
                .IsBetween((MunicipioNome ?? "").Length, 2, 60, "MunicipioNome", "Municipio do endereço do emitente, deve conter entre 2 e 60 caractes")
                .Requires()
                .IsTrue((Cep ?? "").Length == 8, "Cep", "CEP do endereço do emitente, deve conter 8 caractes")
                .Requires()
                .IsBetween(PaisId.ToString().Length, 1, 4, "Cep", "Código do Pais do endereço do emitente, deve conter entre 1 e 4 caractes")
                .Requires()
                .IsBetween((PaisNome ?? "").Length, 2, 60, "PaisNome", "Nome do Pais do endereço do emitente, deve conter entre 2 e 60 caractes")
            );

            if (!Enum.IsDefined(typeof(EEstado), Uf))
                AddNotification("Uf", "UF do endereço do emitente, inválido");
        }

        /// <summary>Porte fiel de VendaEmitenteEndereco.Duplicar (novo Id/FK).</summary>
        public VendaEmitenteEndereco Duplicar(Guid novoEmitenteId, string criadoPor)
            => new(novoEmitenteId, Uf, Logradouro ?? string.Empty, Numero ?? string.Empty, Complemento ?? string.Empty,
                   Bairro ?? string.Empty, MunicipioId, MunicipioNome ?? string.Empty, Cep ?? string.Empty, TenantId, criadoPor, PaisId, PaisNome ?? "BRASIL");

        private static string RemoveMascaras(string valor)
            => new string((valor ?? string.Empty).Where(char.IsLetterOrDigit).ToArray());
    }
}
