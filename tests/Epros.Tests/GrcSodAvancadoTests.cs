using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Application.Handlers;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// GRC-SOD avançado — matriz de conflitos, bloqueio preventivo (D-SOD-03), workflow de exceção
    /// com autoaprovação proibida (D-SOD-02), expiração que reativa violação (RN-SOD-039) e
    /// bypass do admin nunca silencioso (D-SOD-04).
    /// </summary>
    public class GrcSodAvancadoTests
    {
        private static ContextGRC NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextGRC>().UseInMemoryDatabase(db).Options;
            return new ContextGRC(options, new FakeTenant("tenant-1"), new FakeUser("user-1"));
        }

        private static async Task<(Guid a, Guid b)> SemearRegraAtivaAsync(ContextGRC context, string modo)
        {
            var fa = new FuncaoSoD("F-A", "Aprovar Pagamento", null, "tenant-1", "user-1");
            var fb = new FuncaoSoD("F-B", "Executar Pagamento", null, "tenant-1", "user-1");
            context.FuncoesSoD.AddRange(fa, fb);
            var regra = new RegraSoD(null, fa.Id, fb.Id, "Critica", DateTime.UtcNow.AddDays(-1), null, "tenant-1", "user-1");
            regra.Submeter("user-1");
            regra.Ativar("user-1");
            if (modo == "PermiteExcecao") regra.DefinirModoTratamento("PermiteExcecao", "user-1");
            context.RegrasSoD.Add(regra);
            await context.SaveChangesAsync();
            return (fa.Id, fb.Id);
        }

        [Fact]
        public async Task Concessao_Deve_Ser_Bloqueada_Quando_Regra_Bloqueia()
        {
            using var context = NovoContexto(nameof(Concessao_Deve_Ser_Bloqueada_Quando_Regra_Bloqueia));
            var (a, b) = await SemearRegraAtivaAsync(context, "Bloqueia");
            var handler = new AvaliarConcessaoSoDCommandHandler(context, new FakeTenant("tenant-1"));

            var result = await handler.Handle(
                new AvaliarConcessaoSoDCommand(Guid.NewGuid(), null, "Perfil",
                    new List<Guid> { a }, new List<Guid> { b }),
                CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.True(result.Block);
        }

        [Fact]
        public async Task Concessao_Deve_Ser_Permitida_Com_Excecao_Quando_Regra_PermiteExcecao()
        {
            using var context = NovoContexto(nameof(Concessao_Deve_Ser_Permitida_Com_Excecao_Quando_Regra_PermiteExcecao));
            var (a, b) = await SemearRegraAtivaAsync(context, "PermiteExcecao");
            var handler = new AvaliarConcessaoSoDCommandHandler(context, new FakeTenant("tenant-1"));

            var result = await handler.Handle(
                new AvaliarConcessaoSoDCommand(Guid.NewGuid(), null, "Perfil",
                    new List<Guid> { a }, new List<Guid> { b }),
                CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.False(result.Block);
        }

        [Fact]
        public async Task Concessao_Sem_Conflito_Deve_Ser_Permitida()
        {
            using var context = NovoContexto(nameof(Concessao_Sem_Conflito_Deve_Ser_Permitida));
            var (a, _) = await SemearRegraAtivaAsync(context, "Bloqueia");
            var handler = new AvaliarConcessaoSoDCommandHandler(context, new FakeTenant("tenant-1"));

            var result = await handler.Handle(
                new AvaliarConcessaoSoDCommand(Guid.NewGuid(), null, "Perfil",
                    new List<Guid>(), new List<Guid> { a }),
                CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.False(result.Block);
        }

        [Fact]
        public async Task Aprovacao_De_Excecao_Deve_Proibir_Autoaprovacao()
        {
            using var context = NovoContexto(nameof(Aprovacao_De_Excecao_Deve_Proibir_Autoaprovacao));
            var (a, b) = await SemearRegraAtivaAsync(context, "PermiteExcecao");
            var regra = await context.RegrasSoD.FirstAsync();
            var violacao = new ViolacaoSoD(null, regra.Id, Guid.NewGuid(), null, "tenant-1", "user-1");
            context.ViolacoesSoD.Add(violacao);
            await context.SaveChangesAsync();

            var solicitante = Guid.NewGuid();
            var solicitar = new SolicitarExcecaoSoDCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            var sol = await solicitar.Handle(new SolicitarExcecaoSoDCommand(
                violacao.Id, solicitante, "Equipe pequena", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), null, "Dupla aprovacao"),
                CancellationToken.None);
            Assert.True(sol.Sucesso);

            var excecao = await context.ExcecoesSoD.FirstAsync();
            var aprovar = new AprovarExcecaoSoDCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"), new FakeAuditoria());
            var result = await aprovar.Handle(new AprovarExcecaoSoDCommand(excecao.Id, solicitante), CancellationToken.None);

            Assert.False(result.Sucesso); // autoaprovação proibida
        }

        [Fact]
        public async Task Excecao_Sem_Controle_Compensatorio_Deve_Falhar()
        {
            using var context = NovoContexto(nameof(Excecao_Sem_Controle_Compensatorio_Deve_Falhar));
            var (a, b) = await SemearRegraAtivaAsync(context, "PermiteExcecao");
            var regra = await context.RegrasSoD.FirstAsync();
            var violacao = new ViolacaoSoD(null, regra.Id, Guid.NewGuid(), null, "tenant-1", "user-1");
            context.ViolacoesSoD.Add(violacao);
            await context.SaveChangesAsync();

            var solicitar = new SolicitarExcecaoSoDCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            var result = await solicitar.Handle(new SolicitarExcecaoSoDCommand(
                violacao.Id, Guid.NewGuid(), "Sem mitigacao", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), null, null),
                CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact]
        public async Task Aprovacao_Valida_Deve_Mitigar_Violacao_E_Expiracao_Deve_Reativar()
        {
            using var context = NovoContexto(nameof(Aprovacao_Valida_Deve_Mitigar_Violacao_E_Expiracao_Deve_Reativar));
            var (a, b) = await SemearRegraAtivaAsync(context, "PermiteExcecao");
            var regra = await context.RegrasSoD.FirstAsync();
            var violacao = new ViolacaoSoD(null, regra.Id, Guid.NewGuid(), null, "tenant-1", "user-1");
            context.ViolacoesSoD.Add(violacao);
            await context.SaveChangesAsync();

            // Exceção já vencida (DataFim no passado) para exercitar a expiração.
            var excecao = new ExcecaoSoD(violacao.Id, "Equipe pequena", Guid.NewGuid(),
                DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-1), null, "Dupla aprovacao", "tenant-1", "user-1");
            context.ExcecoesSoD.Add(excecao);
            await context.SaveChangesAsync();

            var aprovar = new AprovarExcecaoSoDCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"), new FakeAuditoria());
            var ap = await aprovar.Handle(new AprovarExcecaoSoDCommand(excecao.Id, Guid.NewGuid()), CancellationToken.None);
            Assert.True(ap.Sucesso);
            Assert.Equal("Mitigada", (await context.ViolacoesSoD.FirstAsync()).Status);

            var expirar = new ExpirarExcecoesVencidasSoDCommandHandler(context, new FakeUser("user-1"));
            var exp = await expirar.Handle(new ExpirarExcecoesVencidasSoDCommand(), CancellationToken.None);
            Assert.True(exp.Sucesso);
            Assert.Equal("Ativa", (await context.ViolacoesSoD.FirstAsync()).Status); // RN-SOD-039
        }

        [Fact]
        public async Task Bypass_Admin_Sem_Controle_Compensatorio_Deve_Falhar_E_Com_Controle_Deve_Registrar()
        {
            using var context = NovoContexto(nameof(Bypass_Admin_Sem_Controle_Compensatorio_Deve_Falhar_E_Com_Controle_Deve_Registrar));
            var handler = new RegistrarBypassAdminSoDCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("admin"), new FakeAuditoria());

            var semControle = await handler.Handle(
                new RegistrarBypassAdminSoDCommand(Guid.NewGuid(), Guid.NewGuid(), true, "Emergencia", null, null),
                CancellationToken.None);
            Assert.False(semControle.Sucesso);

            var comControle = await handler.Handle(
                new RegistrarBypassAdminSoDCommand(Guid.NewGuid(), Guid.NewGuid(), true, "Emergencia", null, "Revisao por terceiro + trilha reforcada"),
                CancellationToken.None);
            Assert.True(comControle.Sucesso);
            Assert.Single(await context.BypassesSoD.ToListAsync());
        }

        private class FakeTenant : ITenantProvider
        {
            private readonly string _t;
            public FakeTenant(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class FakeUser : ICurrentUser
        {
            private readonly string _u;
            public FakeUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "test";
            public string? GetUserEmail() => "test@epros.com.br";
        }

        private class FakeAuditoria : IRegistroAuditoriaService
        {
            public Task RegistrarAsync(string entidade, string entidadeId, string acao,
                string? valoresAntes = null, string? valoresDepois = null, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }
    }
}
