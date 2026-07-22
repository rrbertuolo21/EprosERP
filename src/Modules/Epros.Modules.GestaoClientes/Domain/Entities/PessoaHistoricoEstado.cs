using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Histórico de transição de estado de uma pessoa (CAD-PEM 12.15, REG-PEM-158).</summary>
    public class PessoaHistoricoEstado : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public EEstadoPessoa? EstadoAnterior { get; private set; }
        public EEstadoPessoa EstadoNovo { get; private set; }
        public string? Motivo { get; private set; }
        public string UsuarioId { get; private set; } = string.Empty;
        public DateTime DataEvento { get; private set; }
        public string? Ip { get; private set; }

        protected PessoaHistoricoEstado() { } // EF Core

        public PessoaHistoricoEstado(
            Guid pessoaId,
            EEstadoPessoa? estadoAnterior,
            EEstadoPessoa estadoNovo,
            string? motivo,
            string usuarioId,
            string? ip,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PessoaId = pessoaId;
            EstadoAnterior = estadoAnterior;
            EstadoNovo = estadoNovo;
            Motivo = motivo;
            UsuarioId = usuarioId;
            DataEvento = DateTime.UtcNow;
            Ip = ip;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PessoaHistoricoEstado>()
                .Requires()
                .AreNotEquals(PessoaId, Guid.Empty, nameof(PessoaId), "PessoaId é obrigatório [Origem: PessoaHistoricoEstado]")
                .IsTrue(Enum.IsDefined(typeof(EEstadoPessoa), EstadoNovo), nameof(EstadoNovo), "EstadoNovo não consta na lista [Origem: PessoaHistoricoEstado]")
                .IsNotNullOrEmpty(UsuarioId, nameof(UsuarioId), "UsuarioId é obrigatório [Origem: PessoaHistoricoEstado]")
                .HasMaxLen(Motivo ?? string.Empty, 300, nameof(Motivo), "Motivo deve ter no máximo 300 caracteres [Origem: PessoaHistoricoEstado]")
                .HasMaxLen(Ip ?? string.Empty, 45, nameof(Ip), "Ip deve ter no máximo 45 caracteres [Origem: PessoaHistoricoEstado]")
            );
        }
    }
}
