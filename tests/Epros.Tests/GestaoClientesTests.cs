using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Application.Queries;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Epros.API.Security;

namespace Epros.Tests
{
    public class GestaoClientesTests
    {
        #region Testes de Entidades (Domínio)

        [Fact]
        public void Deve_Criar_Plano_Valido_Corretamente()
        {
            // Arrange & Act
            var plano = new Plano("Plano Essencial", 299.90m, "tenant-1", "user-admin");

            // Assert
            Assert.True(plano.IsValid);
            Assert.Equal("Plano Essencial", plano.Nome);
            Assert.Equal(299.90m, plano.Preco);
            Assert.True(plano.Ativo);
            Assert.Empty(plano.Notifications);
        }

        [Fact]
        public void Nao_Deve_Criar_Plano_Com_Preco_Invalido_Ou_Sem_Nome()
        {
            // Act
            var planoSemNome = new Plano("", 100m, "tenant-1", "user-admin");
            var planoPrecoNegativo = new Plano("Plano Teste", -10m, "tenant-1", "user-admin");
 
             // Assert
             Assert.False(planoSemNome.IsValid);
             Assert.Contains(planoSemNome.Notifications, n => n.Key == "Nome");
 
             Assert.False(planoPrecoNegativo.IsValid);
             Assert.Contains(planoPrecoNegativo.Notifications, n => n.Key == "Preco");
        }

        [Fact]
        public void Deve_Criar_Cliente_Valido_Corretamente()
        {
            // Arrange & Act
            var cliente = new Cliente(
                "Empresa Teste Ltda",
                "12.345.678/0001-99",
                "contato@empresateste.com.br",
                Guid.NewGuid(),
                "tenant-1",
                "user-admin"
            );

            // Assert
            Assert.True(cliente.IsValid);
            Assert.Equal("Empresa Teste Ltda", cliente.RazaoSocial);
            Assert.Equal("12.345.678/0001-99", cliente.Cnpj);
            Assert.Equal("contato@empresateste.com.br", cliente.Email);
            Assert.True(cliente.Ativo);
        }

        [Fact]
        public void Nao_Deve_Criar_Cliente_Com_Email_Invalido()
        {
            // Act
            var clienteEmailInvalido = new Cliente(
                "Empresa Teste Ltda",
                "12.345.678/0001-99",
                "email-invalido",
                Guid.NewGuid(),
                "tenant-1",
                "user-admin"
            );

            // Assert
            Assert.False(clienteEmailInvalido.IsValid);
            Assert.Contains(clienteEmailInvalido.Notifications, n => n.Key == "Email");
        }

        [Fact]
        public void Deve_Ativar_E_Inativar_Cliente_Corretamente()
        {
            // Arrange
            var cliente = new Cliente(
                "Empresa Teste Ltda",
                "12.345.678/0001-99",
                "contato@empresateste.com.br",
                Guid.NewGuid(),
                "tenant-1",
                "user-admin"
            );

            // Act - Inativar
            cliente.Inativar("user-editor");

            // Assert
            Assert.False(cliente.Ativo);
            Assert.Equal("user-editor", cliente.AlteradoPor);
            Assert.NotNull(cliente.AlteradoEm);

            // Act - Ativar
            cliente.Ativar("user-editor-2");

            // Assert
            Assert.True(cliente.Ativo);
            Assert.Equal("user-editor-2", cliente.AlteradoPor);
        }

        [Fact]
        public void Deve_Criar_Fatura_Valida_Corretamente()
        {
            // Arrange & Act
            var clienteId = Guid.NewGuid();
            var fatura = new Fatura(clienteId, 299.90m, DateTime.UtcNow.AddDays(30), "tenant-1", "user-admin");

            // Assert
            Assert.True(fatura.IsValid);
            Assert.Equal(clienteId, fatura.ClienteId);
            Assert.Equal(299.90m, fatura.Valor);
            Assert.Equal(FaturaStatus.Pendente, fatura.Status);
        }

        [Fact]
        public void Deve_Baixar_E_Cancelar_Fatura_Corretamente()
        {
            // Arrange
            var fatura = new Fatura(Guid.NewGuid(), 299.90m, DateTime.UtcNow.AddDays(30), "tenant-1", "user-admin");

            // Act - Baixar/Pagar
            fatura.Baixar("user-financeiro");

            // Assert
            Assert.Equal(FaturaStatus.Paga, fatura.Status);
            Assert.NotNull(fatura.DataPagamento);
            Assert.Equal("user-financeiro", fatura.AlteradoPor);

            // Act - Cancelar
            fatura.Cancelar("user-financeiro-2");

            // Assert
            Assert.Equal(FaturaStatus.Cancelada, fatura.Status);
            Assert.Equal("user-financeiro-2", fatura.AlteradoPor);
        }

        #endregion

        #region Testes de Handlers (CQRS)

        [Fact]
        public async Task Deve_Criar_Plano_Via_Handler_Com_Sucesso()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-x");
            var currentUser = new TestCurrentUser("user-1");
            var context = CreateInMemoryContext("db_plano_success", "tenant-x", "user-1");
            var handler = new CriarPlanoCommandHandler(context, tenantProvider, currentUser);
            var command = new CriarPlanoCommand("Plano Corporativo", 499.90m, new List<string> { "Estoque", "Financeiro" });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            Assert.NotNull(result.Dados);

            var planoSalvo = await context.Planos.Include(p => p.Modulos).FirstOrDefaultAsync();
            Assert.NotNull(planoSalvo);
            Assert.Equal("Plano Corporativo", planoSalvo.Nome);
            Assert.Equal(499.90m, planoSalvo.Preco);
            Assert.Equal(2, planoSalvo.Modulos.Count);
            Assert.Contains(planoSalvo.Modulos, m => m.NomeModulo == "Estoque");
            Assert.Contains(planoSalvo.Modulos, m => m.NomeModulo == "Financeiro");
        }

        [Fact]
        public async Task Deve_Gerar_Fatura_Via_Handler_Com_Sucesso()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-y");
            var currentUser = new TestCurrentUser("user-2");
            var context = CreateInMemoryContext("db_fatura_success", "tenant-y", "user-2");

            // Cria cliente na base para poder faturar
            var cliente = new Cliente("Cliente Faturamento", "00.000.000/0001-00", "financeiro@cliente.com", Guid.NewGuid(), "tenant-y", "user-2");
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            var handler = new GerarFaturaCommandHandler(context, tenantProvider, currentUser);
            var command = new GerarFaturaCommand(cliente.Id, 150.00m, DateTime.UtcNow.AddDays(15));

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            
            var faturaSalva = await context.Faturas.FirstOrDefaultAsync();
            Assert.NotNull(faturaSalva);
            Assert.Equal(cliente.Id, faturaSalva.ClienteId);
            Assert.Equal(150.00m, faturaSalva.Valor);
            Assert.Equal(FaturaStatus.Pendente, faturaSalva.Status);
        }

        [Fact]
        public async Task Nao_Deve_Gerar_Fatura_Para_Cliente_Inexistente()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-y");
            var currentUser = new TestCurrentUser("user-2");
            var context = CreateInMemoryContext("db_fatura_failure", "tenant-y", "user-2");

            var handler = new GerarFaturaCommandHandler(context, tenantProvider, currentUser);
            var command = new GerarFaturaCommand(Guid.NewGuid(), 150.00m, DateTime.UtcNow.AddDays(15));

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Cliente não encontrado"));
        }

        [Fact]
        public async Task Deve_Baixar_Fatura_Via_Handler_Com_Sucesso()
        {
            // Arrange
            var currentUser = new TestCurrentUser("user-3");
            var context = CreateInMemoryContext("db_fatura_baixar", "tenant-z", "user-3");

            var fatura = new Fatura(Guid.NewGuid(), 250.00m, DateTime.UtcNow.AddDays(10), "tenant-z", "user-3");
            context.Faturas.Add(fatura);
            await context.SaveChangesAsync();

            var handler = new BaixarFaturaCommandHandler(context, currentUser);
            var command = new BaixarFaturaCommand(fatura.Id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            var faturaBaixada = await context.Faturas.FirstOrDefaultAsync();
            Assert.NotNull(faturaBaixada);
            Assert.Equal(FaturaStatus.Paga, faturaBaixada.Status);
            Assert.NotNull(faturaBaixada.DataPagamento);
            Assert.Equal("user-3", faturaBaixada.AlteradoPor);
        }

        [Fact]
        public async Task Deve_Obter_Clientes_No_Sync_Delta_Corretamente()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName, "tenant-sync", "user-sync");

            var plano = new Plano("Plano Sync", 100m, "tenant-sync", "user-sync");
            context.Planos.Add(plano);

            var cliente1 = new Cliente("Cliente Ativo", "11111111000111", "ativo@sync.com", plano.Id, "tenant-sync", "user-sync");
            var cliente2 = new Cliente("Cliente Deletado", "22222222000122", "deletado@sync.com", plano.Id, "tenant-sync", "user-sync");
            context.Clientes.Add(cliente1);
            context.Clientes.Add(cliente2);
            await context.SaveChangesAsync();

            // Deleta logicamente o cliente 2
            cliente2.Deletar("user-sync");
            await context.SaveChangesAsync();

            var tenantProvider = new TestTenantProvider("tenant-sync");
            var queryHandler = new ObterClientesSyncQueryHandler(context, tenantProvider);

            // Act: busca itens alterados desde uma hora atrás
            var since = DateTime.UtcNow.AddHours(-1);
            var result = await queryHandler.Handle(new ObterClientesSyncQuery(since), CancellationToken.None);

            // Assert: deve retornar ambos os clientes (um ativo e um deletado)
            var lista = result.ToList();
            Assert.Equal(2, lista.Count);
            
            var DtoAtivo = lista.FirstOrDefault(c => c.Id == cliente1.Id);
            Assert.NotNull(DtoAtivo);
            Assert.False(DtoAtivo.Deletado);

            var DtoDeletado = lista.FirstOrDefault(c => c.Id == cliente2.Id);
            Assert.NotNull(DtoDeletado);
            Assert.True(DtoDeletado.Deletado);
        }

        [Fact]
        public async Task Deve_Processar_Webhook_Mercado_Pago_Real_E_Conciliar_Pix_Com_Tarifa_Real()
        {
            // 1.08A — motor unificado: valida x-signature real do MP, lê data.id como payment id,
            // consulta o gateway (tarifa REAL) e resolve a fatura por external_reference.
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName, "tenant-webhook", "user-webhook");

            const string secret = "meusegredo-cifrado"; // cofre identidade nos testes
            var config = new ConfiguracaoGatewayPagamento(
                EProvedorGateway.MercadoPago, EAmbienteGateway.Sandbox,
                accessTokenCifrado: "token-cifrado", publicKey: null, webhookSecretCifrado: secret,
                moeda: "BRL", notificationUrl: null, tenantAlvo: null, ativo: true, criadoPor: "user-webhook");
            context.ConfiguracoesGatewayPagamento.Add(config);

            var fatura = new Fatura(Guid.NewGuid(), 150.00m, DateTime.UtcNow.AddDays(5), "tenant-webhook", "user-webhook");
            context.Faturas.Add(fatura);
            await context.SaveChangesAsync();

            const string paymentId = "1234567890";
            // O gateway devolve o external_reference = Id da fatura, com tarifa real do MP.
            var gateway = new FakePaymentGateway(status: "approved", valor: 150.00m, tarifa: 4.35m,
                externalReference: fatura.Id.ToString(), liquido: 145.65m);
            var cofre = new IdentitySegredoCofre();

            var handler = new ProcessarWebhookPagamentoCommandHandler(context, gateway, cofre);

            var command = BuildWebhookCommand(secret, paymentId, "payment.updated");

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso, string.Join(",", result.Erros ?? new List<string>()));

            var faturaAtualizada = await context.Faturas.FindAsync(fatura.Id);
            Assert.Equal(FaturaStatus.Paga, faturaAtualizada!.Status);
            Assert.NotNull(faturaAtualizada.DataPagamento);

            // Tarifa REAL registrada (não o antigo 0.75 hardcoded).
            var pg = await context.PagamentosFaturas.FirstOrDefaultAsync(p => p.FaturaId == fatura.Id);
            Assert.NotNull(pg);
            Assert.Equal(PagamentoFaturaStatus.Paid, pg!.Status);
            Assert.Equal(4.35m, pg.ValorTarifa);
            Assert.Equal(145.65m, pg.ValorRecebido);
            Assert.Equal(paymentId, pg.IdentificadorPagamento);

            // Recibo gerado.
            var recibo = await context.RecibosPagamento.FirstOrDefaultAsync(r => r.FaturaId == fatura.Id);
            Assert.NotNull(recibo);
        }

        [Fact]
        public async Task Webhook_Mercado_Pago_Deve_Ser_Idempotente_Por_PaymentId()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName, "tenant-webhook", "user-webhook");

            const string secret = "s3cr3t";
            context.ConfiguracoesGatewayPagamento.Add(new ConfiguracaoGatewayPagamento(
                EProvedorGateway.MercadoPago, EAmbienteGateway.Sandbox,
                "tok", null, secret, "BRL", null, null, true, "user-webhook"));

            var fatura = new Fatura(Guid.NewGuid(), 90.00m, DateTime.UtcNow.AddDays(5), "tenant-webhook", "user-webhook");
            context.Faturas.Add(fatura);
            await context.SaveChangesAsync();

            const string paymentId = "99887766";
            var gateway = new FakePaymentGateway("approved", 90.00m, 2.00m, fatura.Id.ToString(), 88.00m);
            var handler = new ProcessarWebhookPagamentoCommandHandler(context, gateway, new IdentitySegredoCofre());
            var command = BuildWebhookCommand(secret, paymentId, "payment.updated");

            var r1 = await handler.Handle(command, CancellationToken.None);
            var r2 = await handler.Handle(command, CancellationToken.None);

            Assert.True(r1.Sucesso);
            Assert.True(r2.Sucesso);
            Assert.Contains("idempot", r2.Mensagem, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(1, await context.PagamentosFaturas.CountAsync(p => p.FaturaId == fatura.Id));
            Assert.Equal(1, await context.RecibosPagamento.CountAsync(r => r.FaturaId == fatura.Id));
        }

        [Fact]
        public async Task Webhook_Mercado_Pago_Deve_Rejeitar_Assinatura_Invalida()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName, "tenant-webhook", "user-webhook");

            context.ConfiguracoesGatewayPagamento.Add(new ConfiguracaoGatewayPagamento(
                EProvedorGateway.MercadoPago, EAmbienteGateway.Sandbox,
                "tok", null, "segredo-certo", "BRL", null, null, true, "user-webhook"));
            await context.SaveChangesAsync();

            var gateway = new FakePaymentGateway("approved", 10m, 0.5m, Guid.NewGuid().ToString(), 9.5m);
            var handler = new ProcessarWebhookPagamentoCommandHandler(context, gateway, new IdentitySegredoCofre());

            // Assinatura calculada com o segredo ERRADO → deve ser rejeitada.
            var command = BuildWebhookCommand("segredo-errado", "5555", "payment.updated");
            var result = await handler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("inválida", StringComparison.OrdinalIgnoreCase));
        }


        [Fact]
        public void Deve_Criar_PerfilUsuario_Valido_Com_Permissoes_Corretamente()
        {
            // Arrange
            var perfil = new PerfilColaborador("usr-99", "João Da Silva", "joao@epros.com", "Vendedor", "Comercial", 10.00m, "tenant-1", "user-admin");
            
            // Act
            perfil.AdicionarPermissao("Vendas", "Ler", true, "user-admin");
            perfil.AdicionarPermissao("Vendas", "Criar", false, "user-admin");

            // Assert
            Assert.True(perfil.IsValid);
            Assert.Equal("usr-99", perfil.UserId);
            Assert.Equal("João Da Silva", perfil.Nome);
            Assert.Equal("Vendedor", perfil.Cargo);
            Assert.Equal(10.00m, perfil.LimiteDesconto);
            Assert.Equal(2, perfil.Permissoes.Count);
            Assert.Contains(perfil.Permissoes, p => p.Recurso == "Vendas" && p.Acao == "Ler" && p.Permitido);
            Assert.Contains(perfil.Permissoes, p => p.Recurso == "Vendas" && p.Acao == "Criar" && !p.Permitido);
        }

        [Fact]
        public async Task Deve_Negar_Acesso_Se_Perfil_Inexistente_Ou_Inativo_No_AbacFilter()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName, "tenant-abac", "user-none");
            var currentUser = new TestCurrentUser("user-none");
            var tenantProvider = new TestTenantProvider("tenant-abac");
            var filter = new AbacFilter("Faturas", "Cancelar", context, currentUser, tenantProvider);

            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var filterContext = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());

            // Act
            await filter.OnAuthorizationAsync(filterContext);

            // Assert
            Assert.NotNull(filterContext.Result);
            Assert.IsType<ForbidResult>(filterContext.Result);
        }

        [Fact]
        public async Task Deve_Permitir_Acesso_Para_Administrador_No_AbacFilter()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName, "tenant-abac", "user-admin");
            
            var perfil = new PerfilColaborador("user-admin", "Admin User", "admin@epros.com", "Administrador", "TI", 100.00m, "tenant-abac", "system");
            context.PerfisUsuarios.Add(perfil);
            await context.SaveChangesAsync();

            var currentUser = new TestCurrentUser("user-admin");
            var tenantProvider = new TestTenantProvider("tenant-abac");
            var filter = new AbacFilter("Faturas", "Cancelar", context, currentUser, tenantProvider);

            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var filterContext = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());

            // Act
            await filter.OnAuthorizationAsync(filterContext);

            // Assert - Sem Result significa que passou direto pelo filtro de autorização
            Assert.Null(filterContext.Result);
        }

        [Fact]
        public async Task Deve_Validar_Limite_De_Desconto_Dinamicamente_No_AbacFilter()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName, "tenant-abac", "user-vendedor");
            
            var perfil = new PerfilColaborador("user-vendedor", "Vendedor 1", "vendedor@epros.com", "Vendedor", "Comercial", 10.00m, "tenant-abac", "system");
            // Adiciona permissão para aplicar desconto
            perfil.AdicionarPermissao("Desconto", "Aplicar", true, "system");
            context.PerfisUsuarios.Add(perfil);
            await context.SaveChangesAsync();

            var currentUser = new TestCurrentUser("user-vendedor");
            var tenantProvider = new TestTenantProvider("tenant-abac");

            // Caso 1: Desconto dentro do limite (5%)
            var httpContextOk = new DefaultHttpContext();
            httpContextOk.Request.QueryString = new QueryString("?percentual=5");
            var actionContextOk = new ActionContext(httpContextOk, new RouteData(), new ActionDescriptor());
            var filterContextOk = new AuthorizationFilterContext(actionContextOk, new List<IFilterMetadata>());
            var filterOk = new AbacFilter("Desconto", "Aplicar", context, currentUser, tenantProvider);

            // Act 1
            await filterOk.OnAuthorizationAsync(filterContextOk);

            // Assert 1 (Ok - passou)
            Assert.Null(filterContextOk.Result);

            // Caso 2: Desconto acima do limite (15% quando limite é 10%)
            var httpContextNegado = new DefaultHttpContext();
            httpContextNegado.Request.QueryString = new QueryString("?percentual=15");
            var actionContextNegado = new ActionContext(httpContextNegado, new RouteData(), new ActionDescriptor());
            var filterContextNegado = new AuthorizationFilterContext(actionContextNegado, new List<IFilterMetadata>());
            var filterNegado = new AbacFilter("Desconto", "Aplicar", context, currentUser, tenantProvider);

            // Act 2
            await filterNegado.OnAuthorizationAsync(filterContextNegado);

            // Assert 2 (Negado - 403 JsonResult)
            Assert.NotNull(filterContextNegado.Result);
            var jsonResult = Assert.IsType<JsonResult>(filterContextNegado.Result);
            Assert.Equal(403, jsonResult.StatusCode);
        }

        [Fact]
        public async Task Deve_Filtrar_Menu_Dinamico_Por_Plano_E_Permissoes()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName, "tenant-menu", "user-menu");

            // Configura plano com Financeiro e Estoque
            var plano = new Plano("Plano Completo", 300.00m, "tenant-menu", "system");
            plano.AdicionarModulo("Financeiro", "system");
            plano.AdicionarModulo("Estoque", "system");
            context.Planos.Add(plano);

            // Configura cliente/tenant
            var cliente = new Cliente("Tenant Menu Ltda", "11.111.111/0001-11", "menu@tenant.com", plano.Id, "tenant-menu", "system");
            context.Clientes.Add(cliente);

            // Configura perfil do usuário com acesso a apenas Financeiro e ContasPagar
            var perfil = new PerfilColaborador("user-menu", "Financeiro User", "financeiro@epros.com", "Operador", "Financeiro", 0.00m, "tenant-menu", "system");
            perfil.AdicionarPermissao("Financeiro", "Ler", true, "system");
            perfil.AdicionarPermissao("ContasPagar", "Ler", true, "system");
            context.PerfisUsuarios.Add(perfil);
            await context.SaveChangesAsync();

            var tenantProvider = new TestTenantProvider("tenant-menu");
            var queryHandler = new ObterMenuDinamicoQueryHandler(context, tenantProvider);

            // Act
            var result = await queryHandler.Handle(new ObterMenuDinamicoQuery("user-menu"), CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var menus = Assert.IsType<List<MenuDinamicoDto>>(result.Dados);
            
            // Deve conter Financeiro (pois está no plano e tem permissão)
            Assert.Contains(menus, m => m.Modulo == "Financeiro");
            // Não deve conter Estoque (está no plano mas não tem permissão)
            Assert.DoesNotContain(menus, m => m.Modulo == "Estoque");

            var menuFinanceiro = menus.First(m => m.Modulo == "Financeiro");
            Assert.Contains(menuFinanceiro.SubMenus, s => s == "Contas a Pagar");
            Assert.DoesNotContain(menuFinanceiro.SubMenus, s => s == "Contas a Receber");
        }

        [Fact]
        public void Deve_Criar_Contrato_Valido_Com_Itens_Corretamente()
        {
            // Arrange & Act
            var contrato = new Contrato(Guid.NewGuid(), 10, DateTime.UtcNow, null, "tenant-1", "user-admin");
            contrato.AdicionarItem("Hospedagem Cloud", 1, 150.00m, "user-admin");
            contrato.AdicionarItem("Suporte Técnico", 2, 50.00m, "user-admin");

            // Assert
            Assert.True(contrato.IsValid);
            Assert.Equal(10, contrato.DiaVencimento);
            Assert.Equal(2, contrato.Itens.Count);
            Assert.Equal(250.00m, contrato.ValorRecorrente);
            Assert.True(contrato.Ativo);
        }

        [Fact]
        public void Nao_Deve_Criar_Contrato_Com_DiaVencimento_Invalido()
        {
            // Act
            var contratoVencimentoInvalido = new Contrato(Guid.NewGuid(), 29, DateTime.UtcNow, null, "tenant-1", "user-admin");

            // Assert
            Assert.False(contratoVencimentoInvalido.IsValid);
            Assert.Contains(contratoVencimentoInvalido.Notifications, n => n.Key == "DiaVencimento");
        }

        [Fact]
        public async Task Deve_Criar_Contrato_Via_Handler_Com_Sucesso()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-contrato");
            var currentUser = new TestCurrentUser("user-1");
            var context = CreateInMemoryContext("db_contrato_handler", "tenant-contrato", "user-1");
            
            // Adiciona cliente para poder associar o contrato
            var cliente = new Cliente("Cliente Contrato", "00.000.000/0001-00", "cliente@contrato.com", Guid.NewGuid(), "tenant-contrato", "user-1");
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            var handler = new CriarContratoCommandHandler(context, tenantProvider, currentUser);
            var command = new CriarContratoCommand(
                cliente.Id,
                15,
                DateTime.UtcNow,
                null,
                new List<CriarContratoItemDto>
                {
                    new("Licença ERP", 5, 80.00m),
                    new("Backup Diário", 1, 50.00m)
                }
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            
            var contratoSalvo = await context.Contratos.Include(c => c.Itens).FirstOrDefaultAsync();
            Assert.NotNull(contratoSalvo);
            Assert.Equal(cliente.Id, contratoSalvo.ClienteId);
            Assert.Equal(15, contratoSalvo.DiaVencimento);
            Assert.Equal(2, contratoSalvo.Itens.Count);
            Assert.Equal(450.00m, contratoSalvo.ValorRecorrente);
        }

        [Fact]
        public async Task Deve_Faturar_Contratos_No_Dia_De_Vencimento_Corretamente()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName, "tenant-faturamento", "user-billing");
            var tenantProvider = new TestTenantProvider("tenant-faturamento");
            var currentUser = new TestCurrentUser("user-billing");

            var cliente = new Cliente("Cliente Faturar", "00.000.000/0001-00", "cliente@faturar.com", Guid.NewGuid(), "tenant-faturamento", "user-billing");
            context.Clientes.Add(cliente);

            // Contrato com vencimento dia 10, iniciado em data FIXA anterior às datas de referência do teste.
            // Antes usava DateTime.UtcNow.AddMonths(-1): como as datas de referência são fixas (jun/2026),
            // com o passar do tempo a data de início passou a ser POSTERIOR ao dia 10/06, e o contrato
            // deixava de ser elegível (DataInicio <= referencia). Data fixa torna o teste determinístico.
            var contrato = new Contrato(cliente.Id, 10, new DateTime(2026, 5, 1), null, "tenant-faturamento", "user-billing");
            contrato.AdicionarItem("Serviço Recorrente", 1, 300.00m, "user-billing");
            context.Contratos.Add(contrato);
            await context.SaveChangesAsync();

            var handler = new ProcessarFaturamentoRecorrenteCommandHandler(context, tenantProvider, currentUser);

            // Caso 1: Executa antes do dia 10 (ex: dia 5) -> Não deve faturar
            var dataDia5 = new DateTime(2026, 6, 5);
            var resultDia5 = await handler.Handle(new ProcessarFaturamentoRecorrenteCommand(dataDia5), CancellationToken.None);
            Assert.True(resultDia5.Sucesso);
            Assert.Equal(0, ((Dictionary<string, int>)resultDia5.Dados!)["TotalFaturasGeradas"]);

            // Caso 2: Executa no dia 10 -> Deve faturar com sucesso
            var dataDia10 = new DateTime(2026, 6, 10);
            var resultDia10 = await handler.Handle(new ProcessarFaturamentoRecorrenteCommand(dataDia10), CancellationToken.None);
            Assert.True(resultDia10.Sucesso);
            Assert.Equal(1, ((Dictionary<string, int>)resultDia10.Dados!)["TotalFaturasGeradas"]);

            // Verifica se a fatura foi salva na base com o valor e vencimento corretos
            var faturaSalva = await context.Faturas.FirstOrDefaultAsync();
            Assert.NotNull(faturaSalva);
            Assert.Equal(cliente.Id, faturaSalva.ClienteId);
            Assert.Equal(300.00m, faturaSalva.Valor);
            Assert.Equal(dataDia10, faturaSalva.DataVencimento);
            Assert.Equal(FaturaStatus.Pendente, faturaSalva.Status);

            // Verifica se o contrato atualizou a data do último faturamento
            var contratoAtualizado = await context.Contratos.FindAsync(contrato.Id);
            Assert.NotNull(contratoAtualizado);
            Assert.NotNull(contratoAtualizado!.UltimoFaturamento);
            Assert.Equal(dataDia10, contratoAtualizado.UltimoFaturamento.Value);
        }

        [Fact]
        public async Task Nao_Deve_Faturar_Contrato_Se_Ja_Faturado_No_Mes()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName, "tenant-faturamento-duplicado", "user-billing");
            var tenantProvider = new TestTenantProvider("tenant-faturamento-duplicado");
            var currentUser = new TestCurrentUser("user-billing");

            var cliente = new Cliente("Cliente Faturar Duplicado", "00.000.000/0001-00", "cliente@faturar.com", Guid.NewGuid(), "tenant-faturamento-duplicado", "user-billing");
            context.Clientes.Add(cliente);

            // Contrato já faturado no mês corrente (referência do último faturamento no dia 10 de Junho de 2026)
            var contrato = new Contrato(cliente.Id, 10, DateTime.UtcNow.AddMonths(-1), null, "tenant-faturamento-duplicado", "user-billing");
            contrato.AdicionarItem("Serviço Recorrente", 1, 200.00m, "user-billing");
            contrato.AtualizarFaturamento(new DateTime(2026, 6, 10), "user-billing");
            context.Contratos.Add(contrato);
            await context.SaveChangesAsync();

            var handler = new ProcessarFaturamentoRecorrenteCommandHandler(context, tenantProvider, currentUser);

            // Act: tenta faturar novamente no dia 15 de Junho de 2026
            var dataDia15 = new DateTime(2026, 6, 15);
            var result = await handler.Handle(new ProcessarFaturamentoRecorrenteCommand(dataDia15), CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            Assert.Equal(0, ((Dictionary<string, int>)result.Dados!)["TotalFaturasGeradas"]); // Nenhuma fatura deve ser gerada duplicada
        }

        [Fact]
        public async Task Deve_Criar_Cliente_Via_Handler_Com_Sucesso()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-h");
            var currentUser = new TestCurrentUser("user-h");
            using var context = CreateInMemoryContext("db_cliente_handler_success", "tenant-h", "user-h");
            var handler = new CriarClienteCommandHandler(context, tenantProvider, currentUser);
            var planoId = Guid.NewGuid();
            var command = new CriarClienteCommand("Inquilino S.A.", "12.345.678/0001-99", "admin@inquilino.com", planoId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            Assert.NotNull(result.Dados);

            var clienteSalvo = await context.Clientes.FirstOrDefaultAsync();
            Assert.NotNull(clienteSalvo);
            Assert.Equal("Inquilino S.A.", clienteSalvo.RazaoSocial);
            Assert.Equal("12.345.678/0001-99", clienteSalvo.Cnpj);
            Assert.Equal("admin@inquilino.com", clienteSalvo.Email);
            Assert.Equal(planoId, clienteSalvo.PlanoId);
            Assert.Equal("tenant-h", clienteSalvo.TenantId);
            Assert.Equal("user-h", clienteSalvo.CriadoPor);
        }

        [Fact]
        public async Task Nao_Deve_Criar_Cliente_Via_Handler_Se_Invalido()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-h");
            var currentUser = new TestCurrentUser("user-h");
            using var context = CreateInMemoryContext("db_cliente_handler_fail", "tenant-h", "user-h");
            var handler = new CriarClienteCommandHandler(context, tenantProvider, currentUser);
            // Email inválido
            var command = new CriarClienteCommand("Inquilino S.A.", "12.345.678/0001-99", "email-invalido", Guid.NewGuid());

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("E-mail"));

            var clienteSalvo = await context.Clientes.FirstOrDefaultAsync();
            Assert.Null(clienteSalvo); // Não deve persistir no banco
        }

        #endregion

        #region Helpers e Doubles de Teste

        private static ProcessarWebhookPagamentoCommand BuildWebhookCommand(string secret, string paymentId, string action)
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var requestId = Guid.NewGuid().ToString();
            var header = Epros.Modules.GestaoClientes.Infrastructure.Gateways.MercadoPagoWebhookSignature
                .MontarHeader(secret, paymentId, requestId, ts);
            return new ProcessarWebhookPagamentoCommand(action, new WebhookData(paymentId), header, requestId);
        }

        private sealed class IdentitySegredoCofre : ISegredoCofreService
        {
            public Task<string> CriptografarAsync(string valor) => Task.FromResult(valor);
            public Task<string> DescriptografarAsync(string ciphertext) => Task.FromResult(ciphertext);
        }

        private sealed class FakePaymentGateway : Epros.Modules.GestaoClientes.Application.Interfaces.IPaymentGateway
        {
            private readonly string _status;
            private readonly decimal _valor;
            private readonly decimal? _tarifa;
            private readonly string? _externalReference;
            private readonly decimal? _liquido;

            public FakePaymentGateway(string status, decimal valor, decimal? tarifa, string? externalReference, decimal? liquido)
            {
                _status = status; _valor = valor; _tarifa = tarifa; _externalReference = externalReference; _liquido = liquido;
            }

            public Task<Epros.Shared.Application.Models.CommandResult> ConsultarPagamentoAsync(
                string paymentId, ConfiguracaoGatewayPagamento config, CancellationToken cancellationToken = default)
                => Task.FromResult(Epros.Shared.Application.Models.CommandResult.Ok("ok",
                    new Epros.Modules.GestaoClientes.Application.Interfaces.ConsultaPagamentoResultado(
                        paymentId, _status, _valor, _tarifa, DateTime.UtcNow, _externalReference, _liquido)));

            public Task<Epros.Shared.Application.Models.CommandResult> GerarCobrancaPixAsync(
                Fatura fatura, ConfiguracaoGatewayPagamento config,
                Epros.Modules.GestaoClientes.Application.Interfaces.DadosPagador pagador, CancellationToken cancellationToken = default)
                => Task.FromResult(Epros.Shared.Application.Models.CommandResult.Ok("ok"));

            public Task<Epros.Shared.Application.Models.CommandResult> TestarConexaoAsync(
                ConfiguracaoGatewayPagamento config, CancellationToken cancellationToken = default)
                => Task.FromResult(Epros.Shared.Application.Models.CommandResult.Ok("ok"));
        }

        private ContextGestaoClientes CreateInMemoryContext(string databaseName, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            return new ContextGestaoClientes(options, tenantProvider, currentUser);
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
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }

        #endregion
    }
}
