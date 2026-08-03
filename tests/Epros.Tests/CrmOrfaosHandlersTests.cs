using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Handlers;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;

namespace Epros.Tests
{
    // Gap CRM (auditoria VENDAS, submódulo 3): Atividades/Webforms/Fidelização/Pix eram órfãs (sem
    // command/handler/endpoint). Estes testes exercem o write-path recém-ligado.
    public class CrmOrfaosHandlersTests
    {
        private const string TenantId = "tenant-crm-orfaos";
        private const string UserId = "user-crm-orfaos";

        private static ContextVendas CreateContext(string dbName)
            => new(new DbContextOptionsBuilder<ContextVendas>().UseInMemoryDatabase(dbName).Options,
                   new FakeTenant(TenantId), new FakeUser(UserId));

        private static (FakeTenant t, FakeUser u) Ids => (new FakeTenant(TenantId), new FakeUser(UserId));

        [Fact]
        public async Task Atividade_Criada_E_Concluida()
        {
            var ctx = CreateContext(nameof(Atividade_Criada_E_Concluida));
            var (t, u) = Ids;
            var criar = await new CriarCrmAtividadeCommandHandler(ctx, t, u).Handle(new CriarCrmAtividadeCommand(
                ECrmEntidadeContexto.Lead, ECrmTipoAtividade.Tarefa, Guid.NewGuid(), null, null, null,
                "Ligar para cliente", "Follow-up", DateTime.UtcNow, "09:00", ECrmPrioridade.Alta,
                ECrmStatusAtividade.EmAndamento, null, null, null, "Retomar proposta", null, null, null, null, null),
                CancellationToken.None);
            Assert.True(criar.Sucesso);
            var id = (Guid)criar.Dados!.GetType().GetProperty("Id")!.GetValue(criar.Dados)!;

            var concluir = await new ConcluirCrmAtividadeCommandHandler(ctx, t, u)
                .Handle(new ConcluirCrmAtividadeCommand(id, "Cliente aceitou"), CancellationToken.None);
            Assert.True(concluir.Sucesso);
            var atividade = await ctx.CrmAtividades.FirstAsync(a => a.Id == id);
            Assert.Equal(ECrmStatusAtividade.Completa, atividade.Status);
            Assert.Equal("Cliente aceitou", atividade.Resultado);
        }

        [Fact]
        public async Task Webform_Criado_Rejeita_Identificador_Duplicado_E_Conta_Submissao()
        {
            var ctx = CreateContext(nameof(Webform_Criado_Rejeita_Identificador_Duplicado_E_Conta_Submissao));
            var (t, u) = Ids;
            CriarCrmWebformCommand Cmd() => new("captura-home", "Fale conosco", "{\"campos\":[]}", null, null, "Enviar", null, null, null, null, false, false);

            var r1 = await new CriarCrmWebformCommandHandler(ctx, t, u).Handle(Cmd(), CancellationToken.None);
            Assert.True(r1.Sucesso);
            var r2 = await new CriarCrmWebformCommandHandler(ctx, t, u).Handle(Cmd(), CancellationToken.None);
            Assert.False(r2.Sucesso); // CRM-018 identificador único por tenant

            var sub = await new RegistrarSubmissaoCrmWebformCommandHandler(ctx, t, u)
                .Handle(new RegistrarSubmissaoCrmWebformCommand("captura-home"), CancellationToken.None);
            Assert.True(sub.Sucesso);
            var webform = await ctx.CrmWebforms.FirstAsync(w => w.IdentificadorUnico == "captura-home");
            Assert.Equal(1, webform.ContadorSubmissoes);
        }

        [Fact]
        public async Task Fidelizacao_Acumula_Pontos_No_Cliente()
        {
            var ctx = CreateContext(nameof(Fidelizacao_Acumula_Pontos_No_Cliente));
            var (t, u) = Ids;
            var criar = await new CriarCrmClienteFidelizadoCommandHandler(ctx, t, u)
                .Handle(new CriarCrmClienteFidelizadoCommand(null, "Maria", "m@x.com", "11999999999", null), CancellationToken.None);
            Assert.True(criar.Sucesso);
            var clienteId = (Guid)criar.Dados!.GetType().GetProperty("Id")!.GetValue(criar.Dados)!;

            await new RegistrarPontuacaoFidelizacaoCommandHandler(ctx, t, u)
                .Handle(new RegistrarPontuacaoFidelizacaoCommand(clienteId, 30m, null, "Compra 1"), CancellationToken.None);
            await new RegistrarPontuacaoFidelizacaoCommandHandler(ctx, t, u)
                .Handle(new RegistrarPontuacaoFidelizacaoCommand(clienteId, 12m, null, "Compra 2"), CancellationToken.None);

            var cliente = await ctx.CrmClientesFidelizados.FirstAsync(c => c.Id == clienteId);
            Assert.Equal(42m, cliente.TotalPontos);
            Assert.Equal(2, await ctx.CrmPontuacoesFidelizacao.CountAsync(p => p.ClienteFidelizadoId == clienteId));
        }

        [Fact]
        public async Task Pix_Config_Cifra_Token_E_Pagamento_Muda_Status()
        {
            var ctx = CreateContext(nameof(Pix_Config_Cifra_Token_E_Pagamento_Muda_Status));
            var (t, u) = Ids;
            var cofre = new IdentityCofre();

            await new SalvarCrmConfiguracaoPixRelacionalCommandHandler(ctx, t, u, cofre)
                .Handle(new SalvarCrmConfiguracaoPixRelacionalCommand("https://api", "tok-pix-secreto", "12345678000199", true), CancellationToken.None);
            var config = await ctx.CrmConfiguracoesPixRelacional.FirstAsync();
            Assert.Equal("enc:tok-pix-secreto", config.TokenPix); // CRM-071: cifrado, nunca em claro

            // Reenviar a máscara não sobrescreve o token cifrado.
            await new SalvarCrmConfiguracaoPixRelacionalCommandHandler(ctx, t, u, cofre)
                .Handle(new SalvarCrmConfiguracaoPixRelacionalCommand("https://api2", SalvarCrmConfiguracaoPixRelacionalCommandHandler.MascaraToken, "12345678000199", true), CancellationToken.None);
            config = await ctx.CrmConfiguracoesPixRelacional.FirstAsync();
            Assert.Equal("enc:tok-pix-secreto", config.TokenPix);
            Assert.Equal("https://api2", config.LinkApiAppVendas);

            var pag = await new CriarCrmPagamentoPixRelacionalCommandHandler(ctx, t, u)
                .Handle(new CriarCrmPagamentoPixRelacionalCommand("Oportunidade", Guid.NewGuid(), 150m, "qr", null, null), CancellationToken.None);
            Assert.True(pag.Sucesso);
            var pagId = (Guid)pag.Dados!.GetType().GetProperty("Id")!.GetValue(pag.Dados)!;

            await new AtualizarStatusCrmPagamentoPixCommandHandler(ctx, t, u)
                .Handle(new AtualizarStatusCrmPagamentoPixCommand(pagId, ECrmStatusPagamentoPix.Aprovado, "prov-123"), CancellationToken.None);
            var pagamento = await ctx.CrmPagamentosPixRelacional.FirstAsync(p => p.Id == pagId);
            Assert.Equal(ECrmStatusPagamentoPix.Aprovado, pagamento.Status);
            Assert.NotNull(pagamento.DataAprovacao);
        }

        private sealed class FakeTenant : ITenantProvider
        {
            private readonly string _t;
            public FakeTenant(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private sealed class FakeUser : ICurrentUser
        {
            private readonly string _u;
            public FakeUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => _u;
            public string? GetUserEmail() => _u + "@test.local";
        }

        private sealed class IdentityCofre : ISegredoCofreService
        {
            public Task<string> CriptografarAsync(string valor) => Task.FromResult("enc:" + valor);
            public Task<string> DescriptografarAsync(string ciphertext) => Task.FromResult(ciphertext.StartsWith("enc:") ? ciphertext.Substring(4) : ciphertext);
        }
    }
}
