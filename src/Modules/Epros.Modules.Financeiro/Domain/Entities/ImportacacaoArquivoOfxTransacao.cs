using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Transação individual de um extrato OFX importado, com flag de conciliação
    /// contra Contas a Receber ou Contas a Pagar.
    /// Porte fiel do legado Importacoes/ImportacacaoArquivoOfxTransacao.
    /// </summary>
    public class ImportacacaoArquivoOfxTransacao : EntidadeSaaSBase
    {
        public Guid ImportacacaoArquivoOfxId { get; private set; }
        public Guid? ContasAReceberId { get; private set; }
        public Guid? ContasAPagarId { get; private set; }

        public string IdentificadorTransacao { get; private set; } = string.Empty;
        public DateTime Data { get; private set; }
        public decimal Valor { get; private set; }
        public string Tipo { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public bool Conciliado { get; private set; }

        // EF (navegação intra-módulo)
        public ImportacacaoArquivoOfx ImportacacaoArquivoOfx { get; private set; } = null!;
        public ContasAReceber? ContasAReceber { get; private set; }
        public ContasAPagar? ContasAPagar { get; private set; }

        protected ImportacacaoArquivoOfxTransacao() { } // EF Core

        public ImportacacaoArquivoOfxTransacao(Guid importacacaoArquivoOfxId, string identificadorTransacao,
                                               DateTime data, decimal valor, string tipo, string? descricao,
                                               string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            ImportacacaoArquivoOfxId = importacacaoArquivoOfxId;
            IdentificadorTransacao = identificadorTransacao;
            Data = data;
            Valor = valor;
            Tipo = tipo;
            Descricao = descricao;
        }

        public void ConciliarCR(Guid contasAReceberId)
        {
            if (!Conciliado && contasAReceberId != Guid.Empty)
            {
                Conciliado = true;
                ContasAReceberId = contasAReceberId;
                MarcarAlterado(CriadoPor ?? "system");
            }
        }

        public void EstornarConciliacaoCR()
        {
            Conciliado = false;
            ContasAReceberId = null;
            MarcarAlterado(CriadoPor ?? "system");
        }

        public void ConciliarCP(Guid contasAPagarId)
        {
            if (!Conciliado && contasAPagarId != Guid.Empty)
            {
                Conciliado = true;
                ContasAPagarId = contasAPagarId;
                MarcarAlterado(CriadoPor ?? "system");
            }
        }

        public void EstornarConciliacaoCP()
        {
            Conciliado = false;
            ContasAPagarId = null;
            MarcarAlterado(CriadoPor ?? "system");
        }

        public void DeletarTransacao(string deletadoPor)
        {
            ContasAReceberId = null;
            ContasAPagarId = null;
            Deletar(deletadoPor);
        }
    }
}
