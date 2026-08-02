using System;
using System.Collections.Generic;
using Epros.Modules.Projetos.Domain.Entities.Encerramento;
using Epros.Modules.Projetos.Domain.Entities.Portfolio;
using Epros.Modules.Projetos.Domain.Entities.Risco;
using Epros.Modules.Projetos.Domain.Enums;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes de regras-chave dos submodulos PRJ-ENC, PRJ-RSK e PRJ-PRT (nivel de dominio).
    /// </summary>
    public class ProjetosSubmodulosAvancadosTests
    {
        private const string TenantId = "tenant-test-001";
        private const string UserId = "user-test-001";
        private static readonly Guid Usuario = Guid.NewGuid();

        // ===================== PRJ-ENC =====================

        [Fact(DisplayName = "EncerramentoProjeto | codigo vazio deve ser invalido (RN-ENC-002)")]
        public void Encerramento_CodigoVazio_DeveSerInvalido()
        {
            var enc = new EncerramentoProjeto(Guid.NewGuid(), "", "Descricao", Guid.NewGuid(), TenantId, UserId);
            Assert.False(enc.IsValid);
        }

        [Fact(DisplayName = "EncerramentoProjeto | sem responsavel deve ser invalido (RN-ENC-004)")]
        public void Encerramento_SemResponsavel_DeveSerInvalido()
        {
            var enc = new EncerramentoProjeto(Guid.NewGuid(), "ENC-001", "Descricao", Guid.Empty, TenantId, UserId);
            Assert.False(enc.IsValid);
        }

        [Fact(DisplayName = "EncerramentoProjeto | aprovar sem submeter deve falhar (workflow)")]
        public void Encerramento_AprovarSemSubmeter_DeveFalhar()
        {
            var enc = new EncerramentoProjeto(Guid.NewGuid(), "ENC-002", "Descricao", Guid.NewGuid(), TenantId, UserId);
            enc.Aprovar(EStatusFinalProjeto.Concluido, Usuario, UserId);
            Assert.False(enc.IsValid);
        }

        [Fact(DisplayName = "EncerramentoProjeto | submeter e aprovar deve ativar e aplicar status final")]
        public void Encerramento_SubmeterEAprovar_DeveAtivar()
        {
            var enc = new EncerramentoProjeto(Guid.NewGuid(), "ENC-003", "Descricao", Guid.NewGuid(), TenantId, UserId);
            enc.Submeter(Usuario, UserId);
            enc.Aprovar(EStatusFinalProjeto.Concluido, Usuario, UserId);
            Assert.True(enc.IsValid);
            Assert.Equal(EProjetoWorkflowStatus.Ativo, enc.Status);
            Assert.Equal(EStatusFinalProjeto.Concluido, enc.StatusFinalProjeto);
        }

        [Fact(DisplayName = "EncerramentoProjeto | rejeitar sem motivo deve falhar (secao 8)")]
        public void Encerramento_RejeitarSemMotivo_DeveFalhar()
        {
            var enc = new EncerramentoProjeto(Guid.NewGuid(), "ENC-004", "Descricao", Guid.NewGuid(), TenantId, UserId);
            enc.Submeter(Usuario, UserId);
            enc.Rejeitar("", Usuario, UserId);
            Assert.False(enc.IsValid);
        }

        [Fact(DisplayName = "EncerramentoProjeto | arquivar apos ativo deve marcar Arquivado (RN-ENC-008)")]
        public void Encerramento_Arquivar_DeveMarcarArquivado()
        {
            var enc = new EncerramentoProjeto(Guid.NewGuid(), "ENC-005", "Descricao", Guid.NewGuid(), TenantId, UserId);
            enc.Submeter(Usuario, UserId);
            enc.Aprovar(EStatusFinalProjeto.Concluido, Usuario, UserId);
            enc.Arquivar(Usuario, UserId);
            Assert.True(enc.IsValid);
            Assert.Equal(EStatusFinalProjeto.Arquivado, enc.StatusFinalProjeto);
        }

        // ===================== PRJ-RSK =====================

        [Fact(DisplayName = "RiscoProjeto | titulo vazio deve ser invalido (RN-RSK-002)")]
        public void Risco_TituloVazio_DeveSerInvalido()
        {
            var risco = new RiscoProjeto(Guid.NewGuid(), "", EPrioridadeRisco.Medium, "desc", Guid.NewGuid(),
                new List<Guid> { Guid.NewGuid() }, null, TenantId, UserId);
            Assert.False(risco.IsValid);
        }

        [Fact(DisplayName = "RiscoProjeto | sem responsavel deve ser invalido (RN-RSK-004)")]
        public void Risco_SemResponsavel_DeveSerInvalido()
        {
            var risco = new RiscoProjeto(Guid.NewGuid(), "Titulo", EPrioridadeRisco.High, "desc", Guid.NewGuid(),
                new List<Guid>(), null, TenantId, UserId);
            Assert.False(risco.IsValid);
        }

        [Fact(DisplayName = "RiscoProjeto | estagio vazio deve ser invalido (RN-RSK-005)")]
        public void Risco_EstagioVazio_DeveSerInvalido()
        {
            var risco = new RiscoProjeto(Guid.NewGuid(), "Titulo", EPrioridadeRisco.High, "desc", Guid.Empty,
                new List<Guid> { Guid.NewGuid() }, null, TenantId, UserId);
            Assert.False(risco.IsValid);
        }

        [Fact(DisplayName = "RiscoProjeto | dados validos deve ser valido")]
        public void Risco_DadosValidos_DeveSerValido()
        {
            var risco = new RiscoProjeto(Guid.NewGuid(), "Titulo", EPrioridadeRisco.High, "desc", Guid.NewGuid(),
                new List<Guid> { Guid.NewGuid() }, null, TenantId, UserId);
            Assert.True(risco.IsValid);
            Assert.Single(risco.Responsaveis);
        }

        [Fact(DisplayName = "RiscoProjeto | comentario vazio deve falhar (RN-RSK-011)")]
        public void Risco_ComentarioVazio_DeveFalhar()
        {
            var risco = new RiscoProjeto(Guid.NewGuid(), "Titulo", EPrioridadeRisco.High, "desc", Guid.NewGuid(),
                new List<Guid> { Guid.NewGuid() }, null, TenantId, UserId);
            risco.Comentar(Guid.NewGuid(), "", UserId);
            Assert.False(risco.IsValid);
        }

        [Fact(DisplayName = "EstagioRisco | cor com mais de 7 caracteres deve ser invalido (RN-RSK-009)")]
        public void Estagio_CorInvalida_DeveSerInvalido()
        {
            var estagio = new EstagioRisco("Backlog", "#FFFFFFF", false, 1, null, TenantId, UserId);
            Assert.False(estagio.IsValid);
        }

        [Fact(DisplayName = "EstagioRisco | dados validos deve ser valido")]
        public void Estagio_DadosValidos_DeveSerValido()
        {
            var estagio = new EstagioRisco("Backlog", "#FFF", false, 1, null, TenantId, UserId);
            Assert.True(estagio.IsValid);
        }

        // ===================== PRJ-PRT =====================

        [Fact(DisplayName = "Portfolio | codigo vazio deve ser invalido (PRJ-PRT-RN-002)")]
        public void Portfolio_CodigoVazio_DeveSerInvalido()
        {
            var pf = new Portfolio("", "Descricao", Guid.NewGuid(), null, null, TenantId, UserId);
            Assert.False(pf.IsValid);
        }

        [Fact(DisplayName = "Portfolio | nasce em Rascunho (PRJ-PRT-RN-005)")]
        public void Portfolio_NasceEmRascunho()
        {
            var pf = new Portfolio("PRT-001", "Descricao", Guid.NewGuid(), null, null, TenantId, UserId);
            Assert.True(pf.IsValid);
            Assert.Equal(EProjetoWorkflowStatus.Rascunho, pf.Status);
        }

        [Fact(DisplayName = "Portfolio | aprovar sem submeter deve falhar (PRJ-PRT-RN-007)")]
        public void Portfolio_AprovarSemSubmeter_DeveFalhar()
        {
            var pf = new Portfolio("PRT-002", "Descricao", Guid.NewGuid(), null, null, TenantId, UserId);
            pf.Aprovar(Usuario, UserId);
            Assert.False(pf.IsValid);
            Assert.False(pf.PodePublicarEventoExecutivo());
        }

        [Fact(DisplayName = "Portfolio | submeter e aprovar ativa e habilita evento executivo (PRJ-PRT-RN-007/018)")]
        public void Portfolio_SubmeterEAprovar_HabilitaEvento()
        {
            var pf = new Portfolio("PRT-003", "Descricao", Guid.NewGuid(), null, null, TenantId, UserId);
            pf.Submeter(Usuario, UserId);
            pf.Aprovar(Usuario, UserId);
            Assert.True(pf.IsValid);
            Assert.Equal(EProjetoWorkflowStatus.Ativo, pf.Status);
            Assert.True(pf.PodePublicarEventoExecutivo());
        }

        [Fact(DisplayName = "Portfolio | rejeitar sem motivo deve falhar (PRJ-PRT-RN-008)")]
        public void Portfolio_RejeitarSemMotivo_DeveFalhar()
        {
            var pf = new Portfolio("PRT-004", "Descricao", Guid.NewGuid(), null, null, TenantId, UserId);
            pf.Submeter(Usuario, UserId);
            pf.Rejeitar("", Usuario, UserId);
            Assert.False(pf.IsValid);
        }

        [Fact(DisplayName = "Portfolio | priorizar manual sem justificativa deve falhar (PRJ-PRT-RN-016)")]
        public void Portfolio_PriorizarSemJustificativa_DeveFalhar()
        {
            var pf = new Portfolio("PRT-005", "Descricao", Guid.NewGuid(), null, null, TenantId, UserId);
            pf.PriorizarManual(10m, "", Usuario, UserId);
            Assert.False(pf.IsValid);
        }

        [Fact(DisplayName = "PortfolioItem | sequencia zero deve ser invalido (PRJ-PRT-RN-014)")]
        public void PortfolioItem_SequenciaZero_DeveSerInvalido()
        {
            var pf = new Portfolio("PRT-006", "Descricao", Guid.NewGuid(), null, null, TenantId, UserId);
            pf.AdicionarItem(0, "projeto", Guid.NewGuid(), null, null, null, null, null, null, null, null, null, null, null, null, UserId);
            Assert.False(pf.IsValid);
        }

        [Fact(DisplayName = "PortfolioItem | dados validos adiciona item")]
        public void PortfolioItem_DadosValidos_Adiciona()
        {
            var pf = new Portfolio("PRT-007", "Descricao", Guid.NewGuid(), null, null, TenantId, UserId);
            pf.AdicionarItem(1, "projeto", Guid.NewGuid(), null, null, 1000m, 40m, null, null, null, null, null, null, null, "obs", UserId);
            Assert.True(pf.IsValid);
            Assert.Single(pf.Itens);
        }

        [Fact(DisplayName = "PortfolioItem | score = media ponderada (beneficios somam, custos penalizam) (DP-PRT-score)")]
        public void PortfolioItem_CalcularScore_MediaPonderada()
        {
            // npv=80, alinhamento=60 (beneficios); payback=20, risco=40 (custos). Pesos iguais (1).
            // score = (80 + 60 - 20 - 40) / 4 = 20
            var item = new PortfolioItem(Guid.NewGuid(), 1, "projeto", null, null, null, null, null, null,
                npv: 80m, payback: 20m, alinhamentoEstrategico: 60m, risco: 40m, score: null,
                justificativaPrioridade: null, observacao: null, TenantId, UserId);
            var score = item.CalcularScore(PesosPortfolio.Padrao);
            Assert.Equal(20m, score);
            Assert.Equal(20m, item.Score);
        }

        [Fact(DisplayName = "Portfolio | RecalcularScores consolida ScoreTotal dos itens ativos (DP-PRT-score)")]
        public void Portfolio_RecalcularScores_ConsolidaTotal()
        {
            var pf = new Portfolio("PRT-SC", "Descricao", Guid.NewGuid(), null, null, TenantId, UserId);
            pf.AdicionarItem(1, "projeto", Guid.NewGuid(), null, null, null, null, null, 100m, 0m, 0m, 0m, null, null, null, UserId); // score 25
            pf.AdicionarItem(2, "projeto", Guid.NewGuid(), null, null, null, null, null, 40m, 0m, 40m, 0m, null, null, null, UserId);  // score 20
            pf.RecalcularScores(PesosPortfolio.Padrao, UserId);
            Assert.True(pf.IsValid);
            Assert.Equal(45m, pf.ScoreTotal); // 25 + 20
        }

        [Fact(DisplayName = "Programa | codigo vazio deve ser invalido (T-PRG)")]
        public void Programa_CodigoVazio_DeveSerInvalido()
        {
            var prog = new Programa("", "Nome", null, Guid.NewGuid(), null, TenantId, UserId);
            Assert.False(prog.IsValid);
        }

        [Fact(DisplayName = "Programa | dados validos nasce em Rascunho e ativa (T-PRG)")]
        public void Programa_DadosValidos_Ativa()
        {
            var prog = new Programa("PRG-001", "Programa Transformacao", "desc", Guid.NewGuid(), Guid.NewGuid(), TenantId, UserId);
            Assert.True(prog.IsValid);
            Assert.Equal(EProjetoWorkflowStatus.Rascunho, prog.Status);
            prog.Ativar(UserId);
            Assert.Equal(EProjetoWorkflowStatus.Ativo, prog.Status);
        }
    }
}
