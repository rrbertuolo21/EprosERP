using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;

namespace Epros.Tests
{
    /// <summary>
    /// D-SOD-03 — o bloqueio de Segregação de Funções é EFETIVO em runtime no caminho de concessão RBAC.
    /// Antes, o handler de bloqueio existia mas nenhum caller o invocava (P0). Aqui a concessão de papel
    /// consulta a porta SoD (ISoDAvaliadorConcessao) e nega quando há regra bloqueante.
    /// </summary>
    public class SoDConcessaoRbacTests
    {
        private const string TenantId = "tenant-sod-rbac";

        private static ContextGestaoClientes NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>().UseInMemoryDatabase(db).Options;
            return new ContextGestaoClientes(options, new FakeTenant(TenantId), new FakeUser("admin"));
        }

        private static async Task<Guid> SemearPapelAsync(ContextGestaoClientes ctx, string nome)
        {
            var papel = new Papel(nome, nome, null, true, false, null, null, null, TenantId, "admin");
            ctx.Papeis.Add(papel);
            await ctx.SaveChangesAsync();
            return papel.Id;
        }

        [Fact(DisplayName = "SoD/RBAC | Concessão de papel é BLOQUEADA quando o avaliador retorna bloqueado (D-SOD-03)")]
        public async Task Concessao_Bloqueada_Por_SoD()
        {
            using var ctx = NovoContexto(nameof(Concessao_Bloqueada_Por_SoD));
            var papelId = await SemearPapelAsync(ctx, "Executar Pagamento");
            var usuarioId = Guid.NewGuid();

            var handler = new AtribuirPapelUsuarioCommandHandler(ctx, new FakeTenant(TenantId), new FakeUser("admin"), new FakeAvaliador(bloquear: true));
            var r = await handler.Handle(new AtribuirPapelUsuarioCommand(usuarioId, papelId, null), CancellationToken.None);

            Assert.False(r.Sucesso);
            Assert.True(r.Block);
            // Nada persistido — a concessão foi negada antes de gravar.
            Assert.Empty(await ctx.UsuariosPapeis.ToListAsync());
        }

        [Fact(DisplayName = "SoD/RBAC | Concessão prossegue quando o avaliador libera")]
        public async Task Concessao_Permitida_Sem_Conflito()
        {
            using var ctx = NovoContexto(nameof(Concessao_Permitida_Sem_Conflito));
            var papelId = await SemearPapelAsync(ctx, "Consultar Relatórios");
            var usuarioId = Guid.NewGuid();

            var handler = new AtribuirPapelUsuarioCommandHandler(ctx, new FakeTenant(TenantId), new FakeUser("admin"), new FakeAvaliador(bloquear: false));
            var r = await handler.Handle(new AtribuirPapelUsuarioCommand(usuarioId, papelId, null), CancellationToken.None);

            Assert.True(r.Sucesso);
            Assert.Single(await ctx.UsuariosPapeis.ToListAsync());
        }

        [Fact(DisplayName = "SoD/RBAC | Sem avaliador (legado) a concessão não é afetada")]
        public async Task Concessao_Sem_Avaliador_Preserva_Legado()
        {
            using var ctx = NovoContexto(nameof(Concessao_Sem_Avaliador_Preserva_Legado));
            var papelId = await SemearPapelAsync(ctx, "Papel X");
            var handler = new AtribuirPapelUsuarioCommandHandler(ctx, new FakeTenant(TenantId), new FakeUser("admin"));
            var r = await handler.Handle(new AtribuirPapelUsuarioCommand(Guid.NewGuid(), papelId, null), CancellationToken.None);
            Assert.True(r.Sucesso);
        }

        private sealed class FakeAvaliador : ISoDAvaliadorConcessao
        {
            private readonly bool _bloquear;
            public FakeAvaliador(bool bloquear) => _bloquear = bloquear;
            public Task<SoDResultadoConcessao> AvaliarConcessaoAsync(Guid? usuarioId, IEnumerable<Guid> funcoesAtuais, IEnumerable<Guid> funcoesNovas, CancellationToken cancellationToken = default)
                => Task.FromResult(_bloquear
                    ? new SoDResultadoConcessao { Bloqueado = true, TemConflito = true, RegrasBloqueantes = new[] { Guid.NewGuid() } }
                    : SoDResultadoConcessao.Liberado);
        }

        private sealed class FakeTenant : ITenantProvider
        {
            private readonly string _t; public FakeTenant(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private sealed class FakeUser : ICurrentUser
        {
            private readonly string _u; public FakeUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "test";
            public string? GetUserEmail() => "t@epros.com";
        }
    }
}
