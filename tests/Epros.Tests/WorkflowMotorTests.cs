using System;
using System.Linq;
using Epros.Modules.Aplicativo.Domain.Entities.Workflow;
using Epros.Modules.Aplicativo.Domain.Enums;
using Xunit;

namespace Epros.Tests
{
    public class WorkflowMotorTests
    {
        private const string TenantId = "tenant-wf-001";
        private const string Maker = "user-maker";
        private const string Checker = "user-checker";

        private static WfInstancia NovaInstancia(string criadoPor)
            => new WfInstancia(Guid.NewGuid(), "Financeiro", "ContaAPagar", Guid.NewGuid().ToString(), "Pagamento acima da alçada", null, null, 15000m, null, null, TenantId, criadoPor);

        [Fact(DisplayName = "WfDefinicao | Dados válidos deve ser válida")]
        public void WfDefinicao_DadosValidos_DeveSerValida()
        {
            var def = new WfDefinicao("Financeiro", "ContaAPagar", "Aprovação de pagamento", 1, null, TenantId, Maker);
            Assert.True(def.IsValid);
            Assert.Equal(EWfDefinicaoStatus.Rascunho, def.Status);
        }

        [Fact(DisplayName = "WfDefinicao | Módulo vazio deve ser inválida")]
        public void WfDefinicao_ModuloVazio_DeveSerInvalida()
        {
            var def = new WfDefinicao("", "ContaAPagar", "Aprovação", 1, null, TenantId, Maker);
            Assert.False(def.IsValid);
        }

        [Fact(DisplayName = "WfInstancia | Nova instância deve iniciar em Rascunho")]
        public void WfInstancia_Nova_DeveIniciarRascunho()
        {
            var inst = NovaInstancia(Maker);
            Assert.True(inst.IsValid);
            Assert.Equal(EWfInstanciaStatus.Rascunho, inst.Status);
        }

        [Fact(DisplayName = "WfInstancia | Submeter a partir de Rascunho deve ir para EmAnalise")]
        public void WfInstancia_Submeter_DeveIrParaEmAnalise()
        {
            var inst = NovaInstancia(Maker);
            inst.Submeter(null, Maker);
            Assert.True(inst.IsValid);
            Assert.Equal(EWfInstanciaStatus.EmAnalise, inst.Status);
        }

        [Fact(DisplayName = "WfInstancia | Aprovar por usuário diferente do criador deve ir para Ativo")]
        public void WfInstancia_AprovarPorChecker_DeveIrParaAtivo()
        {
            var inst = NovaInstancia(Maker);
            inst.Submeter(null, Maker);
            inst.Aprovar(null, null, Checker, "ok", Checker);
            Assert.True(inst.IsValid);
            Assert.Equal(EWfInstanciaStatus.Ativo, inst.Status);
            Assert.Equal(Checker, inst.AprovadoPor);
        }

        [Fact(DisplayName = "WfInstancia | Aprovar pelo próprio criador deve falhar (segregação)")]
        public void WfInstancia_AprovarPeloCriador_DeveFalhar()
        {
            var inst = NovaInstancia(Maker);
            inst.Submeter(null, Maker);
            inst.Aprovar(null, null, Maker, "ok", Maker);
            Assert.False(inst.IsValid);
            Assert.Equal(EWfInstanciaStatus.EmAnalise, inst.Status);
        }

        [Fact(DisplayName = "WfInstancia | Aprovar a partir de Rascunho deve falhar")]
        public void WfInstancia_AprovarSemSubmeter_DeveFalhar()
        {
            var inst = NovaInstancia(Maker);
            inst.Aprovar(null, null, Checker, null, Checker);
            Assert.False(inst.IsValid);
        }

        [Fact(DisplayName = "WfInstancia | Rejeitar deve ir para Rejeitado")]
        public void WfInstancia_Rejeitar_DeveIrParaRejeitado()
        {
            var inst = NovaInstancia(Maker);
            inst.Submeter(null, Maker);
            inst.Rejeitar(null, null, Checker, "faltou anexo", Checker);
            Assert.True(inst.IsValid);
            Assert.Equal(EWfInstanciaStatus.Rejeitado, inst.Status);
        }

        [Fact(DisplayName = "WfInstancia | Reativar a partir de Ativo deve falhar")]
        public void WfInstancia_ReativarAtivo_DeveFalhar()
        {
            var inst = NovaInstancia(Maker);
            inst.Submeter(null, Maker);
            inst.Aprovar(null, null, Checker, null, Checker);
            inst.Reativar(null, Checker);
            Assert.False(inst.IsValid);
        }

        [Fact(DisplayName = "WfTarefa | Concluir deve marcar Concluida com data")]
        public void WfTarefa_Concluir_DeveMarcarConcluida()
        {
            var tarefa = new WfTarefa(Guid.NewGuid(), "Revisar contrato", null, EWfPermissao.Aprovador, null, TenantId, Maker);
            tarefa.Concluir(Checker);
            Assert.True(tarefa.IsValid);
            Assert.Equal(EWfTarefaStatus.Concluida, tarefa.Status);
            Assert.NotNull(tarefa.ConcluidaEm);
        }

        [Fact(DisplayName = "WfSolicitacao | Data final anterior à inicial deve ser inválida")]
        public void WfSolicitacao_DataInvertida_DeveSerInvalida()
        {
            var sol = new WfSolicitacao(new DateTime(2026, 5, 10), new DateTime(2026, 5, 1), 5m, "férias", null, null, null, null, TenantId, Maker);
            Assert.False(sol.IsValid);
        }

        [Fact(DisplayName = "WfAgendamento | Expressão intervalar sem 5 segmentos deve ser inválida")]
        public void WfAgendamento_ExpressaoInvalida_DeveSerInvalida()
        {
            var ag = new WfAgendamento("Fechamento diário", "0::2::*", true, null, TenantId, Maker);
            Assert.False(ag.IsValid);
        }

        [Fact(DisplayName = "WfAgendamento | Expressão intervalar com 5 segmentos deve ser válida")]
        public void WfAgendamento_ExpressaoValida_DeveSerValida()
        {
            var ag = new WfAgendamento("Fechamento diário", "0::2::*::*::*", true, null, TenantId, Maker);
            Assert.True(ag.IsValid);
        }
    }
}
