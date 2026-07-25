using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>
    /// Contato de uma Empresa. Porte fiel de Epros.ERP.Domain.Entities.Cadastros.Empresas.EmpresaContato.
    /// FK EmpresaId (long -> Guid). Email era Value Object (Email) no legado; portado como string com validação de tamanho.
    /// </summary>
    public class EmpresaContato : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string? Email { get; private set; }
        public ETipoContatoTelefonico? TipoTelefone { get; private set; }
        public string? Telefone { get; private set; }

        protected EmpresaContato() { } // EF Core

        public EmpresaContato(
            Guid empresaId,
            string nome,
            string? email,
            ETipoContatoTelefonico? tipo,
            string? numero,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Nome = nome;
            Email = email;
            TipoTelefone = tipo;
            Telefone = numero;
            Validar();
        }

        public void Alterar(string nome, string? email, ETipoContatoTelefonico? tipo, string? numero, string alteradoPor)
        {
            Nome = nome;
            Email = email;
            TipoTelefone = tipo;
            Telefone = numero;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<EmpresaContato>()
                .Requires()
                .AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "EmpresaId campo obrigatório [Origem: EmpresaContato]")
                .IsTrue((Nome ?? "").Length >= 2 && (Nome ?? "").Length <= 150, nameof(Nome), "Nome do contato, deve conter entre 1 e 150 caractes [Origem: EmpresaContato]")
                .HasMaxLen(Email ?? string.Empty, 150, nameof(Email), "O E-mail deve ter no máximo 150 caracteres [Origem: EmpresaContato]")
                .IsTrue((Telefone ?? "").Length == 0 || (Telefone ?? "").Length == 10 || (Telefone ?? "").Length == 11, nameof(Telefone), "Telefone do contato, deve conter 10 e 11 caractes [Origem: EmpresaContato]")
            );

            if (TipoTelefone.HasValue && !Enum.IsDefined(typeof(ETipoContatoTelefonico), TipoTelefone.Value) && !string.IsNullOrEmpty(Telefone))
                AddNotifications(new Contract<EmpresaContato>().Requires()
                    .IsTrue(false, "Numero", "Número do telefone informado inválido. ex [11999999999 ou 1199999999] [Origem: EmpresaContato]"));
        }
    }
}
