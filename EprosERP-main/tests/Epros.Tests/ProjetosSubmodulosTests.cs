using System;
using Epros.Modules.Projetos.Domain.Entities.Faturamento;
using Epros.Modules.Projetos.Domain.Entities.Orcamento;
using Epros.Modules.Projetos.Domain.Entities.Rastreamento;
using Epros.Modules.Projetos.Domain.Entities.Recursos;
using Epros.Modules.Projetos.Domain.Enums;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes de regras-chave dos submodulos PRJ-ORC, PRJ-REC, PRJ-RST e PRJ-FAT (nivel de dominio).
    /// </summary>
    public class ProjetosSubmodulosTests
    {
        private const string TenantId = "tenant-test-001";
        private const string UserId = "user-test-001";

        // ===================== PRJ-ORC =====================

        [Fact(DisplayName = "OrcamentoProjeto | budget >= 0 deve ser valido")]
        public void Orcamento_BudgetValido_DeveSerValido()
        {
            var orcamento = new OrcamentoProjeto(Guid.NewGuid(), 1000m, EBillingType.Fixed, null, null, null, TenantId, UserId);
            Assert.True(orcamento.IsValid);
        }

        [Fact(DisplayName = "OrcamentoProjeto | budget negativo deve ser invalido (RN-ORC-008)")]
        public void Orcamento_BudgetNegativo_DeveSerInvalido()
        {
            var orcamento = new OrcamentoProjeto(Guid.NewGuid(), -1m, null, null, null, null, TenantId, UserId);
            Assert.False(orcamento.IsValid);
        }

        [Fact(DisplayName = "OrcamentoProjeto | aprovar sem submeter deve falhar (RN-ORC workflow)")]
        public void Orcamento_AprovarSemSubmeter_DeveFalhar()
        {
            var orcamento = new OrcamentoProjeto(Guid.NewGuid(), 1000m, null, null, null, null, TenantId, UserId);
            orcamento.Aprovar(UserId);
            Assert.False(orcamento.IsValid);
        }

        [Fact(DisplayName = "OrcamentoProjeto | submeter e aprovar deve ativar")]
        public void Orcamento_SubmeterEAprovar_DeveAtivar()
        {
            var orcamento = new OrcamentoProjeto(Guid.NewGuid(), 1000m, null, null, null, null, TenantId, UserId);
            orcamento.Submeter(UserId);
            orcamento.Aprovar(UserId);
            Assert.True(orcamento.IsValid);
            Assert.Equal(EProjetoWorkflowStatus.Ativo, orcamento.Status);
        }

        [Fact(DisplayName = "MarcoOrcamentario | data fim anterior a inicio deve ser invalido (RN-ORC-005)")]
        public void Marco_DataFimAnterior_DeveSerInvalido()
        {
            var marco = new MarcoOrcamentario(Guid.NewGuid(), Guid.NewGuid(), "Marco 1", 100m,
                new DateTime(2026, 5, 10), new DateTime(2026, 5, 1), null, TenantId, UserId);
            Assert.False(marco.IsValid);
        }

        [Fact(DisplayName = "MarcoOrcamentario | concluir com progresso != 100 deve falhar (RN-ORC-012)")]
        public void Marco_CompleteComProgressoParcial_DeveFalhar()
        {
            var marco = new MarcoOrcamentario(Guid.NewGuid(), Guid.NewGuid(), "Marco 1", 100m,
                new DateTime(2026, 5, 1), new DateTime(2026, 5, 10), null, TenantId, UserId);
            marco.AtualizarProgresso(50, EMarcoStatus.Complete, UserId);
            Assert.False(marco.IsValid);
        }

        // ===================== PRJ-REC =====================

        [Fact(DisplayName = "RecursoTimesheet | horas > 12 deve ser invalido (RN-REC-002)")]
        public void Timesheet_HorasAcimaLimite_DeveSerInvalido()
        {
            var ts = new RecursoTimesheet(Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow, 13, 30, null, ETimesheetTipo.Manual, TenantId, UserId);
            Assert.False(ts.IsValid);
        }

        [Fact(DisplayName = "RecursoTimesheet | minutos 0 deve ser invalido (RN-REC-003)")]
        public void Timesheet_MinutosZero_DeveSerInvalido()
        {
            var ts = new RecursoTimesheet(Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow, 8, 0, null, ETimesheetTipo.Manual, TenantId, UserId);
            Assert.False(ts.IsValid);
        }

        [Fact(DisplayName = "RecursoTimesheet | tipo Project sem projeto deve ser invalido (RN-REC-005)")]
        public void Timesheet_ProjectSemProjeto_DeveSerInvalido()
        {
            var ts = new RecursoTimesheet(Guid.NewGuid(), null, null, DateTime.UtcNow, 8, 30, null, ETimesheetTipo.Project, TenantId, UserId);
            Assert.False(ts.IsValid);
        }

        [Fact(DisplayName = "RecursoTimesheet | dados validos deve ser valido")]
        public void Timesheet_DadosValidos_DeveSerValido()
        {
            var ts = new RecursoTimesheet(Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow, 8, 30, "ok", ETimesheetTipo.Project, TenantId, UserId);
            Assert.True(ts.IsValid);
        }

        // ===================== PRJ-RST =====================

        [Fact(DisplayName = "TarefaProjeto | titulo vazio deve ser invalido (PRJ-RST-RN-002)")]
        public void Tarefa_TituloVazio_DeveSerInvalido()
        {
            var tarefa = new TarefaProjeto(Guid.NewGuid(), "", null, null, null, null, null, null, null, null, null, false, null, 0, TenantId, UserId);
            Assert.False(tarefa.IsValid);
        }

        [Fact(DisplayName = "TarefaProjeto | atualizar progresso > 100 deve falhar (PRJ-RST-RN-009)")]
        public void Tarefa_ProgressoAcima100_DeveFalhar()
        {
            var tarefa = new TarefaProjeto(Guid.NewGuid(), "Tarefa", null, null, null, null, null, null, null, null, null, false, null, 0, TenantId, UserId);
            tarefa.AtualizarProgresso(150m, UserId);
            Assert.False(tarefa.IsValid);
        }

        [Fact(DisplayName = "TarefaProjeto | concluir com dependencia aberta deve falhar (PRJ-RST-RN-013)")]
        public void Tarefa_ConcluirComDependenciaAberta_DeveFalhar()
        {
            var tarefa = new TarefaProjeto(Guid.NewGuid(), "Tarefa", null, null, null, null, null, null, null, null, null, false, null, 0, TenantId, UserId);
            tarefa.Concluir(possuiDependenciaAberta: true, UserId);
            Assert.False(tarefa.IsValid);
        }

        [Fact(DisplayName = "TarefaProjeto | concluir sem dependencia deve concluir")]
        public void Tarefa_ConcluirSemDependencia_DeveConcluir()
        {
            var tarefa = new TarefaProjeto(Guid.NewGuid(), "Tarefa", null, null, null, null, null, null, null, null, null, false, null, 0, TenantId, UserId);
            tarefa.Concluir(possuiDependenciaAberta: false, UserId);
            Assert.True(tarefa.IsValid);
            Assert.Equal(ETarefaEstado.Concluida, tarefa.Estado);
            Assert.Equal(100m, tarefa.PercentualConcluido);
        }

        // ===================== PRJ-FAT =====================

        [Fact(DisplayName = "FaturamentoProjeto | codigo vazio deve ser invalido (RN-FAT-005)")]
        public void Faturamento_CodigoVazio_DeveSerInvalido()
        {
            var fat = new FaturamentoProjeto("", "Descricao", Guid.NewGuid(), Guid.NewGuid(), null, null, null, null, TenantId, UserId);
            Assert.False(fat.IsValid);
        }

        [Fact(DisplayName = "FaturamentoProjeto | submeter sem itens deve falhar (RN-FAT-005)")]
        public void Faturamento_SubmeterSemItens_DeveFalhar()
        {
            var fat = new FaturamentoProjeto("FAT-001", "Descricao", Guid.NewGuid(), Guid.NewGuid(), null, null, null, null, TenantId, UserId);
            fat.Submeter(UserId);
            Assert.False(fat.IsValid);
        }

        [Fact(DisplayName = "FaturamentoProjeto | fluxo item->submeter->aprovar habilita evento financeiro (RN-FAT-006/008)")]
        public void Faturamento_FluxoCompleto_HabilitaEvento()
        {
            var fat = new FaturamentoProjeto("FAT-002", "Descricao", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), EModalidadeFaturamento.Marco, "BRL", null, TenantId, UserId);
            fat.AdicionarItem(1, 1m, "linha", ETipoItemFaturamento.Marco, 500m, 500m, "marco", Guid.NewGuid(), UserId);
            Assert.True(fat.IsValid);
            Assert.Equal(500m, fat.ValorTotal);

            fat.Submeter(UserId);
            fat.Aprovar(UserId);
            Assert.True(fat.IsValid);
            Assert.Equal(EProjetoWorkflowStatus.Ativo, fat.Status);
            Assert.True(fat.PodePublicarEventoFinanceiro());
        }

        [Fact(DisplayName = "FaturamentoProjeto | aprovar sem submeter deve falhar (RN-FAT-006)")]
        public void Faturamento_AprovarSemSubmeter_DeveFalhar()
        {
            var fat = new FaturamentoProjeto("FAT-003", "Descricao", Guid.NewGuid(), Guid.NewGuid(), null, null, null, null, TenantId, UserId);
            fat.Aprovar(UserId);
            Assert.False(fat.IsValid);
            Assert.False(fat.PodePublicarEventoFinanceiro());
        }

        [Fact(DisplayName = "FaturamentoProjeto | rejeitar sem motivo deve falhar (RN-FAT-007)")]
        public void Faturamento_RejeitarSemMotivo_DeveFalhar()
        {
            var fat = new FaturamentoProjeto("FAT-004", "Descricao", Guid.NewGuid(), Guid.NewGuid(), null, null, null, null, TenantId, UserId);
            fat.AdicionarItem(1, 1m, null, ETipoItemFaturamento.Hora, 10m, 10m, null, null, UserId);
            fat.Submeter(UserId);
            fat.Rejeitar("", UserId);
            Assert.False(fat.IsValid);
        }
    }
}
