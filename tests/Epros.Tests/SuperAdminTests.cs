using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Epros.Tests
{
    public class SuperAdminTests
    {
        [Fact]
        public async Task Deve_Aprovar_Assinatura_Manual_Com_Sucesso_E_Reativar_Cliente()
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase("db_superadmin_manual_sub_success")
                .Options;

            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("user-system");
            using var context = new ContextGestaoClientes(options, tenantProvider, currentUser);

            // Cadastra plano e cliente
            var plano = new Plano("Plano Custom", 150.00m, "tenant-abc", "system");
            var planoId = Guid.NewGuid();
            typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(plano, planoId);
            context.Planos.Add(plano);

            var cliente = new Cliente("Cliente Teste Inativo", "99999999000199", "inativo@teste.com", planoId, "tenant-abc", "system");
            var clienteId = Guid.NewGuid();
            typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(cliente, clienteId);
            cliente.Inativar("system"); // Começa suspenso
            context.Clientes.Add(cliente);

            await context.SaveChangesAsync();

            var handler = new AprovarAssinaturaManualCommandHandler(context, tenantProvider, currentUser, NullLogger<AprovarAssinaturaManualCommandHandler>.Instance);
            var command = new AprovarAssinaturaManualCommand(
                clienteId,
                DateTime.UtcNow,
                DateTime.UtcNow.AddMonths(1),
                10,
                150.00m,
                null,
                "Super Admin",
                "Aprovação de contrato manual offline corporativo."
            );

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.True(cliente.Ativo); // Deve ter sido ativado

            var contrato = await context.Contratos.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.ClienteId == clienteId);
            Assert.NotNull(contrato);
            Assert.True(contrato!.Ativo);
            Assert.Equal(150.00m, contrato.ValorRecorrente);
            Assert.Equal(10, contrato.DiaVencimento);
        }

        [Fact]
        public async Task Deve_Rejeitar_Aprovacao_Assinatura_Para_Tenant_Nao_Siser()
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase("db_superadmin_manual_sub_forbidden")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-abc"); // Não é "system"
            var currentUser = new TestCurrentUser("user-abc");
            using var context = new ContextGestaoClientes(options, tenantProvider, currentUser);

            var handler = new AprovarAssinaturaManualCommandHandler(context, tenantProvider, currentUser, NullLogger<AprovarAssinaturaManualCommandHandler>.Instance);
            var command = new AprovarAssinaturaManualCommand(
                Guid.NewGuid(),
                DateTime.UtcNow,
                DateTime.UtcNow.AddMonths(1),
                10,
                150.00m,
                null,
                "Super Admin",
                "Aprovação para cliente inexistente."
            );

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Acesso Proibido"));
        }

        [Fact]
        public async Task Deve_Validar_Precedencia_De_Datas_Na_Assinatura()
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase("db_superadmin_dates_validation")
                .Options;

            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("user-system");
            using var context = new ContextGestaoClientes(options, tenantProvider, currentUser);

            var planoId = Guid.NewGuid();
            var cliente = new Cliente("Cliente Teste", "99999999000199", "inativo@teste.com", planoId, "tenant-abc", "system");
            var clienteId = Guid.NewGuid();
            typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(cliente, clienteId);
            context.Clientes.Add(cliente);

            await context.SaveChangesAsync();

            var handler = new AprovarAssinaturaManualCommandHandler(context, tenantProvider, currentUser, NullLogger<AprovarAssinaturaManualCommandHandler>.Instance);

            // Cenário 1: Data de início posterior à data de término
            var commandInvalido = new AprovarAssinaturaManualCommand(
                clienteId,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(-5), // Término antes do início
                10,
                150.00m,
                null,
                "Super Admin",
                "Tentativa com datas inválidas."
            );

            var resultInvalido = await handler.Handle(commandInvalido, CancellationToken.None);
            Assert.False(resultInvalido.Sucesso);
            Assert.Contains(resultInvalido.Erros, e => e.Contains("posterior"));
        }

        [Fact]
        public async Task Deve_Persistir_Operador_E_Justificativa_E_Recalcular_Datas_De_Vigencia_Na_Assinatura()
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase("db_superadmin_approval_manual_details")
                .Options;

            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("user-system");
            using var context = new ContextGestaoClientes(options, tenantProvider, currentUser);

            // Cadastra plano e cliente
            var planoId = Guid.NewGuid();
            var plano = new Plano("Plano Custom", 150.00m, "tenant-abc", "system");
            typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(plano, planoId);
            context.Planos.Add(plano);

            var clienteId = Guid.NewGuid();
            var cliente = new Cliente("Cliente Teste Inativo", "99999999000199", "inativo@teste.com", planoId, "tenant-abc", "system");
            typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(cliente, clienteId);
            context.Clientes.Add(cliente);

            // Cria uma assinatura pendente (Aguardando)
            var assinaturaId = Guid.NewGuid();
            var dataFimOriginal = DateTime.UtcNow.AddDays(20);
            var assinaturaPendente = new AssinaturaCliente(
                clienteId: clienteId,
                planoId: planoId,
                status: AssinaturaStatus.Aguardando,
                dataInicio: DateTime.UtcNow.AddDays(-5),
                dataFim: dataFimOriginal,
                trialAte: null,
                metodoPagamento: "Manual",
                transacaoId: null,
                detalhesPacoteJson: "{}",
                tenantId: "tenant-abc",
                criadoPor: "system"
            );
            typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(assinaturaPendente, assinaturaId);
            context.AssinaturasClientes.Add(assinaturaPendente);

            await context.SaveChangesAsync();

            var handler = new AprovarAssinaturaManualCommandHandler(context, tenantProvider, currentUser, NullLogger<AprovarAssinaturaManualCommandHandler>.Instance);
            var command = new AprovarAssinaturaManualCommand(
                clienteId,
                DateTime.UtcNow,
                dataFimOriginal,
                10,
                150.00m,
                null,
                "Operador Siser Alfa",
                "Justificativa para homologar cliente no ambiente."
            );

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);

            // Verifica se o contrato foi atualizado com operador e justificativa
            var contrato = await context.Contratos.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.ClienteId == clienteId);
            Assert.NotNull(contrato);
            Assert.Equal("Operador Siser Alfa", contrato!.OperadorAprovacao);
            Assert.Equal("Justificativa para homologar cliente no ambiente.", contrato.JustificativaAprovacao);

            // Verifica se a assinatura foi ativada e teve as datas recalculadas
            var assinaturaAtualizada = await context.AssinaturasClientes.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == assinaturaId);
            Assert.NotNull(assinaturaAtualizada);
            Assert.Equal(AssinaturaStatus.Aprovada, assinaturaAtualizada!.Status);
            Assert.Equal("Operador Siser Alfa", assinaturaAtualizada.OperadorAprovacao);
            Assert.Equal("Justificativa para homologar cliente no ambiente.", assinaturaAtualizada.JustificativaAprovacao);

            // REG-008: DataInicio deve ser hoje (UtcNow) e DataFim recalculada para DataInicio + 30 dias
            Assert.True((DateTime.UtcNow - assinaturaAtualizada.DataInicio!.Value).TotalMinutes < 2);
            Assert.Equal(assinaturaAtualizada.DataInicio.Value.AddDays(30).Date, assinaturaAtualizada.DataFim!.Value.Date);
        }

        [Fact]
        public async Task Deve_Executar_Fluxo_Maker_Checker_Para_Execucao_Massa()
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase("db_superadmin_maker_checker")
                .Options;

            var tenantProvider = new TestTenantProvider("system");

            // Administrador 1 (Maker) cria o lote
            var currentUserMaker = new TestCurrentUser("admin-maker");
            using (var context = new ContextGestaoClientes(options, tenantProvider, currentUserMaker))
            {
                // Cadastra planos para o teste de atualização
                var plano1 = new Plano("Bronze", 50.00m, "system", "system");
                var plano2 = new Plano("Gold", 100.00m, "system", "system");
                context.Planos.AddRange(plano1, plano2);
                await context.SaveChangesAsync();

                var handlerCriar = new CriarExecucaoMassaCommandHandler(context, tenantProvider, currentUserMaker);
                var cmdCriar = new CriarExecucaoMassaCommand("AtualizarPrecosPlanos", "{\"Percentual\": 20}");

                var resCriar = await handlerCriar.Handle(cmdCriar, CancellationToken.None);
                Assert.True(resCriar.Sucesso);

                var execId = (Guid)resCriar.Dados.GetType().GetProperty("ExecucaoMassaId").GetValue(resCriar.Dados);

                // Tenta aprovar novamente com o mesmo usuário Maker (deve falhar - checker deve ser outro)
                var handlerAprovar = new AprovarExecucaoMassaCommandHandler(context, tenantProvider, currentUserMaker);
                var cmdAprovarMesmoUser = new AprovarExecucaoMassaCommand(execId);
                var resAprovarMesmo = await handlerAprovar.Handle(cmdAprovarMesmoUser, CancellationToken.None);
                Assert.False(resAprovarMesmo.Sucesso);

                // Checker aprova e executa
                var currentUserChecker = new TestCurrentUser("admin-checker");
                using var contextChecker = new ContextGestaoClientes(options, tenantProvider, currentUserChecker);
                var handlerAprovarDiferente = new AprovarExecucaoMassaCommandHandler(contextChecker, tenantProvider, currentUserChecker);

                var resAprovarChecker = await handlerAprovarDiferente.Handle(new AprovarExecucaoMassaCommand(execId), CancellationToken.None);
                Assert.True(resAprovarChecker.Sucesso);

                // Verifica se os preços dos planos foram atualizados em 20%
                var planosAtualizados = await contextChecker.Planos.IgnoreQueryFilters().ToListAsync();
                var bronze = planosAtualizados.First(p => p.Nome == "Bronze");
                var gold = planosAtualizados.First(p => p.Nome == "Gold");

                Assert.Equal(60.00m, bronze.Preco); // 50 * 1.2
                Assert.Equal(120.00m, gold.Preco); // 100 * 1.2

                // Verifica se o status do lote está Executado
                var execRecord = await contextChecker.ExecucoesMassa.FirstOrDefaultAsync(e => e.Id == execId);
                Assert.Equal("Executado", execRecord!.Status);
                Assert.Contains("Bronze", execRecord.ResultadoLog);
            }
        }

        [Fact]
        public async Task Deve_Executar_Suspensao_De_Inadimplentes_Em_Massa()
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase("db_superadmin_mass_suspension")
                .Options;

            var tenantProvider = new TestTenantProvider("system");
            var currentUserMaker = new TestCurrentUser("admin-maker");

            using (var context = new ContextGestaoClientes(options, tenantProvider, currentUserMaker))
            {
                var planoId = Guid.NewGuid();

                // Cadastra 2 clientes
                var cliente1 = new Cliente("Tenant A Corp", "11111111000111", "a@corp.com", planoId, "tenant-a", "system");
                var cliente1Id = Guid.NewGuid();
                typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(cliente1, cliente1Id);

                var cliente2 = new Cliente("Tenant B Corp", "22222222000122", "b@corp.com", planoId, "tenant-b", "system");
                var cliente2Id = Guid.NewGuid();
                typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(cliente2, cliente2Id);

                context.Clientes.AddRange(cliente1, cliente2);

                // Cria faturas vencidas (vencimento há 20 dias atrás)
                var fatura1 = new Fatura(cliente1Id, 199.90m, DateTime.UtcNow.AddDays(-20), "tenant-a", "system");
                var fatura2 = new Fatura(cliente2Id, 299.90m, DateTime.UtcNow.AddDays(-20), "tenant-b", "system");

                context.Faturas.AddRange(fatura1, fatura2);
                await context.SaveChangesAsync();

                // Cria lote de suspensão em massa por atraso maior que 15 dias
                var handlerCriar = new CriarExecucaoMassaCommandHandler(context, tenantProvider, currentUserMaker);
                var cmdCriar = new CriarExecucaoMassaCommand("SuspenderInadimplentes", "{\"DiasAtraso\": 15}");
                var resCriar = await handlerCriar.Handle(cmdCriar, CancellationToken.None);

                var execId = (Guid)resCriar.Dados.GetType().GetProperty("ExecucaoMassaId").GetValue(resCriar.Dados);

                // Checker aprova e executa
                var currentUserChecker = new TestCurrentUser("admin-checker");
                using var contextChecker = new ContextGestaoClientes(options, tenantProvider, currentUserChecker);
                var handlerAprovar = new AprovarExecucaoMassaCommandHandler(contextChecker, tenantProvider, currentUserChecker);

                var resAprovar = await handlerAprovar.Handle(new AprovarExecucaoMassaCommand(execId), CancellationToken.None);
                Assert.True(resAprovar.Sucesso);

                // Ambos clientes devem ter sido inativados (suspensos)
                var c1 = await contextChecker.Clientes.IgnoreQueryFilters().FirstAsync(c => c.Id == cliente1Id);
                var c2 = await contextChecker.Clientes.IgnoreQueryFilters().FirstAsync(c => c.Id == cliente2Id);

                Assert.False(c1.Ativo);
                Assert.False(c2.Ativo);
            }
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
            public string? GetUserName() => "Super Admin Tester";
            public string? GetUserEmail() => "super@siser.com";
        }
    }
}
