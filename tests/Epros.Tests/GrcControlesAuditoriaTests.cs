using System;
using System.Linq;
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
    /// GRC-CIA — amostragem com justificativa (D-CIA-02), token de acesso externo com expiração
    /// obrigatória (D-CIA-04) e ciclo de achado com evidência/aprovação (RN-CIA-013/014).
    /// </summary>
    public class GrcControlesAuditoriaTests
    {
        private static ContextGRC NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextGRC>().UseInMemoryDatabase(db).Options;
            return new ContextGRC(options, new FakeTenant("tenant-1"), new FakeUser("user-1"));
        }

        [Fact]
        public async Task Amostra_Sem_Justificativa_Deve_Falhar()
        {
            using var context = NovoContexto(nameof(Amostra_Sem_Justificativa_Deve_Falhar));
            var handler = new DefinirAmostraAuditoriaCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            var result = await handler.Handle(new DefinirAmostraAuditoriaCommand(Guid.NewGuid(), null, "Estatistico", 30, "aleatorio", ""), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        [Fact]
        public async Task Token_Acesso_Externo_Deve_Ter_Expiracao_E_Retornar_Token_Uma_Vez()
        {
            using var context = NovoContexto(nameof(Token_Acesso_Externo_Deve_Ter_Expiracao_E_Retornar_Token_Uma_Vez));
            var plano = new PlanoAuditoria("PA-1", "Plano", "Desc", "Anual", "tenant-1", "user-1");
            context.PlanosAuditoria.Add(plano);
            await context.SaveChangesAsync();

            var handler = new EmitirTokenAcessoAuditoriaCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            var result = await handler.Handle(new EmitirTokenAcessoAuditoriaCommand(plano.Id, Guid.NewGuid(), "Leitura", null), CancellationToken.None);

            Assert.True(result.Sucesso);
            var token = await context.TokensAcessoAuditoria.FirstAsync();
            Assert.True(token.ExpiraEm > DateTime.UtcNow);
            // O hash é persistido; o valor em claro não é o mesmo que o hash.
            Assert.True(token.TokenHash.Length >= 32);
        }

        [Fact]
        public async Task Achado_Critico_Nao_Encerra_Sem_Aprovacao_E_Encerra_Com_Evidencia_E_Aprovacao()
        {
            // Cada operação usa um context FRESCO (mesmo store in-memory), espelhando produção:
            // em produção cada request tem DbContext/entidade novos, sem acúmulo de notificações Flunt.
            var db = nameof(Achado_Critico_Nao_Encerra_Sem_Aprovacao_E_Encerra_Com_Evidencia_E_Aprovacao);
            Guid achadoId;
            using (var context = NovoContexto(db))
            {
                var achado = new Achado(null, "Falha critica", "Desc", "Critica", DateTime.UtcNow.AddDays(10), "tenant-1", "user-1");
                achado.IniciarRemediacao("user-1");
                context.Achados.Add(achado);
                await context.SaveChangesAsync();
                achadoId = achado.Id;
            }

            // Sem evidência e sem aprovação -> bloqueia.
            using (var context = NovoContexto(db))
            {
                var encerrar = new EncerrarAchadoCommandHandler(context, new FakeUser("user-1"));
                var semNada = await encerrar.Handle(new EncerrarAchadoCommand(achadoId, "resolvido"), CancellationToken.None);
                Assert.False(semNada.Sucesso);
            }

            // Registra evidência.
            using (var context = NovoContexto(db))
            {
                var evid = new RegistrarEvidenciaAuditoriaCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
                await evid.Handle(new RegistrarEvidenciaAuditoriaCommand(achadoId, null, Guid.NewGuid(), "print"), CancellationToken.None);
            }

            // Com evidência mas sem aprovação (crítico) -> ainda bloqueia (RN-CIA-013).
            using (var context = NovoContexto(db))
            {
                var encerrar = new EncerrarAchadoCommandHandler(context, new FakeUser("user-1"));
                var semAprov = await encerrar.Handle(new EncerrarAchadoCommand(achadoId, "resolvido"), CancellationToken.None);
                Assert.False(semAprov.Sucesso);
            }

            // Aprova.
            using (var context = NovoContexto(db))
            {
                var aprovar = new AprovarAchadoCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
                await aprovar.Handle(new AprovarAchadoCommand(achadoId, Guid.NewGuid()), CancellationToken.None);
            }

            // Encerra (com evidência + aprovação).
            using (var context = NovoContexto(db))
            {
                var encerrar = new EncerrarAchadoCommandHandler(context, new FakeUser("user-1"));
                var ok = await encerrar.Handle(new EncerrarAchadoCommand(achadoId, "resolvido"), CancellationToken.None);
                Assert.True(ok.Sucesso);
            }

            using (var context = NovoContexto(db))
                Assert.Equal("Encerrado", (await context.Achados.FirstAsync()).Status);
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
    }
}
