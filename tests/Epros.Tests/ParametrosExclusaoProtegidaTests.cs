using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;

namespace Epros.Tests
{
    // P1-1 (auditoria CADASTROS): a exclusão-protegida de Categoria/Unidade/Armazém/Projeto/Imposto era um
    // STUB (`if (Nome.Contains("Em Uso"))`) que apagava fisicamente registros em uso. Estes testes cobrem o
    // novo comportamento REAL: (1) UnidadeMedida referenciada por ConversaoUnidade não pode ser excluída;
    // (2) a exclusão é soft-delete (some das listagens, preserva a linha física); (3) recriar um catálogo
    // com o mesmo nome de um soft-deletado restaura a linha (sem colisão do índice único (TenantId, Nome)).
    public class ParametrosExclusaoProtegidaTests
    {
        private const string TenantId = "tenant-parametros-test";
        private const string UsuarioId = "user-parametros-test";

        private ContextGestaoClientes CreateContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(databaseName)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ContextGestaoClientes(options, new FakeTenant(TenantId), new FakeUser(UsuarioId));
        }

        [Fact]
        public async Task Unidade_Em_Uso_Por_Conversao_Nao_Pode_Ser_Excluida()
        {
            var ctx = CreateContext(nameof(Unidade_Em_Uso_Por_Conversao_Nao_Pode_Ser_Excluida));
            var tenant = new FakeTenant(TenantId);
            var user = new FakeUser(UsuarioId);

            var origem = await new CriarUnidadeMedidaCommandHandler(ctx, tenant, user)
                .Handle(new CriarUnidadeMedidaCommand("Caixa", null), CancellationToken.None);
            var destino = await new CriarUnidadeMedidaCommandHandler(ctx, tenant, user)
                .Handle(new CriarUnidadeMedidaCommand("Unidade", null), CancellationToken.None);
            var origemId = (Guid)origem.Dados!.GetType().GetProperty("UnidadeId")!.GetValue(origem.Dados)!;
            var destinoId = (Guid)destino.Dados!.GetType().GetProperty("UnidadeId")!.GetValue(destino.Dados)!;

            await new AdicionarConversaoUnidadeCommandHandler(ctx, tenant, user)
                .Handle(new AdicionarConversaoUnidadeCommand(origemId, destinoId, 12m), CancellationToken.None);

            var resultado = await new ExcluirUnidadeMedidaCommandHandler(ctx, tenant, user)
                .Handle(new ExcluirUnidadeMedidaCommand(origemId), CancellationToken.None);

            Assert.False(resultado.Sucesso);
            // A unidade continua existindo (não foi apagada).
            Assert.True(await ctx.UnidadesMedida.IgnoreQueryFilters().AnyAsync(u => u.Id == origemId));
        }

        [Fact]
        public async Task Unidade_Sem_Vinculo_E_Soft_Deletada()
        {
            var ctx = CreateContext(nameof(Unidade_Sem_Vinculo_E_Soft_Deletada));
            var tenant = new FakeTenant(TenantId);
            var user = new FakeUser(UsuarioId);

            var criada = await new CriarUnidadeMedidaCommandHandler(ctx, tenant, user)
                .Handle(new CriarUnidadeMedidaCommand("Litro", null), CancellationToken.None);
            var id = (Guid)criada.Dados!.GetType().GetProperty("UnidadeId")!.GetValue(criada.Dados)!;

            var resultado = await new ExcluirUnidadeMedidaCommandHandler(ctx, tenant, user)
                .Handle(new ExcluirUnidadeMedidaCommand(id), CancellationToken.None);

            Assert.True(resultado.Sucesso);
            // Some das consultas normais (filtro global de soft-delete)...
            Assert.False(await ctx.UnidadesMedida.AnyAsync(u => u.Id == id));
            // ...mas a linha física permanece (integridade referencial preservada).
            var fisica = await ctx.UnidadesMedida.IgnoreQueryFilters().FirstAsync(u => u.Id == id);
            Assert.NotNull(fisica.DeletadoEm);
        }

        [Fact]
        public async Task Recriar_Categoria_Homonima_Restaura_Soft_Deletada()
        {
            var ctx = CreateContext(nameof(Recriar_Categoria_Homonima_Restaura_Soft_Deletada));
            var tenant = new FakeTenant(TenantId);
            var user = new FakeUser(UsuarioId);

            var criada = await new CriarCategoriaCommandHandler(ctx, tenant, user)
                .Handle(new CriarCategoriaCommand("Bebidas", null), CancellationToken.None);
            var id = (Guid)criada.Dados!.GetType().GetProperty("CategoriaId")!.GetValue(criada.Dados)!;

            await new ExcluirCategoriaCommandHandler(ctx, tenant, user)
                .Handle(new ExcluirCategoriaCommand(id), CancellationToken.None);

            var recriada = await new CriarCategoriaCommandHandler(ctx, tenant, user)
                .Handle(new CriarCategoriaCommand("Bebidas", null), CancellationToken.None);
            var idRecriada = (Guid)recriada.Dados!.GetType().GetProperty("CategoriaId")!.GetValue(recriada.Dados)!;

            Assert.True(recriada.Sucesso);
            // Restaura a MESMA linha (mesmo Id) em vez de inserir uma duplicata / violar o índice único.
            Assert.Equal(id, idRecriada);
            var linha = await ctx.Categorias.IgnoreQueryFilters().FirstAsync(c => c.Id == id);
            Assert.Null(linha.DeletadoEm);
            Assert.Equal(1, await ctx.Categorias.IgnoreQueryFilters().CountAsync(c => c.Nome == "Bebidas" && c.TenantId == TenantId));
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
    }
}
