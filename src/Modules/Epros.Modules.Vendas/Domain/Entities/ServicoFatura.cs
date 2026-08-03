using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Fatura comercial de serviços (ven_servico_fatura).
    /// Fonte: EF_7_VENDAS_GESTAO_DE_SERVICOS_V1 §9, §11, §12 e §19.2.
    /// A fatura NÃO movimenta estoque (EF §9.16). O gatilho financeiro nasce aqui, mas o
    /// detalhamento contábil/contas a receber pertence ao Financeiro (EF §12.12) — integração
    /// via referência de lançamento + Outbox, nunca chamando o Context financeiro diretamente.
    /// </summary>
    public class ServicoFatura : EntidadeSaaSBase
    {
        private readonly List<ServicoFaturaLinha> _linhas = new();

        public Guid EmpresaId { get; private set; }
        public Guid ClienteId { get; private set; }
        public Guid? FuncionarioId { get; private set; }
        public Guid? ContaPagamentoId { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public string NumeroFatura { get; private set; } = string.Empty;
        public DateTime DataFatura { get; private set; }
        public EServicoFaturaStatus Status { get; private set; } = EServicoFaturaStatus.Rascunho;

        public decimal DescontoCabecalho { get; private set; }
        public decimal TotalDesconto { get; private set; }
        public decimal ValorImposto { get; private set; }
        public decimal TotalImposto { get; private set; }
        public decimal CustoEnvioEntrega { get; private set; }
        public decimal TotalGeral { get; private set; }
        public decimal TotalLiquido { get; private set; }
        public decimal ValorPago { get; private set; }
        public decimal SaldoDevido { get; private set; }
        public decimal Troco { get; private set; }
        public string? Detalhes { get; private set; }
        public bool Ativo { get; private set; } = true;

        public IReadOnlyCollection<ServicoFaturaLinha> Linhas => _linhas.AsReadOnly();

        protected ServicoFatura() { } // EF Core

        public ServicoFatura(
            Guid empresaId,
            Guid clienteId,
            Guid? funcionarioId,
            Guid? contaPagamentoId,
            Guid? usuarioId,
            string numeroFatura,
            DateTime? dataFatura,
            decimal descontoCabecalho,
            decimal custoEnvioEntrega,
            decimal valorPago,
            string? detalhes,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            ClienteId = clienteId;
            FuncionarioId = funcionarioId;
            ContaPagamentoId = contaPagamentoId;
            UsuarioId = usuarioId;
            // EF §9.7: número gerado automaticamente na criação (data/hora de alta precisão).
            NumeroFatura = string.IsNullOrWhiteSpace(numeroFatura) ? GerarNumero() : numeroFatura;
            DataFatura = dataFatura ?? DateTime.UtcNow; // EF §9.4: data padrão = data corrente.
            DescontoCabecalho = descontoCabecalho < 0 ? 0 : descontoCabecalho;
            CustoEnvioEntrega = custoEnvioEntrega < 0 ? 0 : custoEnvioEntrega;
            ValorPago = valorPago < 0 ? 0 : valorPago;
            Detalhes = detalhes;
            Status = EServicoFaturaStatus.Rascunho;
            Ativo = true;
            Validar();
        }

        public static string GerarNumero() => DateTime.UtcNow.ToString("yyyyMMddHHmmssfffffff");

        public void AdicionarLinha(ServicoFaturaLinha linha)
        {
            linha.DefinirFaturaId(Id);
            _linhas.Add(linha);
        }

        public void LimparLinhas()
        {
            _linhas.Clear();
        }

        public void AlterarCabecalho(
            Guid? funcionarioId,
            Guid? contaPagamentoId,
            decimal descontoCabecalho,
            decimal custoEnvioEntrega,
            decimal valorPago,
            string? detalhes,
            string alteradoPor)
        {
            FuncionarioId = funcionarioId;
            ContaPagamentoId = contaPagamentoId;
            DescontoCabecalho = descontoCabecalho < 0 ? 0 : descontoCabecalho;
            CustoEnvioEntrega = custoEnvioEntrega < 0 ? 0 : custoEnvioEntrega;
            ValorPago = valorPago < 0 ? 0 : valorPago;
            Detalhes = detalhes;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>
        /// EF §11: recalcula totais, imposto (exclusivo/inclusivo), total líquido, saldo devido e troco.
        /// O percentual e o tipo de imposto vêm da configuração da empresa (EF §11.3/§11.4), nunca do
        /// campo taxa do catálogo (EF §8.9/§14 do dicionário).
        /// </summary>
        public void Recalcular(decimal percentualImposto, ETipoImpostoServico tipoImposto)
        {
            if (percentualImposto < 0) percentualImposto = 0;

            // EF §11.1: total geral = soma dos totais das linhas.
            TotalGeral = decimal.Round(_linhas.Where(l => l.Ativo).Sum(l => l.Total), 2);

            // EF §11.2: desconto total = desconto de cabeçalho + descontos das linhas.
            var descontoLinhas = _linhas.Where(l => l.Ativo).Sum(l => l.DescontoValor());
            TotalDesconto = decimal.Round(DescontoCabecalho + descontoLinhas, 2);

            // Base para imposto: total geral menos desconto de cabeçalho (linhas já vêm líquidas).
            var baseImposto = TotalGeral - DescontoCabecalho;
            if (baseImposto < 0) baseImposto = 0;

            if (tipoImposto == ETipoImpostoServico.Exclusivo)
            {
                // EF §11.5/§11.6: imposto por fora; total líquido inclui imposto + envio.
                ValorImposto = decimal.Round(baseImposto * (percentualImposto / 100m), 2);
                TotalLiquido = decimal.Round(baseImposto + ValorImposto + CustoEnvioEntrega, 2);
            }
            else
            {
                // EF §11.7/§11.8: imposto por dentro; total líquido NÃO soma o imposto novamente.
                ValorImposto = decimal.Round(baseImposto * (percentualImposto / (100m + percentualImposto)), 2);
                TotalLiquido = decimal.Round(baseImposto + CustoEnvioEntrega, 2);
            }

            // EF §11.14: saldo devido = total líquido - valor pago.
            var saldo = TotalLiquido - ValorPago;
            SaldoDevido = saldo < 0 ? 0 : decimal.Round(saldo, 2);

            // EF §11.15: troco quando valor pago > total líquido.
            var troco = ValorPago - TotalLiquido;
            Troco = troco > 0 ? decimal.Round(troco, 2) : 0;
        }

        /// <summary>EF §7.2: Rascunho -> Confirmado. Exige cliente, data e ao menos uma linha válida (§21).</summary>
        public void Confirmar(string alteradoPor)
        {
            if (Status != EServicoFaturaStatus.Rascunho)
            {
                AddNotification(nameof(Status), "Somente faturas em rascunho podem ser confirmadas. [Origem: ServicoFatura]");
                return;
            }
            if (!_linhas.Any(l => l.Ativo))
            {
                AddNotification(nameof(Linhas), "A fatura deve possuir ao menos uma linha válida para ser confirmada. [Origem: ServicoFatura]");
                return;
            }
            Status = EServicoFaturaStatus.Confirmado;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>EF §7.2: Confirmado -> Faturado. Gera efeitos financeiros (EF §12).</summary>
        public void Faturar(string alteradoPor)
        {
            if (Status != EServicoFaturaStatus.Confirmado)
            {
                AddNotification(nameof(Status), "Somente faturas confirmadas podem ser faturadas. [Origem: ServicoFatura]");
                return;
            }
            Status = EServicoFaturaStatus.Faturado;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>EF §7.2: cancelamento; faturas faturadas devem estornar lançamentos vinculados (EF §13.5).</summary>
        /// <summary>
        /// EF §7.2 (EC-08): Cancelar é transição de fatura NÃO faturada (Rascunho/Confirmado → Cancelado).
        /// Fatura já faturada não se cancela — usa-se <see cref="Estornar"/> (reverte efeitos financeiros).
        /// </summary>
        public void Cancelar(string alteradoPor)
        {
            Clear(); // transição independente: reavalia notificações do zero.
            if (Status == EServicoFaturaStatus.Cancelado || Status == EServicoFaturaStatus.Estornado)
            {
                AddNotification(nameof(Status), "A fatura já está cancelada ou estornada. [Origem: ServicoFatura]");
                return;
            }
            if (Status == EServicoFaturaStatus.Faturado)
            {
                AddNotification(nameof(Status), "Fatura já faturada não pode ser cancelada — utilize Estornar. [Origem: ServicoFatura EC-08]");
                return;
            }
            Status = EServicoFaturaStatus.Cancelado;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>
        /// EF §7.2 (EC-08): Estornar é transição de fatura FATURADA (Faturado → Estornado), revertendo os
        /// lançamentos financeiros (feito no handler via referências de integração — T-06).
        /// </summary>
        public void Estornar(string alteradoPor)
        {
            Clear(); // transição independente: reavalia notificações do zero.
            if (Status == EServicoFaturaStatus.Cancelado || Status == EServicoFaturaStatus.Estornado)
            {
                AddNotification(nameof(Status), "A fatura já está cancelada ou estornada. [Origem: ServicoFatura]");
                return;
            }
            if (Status != EServicoFaturaStatus.Faturado)
            {
                AddNotification(nameof(Status), "Somente fatura faturada pode ser estornada — use Cancelar. [Origem: ServicoFatura EC-08]");
                return;
            }
            Status = EServicoFaturaStatus.Estornado;
            MarcarAlterado(alteradoPor);
        }

        public void Reativar(string alteradoPor)
        {
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ServicoFatura>()
                .Requires()
                .AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "A empresa da fatura é obrigatória. [Origem: ServicoFatura]")
                .AreNotEquals(ClienteId, Guid.Empty, nameof(ClienteId), "A fatura de serviço deve possuir cliente. [Origem: ServicoFatura]")
                .IsNotNullOrEmpty(NumeroFatura, nameof(NumeroFatura), "O número da fatura é obrigatório. [Origem: ServicoFatura]"));

            // EF §9.6: conta de pagamento obrigatória quando houver valor pago.
            if (ValorPago > 0 && (ContaPagamentoId == null || ContaPagamentoId == Guid.Empty))
                AddNotification(nameof(ContaPagamentoId), "A conta de pagamento é obrigatória quando há valor pago. [Origem: ServicoFatura]");

            if (!string.IsNullOrEmpty(Detalhes) && Detalhes.Length > 4000)
                AddNotification(nameof(Detalhes), "Os detalhes da fatura devem ter no máximo 4000 caracteres. [Origem: ServicoFatura]");
        }
    }
}
