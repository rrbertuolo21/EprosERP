using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Cabeçalho de importação de arquivo OFX (extrato bancário).
    /// Porte fiel do legado Importacoes/ImportacacaoArquivoOfx.
    /// </summary>
    public class ImportacacaoArquivoOfx : EntidadeSaaSBase
    {
        public string CodigoBanco { get; private set; } = string.Empty;
        public string NumeroConta { get; private set; } = string.Empty;
        public string TipoConta { get; private set; } = string.Empty;
        public DateTime DataInicioExtrato { get; private set; }
        public DateTime DataFimExtrato { get; private set; }

        private readonly List<ImportacacaoArquivoOfxTransacao> _transacoes = new();
        public ICollection<ImportacacaoArquivoOfxTransacao> Transacoes => _transacoes;

        protected ImportacacaoArquivoOfx() { } // EF Core

        public ImportacacaoArquivoOfx(string codigoBanco, string numeroConta, string tipoConta,
                                      DateTime dataInicioExtrato, DateTime dataFimExtrato,
                                      string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            CodigoBanco = codigoBanco;
            NumeroConta = numeroConta;
            TipoConta = tipoConta;
            DataInicioExtrato = dataInicioExtrato;
            DataFimExtrato = dataFimExtrato;
            Validar();
        }

        public void Validar()
        {
            foreach (var transacao in _transacoes)
                AddNotifications(transacao.Notifications);
        }

        public void AdicionarTransacao(string identificadorTransacao, DateTime data, decimal valor, string tipo,
                                       string? descricao, string criadoPor)
        {
            var transacao = new ImportacacaoArquivoOfxTransacao(Id, identificadorTransacao, data, valor, tipo,
                descricao, TenantId, criadoPor);
            _transacoes.Add(transacao);

            if (transacao.Notifications.Any())
                AddNotifications(transacao.Notifications);
        }

        public void AdicionarConciliacaoCR(Guid transacaoId, Guid contasAReceberId)
        {
            var localizado = _transacoes.FirstOrDefault(x => x.Id == transacaoId);

            if (localizado != null)
            {
                localizado.ConciliarCR(contasAReceberId);

                if (localizado.Notifications.Any())
                    AddNotifications(localizado.Notifications);
            }
        }

        public void AdicionarConciliacaoCP(Guid transacaoId, Guid contasAPagarId)
        {
            var localizado = _transacoes.FirstOrDefault(x => x.Id == transacaoId);

            if (localizado != null)
            {
                localizado.ConciliarCP(contasAPagarId);

                if (localizado.Notifications.Any())
                    AddNotifications(localizado.Notifications);
            }
        }

        public void DeletarComTransacoes(string deletadoPor)
        {
            foreach (var transacao in _transacoes)
                transacao.Deletar(deletadoPor);

            Deletar(deletadoPor);
        }
    }
}
