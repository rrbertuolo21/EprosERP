using System;
using Epros.Modules.Imobiliaria.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Cobranca recorrente do aluguel por competencia (ID8/NF-01). A IMOBILIARIA orquestra e
    /// guarda o RESUMO; o TITULO e a BAIXA vivem no CONTAS_RECEBER (FINANCEIRO), refletidos por
    /// evento idempotente (chave locacao+competencia+tipo — T2). Juros/multa/desconto/estorno e
    /// a maquina do recebivel sao governados pelo FINANCEIRO — NAO recalculados aqui.
    /// </summary>
    public class CobrancaAluguel : EntidadeSaaSBase
    {
        public Guid LocacaoId { get; private set; }
        /// <summary>Primeiro dia do mes de competencia (normalizado).</summary>
        public DateTime Competencia { get; private set; }
        public ETipoCobrancaAluguel Tipo { get; private set; }
        public decimal Valor { get; private set; }
        public int Vencimento { get; private set; }
        public EStatusCobrancaAluguel Status { get; private set; }
        /// <summary>Valor efetivamente baixado (refletido do FINANCEIRO). Nao governa juros/multa.</summary>
        public decimal ValorPago { get; private set; }
        /// <summary>Referencia do titulo/recebivel no CONTAS_RECEBER (preenchida na reflexao).</summary>
        public string? ReceberRef { get; private set; }
        public DateTime? DataBaixa { get; private set; }

        protected CobrancaAluguel() { } // EF Core

        public CobrancaAluguel(
            Guid locacaoId,
            DateTime competencia,
            ETipoCobrancaAluguel tipo,
            decimal valor,
            int vencimento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            LocacaoId = locacaoId;
            Competencia = new DateTime(competencia.Year, competencia.Month, 1);
            Tipo = tipo;
            Valor = valor;
            Vencimento = vencimento;
            Status = EStatusCobrancaAluguel.EmAberto;
            ValorPago = 0m;
            Validar();
        }

        /// <summary>
        /// Reflete a baixa vinda do FINANCEIRO (ID8/NF-01). Parcial se o pago &lt; valor devido.
        /// O calculo de encargos e do CONTAS_RECEBER; aqui apenas espelhamos o resultado.
        /// </summary>
        public void RefletirBaixa(decimal valorPago, string? receberRef, DateTime dataBaixa, string usuario)
        {
            if (Status == EStatusCobrancaAluguel.Estornado)
            {
                AddNotification(nameof(Status), "Cobranca estornada nao pode receber baixa.");
                return;
            }
            if (valorPago <= 0)
            {
                AddNotification(nameof(ValorPago), "O valor da baixa deve ser positivo.");
                return;
            }
            ValorPago += valorPago;
            ReceberRef = receberRef ?? ReceberRef;
            DataBaixa = dataBaixa;
            Status = ValorPago >= Valor ? EStatusCobrancaAluguel.Pago : EStatusCobrancaAluguel.Parcial;
            MarcarAlterado(usuario);
        }

        /// <summary>Reflete o estorno da baixa (ID8/NF-01). Volta a EmAberto zerando o pago.</summary>
        public void RefletirEstorno(string usuario)
        {
            if (Status == EStatusCobrancaAluguel.EmAberto)
            {
                AddNotification(nameof(Status), "Cobranca em aberto nao possui baixa para estornar.");
                return;
            }
            ValorPago = 0m;
            DataBaixa = null;
            Status = EStatusCobrancaAluguel.Estornado;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<CobrancaAluguel>()
                .Requires()
                .AreNotEquals(LocacaoId, Guid.Empty, nameof(LocacaoId),
                    "A cobranca exige locacao. [Origem: CobrancaAluguel] (NF-01)")
                .IsGreaterThan(Valor, 0, nameof(Valor),
                    "O valor da cobranca deve ser positivo. [Origem: CobrancaAluguel] (NF-01)")
                .IsBetween(Vencimento, 1, 31, nameof(Vencimento),
                    "O vencimento deve estar entre 1 e 31. [Origem: CobrancaAluguel] (NF-03)"));
        }
    }
}
