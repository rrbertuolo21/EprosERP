using System;
using System.Linq;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Domain.Enums;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes de domínio FIN-CGL (Contabilidade Geral — contabilidade plena).
    /// Cobrem: partida dobrada (CGL-VAL-015/016), estados de lançamento e período, conta contábil.
    /// </summary>
    public class ContabilidadeGeralTests
    {
        private const string TenantId = "tenant-cgl-001";
        private const string UserId = "user-cgl-001";

        private static ContaContabil NovaConta(string codigo = "1.01.001")
            => new ContaContabil(codigo, "Caixa", null, 3, ETipoContaContabil.Ativo, true, true, false, false, TenantId, UserId);

        [Fact]
        public void ContaContabil_Valida_QuandoCamposObrigatoriosPreenchidos()
        {
            var conta = NovaConta();
            Assert.True(conta.IsValid);
            Assert.True(conta.Ativo);
        }

        [Fact]
        public void ContaContabil_Invalida_QuandoNomeVazio()
        {
            var conta = new ContaContabil("1.01", "", null, 2, ETipoContaContabil.Ativo, false, true, false, false, TenantId, UserId);
            Assert.False(conta.IsValid);
        }

        [Fact]
        public void Lancamento_Confirmar_Falha_QuandoDebitosDiferentesDeCreditos()
        {
            var lanc = new LancamentoContabil(null, "L-001", DateTime.UtcNow, "Teste", TenantId, UserId);
            lanc.AdicionarLinha(Guid.NewGuid(), 100m, 0m, null, UserId);
            lanc.AdicionarLinha(Guid.NewGuid(), 0m, 80m, null, UserId);
            lanc.Confirmar(UserId);
            Assert.False(lanc.IsValid);
            Assert.Equal(EEstadoLancamentoContabil.Rascunho, lanc.Estado);
            Assert.Contains(lanc.Notifications, n => n.Message.Contains("débitos deve ser igual"));
        }

        [Fact]
        public void Lancamento_Confirmar_Falha_QuandoTotaisZerados()
        {
            var lanc = new LancamentoContabil(null, "L-002", DateTime.UtcNow, "Teste", TenantId, UserId);
            lanc.AdicionarLinha(Guid.NewGuid(), 0m, 0m, null, UserId);
            lanc.Confirmar(UserId);
            Assert.False(lanc.IsValid);
            Assert.Contains(lanc.Notifications, n => n.Message.Contains("maior que zero"));
        }

        [Fact]
        public void Lancamento_Confirmar_Sucesso_QuandoBalanceado()
        {
            var lanc = new LancamentoContabil(null, "L-003", DateTime.UtcNow, "Teste", TenantId, UserId);
            lanc.AdicionarLinha(Guid.NewGuid(), 150m, 0m, null, UserId);
            lanc.AdicionarLinha(Guid.NewGuid(), 0m, 150m, null, UserId);
            lanc.Confirmar(UserId);
            Assert.True(lanc.IsValid);
            Assert.True(lanc.Balanceado);
            Assert.Equal(EEstadoLancamentoContabil.Confirmado, lanc.Estado);
        }

        [Fact]
        public void Lancamento_Estornar_SomenteQuandoConfirmado()
        {
            var lanc = new LancamentoContabil(null, "L-004", DateTime.UtcNow, "Teste", TenantId, UserId);
            lanc.Estornar(UserId); // ainda rascunho
            Assert.False(lanc.IsValid);

            var lanc2 = new LancamentoContabil(null, "L-005", DateTime.UtcNow, "Teste", TenantId, UserId);
            lanc2.AdicionarLinha(Guid.NewGuid(), 10m, 0m, null, UserId);
            lanc2.AdicionarLinha(Guid.NewGuid(), 0m, 10m, null, UserId);
            lanc2.Confirmar(UserId);
            lanc2.Estornar(UserId);
            Assert.True(lanc2.IsValid);
            Assert.Equal(EEstadoLancamentoContabil.Estornado, lanc2.Estado);
        }

        [Fact]
        public void Periodo_Fechar_BloqueiaLancamentos_E_ReabreComMotivo()
        {
            var p = new PeriodoContabil(2026, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), TenantId, UserId);
            p.Fechar(null, DateTime.UtcNow, UserId);
            Assert.Equal(EEstadoPeriodoContabil.Fechado, p.Estado);
            Assert.True(p.BloqueiaLancamento);

            p.Reabrir(null, "", UserId); // sem motivo
            Assert.False(p.IsValid);

            var p2 = new PeriodoContabil(2026, null, null, TenantId, UserId);
            p2.Fechar(null, DateTime.UtcNow, UserId);
            p2.Reabrir(Guid.NewGuid(), "auditoria fiscal", UserId);
            Assert.True(p2.IsValid);
            Assert.Equal(EEstadoPeriodoContabil.Reaberto, p2.Estado);
        }

        [Fact]
        public void SaldoAbertura_Valido_ComContaEHistorico()
        {
            var saldo = new SaldoAbertura(null, DateTime.UtcNow, Guid.NewGuid(), "1.01.001", ETipoSaldoContabil.Debito, 500m, "Saldo inicial", TenantId, UserId);
            Assert.True(saldo.IsValid);
            Assert.True(saldo.SaldoInicial);
        }
    }
}
