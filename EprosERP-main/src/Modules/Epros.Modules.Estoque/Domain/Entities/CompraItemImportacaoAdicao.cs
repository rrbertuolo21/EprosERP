using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Adição de uma declaração de importação de item de compra. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraItemImportacaoAdicao.
    /// </summary>
    public class CompraItemImportacaoAdicao : EntidadeSaaSBase
    {
        public Guid CompraItemImportacaoId { get; private set; }
        public int NumeroAdicao { get; private set; }
        public int NumeroSequencialAdicao { get; private set; }
        public string CodigoFabricante { get; private set; } = string.Empty;
        public decimal ValorDesconto { get; private set; }
        public string? NumeroAtoConcessorio { get; private set; }

        // Navegação intra-módulo
        public CompraItemImportacao? CompraItemImportacao { get; private set; }

        protected CompraItemImportacaoAdicao() { } // EF Core

        public CompraItemImportacaoAdicao(Guid compraItemImportacaoId, int numeroAdicao, int numeroSequencialAdicao, string codigoFabricante, decimal valorDesconto, string? numeroAtoConcessorio, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraItemImportacaoId = compraItemImportacaoId;
            NumeroAdicao = numeroAdicao;
            NumeroSequencialAdicao = numeroSequencialAdicao;
            CodigoFabricante = codigoFabricante ?? string.Empty;
            ValorDesconto = valorDesconto;
            NumeroAtoConcessorio = numeroAtoConcessorio;
        }

        public void Alterar(int numeroAdicao, int numeroSequencialAdicao, string codigoFabricante, decimal valorDesconto, string? numeroAtoConcessorio, string usuario)
        {
            NumeroAdicao = numeroAdicao;
            NumeroSequencialAdicao = numeroSequencialAdicao;
            CodigoFabricante = codigoFabricante ?? string.Empty;
            ValorDesconto = valorDesconto;
            NumeroAtoConcessorio = numeroAtoConcessorio;
            MarcarAlterado(usuario);
        }
    }
}
