using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Modules.Manutencao.Domain.Services;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// MAN-CRV/MAN-PAR — D16/D24: motor unico de indicadores (MTTR/MTBF/Disponibilidade).
    /// Cobre a funcao pura e os dois handlers que a consomem (Confiabilidade e Paradas).
    /// </summary>
    public class ManutencaoIndicadoresTests
    {
        private const string TenantId = "tenant-man-ind";
        private const string UserId = "user-man-ind";

        private static ContextManutencao NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextManutencao>()
                .UseInMemoryDatabase(db)
                .Options;
            return new ContextManutencao(options, new TP(TenantId), new CU(UserId));
        }

        // ===================== Motor puro =====================
        [Fact(DisplayName = "Motor | Sem falhas: MTTR/MTBF nulos e disponibilidade 100%")]
        public void Motor_SemFalhas_MttrMtbfNulos()
        {
            var ini = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var fim = ini.AddDays(10);
            var r = MotorIndicadoresConfiabilidade.Calcular(Array.Empty<MotorIndicadoresConfiabilidade.ParadaCalculo>(), ini, fim);
            Assert.Null(r.MttrHoras);
            Assert.Null(r.MtbfHoras);
            Assert.Equal(0, r.QuantidadeFalhas);
            Assert.Equal(100m, r.DisponibilidadePercentual);
        }

        [Fact(DisplayName = "Motor | Duas falhas de 2h calculam MTTR=2h")]
        public void Motor_DuasFalhas_CalculaMttr()
        {
            var ini = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var fim = ini.AddDays(30);
            var paradas = new[]
            {
                new MotorIndicadoresConfiabilidade.ParadaCalculo(ini.AddDays(2), ini.AddDays(2).AddHours(2), true),
                new MotorIndicadoresConfiabilidade.ParadaCalculo(ini.AddDays(10), ini.AddDays(10).AddHours(2), true),
            };
            var r = MotorIndicadoresConfiabilidade.Calcular(paradas, ini, fim);
            Assert.Equal(2, r.QuantidadeFalhas);
            Assert.Equal(2m, r.MttrHoras); // (2h + 2h) / 2 falhas = 2h
            Assert.NotNull(r.MtbfHoras);
            // MTBF = tempo operacional / falhas; operacional = 720h - 4h = 716h; /2 = 358h
            Assert.Equal(358m, r.MtbfHoras);
        }

        [Fact(DisplayName = "Motor | Parada planejada reduz disponibilidade mas nao entra em MTTR")]
        public void Motor_ParadaPlanejada_NaoAfetaMttr()
        {
            var ini = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var fim = ini.AddHours(100);
            var paradas = new[]
            {
                // planejada 10h -> so disponibilidade
                new MotorIndicadoresConfiabilidade.ParadaCalculo(ini.AddHours(1), ini.AddHours(11), false),
            };
            var r = MotorIndicadoresConfiabilidade.Calcular(paradas, ini, fim);
            Assert.Null(r.MttrHoras);
            Assert.Equal(0, r.QuantidadeFalhas);
            Assert.Equal(90m, r.DisponibilidadePercentual); // (100 - 10)/100
        }

        [Fact(DisplayName = "Motor | Parada em aberto (sem fim) e ignorada")]
        public void Motor_ParadaEmAberto_Ignorada()
        {
            var ini = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var fim = ini.AddHours(10);
            var paradas = new[]
            {
                new MotorIndicadoresConfiabilidade.ParadaCalculo(ini.AddHours(1), null, true),
            };
            var r = MotorIndicadoresConfiabilidade.Calcular(paradas, ini, fim);
            Assert.Equal(0, r.QuantidadeFalhas);
            Assert.Equal(100m, r.DisponibilidadePercentual);
        }

        [Fact(DisplayName = "Motor | Downtime fora do periodo e recortado")]
        public void Motor_RecorteDePeriodo()
        {
            var ini = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            var fim = ini.AddHours(10);
            var paradas = new[]
            {
                // comeca antes do periodo, termina 2h dentro
                new MotorIndicadoresConfiabilidade.ParadaCalculo(ini.AddHours(-5), ini.AddHours(2), true),
            };
            var r = MotorIndicadoresConfiabilidade.Calcular(paradas, ini, fim);
            Assert.Equal(1, r.QuantidadeFalhas);
            Assert.Equal(2m, r.MttrHoras); // so 2h dentro do periodo
        }

        [Fact(DisplayName = "Motor | Periodo invalido lanca")]
        public void Motor_PeriodoInvalido_Lanca()
        {
            var ini = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.Throws<ArgumentException>(() =>
                MotorIndicadoresConfiabilidade.Calcular(Array.Empty<MotorIndicadoresConfiabilidade.ParadaCalculo>(), ini, ini));
        }

        // ===================== Handler CRV =====================
        [Fact(DisplayName = "MAN-CRV | Motor calcula e persiste indicadores do sistema")]
        public async Task Crv_CalcularIndicadores_PersisteSistema()
        {
            var db = nameof(Crv_CalcularIndicadores_PersisteSistema);
            var equip = Guid.NewGuid();
            var ini = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var fim = ini.AddDays(30);
            Guid revId;

            using (var seed = NovoContexto(db))
            {
                // duas falhas de 3h
                var p1 = new Parada("PAR-1", "Falha rolamento", Guid.NewGuid(), ETipoParada.NaoPlanejada, ini.AddDays(3), Guid.NewGuid(), equip, null, null, null, null, TenantId, UserId);
                p1.Finalizar(ini.AddDays(3).AddHours(3), UserId);
                var p2 = new Parada("PAR-2", "Falha motor", Guid.NewGuid(), ETipoParada.NaoPlanejada, ini.AddDays(15), Guid.NewGuid(), equip, null, null, null, null, TenantId, UserId);
                p2.Finalizar(ini.AddDays(15).AddHours(3), UserId);
                seed.Paradas.AddRange(p1, p2);
                var rev = new RevisaoConfiabilidade("CRV-IND", "Revisao bomba", Guid.NewGuid(), equip, null, null, "Alta", TenantId, UserId);
                seed.RevisoesConfiabilidade.Add(rev);
                await seed.SaveChangesAsync();
                revId = rev.Id;
            }

            using var ctx = NovoContexto(db);
            var handler = new CalcularIndicadoresConfiabilidadeCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new CalcularIndicadoresConfiabilidadeCommand(revId, ini, fim, null), CancellationToken.None);
            Assert.True(result.Sucesso);

            var indicadores = await ctx.IndicadoresConfiabilidade.Where(i => i.RevisaoId == revId).ToListAsync();
            Assert.Equal(3, indicadores.Count); // MTTR + MTBF + Disponibilidade
            Assert.All(indicadores, i => Assert.Equal(ECalculadoPorConfiabilidade.Sistema, i.CalculadoPor));
            var mttr = indicadores.First(i => i.TipoIndicador == ETipoIndicadorConfiabilidade.Mttr);
            Assert.Equal(3m, mttr.Valor);
        }

        [Fact(DisplayName = "MAN-CRV | Sem equipamento vinculado deve falhar")]
        public async Task Crv_SemEquipamento_DeveFalhar()
        {
            var db = nameof(Crv_SemEquipamento_DeveFalhar);
            var ini = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Guid revId;
            using (var seed = NovoContexto(db))
            {
                var rev = new RevisaoConfiabilidade("CRV-NOEQ", "Revisao", Guid.NewGuid(), null, null, null, null, TenantId, UserId);
                seed.RevisoesConfiabilidade.Add(rev);
                await seed.SaveChangesAsync();
                revId = rev.Id;
            }

            using var ctx = NovoContexto(db);
            var handler = new CalcularIndicadoresConfiabilidadeCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new CalcularIndicadoresConfiabilidadeCommand(revId, ini, ini.AddDays(10), null), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        // ===================== Handler PAR =====================
        [Fact(DisplayName = "MAN-PAR | Motor persiste snapshot em man_par_indicador")]
        public async Task Par_CalcularIndicadores_PersisteSnapshot()
        {
            var db = nameof(Par_CalcularIndicadores_PersisteSnapshot);
            var equip = Guid.NewGuid();
            var ini = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
            var fim = ini.AddDays(30);
            Guid paradaId;

            using (var seed = NovoContexto(db))
            {
                var p1 = new Parada("PARX-1", "Falha", Guid.NewGuid(), ETipoParada.NaoPlanejada, ini.AddDays(2), Guid.NewGuid(), equip, null, null, null, null, TenantId, UserId);
                p1.Finalizar(ini.AddDays(2).AddHours(4), UserId);
                seed.Paradas.Add(p1);
                await seed.SaveChangesAsync();
                paradaId = p1.Id;
            }

            using var ctx = NovoContexto(db);
            var handler = new CalcularIndicadoresParadaCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new CalcularIndicadoresParadaCommand(paradaId, ini, fim, null), CancellationToken.None);
            Assert.True(result.Sucesso);

            var indicadores = await ctx.ParadaIndicadores.Where(i => i.ParadaId == paradaId).ToListAsync();
            Assert.Equal(3, indicadores.Count);
            Assert.Contains(indicadores, i => i.TipoIndicador == ETipoIndicadorParada.MTTR && i.Valor == 4m);
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
            public string? GetUserName() => "man-ind";
            public string? GetUserEmail() => "man-ind@epros.com.br";
        }
    }
}
