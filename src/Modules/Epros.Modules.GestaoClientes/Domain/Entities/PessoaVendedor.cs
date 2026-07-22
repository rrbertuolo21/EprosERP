using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Extensão de papel Vendedor (CAD-PEM 12.7). 1:1 com Pessoa via PessoaId.</summary>
    public class PessoaVendedor : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        // REG-PEM-039: senha de aplicativo tamanho máximo 6. Guardar como segredo/hash, nunca em claro.
        public string? SenhaAPP { get; private set; }
        public string? Email { get; private set; }
        public decimal? Meta { get; private set; }
        public bool Gestor { get; private set; }
        public string? FormaDesconto { get; private set; }
        public string? TipoDesconto { get; private set; }

        protected PessoaVendedor() { } // EF Core

        public PessoaVendedor(
            Guid pessoaId,
            string? senhaApp,
            string? email,
            decimal? meta,
            bool gestor,
            string? formaDesconto,
            string? tipoDesconto,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PessoaId = pessoaId;
            SenhaAPP = senhaApp;
            Email = email;
            Meta = meta;
            Gestor = gestor;
            FormaDesconto = formaDesconto;
            TipoDesconto = tipoDesconto;
            Validar();
        }

        public void Alterar(
            string? email,
            decimal? meta,
            bool gestor,
            string? formaDesconto,
            string? tipoDesconto,
            string alteradoPor)
        {
            Email = email;
            Meta = meta;
            Gestor = gestor;
            FormaDesconto = formaDesconto;
            TipoDesconto = tipoDesconto;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void DefinirSenhaApp(string? senhaApp, string alteradoPor)
        {
            SenhaAPP = senhaApp;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PessoaVendedor>()
                .Requires()
                .AreNotEquals(PessoaId, Guid.Empty, nameof(PessoaId), "PessoaId é obrigatório [Origem: PessoaVendedor]")
                .HasMaxLen(SenhaAPP ?? string.Empty, 6, nameof(SenhaAPP), "SenhaAPP deve ter no máximo 6 caracteres [Origem: PessoaVendedor]")
                .HasMaxLen(Email ?? string.Empty, 150, nameof(Email), "Email deve ter no máximo 150 caracteres [Origem: PessoaVendedor]")
            );
        }
    }
}
