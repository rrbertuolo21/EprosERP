using System;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Endereço de entrega da compra. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraEntrega. Os ValueObjects Documento e CEP foram
    /// achatados para strings (padrão do módulo).
    /// </summary>
    public class CompraEntrega : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Fone { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string IE { get; private set; } = string.Empty;
        public string Documento { get; private set; } = string.Empty;
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
        public Compra? Compra { get; private set; }

        protected CompraEntrega() { } // EF Core

        public CompraEntrega(Guid compraId, string nome, string fone, string email, string ie, string documento, EEstado uf, string? logradouro, string? numero, string? complemento, string? bairro, int municipioId, string? municipioNome, string? cep, int paisId, string? paisNome, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
            Nome = nome ?? string.Empty;
            Fone = fone ?? string.Empty;
            Email = email ?? string.Empty;
            IE = ie ?? string.Empty;
            Documento = documento ?? string.Empty;
            Uf = uf;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            MunicipioId = municipioId;
            MunicipioNome = municipioNome;
            Cep = cep;
            PaisId = paisId;
            PaisNome = paisNome;
        }

        public void Alterar(string nome, string fone, string email, string ie, string documento, EEstado uf, string? logradouro, string? numero, string? complemento, string? bairro, int municipioId, string? municipioNome, string? cep, int paisId, string? paisNome, string usuario)
        {
            Nome = nome ?? string.Empty;
            Fone = fone ?? string.Empty;
            Email = email ?? string.Empty;
            IE = ie ?? string.Empty;
            Documento = documento ?? string.Empty;
            Uf = uf;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            MunicipioId = municipioId;
            MunicipioNome = municipioNome;
            Cep = cep;
            PaisId = paisId;
            PaisNome = paisNome;
            MarcarAlterado(usuario);
        }
    }
}
