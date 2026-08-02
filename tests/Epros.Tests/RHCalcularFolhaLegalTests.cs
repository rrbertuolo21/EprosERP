using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Application.Handlers;
using Epros.Modules.RH.Domain.Entities;
using Epros.Modules.RH.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    // Integração do MOTOR LEGAL de folha na camada de aplicação (RH-FP).
    public class RHCalcularFolhaLegalTests
    {
        private static ContextRH NovoContext(string db, out Colaborador colaborador, string status = "Ativo", decimal salario = 3000m)
        {
            var options = new DbContextOptionsBuilder<ContextRH>().UseInMemoryDatabase(db).Options;
            var ctx = new ContextRH(options, new TP("tenant-1"), new CU("user-1"));
            colaborador = new Colaborador("Maria", "11122233344", "maria@epros.com.br", "Analista", "TI",
                salario, DateTime.UtcNow.AddDays(-100), "tenant-1", "user-1");
            if (status != "Ativo") colaborador.Desligar(DateTime.UtcNow, "user-1");
            ctx.Colaboradores.Add(colaborador);
            ctx.SaveChanges();
            return ctx;
        }

        [Fact]
        public async Task Apura_Folha_Legal_Com_Sucesso()
        {
            using var ctx = NovoContext(nameof(Apura_Folha_Legal_Com_Sucesso), out var col);
            var handler = new CalcularFolhaLegalCommandHandler(ctx);

            var r = await handler.Handle(new CalcularFolhaLegalCommand(col.Id, 7, 2026), CancellationToken.None);

            Assert.True(r.Sucesso);
            Assert.Contains("motor legal", r.Mensagem);
            Assert.NotNull(r.Dados);
        }

        [Fact]
        public async Task Ano_Sem_Tabela_Falha_Sem_Inventar()
        {
            using var ctx = NovoContext(nameof(Ano_Sem_Tabela_Falha_Sem_Inventar), out var col);
            var handler = new CalcularFolhaLegalCommandHandler(ctx);

            // 2030 não tem tabela confirmada -> regra #0: falha, não inventa.
            var r = await handler.Handle(new CalcularFolhaLegalCommand(col.Id, 7, 2030), CancellationToken.None);

            Assert.False(r.Sucesso);
            Assert.Contains("não inventar", string.Join(" ", r.Erros) + " " + r.Mensagem);
        }

        [Fact]
        public async Task Colaborador_Inexistente_Falha()
        {
            using var ctx = NovoContext(nameof(Colaborador_Inexistente_Falha), out _);
            var handler = new CalcularFolhaLegalCommandHandler(ctx);

            var r = await handler.Handle(new CalcularFolhaLegalCommand(Guid.NewGuid(), 7, 2026), CancellationToken.None);

            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task Colaborador_Desligado_Falha()
        {
            using var ctx = NovoContext(nameof(Colaborador_Desligado_Falha), out var col, status: "Desligado");
            var handler = new CalcularFolhaLegalCommandHandler(ctx);

            var r = await handler.Handle(new CalcularFolhaLegalCommand(col.Id, 7, 2026), CancellationToken.None);

            Assert.False(r.Sucesso);
        }

        private sealed class TP : ITenantProvider
        {
            private readonly string _t;
            public TP(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private sealed class CU : ICurrentUser
        {
            private readonly string _u;
            public CU(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "test_user";
            public string? GetUserEmail() => "test@epros.com.br";
        }
    }
}
