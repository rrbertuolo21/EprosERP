using System;
using System.Linq;
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
    /// GRC-REG — certificado com cofre (D-REG-01, segredo nunca em claro), validação local
    /// determinística de pertencimento/validade (D-REG-04) e alertas de vencimento (D-REG-02).
    /// </summary>
    public class GrcComplianceRegTests
    {
        private static ContextGRC NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextGRC>().UseInMemoryDatabase(db).Options;
            return new ContextGRC(options, new FakeTenant("tenant-1"), new FakeUser("user-1"));
        }

        [Fact]
        public async Task Certificado_Deve_Guardar_So_Referencias_De_Cofre_Nao_O_Segredo()
        {
            using var context = NovoContexto(nameof(Certificado_Deve_Guardar_So_Referencias_De_Cofre_Nao_O_Segredo));
            var handler = new CadastrarCertificadoCompletoCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));

            var result = await handler.Handle(new CadastrarCertificadoCompletoCommand(
                Guid.NewGuid(), "12345678000199", "SERIAL-1", "A1", "Empresa", "Cert NFe", "Resumo",
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddYears(1),
                "vault://secret/senha-abc", "vault://path/cert-abc"), CancellationToken.None);

            Assert.True(result.Sucesso);
            var cert = await context.CertificadosDigitais.FirstAsync();
            Assert.Equal("vault://secret/senha-abc", cert.SenhaReferenciaSegura);
            Assert.Equal("vault://path/cert-abc", cert.CaminhoReferenciaSegura);
            Assert.NotNull(cert.ValidadeInicio);
            Assert.NotNull(cert.ValidadeFim);
        }

        [Fact]
        public void Validacao_Local_Deve_Reprovar_Cnpj_Que_Nao_Pertence()
        {
            var cert = new CertificadoDigital(Guid.NewGuid(), "12345678000199", "S", "A1", "Empresa",
                DateTime.UtcNow.AddYears(1), "tenant-1", "user-1");
            cert.DefinirDetalhes("c", "r", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddYears(1), "user-1");

            var falhas = cert.ValidarLocal(empresaExiste: true, cnpjEmpresa: "99999999000100", DateTime.UtcNow);
            Assert.Contains(falhas, f => f.Contains("pertencimento"));
        }

        [Fact]
        public async Task Validacao_Local_Aprovada_Deve_Marcar_Certificado_Validado()
        {
            using var context = NovoContexto(nameof(Validacao_Local_Aprovada_Deve_Marcar_Certificado_Validado));
            var cadastrar = new CadastrarCertificadoCompletoCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            var criado = await cadastrar.Handle(new CadastrarCertificadoCompletoCommand(
                Guid.NewGuid(), "12345678000199", "S", "A1", "Empresa", "c", "r",
                DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddYears(1), "vault://s", "vault://p"), CancellationToken.None);
            Assert.True(criado.Sucesso);
            var cert = await context.CertificadosDigitais.FirstAsync();

            var validar = new ValidarCertificadoLocalCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            var result = await validar.Handle(new ValidarCertificadoLocalCommand(cert.Id, true, "12.345.678/0001-99"), CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal("Validado", (await context.CertificadosDigitais.FirstAsync()).Status);
            Assert.Equal("Sucesso", (await context.ValidacoesCertificado.FirstAsync()).Resultado);
        }

        [Fact]
        public async Task Rotina_De_Alerta_Deve_Marcar_Vencimento_Na_Janela()
        {
            using var context = NovoContexto(nameof(Rotina_De_Alerta_Deve_Marcar_Vencimento_Na_Janela));
            var criar = new CriarVencimentoRegulatorioCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            // Vence em 5 dias -> cai na janela 7 (default 30,15,7).
            await criar.Handle(new CriarVencimentoRegulatorioCommand(
                Guid.NewGuid(), null, "Vencimento certificado", DateTime.UtcNow.AddDays(5), null, null, "Renovar"), CancellationToken.None);
            // Vence em 60 dias -> fora da janela.
            await criar.Handle(new CriarVencimentoRegulatorioCommand(
                Guid.NewGuid(), null, "Vencimento distante", DateTime.UtcNow.AddDays(60), null, null, null), CancellationToken.None);

            var rotina = new AvaliarVencimentosRegulatoriosCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            var result = await rotina.Handle(new AvaliarVencimentosRegulatoriosCommand(), CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal(1, await context.CalendariosRegulatorios.CountAsync(c => c.Status == "Alertado"));
        }

        [Fact]
        public async Task Deve_Definir_Framework_E_Prazo_Da_Obrigacao()
        {
            using var context = NovoContexto(nameof(Deve_Definir_Framework_E_Prazo_Da_Obrigacao));
            var registro = new RegistroRegulatorio("REG-1", "LGPD art. 48", "LGPD", Guid.NewGuid(), "tenant-1", "user-1");
            context.RegistrosRegulatorios.Add(registro);
            await context.SaveChangesAsync();

            var handler = new DefinirObrigacaoRegistroCommandHandler(context, new FakeUser("user-1"));
            var result = await handler.Handle(new DefinirObrigacaoRegistroCommand(
                registro.Id, "Comunicacao de incidente", "LGPD", DateTime.UtcNow, DateTime.UtcNow.AddDays(2)), CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal("LGPD", (await context.RegistrosRegulatorios.FirstAsync()).Framework);
        }

        private class FakeTenant : ITenantProvider
        {
            private readonly string _t;
            public FakeTenant(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class FakeUser : ICurrentUser
        {
            private readonly string _u;
            public FakeUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "test";
            public string? GetUserEmail() => "test@epros.com.br";
        }
    }
}
