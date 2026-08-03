using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// MAN-TRB — D19: gating da maquina de estado da OS + evolucao + faturamento.
    /// </summary>
    public class ManutencaoTrabalhoGatingTests
    {
        private const string TenantId = "tenant-man-trb2";
        private const string UserId = "user-man-trb2";

        private static ContextManutencao NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextManutencao>().UseInMemoryDatabase(db).Options;
            return new ContextManutencao(options, new TP(TenantId), new CU(UserId));
        }

        private static OrdemServico NovaOs() =>
            new OrdemServico(EPerfilOrdem.Oficina, 1, Guid.NewGuid(), DateTime.UtcNow, false, null, null, null, null, "OS-1", TenantId, UserId);

        [Fact(DisplayName = "MAN-TRB | Transicao valida Aberta -> EmOrcamento -> Aprovada")]
        public void Trb_TransicaoValida_Ok()
        {
            var os = NovaOs();
            os.TransicionarStatus(EStatusOrdemServico.EmOrcamento, UserId);
            Assert.True(os.IsValid);
            os.TransicionarStatus(EStatusOrdemServico.Aprovada, UserId);
            Assert.True(os.IsValid);
            Assert.Equal(EStatusOrdemServico.Aprovada, os.StatusCodigo);
        }

        [Fact(DisplayName = "MAN-TRB | Transicao invalida Aberta -> Entregue e bloqueada (D19)")]
        public void Trb_TransicaoInvalida_Bloqueada()
        {
            var os = NovaOs();
            os.TransicionarStatus(EStatusOrdemServico.Entregue, UserId);
            Assert.False(os.IsValid);
            Assert.Equal(EStatusOrdemServico.Aberta, os.StatusCodigo);
        }

        [Fact(DisplayName = "MAN-TRB | Cancelar via TransicionarStatus e bloqueado (usar cancelamento)")]
        public void Trb_CancelarViaTransicao_Bloqueado()
        {
            var os = NovaOs();
            os.TransicionarStatus(EStatusOrdemServico.Cancelada, UserId);
            Assert.False(os.IsValid);
        }

        [Fact(DisplayName = "MAN-TRB | PodeTransicionar cobre o fluxo feliz e nega saltos")]
        public void Trb_PodeTransicionar()
        {
            Assert.True(OrdemServico.PodeTransicionar(EStatusOrdemServico.Aprovada, EStatusOrdemServico.Montagem));
            Assert.True(OrdemServico.PodeTransicionar(EStatusOrdemServico.Pronta, EStatusOrdemServico.Entregue));
            Assert.False(OrdemServico.PodeTransicionar(EStatusOrdemServico.Aberta, EStatusOrdemServico.Montagem));
            Assert.False(OrdemServico.PodeTransicionar(EStatusOrdemServico.Entregue, EStatusOrdemServico.Pronta));
        }

        [Fact(DisplayName = "MAN-TRB | Handler rejeita transicao invalida")]
        public async Task Trb_Handler_RejeitaTransicaoInvalida()
        {
            var db = nameof(Trb_Handler_RejeitaTransicaoInvalida);
            Guid osId;
            using (var seed = NovoContexto(db))
            {
                var os = NovaOs();
                seed.OrdensServico.Add(os);
                await seed.SaveChangesAsync();
                osId = os.Id;
            }
            using var ctx = NovoContexto(db);
            var handler = new TransicionarStatusOrdemServicoCommandHandler(ctx, new CU(UserId));
            var result = await handler.Handle(new TransicionarStatusOrdemServicoCommand(osId, EStatusOrdemServico.Entregue), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        [Fact(DisplayName = "MAN-TRB | Registrar evolucao persiste apontamento")]
        public async Task Trb_RegistrarEvolucao_Persiste()
        {
            var db = nameof(Trb_RegistrarEvolucao_Persiste);
            Guid osId;
            using (var seed = NovoContexto(db))
            {
                var os = NovaOs();
                seed.OrdensServico.Add(os);
                await seed.SaveChangesAsync();
                osId = os.Id;
            }
            using var ctx = NovoContexto(db);
            var handler = new RegistrarEvolucaoOrdemServicoCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new RegistrarEvolucaoOrdemServicoCommand(osId, "Diagnostico inicial", "10:00", false, null), CancellationToken.None);
            Assert.True(result.Sucesso);
            Assert.Equal(1, await ctx.OrdemServicoEvolucoes.CountAsync(e => e.OrdemServicoId == osId));
        }

        [Fact(DisplayName = "MAN-TRB | Faturar marca a OS como faturada")]
        public async Task Trb_Faturar_MarcaFaturada()
        {
            var db = nameof(Trb_Faturar_MarcaFaturada);
            Guid osId;
            using (var seed = NovoContexto(db))
            {
                var os = NovaOs();
                seed.OrdensServico.Add(os);
                await seed.SaveChangesAsync();
                osId = os.Id;
            }
            using var ctx = NovoContexto(db);
            var handler = new MarcarOrdemServicoFaturadaCommandHandler(ctx, new CU(UserId));
            var result = await handler.Handle(new MarcarOrdemServicoFaturadaCommand(osId, Guid.NewGuid()), CancellationToken.None);
            Assert.True(result.Sucesso);
            var osFat = await ctx.OrdensServico.FindAsync(osId);
            Assert.True(osFat!.Faturado);
            Assert.True(osFat.DocumentoFiscalEmitido);
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
            public string? GetUserName() => "man-trb2";
            public string? GetUserEmail() => "man-trb2@epros.com.br";
        }
    }
}
