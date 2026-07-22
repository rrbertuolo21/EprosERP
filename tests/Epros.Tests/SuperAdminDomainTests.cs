using System;
using Epros.Modules.Aplicativo.Domain.Entities;
using Xunit;

namespace Epros.Tests
{
    public class SuperAdminDomainTests
    {
        [Fact]
        public void UsuarioInterno_Deve_Criar_Com_Sucesso_Quando_Dados_Validos()
        {
            // Arrange & Act
            var usuario = new UsuarioInterno(
                nome: "Operador Siser A",
                email: "operador.a@siser.com",
                senha: "senhaSegura123",
                creatorId: Guid.NewGuid(),
                uniqueId: "opt-123",
                timezone: "America/Sao_Paulo",
                primaryAdmin: false,
                tenantId: "system",
                criadoPor: "system"
            );

            // Assert
            Assert.True(usuario.IsValid);
            Assert.Equal("Operador Siser A", usuario.Nome);
            Assert.Equal("operador.a@siser.com", usuario.Email);
            Assert.False(usuario.PrimaryAdmin);
        }

        [Fact]
        public void UsuarioInterno_Deve_Falhar_Quando_Senha_For_Curta()
        {
            // Arrange & Act
            var usuario = new UsuarioInterno(
                nome: "Operador Siser B",
                email: "operador.b@siser.com",
                senha: "123", // curta
                creatorId: Guid.NewGuid(),
                uniqueId: "opt-124",
                timezone: "America/Sao_Paulo",
                primaryAdmin: false,
                tenantId: "system",
                criadoPor: "system"
            );

            // Assert
            Assert.False(usuario.IsValid);
            Assert.Contains(usuario.Notifications, n => n.Key == "Senha");
        }

        [Fact]
        public void UsuarioInterno_Deve_Alterar_Senha_Com_Sucesso()
        {
            // Arrange
            var usuario = new UsuarioInterno(
                nome: "Operador Siser C",
                email: "operador.c@siser.com",
                senha: "senhaOriginal123",
                creatorId: Guid.NewGuid(),
                uniqueId: "opt-125",
                timezone: "America/Sao_Paulo",
                primaryAdmin: false,
                tenantId: "system",
                criadoPor: "system"
            );

            // Act
            usuario.AlterarSenha("novaSenhaSegura456", "system");

            // Assert
            Assert.True(usuario.IsValid);
            Assert.Equal("novaSenhaSegura456", usuario.Senha);
        }

        [Fact]
        public void UsuarioInterno_Deve_Tornar_Admin_Principal_Com_Sucesso()
        {
            // Arrange
            var usuario = new UsuarioInterno(
                nome: "Operador Siser D",
                email: "operador.d@siser.com",
                senha: "senhaOriginal123",
                creatorId: Guid.NewGuid(),
                uniqueId: "opt-126",
                timezone: "America/Sao_Paulo",
                primaryAdmin: false,
                tenantId: "system",
                criadoPor: "system"
            );

            // Act
            usuario.TornarAdminPrincipal("system");

            // Assert
            Assert.True(usuario.PrimaryAdmin);
        }

        [Theory]
        [InlineData("global")]
        [InlineData("landlord")]
        [InlineData("tenant")]
        [InlineData("gateway")]
        [InlineData("install")]
        public void SystemSetting_Deve_Aceitar_Escopos_Validos(string escopoValido)
        {
            // Arrange & Act
            var setting = new SystemSetting("smtp.port", "587", escopoValido, false, "system", "system");

            // Assert
            Assert.True(setting.IsValid);
            Assert.Equal(escopoValido, setting.Escopo);
        }

        [Fact]
        public void SystemSetting_Deve_Falhar_Quando_Escopo_Invalido()
        {
            // Arrange & Act
            var setting = new SystemSetting("smtp.port", "587", "outro_escopo", false, "system", "system");

            // Assert
            Assert.False(setting.IsValid);
            Assert.Contains(setting.Notifications, n => n.Key == "Escopo");
        }

        [Fact]
        public void ExecucaoMassaGlobal_Deve_Ativar_E_Concluir_Com_Sucesso()
        {
            // Arrange
            var execucao = new ExecucaoMassaGlobal(
                descricao: "Suspender inadimplentes em lote",
                actionPayload: "{}",
                status: "Draft",
                tenantId: "system",
                criadoPor: "operador-criador"
            );
            Assert.Equal("Draft", execucao.Status);

            // Act: Ativar por um aprovador diferente do criador
            var aprovadorId = Guid.NewGuid();
            execucao.Ativar(aprovadorId, aprovadoPorUserId: "operador-aprovador", alteradoPor: "operador-aprovador");

            // Assert
            Assert.True(execucao.IsValid);
            Assert.Equal("Active", execucao.Status);
            Assert.Equal(aprovadorId, execucao.AprovadoPor);

            // Act: Concluir
            execucao.Concluir("system");

            // Assert
            Assert.True(execucao.IsValid);
            Assert.Equal("Completed", execucao.Status);
        }

        [Fact]
        public void ExecucaoMassaGlobal_Deve_Falhar_No_Maker_Checker_Se_Aprovador_For_Igual_Criador()
        {
            // Arrange
            var execucao = new ExecucaoMassaGlobal(
                descricao: "Suspender inadimplentes em lote",
                actionPayload: "{}",
                status: "Draft",
                tenantId: "system",
                criadoPor: "operador-siser-unico"
            );

            // Act: Tenta aprovar usando o mesmo criador
            execucao.Ativar(Guid.NewGuid(), aprovadoPorUserId: "operador-siser-unico", alteradoPor: "operador-siser-unico");

            // Assert
            Assert.False(execucao.IsValid);
            Assert.Contains(execucao.Notifications, n => n.Key == "AprovadoPor");
        }

        [Fact]
        public void ExecucaoMassaGlobal_Deve_Bloquear_Ativacao_Fora_Do_Status_Draft()
        {
            // Arrange
            var execucao = new ExecucaoMassaGlobal(
                descricao: "Suspender inadimplentes em lote",
                actionPayload: "{}",
                status: "Completed",
                tenantId: "system",
                criadoPor: "operador-criador"
            );

            // Act
            execucao.Ativar(Guid.NewGuid(), aprovadoPorUserId: "operador-aprovador", alteradoPor: "operador-aprovador");

            // Assert
            Assert.False(execucao.IsValid);
            Assert.Contains(execucao.Notifications, n => n.Key == "Status");
        }

        [Fact]
        public void ExecucaoMassaGlobal_Deve_Bloquear_Conclusao_Se_Nao_Estiver_Active()
        {
            // Arrange
            var execucao = new ExecucaoMassaGlobal(
                descricao: "Suspender inadimplentes em lote",
                actionPayload: "{}",
                status: "Draft",
                tenantId: "system",
                criadoPor: "operador-criador"
            );

            // Act
            execucao.Concluir("system");

            // Assert
            Assert.False(execucao.IsValid);
            Assert.Contains(execucao.Notifications, n => n.Key == "Status");
        }

        [Theory]
        [InlineData("slug-valido")]
        [InlineData("slug-com-numeros-123")]
        [InlineData("slug")]
        public void CustomPage_Deve_Aceitar_Slugs_Validos(string slugValido)
        {
            // Arrange & Act
            var pagina = new CustomPage(slugValido, "<h1>Conteudo</h1>", "Rascunho", "system", "system");

            // Assert
            Assert.True(pagina.IsValid);
            Assert.Equal(slugValido, pagina.Slug);
        }

        [Theory]
        [InlineData("Slug-Valido-Com-Maiusculas")]
        [InlineData("slug com espacos")]
        [InlineData("slug_com_underline")]
        [InlineData("slug-com-caracter!")]
        public void CustomPage_Deve_Falhar_Com_Slugs_Invalidos(string slugInvalido)
        {
            // Arrange & Act
            var pagina = new CustomPage(slugInvalido, "<h1>Conteudo</h1>", "Rascunho", "system", "system");

            // Assert
            Assert.False(pagina.IsValid);
            Assert.Contains(pagina.Notifications, n => n.Key == "Slug");
        }

        [Fact]
        public void CustomPage_Deve_Controlar_Transicoes_De_Publicacao()
        {
            // Arrange
            var pagina = new CustomPage("sobre-nos", "<h1>Sobre</h1>", "Rascunho", "system", "system");
            Assert.Equal("Rascunho", pagina.Status);

            // Act & Assert (Rascunho -> Publicada)
            pagina.Publicar("system");
            Assert.True(pagina.IsValid);
            Assert.Equal("Publicada", pagina.Status);

            // Act & Assert (Publicada -> Rascunho)
            pagina.DefinirComoRascunho("system");
            Assert.True(pagina.IsValid);
            Assert.Equal("Rascunho", pagina.Status);
        }

        [Fact]
        public void NewsletterSubscriber_Deve_Realizar_OptOut_E_OptIn_Com_Sucesso()
        {
            // Arrange
            var subscriber = new NewsletterSubscriber("contato@cliente.com", "system", "system");
            Assert.True(subscriber.IsValid);
            Assert.True(subscriber.Ativo);

            // Act (Opt-Out)
            subscriber.CancelarInscricao("system");

            // Assert
            Assert.False(subscriber.Ativo);

            // Act (Opt-In)
            subscriber.ReativarInscricao("system");

            // Assert
            Assert.True(subscriber.Ativo);
        }

        [Fact]
        public void ComunicacaoSuperAdmin_Deve_Criar_Com_Sucesso_E_Valores_Padrao()
        {
            // Arrange & Act
            var comunicacao = new ComunicacaoSuperAdmin(
                businessIds: new List<string> { "tenant-a", "tenant-b" },
                assunto: "Manutenção programada",
                mensagem: "Olá {Nome}, teremos uma manutenção hoje no assunto {Assunto}",
                enviadoPor: Guid.NewGuid(),
                status: "Pendente",
                tenantId: "system",
                criadoPor: "system",
                canais: new List<string> { "Email", "WhatsApp" }
            );

            // Assert
            Assert.True(comunicacao.IsValid);
            Assert.Equal("Pendente", comunicacao.Status);
            Assert.Equal(2, comunicacao.BusinessIds.Count);
            Assert.Equal(2, comunicacao.Canais.Count);
            Assert.Contains("Email", comunicacao.Canais);
            Assert.Contains("WhatsApp", comunicacao.Canais);
        }

        [Fact]
        public void ComunicacaoSuperAdmin_Deve_Falhar_Quando_Sem_Destinatarios()
        {
            // Arrange & Act
            var comunicacao = new ComunicacaoSuperAdmin(
                businessIds: new List<string>(),
                assunto: "Assunto",
                mensagem: "Mensagem",
                enviadoPor: Guid.NewGuid(),
                status: "Pendente",
                tenantId: "system",
                criadoPor: "system"
            );

            // Assert
            Assert.False(comunicacao.IsValid);
            Assert.Contains(comunicacao.Notifications, n => n.Key == "BusinessIds");
        }

        [Fact]
        public void ComunicacaoSuperAdmin_Deve_Atualizar_Status_Com_Sucesso()
        {
            // Arrange
            var comunicacao = new ComunicacaoSuperAdmin(
                businessIds: new List<string> { "tenant-a" },
                assunto: "Assunto",
                mensagem: "Mensagem",
                enviadoPor: Guid.NewGuid(),
                status: "Pendente",
                tenantId: "system",
                criadoPor: "system"
            );

            // Act
            comunicacao.AtualizarStatus("Sucesso", "system");

            // Assert
            Assert.True(comunicacao.IsValid);
            Assert.Equal("Sucesso", comunicacao.Status);
        }

        [Fact]
        public void ComunicacaoSuperAdmin_Deve_Falhar_Ao_Atualizar_Para_Status_Invalido()
        {
            // Arrange
            var comunicacao = new ComunicacaoSuperAdmin(
                businessIds: new List<string> { "tenant-a" },
                assunto: "Assunto",
                mensagem: "Mensagem",
                enviadoPor: Guid.NewGuid(),
                status: "Pendente",
                tenantId: "system",
                criadoPor: "system"
            );

            // Act
            comunicacao.AtualizarStatus("Invalido", "system");

            // Assert
            Assert.False(comunicacao.IsValid);
            Assert.Contains(comunicacao.Notifications, n => n.Key == "Status");
        }
    }
}
