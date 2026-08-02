using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Imobiliaria.Application.Commands;
using Epros.Modules.Imobiliaria.Application.Handlers;
using Epros.Modules.Imobiliaria.Application.Queries;
using Epros.Modules.Imobiliaria.Domain.Entities;
using Epros.Modules.Imobiliaria.Domain.Enums;
using Epros.Modules.Imobiliaria.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes do ciclo completo do submodulo IMO-001 (escopo maximo): transicoes de estado,
    /// ciclo financeiro do aluguel (cobranca/baixa/recibo + evento CONTAS_RECEBER), reajuste,
    /// rescisao, garantias e propostas. InMemory DbContext.
    /// </summary>
    public class ImobiliariaCicloTests
    {
        private const string Tenant = "tenant-imo-ciclo";
        private const string User = "user-imo-ciclo";

        private static ContextImobiliaria NovoContexto(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextImobiliaria>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ContextImobiliaria(options, new TProv(Tenant), new TUser(User));
        }

        private static async Task<Guid> CriarImovelDisponivel(ContextImobiliaria ctx, string db)
        {
            var criar = new CriarImovelCommandHandler(ctx, new TProv(Tenant), new TUser(User));
            var r = await criar.Handle(new CriarImovelCommand(
                "Sala comercial", null, "13000-000", "Rua A", "10", null, "Centro",
                new List<ProprietarioInput> { new(Guid.NewGuid()) }, null, null), CancellationToken.None);
            var imovelId = (Guid)r.Dados!.GetType().GetProperty("ImovelId")!.GetValue(r.Dados)!;
            await new TransicaoImovelCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new DisponibilizarImovelCommand(imovelId), CancellationToken.None);
            return imovelId;
        }

        private static async Task<Guid> CriarLocacaoVigente(ContextImobiliaria ctx, Guid imovelId, decimal valor = 2000m)
        {
            var criar = new CriarLocacaoCommandHandler(ctx, new TProv(Tenant), new TUser(User));
            var r = await criar.Handle(new CriarLocacaoCommand(
                imovelId, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), valor, 10,
                new List<Guid> { Guid.NewGuid() }, null), CancellationToken.None);
            var locacaoId = (Guid)r.Dados!.GetType().GetProperty("LocacaoId")!.GetValue(r.Dados)!;
            await new FormalizarLocacaoCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new FormalizarLocacaoCommand(locacaoId), CancellationToken.None);
            return locacaoId;
        }

        // ==================== Estados do imovel ====================

        [Fact(DisplayName = "Imovel | Disponibilizar exige proprietario e move para Disponivel (ID1)")]
        public void Imovel_Disponibilizar()
        {
            var imovel = new Imovel("Apto", null, null, null, null, null, null, Tenant, User);
            imovel.AdicionarProprietario(new ImovelProprietario(Guid.NewGuid(), Tenant, User));
            imovel.Disponibilizar(User);
            Assert.True(imovel.IsValid);
            Assert.Equal(EStatusImovel.Disponivel, imovel.Status);
        }

        [Fact(DisplayName = "Imovel | Locado nao pode ser inativado (ID1)")]
        public void Imovel_Locado_NaoInativa()
        {
            var imovel = new Imovel("Apto", null, null, null, null, null, null, Tenant, User);
            imovel.AdicionarProprietario(new ImovelProprietario(Guid.NewGuid(), Tenant, User));
            imovel.Disponibilizar(User);
            imovel.MarcarLocado(User);
            imovel.Inativar(User);
            Assert.False(imovel.IsValid);
            Assert.Equal(EStatusImovel.Locado, imovel.Status);
        }

        // ==================== Efeito colateral formalizar/encerrar ====================

        [Fact(DisplayName = "Locacao | Formalizar loca o imovel e publica evento (ID1/T2)")]
        public async Task Formalizar_LocaImovel_PublicaEvento()
        {
            var ctx = NovoContexto(nameof(Formalizar_LocaImovel_PublicaEvento));
            var imovelId = await CriarImovelDisponivel(ctx, nameof(Formalizar_LocaImovel_PublicaEvento));
            await CriarLocacaoVigente(ctx, imovelId);

            var imovel = await ctx.Imoveis.FirstAsync(i => i.Id == imovelId);
            Assert.Equal(EStatusImovel.Locado, imovel.Status);
            Assert.True(await ctx.OutboxMessages.AnyAsync(o => o.EventType == CatalogoEventosIntegracao.Imobiliaria.LocacaoFormalizada));
        }

        [Fact(DisplayName = "Locacao | Encerrar libera o imovel (ID1)")]
        public async Task Encerrar_LiberaImovel()
        {
            var ctx = NovoContexto(nameof(Encerrar_LiberaImovel));
            var imovelId = await CriarImovelDisponivel(ctx, nameof(Encerrar_LiberaImovel));
            var locacaoId = await CriarLocacaoVigente(ctx, imovelId);

            var r = await new EncerrarLocacaoCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new EncerrarLocacaoCommand(locacaoId), CancellationToken.None);

            Assert.True(r.Sucesso);
            var imovel = await ctx.Imoveis.FirstAsync(i => i.Id == imovelId);
            Assert.Equal(EStatusImovel.Disponivel, imovel.Status);
        }

        // ==================== Ciclo financeiro ====================

        [Fact(DisplayName = "Aluguel | Gerar cobranca e idempotente por competencia (ID8/NF-01)")]
        public async Task Cobranca_Idempotente()
        {
            var ctx = NovoContexto(nameof(Cobranca_Idempotente));
            var imovelId = await CriarImovelDisponivel(ctx, nameof(Cobranca_Idempotente));
            var locacaoId = await CriarLocacaoVigente(ctx, imovelId);
            var handler = new GerarCobrancaAluguelCommandHandler(ctx, new TProv(Tenant), new TUser(User));

            var r1 = await handler.Handle(new GerarCobrancaAluguelCommand(locacaoId, 2026, 3), CancellationToken.None);
            var r2 = await handler.Handle(new GerarCobrancaAluguelCommand(locacaoId, 2026, 3), CancellationToken.None);

            Assert.True(r1.Sucesso);
            Assert.True(r2.Sucesso);
            Assert.Equal(1, await ctx.CobrancasAluguel.CountAsync());
            Assert.True(await ctx.OutboxMessages.AnyAsync(o => o.EventType == CatalogoEventosIntegracao.Imobiliaria.AluguelCobrancaGerada));
        }

        [Fact(DisplayName = "Aluguel | Baixa parcial e total refletem status (ID8/NF-01)")]
        public async Task Baixa_ParcialETotal()
        {
            var ctx = NovoContexto(nameof(Baixa_ParcialETotal));
            var imovelId = await CriarImovelDisponivel(ctx, nameof(Baixa_ParcialETotal));
            var locacaoId = await CriarLocacaoVigente(ctx, imovelId, 1000m);
            var gerar = await new GerarCobrancaAluguelCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new GerarCobrancaAluguelCommand(locacaoId, 2026, 4), CancellationToken.None);
            var cobrancaId = (Guid)gerar.Dados!.GetType().GetProperty("CobrancaId")!.GetValue(gerar.Dados)!;

            var baixa = new RefletirBaixaAluguelCommandHandler(ctx, new TProv(Tenant), new TUser(User));
            await baixa.Handle(new RefletirBaixaAluguelCommand(cobrancaId, 400m, "CR-1", null), CancellationToken.None);
            var parcial = await ctx.CobrancasAluguel.FirstAsync(c => c.Id == cobrancaId);
            Assert.Equal(EStatusCobrancaAluguel.Parcial, parcial.Status);

            await baixa.Handle(new RefletirBaixaAluguelCommand(cobrancaId, 600m, "CR-1", null), CancellationToken.None);
            var pago = await ctx.CobrancasAluguel.FirstAsync(c => c.Id == cobrancaId);
            Assert.Equal(EStatusCobrancaAluguel.Pago, pago.Status);
            Assert.Equal(1000m, pago.ValorPago);
        }

        [Fact(DisplayName = "Aluguel | Recibo usa numeracao central e e idempotente (ID8/NF-05/T9)")]
        public async Task Recibo_NumeracaoIdempotente()
        {
            var ctx = NovoContexto(nameof(Recibo_NumeracaoIdempotente));
            var imovelId = await CriarImovelDisponivel(ctx, nameof(Recibo_NumeracaoIdempotente));
            var locacaoId = await CriarLocacaoVigente(ctx, imovelId, 1500m);
            var gerar = await new GerarCobrancaAluguelCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new GerarCobrancaAluguelCommand(locacaoId, 2026, 5), CancellationToken.None);
            var cobrancaId = (Guid)gerar.Dados!.GetType().GetProperty("CobrancaId")!.GetValue(gerar.Dados)!;
            await new RefletirBaixaAluguelCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new RefletirBaixaAluguelCommand(cobrancaId, 1500m, null, null), CancellationToken.None);

            var recibo = new EmitirReciboAluguelCommandHandler(ctx, new TProv(Tenant), new TUser(User), new TNum());
            var r1 = await recibo.Handle(new EmitirReciboAluguelCommand(cobrancaId), CancellationToken.None);
            var r2 = await recibo.Handle(new EmitirReciboAluguelCommand(cobrancaId), CancellationToken.None);

            Assert.True(r1.Sucesso);
            Assert.True(r2.Sucesso);
            Assert.Equal(1, await ctx.RecibosAluguel.CountAsync());
        }

        [Fact(DisplayName = "Aluguel | Recibo sem baixa e bloqueado (NF-05)")]
        public async Task Recibo_SemBaixa_Bloqueado()
        {
            var ctx = NovoContexto(nameof(Recibo_SemBaixa_Bloqueado));
            var imovelId = await CriarImovelDisponivel(ctx, nameof(Recibo_SemBaixa_Bloqueado));
            var locacaoId = await CriarLocacaoVigente(ctx, imovelId);
            var gerar = await new GerarCobrancaAluguelCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new GerarCobrancaAluguelCommand(locacaoId, 2026, 6), CancellationToken.None);
            var cobrancaId = (Guid)gerar.Dados!.GetType().GetProperty("CobrancaId")!.GetValue(gerar.Dados)!;

            var r = await new EmitirReciboAluguelCommandHandler(ctx, new TProv(Tenant), new TUser(User), new TNum())
                .Handle(new EmitirReciboAluguelCommand(cobrancaId), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        // ==================== Reajuste ====================

        [Fact(DisplayName = "Reajuste | Atualiza valor da locacao e grava historico (ID7/NF-02)")]
        public async Task Reajuste_AtualizaValorEHistorico()
        {
            var ctx = NovoContexto(nameof(Reajuste_AtualizaValorEHistorico));
            var imovelId = await CriarImovelDisponivel(ctx, nameof(Reajuste_AtualizaValorEHistorico));
            var locacaoId = await CriarLocacaoVigente(ctx, imovelId, 1000m);

            var r = await new AplicarReajusteCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new AplicarReajusteCommand(locacaoId, 1080m, "IGP-M", DateTime.UtcNow, 8m), CancellationToken.None);

            Assert.True(r.Sucesso);
            var locacao = await ctx.Locacoes.FirstAsync(l => l.Id == locacaoId);
            Assert.Equal(1080m, locacao.Valor);
            Assert.Equal(1, await ctx.LocacaoReajustes.CountAsync());
        }

        // ==================== Rescisao ====================

        [Fact(DisplayName = "Rescisao | Encerra locacao, libera imovel e registra multa (ID7)")]
        public async Task Rescisao_EncerraELibera()
        {
            var ctx = NovoContexto(nameof(Rescisao_EncerraELibera));
            var imovelId = await CriarImovelDisponivel(ctx, nameof(Rescisao_EncerraELibera));
            var locacaoId = await CriarLocacaoVigente(ctx, imovelId);

            var r = await new RescindirLocacaoCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new RescindirLocacaoCommand(locacaoId, "Saida antecipada", DateTime.UtcNow, 30, 500m, null), CancellationToken.None);

            Assert.True(r.Sucesso);
            var locacao = await ctx.Locacoes.FirstAsync(l => l.Id == locacaoId);
            Assert.Equal(EStatusLocacao.Encerrada, locacao.Status);
            var imovel = await ctx.Imoveis.FirstAsync(i => i.Id == imovelId);
            Assert.Equal(EStatusImovel.Disponivel, imovel.Status);
            Assert.Equal(1, await ctx.LocacaoRescisoes.CountAsync());
        }

        // ==================== Garantias ====================

        [Fact(DisplayName = "Garantia | Fiador exige pessoa fiadora (ID6)")]
        public async Task Garantia_Fiador_ExigePessoa()
        {
            var ctx = NovoContexto(nameof(Garantia_Fiador_ExigePessoa));
            var imovelId = await CriarImovelDisponivel(ctx, nameof(Garantia_Fiador_ExigePessoa));
            var locacaoId = await CriarLocacaoVigente(ctx, imovelId);

            var r = await new AdicionarGarantiaCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new AdicionarGarantiaCommand(locacaoId, ETipoGarantia.Fiador, 5000m, null, null, null, null), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact(DisplayName = "Garantia | Substituir marca anterior e cria nova (ID6)")]
        public async Task Garantia_Substituir()
        {
            var ctx = NovoContexto(nameof(Garantia_Substituir));
            var imovelId = await CriarImovelDisponivel(ctx, nameof(Garantia_Substituir));
            var locacaoId = await CriarLocacaoVigente(ctx, imovelId);
            var add = await new AdicionarGarantiaCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new AdicionarGarantiaCommand(locacaoId, ETipoGarantia.Caucao, 3000m, null, null, "3 alugueis", null), CancellationToken.None);
            var garantiaId = (Guid)add.Dados!.GetType().GetProperty("GarantiaId")!.GetValue(add.Dados)!;

            var r = await new SubstituirGarantiaCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new SubstituirGarantiaCommand(garantiaId, ETipoGarantia.SeguroFianca, 4000m, null, null, "Seguradora X", null), CancellationToken.None);

            Assert.True(r.Sucesso);
            var anterior = await ctx.LocacaoGarantias.FirstAsync(g => g.Id == garantiaId);
            Assert.Equal(EStatusGarantia.Substituida, anterior.Status);
            Assert.Equal(2, await ctx.LocacaoGarantias.CountAsync());
        }

        // ==================== Propostas ====================

        [Fact(DisplayName = "Proposta | Aprovar e converter gera locacao (ID2)")]
        public async Task Proposta_ConverterGeraLocacao()
        {
            var ctx = NovoContexto(nameof(Proposta_ConverterGeraLocacao));
            var imovelId = await CriarImovelDisponivel(ctx, nameof(Proposta_ConverterGeraLocacao));
            var proponente = Guid.NewGuid();

            var criar = await new CriarPropostaCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new CriarPropostaCommand(ETipoProposta.Locacao, imovelId, DateTime.UtcNow.AddDays(15), 2500m,
                    "Proposta locacao", null, new List<PropostaParteInput> { new(proponente, EPapelParteProposta.Proponente) }), CancellationToken.None);
            var propostaId = (Guid)criar.Dados!.GetType().GetProperty("PropostaId")!.GetValue(criar.Dados)!;

            await new PropostaDecisaoCommandHandler(ctx, new TUser(User))
                .Handle(new AprovarPropostaCommand(propostaId), CancellationToken.None);
            var conv = await new ConverterPropostaCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new ConverterPropostaCommand(propostaId, new DateTime(2026, 2, 1), new DateTime(2027, 1, 31), 5), CancellationToken.None);

            Assert.True(conv.Sucesso);
            var proposta = await ctx.Propostas.FirstAsync(p => p.Id == propostaId);
            Assert.Equal(EStatusProposta.Convertida, proposta.Status);
            Assert.NotNull(proposta.LocacaoGeradaId);
            var locacao = await ctx.Locacoes.FirstAsync(l => l.Id == proposta.LocacaoGeradaId!.Value);
            Assert.Equal(2500m, locacao.Valor);
            Assert.Single(locacao.Partes);
        }

        [Fact(DisplayName = "Proposta | Converter sem aprovacao e bloqueado (ID2)")]
        public async Task Proposta_ConverterSemAprovar_Bloqueado()
        {
            var ctx = NovoContexto(nameof(Proposta_ConverterSemAprovar_Bloqueado));
            var imovelId = await CriarImovelDisponivel(ctx, nameof(Proposta_ConverterSemAprovar_Bloqueado));
            var criar = await new CriarPropostaCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new CriarPropostaCommand(ETipoProposta.Locacao, imovelId, DateTime.UtcNow.AddDays(15), 2500m, null, null, null), CancellationToken.None);
            var propostaId = (Guid)criar.Dados!.GetType().GetProperty("PropostaId")!.GetValue(criar.Dados)!;

            var conv = await new ConverterPropostaCommandHandler(ctx, new TProv(Tenant), new TUser(User))
                .Handle(new ConverterPropostaCommand(propostaId, null, null, 10), CancellationToken.None);
            Assert.False(conv.Sucesso);
        }

        // ==================== Test doubles ====================

        private class TProv : ITenantProvider
        {
            private readonly string _t;
            public TProv(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class TUser : ICurrentUser
        {
            private readonly string _u;
            public TUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => _u;
            public string? GetUserEmail() => $"{_u}@epros.local";
        }

        private class TNum : INumeracaoService
        {
            private long _seq = 0;
            public Task<long> ProximoNumeroAsync(string tipoDocumento, long valorInicial = 1, CancellationToken cancellationToken = default)
                => Task.FromResult(System.Threading.Interlocked.Increment(ref _seq));
        }
    }
}
