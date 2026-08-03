using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;

namespace Epros.Tests
{
    /// <summary>
    /// CADASTROS_BASE — cobre E-05/P-02 (navegação dos papéis Fornecedor/Vendedor/Comprador/Contador
    /// no root Pessoa, tornando Pessoa a fonte única) e E-01/E-02/E-03 (tamanhos de campo unificados,
    /// enforcement no domínio). Ref: especificacoes/1_CADASTROS_BASE/DECISOES_IMPLANTACAO_V1.md.
    /// </summary>
    public class CadastrosBasePapeisTamanhosTests
    {
        private const string TenantId = "tenant-cad-test";
        private const string Usuario = "user-cad-test";

        private static Pessoa NovaPessoaBase() => new Pessoa(
            ETipoPessoa.PessoaJuridica,
            ETipoIndicadorIe.NaoContribuinte,
            null, null, null, null, null, null, null, null,
            TenantId, Usuario);

        // ===================== E-05 / P-02: navegação de papéis no root =====================

        [Fact]
        public void VincularFornecedor_Define_Navegacao_E_Flag()
        {
            var pessoa = NovaPessoaBase();
            var fornecedor = new PessoaFornecedor(
                pessoa.Id, null, null, true, "SP", false, null, null, null,
                null, null, null, null, null, null, null, TenantId, Usuario);

            pessoa.VincularFornecedor(fornecedor);

            Assert.NotNull(pessoa.PessoaFornecedor);
            Assert.True(pessoa.EhFornecedor);

            pessoa.VincularFornecedor(null);
            Assert.Null(pessoa.PessoaFornecedor);
            Assert.False(pessoa.EhFornecedor);
        }

        [Fact]
        public void VincularVendedorCompradorContador_Define_Navegacoes()
        {
            var pessoa = NovaPessoaBase();
            var vendedor = new PessoaVendedor(pessoa.Id, null, "v@x.com", 1000m, false, null, null, TenantId, Usuario);
            var comprador = new PessoaComprador(pessoa.Id, TenantId, Usuario);
            var contador = new PessoaContador(pessoa.Id, "CRC-123", TenantId, Usuario);

            pessoa.VincularVendedor(vendedor);
            pessoa.VincularComprador(comprador);
            pessoa.VincularContador(contador);

            Assert.Same(vendedor, pessoa.PessoaVendedor);
            Assert.Same(comprador, pessoa.PessoaComprador);
            Assert.Same(contador, pessoa.PessoaContador);
        }

        [Fact]
        public async Task Navegacao_De_Papel_Persiste_E_Recarrega_Via_Contexto()
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase("db_cad_papeis_nav")
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            var tenant = new CadTestTenantProvider();
            var user = new CadTestCurrentUser();

            Guid pessoaId;
            using (var ctx = new ContextGestaoClientes(options, tenant, user))
            {
                var pessoa = NovaPessoaBase();
                pessoaId = pessoa.Id;
                var vendedor = new PessoaVendedor(pessoa.Id, null, "vend@x.com", 500m, true, null, null, TenantId, Usuario);
                pessoa.VincularVendedor(vendedor);
                ctx.Pessoas.Add(pessoa);
                await ctx.SaveChangesAsync();
            }

            using (var ctx = new ContextGestaoClientes(options, tenant, user))
            {
                var recarregada = await ctx.Pessoas
                    .Include(p => p.PessoaVendedor)
                    .FirstOrDefaultAsync(p => p.Id == pessoaId);

                Assert.NotNull(recarregada);
                Assert.NotNull(recarregada!.PessoaVendedor);
                Assert.Equal("vend@x.com", recarregada.PessoaVendedor!.Email);
            }
        }

        // ===================== E-01: razão social/fantasia 250 =====================

        [Fact]
        public void PessoaJuridica_Aceita_RazaoSocial_Ate_250_E_Rejeita_Acima()
        {
            var cnpj = new Cnpj("11222333000181");
            var razao200 = new string('A', 200);
            var pjValida = new PessoaJuridica(Guid.NewGuid(), cnpj, razao200, null, null, null, null, TenantId, Usuario);
            Assert.DoesNotContain(pjValida.Notifications, n => n.Key == nameof(PessoaJuridica.RazaoSocial));

            var razao300 = new string('B', 300);
            var pjInvalida = new PessoaJuridica(Guid.NewGuid(), cnpj, razao300, null, null, null, null, TenantId, Usuario);
            Assert.Contains(pjInvalida.Notifications, n => n.Key == nameof(PessoaJuridica.RazaoSocial));
        }

        // ===================== E-03: identificação estrangeiro 30 =====================

        [Fact]
        public void PessoaEstrangeiro_Aceita_Identificacao_Ate_30_E_Rejeita_Acima()
        {
            var idValido = new string('X', 30);
            var peValido = new PessoaEstrangeiro(Guid.NewGuid(), "Nome", idValido, TenantId, Usuario);
            Assert.DoesNotContain(peValido.Notifications, n => n.Key == nameof(PessoaEstrangeiro.IdentificacaoEstrangeiro));

            var idInvalido = new string('Y', 31);
            var peInvalido = new PessoaEstrangeiro(Guid.NewGuid(), "Nome", idInvalido, TenantId, Usuario);
            Assert.Contains(peInvalido.Notifications, n => n.Key == nameof(PessoaEstrangeiro.IdentificacaoEstrangeiro));
        }

        // ===================== E-02: e-mail de contato 150 =====================

        [Fact]
        public void PessoaContato_Rejeita_Email_Acima_De_150()
        {
            var emailLongo = new string('a', 145) + "@teste.com"; // 155 chars
            var contato = new PessoaContato(
                Guid.NewGuid(), "Fulano", ETipoContatoTelefonico.Comercial, "1130001000",
                ETipoContatoEmail.EnvioNFe, emailLongo, true, TenantId, Usuario);

            Assert.Contains(contato.Notifications, n => n.Key == nameof(PessoaContato.Email));
        }

        private sealed class CadTestTenantProvider : ITenantProvider
        {
            public string GetTenantId() => TenantId;
        }

        private sealed class CadTestCurrentUser : ICurrentUser
        {
            public string? GetUserId() => Usuario;
            public string? GetUserName() => Usuario;
            public string? GetUserEmail() => "user-cad-test@epros.local";
        }
    }
}
