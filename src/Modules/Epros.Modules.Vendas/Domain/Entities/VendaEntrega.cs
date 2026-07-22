using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaEntrega (endereço de entrega da venda). FK long -> Guid;
    /// VOs Documento/CEP -> string?; herda EntidadeSaaSBase. EnderecoId referencia
    /// endereço de outro módulo por Guid FK.
    /// </summary>
    public class VendaEntrega : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Fone { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string? IE { get; private set; } = string.Empty;
        public string? Documento { get; private set; }
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
        public Guid? EnderecoId { get; private set; }

        // Navegação intra-módulo
        public Venda Venda { get; private set; } = null!;

        protected VendaEntrega() { } // EF Core

        public VendaEntrega(Guid vendaId, string nome, string fone, string email, string? ie, string? documento, EEstado uf, string? logradouro, string? numero, string? complemento, string? bairro, int municipioId, string? municipioNome, string? cep, int paisId, string? paisNome, Guid? enderecoId,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            Nome = nome;
            Fone = fone;
            Email = email;
            // IE não é atribuído no legado (comentado); mantido fiel.
            Documento = documento;
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
            EnderecoId = enderecoId;
        }

        public void Alterar(string nome, string fone, string email, string? ie, string? documento, EEstado uf, string? logradouro, string? numero, string? complemento, string? bairro, int municipioId, string? municipioNome, string? cep, int paisId, string? paisNome, Guid? enderecoId, string alteradoPor)
        {
            Nome = nome;
            Fone = fone;
            Email = email;
            // IE não é atribuído no legado (comentado); mantido fiel.
            Documento = documento;
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
            EnderecoId = enderecoId;
            MarcarAlterado(alteradoPor);
        }
    }
}
