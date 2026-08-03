using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Alerta de planejamento de estoque (EF Análise e Planejamento §16.3 `ape_alerta_estoque`).
    /// APE-010: saldo ≤ mínimo gera alerta de reposição (gatilho de requisição de compra);
    /// APE-011: saldo/projeção &gt; máximo sinaliza excesso de política. Ciclo aberto→ignorado/resolvido.
    /// </summary>
    public class AlertaEstoque : EntidadeSaaSBase
    {
        public Guid PosicaoEstoqueId { get; private set; }
        public Guid EmpresaId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public ETipoAlertaEstoque TipoAlerta { get; private set; }
        public decimal QuantidadeReferencia { get; private set; }
        public decimal QuantidadeAtual { get; private set; }
        public EStatusAlertaEstoque StatusAlerta { get; private set; } = EStatusAlertaEstoque.Aberto;
        public DateTime DataAlerta { get; private set; }
        public DateTime? ResolvidoEm { get; private set; }
        public string? ResolvidoPor { get; private set; }

        protected AlertaEstoque() { } // EF Core

        public AlertaEstoque(Guid posicaoEstoqueId, Guid empresaId, Guid produtoId, ETipoAlertaEstoque tipoAlerta,
            decimal quantidadeReferencia, decimal quantidadeAtual, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PosicaoEstoqueId = posicaoEstoqueId;
            EmpresaId = empresaId;
            ProdutoId = produtoId;
            TipoAlerta = tipoAlerta;
            QuantidadeReferencia = quantidadeReferencia;
            QuantidadeAtual = quantidadeAtual;
            StatusAlerta = EStatusAlertaEstoque.Aberto;
            DataAlerta = DateTime.UtcNow;
        }

        public void Resolver(string usuario)
        {
            StatusAlerta = EStatusAlertaEstoque.Resolvido;
            ResolvidoEm = DateTime.UtcNow;
            ResolvidoPor = usuario;
            MarcarAlterado(usuario);
        }

        public void Ignorar(string usuario)
        {
            StatusAlerta = EStatusAlertaEstoque.Ignorado;
            ResolvidoEm = DateTime.UtcNow;
            ResolvidoPor = usuario;
            MarcarAlterado(usuario);
        }
    }
}
