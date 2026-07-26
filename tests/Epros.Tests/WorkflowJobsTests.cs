using System;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Modules.Aplicativo.Domain.Entities.Workflow;
using Epros.Modules.Aplicativo.Domain.Enums;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Cobre as regras-chave da camada de agendamentos/jobs e do mascaramento LGPD do motor de Workflow
    /// (PLT-WF §7.4/§7.5/§7.7.3). Testes de domínio/serviço puros — sem infraestrutura.
    /// </summary>
    public class WorkflowJobsTests
    {
        private const string TenantId = "tenant-wf-jobs";
        private const string User = "user-sched";
        private readonly AgendaIntervalarService _agenda = new();

        // ---------- Expressão intervalar ----------

        [Fact(DisplayName = "Agenda | Expressão com 5 segmentos válidos deve ser aceita")]
        public void Expressao_CincoSegmentos_DeveSerValida()
        {
            Assert.True(_agenda.ExpressaoValida("0::9::*::*::1", out var erro));
            Assert.Null(erro);
        }

        [Fact(DisplayName = "Agenda | Expressão com número de segmentos errado deve falhar")]
        public void Expressao_SegmentosErrados_DeveFalhar()
        {
            Assert.False(_agenda.ExpressaoValida("0::9::*", out _));
        }

        [Fact(DisplayName = "Agenda | Segmento fora da faixa (hora 99) deve falhar")]
        public void Expressao_ForaDaFaixa_DeveFalhar()
        {
            Assert.False(_agenda.ExpressaoValida("0::99::*::*::*", out _));
        }

        [Fact(DisplayName = "Agenda | Próxima execução calcula o próximo minuto que casa a expressão")]
        public void ProximaExecucao_DeveCalcularProximoInstante()
        {
            // Toda hora no minuto 30. A partir de 10:00 → 10:30.
            var apos = new DateTime(2026, 7, 25, 10, 0, 0, DateTimeKind.Utc);
            var proxima = _agenda.ProximaExecucao("30::*::*::*::*", apos);
            Assert.Equal(new DateTime(2026, 7, 25, 10, 30, 0, DateTimeKind.Utc), proxima);
        }

        [Fact(DisplayName = "Agenda | Próxima execução avança de dia quando o horário já passou")]
        public void ProximaExecucao_DeveAvancarDeDia()
        {
            // Todo dia às 09:00. A partir de 25/07 10:00 → 26/07 09:00.
            var apos = new DateTime(2026, 7, 25, 10, 0, 0, DateTimeKind.Utc);
            var proxima = _agenda.ProximaExecucao("0::9::*::*::*", apos);
            Assert.Equal(new DateTime(2026, 7, 26, 9, 0, 0, DateTimeKind.Utc), proxima);
        }

        // ---------- Job: política de retry (domínio) ----------

        [Fact(DisplayName = "WfJob | Iniciar incrementa a tentativa e vai para EmExecucao")]
        public void WfJob_Iniciar_IncrementaTentativa()
        {
            var job = new WfJob(Guid.NewGuid(), "job-x", DateTime.UtcNow, null, TenantId, User);
            job.Iniciar(User);
            Assert.Equal(EWfJobStatus.EmExecucao, job.Status);
            Assert.Equal(1, job.TentativaAtual);
        }

        [Fact(DisplayName = "WfJob | Adiar reprograma a previsão e marca Adiado")]
        public void WfJob_Adiar_DeveReprogramar()
        {
            var job = new WfJob(Guid.NewGuid(), "job-x", DateTime.UtcNow, null, TenantId, User);
            var nova = DateTime.UtcNow.AddMinutes(10);
            job.Adiar(nova, User);
            Assert.Equal(EWfJobStatus.Adiado, job.Status);
            Assert.Equal(nova, job.PrevistoPara);
        }

        [Fact(DisplayName = "WfJob | FalhaFinal encerra o job registrando log")]
        public void WfJob_FalhaFinal_DeveEncerrar()
        {
            var job = new WfJob(Guid.NewGuid(), "job-x", DateTime.UtcNow, null, TenantId, User);
            job.FalhaFinal("erro fatal", User);
            Assert.Equal(EWfJobStatus.FalhaFinal, job.Status);
            Assert.NotNull(job.FinalizadoEm);
        }

        [Fact(DisplayName = "WfJob | Agenda vazia deve invalidar o job")]
        public void WfJob_AgendaVazia_DeveSerInvalido()
        {
            var job = new WfJob(Guid.Empty, "job-x", DateTime.UtcNow, null, TenantId, User);
            Assert.False(job.IsValid);
        }

        [Fact(DisplayName = "WfJobTentativa | Número de tentativa zero deve ser inválido")]
        public void WfJobTentativa_NumeroZero_DeveSerInvalido()
        {
            var t = new WfJobTentativa(Guid.NewGuid(), 0, EWfJobTentativaStatus.Falha, "x", null, null, TenantId, User);
            Assert.False(t.IsValid);
        }

        // ---------- Mascaramento LGPD ----------

        [Fact(DisplayName = "Masker | Campos sensíveis devem ser mascarados no JSON")]
        public void Masker_DeveMascararCamposSensiveis()
        {
            var json = "{\"cpf\":\"12345678901\",\"nome\":\"Fulano\",\"senha\":\"abc\"}";
            var mascarado = DadoSensivelMasker.MascararJson(json);
            Assert.NotNull(mascarado);
            Assert.DoesNotContain("12345678901", mascarado);
            Assert.DoesNotContain("abc", mascarado);
            Assert.Contains("Fulano", mascarado); // campo não sensível preservado
        }

        [Fact(DisplayName = "Masker | JSON não estruturado retorna máscara total por precaução")]
        public void Masker_JsonInvalido_DeveMascararTudo()
        {
            var mascarado = DadoSensivelMasker.MascararJson("texto-solto-nao-json");
            Assert.Equal("\"***\"", mascarado);
        }
    }
}
