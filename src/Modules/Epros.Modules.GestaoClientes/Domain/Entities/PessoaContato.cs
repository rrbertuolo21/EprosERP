using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PessoaContato : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public string? Nome { get; private set; }
        public ETipoContatoTelefonico TipoContatoTelefonico { get; private set; }
        public string? NumeroTelefone { get; private set; }
        public ETipoContatoEmail TipoContatoEmail { get; private set; }
        public string? Email { get; private set; }
        public bool EhPrincipal { get; private set; }

        protected PessoaContato() { } // EF Core

        public PessoaContato(
            Guid pessoaId,
            string? nome,
            ETipoContatoTelefonico tipoContatoTelefonico,
            string? numeroTelefone,
            ETipoContatoEmail tipoContatoEmail,
            string? email,
            bool ehPrincipal,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PessoaContato>()
                .Requires()
                .HasMaxLen(nome ?? string.Empty, 60, nameof(Nome), "O campo Nome deve ter no máximo 60 caracteres [Origem: PessoaContato]")
                .HasMaxLen(numeroTelefone ?? string.Empty, 14, nameof(NumeroTelefone), "O campo NumeroTelefone deve ter no máximo 14 caracteres [Origem: PessoaContato]")
                .IsTrue(Enum.IsDefined(typeof(ETipoContatoEmail), tipoContatoEmail), nameof(TipoContatoEmail), "TipoContatoEmail não consta na lista [Origem: PessoaContato]")
                .IsTrue(Enum.IsDefined(typeof(ETipoContatoTelefonico), tipoContatoTelefonico), nameof(TipoContatoTelefonico), "TipoContatoTelefonico não consta na lista [Origem: PessoaContato]")
            );

            PessoaId = pessoaId;
            Nome = nome;
            TipoContatoTelefonico = tipoContatoTelefonico;
            NumeroTelefone = numeroTelefone;
            TipoContatoEmail = tipoContatoEmail;
            Email = email;
            EhPrincipal = ehPrincipal;
        }

        public void DefinirComoPrincipal(bool ehPrincipal)
        {
            EhPrincipal = ehPrincipal;
        }
    }
}
