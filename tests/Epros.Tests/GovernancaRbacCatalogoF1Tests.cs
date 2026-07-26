using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    public class GovernancaRbacCatalogoF1Tests
    {
        private const string TenantId = "tenant-f1-001";
        private const string UserId = "user-f1-001";

        // ===================== Entidades — Validar() =====================

        [Fact(DisplayName = "ConsentimentoTitular | Dados válidos deve ser válido")]
        public void ConsentimentoTitular_DadosValidos_DeveSerValido()
        {
            var c = new ConsentimentoTitular(Guid.NewGuid(), EFinalidadeDados.Marketing, EBaseLegalPrivacidade.Consentimento, DateTime.UtcNow, "web", TenantId, UserId);
            Assert.True(c.IsValid);
        }

        [Fact(DisplayName = "ConsentimentoTitular | PessoaId vazio deve ser inválido")]
        public void ConsentimentoTitular_PessoaIdVazio_DeveSerInvalido()
        {
            var c = new ConsentimentoTitular(Guid.Empty, EFinalidadeDados.Marketing, EBaseLegalPrivacidade.Consentimento, null, null, TenantId, UserId);
            Assert.False(c.IsValid);
            Assert.Contains(c.Notifications, n => n.Key == "PessoaId");
        }

        [Fact(DisplayName = "IdentificadorFiscal | Dados válidos deve ser válido")]
        public void IdentificadorFiscal_DadosValidos_DeveSerValido()
        {
            var i = new IdentificadorFiscal(Guid.NewGuid(), Guid.NewGuid(), ETipoIdFiscal.CPF, "12345678909", true, TenantId, UserId);
            Assert.True(i.IsValid);
        }

        [Fact(DisplayName = "IdentificadorFiscal | Valor vazio deve ser inválido")]
        public void IdentificadorFiscal_ValorVazio_DeveSerInvalido()
        {
            var i = new IdentificadorFiscal(Guid.NewGuid(), Guid.NewGuid(), ETipoIdFiscal.CPF, "", true, TenantId, UserId);
            Assert.False(i.IsValid);
            Assert.Contains(i.Notifications, n => n.Key == "Valor");
        }

        [Fact(DisplayName = "RelacionamentoParceiro | Origem igual a Destino deve ser inválido")]
        public void RelacionamentoParceiro_OrigemIgualDestino_DeveSerInvalido()
        {
            var id = Guid.NewGuid();
            var r = new RelacionamentoParceiro(id, id, ETipoRelacaoParceiro.MatrizFilial, null, null, TenantId, UserId);
            Assert.False(r.IsValid);
        }

        [Fact(DisplayName = "CandidatoDuplicata | Pessoas iguais deve ser inválido")]
        public void CandidatoDuplicata_PessoasIguais_DeveSerInvalido()
        {
            var id = Guid.NewGuid();
            var c = new CandidatoDuplicata(id, id, 0.9m, TenantId, UserId);
            Assert.False(c.IsValid);
        }

        [Fact(DisplayName = "Papel | Dados válidos deve ser válido")]
        public void Papel_DadosValidos_DeveSerValido()
        {
            var p = new Papel("Gerente", "Gerente", null, true, false, ETipoPapel.Equipe, null, null, TenantId, UserId);
            Assert.True(p.IsValid);
        }

        [Fact(DisplayName = "Papel | Name vazio deve ser inválido")]
        public void Papel_NameVazio_DeveSerInvalido()
        {
            var p = new Papel("", null, null, true, false, null, null, null, TenantId, UserId);
            Assert.False(p.IsValid);
            Assert.Contains(p.Notifications, n => n.Key == "Name");
        }

        [Fact(DisplayName = "Capacidade | Name vazio deve ser inválido")]
        public void Capacidade_NameVazio_DeveSerInvalido()
        {
            var c = new Capacidade("", null, null, null, null, TenantId, UserId);
            Assert.False(c.IsValid);
        }

        [Fact(DisplayName = "NivelUsuario | LevelId zero deve ser inválido")]
        public void NivelUsuario_LevelIdZero_DeveSerInvalido()
        {
            var n = new NivelUsuario(0, "Free", true, 0, 0, 0, false, false, 360, 50, 5, 0, 0, 0, 0, ENivelUsuarioTipo.Free, false, TenantId, UserId);
            Assert.False(n.IsValid);
        }

        [Fact(DisplayName = "Funcionalidade | Dados válidos deve ser válida")]
        public void Funcionalidade_DadosValidos_DeveSerValida()
        {
            var f = new Funcionalidade("PDV", "Ponto de venda", UserId);
            Assert.True(f.IsValid);
        }

        [Fact(DisplayName = "Funcionalidade | Título vazio deve ser inválida")]
        public void Funcionalidade_TituloVazio_DeveSerInvalida()
        {
            var f = new Funcionalidade("", "desc", UserId);
            Assert.False(f.IsValid);
        }

        [Fact(DisplayName = "AddOn | Preço negativo deve ser inválido")]
        public void AddOn_PrecoNegativo_DeveSerInvalido()
        {
            var a = new AddOn("crm", "CRM", -1m, 0m, null, true, false, null, UserId);
            Assert.False(a.IsValid);
        }

        [Fact(DisplayName = "EmpresaGrupo | Nome vazio deve ser inválido")]
        public void EmpresaGrupo_NomeVazio_DeveSerInvalido()
        {
            var g = new EmpresaGrupo("", TenantId, UserId);
            Assert.False(g.IsValid);
        }

        // ===================== Handlers =====================

        [Fact(DisplayName = "RegistrarConsentimentoTitular | Pessoa existente deve persistir")]
        public async Task RegistrarConsentimento_PessoaExistente_DevePersistir()
        {
            var db = Guid.NewGuid().ToString();
            var ctx = CreateContext(db);
            var pessoa = CriarPessoaMinima();
            ctx.Pessoas.Add(pessoa);
            await ctx.SaveChangesAsync();

            var handler = new RegistrarConsentimentoTitularCommandHandler(ctx, Tenant(), User());
            var result = await handler.Handle(new RegistrarConsentimentoTitularCommand(pessoa.Id, EFinalidadeDados.Cobranca, EBaseLegalPrivacidade.Contrato, null, "app"), CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal(1, await ctx.ConsentimentosTitular.CountAsync());
        }

        [Fact(DisplayName = "RegistrarConsentimentoTitular | Pessoa inexistente deve falhar")]
        public async Task RegistrarConsentimento_PessoaInexistente_DeveFalhar()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var handler = new RegistrarConsentimentoTitularCommandHandler(ctx, Tenant(), User());
            var result = await handler.Handle(new RegistrarConsentimentoTitularCommand(Guid.NewGuid(), EFinalidadeDados.Marketing, EBaseLegalPrivacidade.Consentimento, null, null), CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "InativarPessoa | Deve publicar evento pessoa.inativada no Outbox")]
        public async Task InativarPessoa_DevePublicarOutbox()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var pessoa = CriarPessoaMinima();
            ctx.Pessoas.Add(pessoa);
            await ctx.SaveChangesAsync();

            var handler = new InativarPessoaCommandHandler(ctx, Tenant(), User());
            var result = await handler.Handle(new InativarPessoaCommand(pessoa.Id, "gestor decidiu", "127.0.0.1"), CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Contains(await ctx.OutboxMessages.ToListAsync(), o => o.EventType == "pessoa.inativada");
            Assert.Equal(1, await ctx.PessoasHistoricoEstado.CountAsync());
        }

        [Fact(DisplayName = "AnonimizarPessoa | Deve publicar evento pessoa.anonimizada no Outbox")]
        public async Task AnonimizarPessoa_DevePublicarOutbox()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var pessoa = CriarPessoaMinima();
            ctx.Pessoas.Add(pessoa);
            await ctx.SaveChangesAsync();

            var handler = new AnonimizarPessoaCommandHandler(ctx, Tenant(), User());
            var result = await handler.Handle(new AnonimizarPessoaCommand(pessoa.Id), CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Contains(await ctx.OutboxMessages.ToListAsync(), o => o.EventType == "pessoa.anonimizada");
        }

        [Fact(DisplayName = "AnonimizarPessoa | Pessoa inexistente deve falhar")]
        public async Task AnonimizarPessoa_Inexistente_DeveFalhar()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var handler = new AnonimizarPessoaCommandHandler(ctx, Tenant(), User());
            var result = await handler.Handle(new AnonimizarPessoaCommand(Guid.NewGuid()), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "CriarPapel | Dados válidos deve persistir")]
        public async Task CriarPapel_DadosValidos_DevePersistir()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var handler = new CriarPapelCommandHandler(ctx, Tenant(), User());
            var result = await handler.Handle(new CriarPapelCommand("Financeiro", "Financeiro", null, true, false, ETipoPapel.Equipe, null, null, null), CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal(1, await ctx.Papeis.CountAsync());
        }

        [Fact(DisplayName = "CriarPapel | Nome duplicado deve falhar")]
        public async Task CriarPapel_NomeDuplicado_DeveFalhar()
        {
            var db = Guid.NewGuid().ToString();
            var ctx = CreateContext(db);
            var handler = new CriarPapelCommandHandler(ctx, Tenant(), User());
            await handler.Handle(new CriarPapelCommand("Financeiro", null, null, true, false, null, null, null, null), CancellationToken.None);

            var ctx2 = CreateContext(db);
            var handler2 = new CriarPapelCommandHandler(ctx2, Tenant(), User());
            var result = await handler2.Handle(new CriarPapelCommand("Financeiro", null, null, true, false, null, null, null, null), CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "DeletarPapel | Papel de sistema deve falhar")]
        public async Task DeletarPapel_PapelSistema_DeveFalhar()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var papel = new Papel("Root", null, null, false, true, null, null, null, TenantId, UserId);
            ctx.Papeis.Add(papel);
            await ctx.SaveChangesAsync();

            var handler = new DeletarPapelCommandHandler(ctx, Tenant(), User());
            var result = await handler.Handle(new DeletarPapelCommand(papel.Id), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "CriarFuncionalidade | Título duplicado deve falhar")]
        public async Task CriarFuncionalidade_TituloDuplicado_DeveFalhar()
        {
            var db = Guid.NewGuid().ToString();
            var ctx = CreateContext(db);
            var handler = new CriarFuncionalidadeCommandHandler(ctx, User());
            await handler.Handle(new CriarFuncionalidadeCommand("PDV", "desc"), CancellationToken.None);

            var ctx2 = CreateContext(db);
            var handler2 = new CriarFuncionalidadeCommandHandler(ctx2, User());
            var result = await handler2.Handle(new CriarFuncionalidadeCommand("PDV", "outra"), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "CriarEmpresaGrupo | Nome duplicado deve falhar")]
        public async Task CriarEmpresaGrupo_NomeDuplicado_DeveFalhar()
        {
            var db = Guid.NewGuid().ToString();
            var ctx = CreateContext(db);
            var handler = new CriarEmpresaGrupoCommandHandler(ctx, Tenant(), User());
            await handler.Handle(new CriarEmpresaGrupoCommand("Grupo Alpha"), CancellationToken.None);

            var ctx2 = CreateContext(db);
            var handler2 = new CriarEmpresaGrupoCommandHandler(ctx2, Tenant(), User());
            var result = await handler2.Handle(new CriarEmpresaGrupoCommand("Grupo Alpha"), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "ObterAcessosUsuario | Não admin sem perfil deve falhar")]
        public async Task ObterAcessos_NaoAdminSemPerfil_DeveFalhar()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var handler = new ObterAcessosUsuarioQueryHandler(ctx, Tenant());
            var result = await handler.Handle(new ObterAcessosUsuarioQuery(null, false, "joao", "tok", null, null, null), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "ObterAcessosUsuario | Admin deve retornar árvore com bypass")]
        public async Task ObterAcessos_Admin_DeveRetornarArvore()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var menu = new Menu("Cadastros", "box", null, 1, null);
            ctx.Menus.Add(menu);
            await ctx.SaveChangesAsync();

            var handler = new ObterAcessosUsuarioQueryHandler(ctx, Tenant());
            var result = await handler.Handle(new ObterAcessosUsuarioQuery(null, true, "admin", "tok", null, null, null), CancellationToken.None);
            Assert.True(result.Sucesso);
        }

        [Fact(DisplayName = "ResolverModulosAtivos | Retorna apenas módulos habilitados no catálogo")]
        public async Task ResolverModulosAtivos_ApenasHabilitados()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            ctx.AddOns.Add(new AddOn("financeiro", "Financeiro", 0, 0, null, true, false, null, UserId));
            ctx.AddOns.Add(new AddOn("crm", "CRM", 0, 0, null, false, false, null, UserId));
            var usuarioId = Guid.NewGuid();
            ctx.ModulosAtivosUsuario.Add(new ModuloAtivoUsuario(usuarioId, "financeiro", EOrigemModuloAtivo.Avulso, TenantId, UserId));
            ctx.ModulosAtivosUsuario.Add(new ModuloAtivoUsuario(usuarioId, "crm", EOrigemModuloAtivo.Avulso, TenantId, UserId));
            await ctx.SaveChangesAsync();

            var handler = new ResolverModulosAtivosQueryHandler(ctx, Tenant());
            var result = await handler.Handle(new ResolverModulosAtivosQuery(usuarioId), CancellationToken.None);

            Assert.True(result.Sucesso);
            var modulosProp = result.Dados!.GetType().GetProperty("Modulos")!.GetValue(result.Dados);
            var modulos = (System.Collections.Generic.List<string>)modulosProp!;
            Assert.Contains("financeiro", modulos);
            Assert.DoesNotContain("crm", modulos); // crm não habilitado
        }

        // ===================== Helpers =====================

        private static Pessoa CriarPessoaMinima()
            => new Pessoa(ETipoPessoa.PessoaFisica, ETipoIndicadorIe.NaoContribuinte, null, null, null, null, null, null, null, null, TenantId, UserId);

        private ContextGestaoClientes CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ContextGestaoClientes(options, Tenant(), User());
        }

        private static ITenantProvider Tenant() => new TestTenantProvider(TenantId);
        private static ICurrentUser User() => new TestCurrentUser(UserId);

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _t;
            public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _u;
            public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
