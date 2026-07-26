using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// MAN-CRV (Confiabilidade e Revisao) e MAN-PDT (Manutencao Preditiva).
    /// Testa regras-chave: unicidade de codigo, RPN=SxOxD, workflow, leitura/alarme so com ativo, motivos obrigatorios.
    /// </summary>
    public class ManutencaoConfiabilidadePreditivaTests
    {
        private const string TenantId = "tenant-man-crvpdt";
        private const string UserId = "user-man-crvpdt";

        private static ContextManutencao NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextManutencao>()
                .UseInMemoryDatabase(db)
                .Options;
            return new ContextManutencao(options, new TP(TenantId), new CU(UserId));
        }

        // ===================== MAN-CRV =====================
        [Fact(DisplayName = "MAN-CRV | Criar revisao valida gera rascunho")]
        public async Task Crv_CriarRevisao_DeveCriarRascunho()
        {
            using var ctx = NovoContexto(nameof(Crv_CriarRevisao_DeveCriarRascunho));
            var handler = new CriarRevisaoConfiabilidadeCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new CriarRevisaoConfiabilidadeCommand("CRV-01", "Revisao bomba", Guid.NewGuid(), Guid.NewGuid(), null, null, "Alta"), CancellationToken.None);
            Assert.True(result.Sucesso);
            var rev = await ctx.RevisoesConfiabilidade.FirstAsync();
            Assert.Equal(EStatusRegistroManutencao.Rascunho, rev.Status);
        }

        [Fact(DisplayName = "MAN-CRV | Codigo duplicado deve bloquear (RN-CRV-001)")]
        public async Task Crv_CodigoDuplicado_DeveFalhar()
        {
            using var ctx = NovoContexto(nameof(Crv_CodigoDuplicado_DeveFalhar));
            var handler = new CriarRevisaoConfiabilidadeCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            await handler.Handle(new CriarRevisaoConfiabilidadeCommand("CRV-DUP", "A", Guid.NewGuid(), null, null, null, null), CancellationToken.None);
            var result = await handler.Handle(new CriarRevisaoConfiabilidadeCommand("CRV-DUP", "B", Guid.NewGuid(), null, null, null, null), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "MAN-CRV | Modo de falha calcula RPN = S x O x D")]
        public void Crv_ModoFalha_DeveCalcularRpn()
        {
            var modo = new ModoFalhaConfiabilidade(Guid.NewGuid(), 1, "Rolamento", "Desgaste", "Ruido", "Falta de lubrificacao", "Inspecao", 8, 5, 4, null, null, TenantId, UserId);
            Assert.True(modo.IsValid);
            Assert.Equal(160, modo.Rpn);
        }

        [Fact(DisplayName = "MAN-CRV | Fator FMEA fora da escala 1-10 deve invalidar")]
        public void Crv_ModoFalha_FatorForaEscala_DeveInvalidar()
        {
            var modo = new ModoFalhaConfiabilidade(Guid.NewGuid(), 1, null, "Falha", null, null, null, 11, 5, 4, null, null, TenantId, UserId);
            Assert.False(modo.IsValid);
        }

        [Fact(DisplayName = "MAN-CRV | Submeter e aprovar move rascunho -> analise -> ativo")]
        public void Crv_Workflow_SubmeterAprovar_DeveAtivar()
        {
            var rev = new RevisaoConfiabilidade("CRV-WF", "Revisao", Guid.NewGuid(), Guid.NewGuid(), null, null, null, TenantId, UserId);
            rev.Submeter(UserId);
            Assert.True(rev.IsValid);
            Assert.Equal(EStatusRegistroManutencao.EmAnalise, rev.Status);
            rev.Aprovar(Guid.NewGuid(), UserId);
            Assert.True(rev.IsValid);
            Assert.Equal(EStatusRegistroManutencao.Ativo, rev.Status);
        }

        [Fact(DisplayName = "MAN-CRV | Suspender ativa sem motivo deve falhar (RN-CRV-009)")]
        public void Crv_SuspenderSemMotivo_DeveFalhar()
        {
            var rev = new RevisaoConfiabilidade("CRV-SUS", "Revisao", Guid.NewGuid(), Guid.NewGuid(), null, null, null, TenantId, UserId);
            rev.Submeter(UserId);
            rev.Aprovar(Guid.NewGuid(), UserId);
            rev.Suspender("", UserId);
            Assert.False(rev.IsValid);
        }

        [Fact(DisplayName = "MAN-CRV | Suspenso retorna para ativo ao retomar (RN-CRV-010)")]
        public void Crv_Retomar_DeveVoltarAtivo()
        {
            var rev = new RevisaoConfiabilidade("CRV-RET", "Revisao", Guid.NewGuid(), Guid.NewGuid(), null, null, null, TenantId, UserId);
            rev.Submeter(UserId);
            rev.Aprovar(Guid.NewGuid(), UserId);
            rev.Suspender("Manutencao pausada", UserId);
            Assert.Equal(EStatusRegistroManutencao.Suspenso, rev.Status);
            rev.Retomar(UserId);
            Assert.Equal(EStatusRegistroManutencao.Ativo, rev.Status);
        }

        // ===================== MAN-PDT =====================
        [Fact(DisplayName = "MAN-PDT | Criar monitoramento valido gera rascunho")]
        public async Task Pdt_CriarMonitoramento_DeveCriarRascunho()
        {
            using var ctx = NovoContexto(nameof(Pdt_CriarMonitoramento_DeveCriarRascunho));
            var handler = new CriarMonitoramentoPreditivoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new CriarMonitoramentoPreditivoCommand("PDT-01", "Vibracao motor", Guid.NewGuid(), Guid.NewGuid(), null), CancellationToken.None);
            Assert.True(result.Sucesso);
            var m = await ctx.MonitoramentosPreditivos.FirstAsync();
            Assert.Equal(EStatusRegistroManutencao.Rascunho, m.Status);
        }

        [Fact(DisplayName = "MAN-PDT | Codigo duplicado deve bloquear")]
        public async Task Pdt_CodigoDuplicado_DeveFalhar()
        {
            using var ctx = NovoContexto(nameof(Pdt_CodigoDuplicado_DeveFalhar));
            var handler = new CriarMonitoramentoPreditivoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            await handler.Handle(new CriarMonitoramentoPreditivoCommand("PDT-DUP", "A", Guid.NewGuid(), Guid.NewGuid(), null), CancellationToken.None);
            var result = await handler.Handle(new CriarMonitoramentoPreditivoCommand("PDT-DUP", "B", Guid.NewGuid(), Guid.NewGuid(), null), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "MAN-PDT | Leitura em monitoramento nao ativo deve falhar")]
        public async Task Pdt_LeituraMonitoramentoNaoAtivo_DeveFalhar()
        {
            using var ctx = NovoContexto(nameof(Pdt_LeituraMonitoramentoNaoAtivo_DeveFalhar));
            var equip = Guid.NewGuid();
            var m = new MonitoramentoPreditivo("PDT-LC", "Mon", Guid.NewGuid(), equip, null, TenantId, UserId);
            var ponto = new PontoMedicao(m.Id, equip, "P1", "Vibracao", "mm/s", null, null, TenantId, UserId);
            m.AdicionarPontoMedicao(ponto, UserId);
            ctx.MonitoramentosPreditivos.Add(m);
            await ctx.SaveChangesAsync();

            var handler = new RegistrarLeituraCondicaoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new RegistrarLeituraCondicaoCommand(ponto.Id, DateTime.UtcNow, 4.2m, "mm/s", null, "sensor", null), CancellationToken.None);
            Assert.False(result.Sucesso); // monitoramento ainda em rascunho
        }

        [Fact(DisplayName = "MAN-PDT | Ponto suspenso nao recebe leitura")]
        public void Pdt_PontoSuspenso_NaoRecebeLeitura()
        {
            var ponto = new PontoMedicao(Guid.NewGuid(), Guid.NewGuid(), "P2", "Temperatura", "C", null, null, TenantId, UserId);
            ponto.Suspender(UserId);
            var leitura = new LeituraCondicao(ponto.Id, DateTime.UtcNow, 70m, "C", null, "sensor", null, TenantId, UserId);
            ponto.RegistrarLeitura(leitura, UserId);
            Assert.False(ponto.IsValid);
        }

        [Fact(DisplayName = "MAN-PDT | Alarme com regra nao ativa deve falhar")]
        public async Task Pdt_AlarmeRegraNaoAtiva_DeveFalhar()
        {
            using var ctx = NovoContexto(nameof(Pdt_AlarmeRegraNaoAtiva_DeveFalhar));
            var equip = Guid.NewGuid();
            var m = new MonitoramentoPreditivo("PDT-AL", "Mon", Guid.NewGuid(), equip, null, TenantId, UserId);
            m.Submeter(UserId);
            m.Aprovar(UserId);
            var ponto = new PontoMedicao(m.Id, equip, "P3", "Corrente", "A", null, null, TenantId, UserId);
            var regra = new RegraMonitoramento(ponto.Id, ETipoRegraMonitoramento.Limite, ">", null, 10m, null, "Alta", "Alerta", null, null, TenantId, UserId);
            // regra permanece em Rascunho (nao ativada)
            ctx.MonitoramentosPreditivos.Add(m);
            ctx.PontosMedicao.Add(ponto);
            ctx.RegrasMonitoramento.Add(regra);
            await ctx.SaveChangesAsync();

            var handler = new DispararAlarmePreditivoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new DispararAlarmePreditivoCommand(m.Id, ponto.Id, regra.Id, null, "Alta", "Limite excedido"), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "MAN-PDT | Alarme com regra ativa e monitoramento ativo deve disparar")]
        public async Task Pdt_AlarmeRegraAtiva_DeveDisparar()
        {
            using var ctx = NovoContexto(nameof(Pdt_AlarmeRegraAtiva_DeveDisparar));
            var equip = Guid.NewGuid();
            var m = new MonitoramentoPreditivo("PDT-OK", "Mon", Guid.NewGuid(), equip, null, TenantId, UserId);
            m.Submeter(UserId);
            m.Aprovar(UserId);
            var ponto = new PontoMedicao(m.Id, equip, "P4", "Vibracao", "mm/s", null, null, TenantId, UserId);
            var regra = new RegraMonitoramento(ponto.Id, ETipoRegraMonitoramento.Limite, ">", null, 5m, null, "Alta", "Alerta", null, null, TenantId, UserId);
            regra.Ativar(UserId);
            ctx.MonitoramentosPreditivos.Add(m);
            ctx.PontosMedicao.Add(ponto);
            ctx.RegrasMonitoramento.Add(regra);
            await ctx.SaveChangesAsync();

            var handler = new DispararAlarmePreditivoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new DispararAlarmePreditivoCommand(m.Id, ponto.Id, regra.Id, null, "Alta", "Limite excedido"), CancellationToken.None);
            Assert.True(result.Sucesso);
        }

        [Fact(DisplayName = "MAN-PDT | Descartar alarme sem motivo deve falhar")]
        public void Pdt_DescartarAlarmeSemMotivo_DeveFalhar()
        {
            var alarme = new AlarmePreditivo(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, "Alta", "Ruido anormal", TenantId, UserId);
            alarme.Descartar("", UserId);
            Assert.False(alarme.IsValid);
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
            public string? GetUserName() => "man-crvpdt";
            public string? GetUserEmail() => "man-crvpdt@epros.com.br";
        }
    }
}
