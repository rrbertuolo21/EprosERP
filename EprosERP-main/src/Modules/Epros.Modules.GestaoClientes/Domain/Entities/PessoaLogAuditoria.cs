using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Log de auditoria de alteração/acesso de pessoa/empresa (CAD-PEM 12.15, REG-PEM-159).</summary>
    public class PessoaLogAuditoria : EntidadeSaaSBase
    {
        public string Entidade { get; private set; } = string.Empty;
        public Guid EntidadeId { get; private set; }
        public string? Campo { get; private set; }
        public string? ValorAnterior { get; private set; }
        public string? ValorNovo { get; private set; }
        public string UsuarioId { get; private set; } = string.Empty;
        public DateTime DataEvento { get; private set; }
        public string TipoEvento { get; private set; } = string.Empty;

        protected PessoaLogAuditoria() { } // EF Core

        public PessoaLogAuditoria(
            string entidade,
            Guid entidadeId,
            string? campo,
            string? valorAnterior,
            string? valorNovo,
            string usuarioId,
            string tipoEvento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Entidade = entidade;
            EntidadeId = entidadeId;
            Campo = campo;
            ValorAnterior = valorAnterior;
            ValorNovo = valorNovo;
            UsuarioId = usuarioId;
            DataEvento = DateTime.UtcNow;
            TipoEvento = tipoEvento;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PessoaLogAuditoria>()
                .Requires()
                .IsNotNullOrEmpty(Entidade, nameof(Entidade), "Entidade é obrigatória [Origem: PessoaLogAuditoria]")
                .HasMaxLen(Entidade ?? string.Empty, 100, nameof(Entidade), "Entidade deve ter no máximo 100 caracteres [Origem: PessoaLogAuditoria]")
                .AreNotEquals(EntidadeId, Guid.Empty, nameof(EntidadeId), "EntidadeId é obrigatório [Origem: PessoaLogAuditoria]")
                .IsNotNullOrEmpty(UsuarioId, nameof(UsuarioId), "UsuarioId é obrigatório [Origem: PessoaLogAuditoria]")
                .IsNotNullOrEmpty(TipoEvento, nameof(TipoEvento), "TipoEvento é obrigatório [Origem: PessoaLogAuditoria]")
                .HasMaxLen(TipoEvento ?? string.Empty, 50, nameof(TipoEvento), "TipoEvento deve ter no máximo 50 caracteres [Origem: PessoaLogAuditoria]")
                .HasMaxLen(Campo ?? string.Empty, 100, nameof(Campo), "Campo deve ter no máximo 100 caracteres [Origem: PessoaLogAuditoria]")
            );
        }
    }
}
