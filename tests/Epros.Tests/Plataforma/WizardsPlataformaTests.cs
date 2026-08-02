using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Plataforma.Wizards;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests.Plataforma
{
    /// <summary>PLT · Wizards — builder (etapas/campos), execução multi-etapa, sanitização, conclusão.</summary>
    public class WizardsPlataformaTests
    {
        private static ContextAplicativo Novo(string db)
        {
            var options = new DbContextOptionsBuilder<ContextAplicativo>().UseInMemoryDatabase(db).Options;
            return new ContextAplicativo(options, T(), U());
        }

        private static ITenantProvider T() => new PlataformaTestFixtures.TestTenantProvider("tenant-1");
        private static ICurrentUser U() => new PlataformaTestFixtures.TestCurrentUser("user-1");

        private static async Task<(Guid defId, Guid etapa1)> MontarWizardUmaEtapa(ContextAplicativo ctx, bool publico = false)
        {
            var dr = await new CriarDefinicaoWizardCommandHandler(ctx, T(), U()).Handle(
                new CriarDefinicaoWizardCommand("W1", "Cadastro", null, publico), CancellationToken.None);
            var defId = await ctx.DefinicoesWizard.Select(d => d.Id).FirstAsync();
            await new AdicionarEtapaWizardCommandHandler(ctx, T(), U()).Handle(
                new AdicionarEtapaWizardCommand(defId, 1, "Dados", null), CancellationToken.None);
            var etapa1 = await ctx.EtapasWizard.Select(e => e.Id).FirstAsync();
            await new AdicionarCampoWizardCommandHandler(ctx, T(), U()).Handle(
                new AdicionarCampoWizardCommand(etapa1, "nome", "Nome", "texto", true, null, 1), CancellationToken.None);
            return (defId, etapa1);
        }

        [Fact]
        public async Task Campo_Rejeita_Tipo_Fora_Do_Catalogo()
        {
            using var ctx = Novo(nameof(Campo_Rejeita_Tipo_Fora_Do_Catalogo));
            var (_, etapa1) = await MontarWizardUmaEtapa(ctx);
            var r = await new AdicionarCampoWizardCommandHandler(ctx, T(), U()).Handle(
                new AdicionarCampoWizardCommand(etapa1, "x", "X", "tipo_invalido", false, null, 2), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task Nao_Publica_Wizard_Sem_Etapa()
        {
            using var ctx = Novo(nameof(Nao_Publica_Wizard_Sem_Etapa));
            await new CriarDefinicaoWizardCommandHandler(ctx, T(), U()).Handle(
                new CriarDefinicaoWizardCommand("W0", "Vazio", null, false), CancellationToken.None);
            var defId = await ctx.DefinicoesWizard.Select(d => d.Id).FirstAsync();
            var r = await new PublicarDefinicaoWizardCommandHandler(ctx, T(), U()).Handle(
                new PublicarDefinicaoWizardCommand(defId), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task Nao_Inicia_Execucao_De_Wizard_Nao_Publicado()
        {
            using var ctx = Novo(nameof(Nao_Inicia_Execucao_De_Wizard_Nao_Publicado));
            var (defId, _) = await MontarWizardUmaEtapa(ctx);
            var r = await new IniciarExecucaoWizardCommandHandler(ctx, T(), U()).Handle(
                new IniciarExecucaoWizardCommand(defId, false), CancellationToken.None);
            Assert.False(r.Sucesso); // ainda inativo
        }

        [Fact]
        public async Task Responder_Sem_Campo_Obrigatorio_Falha()
        {
            using var ctx = Novo(nameof(Responder_Sem_Campo_Obrigatorio_Falha));
            var (defId, _) = await MontarWizardUmaEtapa(ctx);
            await new PublicarDefinicaoWizardCommandHandler(ctx, T(), U()).Handle(new PublicarDefinicaoWizardCommand(defId), CancellationToken.None);
            await new IniciarExecucaoWizardCommandHandler(ctx, T(), U()).Handle(new IniciarExecucaoWizardCommand(defId, false), CancellationToken.None);
            var execId = await ctx.ExecucoesWizard.Select(e => e.Id).FirstAsync();

            var r = await new ResponderEtapaWizardCommandHandler(ctx, T(), U()).Handle(
                new ResponderEtapaWizardCommand(execId, new Dictionary<string, string>()), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task Responder_Ultima_Etapa_Conclui_E_Sanitiza_Html()
        {
            using var ctx = Novo(nameof(Responder_Ultima_Etapa_Conclui_E_Sanitiza_Html));
            var (defId, _) = await MontarWizardUmaEtapa(ctx);
            await new PublicarDefinicaoWizardCommandHandler(ctx, T(), U()).Handle(new PublicarDefinicaoWizardCommand(defId), CancellationToken.None);
            await new IniciarExecucaoWizardCommandHandler(ctx, T(), U()).Handle(new IniciarExecucaoWizardCommand(defId, false), CancellationToken.None);
            var execId = await ctx.ExecucoesWizard.Select(e => e.Id).FirstAsync();

            var r = await new ResponderEtapaWizardCommandHandler(ctx, T(), U()).Handle(
                new ResponderEtapaWizardCommand(execId, new Dictionary<string, string>
                {
                    ["nome"] = "<script>alert(1)</script>João"
                }), CancellationToken.None);

            Assert.True(r.Sucesso);
            var exec = await ctx.ExecucoesWizard.FirstAsync();
            Assert.Equal("Concluida", exec.Status);
            var respostas = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(exec.RespostasJson)!;
            Assert.Equal("alert(1)João", respostas["nome"]); // tags removidas, texto preservado
            Assert.DoesNotContain("<", respostas["nome"]); // sanitizado (sem HTML)
        }

        [Fact]
        public async Task Execucao_Multi_Etapa_Avanca_Ate_Concluir()
        {
            using var ctx = Novo(nameof(Execucao_Multi_Etapa_Avanca_Ate_Concluir));
            await new CriarDefinicaoWizardCommandHandler(ctx, T(), U()).Handle(new CriarDefinicaoWizardCommand("W2", "Multi", null, false), CancellationToken.None);
            var defId = await ctx.DefinicoesWizard.Where(d => d.Codigo == "W2").Select(d => d.Id).FirstAsync();
            var eh = new AdicionarEtapaWizardCommandHandler(ctx, T(), U());
            await eh.Handle(new AdicionarEtapaWizardCommand(defId, 1, "E1", null), CancellationToken.None);
            await eh.Handle(new AdicionarEtapaWizardCommand(defId, 2, "E2", null), CancellationToken.None);
            await new PublicarDefinicaoWizardCommandHandler(ctx, T(), U()).Handle(new PublicarDefinicaoWizardCommand(defId), CancellationToken.None);
            await new IniciarExecucaoWizardCommandHandler(ctx, T(), U()).Handle(new IniciarExecucaoWizardCommand(defId, false), CancellationToken.None);
            var execId = await ctx.ExecucoesWizard.Where(e => e.DefinicaoId == defId).Select(e => e.Id).FirstAsync();

            var rh = new ResponderEtapaWizardCommandHandler(ctx, T(), U());
            var r1 = await rh.Handle(new ResponderEtapaWizardCommand(execId, new Dictionary<string, string> { ["a"] = "1" }), CancellationToken.None);
            var r2 = await rh.Handle(new ResponderEtapaWizardCommand(execId, new Dictionary<string, string> { ["b"] = "2" }), CancellationToken.None);

            Assert.True(r1.Sucesso);
            Assert.True(r2.Sucesso);
            var exec = await ctx.ExecucoesWizard.FirstAsync(e => e.Id == execId);
            Assert.Equal("Concluida", exec.Status);
        }
    }
}
