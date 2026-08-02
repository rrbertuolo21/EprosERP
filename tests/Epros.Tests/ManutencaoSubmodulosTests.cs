using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Application.Queries;
using Epros.Modules.Manutencao.Application.Handlers;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    public class ManutencaoSubmodulosTests
    {
        private const string TenantId = "tenant-man-sub";
        private const string UserId = "user-man-sub";

        private static ContextManutencao NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextManutencao>()
                .UseInMemoryDatabase(db)
                .Options;
            return new ContextManutencao(options, new TP(TenantId), new CU(UserId));
        }

        // ===================== MAN-PRV =====================
        [Fact(DisplayName = "MAN-PRV | Criar plano valido gera rascunho")]
        public async Task Prv_CriarPlano_DeveCriarRascunho()
        {
            using var ctx = NovoContexto(nameof(Prv_CriarPlano_DeveCriarRascunho));
            var handler = new CriarPlanoPreventivoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new CriarPlanoPreventivoCommand("PRV-01", "Lubrificacao mensal", Guid.NewGuid(), "Equipamento", Guid.NewGuid(), null), CancellationToken.None);
            Assert.True(result.Sucesso);
            var plano = await ctx.PlanosPreventivos.FirstAsync();
            Assert.Equal(EStatusRegistroManutencao.Rascunho, plano.Status);
        }

        [Fact(DisplayName = "MAN-PRV | Codigo duplicado deve bloquear (RN-PRV-001)")]
        public async Task Prv_CodigoDuplicado_DeveFalhar()
        {
            using var ctx = NovoContexto(nameof(Prv_CodigoDuplicado_DeveFalhar));
            var handler = new CriarPlanoPreventivoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            await handler.Handle(new CriarPlanoPreventivoCommand("PRV-DUP", "A", Guid.NewGuid(), null, null, null), CancellationToken.None);
            var result = await handler.Handle(new CriarPlanoPreventivoCommand("PRV-DUP", "B", Guid.NewGuid(), null, null, null), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "MAN-PRV | Ativar sem periodicidade/alvo deve falhar (RN-PRV-008/009)")]
        public void Prv_AtivarSemPeriodicidade_DeveFalhar()
        {
            var plano = new PlanoPreventivo("PRV-02", "Teste", Guid.NewGuid(), "Equipamento", Guid.NewGuid(), null, TenantId, UserId);
            plano.Ativar(UserId);
            Assert.False(plano.IsValid); // sem periodicidade
        }

        [Fact(DisplayName = "MAN-PRV | Ativar com periodicidade e alvo deve ativar")]
        public void Prv_AtivarComPeriodicidadeEAlvo_DeveAtivar()
        {
            var plano = new PlanoPreventivo("PRV-03", "Teste", Guid.NewGuid(), "Equipamento", Guid.NewGuid(), null, TenantId, UserId);
            var per = new PlanoPreventivoPeriodicidade(plano.Id, ETipoPeriodicidade.Calendario, DateTime.UtcNow, 30, "Dia", null, null, null, null, DateTime.UtcNow.AddDays(30), TenantId, UserId);
            plano.AdicionarPeriodicidade(per, UserId);
            plano.Ativar(UserId);
            Assert.True(plano.IsValid);
            Assert.Equal(EStatusRegistroManutencao.Ativo, plano.Status);
        }

        [Fact(DisplayName = "MAN-PRV | Kit de peca com quantidade zero deve ser invalido (RN-PRV-012)")]
        public void Prv_KitQuantidadeZero_DeveSerInvalido()
        {
            var kit = new PlanoPreventivoKitPeca(Guid.NewGuid(), Guid.NewGuid(), 0m, "un", true, null, TenantId, UserId);
            Assert.False(kit.IsValid);
        }

        // ===================== MAN-TRB =====================
        [Fact(DisplayName = "MAN-TRB | OS perfil Campo sem colaborador deve ser invalida")]
        public void Trb_CampoSemColaborador_DeveSerInvalido()
        {
            var os = new OrdemServico(EPerfilOrdem.Campo, 1, Guid.NewGuid(), DateTime.UtcNow, false, null, null, null, null, null, TenantId, UserId);
            Assert.False(os.IsValid);
        }

        [Fact(DisplayName = "MAN-TRB | Item recalcula total com desconto")]
        public void Trb_ItemRecalculaTotal()
        {
            var item = new OrdemServicoItem(Guid.NewGuid(), Guid.NewGuid(), ETipoItemOrdemServico.Produto, null, 2m, 100m, 10m, ETipoSaidaItem.Venda, null, TenantId, UserId);
            Assert.True(item.IsValid);
            Assert.Equal(200m, item.ValorSubtotal);
            Assert.Equal(20m, item.ValorDesconto);
            Assert.Equal(180m, item.ValorTotal);
        }

        [Fact(DisplayName = "MAN-TRB | Abrir OS oficina valida persiste")]
        public async Task Trb_AbrirOs_DevePersistir()
        {
            using var ctx = NovoContexto(nameof(Trb_AbrirOs_DevePersistir));
            var handler = new AbrirOrdemServicoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new AbrirOrdemServicoCommand(EPerfilOrdem.Oficina, 1, Guid.NewGuid(), DateTime.UtcNow, false, null, null, null, null, "OS-1"), CancellationToken.None);
            Assert.True(result.Sucesso);
            var os = await ctx.OrdensServico.FirstAsync();
            Assert.Equal(EStatusOrdemServico.Aberta, os.StatusCodigo);
        }

        // ===================== MAN-PEC =====================
        [Fact(DisplayName = "MAN-PEC | Item peca inicia entregue zero")]
        public void Pec_ItemIniciaEntregueZero()
        {
            var item = new ItemPecaReposicao(Guid.NewGuid(), Guid.NewGuid(), 1, 5m, null, null, 10m, 0m, ETipoSaidaItem.Venda, TenantId, UserId);
            Assert.True(item.IsValid);
            Assert.Equal(0m, item.QuantidadeEntregue);
            Assert.Equal(EStatusItemPeca.Rascunho, item.StatusItem);
        }

        [Fact(DisplayName = "MAN-PEC | Entrega parcial e total atualizam status")]
        public void Pec_EntregaAtualizaStatus()
        {
            var item = new ItemPecaReposicao(Guid.NewGuid(), Guid.NewGuid(), 1, 4m, null, null, 10m, 0m, null, TenantId, UserId);
            item.RegistrarEntrega(2m, UserId);
            Assert.Equal(EStatusItemPeca.EntregueParcial, item.StatusItem);
            item.RegistrarEntrega(2m, UserId);
            Assert.Equal(EStatusItemPeca.EntregueTotal, item.StatusItem);
        }

        [Fact(DisplayName = "MAN-PEC | Politica com maximo menor que minimo deve ser invalida")]
        public void Pec_PoliticaMaxMenorMin_DeveSerInvalida()
        {
            var pol = new PoliticaReposicao(Guid.NewGuid(), 10m, 5m, 7m, 3, "Alta", null, null, TenantId, UserId);
            Assert.False(pol.IsValid);
        }

        // ===================== MAN-PAR =====================
        [Fact(DisplayName = "MAN-PAR | Finalizar calcula duracao em minutos")]
        public void Par_Finalizar_CalculaDuracao()
        {
            var inicio = new DateTime(2026, 7, 1, 8, 0, 0, DateTimeKind.Utc);
            var parada = new Parada("PAR-01", "Falha motor", Guid.NewGuid(), ETipoParada.NaoPlanejada, inicio, Guid.NewGuid(), null, null, null, null, null, TenantId, UserId);
            parada.Finalizar(inicio.AddMinutes(90), UserId);
            Assert.True(parada.IsValid);
            Assert.Equal(90m, parada.DuracaoMinutos);
        }

        [Fact(DisplayName = "MAN-PAR | Finalizar com fim antes do inicio deve falhar")]
        public void Par_FinalizarFimAntesInicio_DeveFalhar()
        {
            var inicio = DateTime.UtcNow;
            var parada = new Parada("PAR-02", "Teste", Guid.NewGuid(), ETipoParada.Planejada, inicio, Guid.NewGuid(), null, null, null, null, null, TenantId, UserId);
            parada.Finalizar(inicio.AddMinutes(-10), UserId);
            Assert.False(parada.IsValid);
        }

        [Fact(DisplayName = "MAN-PAR | Gerar OS corretiva cria a OT canonica e vincula a parada (T5)")]
        public async Task Par_GerarOsCorretiva_CriaOrdemCanonica()
        {
            using var ctx = NovoContexto(nameof(Par_GerarOsCorretiva_CriaOrdemCanonica));
            var reg = new RegistrarParadaCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var r = await reg.Handle(new RegistrarParadaCommand("PAR-COR", "Falha rolamento", Guid.NewGuid(), ETipoParada.NaoPlanejada, DateTime.UtcNow, Guid.NewGuid(), null, null, null, null, null), CancellationToken.None);
            Assert.True(r.Sucesso);
            var parada = await ctx.Paradas.FirstAsync();

            var handler = new GerarOsCorretivaParadaCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new GerarOsCorretivaParadaCommand(parada.Id), CancellationToken.None);
            Assert.True(result.Sucesso);

            var os = await ctx.OrdensServico.SingleAsync();
            Assert.Equal(EOrigemOrdemServico.Corretiva, os.OrigemTipo);
            Assert.Equal(parada.Id, os.OrigemId);
            Assert.Null(os.PessoaId);

            var paradaAtual = await ctx.Paradas.Include(p => p.VinculosOs).FirstAsync();
            Assert.Equal(os.Id, paradaAtual.OsGeradaId);
            var vinc = Assert.Single(paradaAtual.VinculosOs);
            Assert.Equal(EStatusVinculoOsParada.Gerada, vinc.StatusVinculo);
            Assert.Equal(os.Id, vinc.OrdemServicoId);

            // Idempotente: segunda chamada nao cria nova OS.
            await handler.Handle(new GerarOsCorretivaParadaCommand(parada.Id), CancellationToken.None);
            Assert.Equal(1, await ctx.OrdensServico.CountAsync());
        }

        // ===================== MAN-IND =====================
        [Fact(DisplayName = "MAN-IND | Aprovar inducao em rascunho define aprovada e aceite")]
        public void Ind_AprovarInducao_DefineAprovada()
        {
            var inducao = new EquipamentoInducao(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), null, null, TenantId, UserId);
            inducao.Aprovar(UserId);
            Assert.True(inducao.IsValid);
            Assert.Equal(EStatusInducao.Aprovada, inducao.StatusInducao);
            Assert.NotNull(inducao.DataAceite);
        }

        [Fact(DisplayName = "MAN-IND | Ativar inducao nao aprovada deve falhar")]
        public void Ind_AtivarNaoAprovada_DeveFalhar()
        {
            var inducao = new EquipamentoInducao(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), null, null, TenantId, UserId);
            inducao.Ativar(UserId);
            Assert.False(inducao.IsValid);
        }

        [Fact(DisplayName = "MAN-IND | Enriquecer configuracao do equipamento persiste campos")]
        public async Task Ind_ConfigurarEquipamento_Persiste()
        {
            using var ctx = NovoContexto(nameof(Ind_ConfigurarEquipamento_Persiste));
            var eq = new Equipamento("Bomba", "BMB-1", "Utilidades", DateTime.UtcNow, "Media", TenantId, UserId);
            ctx.Equipamentos.Add(eq);
            await ctx.SaveChangesAsync();

            var handler = new ConfigurarEquipamentoCommandHandler(ctx, new CU(UserId));
            var result = await handler.Handle(new ConfigurarEquipamentoCommand(eq.Id, "Bomba centrifuga", Guid.NewGuid(), Guid.NewGuid(), "SN-123", "Bombeamento", null, null, null), CancellationToken.None);
            Assert.True(result.Sucesso);
            var atualizado = await ctx.Equipamentos.FindAsync(eq.Id);
            Assert.Equal("SN-123", atualizado!.NumeroSerie);
        }

        private class TP : ITenantProvider
        {
            private readonly string _t;
            public TP(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class CU : ICurrentUser
        {
            private readonly string _u;
            public CU(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "man-sub";
            public string? GetUserEmail() => "man-sub@epros.com.br";
        }
    }
}
