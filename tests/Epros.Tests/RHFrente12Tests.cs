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
    // RH Frentes 1-2: RH-FP, RH-PNT, RH-SSO, RH-REC. Regras-chave das EFs.
    public class RHFrente12Tests
    {
        private static ContextRH NovoContext(string db)
        {
            var options = new DbContextOptionsBuilder<ContextRH>().UseInMemoryDatabase(db).Options;
            return new ContextRH(options, new TP("tenant-1"), new CU("user-1"));
        }

        // RF-FOL-004: rubrica deve ter exatamente um tipo valido.
        [Fact]
        public async Task RHFP_Rubrica_Deve_Rejeitar_Tipo_Invalido()
        {
            using var ctx = NovoContext(nameof(RHFP_Rubrica_Deve_Rejeitar_Tipo_Invalido));
            var handler = new CriarRubricaCommandHandler(ctx, new TP("tenant-1"), new CU("user-1"));

            var okCmd = new CriarRubricaCommand(Guid.NewGuid(), "001", "Salario", "Salario base", "Provento", "Horas", "Fixo", null, true);
            var badCmd = new CriarRubricaCommand(Guid.NewGuid(), "002", "Estranha", "X", "Outro", "Horas", "Fixo", null, true);

            var ok = await handler.Handle(okCmd, CancellationToken.None);
            var bad = await handler.Handle(badCmd, CancellationToken.None);

            Assert.True(ok.Sucesso);
            Assert.False(bad.Sucesso);
        }

        // Estados da competencia: fechar so a partir de Concluido.
        [Fact]
        public void RHFP_Competencia_So_Fecha_De_Concluido()
        {
            var comp = new FolCompetencia(Guid.NewGuid(), "2026-07", "Mensal", null, null, null, null, null, null,
                FolCompetencia.StRascunho, null, null, "tenant-1", "user-1");

            comp.Fechar("user-1"); // invalido: ainda em Rascunho
            Assert.False(comp.IsValid);

            var comp2 = new FolCompetencia(Guid.NewGuid(), "2026-08", "Mensal", null, null, null, null, null, null,
                FolCompetencia.StRascunho, null, null, "tenant-1", "user-1");
            comp2.IniciarProcessamento("user-1");
            comp2.Concluir("user-1");
            comp2.Fechar("user-1");
            Assert.Equal(FolCompetencia.StFechado, comp2.Status);
        }

        // PNT-012: exportacao para folha exige periodo Fechado.
        [Fact]
        public void RHPNT_Periodo_Exporta_Somente_Fechado()
        {
            var p = new PntPeriodoApuracao(Guid.NewGuid(), "2026-07", DateTime.UtcNow, DateTime.UtcNow.AddDays(30),
                PntPeriodoApuracao.StAberto, null, null, "tenant-1", "user-1");

            p.Exportar("user-1"); // invalido: aberto
            Assert.False(p.IsValid);

            var p2 = new PntPeriodoApuracao(Guid.NewGuid(), "2026-08", DateTime.UtcNow, DateTime.UtcNow.AddDays(30),
                PntPeriodoApuracao.StAberto, null, null, "tenant-1", "user-1");
            p2.Fechar("user-1");
            p2.Exportar("user-1");
            Assert.Equal(PntPeriodoApuracao.StExportado, p2.Status);
            Assert.NotNull(p2.ExportadoEm);
        }

        // SSO-REG-003: observacao do PPP obrigatoria.
        [Fact]
        public async Task RHSSO_Ppp_Exige_Observacao()
        {
            using var ctx = NovoContext(nameof(RHSSO_Ppp_Exige_Observacao));
            var handler = new CriarPppCommandHandler(ctx, new TP("tenant-1"), new CU("user-1"));

            var bad = await handler.Handle(new CriarPppCommand(Guid.NewGuid(), ""), CancellationToken.None);
            var ok = await handler.Handle(new CriarPppCommand(Guid.NewGuid(), "Exposicao a ruido"), CancellationToken.None);

            Assert.False(bad.Sucesso);
            Assert.True(ok.Sucesso);
        }

        // REC-REG-037: nota geral do feedback e a media das tres notas.
        [Fact]
        public void RHREC_Feedback_Media_Correta()
        {
            var media = RecFeedbackEntrevista.CalcularNotaGeral(9m, 6m, 6m);
            Assert.Equal(7.00m, media);
        }

        // REC-REG-019: publicar sincroniza status, indicador e data.
        [Fact]
        public void RHREC_Publicar_Vaga_Sincroniza_Estado()
        {
            var vaga = new RecVaga("JOB0001", "JP20260001", "Dev", 1, 0, null, null, null, null, "Desc", null,
                "[\"csharp\"]", null, null, false, null, false, null, false, RecVaga.StRascunho, "existing", null,
                null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(),
                "tenant-1", "user-1");

            Assert.False(vaga.Publicada);
            vaga.Publicar("user-1");
            Assert.True(vaga.Publicada);
            Assert.Equal(RecVaga.StAtiva, vaga.Status);
            Assert.NotNull(vaga.DataPublicacao);
        }

        // REC-REG-020: candidatura exige vaga publicada e ativa.
        [Fact]
        public async Task RHREC_Candidatura_Exige_Vaga_Publicada()
        {
            using var ctx = NovoContext(nameof(RHREC_Candidatura_Exige_Vaga_Publicada));
            var vaga = new RecVaga("JOB0001", "JP20260001", "Dev", 1, 0, null, null, null, null, "Desc", null,
                "[\"csharp\"]", null, null, false, null, false, null, false, RecVaga.StRascunho, "existing", null,
                null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(),
                "tenant-1", "user-1");
            ctx.RecVagas.Add(vaga);
            await ctx.SaveChangesAsync();

            var handler = new RegistrarCandidaturaCommandHandler(ctx, new TP("tenant-1"), new CU("user-1"));
            var cmd = new RegistrarCandidaturaCommand("Ana", "Silva", "ana@ex.com", 3m, vaga.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            var negado = await handler.Handle(cmd, CancellationToken.None);
            Assert.False(negado.Sucesso); // vaga em rascunho

            vaga.Publicar("user-1");
            await ctx.SaveChangesAsync();
            var ok = await handler.Handle(cmd, CancellationToken.None);
            Assert.True(ok.Sucesso);
        }

        // REC-REG-027: status de candidato aceita apenas 0..5.
        [Fact]
        public void RHREC_Candidato_Status_Dominio()
        {
            Assert.True(RecCandidato.StatusValido("4"));
            Assert.False(RecCandidato.StatusValido("9"));
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
