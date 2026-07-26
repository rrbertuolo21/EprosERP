using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Application.Handlers;
using Epros.Modules.Financeiro.Application.Queries;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    public class BancoAndContaAndCartaoTests
    {
        private const string TenantId = "tenant-fin-001";
        private const string UserId = "user-fin-001";
        private static readonly Guid EmpresaId = Guid.NewGuid();

        [Fact(DisplayName = "Banco | Dados válidos deve ser válido")]
        public void CriarBanco_DadosValidos_DeveSerValido()
        {
            var banco = new Banco("001", "Banco do Brasil S.A.", UserId);

            Assert.True(banco.IsValid);
            Assert.Equal("001", banco.Codigo);
            Assert.Equal("Banco do Brasil S.A.", banco.Descricao);
        }

        [Fact(DisplayName = "Banco | Código longo deve ser inválido")]
        public void CriarBanco_CodigoLongo_DeveSerInvalido()
        {
            var banco = new Banco("0001", "Banco Inválido", UserId);

            Assert.False(banco.IsValid);
            Assert.Contains(banco.Notifications, n => n.Key == "Codigo");
        }

        [Fact(DisplayName = "ContaBancaria | Dados válidos deve ser válida")]
        public void CriarContaBancaria_DadosValidos_DeveSerValida()
        {
            var bancoId = Guid.NewGuid();
            var conta = new ContaBancaria(
                EmpresaId,
                bancoId,
                ETipoContaBancaria.ContaCorrente,
                "Principal",
                "Epros ERP Tech Ltda",
                "1234",
                "56789",
                "João Gerente",
                "11999998888",
                "Agência Central",
                "0",
                null,
                TenantId,
                UserId
            );

            Assert.True(conta.IsValid);
            Assert.Equal(bancoId, conta.BancoId);
            Assert.Equal("Principal", conta.Apelido);
        }

        [Fact(DisplayName = "CartaoDeCredito | Dados válidos deve ser válido")]
        public void CriarCartaoDeCredito_DadosValidos_DeveSerValido()
        {
            var contaId = Guid.NewGuid();
            var cartao = new CartaoDeCredito(
                contaId,
                "Cartão Corporativo Visa",
                "Sócio Admin",
                EBandeiraCartao.bcVisa,
                "Limite R$ 10.000",
                TenantId,
                UserId
            );

            Assert.True(cartao.IsValid);
            Assert.Equal(contaId, cartao.ContaBancariaId);
            Assert.Equal("Cartão Corporativo Visa", cartao.Apelido);
        }

        private static Guid GetIdFromCommandResult(CommandResult result)
        {
            var prop = result.Dados.GetType().GetProperty("Id");
            if (prop != null) return (Guid)prop.GetValue(result.Dados)!;
            prop = result.Dados.GetType().GetProperty("id");
            if (prop != null) return (Guid)prop.GetValue(result.Dados)!;
            throw new Exception("Property Id or id not found on result.Dados type: " + result.Dados.GetType().FullName);
        }

        [Fact(DisplayName = "CQRS | Criar Banco, Conta Bancária e Cartão de Crédito")]
        public async Task CQRS_CriarBancoContaCartao_DevePersistirCorretamente()
        {
            using var context = CreateInMemoryContext("db_cqrs_financeiro");

            // 1. Criar Banco
            var cmdBanco = new CriarBancoCommand("033", "Banco Santander");
            var handlerBanco = new CriarBancoCommandHandler(context, new TestCurrentUser(UserId));
            var resultBanco = await handlerBanco.Handle(cmdBanco, CancellationToken.None);

            Assert.True(resultBanco.Sucesso);
            var bancoId = GetIdFromCommandResult(resultBanco);

            // 2. Criar Conta Bancária
            var cmdConta = new CriarContaBancariaCommand(
                EmpresaId,
                bancoId,
                ETipoContaBancaria.ContaCorrente,
                "Santander Empresa",
                "Epros Tech",
                "3456",
                "98765",
                "Giselle",
                "11988887777",
                "Santander Empresas 1",
                "1",
                null
            );
            var handlerConta = new CriarContaBancariaCommandHandler(context, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
            var resultConta = await handlerConta.Handle(cmdConta, CancellationToken.None);

            Assert.True(resultConta.Sucesso);
            var contaId = GetIdFromCommandResult(resultConta);

            // 3. Criar Cartão de Crédito
            var cmdCartao = new CriarCartaoDeCreditoCommand(
                contaId,
                "Santander Platinum",
                "Epros Tech Manager",
                EBandeiraCartao.bcMasterCard,
                "Fatura dia 10"
            );
            var handlerCartao = new CriarCartaoDeCreditoCommandHandler(context, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
            var resultCartao = await handlerCartao.Handle(cmdCartao, CancellationToken.None);

            Assert.True(resultCartao.Sucesso);
            var cartaoId = GetIdFromCommandResult(resultCartao);

            // 4. Verificar no Contexto
            var dbBanco = await context.Bancos.FindAsync(bancoId);
            var dbConta = await context.ContasBancarias.FindAsync(contaId);
            var dbCartao = await context.CartoesDeCredito.FindAsync(cartaoId);

            Assert.NotNull(dbBanco);
            Assert.NotNull(dbConta);
            Assert.NotNull(dbCartao);
            Assert.Equal("Banco Santander", dbBanco.Descricao);
            Assert.Equal("Santander Empresa", dbConta.Apelido);
            Assert.Equal("Santander Platinum", dbCartao.Apelido);
        }

        [Fact(DisplayName = "OFX | Reconciliação automática deve quitar títulos correspondentes")]
        public async Task OFX_ProcessarExtrato_DeveReconciliarContasAberta()
        {
            var dbName = "db_ofx_reconciliation_" + Guid.NewGuid().ToString();

            using (var contextArrange = CreateInMemoryContext(dbName))
            {
                // Banco e Conta Bancária
                var banco = new Banco("001", "Banco do Brasil", UserId);
                contextArrange.Bancos.Add(banco);
                var contaBancaria = new ContaBancaria(
                    EmpresaId, banco.Id, ETipoContaBancaria.ContaCorrente, "Principal BB", "Epros", "1111", "2222", null, null, null, null, null, TenantId, UserId
                );
                contextArrange.ContasBancarias.Add(contaBancaria);

                // Título a Pagar em aberto (modelo fiel)
                var contaPagar = new ContasAPagar(
                    Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Fornecedor de Software",
                    ESituacao.Aberto, DateTime.Today.AddDays(5), DateTime.Today, null,
                    "NF-SOFT", 150.00m, 0m, 0m, 0m, 0m, 1, "Fornecedor de Software", null, TenantId, UserId
                );
                contextArrange.ContasAPagarAgregado.Add(contaPagar);

                // Título a Receber em aberto (modelo fiel)
                var contaReceber = new ContasAReceber(
                    Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Serviço de Consultoria",
                    ESituacao.Aberto, DateTime.Today.AddDays(5), DateTime.Today, null,
                    "NF-CONS", 300.00m, 0m, 0m, 0m, 0m, 1, "Serviço de Consultoria", null, TenantId, UserId
                );
                contextArrange.ContasAReceberAgregado.Add(contaReceber);

                await contextArrange.SaveChangesAsync();
            }

            // Conteúdo OFX mockado
            var ofxConteudo = @"
<OFX>
  <BANKMSGSRSV1>
    <STMTTRNRS>
      <STMTRS>
        <BANKTRANLIST>
          <STMTTRN>
            <TRNTYPE>DEBIT</TRNTYPE>
            <DTPOSTED>20260701120000</DTPOSTED>
            <TRNAMT>-150.00</TRNAMT>
            <FITID>TXN001</FITID>
            <MEMO>PAGAMENTO FORNECEDOR</MEMO>
          </STMTTRN>
          <STMTTRN>
            <TRNTYPE>CREDIT</TRNTYPE>
            <DTPOSTED>20260702120000</DTPOSTED>
            <TRNAMT>300.00</TRNAMT>
            <FITID>TXN002</FITID>
            <MEMO>RECEBIMENTO CLIENTE</MEMO>
          </STMTTRN>
        </BANKTRANLIST>
      </STMTRS>
    </STMTTRNRS>
  </BANKMSGSRSV1>
</OFX>";

            using (var contextAct = CreateInMemoryContext(dbName))
            {
                var contaBancaria = await contextAct.ContasBancarias.FirstAsync();
                var contaPagar = await contextAct.ContasAPagarAgregado.FirstAsync();
                var contaReceber = await contextAct.ContasAReceberAgregado.FirstAsync();

                // Executar Reconciliação OFX
                var cmdOfx = new ProcessarExtratoOfxCommand(contaBancaria.Id, ofxConteudo);
                var handlerOfx = new ProcessarExtratoOfxCommandHandler(contextAct, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
                var result = await handlerOfx.Handle(cmdOfx, CancellationToken.None);

                Assert.True(result.Sucesso);
            }

            using (var contextAssert = CreateInMemoryContext(dbName))
            {
                var dbContaPagar = await contextAssert.ContasAPagarAgregado.FirstAsync();
                var dbContaReceber = await contextAssert.ContasAReceberAgregado.FirstAsync();

                Assert.Equal(ESituacao.Pago, dbContaPagar.Situacao);
                Assert.Equal(ESituacao.Pago, dbContaReceber.Situacao);
            }
        }

        // Helpers de Contexto InMemory
        private ContextFinanceiro CreateInMemoryContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ContextFinanceiro>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            var tenantProvider = new TestTenantProvider(TenantId);
            var currentUser = new TestCurrentUser(UserId);

            var context = new ContextFinanceiro(options, tenantProvider, currentUser);
            context.Database.EnsureCreated();
            return context;
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
    }
}
