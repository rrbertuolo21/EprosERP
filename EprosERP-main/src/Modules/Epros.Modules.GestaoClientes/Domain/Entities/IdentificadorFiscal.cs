using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Identificador fiscal por país/jurisdição de uma pessoa (CAD-PEM 12.14).</summary>
    public class IdentificadorFiscal : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public Guid PaisId { get; private set; }
        public ETipoIdFiscal Tipo { get; private set; }
        public string Valor { get; private set; } = string.Empty;
        public bool Validado { get; private set; }

        protected IdentificadorFiscal() { } // EF Core

        public IdentificadorFiscal(
            Guid pessoaId,
            Guid paisId,
            ETipoIdFiscal tipo,
            string valor,
            bool validado,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PessoaId = pessoaId;
            PaisId = paisId;
            Tipo = tipo;
            Valor = valor;
            Validado = validado;
            Validar();
        }

        public void Alterar(ETipoIdFiscal tipo, string valor, bool validado, string alteradoPor)
        {
            Tipo = tipo;
            Valor = valor;
            Validado = validado;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void MarcarValidado(bool validado, string alteradoPor)
        {
            Validado = validado;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<IdentificadorFiscal>()
                .Requires()
                .AreNotEquals(PessoaId, Guid.Empty, nameof(PessoaId), "PessoaId é obrigatório [Origem: IdentificadorFiscal]")
                .AreNotEquals(PaisId, Guid.Empty, nameof(PaisId), "PaisId é obrigatório [Origem: IdentificadorFiscal]")
                .IsTrue(Enum.IsDefined(typeof(ETipoIdFiscal), Tipo), nameof(Tipo), "Tipo não consta na lista [Origem: IdentificadorFiscal]")
                .IsNotNullOrEmpty(Valor, nameof(Valor), "Valor é obrigatório [Origem: IdentificadorFiscal]")
                .HasMaxLen(Valor ?? string.Empty, 50, nameof(Valor), "Valor deve ter no máximo 50 caracteres [Origem: IdentificadorFiscal]")
            );
        }
    }
}
