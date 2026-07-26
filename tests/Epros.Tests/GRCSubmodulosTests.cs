using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Application.Handlers;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes das regras-chave dos submodulos GRC Frente 1-2:
    /// POL (Politicas), REG (Compliance Regulatorio), RIS (Riscos avancado),
    /// CIA (Controles/Auditoria) e SOD (Segregacao de Funcoes).
    /// </summary>
    public class GRCSubmodulosTests
    {
        private static ContextGRC NovoContexto(string db, string tenant = "tenant-1", string user = "user-1")
        {
            var options = new DbContextOptionsBuilder<ContextGRC>()
                .UseInMemoryDatabase(db)
                .Options;
            return new ContextGRC(options, new TestTenantProvider(tenant), new TestCurrentUser(user));
        }

        // ===================== GRC-POL =====================

        [Fact]
        public async Task POL_Deve_Criar_Politica_Em_Rascunho()
        {
            using var context = NovoContexto(nameof(POL_Deve_Criar_Politica_Em_Rascunho));
            var handler = new CriarPoliticaCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            var command = new CriarPoliticaCommand("POL-001", "Codigo de Conduta", "Politica de conduta corporativa", "Conduta", Guid.NewGuid(), "RH");

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);
            var politica = await context.Politicas.FirstAsync();
            Assert.Equal("POL-001", politica.Codigo);
            Assert.Equal("Rascunho", politica.Status);
        }

        [Fact]
        public async Task POL_Deve_Bloquear_Codigo_Duplicado()
        {
            using var context = NovoContexto(nameof(POL_Deve_Bloquear_Codigo_Duplicado));
            var tenant = new TestTenantProvider("tenant-1");
            var user = new TestCurrentUser("user-1");
            var handler = new CriarPoliticaCommandHandler(context, tenant, user);
            var cmd = new CriarPoliticaCommand("POL-DUP", "T", "D", null, Guid.NewGuid(), null);

            var ok = await handler.Handle(cmd, CancellationToken.None);
            var dup = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(ok.Sucesso);
            Assert.False(dup.Sucesso);
        }

        [Fact]
        public async Task POL_Deve_Bloquear_Aceite_De_Versao_Nao_Publicada()
        {
            using var context = NovoContexto(nameof(POL_Deve_Bloquear_Aceite_De_Versao_Nao_Publicada));
            var tenant = new TestTenantProvider("tenant-1");
            var user = new TestCurrentUser("user-1");

            var politica = new Politica("POL-9", "T", "D", null, Guid.NewGuid(), null, "tenant-1", "user-1");
            var versao = new PoliticaVersao(politica.Id, "1.0", DateTime.UtcNow, null, null, null, "tenant-1", "user-1");
            // versao continua "Rascunho" (nao publicada)
            context.Politicas.Add(politica);
            context.PoliticaVersoes.Add(versao);
            await context.SaveChangesAsync();

            var handler = new RegistrarAceitePoliticaCommandHandler(context, tenant, user);
            var result = await handler.Handle(
                new RegistrarAceitePoliticaCommand(politica.Id, versao.Id, Guid.NewGuid(), "10.0.0.1", "Portal", null),
                CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact]
        public async Task POL_Deve_Registrar_Aceite_E_Bloquear_Duplicado()
        {
            using var context = NovoContexto(nameof(POL_Deve_Registrar_Aceite_E_Bloquear_Duplicado));
            var tenant = new TestTenantProvider("tenant-1");
            var user = new TestCurrentUser("user-1");

            var politica = new Politica("POL-10", "T", "D", null, Guid.NewGuid(), null, "tenant-1", "user-1");
            var versao = new PoliticaVersao(politica.Id, "1.0", DateTime.UtcNow, null, null, null, "tenant-1", "user-1");
            versao.Aprovar("user-1");
            versao.Publicar("user-1");
            context.Politicas.Add(politica);
            context.PoliticaVersoes.Add(versao);
            await context.SaveChangesAsync();

            var handler = new RegistrarAceitePoliticaCommandHandler(context, tenant, user);
            var usuarioAlvo = Guid.NewGuid();

            var primeiro = await handler.Handle(
                new RegistrarAceitePoliticaCommand(politica.Id, versao.Id, usuarioAlvo, "10.0.0.1", "Portal", "Li e concordo"),
                CancellationToken.None);
            var segundo = await handler.Handle(
                new RegistrarAceitePoliticaCommand(politica.Id, versao.Id, usuarioAlvo, "10.0.0.1", "Portal", "Li e concordo"),
                CancellationToken.None);

            Assert.True(primeiro.Sucesso);
            Assert.False(segundo.Sucesso); // RN-POL-005: um aceite por usuario/versao/tenant
            Assert.Single(await context.PoliticaAceites.ToListAsync());
        }

        // ===================== GRC-REG =====================

        [Fact]
        public async Task REG_Deve_Cadastrar_Certificado_Digital_A1()
        {
            using var context = NovoContexto(nameof(REG_Deve_Cadastrar_Certificado_Digital_A1));
            var handler = new CadastrarCertificadoDigitalCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            var command = new CadastrarCertificadoDigitalCommand(Guid.NewGuid(), "12345678000199", "SERIAL-1", "A1", "Empresa", DateTime.UtcNow.AddYears(1));

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);
            var cert = await context.CertificadosDigitais.FirstAsync();
            Assert.Equal("A1", cert.Tipo);
            Assert.Equal("Rascunho", cert.Status);
        }

        [Fact]
        public void REG_Deve_Rejeitar_Tipo_Certificado_Invalido()
        {
            var cert = new CertificadoDigital(Guid.NewGuid(), "12345678000199", "S", "A9", "Empresa", DateTime.UtcNow.AddYears(1), "tenant-1", "user-1");
            Assert.False(cert.IsValid);
        }

        // ===================== GRC-RIS =====================

        [Fact]
        public async Task RIS_Deve_Avaliar_Risco_E_Calcular_Score()
        {
            using var context = NovoContexto(nameof(RIS_Deve_Avaliar_Risco_E_Calcular_Score));
            var risco = new RiscoCorporativo("Risco T", "Desc", "Operacional", 2, 2, "tenant-1", "user-1");
            context.RiscosCorporativos.Add(risco);
            await context.SaveChangesAsync();

            var handler = new AvaliarRiscoCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            var result = await handler.Handle(new AvaliarRiscoCommand(risco.Id, 4, 5, "Justificativa"), CancellationToken.None);

            Assert.True(result.Sucesso);
            var avaliacao = await context.AvaliacoesRisco.FirstAsync();
            Assert.Equal(20, avaliacao.Score);
        }

        [Fact]
        public async Task RIS_Deve_Bloquear_Avaliacao_De_Risco_Inexistente()
        {
            using var context = NovoContexto(nameof(RIS_Deve_Bloquear_Avaliacao_De_Risco_Inexistente));
            var handler = new AvaliarRiscoCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            var result = await handler.Handle(new AvaliarRiscoCommand(Guid.NewGuid(), 3, 3, null), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        // ===================== GRC-CIA =====================

        [Fact]
        public async Task CIA_Teste_NaoConforme_Deve_Exigir_Achado()
        {
            using var context = NovoContexto(nameof(CIA_Teste_NaoConforme_Deve_Exigir_Achado));
            var controle = new ControleInterno("CTRL-1", "Nome", "Desc", "Mensal", "tenant-1", "user-1");
            context.ControlesInternos.Add(controle);
            await context.SaveChangesAsync();

            var handler = new RegistrarTesteControleCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            var result = await handler.Handle(new RegistrarTesteControleCommand(controle.Id, null, "NaoConforme", "Falha detectada"), CancellationToken.None);

            Assert.True(result.Sucesso);
            var teste = await context.TestesControle.FirstAsync();
            Assert.True(teste.ExigeAchado());
        }

        [Fact]
        public async Task CIA_Deve_Bloquear_Plano_Auditoria_Codigo_Duplicado()
        {
            using var context = NovoContexto(nameof(CIA_Deve_Bloquear_Plano_Auditoria_Codigo_Duplicado));
            var handler = new CriarPlanoAuditoriaCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            var cmd = new CriarPlanoAuditoriaCommand("PA-1", "Plano", "Desc", "Anual");

            var ok = await handler.Handle(cmd, CancellationToken.None);
            var dup = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(ok.Sucesso);
            Assert.False(dup.Sucesso);
        }

        // ===================== GRC-SOD =====================

        [Fact]
        public void SOD_Regra_Com_Funcoes_Iguais_Deve_Ser_Invalida()
        {
            var funcao = Guid.NewGuid();
            var regra = new RegraSoD(null, funcao, funcao, "Alta", DateTime.UtcNow, null, "tenant-1", "user-1");
            Assert.False(regra.IsValid);
        }

        [Fact]
        public async Task SOD_Simulacao_Deve_Detectar_Violacao_Quando_Ambas_Funcoes_Presentes()
        {
            using var context = NovoContexto(nameof(SOD_Simulacao_Deve_Detectar_Violacao_Quando_Ambas_Funcoes_Presentes));
            var tenant = new TestTenantProvider("tenant-1");
            var user = new TestCurrentUser("user-1");

            var funcaoA = new FuncaoSoD("F-A", "Aprovar Pagamento", null, "tenant-1", "user-1");
            var funcaoB = new FuncaoSoD("F-B", "Executar Pagamento", null, "tenant-1", "user-1");
            context.FuncoesSoD.AddRange(funcaoA, funcaoB);

            var regra = new RegraSoD(null, funcaoA.Id, funcaoB.Id, "Critica", DateTime.UtcNow.AddDays(-1), null, "tenant-1", "user-1");
            regra.Submeter("user-1");
            regra.Ativar("user-1");
            context.RegrasSoD.Add(regra);
            await context.SaveChangesAsync();

            var handler = new SimularSoDCommandHandler(context, tenant, user);
            var perfil = Guid.NewGuid();
            var result = await handler.Handle(
                new SimularSoDCommand(perfil, null, "Perfil", new List<Guid> { funcaoA.Id, funcaoB.Id }),
                CancellationToken.None);

            Assert.True(result.Sucesso);
            var violacoes = await context.ViolacoesSoD.ToListAsync();
            Assert.Single(violacoes);
            Assert.Equal("Ativa", violacoes[0].Status);
        }

        [Fact]
        public async Task SOD_Simulacao_Sem_Conflito_Nao_Gera_Violacao()
        {
            using var context = NovoContexto(nameof(SOD_Simulacao_Sem_Conflito_Nao_Gera_Violacao));
            var tenant = new TestTenantProvider("tenant-1");
            var user = new TestCurrentUser("user-1");

            var funcaoA = new FuncaoSoD("F-A2", "A", null, "tenant-1", "user-1");
            var funcaoB = new FuncaoSoD("F-B2", "B", null, "tenant-1", "user-1");
            context.FuncoesSoD.AddRange(funcaoA, funcaoB);
            var regra = new RegraSoD(null, funcaoA.Id, funcaoB.Id, "Alta", DateTime.UtcNow.AddDays(-1), null, "tenant-1", "user-1");
            regra.Submeter("user-1");
            regra.Ativar("user-1");
            context.RegrasSoD.Add(regra);
            await context.SaveChangesAsync();

            var handler = new SimularSoDCommandHandler(context, tenant, user);
            // usuario possui apenas funcaoA => sem conflito
            var result = await handler.Handle(
                new SimularSoDCommand(null, Guid.NewGuid(), "Usuario", new List<Guid> { funcaoA.Id }),
                CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Empty(await context.ViolacoesSoD.ToListAsync());
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
