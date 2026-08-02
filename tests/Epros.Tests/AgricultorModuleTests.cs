using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Agricultor.Application.Commands;
using Epros.Modules.Agricultor.Application.Handlers;
using Epros.Modules.Agricultor.Application.Queries;
using Epros.Modules.Agricultor.Domain.Entities;
using Epros.Modules.Agricultor.Domain.Enums;
using Epros.Modules.Agricultor.Domain.Services;
using Epros.Modules.Agricultor.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes do módulo AGRICULTOR (Painel do Produtor + Livro Caixa Digital LCDPR).
    /// InMemory DbContext, mesmo padrão de ImobiliariaModuleTests.
    /// </summary>
    public class AgricultorModuleTests
    {
        private const string Tenant = "tenant-agr-001";
        private const string User = "user-agr-001";

        private static ContextAgricultor NovoContexto(string dbName, string tenant = Tenant)
        {
            var options = new DbContextOptionsBuilder<ContextAgricultor>()
                .UseInMemoryDatabase(dbName).Options;
            return new ContextAgricultor(options, new TestTenantProvider(tenant), new TestCurrentUser(User));
        }

        // ======================= PAINEL DO PRODUTOR =======================

        [Fact(DisplayName = "Propriedade | Sem nome é inválida (AGR-D01)")]
        public void Propriedade_SemNome_Invalida()
        {
            var p = new PropriedadeRural("", null, null, null, null, ETipoExploracao.Individual, 100m,
                "SP", "3550308", null, null, Tenant, User);
            Assert.False(p.IsValid);
        }

        [Fact(DisplayName = "Propriedade | CAFIR não numérico é inválido (AGR-D17)")]
        public void Propriedade_CafirInvalido()
        {
            var p = new PropriedadeRural("Fazenda", null, null, "ABC123", null, ETipoExploracao.Individual, 100m,
                null, null, null, null, Tenant, User);
            Assert.False(p.IsValid);
        }

        [Fact(DisplayName = "Talhão | Área recalculada no back a partir do GeoJSON (AGR-D05)")]
        public void Talhao_AreaDoGeoJson()
        {
            // Quadrado ~ 0.01° x 0.01° perto do equador → área na casa de milhões de m².
            var geo = "{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[0,0.01],[0.01,0.01],[0.01,0],[0,0]]]}";
            var t = new Talhao("T1", null, geo, null, Tenant, User);
            Assert.True(t.IsValid);
            Assert.True(t.AreaM2 > 0);
            Assert.Equal(t.AreaM2 / 10000m, t.AreaHectares);
        }

        [Fact(DisplayName = "Talhão | Sem polígono e sem área informada é inválido (área > 0)")]
        public void Talhao_SemArea_Invalido()
        {
            var t = new Talhao("T1", null, null, null, Tenant, User);
            Assert.False(t.IsValid);
        }

        [Fact(DisplayName = "RegistrarDespesa | Persiste e publica evento agr.* no Outbox")]
        public async Task RegistrarDespesa_Persiste()
        {
            var ctx = NovoContexto(nameof(RegistrarDespesa_Persiste));
            var h = new DespesaReceitaCommandHandlers(ctx, new TestTenantProvider(Tenant), new TestCurrentUser(User));
            var r = await h.Handle(new RegistrarDespesaCommand(Guid.NewGuid(), null, null, null, null,
                new DateTime(2026, 3, 10), 1500m, "NF-1"), CancellationToken.None);
            Assert.True(r.Sucesso);
            Assert.Equal(1, await ctx.Despesas.CountAsync());
            Assert.Equal(1, await ctx.OutboxMessages.CountAsync());
        }

        [Fact(DisplayName = "RegistrarReceita | Valor = quantidade × preço")]
        public async Task RegistrarReceita_CalculaValor()
        {
            var ctx = NovoContexto(nameof(RegistrarReceita_CalculaValor));
            var h = new DespesaReceitaCommandHandlers(ctx, new TestTenantProvider(Tenant), new TestCurrentUser(User));
            var r = await h.Handle(new RegistrarReceitaCommand(Guid.NewGuid(), null, null,
                new DateTime(2026, 5, 1), 100m, 25m, "Cerealista", "NF-9"), CancellationToken.None);
            Assert.True(r.Sucesso);
            var rec = await ctx.Receitas.FirstAsync();
            Assert.Equal(2500m, rec.Valor);
        }

        [Fact(DisplayName = "Painel consolidado | Saldo = receitas − despesas por safra")]
        public async Task Painel_Consolidado()
        {
            var ctx = NovoContexto(nameof(Painel_Consolidado));
            var prop = Guid.NewGuid();
            var safra = Guid.NewGuid();
            var dr = new DespesaReceitaCommandHandlers(ctx, new TestTenantProvider(Tenant), new TestCurrentUser(User));
            await dr.Handle(new RegistrarReceitaCommand(prop, null, safra, new DateTime(2026, 6, 1), 10m, 100m, null, null), CancellationToken.None);
            await dr.Handle(new RegistrarDespesaCommand(prop, null, safra, null, null, new DateTime(2026, 6, 2), 300m, null), CancellationToken.None);

            var q = new PainelQueryHandlers(ctx);
            var res = await q.Handle(new PainelConsolidadoQuery(prop, 2026), CancellationToken.None);
            Assert.True(res.Sucesso);
            var painel = res.Dados!;
            decimal Prop(string nome) => (decimal)painel.GetType().GetProperty(nome)!.GetValue(painel)!;
            Assert.Equal(1000m, Prop("TotalReceita"));
            Assert.Equal(300m, Prop("TotalDespesa"));
            Assert.Equal(700m, Prop("Saldo"));
        }

        [Fact(DisplayName = "Isolamento multi-tenant | Propriedade de outro tenant não vaza (AGR-D06)")]
        public async Task CrossTenant_NaoVaza()
        {
            var db = nameof(CrossTenant_NaoVaza);
            var ctxA = NovoContexto(db, "tenant-A");
            var hA = new CriarPropriedadeCommandHandler(ctxA, new TestTenantProvider("tenant-A"), new TestCurrentUser(User));
            await hA.Handle(new CriarPropriedadeCommand("Fazenda A", null, null, null, null,
                ETipoExploracao.Individual, 100m, null, null, null, null, null), CancellationToken.None);

            var ctxB = NovoContexto(db, "tenant-B");
            var qB = new PainelQueryHandlers(ctxB);
            var res = await qB.Handle(new ListarPropriedadesQuery(), CancellationToken.None);
            var lista = (System.Collections.IEnumerable)res.Dados!;
            Assert.Empty(lista.Cast<object>());
        }

        // ======================= LCDPR — domínio =======================

        [Fact(DisplayName = "Q100 | Receita em COD_IMOVEL 000 é bloqueada (AGR-D12)")]
        public void Q100_ReceitaEmImovel000_Bloqueada()
        {
            var l = new LcdprLancamento(0, 0, new DateTime(2026, 1, 5), ETipoDocumentoLcdpr.NotaFiscal,
                "NF1", "Venda", "12345678901", ETipoLancamentoLcdpr.Receita, 1000m, 0m, Tenant, User);
            Assert.False(l.IsValid);
        }

        [Fact(DisplayName = "Q100 | Entrada e saída simultâneas é inválido")]
        public void Q100_EntradaESaida_Invalido()
        {
            var l = new LcdprLancamento(1, 1, new DateTime(2026, 1, 5), ETipoDocumentoLcdpr.NotaFiscal,
                "NF1", "X", null, ETipoLancamentoLcdpr.Receita, 100m, 50m, Tenant, User);
            Assert.False(l.IsValid);
        }

        [Fact(DisplayName = "Escrituração | Lançamento com COD_IMOVEL fora do 0040 é rejeitado (RN-05)")]
        public void Escrituracao_LancamentoImovelInexistente_Rejeitado()
        {
            var esc = EscrituracaoBase();
            esc.AdicionarLancamento(new LcdprLancamento(9, 1, new DateTime(2026, 1, 10), ETipoDocumentoLcdpr.NotaFiscal,
                "NF", "x", null, ETipoLancamentoLcdpr.Receita, 100m, 0m, Tenant, User));
            Assert.False(esc.IsValid);
            Assert.Empty(esc.Lancamentos);
        }

        [Fact(DisplayName = "Escrituração | COD_CONTA fora de {000,999}∪0050 é rejeitado (RN-06)")]
        public void Escrituracao_ContaInvalida_Rejeitada()
        {
            var esc = EscrituracaoBase();
            esc.AdicionarLancamento(new LcdprLancamento(1, 55, new DateTime(2026, 1, 10), ETipoDocumentoLcdpr.NotaFiscal,
                "NF", "x", null, ETipoLancamentoLcdpr.Receita, 100m, 0m, Tenant, User));
            Assert.False(esc.IsValid);
        }

        [Fact(DisplayName = "Escrituração | COD_CONTA 999 (trânsito) e 000 (espécie) são aceitos (RN39)")]
        public void Escrituracao_Conta999e000_Aceitas()
        {
            var esc = EscrituracaoBase();
            esc.AdicionarLancamento(new LcdprLancamento(1, 999, new DateTime(2026, 1, 10), ETipoDocumentoLcdpr.Outros,
                null, "trânsito", null, ETipoLancamentoLcdpr.Despesa_Custeio, 0m, 100m, Tenant, User));
            esc.AdicionarLancamento(new LcdprLancamento(1, 0, new DateTime(2026, 1, 11), ETipoDocumentoLcdpr.Outros,
                null, "espécie", null, ETipoLancamentoLcdpr.Despesa_Custeio, 0m, 50m, Tenant, User));
            Assert.True(esc.IsValid);
            Assert.Equal(2, esc.Lancamentos.Count);
        }

        [Fact(DisplayName = "Fechar | Exige ao menos um 0040 e um 0050")]
        public void Fechar_SemImovelOuConta_Bloqueia()
        {
            var esc = new LcdprEscrituracao("12345678901", "Produtor", new DateTime(2026, 1, 1), new DateTime(2026, 12, 31),
                0, 0, EFormaApuracao.LivroCaixa, Tenant, User);
            esc.Fechar("Contador", "99999999000199", User);
            Assert.NotEqual(EStatusEscrituracaoLcdpr.Fechada, esc.Status);
        }

        // ======================= LCDPR — apuração / gerador =======================

        [Fact(DisplayName = "Q100 | SLD_FIN é saldo corrido (RN40): +1000 então −400 = 600")]
        public void Gerador_SaldoCorrido()
        {
            var esc = EscrituracaoCompleta();
            var arquivo = new GeradorArquivoLcdprService().Gerar(esc);
            var q100 = arquivo.Conteudo.Split("\r\n").Where(l => l.StartsWith("Q100")).ToList();
            Assert.Equal(2, q100.Count);
            // 1º Q100 (receita 1000, jan): SLD_FIN = 100000 (implícito), NAT = P
            var f1 = q100[0].Split('|');
            Assert.Equal("100000", f1[9]); // VL_ENTRADA
            Assert.Equal("0", f1[10]);     // VL_SAIDA
            Assert.Equal("100000", f1[11]);// SLD_FIN
            Assert.Equal("P", f1[12]);
            // 2º Q100 (despesa 400, fev): SLD_FIN = 60000
            var f2 = q100[1].Split('|');
            Assert.Equal("40000", f2[10]);  // VL_SAIDA
            Assert.Equal("60000", f2[11]);  // SLD_FIN corrido
        }

        [Fact(DisplayName = "Q200 | Resumo mensal derivado do Q100, SLD_FIN cumulativo (RN41)")]
        public void Gerador_Q200_Cumulativo()
        {
            var esc = EscrituracaoCompleta();
            var gerador = new GeradorArquivoLcdprService();
            var resumo = gerador.DerivarResumoMensal(esc.Lancamentos);
            Assert.Equal(2, resumo.Count);
            // Jan: entrada 1000, cumulativo 1000
            Assert.Equal(1, resumo[0].Mes);
            Assert.Equal(1000m, resumo[0].VlEntrada);
            Assert.Equal(1000m, resumo[0].SldFinCumulativo);
            // Fev: saída 400, cumulativo 600
            Assert.Equal(2, resumo[1].Mes);
            Assert.Equal(400m, resumo[1].VlSaida);
            Assert.Equal(600m, resumo[1].SldFinCumulativo);
        }

        [Fact(DisplayName = "Arquivo | Ordem dos registros 0000→0010→0030→0040→0050→Q100→Q200→9999 (RN47)")]
        public void Gerador_OrdemRegistros()
        {
            var esc = EscrituracaoCompleta();
            var arquivo = new GeradorArquivoLcdprService().Gerar(esc);
            var regs = arquivo.Conteudo.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Split('|')[0]).ToList();
            Assert.Equal("0000", regs.First());
            Assert.Equal("9999", regs.Last());
            Assert.Equal(new[] { "0000", "0010", "0030", "0040", "0050", "Q100", "Q100", "Q200", "Q200", "9999" }, regs);
        }

        [Fact(DisplayName = "Arquivo | 9999.QTD_LIN = nº real de linhas incluindo o próprio 9999 (RN46)")]
        public void Gerador_QtdLinReal()
        {
            var esc = EscrituracaoCompleta();
            var arquivo = new GeradorArquivoLcdprService().Gerar(esc);
            var linhas = arquivo.Conteudo.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            var linha9999 = linhas.Single(l => l.StartsWith("9999"));
            var qtd = int.Parse(linha9999.Split('|').Last());
            Assert.Equal(linhas.Length, qtd);
            Assert.Equal(arquivo.QtdLinhas, qtd);
        }

        [Fact(DisplayName = "Arquivo | 0000 traz LCDPR e COD_VER 0013; CRLF por linha")]
        public void Gerador_Cabecalho0000()
        {
            var esc = EscrituracaoCompleta();
            var arquivo = new GeradorArquivoLcdprService().Gerar(esc);
            Assert.Contains("\r\n", arquivo.Conteudo);
            var l0 = arquivo.Conteudo.Split("\r\n")[0].Split('|');
            Assert.Equal("0000", l0[0]);
            Assert.Equal("LCDPR", l0[1]);
            Assert.Equal("0013", l0[2]);
            Assert.Equal("12345678901", l0[3]);
        }

        [Fact(DisplayName = "Formatação | Percentual implícito 50% → 05000; valor 10.000,00 → 1000000")]
        public void Formatador_Implicitos()
        {
            Assert.Equal("05000", FormatadorLcdpr.PercentualImplicito(50m));
            Assert.Equal("10000", FormatadorLcdpr.PercentualImplicito(100m));
            Assert.Equal("1000000", FormatadorLcdpr.ValorImplicito(10000.00m));
            Assert.Equal("30012025", FormatadorLcdpr.Data(new DateTime(2025, 1, 30)));
            Assert.Equal("012026", FormatadorLcdpr.Periodo(2026, 1));
        }

        [Fact(DisplayName = "Formatação | '|' no conteúdo é bloqueante")]
        public void Formatador_PipeProibido()
        {
            Assert.Throws<InvalidOperationException>(() => FormatadorLcdpr.MontarLinha("Q100", "hist|com|pipe"));
        }

        [Fact(DisplayName = "Validador | Escrituração completa é válida (sem bloqueantes)")]
        public void Validador_Completa_Valida()
        {
            var esc = EscrituracaoCompleta();
            var val = new ValidadorLcdprService(new GeradorArquivoLcdprService());
            var res = val.Validar(esc);
            Assert.True(res.Valido);
        }

        [Fact(DisplayName = "Validador | Participação < 100% sem 0045 é bloqueante (RN35)")]
        public void Validador_ParticipacaoParcialSemTerceiros_Bloqueia()
        {
            var esc = new LcdprEscrituracao("12345678901", "Produtor", new DateTime(2026, 1, 1), new DateTime(2026, 12, 31),
                0, 0, EFormaApuracao.LivroCaixa, Tenant, User);
            esc.AdicionarImovel(new LcdprImovel(1, "Sítio", "12345678", null, "SP", "3550308",
                ETipoExploracao.Parceria, 60m, Tenant, User)); // parceria 60%, sem 0045
            esc.AdicionarConta(new LcdprConta(1, 1, 1234, "12345", Tenant, User));
            var val = new ValidadorLcdprService(new GeradorArquivoLcdprService());
            var res = val.Validar(esc);
            Assert.False(res.Valido);
            Assert.Contains(res.Bloqueantes, b => b.Codigo == "RN35");
        }

        [Fact(DisplayName = "Obrigatoriedade | Motor parametrizado por ano decide pela receita (AGR-D10)")]
        public void ParamObrigatoriedade_DecidePorReceita()
        {
            var param = new LcdprParamObrigatoriedade(2025, 4_800_000m, "regra vigente", Tenant, User);
            Assert.True(param.ObrigadoAEscriturar(5_000_000m));
            Assert.False(param.ObrigadoAEscriturar(1_000_000m));
        }

        [Fact(DisplayName = "Handler | Gerar arquivo marca escrituração como Exportada e devolve hash")]
        public async Task Handler_GerarArquivo_Exporta()
        {
            // Um contexto novo por passo (mesmo InMemory DB): espelha o escopo por request em produção
            // e evita o rastreamento compartilhado do provider InMemory.
            var db = nameof(Handler_GerarArquivo_Exporta);
            var gerador = new GeradorArquivoLcdprService();
            var validador = new ValidadorLcdprService(gerador);
            var tp = new TestTenantProvider(Tenant);
            var cu = new TestCurrentUser(User);
            LcdprCommandHandlers H() => new(NovoContexto(db), tp, cu, gerador, validador);

            var abrir = await H().Handle(new AbrirEscrituracaoCommand("12345678901", "Produtor",
                new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), 0, 0, EFormaApuracao.LivroCaixa), CancellationToken.None);
            var escId = (Guid)abrir.Dados!.GetType().GetProperty("EscrituracaoId")!.GetValue(abrir.Dados)!;

            await H().Handle(new AdicionarImovelLcdprCommand(escId, 1, "Fazenda", "12345678", "12345678901",
                "SP", "3550308", ETipoExploracao.Individual, 100m, null), CancellationToken.None);
            await H().Handle(new AdicionarContaLcdprCommand(escId, 1, 1, 1234, "12345"), CancellationToken.None);
            await H().Handle(new RegistrarLancamentoCommand(escId, 1, 1, new DateTime(2026, 1, 15),
                ETipoDocumentoLcdpr.NotaFiscal, "NF1", "Venda soja", "98765432100", ETipoLancamentoLcdpr.Receita, 1000m, 0m), CancellationToken.None);

            var gerar = await H().Handle(new GerarArquivoLcdprCommand(escId), CancellationToken.None);
            Assert.True(gerar.Sucesso);
            var esc = await NovoContexto(db).Escrituracoes.FirstAsync();
            Assert.Equal(EStatusEscrituracaoLcdpr.Exportada, esc.Status);
        }

        // ======================= Helpers =======================

        private static LcdprEscrituracao EscrituracaoBase()
        {
            var esc = new LcdprEscrituracao("12345678901", "Produtor", new DateTime(2026, 1, 1), new DateTime(2026, 12, 31),
                0, 0, EFormaApuracao.LivroCaixa, Tenant, User);
            esc.AdicionarImovel(new LcdprImovel(1, "Fazenda", "12345678", "12345678901", "SP", "3550308",
                ETipoExploracao.Individual, 100m, Tenant, User));
            esc.AdicionarConta(new LcdprConta(1, 1, 1234, "12345", Tenant, User));
            return esc;
        }

        private static LcdprEscrituracao EscrituracaoCompleta()
        {
            var esc = EscrituracaoBase();
            esc.DefinirDadosCadastrais(new LcdprDadosCadastrais("Rua A", "SP", "3550308", "13000000", "p@x.com", Tenant, User));
            esc.AdicionarLancamento(new LcdprLancamento(1, 1, new DateTime(2026, 1, 15), ETipoDocumentoLcdpr.NotaFiscal,
                "NF1", "Venda soja", "98765432100", ETipoLancamentoLcdpr.Receita, 1000m, 0m, Tenant, User));
            esc.AdicionarLancamento(new LcdprLancamento(1, 1, new DateTime(2026, 2, 10), ETipoDocumentoLcdpr.NotaFiscal,
                "NF2", "Compra insumo", "11122233344", ETipoLancamentoLcdpr.Despesa_Custeio, 0m, 400m, Tenant, User));
            esc.Fechar("Contador", "99999999000199", User);
            return esc;
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenant;
            public TestTenantProvider(string tenant) => _tenant = tenant;
            public string GetTenantId() => _tenant;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _user;
            public TestCurrentUser(string user) => _user = user;
            public string? GetUserId() => _user;
            public string? GetUserName() => _user;
            public string? GetUserEmail() => $"{_user}@epros.local";
        }
    }
}
