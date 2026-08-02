using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>
    /// 1.08I — Lançamento de RECONHECIMENTO DE RECEITA por competência (1 linha do cronograma de
    /// diferimento). Enquanto <see cref="ReconhecimentoReceitaStatus.Pendente"/>, o valor é RECEITA
    /// DIFERIDA (passivo de contrato — receita já recebida mas ainda NÃO ganha). A rotina mensal o move
    /// para <see cref="ReconhecimentoReceitaStatus.Reconhecido"/> na competência devida (apropriação 1/N).
    ///
    /// MECANISMO universal do CPC 47 / IFRS 15 (skill Negocio-acumulado/contabil):
    ///   • Plano ANUAL à vista → 12 avos, 1/12 por mês (RN05, CPC 47 itens 35(a) e 106).
    ///   • Plano MENSAL → reconhece integral no mês (RN04), 1 única parcela.
    ///   • Plano VITALÍCIO → RN07 é [PARÂMETRO] (pró-rata por período estimado × ponto único): o
    ///     mecanismo usa 1 parcela (ponto único) por default — ⚠️ VALIDA CONTADOR.
    ///
    /// ⚠️ As CONTAS CONTÁBEIS (receita diferida × receita de assinatura), a POLÍTICA de pró-rata e a
    /// escolha do vitalício são PARÂMETRO do cliente/contador — este entity só carrega o MECANISMO
    /// (competência, valor, status) citando a norma; NÃO fixa conta nem regime fiscal.
    /// </summary>
    public class ReconhecimentoReceita : EntidadeSaaSBase
    {
        /// <summary>Fatura que originou o cronograma (o faturamento único, ex.: R$1.200 anual à vista).</summary>
        public Guid FaturaId { get; private set; }
        public Guid ClienteId { get; private set; }

        /// <summary>Mês de competência (sempre normalizado para o 1º dia do mês, UTC).</summary>
        public DateTime Competencia { get; private set; }

        /// <summary>Ordem da parcela no cronograma (1..<see cref="TotalParcelas"/>).</summary>
        public int Sequencia { get; private set; }

        /// <summary>Total de parcelas do cronograma (12 no anual, 1 no mensal/vitalício-ponto-único).</summary>
        public int TotalParcelas { get; private set; }

        /// <summary>Valor a apropriar nesta competência (1/N do valor da fatura; resíduo de centavos na última).</summary>
        public decimal Valor { get; private set; }

        public ReconhecimentoReceitaStatus Status { get; private set; } = ReconhecimentoReceitaStatus.Pendente;

        /// <summary>Momento em que a parcela foi apropriada como receita (null enquanto diferida).</summary>
        public DateTime? ReconhecidoEm { get; private set; }

        protected ReconhecimentoReceita() { } // EF Core

        public ReconhecimentoReceita(
            Guid faturaId,
            Guid clienteId,
            DateTime competencia,
            int sequencia,
            int totalParcelas,
            decimal valor,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ReconhecimentoReceita>()
                .Requires()
                .AreNotEquals(faturaId, Guid.Empty, nameof(FaturaId), "FaturaId é obrigatório")
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "ClienteId é obrigatório")
                .IsGreaterThan(sequencia, 0, nameof(Sequencia), "Sequência deve ser maior que zero")
                .IsGreaterThan(totalParcelas, 0, nameof(TotalParcelas), "TotalParcelas deve ser maior que zero")
                .IsGreaterThan(valor, -0.01m, nameof(Valor), "Valor da competência deve ser maior ou igual a zero")
            );

            FaturaId = faturaId;
            ClienteId = clienteId;
            Competencia = NormalizarCompetencia(competencia);
            Sequencia = sequencia;
            TotalParcelas = totalParcelas;
            Valor = valor;
            Status = ReconhecimentoReceitaStatus.Pendente;
        }

        /// <summary>Normaliza uma data para o 1º dia do seu mês (competência), em UTC.</summary>
        public static DateTime NormalizarCompetencia(DateTime data)
            => new DateTime(data.Year, data.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Aproria a parcela (move de receita diferida → receita reconhecida). Idempotente: se já estava
        /// reconhecida, é no-op e retorna false. [NORMA: CPC 47 item 35(a) — satisfação ao longo do tempo]
        /// </summary>
        public bool Reconhecer(DateTime dataReferencia, string alteradoPor)
        {
            if (Status == ReconhecimentoReceitaStatus.Reconhecido)
                return false;

            Status = ReconhecimentoReceitaStatus.Reconhecido;
            ReconhecidoEm = dataReferencia;
            MarcarAlterado(alteradoPor);
            return true;
        }

        /// <summary>
        /// 1.08I — Gancho de ESTORNO (cancelamento/reembolso antes do fim do ciclo): devolve a parcela
        /// ao estado diferido. [NORMA: CPC 47 — RN08 estorna o saldo remanescente do passivo de receita
        /// diferida] · ⚠️ política de reembolso/estorno = PARÂMETRO, VALIDA CONTADOR.
        /// </summary>
        public bool Estornar(string alteradoPor)
        {
            if (Status == ReconhecimentoReceitaStatus.Pendente)
                return false;

            Status = ReconhecimentoReceitaStatus.Pendente;
            ReconhecidoEm = null;
            MarcarAlterado(alteradoPor);
            return true;
        }
    }
}
