using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class ComprovantePagamento : EntidadeSaaSBase
    {
        public Guid PagamentoTransferenciaId { get; private set; }
        public string NomeArquivo { get; private set; } = string.Empty;
        public string CaminhoArquivo { get; private set; } = string.Empty;
        public long TamanhoBytes { get; private set; }
        public decimal Valor { get; private set; }
        public DateTime DataComprovante { get; private set; }
        public string StatusLeitura { get; private set; } = "Unread"; // Unread, Read

        protected ComprovantePagamento() { } // EF Core

        public ComprovantePagamento(
            Guid pagamentoTransferenciaId,
            string nomeArquivo,
            string caminhoArquivo,
            long tamanhoBytes,
            decimal valor,
            DateTime dataComprovante,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ComprovantePagamento>()
                .Requires()
                .AreNotEquals(pagamentoTransferenciaId, Guid.Empty, nameof(PagamentoTransferenciaId), "PagamentoTransferenciaId é obrigatório")
                .IsNotNullOrEmpty(nomeArquivo, nameof(NomeArquivo), "Nome do arquivo é obrigatório")
                .IsNotNullOrEmpty(caminhoArquivo, nameof(CaminhoArquivo), "Caminho do arquivo é obrigatório")
                .IsGreaterThan(tamanhoBytes, 0, nameof(TamanhoBytes), "Tamanho do arquivo deve ser maior que zero")
                .IsGreaterThan(valor, 0, nameof(Valor), "Valor do comprovante deve ser maior que zero")
            );

            PagamentoTransferenciaId = pagamentoTransferenciaId;
            NomeArquivo = nomeArquivo;
            CaminhoArquivo = caminhoArquivo;
            TamanhoBytes = tamanhoBytes;
            Valor = valor;
            DataComprovante = dataComprovante;
            StatusLeitura = "Unread";
        }

        public void MarcarComoLido(string alteradoPor)
        {
            StatusLeitura = "Read";
            MarcarAlterado(alteradoPor);
        }
    }
}
