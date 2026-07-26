using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Dados de contato e endereço do local de entrega da compra
    /// (EF Logística de Entrada §15.2 `lde_local_entrega_compra`).
    /// LDE-002 a LDE-010: compra, nome, fone, email, inscrição, documento, UF, município e país obrigatórios.
    /// LDE-011: logradouro, número, complemento, bairro, município_nome, CEP e país_nome opcionais.
    /// </summary>
    public class LdeLocalEntregaCompra : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Fone { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string InscricaoEstadual { get; private set; } = string.Empty;
        public string Documento { get; private set; } = string.Empty;
        public string Uf { get; private set; } = string.Empty;
        public string? Logradouro { get; private set; }
        public string? Numero { get; private set; }
        public string? Complemento { get; private set; }
        public string? Bairro { get; private set; }
        public Guid MunicipioId { get; private set; }
        public string? MunicipioNome { get; private set; }
        public string? Cep { get; private set; }
        public int PaisId { get; private set; }
        public string? PaisNome { get; private set; }

        protected LdeLocalEntregaCompra() { } // EF Core

        public LdeLocalEntregaCompra(
            Guid compraId, string nome, string fone, string email, string inscricaoEstadual, string documento,
            string uf, string? logradouro, string? numero, string? complemento, string? bairro,
            Guid municipioId, string? municipioNome, string? cep, int paisId, string? paisNome,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
            Nome = nome ?? string.Empty;
            Fone = fone ?? string.Empty;
            Email = email ?? string.Empty;
            InscricaoEstadual = inscricaoEstadual ?? string.Empty;
            Documento = documento ?? string.Empty;
            Uf = uf ?? string.Empty;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            MunicipioId = municipioId;
            MunicipioNome = municipioNome;
            Cep = cep;
            PaisId = paisId;
            PaisNome = paisNome;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LdeLocalEntregaCompra>()
                .Requires()
                .AreNotEquals(CompraId, Guid.Empty, nameof(CompraId), "A compra do local de entrega é obrigatória [LDE-002] [Origem: LdeLocalEntregaCompra]")
                .IsNotNullOrEmpty(Nome, nameof(Nome), "O nome do local de entrega é obrigatório [LDE-003] [Origem: LdeLocalEntregaCompra]")
                .IsNotNullOrEmpty(Fone, nameof(Fone), "O telefone do local de entrega é obrigatório [LDE-004] [Origem: LdeLocalEntregaCompra]")
                .IsNotNullOrEmpty(Email, nameof(Email), "O email do local de entrega é obrigatório [LDE-005] [Origem: LdeLocalEntregaCompra]")
                .IsNotNullOrEmpty(InscricaoEstadual, nameof(InscricaoEstadual), "A inscrição do local de entrega é obrigatória [LDE-006] [Origem: LdeLocalEntregaCompra]")
                .IsNotNullOrEmpty(Documento, nameof(Documento), "O documento do local de entrega é obrigatório [LDE-007] [Origem: LdeLocalEntregaCompra]")
                .IsNotNullOrEmpty(Uf, nameof(Uf), "A UF do local de entrega é obrigatória [LDE-008] [Origem: LdeLocalEntregaCompra]")
                .AreNotEquals(MunicipioId, Guid.Empty, nameof(MunicipioId), "O município do local de entrega é obrigatório [LDE-009] [Origem: LdeLocalEntregaCompra]")
                .IsGreaterThan(PaisId, 0, nameof(PaisId), "O país do local de entrega é obrigatório [LDE-010] [Origem: LdeLocalEntregaCompra]"));
        }
    }
}
