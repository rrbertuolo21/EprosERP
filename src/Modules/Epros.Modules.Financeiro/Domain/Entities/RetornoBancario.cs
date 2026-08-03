using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Registro de importação/processamento de RETORNO bancário (EF FIN-SF §7.6 sf_retorno).
    /// Auditoria do arquivo processado: layout detectado, quantidade de ocorrências, baixas efetivadas
    /// e rejeitadas, e hash do conteúdo (anti-duplicidade / retenção — FD-TEC2). O arquivo não é
    /// excluído sem permissão (§10.5).
    /// </summary>
    public class RetornoBancario : EntidadeSaaSBase
    {
        public string NomeArquivo { get; private set; } = string.Empty;
        public ELayoutCnab Layout { get; private set; }
        public DateTime DataProcessamento { get; private set; }
        public string HashArquivo { get; private set; } = string.Empty;
        public int QuantidadeOcorrencias { get; private set; }
        public int QuantidadeBaixadas { get; private set; }
        public int QuantidadeRejeitadas { get; private set; }
        public decimal ValorTotalBaixado { get; private set; }

        protected RetornoBancario() { } // EF Core

        public RetornoBancario(
            string nomeArquivo, ELayoutCnab layout, string hashArquivo, int quantidadeOcorrencias,
            int quantidadeBaixadas, int quantidadeRejeitadas, decimal valorTotalBaixado,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            NomeArquivo = nomeArquivo;
            Layout = layout;
            DataProcessamento = DateTime.UtcNow;
            HashArquivo = hashArquivo;
            QuantidadeOcorrencias = quantidadeOcorrencias;
            QuantidadeBaixadas = quantidadeBaixadas;
            QuantidadeRejeitadas = quantidadeRejeitadas;
            ValorTotalBaixado = valorTotalBaixado;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<RetornoBancario>()
                .Requires()
                .IsNotNullOrEmpty(HashArquivo, nameof(HashArquivo), "O hash do arquivo de retorno é obrigatório.")
            );
        }
    }
}
