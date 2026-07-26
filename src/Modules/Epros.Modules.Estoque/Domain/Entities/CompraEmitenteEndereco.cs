using System;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Endereço do emitente da compra (1:1 com CompraEmitente). Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraEmitenteEndereco (que no legado era um Notifiable/owned;
    /// aqui é EntidadeSaaSBase com FK para o emitente, para manter o padrão do módulo).
    /// </summary>
    public class CompraEmitenteEndereco : EntidadeSaaSBase
    {
        public Guid CompraEmitenteId { get; private set; }
        public EEstado Uf { get; private set; }
        public string? Logradouro { get; private set; }
        public string? Numero { get; private set; }
        public string? Complemento { get; private set; }
        public string? Bairro { get; private set; }
        public int MunicipioId { get; private set; }
        public string? MunicipioNome { get; private set; }
        public string? Cep { get; private set; }
        public int PaisId { get; private set; } = 1058;
        public string? PaisNome { get; private set; } = "BRASIL";

        // Navegação intra-módulo
        public CompraEmitente? CompraEmitente { get; private set; }

        protected CompraEmitenteEndereco() { } // EF Core

        public CompraEmitenteEndereco(Guid compraEmitenteId, EEstado uf, string? logradouro, string? numero, string? complemento, string? bairro, int codMunicipioIbge, string? nomeMunicipio, string? cep, string tenantId, string criadoPor, int paisId = 1058, string? paisNome = "BRASIL")
            : base(tenantId, criadoPor)
        {
            CompraEmitenteId = compraEmitenteId;
            Uf = uf;
            Logradouro = string.IsNullOrEmpty(logradouro) ? null : logradouro;
            Numero = string.IsNullOrEmpty(numero) ? null : numero;
            Complemento = string.IsNullOrEmpty(complemento) ? null : complemento;
            Bairro = string.IsNullOrEmpty(bairro) ? null : bairro;
            MunicipioId = codMunicipioIbge;
            MunicipioNome = string.IsNullOrEmpty(nomeMunicipio) ? null : nomeMunicipio;
            Cep = string.IsNullOrEmpty(cep) ? null : cep;
            PaisId = paisId == 0 ? 1058 : paisId;
            PaisNome = string.IsNullOrEmpty(paisNome) ? "BRASIL" : paisNome;
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<CompraEmitenteEndereco>()
                .Requires()
                .IsBetween((Logradouro ?? "").Length, 2, 60, nameof(Logradouro), "Logradouro do endereço do emitente, deve conter entre 2 e 60 caractes")
                .IsBetween((Numero ?? "").Length, 1, 60, nameof(Numero), "Numero do endereço do emitente, deve conter entre 1 e 60 caractes")
                .IsTrue((Complemento ?? "").Length == 0 || (Complemento ?? "").Length >= 1 && (Complemento ?? "").Length <= 60, nameof(Complemento), "Complemento do endereço do emitente, deve conter entre 1 e 60 caractes")
                .IsBetween((Bairro ?? "").Length, 2, 60, nameof(Bairro), "Bairro do endereço do emitente, deve conter entre 2 e 60 caractes")
                .IsBetween((MunicipioNome ?? "").Length, 2, 60, nameof(MunicipioNome), "Municipio do endereço do emitente, deve conter entre 2 e 60 caractes")
                .IsTrue((Cep ?? "").Length == 8, nameof(Cep), "CEP do endereço do emitente, deve conter 8 caractes")
                .IsBetween(PaisId.ToString().Length, 1, 4, nameof(PaisId), "Código do Pais do endereço do emitente, deve conter entre 1 e 4 caractes")
                .IsBetween((PaisNome ?? "").Length, 2, 60, nameof(PaisNome), "Nome do Pais do endereço do emitente, deve conter entre 2 e 60 caractes")
            );
        }

        public void Alterar(EEstado uf, string? logradouro, string? numero, string? complemento, string? bairro, int codMunicipioIbge, string? nomeMunicipio, string? cep, string usuario, int paisId = 1058, string? paisNome = "BRASIL")
        {
            Uf = uf;
            Logradouro = string.IsNullOrEmpty(logradouro) ? null : logradouro;
            Numero = string.IsNullOrEmpty(numero) ? null : numero;
            Complemento = string.IsNullOrEmpty(complemento) ? null : complemento;
            Bairro = string.IsNullOrEmpty(bairro) ? null : bairro;
            MunicipioId = codMunicipioIbge;
            MunicipioNome = string.IsNullOrEmpty(nomeMunicipio) ? null : nomeMunicipio;
            Cep = string.IsNullOrEmpty(cep) ? null : cep;
            PaisId = paisId == 0 ? 1058 : paisId;
            PaisNome = string.IsNullOrEmpty(paisNome) ? "BRASIL" : paisNome;
            MarcarAlterado(usuario);
            Validar();
        }
    }
}
