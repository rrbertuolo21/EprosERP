using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Modules.Financeiro.Application.Handlers;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Application.Handlers;
using Epros.Modules.RH.Application.Queries;
using Epros.Modules.RH.Domain.Entities;
using Epros.Modules.RH.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    public class RHModuleTests
    {
        [Fact]
        public async Task Deve_Admitir_Colaborador_E_Falhar_Se_Cpf_Duplicado()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextRH>()
                .UseInMemoryDatabase("db_rh_admissao")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextRH(options, tenantProvider, currentUser);

            var handler = new AdmitirColaboradorCommandHandler(context, tenantProvider, currentUser);
            var command1 = new AdmitirColaboradorCommand("José Silva", "11122233344", "jose@epros.com.br", "Analista", "TI", 5000m, DateTime.UtcNow);
            var command2 = new AdmitirColaboradorCommand("José Santos", "11122233344", "santos@epros.com.br", "Analista Jr", "TI", 4000m, DateTime.UtcNow);

            // Agir
            var result1 = await handler.Handle(command1, CancellationToken.None);
            var result2 = await handler.Handle(command2, CancellationToken.None);

            // Assertiva
            Assert.True(result1.Sucesso);
            Assert.False(result2.Sucesso);
            Assert.Contains("Já existe um colaborador cadastrado com este CPF", result2.Erros.FirstOrDefault() ?? "");

            var colaboradores = await context.Colaboradores.ToListAsync();
            Assert.Single(colaboradores);
            Assert.Equal("José Silva", colaboradores[0].Nome);
            Assert.Equal("Ativo", colaboradores[0].Status);
        }

        [Fact]
        public async Task Deve_Desligar_Colaborador()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextRH>()
                .UseInMemoryDatabase("db_rh_desligamento")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextRH(options, tenantProvider, currentUser);

            var colaborador = new Colaborador("José Silva", "11122233344", "jose@epros.com.br", "Analista", "TI", 5000m, DateTime.UtcNow.AddDays(-10), "tenant-1", "user-1");
            context.Colaboradores.Add(colaborador);
            await context.SaveChangesAsync();

            var handler = new DesligarColaboradorCommandHandler(context, currentUser);
            var demissaoData = DateTime.UtcNow;
            var command = new DesligarColaboradorCommand(colaborador.Id, demissaoData);

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var atualizado = await context.Colaboradores.FindAsync(colaborador.Id);
            Assert.Equal("Desligado", atualizado!.Status);
            Assert.Equal(demissaoData, atualizado.DataDemissao);
        }

        [Fact]
        public async Task Deve_Desligar_E_Apurar_Rescisao_Pelo_Motor()
        {
            var options = new DbContextOptionsBuilder<ContextRH>()
                .UseInMemoryDatabase("db_rh_rescisao_motor")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextRH(options, tenantProvider, currentUser);

            var admissao = new DateTime(2024, 1, 10);
            var colaborador = new Colaborador("Ana Souza", "22233344455", "ana@epros.com.br", "Analista", "TI", 3000m, admissao, "tenant-1", "user-1");
            context.Colaboradores.Add(colaborador);
            await context.SaveChangesAsync();

            var handler = new DesligarColaboradorCommandHandler(context, currentUser);
            var demissao = new DateTime(2026, 6, 20);
            var command = new DesligarColaboradorCommand(
                colaborador.Id,
                demissao,
                TipoDesligamento: Epros.Modules.RH.Domain.Folha.Calculo.TipoDesligamento.SemJustaCausaEmpregador,
                SaldoFgtsDepositado: 5000m,
                TemFeriasVencidas: false);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);

            // Colaborador desligado.
            var atualizado = await context.Colaboradores.FindAsync(colaborador.Id);
            Assert.Equal("Desligado", atualizado!.Status);

            // Rescisão persistida pelo motor (multa 40% do FGTS = 2000; aviso proporcional > 30 dias).
            var rescisoes = await context.FolRescisaos.ToListAsync();
            Assert.Single(rescisoes);
            Assert.Equal(colaborador.Id, rescisoes[0].ColaboradorId);
            Assert.Equal(2000m, rescisoes[0].FgtsValorRescisao);
            Assert.True(rescisoes[0].DiasAvisoPrevio >= 30);

            // Confere contra o motor diretamente (verbas por tipo).
            var esperado = Epros.Modules.RH.Domain.Folha.Calculo.MotorRescisao.Calcular(
                new Epros.Modules.RH.Domain.Folha.Calculo.EntradaRescisao(
                    Epros.Modules.RH.Domain.Folha.Calculo.TipoDesligamento.SemJustaCausaEmpregador,
                    3000m, admissao, demissao, demissao.Day, SaldoFgtsDepositado: 5000m),
                Epros.Modules.RH.Domain.Folha.Calculo.TabelasFolha.Vigente(2026));
            Assert.Equal(esperado.DiasAvisoPrevio, rescisoes[0].DiasAvisoPrevio);
            Assert.Equal(esperado.MultaFgts, rescisoes[0].FgtsValorRescisao);
            Assert.True(esperado.TemDireitoSeguroDesemprego);
        }

        [Fact]
        public async Task Deve_Registrar_Timesheet_E_Falhar_Se_Desligado()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextRH>()
                .UseInMemoryDatabase("db_rh_timesheet")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextRH(options, tenantProvider, currentUser);

            var colAtivo = new Colaborador("Ativo", "111", "ativo@epros.com.br", "Dev", "TI", 5000m, DateTime.UtcNow.AddDays(-5), "tenant-1", "user-1");
            var colDesligado = new Colaborador("Desligado", "222", "desligado@epros.com.br", "Dev", "TI", 5000m, DateTime.UtcNow.AddDays(-5), "tenant-1", "user-1");
            colDesligado.Desligar(DateTime.UtcNow, "user-1");

            context.Colaboradores.AddRange(colAtivo, colDesligado);
            await context.SaveChangesAsync();

            var handler = new RegistrarTimesheetCommandHandler(context, tenantProvider, currentUser);

            // Agir
            var resultAtivo = await handler.Handle(new RegistrarTimesheetCommand(colAtivo.Id, DateTime.UtcNow, 8m, "Programação C#"), CancellationToken.None);
            var resultDesligado = await handler.Handle(new RegistrarTimesheetCommand(colDesligado.Id, DateTime.UtcNow, 8m, "Programação C#"), CancellationToken.None);

            // Assertiva
            Assert.True(resultAtivo.Sucesso);
            Assert.False(resultDesligado.Sucesso);
            Assert.Contains("Não é possível registrar horas", resultDesligado.Erros.FirstOrDefault() ?? "");

            var timesheets = await context.Timesheets.ToListAsync();
            Assert.Single(timesheets);
            Assert.Equal(colAtivo.Id, timesheets[0].ColaboradorId);
            Assert.Equal(8m, timesheets[0].HorasTrabalhadas);
        }

        [Fact]
        public async Task Deve_Processar_Folha_E_Gerar_Outbox_Event()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextRH>()
                .UseInMemoryDatabase("db_rh_folha")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextRH(options, tenantProvider, currentUser);

            var colaborador = new Colaborador("José Silva", "11122233344", "jose@epros.com.br", "Analista", "TI", 5000m, DateTime.UtcNow.AddDays(-10), "tenant-1", "user-1");
            context.Colaboradores.Add(colaborador);
            await context.SaveChangesAsync();

            var handler = new ProcessarFolhaPagamentoCommandHandler(context, tenantProvider, currentUser);

            var verbas = new List<FolhaPagamentoVerbaInput>
            {
                new("Bônus", "Provento", 500m),
                new("Vale Refeição", "Desconto", 200m)
            };
            var command = new ProcessarFolhaPagamentoCommand(colaborador.Id, 6, 2026, verbas);

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var folhas = await context.FolhasPagamento.Include(f => f.Verbas).ToListAsync();
            Assert.Single(folhas);
            Assert.Equal(6, folhas[0].MesCompetencia);
            Assert.Equal(2026, folhas[0].AnoCompetencia);

            // O líquido agora vem do MOTOR LEGAL (INSS/IRRF descontados), não mais de bruto − verbas.
            // Bruto = 5000 (base) + 500 (bônus) = 5500; descontos = INSS + IRRF + VR (200).
            var esperado = Epros.Modules.RH.Domain.Folha.Calculo.MotorFolhaMensal.Calcular(
                new Epros.Modules.RH.Domain.Folha.Calculo.EntradaFolhaMensal(
                    5000m,
                    ProventosAdicionais: new List<Epros.Modules.RH.Domain.Folha.Calculo.ItemProvento>
                    {
                        new("P001", "Bônus", 500m)
                    },
                    DescontosDiversos: new List<Epros.Modules.RH.Domain.Folha.Calculo.ItemDesconto>
                    {
                        new("D001", "Vale Refeição", 200m)
                    }),
                Epros.Modules.RH.Domain.Folha.Calculo.TabelasFolha.Vigente(2026));

            Assert.Equal(5500m, folhas[0].SalarioBruto);
            Assert.Equal(esperado.Liquido, folhas[0].SalarioLiquido);
            Assert.True(folhas[0].SalarioLiquido < 5300m, "O líquido do motor deve refletir INSS/IRRF, ficando abaixo do bruto − verbas manuais.");
            // Rubricas persistidas: bônus, INSS, VR e o encargo FGTS (salário-base é o seed do bruto).
            Assert.Contains(folhas[0].Verbas, v => v.Descricao == "Bônus" && v.Tipo == "Provento");
            Assert.Contains(folhas[0].Verbas, v => v.Descricao == "INSS" && v.Tipo == "Desconto");
            Assert.Contains(folhas[0].Verbas, v => v.Descricao == "Vale Refeição" && v.Tipo == "Desconto");
            Assert.Contains(folhas[0].Verbas, v => v.Tipo == "Encargo");

            var outboxMsg = await context.OutboxMessages.FirstOrDefaultAsync();
            Assert.NotNull(outboxMsg);
            Assert.Equal("FolhaProcessada", outboxMsg!.EventType);
            Assert.Contains(esperado.Liquido.ToString(System.Globalization.CultureInfo.InvariantCulture), outboxMsg.Payload);
        }

        [Fact]
        public async Task Deve_Processar_Folha_Com_Jornada_E_Sst_Como_Proventos()
        {
            var options = new DbContextOptionsBuilder<ContextRH>()
                .UseInMemoryDatabase("db_rh_folha_jornada_sst")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextRH(options, tenantProvider, currentUser);

            var colaborador = new Colaborador("Bruno Lima", "33344455566", "bruno@epros.com.br", "Operador", "Produção", 2000m, DateTime.UtcNow.AddDays(-100), "tenant-1", "user-1");
            context.Colaboradores.Add(colaborador);
            await context.SaveChangesAsync();

            var handler = new ProcessarFolhaPagamentoCommandHandler(context, tenantProvider, currentUser);

            // Periculosidade (30% de 2000 = 600) + 10 horas extras a 50%.
            var command = new ProcessarFolhaPagamentoCommand(
                colaborador.Id, 6, 2026, new List<FolhaPagamentoVerbaInput>(),
                HorasExtras: 10m,
                AdicionalHorasExtras: 0.5m,
                TemPericulosidade: true);

            var result = await handler.Handle(command, CancellationToken.None);
            Assert.True(result.Sucesso);

            var folha = await context.FolhasPagamento.Include(f => f.Verbas).FirstAsync();

            // valor-hora = 2000/220 = 9,090909; HE = 9,090909 × 1,5 × 10 = 136,36.
            var valorHora = Epros.Modules.RH.Domain.Jornada.Calculo.MotorJornada.ValorHora(2000m);
            var heEsperado = Epros.Modules.RH.Domain.Jornada.Calculo.MotorJornada.HorasExtras(valorHora, 10m, 0.5m);

            Assert.Contains(folha.Verbas, v => v.Descricao == "Adicional Periculosidade" && v.Tipo == "Provento" && v.Valor == 600m);
            Assert.Contains(folha.Verbas, v => v.Descricao == "Horas extras" && v.Tipo == "Provento" && v.Valor == heEsperado);
            // Bruto = 2000 + 600 (pericul.) + HE.
            Assert.Equal(2000m + 600m + heEsperado, folha.SalarioBruto);
        }

        [Fact]
        public async Task Deve_Criar_ContaPagar_Ao_Processar_FolhaProcessada()
        {
            // Organizar
            var optionsEstoque = new DbContextOptionsBuilder<ContextFinanceiro>()
                .UseInMemoryDatabase("db_financeiro_folha_integracao")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var contextFinanceiro = new ContextFinanceiro(optionsEstoque, tenantProvider, currentUser);

            var handler = new FolhaProcessadaFinanceiroHandler(contextFinanceiro);

            var notification = new FolhaProcessadaEventNotification(
                FolhaPagamentoId: Guid.NewGuid(),
                ColaboradorId: Guid.NewGuid(),
                NomeColaborador: "José Silva",
                CpfColaborador: "11122233344",
                MesCompetencia: 6,
                AnoCompetencia: 2026,
                SalarioLiquido: 5300m,
                TenantId: "tenant-1"
            );

            // Agir
            await handler.Handle(notification, CancellationToken.None);

            // Assertiva
            var contas = await contextFinanceiro.ContasAPagarAgregado.ToListAsync();
            Assert.Single(contas);
            Assert.Equal(notification.ColaboradorId, contas[0].PessoaId);
            Assert.Equal(5300m, contas[0].ValorTitulo);
            Assert.Contains("José Silva", contas[0].Detalhamento);
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
