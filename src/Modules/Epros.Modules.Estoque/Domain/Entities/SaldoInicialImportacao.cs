using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Controle da importação de saldo inicial (EF Movimentação Manual e Ajustes §15.8).
    /// </summary>
    public class SaldoInicialImportacao : EntidadeSaaSBase
    {
        public string ArquivoNome { get; private set; } = string.Empty;
        public EStatusImportacaoSaldoInicial Situacao { get; private set; } = EStatusImportacaoSaldoInicial.Pendente;
        public int LinhasTotal { get; private set; }
        public int LinhasProcessadas { get; private set; }
        public int LinhasErro { get; private set; }

        // Navegação intra-módulo
        public ICollection<SaldoInicialItem> Itens { get; private set; } = new List<SaldoInicialItem>();

        protected SaldoInicialImportacao() { } // EF Core

        public SaldoInicialImportacao(string arquivoNome, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ArquivoNome = arquivoNome ?? string.Empty;
            Situacao = EStatusImportacaoSaldoInicial.Pendente;
        }

        public void Validar() { }

        public void AdicionarItem(SaldoInicialItem item) => Itens.Add(item);

        /// <summary>Consolida o resumo de processamento (linhas aceitas/rejeitadas) — EF §10.4 / CA-MVM-011.</summary>
        public void ConcluirProcessamento(int linhasTotal, int linhasProcessadas, int linhasErro, string usuario)
        {
            LinhasTotal = linhasTotal;
            LinhasProcessadas = linhasProcessadas;
            LinhasErro = linhasErro;
            Situacao = linhasErro > 0 ? EStatusImportacaoSaldoInicial.ConcluidaComErros : EStatusImportacaoSaldoInicial.Concluida;
            MarcarAlterado(usuario);
        }
    }
}
