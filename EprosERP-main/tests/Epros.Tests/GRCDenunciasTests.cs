using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Application.Handlers;
using Epros.Modules.GRC.Application.Queries;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes das regras-chave do submodulo GRC-DEN (Investigacoes e Denuncias).
    /// Cobre RN-DEN-001 (resolved_at valido), RN-DEN-004 (categoria inativa),
    /// RN-DEN-005 (resposta interna nao visivel) e RN-DEN-006 (segregacao investigador).
    /// </summary>
    public class GRCDenunciasTests
    {
        private static ContextGRC NovoContexto(string db, string tenant = "tenant-1", string user = "user-1")
        {
            var options = new DbContextOptionsBuilder<ContextGRC>()
                .UseInMemoryDatabase(db)
                .Options;
            return new ContextGRC(options, new TestTenantProvider(tenant), new TestCurrentUser(user));
        }

        [Fact]
        public async Task DEN_Deve_Registrar_Denuncia_Detalhada_Com_Protocolo()
        {
            using var context = NovoContexto(nameof(DEN_Deve_Registrar_Denuncia_Detalhada_Com_Protocolo));
            var handler = new RegistrarDenunciaDetalhadaCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));

            var result = await handler.Handle(
                new RegistrarDenunciaDetalhadaCommand("Assedio", "Relato detalhado do caso", null, "Alta", true),
                CancellationToken.None);

            Assert.True(result.Sucesso);
            var denuncia = await context.Denuncias.FirstAsync();
            Assert.StartsWith("DEN-", denuncia.CodigoAcompanhamento);
            Assert.True(denuncia.Anonima);
            Assert.False(string.IsNullOrEmpty(denuncia.TokenAcompanhamentoHash)); // RN-DEN-007 token anonimo
        }

        [Fact]
        public async Task DEN_Deve_Bloquear_Denuncia_Com_Categoria_Inativa()
        {
            // RN-DEN-004: categoria inativa nao pode ser usada em nova denuncia.
            using var context = NovoContexto(nameof(DEN_Deve_Bloquear_Denuncia_Com_Categoria_Inativa));
            var categoria = new DenunciaCategoria("Fraude", null, "#f00", null, "tenant-1", "user-1");
            categoria.Inativar("user-1");
            context.DenunciaCategorias.Add(categoria);
            await context.SaveChangesAsync();

            var handler = new RegistrarDenunciaDetalhadaCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            var result = await handler.Handle(
                new RegistrarDenunciaDetalhadaCommand("T", "Relato", categoria.Id, null, false),
                CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact]
        public async Task DEN_Deve_Bloquear_Categoria_Duplicada()
        {
            using var context = NovoContexto(nameof(DEN_Deve_Bloquear_Categoria_Duplicada));
            var handler = new CriarCategoriaDenunciaCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            var cmd = new CriarCategoriaDenunciaCommand("Conduta", null, null, null);

            var ok = await handler.Handle(cmd, CancellationToken.None);
            var dup = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(ok.Sucesso);
            Assert.False(dup.Sucesso);
        }

        [Fact]
        public async Task DEN_Deve_Bloquear_Investigador_Em_Conflito_De_Interesse()
        {
            // RN-DEN-006: investigador nao pode ser denunciado nem beneficiario.
            using var context = NovoContexto(nameof(DEN_Deve_Bloquear_Investigador_Em_Conflito_De_Interesse));
            var denuncia = new Denuncia("T", "Relato", null, "Media", false, "tenant-1", "user-1");
            context.Denuncias.Add(denuncia);
            var investigador = Guid.NewGuid();
            // mesma pessoa registrada como denunciada
            context.DenunciaParticipantes.Add(new DenunciaParticipante(denuncia.Id, investigador, "Denunciado", null, false, "tenant-1", "user-1"));
            await context.SaveChangesAsync();

            var handler = new AtribuirInvestigacaoCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            var result = await handler.Handle(new AtribuirInvestigacaoCommand(denuncia.Id, investigador, null), CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Empty(await context.DenunciaInvestigacoes.ToListAsync());
        }

        [Fact]
        public async Task DEN_Deve_Atribuir_Investigacao_Sem_Conflito()
        {
            using var context = NovoContexto(nameof(DEN_Deve_Atribuir_Investigacao_Sem_Conflito));
            var denuncia = new Denuncia("T", "Relato", null, "Media", false, "tenant-1", "user-1");
            denuncia.Triar(null, "Alta", "user-1");
            context.Denuncias.Add(denuncia);
            await context.SaveChangesAsync();

            var handler = new AtribuirInvestigacaoCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            var result = await handler.Handle(new AtribuirInvestigacaoCommand(denuncia.Id, Guid.NewGuid(), DateTime.UtcNow.AddDays(10)), CancellationToken.None);

            Assert.True(result.Sucesso);
            var inv = await context.DenunciaInvestigacoes.FirstAsync();
            Assert.Equal("EmAndamento", inv.Status);
            var atualizada = await context.Denuncias.FirstAsync();
            Assert.Equal("Investigacao", atualizada.Status);
        }

        [Fact]
        public void DEN_Concluir_Com_Data_Invalida_Deve_Falhar()
        {
            // RN-DEN-001: resolved_at deve ser data/hora valida.
            var denuncia = new Denuncia("T", "Relato", null, null, false, "tenant-1", "user-1");
            denuncia.Concluir(DateTime.MinValue, "Parecer", "user-1");
            Assert.False(denuncia.IsValid);
            Assert.Null(denuncia.ResolvedAt);
        }

        [Fact]
        public void DEN_Concluir_Com_Data_Valida_Deve_Encerrar()
        {
            var denuncia = new Denuncia("T", "Relato", null, null, false, "tenant-1", "user-1");
            denuncia.Concluir(DateTime.UtcNow, "Parecer final", "user-1");
            Assert.True(denuncia.IsValid);
            Assert.Equal("Encerrado", denuncia.Status);
            Assert.NotNull(denuncia.ResolvedAt);
        }

        [Fact]
        public async Task DEN_Resposta_Interna_Nao_Deve_Aparecer_Para_Denunciante()
        {
            // RN-DEN-005: resposta interna nao fica visivel ao denunciante.
            using var context = NovoContexto(nameof(DEN_Resposta_Interna_Nao_Deve_Aparecer_Para_Denunciante));
            var denuncia = new Denuncia("T", "Relato", null, null, false, "tenant-1", "user-1");
            context.Denuncias.Add(denuncia);
            await context.SaveChangesAsync();

            var respHandler = new ResponderDenunciaCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            await respHandler.Handle(new ResponderDenunciaCommand(denuncia.Id, "Resposta publica", false), CancellationToken.None);
            await respHandler.Handle(new ResponderDenunciaCommand(denuncia.Id, "Nota interna sigilosa", true), CancellationToken.None);

            var queryHandler = new ObterRespostasDenunciaQueryHandler(context);
            var visiveis = await queryHandler.Handle(new ObterRespostasDenunciaQuery(denuncia.Id, false), CancellationToken.None);
            var todas = await queryHandler.Handle(new ObterRespostasDenunciaQuery(denuncia.Id, true), CancellationToken.None);

            var listaVisiveis = (System.Collections.Generic.List<DenunciaResposta>)visiveis.Dados;
            var listaTodas = (System.Collections.Generic.List<DenunciaResposta>)todas.Dados;
            Assert.Single(listaVisiveis);
            Assert.Equal(2, listaTodas.Count);
        }

        [Fact]
        public async Task DEN_Anexo_De_Investigacao_Deve_Ser_Sigiloso_Por_Padrao()
        {
            // RN-DEN-016: anexos de investigacao sao sigilosos por padrao.
            using var context = NovoContexto(nameof(DEN_Anexo_De_Investigacao_Deve_Ser_Sigiloso_Por_Padrao));
            var denuncia = new Denuncia("T", "Relato", null, null, false, "tenant-1", "user-1");
            context.Denuncias.Add(denuncia);
            await context.SaveChangesAsync();

            var handler = new AnexarEvidenciaDenunciaCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            var result = await handler.Handle(new AnexarEvidenciaDenunciaCommand(denuncia.Id, null, Guid.NewGuid(), true), CancellationToken.None);

            Assert.True(result.Sucesso);
            var anexo = await context.DenunciaAnexos.FirstAsync();
            Assert.True(anexo.Sigiloso);
        }

        [Fact]
        public async Task DEN_Parametro_Deve_Atualizar_Se_Chave_Ja_Existe()
        {
            using var context = NovoContexto(nameof(DEN_Parametro_Deve_Atualizar_Se_Chave_Ja_Existe));
            var handler = new DefinirParametroDenunciaCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));

            await handler.Handle(new DefinirParametroDenunciaCommand("DEN_PERMITIR_ANONIMATO", "true"), CancellationToken.None);
            await handler.Handle(new DefinirParametroDenunciaCommand("DEN_PERMITIR_ANONIMATO", "false"), CancellationToken.None);

            var parametros = await context.DenunciaParametros.ToListAsync();
            Assert.Single(parametros);
            Assert.Equal("false", parametros[0].ValorJson);
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
