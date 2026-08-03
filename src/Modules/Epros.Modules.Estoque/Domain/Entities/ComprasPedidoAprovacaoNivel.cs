using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Nível individual de uma instância de aprovação por alçada (CD3 / EF SOURCING §5.8
    /// `com_pedido_aprovacao`). Um nível por regra aplicável; a decisão (aprovar/reprovar), autor,
    /// data e justificativa são registrados aqui e na auditoria central (T8).
    /// </summary>
    public class ComprasPedidoAprovacaoNivel : EntidadeSaaSBase
    {
        public Guid PedidoAprovacaoId { get; private set; }
        public int Nivel { get; private set; }
        public Guid? AprovadorId { get; private set; }
        public string? PapelAprovador { get; private set; }
        public decimal ValorMinimo { get; private set; }
        public decimal? ValorMaximo { get; private set; }
        public EStatusNivelAprovacaoCompra Status { get; private set; } = EStatusNivelAprovacaoCompra.Pendente;
        public string? DecididoPor { get; private set; }
        public DateTime? DecididoEm { get; private set; }
        public string? Justificativa { get; private set; }

        public ComprasPedidoAprovacao? Pedido { get; private set; }

        protected ComprasPedidoAprovacaoNivel() { } // EF Core

        public ComprasPedidoAprovacaoNivel(
            Guid pedidoAprovacaoId,
            int nivel,
            Guid? aprovadorId,
            string? papelAprovador,
            decimal valorMinimo,
            decimal? valorMaximo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PedidoAprovacaoId = pedidoAprovacaoId;
            Nivel = nivel;
            AprovadorId = aprovadorId;
            PapelAprovador = papelAprovador;
            ValorMinimo = valorMinimo;
            ValorMaximo = valorMaximo;
            Status = EStatusNivelAprovacaoCompra.Pendente;
        }

        public bool EstaPendente() => Status == EStatusNivelAprovacaoCompra.Pendente;

        public void Aprovar(string usuario, string? justificativa)
        {
            Status = EStatusNivelAprovacaoCompra.Aprovado;
            DecididoPor = usuario;
            DecididoEm = DateTime.UtcNow;
            Justificativa = justificativa;
            MarcarAlterado(usuario);
        }

        public void Reprovar(string usuario, string? justificativa)
        {
            Status = EStatusNivelAprovacaoCompra.Reprovado;
            DecididoPor = usuario;
            DecididoEm = DateTime.UtcNow;
            Justificativa = justificativa;
            MarcarAlterado(usuario);
        }
    }
}
