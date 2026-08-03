using System;
using System.Linq;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Domain.Services;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// FIN-CGL — motor evento→partida (contabilização automática). Garante a partida dobrada
    /// (débito=crédito; cita Negocio-acumulado/contabil). O mapeamento evento→conta é config do
    /// contador (valida-contador); estes testes verificam o mecanismo de balanceamento.
    /// </summary>
    public class MotorContabilizacaoTests
    {
        private const string TenantId = "tenant-cgl-mot-001";
        private const string UserId = "user-cgl-mot-001";

        [Fact(DisplayName = "Motor | partida simples nasce balanceada (débito=crédito) e em rascunho")]
        public void PartidaSimples_Balanceada_EmRascunho()
        {
            var debito = Guid.NewGuid();
            var credito = Guid.NewGuid();
            var lanc = MotorContabilizacao.GerarPartidaSimples(
                null, "AUTO-1", DateTime.UtcNow, debito, credito, 1000m, "Compra NF 123", TenantId, UserId);

            Assert.Equal(1000m, lanc.TotalDebitos);
            Assert.Equal(1000m, lanc.TotalCreditos);
            Assert.True(lanc.Balanceado);
            Assert.Equal(EEstadoLancamentoContabil.Rascunho, lanc.Estado); // não impacta saldos até confirmar
            Assert.Equal(2, lanc.Linhas.Count);
        }

        [Fact(DisplayName = "Motor | lançamento gerado pode ser confirmado (validação de balanceamento passa)")]
        public void Gerado_PodeSerConfirmado()
        {
            var lanc = MotorContabilizacao.GerarPartidaSimples(
                null, "AUTO-2", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), 250.75m, "Venda faturada", TenantId, UserId);
            lanc.Confirmar(UserId);
            Assert.True(lanc.IsValid);
            Assert.Equal(EEstadoLancamentoContabil.Confirmado, lanc.Estado);
        }

        [Fact(DisplayName = "Motor | regra multi-perna permanece balanceada por construção")]
        public void MultiPerna_Balanceada()
        {
            var regra = new RegraContabilizacao("Folha processada", new[]
            {
                new PernaContabil(Guid.NewGuid(), Guid.NewGuid(), 8000m), // salários
                new PernaContabil(Guid.NewGuid(), Guid.NewGuid(), 1600m), // encargos (INSS patronal etc. — valor factual)
            });
            var lanc = MotorContabilizacao.Gerar(null, "AUTO-3", DateTime.UtcNow, regra, TenantId, UserId);
            Assert.Equal(9600m, lanc.TotalDebitos);
            Assert.Equal(lanc.TotalDebitos, lanc.TotalCreditos);
            Assert.True(lanc.Balanceado);
            Assert.Equal(4, lanc.Linhas.Count);
        }

        [Fact(DisplayName = "Motor | valor não-positivo é rejeitado (não gera lançamento inválido)")]
        public void ValorNaoPositivo_Rejeitado()
        {
            Assert.Throws<ArgumentException>(() => MotorContabilizacao.GerarPartidaSimples(
                null, "AUTO-4", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), 0m, "x", TenantId, UserId));
        }

        [Fact(DisplayName = "Motor | regra sem pernas é rejeitada")]
        public void SemPernas_Rejeitado()
        {
            Assert.Throws<ArgumentException>(() => MotorContabilizacao.Gerar(
                null, "AUTO-5", DateTime.UtcNow, new RegraContabilizacao("x", Array.Empty<PernaContabil>()), TenantId, UserId));
        }
    }
}
