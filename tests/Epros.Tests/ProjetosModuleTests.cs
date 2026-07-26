using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Modules.Financeiro.Application.Handlers;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Application.Handlers;
using Epros.Modules.Projetos.Application.Queries;
using Epros.Modules.Projetos.Domain.Entities;
using Epros.Modules.Projetos.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    public class ProjetosModuleTests
    {
        [Fact]
        public async Task Deve_Criar_Projeto()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextProjetos>()
                .UseInMemoryDatabase("db_projetos_criar")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextProjetos(options, tenantProvider, currentUser);

            var handler = new CriarProjetoCommandHandler(context, tenantProvider, currentUser);
            var command = new CriarProjetoCommand("Construção Fábrica", "Projeto de construção civil", Guid.NewGuid(), DateTime.UtcNow, null, 100000m);

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var projetos = await context.Projetos.ToListAsync();
            Assert.Single(projetos);
            Assert.Equal("Construção Fábrica", projetos[0].Nome);
            Assert.Equal("Planejado", projetos[0].Status);
            Assert.Equal(100000m, projetos[0].OrcamentoTotal);
        }

        [Fact]
        public async Task Deve_Adicionar_Tarefa_Wbs_E_Alocar_Recurso()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextProjetos>()
                .UseInMemoryDatabase("db_projetos_wbs_alocacao")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextProjetos(options, tenantProvider, currentUser);

            var projeto = new Projeto("Construção Fábrica", "Construção civil", Guid.NewGuid(), DateTime.UtcNow, null, 100000m, "tenant-1", "user-1");
            context.Projetos.Add(projeto);
            await context.SaveChangesAsync();

            var wbsHandler = new AdicionarWbsItemCommandHandler(context, currentUser);
            var alocHandler = new AlocarRecursoCommandHandler(context, currentUser);

            // Agir
            var wbsResult = await wbsHandler.Handle(new AdicionarWbsItemCommand(projeto.Id, "Fundação", "Fazer fundação da fábrica", DateTime.UtcNow, DateTime.UtcNow.AddDays(5), 10m), CancellationToken.None);
            var alocResult = await alocHandler.Handle(new AlocarRecursoCommand(projeto.Id, Guid.NewGuid(), "Engenheiro Civil", 150m, 80m), CancellationToken.None);

            // Assertiva
            Assert.True(wbsResult.Sucesso);
            Assert.True(alocResult.Sucesso);

            var atualizado = await context.Projetos.Include(p => p.ItensWbs).Include(p => p.Alocacoes).FirstAsync(p => p.Id == projeto.Id);
            Assert.Single(atualizado.ItensWbs);
            Assert.Single(atualizado.Alocacoes);
            Assert.Equal("Fundação", atualizado.ItensWbs[0].Nome);
            Assert.Equal("Engenheiro Civil", atualizado.Alocacoes[0].Funcao);
            Assert.Equal(150m, atualizado.Alocacoes[0].CustoHora);
        }

        [Fact]
        public async Task Deve_Atualizar_Progresso_E_Pular_Para_EmAndamento()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextProjetos>()
                .UseInMemoryDatabase("db_projetos_progresso")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextProjetos(options, tenantProvider, currentUser);

            var projeto = new Projeto("Construção Fábrica", "Construção civil", Guid.NewGuid(), DateTime.UtcNow, null, 100000m, "tenant-1", "user-1");
            projeto.AdicionarItemWbs("Fundação", "Fazer fundação", DateTime.UtcNow, DateTime.UtcNow.AddDays(5), 10m, "user-1");
            projeto.AdicionarItemWbs("Estrutura", "Fazer estrutura", DateTime.UtcNow, DateTime.UtcNow.AddDays(10), 10m, "user-1");
            context.Projetos.Add(projeto);
            await context.SaveChangesAsync();

            var handler = new AtualizarProgressoTarefaCommandHandler(context, tenantProvider, currentUser);
            var wbsTarefa = projeto.ItensWbs[0];

            // Agir
            var result = await handler.Handle(new AtualizarProgressoTarefaCommand(projeto.Id, wbsTarefa.Id, 50m), CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var atualizado = await context.Projetos.Include(p => p.ItensWbs).FirstAsync(p => p.Id == projeto.Id);
            Assert.Equal("EmAndamento", atualizado.Status);
            // 50% de conclusão na tarefa de peso 10, com outra tarefa de peso 10 zerada: (50 * 10 + 0 * 10) / 20 = 25% de progresso do projeto
            Assert.Equal(25m, atualizado.PercentualConclusao);

            var outboxMessages = await context.OutboxMessages.ToListAsync();
            // Atingiu exatamente 25%, então deve ter gerado a mensagem do outbox para o marco de 25%
            Assert.Single(outboxMessages);
            Assert.Equal("ProjetoFaturado", outboxMessages[0].EventType);
            Assert.Contains("25", outboxMessages[0].Payload);
            // Valor do faturamento do marco de 25% = 25% de 100k = 25k
            Assert.Contains("25000", outboxMessages[0].Payload);
        }

        [Fact]
        public async Task Deve_Criar_ContaReceber_Ao_Processar_ProjetoFaturado()
        {
            // Organizar
            var optionsFinanceiro = new DbContextOptionsBuilder<ContextFinanceiro>()
                .UseInMemoryDatabase("db_financeiro_projeto_integracao")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var contextFinanceiro = new ContextFinanceiro(optionsFinanceiro, tenantProvider, currentUser);

            var handler = new ProjetoFaturadoFinanceiroHandler(contextFinanceiro);

            var notification = new ProjetoFaturadoEventNotification(
                ProjetoId: Guid.NewGuid(),
                NomeProjeto: "Construção Fábrica",
                ClienteId: Guid.NewGuid(),
                Milestone: 25m,
                ValorFaturamento: 25000m,
                TenantId: "tenant-1"
            );

            // Agir
            await handler.Handle(notification, CancellationToken.None);

            // Assertiva
            var contas = await contextFinanceiro.ContasAReceberAgregado.ToListAsync();
            Assert.Single(contas);
            Assert.Equal(notification.ClienteId, contas[0].PessoaId);
            Assert.Equal(25000m, contas[0].ValorTitulo);
            Assert.Contains("Faturamento automático", contas[0].Detalhamento);
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public TestTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "test_user";
            public string? GetUserEmail() => "test@epros.com.br";
        }
    }
}
