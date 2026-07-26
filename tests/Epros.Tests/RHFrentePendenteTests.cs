using System;
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
    // RH Frentes 1-2 (pendentes): RH-DEV, RH-LMS, RH-PLN, RH-TLT, RH-WFM. Regras-chave das EFs.
    public class RHFrentePendenteTests
    {
        private static ContextRH NovoContext(string db)
        {
            var options = new DbContextOptionsBuilder<ContextRH>().UseInMemoryDatabase(db).Options;
            return new ContextRH(options, new TP("tenant-1"), new CU("user-1"));
        }

        // RH-DEV secao 14.9: status de promocao aceita apenas Pendente/Aprovado/Rejeitado.
        [Fact]
        public void DEV_Promocao_Status_Dominio()
        {
            Assert.True(DevPromocao.StatusValido(DevPromocao.StAprovado));
            Assert.False(DevPromocao.StatusValido("QualquerCoisa"));
        }

        // RH-DEV: promocao so aprova a partir de Pendente.
        [Fact]
        public void DEV_Promocao_So_Aprova_De_Pendente()
        {
            var p = new DevPromocao(Guid.NewGuid(), null, null, null, null, null, null, null, null, null,
                DevPromocao.StPendente, "tenant-1", "user-1");
            p.Aprovar("user-1");
            Assert.Equal(DevPromocao.StAprovado, p.Status);

            // ja aprovada: rejeitar deve falhar (notificacao).
            p.Rejeitar("user-1");
            Assert.False(p.IsValid);
        }

        // RH-LMS secao 16: data_fim >= data_inicio e hora_fim > hora_inicio.
        [Fact]
        public void LMS_Treinamento_Valida_Periodo_E_Horario()
        {
            var bom = new LmsTreinamento("Curso", null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                new DateTime(2026, 7, 1), new DateTime(2026, 7, 2), new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0),
                null, null, null, LmsTreinamento.StScheduled, Guid.NewGuid(), Guid.NewGuid(), "tenant-1", "user-1");
            bom.ValidarRegras();
            Assert.True(bom.IsValid);

            var ruim = new LmsTreinamento("Curso", null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                new DateTime(2026, 7, 5), new DateTime(2026, 7, 2), new TimeSpan(12, 0, 0), new TimeSpan(8, 0, 0),
                null, null, null, LmsTreinamento.StScheduled, Guid.NewGuid(), Guid.NewGuid(), "tenant-1", "user-1");
            ruim.ValidarRegras();
            Assert.False(ruim.IsValid);
        }

        // RH-LMS secao 19: nota de feedback entre 1 e 5.
        [Fact]
        public void LMS_Feedback_Nota_Dominio()
        {
            Assert.True(LmsFeedback.NotaValida(5));
            Assert.False(LmsFeedback.NotaValida(6));
            Assert.False(LmsFeedback.NotaValida(0));
        }

        // RH-TLT secao 20.3: data final da meta posterior a data inicial.
        [Fact]
        public void TLT_Meta_Data_Final_Posterior()
        {
            var ruim = new TltMetaColaborador(Guid.NewGuid(), null, "Meta", null,
                new DateTime(2026, 7, 10), new DateTime(2026, 7, 1), null, 10m,
                TltMetaColaborador.StNaoIniciada, null, Guid.NewGuid(), "tenant-1", "user-1");
            ruim.ValidarRegras();
            Assert.False(ruim.IsValid);
        }

        // RH-TLT secao 20.5: nota do indicador inteira entre 1 e 5.
        [Fact]
        public void TLT_Nota_Indicador_Dominio()
        {
            Assert.True(TltNotaIndicador.NotaValida(3));
            Assert.False(TltNotaIndicador.NotaValida(7));
        }

        // RH-TLT secao 18: media = media das notas maiores que zero, arredondada.
        [Fact]
        public void TLT_Avaliacao_Media_Ignora_Zeros()
        {
            var media = TltAvaliacaoColaborador.CalcularMedia(4m, 0m, 5m, 3m);
            Assert.Equal(4.00m, media);
        }

        // RH-TLT secao 20.7: licenca aceita apenas Pendente/Aprovada/Rejeitada; aprova so de pendente.
        [Fact]
        public void TLT_Licenca_Aprova_So_De_Pendente()
        {
            Assert.True(TltSolicitacaoLicenca.StatusValido(TltSolicitacaoLicenca.StPendente));
            Assert.False(TltSolicitacaoLicenca.StatusValido("XPTO"));

            var s = new TltSolicitacaoLicenca(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(2),
                2, null, null, TltSolicitacaoLicenca.StPendente, null, null, null, null, Guid.NewGuid(), "tenant-1", "user-1");
            s.Aprovar(Guid.NewGuid(), "ok", "user-1");
            Assert.Equal(TltSolicitacaoLicenca.StAprovada, s.Status);
            Assert.NotNull(s.AprovadoEm);
        }

        // RH-WFM secao 20.7/20.8: percentual maximo 100; tipo de cargo de dominio fechado.
        [Fact]
        public void WFM_Comissao_Valida_Percentual_E_TipoCargo()
        {
            var boa = new WfmComissaoColaborador(Guid.NewGuid(), WfmComissaoColaborador.CargoVendedor, 15m, true, "tenant-1", "user-1");
            boa.ValidarRegras();
            Assert.True(boa.IsValid);

            var acima = new WfmComissaoColaborador(Guid.NewGuid(), WfmComissaoColaborador.CargoVendedor, 150m, true, "tenant-1", "user-1");
            acima.ValidarRegras();
            Assert.False(acima.IsValid);

            var tipoRuim = new WfmComissaoColaborador(Guid.NewGuid(), "Diretor", 10m, true, "tenant-1", "user-1");
            tipoRuim.ValidarRegras();
            Assert.False(tipoRuim.IsValid);
        }

        // RH-WFM secao 20.9: demissao bloqueia folha normal.
        [Fact]
        public void WFM_Demissao_Bloqueia_Folha()
        {
            var c = new WfmColaborador(Guid.NewGuid(), null, "M001", null, null, DateTime.UtcNow, null, null, null, null,
                WfmColaborador.StAtivo, Guid.NewGuid(), Guid.NewGuid(), null, null, null, null, null, null, null, null, null, true,
                "tenant-1", "user-1");
            Assert.False(c.BloqueiaFolhaNormal());
            c.Demitir("user-1");
            Assert.Equal(WfmColaborador.StDemitido, c.Status);
            Assert.True(c.BloqueiaFolhaNormal());
            Assert.False(c.Ativo);
        }

        // RH-WFM secao 20.2: matricula unica por tenant.
        [Fact]
        public async Task WFM_Matricula_Unica_Por_Tenant()
        {
            using var ctx = NovoContext(nameof(WFM_Matricula_Unica_Por_Tenant));
            var handler = new AdmitirWfmColaboradorCommandHandler(ctx, new TP("tenant-1"), new CU("user-1"));
            var cmd = new AdmitirWfmColaboradorCommand(Guid.NewGuid(), "MAT-100", Guid.NewGuid(), Guid.NewGuid(), null, null, DateTime.UtcNow, null, null);

            var ok = await handler.Handle(cmd, CancellationToken.None);
            Assert.True(ok.Sucesso);

            var dup = new AdmitirWfmColaboradorCommand(Guid.NewGuid(), "MAT-100", Guid.NewGuid(), Guid.NewGuid(), null, null, DateTime.UtcNow, null, null);
            var negado = await handler.Handle(dup, CancellationToken.None);
            Assert.False(negado.Sucesso);
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
            public string? GetUserName() => "test_user";
            public string? GetUserEmail() => "test@epros.com.br";
        }
    }
}
