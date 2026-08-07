using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Entities;
using Endereco = Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco;

namespace Epros.Tests
{
    // O EmpresasController agora é fino (apenas IMediator): a lógica de certificados digitais e
    // teste de SMTP migrou para handlers MediatR (UploadCertificadoDigitalCommandHandler,
    // ExcluirCertificadoDigitalCommandHandler, ListarCertificadosEmpresaQueryHandler,
    // TestarEmailEmpresaCommandHandler). Os testes cobrem esses handlers diretamente,
    // preservando a intenção original (validação de certificado/CNPJ, exclusão, listagem, SMTP).
    public class EmpresaCertificadoTests
    {
        private const string TenantId = "tenant-cert-test";
        private const string UsuarioId = "user-cert-test";

        private ContextGestaoClientes CreateInMemoryContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(databaseName)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var tenantProvider = new TestTenantProvider(TenantId);
            var currentUser = new TestCurrentUser(UsuarioId);

            return new ContextGestaoClientes(options, tenantProvider, currentUser);
        }

        private static ITenantProvider Tenant() => new TestTenantProvider(TenantId);
        private static ICurrentUser User() => new TestCurrentUser(UsuarioId);

        private static Empresa NovaEmpresa(string razaoSocial = "Empresa Teste", string cnpj = "12345678000195", Guid? certificadoDigitalId = null)
            => new Empresa(razaoSocial, null, cnpj, null, null, null, null, RegimeTributario.SimplesNacional, RegimeApuracao.Cumulativo, null, null, null, null, null, certificadoDigitalId, null, null, null, null, new Endereco("Rua", "1", null, "Bairro", "01310-100", "Cidade", "SP"), TenantId, UsuarioId);

        // Helper to generate a dummy self-signed certificate bytes
        private (string base64, string password, string subject, string serialNumber) GenerateTestCertificate(string subjectName)
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(10));
            var password = "password123";
            var pfxBytes = cert.Export(X509ContentType.Pfx, password);
            return (Convert.ToBase64String(pfxBytes), password, cert.Subject, cert.SerialNumber);
        }

        [Fact]
        public async Task Deve_Listar_Certificados_Com_Sucesso()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_cert_listar");
            var handler = new ListarCertificadosEmpresaQueryHandler(context, Tenant());

            var empresaId = Guid.NewGuid();
            var cert1 = new EmpresaCertificado(empresaId, Guid.NewGuid(), Guid.NewGuid(), "123456", "CN=Test", "Info", "12345678000195", DateTime.UtcNow, DateTime.UtcNow.AddYears(1), TenantId, UsuarioId);
            var cert2 = new EmpresaCertificado(empresaId, Guid.NewGuid(), Guid.NewGuid(), "789012", "CN=Test2", "Info", "12345678000195", DateTime.UtcNow, DateTime.UtcNow.AddYears(1), TenantId, UsuarioId);
            context.EmpresasCertificados.AddRange(cert1, cert2);
            await context.SaveChangesAsync();

            // Act
            var result = await handler.Handle(new ListarCertificadosEmpresaQuery(empresaId), CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var list = Assert.IsAssignableFrom<IEnumerable<EmpresaCertificado>>(result.Dados);
            Assert.Equal(2, list.Count());
        }

        [Fact]
        public async Task UploadCertificado_Deve_Rejeitar_Senhas_Ou_Arquivos_Em_Branco()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_cert_invalid_inputs");
            var cofre = new FakeSegredoCofreService();
            var handler = new UploadCertificadoDigitalCommandHandler(context, Tenant(), User(), cofre);

            var empresa = NovaEmpresa();
            context.Empresas.Add(empresa);
            await context.SaveChangesAsync();

            // Act & Assert 1: Empty file
            var result1 = await handler.Handle(new UploadCertificadoDigitalCommand(empresa.Id, "", "password"), CancellationToken.None);
            Assert.False(result1.Sucesso);
            Assert.Contains(result1.Erros, e => e.Contains("obrigatórios"));

            // Act & Assert 2: Empty password
            var result2 = await handler.Handle(new UploadCertificadoDigitalCommand(empresa.Id, "base64data", ""), CancellationToken.None);
            Assert.False(result2.Sucesso);
            Assert.Contains(result2.Erros, e => e.Contains("obrigatórios"));
        }

        [Fact]
        public async Task UploadCertificado_Deve_Rejeitar_Certificado_Com_Senha_Incorreta()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_cert_wrong_pwd");
            var cofre = new FakeSegredoCofreService();
            var handler = new UploadCertificadoDigitalCommandHandler(context, Tenant(), User(), cofre);

            var empresa = NovaEmpresa();
            context.Empresas.Add(empresa);
            await context.SaveChangesAsync();

            var (base64, _, _, _) = GenerateTestCertificate("CN=12345678000195");

            // Act: Usando senha incorreta
            var result = await handler.Handle(new UploadCertificadoDigitalCommand(empresa.Id, base64, "senha_errada"), CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Falha ao carregar e validar o certificado"));
        }

        [Fact]
        public async Task UploadCertificado_Deve_Aceitar_E_Salvar_Certificado_Com_Sucesso_E_Associar_A_Empresa()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_cert_success");
            var cofre = new FakeSegredoCofreService();
            var handler = new UploadCertificadoDigitalCommandHandler(context, Tenant(), User(), cofre);

            var empresa = NovaEmpresa("Empresa Teste S.A.");
            context.Empresas.Add(empresa);
            await context.SaveChangesAsync();

            // Geramos certificado auto-assinado com o CNPJ da empresa
            var (base64, password, subject, serial) = GenerateTestCertificate("CN=Empresa Teste S.A.:12345678000195");

            // Act
            var result = await handler.Handle(new UploadCertificadoDigitalCommand(empresa.Id, base64, password), CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            // Verifica se salvou o certificado no banco
            var certificadoSalvo = await context.EmpresasCertificados.FirstOrDefaultAsync(c => c.EmpresaId == empresa.Id);
            Assert.NotNull(certificadoSalvo);
            Assert.Equal("12345678000195", certificadoSalvo!.Cnpj);
            Assert.Equal(serial, certificadoSalvo.Serial);

            // Verifica se a empresa está vinculada ao certificado
            var empresaAtualizada = await context.Empresas.FindAsync(empresa.Id);
            Assert.NotNull(empresaAtualizada);
            Assert.Equal(certificadoSalvo.Id, empresaAtualizada!.CertificadoDigitalId);

            // Verifica se gravou os segredos criptografados
            var segredoCert = await context.ConfiguracoesGlobais.FirstOrDefaultAsync(g => g.Chave == $"certificado.{certificadoSalvo.CertificadoSegredoId}");
            var segredoSenha = await context.ConfiguracoesGlobais.FirstOrDefaultAsync(g => g.Chave == $"senha_certificado.{certificadoSalvo.SenhaSegredoId}");
            Assert.NotNull(segredoCert);
            Assert.NotNull(segredoSenha);
            Assert.True(segredoCert!.EhSegredo);
            Assert.True(segredoSenha!.EhSegredo);
        }

        [Fact]
        public async Task UploadCertificado_Deve_Rejeitar_Se_Cnpj_Nao_Coincidir()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_cert_cnpj_mismatch");
            var cofre = new FakeSegredoCofreService();
            var handler = new UploadCertificadoDigitalCommandHandler(context, Tenant(), User(), cofre);

            var empresa = NovaEmpresa();
            context.Empresas.Add(empresa);
            await context.SaveChangesAsync();

            // Certificado com CNPJ diferente
            var (base64, password, _, _) = GenerateTestCertificate("CN=Outra Empresa:99999999000199");

            // Act
            var result = await handler.Handle(new UploadCertificadoDigitalCommand(empresa.Id, base64, password), CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("não coincide com o CNPJ da empresa"));
        }

        [Fact]
        public async Task ExcluirCertificado_Deve_Impedir_Se_Certificado_Estiver_Ativo_Na_Empresa()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_cert_delete_active");
            var handler = new ExcluirCertificadoDigitalCommandHandler(context, Tenant(), User());

            var certId = Guid.NewGuid();
            var empresa = NovaEmpresa(certificadoDigitalId: certId);
            context.Empresas.Add(empresa);

            var certificado = new EmpresaCertificado(empresa.Id, Guid.NewGuid(), Guid.NewGuid(), "123456", "CN=Test", "Info", "12345678000195", DateTime.UtcNow, DateTime.UtcNow.AddYears(1), TenantId, UsuarioId);
            // Sobrescreve o ID gerado automaticamente para simular o ID ativo
            var idField = typeof(EntidadeSaaSBase).GetProperty("Id");
            idField?.SetValue(certificado, certId);
            context.EmpresasCertificados.Add(certificado);

            await context.SaveChangesAsync();

            // Act
            var result = await handler.Handle(new ExcluirCertificadoDigitalCommand(empresa.Id, certId), CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Não é possível excluir o certificado digital porque ele está atualmente ativo"));
        }

        [Fact]
        public async Task ExcluirCertificado_Deve_Excluir_Com_Sucesso_Se_Nao_Ativo()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_cert_delete_success");
            var handler = new ExcluirCertificadoDigitalCommandHandler(context, Tenant(), User());

            var empresa = NovaEmpresa();
            context.Empresas.Add(empresa);

            var certificado = new EmpresaCertificado(empresa.Id, Guid.NewGuid(), Guid.NewGuid(), "123456", "CN=Test", "Info", "12345678000195", DateTime.UtcNow, DateTime.UtcNow.AddYears(1), TenantId, UsuarioId);
            context.EmpresasCertificados.Add(certificado);
            await context.SaveChangesAsync();

            // Act
            var result = await handler.Handle(new ExcluirCertificadoDigitalCommand(empresa.Id, certificado.Id), CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            // Verifica soft-delete
            var certDeletado = await context.EmpresasCertificados.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == certificado.Id);
            Assert.NotNull(certDeletado);
            Assert.NotNull(certDeletado.DeletadoEm);
        }

        [Fact]
        public async Task TestarEmail_Deve_Retornar_Erro_Se_Configuracao_Ausente()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_test_email_missing");
            var cofre = new FakeSegredoCofreService();
            var handler = new TestarEmailEmpresaCommandHandler(context, Tenant(), cofre);

            // Act
            var result = await handler.Handle(new TestarEmailEmpresaCommand(Guid.NewGuid()), CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("não encontrada"));
        }

        [Fact]
        public async Task TestarEmail_Deve_Retornar_Erro_Se_Configuracao_Invalida()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_test_email_invalid_host");
            var cofre = new FakeSegredoCofreService();
            var handler = new TestarEmailEmpresaCommandHandler(context, Tenant(), cofre);

            // IP de loopback + porta fechada: ConnectionRefused imediato, sem DNS
            // (host inexistente travava o CI em resolução DNS / Dispose de TcpClient pendente).
            var emailConfig = new ConfiguracaoEmail("127.0.0.1", 1, "user", "pass", "remetente@teste.com", TenantId, UsuarioId);
            context.ConfiguracoesEmail.Add(emailConfig);
            await context.SaveChangesAsync();

            // Act
            var result = await handler.Handle(new TestarEmailEmpresaCommand(Guid.NewGuid()), CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            var errorMsg = string.Join(" | ", result.Erros);
            Assert.True(errorMsg.Contains("Falha na validação SMTP") || errorMsg.Contains("Falha na conexão TCP com o servidor SMTP") || errorMsg.Contains("Timeout"),
                $"Expected SMTP or TCP connection failure message, but got: {errorMsg}");
        }

        private class FakeSegredoCofreService : ISegredoCofreService
        {
            public Task<string> CriptografarAsync(string valor) => Task.FromResult(valor);
            public Task<string> DescriptografarAsync(string ciphertext) => Task.FromResult(ciphertext);
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
            public string? GetUserName() => "Certificado Tester";
            public string? GetUserEmail() => "cert@epros.com";
        }
    }
}
