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
    // T-02 / P1-3 (auditoria CADASTROS): importação em lote OPERÁVEL. As entidades existiam sem
    // command/handler; estes testes exercem o fluxo (mapeamento + validação + processamento) e a
    // política tudo-ou-nada.
    public class ImportacaoLotePessoasTests
    {
        private const string TenantId = "tenant-import-lote";
        private const string UserId = "user-import-lote";
        private const string CnpjValido = "12345678000195";

        private ContextGestaoClientes CreateContext(string db)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(db)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ContextGestaoClientes(options, new FakeTenant(TenantId), new FakeUser(UserId));
        }

        private ImportarPessoasLoteCommandHandler Handler(ContextGestaoClientes ctx)
            => new(ctx, new FakeTenant(TenantId), new FakeUser(UserId), new FakeLimites());

        [Fact]
        public async Task Importa_Lote_Valido_Cria_Pessoas_E_Marca_Concluida()
        {
            var ctx = CreateContext(nameof(Importa_Lote_Valido_Cria_Pessoas_E_Marca_Concluida));
            var cmd = new ImportarPessoasLoteCommand("clientes.csv", "v1", false, new List<PessoaImportacaoLinhaInput>
            {
                new(1, ETipoPessoa.PessoaJuridica, CnpjValido, "Acme LTDA", null, "Acme", "acme@x.com", "1130001000"),
            });

            var result = await Handler(ctx).Handle(cmd, CancellationToken.None);
            Assert.True(result.Sucesso);

            var lote = await ctx.PessoasImportacaoLote.Include(l => l.Linhas).FirstAsync();
            Assert.Equal(EStatusImportacao.Concluida, lote.Status);
            Assert.Equal(1, lote.LinhasAceitas);
            Assert.Equal(0, lote.LinhasRejeitadas);
            Assert.Equal(1, await ctx.Pessoas.CountAsync());
            Assert.All(lote.Linhas, l => Assert.NotNull(l.PessoaIdGerada));
        }

        [Fact]
        public async Task Importa_Parcial_Aceita_Validas_E_Rejeita_Invalidas()
        {
            var ctx = CreateContext(nameof(Importa_Parcial_Aceita_Validas_E_Rejeita_Invalidas));
            var cmd = new ImportarPessoasLoteCommand("clientes.csv", "v1", false, new List<PessoaImportacaoLinhaInput>
            {
                new(1, ETipoPessoa.PessoaJuridica, CnpjValido, "Acme LTDA", null, "Acme", null, null),
                new(2, ETipoPessoa.PessoaJuridica, null, "Sem Documento LTDA", null, null, null, null), // erro de mapeamento
            });

            var result = await Handler(ctx).Handle(cmd, CancellationToken.None);
            Assert.True(result.Sucesso);

            var lote = await ctx.PessoasImportacaoLote.Include(l => l.Linhas).FirstAsync();
            Assert.Equal(EStatusImportacao.ConcluidaComErros, lote.Status);
            Assert.Equal(1, lote.LinhasAceitas);
            Assert.Equal(1, lote.LinhasRejeitadas);
            Assert.Equal(1, await ctx.Pessoas.CountAsync());
            var rejeitada = lote.Linhas.First(l => l.NumeroLinha == 2);
            Assert.Equal(EStatusLinhaImportacao.Rejeitada, rejeitada.Status);
            Assert.False(string.IsNullOrWhiteSpace(rejeitada.MensagemErro));
        }

        [Fact]
        public async Task Politica_Tudo_Ou_Nada_Rejeita_Lote_Sem_Criar_Pessoa()
        {
            var ctx = CreateContext(nameof(Politica_Tudo_Ou_Nada_Rejeita_Lote_Sem_Criar_Pessoa));
            var cmd = new ImportarPessoasLoteCommand("clientes.csv", "v1", true /*abortar*/, new List<PessoaImportacaoLinhaInput>
            {
                new(1, ETipoPessoa.PessoaJuridica, CnpjValido, "Acme LTDA", null, "Acme", null, null),
                new(2, ETipoPessoa.PessoaJuridica, null, "Sem Documento LTDA", null, null, null, null), // erro → aborta tudo
            });

            var result = await Handler(ctx).Handle(cmd, CancellationToken.None);
            Assert.True(result.Sucesso);

            var lote = await ctx.PessoasImportacaoLote.Include(l => l.Linhas).FirstAsync();
            Assert.Equal(EStatusImportacao.Rejeitada, lote.Status);
            Assert.Equal(0, lote.LinhasAceitas);
            Assert.Equal(2, lote.LinhasRejeitadas);
            Assert.Equal(0, await ctx.Pessoas.CountAsync()); // nenhuma pessoa criada
            Assert.All(lote.Linhas, l => Assert.Equal(EStatusLinhaImportacao.Rejeitada, l.Status));
        }

        [Fact]
        public async Task Lote_Vazio_Retorna_Falha()
        {
            var ctx = CreateContext(nameof(Lote_Vazio_Retorna_Falha));
            var result = await Handler(ctx).Handle(
                new ImportarPessoasLoteCommand("x.csv", "v1", false, new List<PessoaImportacaoLinhaInput>()),
                CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        private sealed class FakeTenant : ITenantProvider
        {
            private readonly string _t;
            public FakeTenant(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private sealed class FakeUser : ICurrentUser
        {
            private readonly string _u;
            public FakeUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => _u;
            public string? GetUserEmail() => _u + "@test.local";
        }

        private sealed class FakeLimites : IValidadorLimitesSaaS
        {
            public Task<bool> PossuiFolgaUsuariosAsync(string tenantId, CancellationToken ct = default) => Task.FromResult(true);
            public Task<bool> PossuiFolgaEmpresasAsync(string tenantId, CancellationToken ct = default) => Task.FromResult(true);
            public Task<(bool Excedido, string Mensagem)> ValidarLimiteUsuariosAsync(string tenantId, CancellationToken ct = default) => Task.FromResult((false, ""));
            public Task<(bool Excedido, string Mensagem)> ValidarLimiteEmpresasAsync(string tenantId, CancellationToken ct = default) => Task.FromResult((false, ""));
            public Task<(bool Excedido, string Mensagem)> ValidarLimiteClientesAsync(string tenantId, CancellationToken ct = default) => Task.FromResult((false, ""));
            public Task<(bool Excedido, string Mensagem)> ValidarLimitePermissoesAsync(string tenantId, CancellationToken ct = default) => Task.FromResult((false, ""));
        }
    }
}
