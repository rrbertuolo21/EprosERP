using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// MAN-IND — D4/D5: caminhos de escrita antes ausentes (vinculo patrimonial, servico, atributo).
    /// </summary>
    public class ManutencaoInducaoEscritaTests
    {
        private const string TenantId = "tenant-man-ind2";
        private const string UserId = "user-man-ind2";

        private static ContextManutencao NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextManutencao>().UseInMemoryDatabase(db).Options;
            return new ContextManutencao(options, new TP(TenantId), new CU(UserId));
        }

        private static Guid SeedEquipamento(string db)
        {
            using var seed = NovoContexto(db);
            var eq = new Equipamento("Bomba", "BMB-1", "Utilidades", DateTime.UtcNow, "Media", TenantId, UserId);
            seed.Equipamentos.Add(eq);
            seed.SaveChanges();
            return eq.Id;
        }

        [Fact(DisplayName = "MAN-IND | Vincular patrimonio persiste snapshot")]
        public async Task Ind_VincularPatrimonio_Persiste()
        {
            var db = nameof(Ind_VincularPatrimonio_Persiste);
            var eqId = SeedEquipamento(db);
            using var ctx = NovoContexto(db);
            var handler = new VincularPatrimonioEquipamentoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new VincularPatrimonioEquipamentoCommand(eqId, Guid.NewGuid(), "PAT-100", "Bomba centrifuga"), CancellationToken.None);
            Assert.True(result.Sucesso);
            Assert.Equal(1, await ctx.EquipamentoVinculosPatrimoniais.CountAsync(v => v.EquipamentoId == eqId));
        }

        [Fact(DisplayName = "MAN-IND | Adicionar servico ao equipamento e bloquear duplicado")]
        public async Task Ind_AdicionarServico_Dedup()
        {
            var db = nameof(Ind_AdicionarServico_Dedup);
            var eqId = SeedEquipamento(db);
            var servicoId = Guid.NewGuid();

            using (var ctx1 = NovoContexto(db))
            {
                var h1 = new AdicionarServicoEquipamentoCommandHandler(ctx1, new TP(TenantId), new CU(UserId));
                var r1 = await h1.Handle(new AdicionarServicoEquipamentoCommand(servicoId, eqId, null, null, null), CancellationToken.None);
                Assert.True(r1.Sucesso);
            }
            using var ctx = NovoContexto(db);
            var h2 = new AdicionarServicoEquipamentoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var r2 = await h2.Handle(new AdicionarServicoEquipamentoCommand(servicoId, eqId, null, null, null), CancellationToken.None);
            Assert.False(r2.Sucesso); // duplicado
        }

        [Fact(DisplayName = "MAN-IND | Definir atributo livre persiste")]
        public async Task Ind_DefinirAtributo_Persiste()
        {
            var db = nameof(Ind_DefinirAtributo_Persiste);
            var eqId = SeedEquipamento(db);
            using var ctx = NovoContexto(db);
            var handler = new DefinirAtributoEquipamentoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new DefinirAtributoEquipamentoCommand(eqId, "Potencia", "150cv", "Ficha tecnica"), CancellationToken.None);
            Assert.True(result.Sucesso);
            Assert.Equal(1, await ctx.EquipamentoAtributos.CountAsync(a => a.EquipamentoId == eqId && a.Chave == "Potencia"));
        }

        [Fact(DisplayName = "MAN-IND | Vincular patrimonio em equipamento inexistente falha")]
        public async Task Ind_VincularPatrimonio_EquipamentoInexistente_Falha()
        {
            var db = nameof(Ind_VincularPatrimonio_EquipamentoInexistente_Falha);
            using var ctx = NovoContexto(db);
            var handler = new VincularPatrimonioEquipamentoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new VincularPatrimonioEquipamentoCommand(Guid.NewGuid(), Guid.NewGuid(), null, null), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        private class TP : ITenantProvider
        {
            private readonly string _t;
            public TP(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class CU : ICurrentUser
        {
            private readonly string _u;
            public CU(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "man-ind2";
            public string? GetUserEmail() => "man-ind2@epros.com.br";
        }
    }
}
