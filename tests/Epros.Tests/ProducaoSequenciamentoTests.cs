using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Producao.Domain.Services;
using Xunit;
using Op = Epros.Modules.Producao.Domain.Services.SequenciamentoService.OperacaoEntrada;

namespace Epros.Tests
{
    /// <summary>
    /// PRD-ESC — Testes do motor de sequenciamento/APS com capacidade finita (PD5).
    /// </summary>
    public class ProducaoSequenciamentoTests
    {
        private static readonly Guid[] NoDep = Array.Empty<Guid>();

        [Fact(DisplayName = "ESC-ENG | Precedência força ordem mesmo contra prioridade")]
        public void Precedencia_ForcaOrdem()
        {
            var centro = Guid.NewGuid();
            var a = Guid.NewGuid();
            var b = Guid.NewGuid();
            var svc = new SequenciamentoService();
            var inicio = new DateTime(2026, 8, 2, 8, 0, 0);

            // B tem prioridade maior (1) mas depende de A (prioridade 2)
            var ops = new[]
            {
                new Op(a, centro, 2, 30m, 0m, NoDep),
                new Op(b, centro, 1, 30m, 0m, new[] { a })
            };

            var agenda = svc.Sequenciar(ops, inicio);
            Assert.Equal(a, agenda[0].Id);
            Assert.Equal(b, agenda[1].Id);
            // capacidade finita no mesmo centro: B começa quando A termina
            Assert.Equal(agenda[0].Fim, agenda[1].Inicio);
            Assert.Equal(inicio.AddMinutes(30), agenda[0].Fim);
        }

        [Fact(DisplayName = "ESC-ENG | Centros distintos processam em paralelo")]
        public void CentrosDistintos_Paralelo()
        {
            var svc = new SequenciamentoService();
            var inicio = new DateTime(2026, 8, 2, 8, 0, 0);
            var op1 = new Op(Guid.NewGuid(), Guid.NewGuid(), 1, 60m, 0m, NoDep);
            var op2 = new Op(Guid.NewGuid(), Guid.NewGuid(), 1, 60m, 0m, NoDep);

            var agenda = svc.Sequenciar(new[] { op1, op2 }, inicio);
            Assert.All(agenda, a => Assert.Equal(inicio, a.Inicio)); // ambos iniciam no horizonte
        }

        [Fact(DisplayName = "ESC-ENG | Mesmo centro serializa por prioridade e soma setup")]
        public void MesmoCentro_SerializaComSetup()
        {
            var centro = Guid.NewGuid();
            var svc = new SequenciamentoService();
            var inicio = new DateTime(2026, 8, 2, 8, 0, 0);

            var alta = new Op(Guid.NewGuid(), centro, 1, 20m, 5m, NoDep);
            var baixa = new Op(Guid.NewGuid(), centro, 2, 20m, 5m, NoDep);

            var agenda = svc.Sequenciar(new[] { baixa, alta }, inicio);
            Assert.Equal(alta.Id, agenda[0].Id);                       // prioridade vence
            Assert.Equal(inicio.AddMinutes(25), agenda[0].Fim);        // setup 5 + duração 20
            Assert.Equal(agenda[0].Fim, agenda[1].Inicio);             // serializado no centro
            Assert.Equal(inicio.AddMinutes(50), agenda[1].Fim);
        }

        [Fact(DisplayName = "ESC-ENG | Ciclo de precedência é rejeitado (DP-ESC-009)")]
        public void CicloPrecedencia_Rejeita()
        {
            var centro = Guid.NewGuid();
            var a = Guid.NewGuid();
            var b = Guid.NewGuid();
            var svc = new SequenciamentoService();

            var ops = new[]
            {
                new Op(a, centro, 1, 10m, 0m, new[] { b }),
                new Op(b, centro, 1, 10m, 0m, new[] { a })
            };

            Assert.Throws<InvalidOperationException>(() => svc.Sequenciar(ops, DateTime.UtcNow));
        }
    }
}
