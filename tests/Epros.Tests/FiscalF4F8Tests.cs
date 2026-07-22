using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Handlers;
using Epros.Modules.Fiscal.Application.Queries;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// Cobertura dos itens F4 (loaders, resolução CST por NCM, DELETEs) e F8 (NFS-e/CT-e/MDF-e) do módulo Fiscal.
    /// </summary>
    public class FiscalF4F8Tests
    {
        // =========================== Infra de teste ===========================

        private static ContextFiscal CriarContexto(string db, string tenantId = "tenant-1", string userId = "user-1")
        {
            var options = new DbContextOptionsBuilder<ContextFiscal>()
                .UseInMemoryDatabase(db)
                .Options;
            return new ContextFiscal(options, new FakeTenantProvider(tenantId), new FakeCurrentUser(userId));
        }

        private static Stream Texto(string conteudo) => new MemoryStream(Encoding.UTF8.GetBytes(conteudo));

        private sealed class FakeTenantProvider : ITenantProvider
        {
            private readonly string _id;
            public FakeTenantProvider(string id) => _id = id;
            public string GetTenantId() => _id;
        }

        private sealed class FakeCurrentUser : ICurrentUser
        {
            private readonly string _id;
            public FakeCurrentUser(string id) => _id = id;
            public string? GetUserId() => _id;
            public string? GetUserName() => "Test";
            public string? GetUserEmail() => "t@e.com";
        }

        /// <summary>Adapter NFS-e configurável para os testes (sem tocar em webservice real).</summary>
        private sealed class FakeNfseService : INfseFiscalService
        {
            public bool Sucesso { get; set; } = true;
            public RetornoNfseConfigDto ObterConfiguracao(int codigoMunicipioIbge) => new() { Sucesso = Sucesso };
            public Task<RetornoNfseDto> EmitirLoteAsync(NfseEmissaoInput input, CancellationToken ct = default)
                => Task.FromResult(Retorno());
            public Task<RetornoNfseDto> ConsultarLoteAsync(NfseConsultaLoteInput input, CancellationToken ct = default)
                => Task.FromResult(Retorno());
            public Task<RetornoNfseDto> ConsultarPorRpsAsync(NfseConsultaRpsInput input, CancellationToken ct = default)
                => Task.FromResult(Retorno());
            public Task<RetornoNfseDto> CancelarAsync(NfseCancelamentoInput input, CancellationToken ct = default)
                => Task.FromResult(Retorno());
            public Task<RetornoNfsePdfDto> GerarPdfDeXmlAsync(Stream xmlStream, int? codigoMunicipioIbge, CancellationToken ct = default)
                => Task.FromResult(new RetornoNfsePdfDto { Sucesso = Sucesso });

            private RetornoNfseDto Retorno() => Sucesso
                ? new RetornoNfseDto { Sucesso = true, StatusPrefeitura = 100, NumeroNfse = "123", CodigoVerificacao = "ABC", Protocolo = "P1", Motivo = "Autorizada" }
                : new RetornoNfseDto { Sucesso = false, StatusPrefeitura = 400, Motivo = "Rejeitada pela prefeitura" };
        }

        /// <summary>Adapter CT-e configurável.</summary>
        private sealed class FakeCteService : ICteFiscalService
        {
            public bool Sucesso { get; set; } = true;
            public string Chave { get; set; } = "35260612345678000199570010000000011000000015";
            public Task<RetornoDfeTransporteDto> EmitirAsync(ConhecimentoTransporteEletronico cte, CancellationToken ct = default)
                => Task.FromResult(Retorno());
            public Task<RetornoDfeTransporteDto> CancelarAsync(ConhecimentoTransporteEletronico cte, string justificativa, CancellationToken ct = default)
                => Task.FromResult(Retorno());
            private RetornoDfeTransporteDto Retorno() => Sucesso
                ? new RetornoDfeTransporteDto { Sucesso = true, StatusSefaz = 100, ChaveAcesso = Chave, Protocolo = "P1" }
                : new RetornoDfeTransporteDto { Sucesso = false, StatusSefaz = 400, Motivo = "Rejeitado" };
        }

        /// <summary>Adapter MDF-e configurável.</summary>
        private sealed class FakeMdfeService : IMdfeFiscalService
        {
            public bool Sucesso { get; set; } = true;
            public string Chave { get; set; } = "35260612345678000199580010000000011000000016";
            public Task<RetornoDfeTransporteDto> EmitirAsync(ManifestoEletronicoDocumentosFiscais mdfe, CancellationToken ct = default)
                => Task.FromResult(Retorno());
            public Task<RetornoDfeTransporteDto> EncerrarAsync(ManifestoEletronicoDocumentosFiscais mdfe, string municipioIbge, CancellationToken ct = default)
                => Task.FromResult(Retorno());
            private RetornoDfeTransporteDto Retorno() => Sucesso
                ? new RetornoDfeTransporteDto { Sucesso = true, StatusSefaz = 100, ChaveAcesso = Chave, Protocolo = "P1" }
                : new RetornoDfeTransporteDto { Sucesso = false, StatusSefaz = 400, Motivo = "Rejeitado" };
        }

        // =========================== NFS-e (F8) ===========================

        private static EmitirLoteNfseCommand NfseValida() => new(
            NumeroLote: 1,
            Rps: new NfseRpsCmd("1", "A"),
            Prestador: new NfsePrestadorCmd("12345678000199"),
            Tomador: new NfseTomadorCmd("98765432000188"),
            Servico: new NfseServicoCmd("1.01", 100m));

        [Fact]
        public async Task EmitirNfse_ComProvedorSucesso_PersisteAutorizada()
        {
            var ctx = CriarContexto(nameof(EmitirNfse_ComProvedorSucesso_PersisteAutorizada));
            var handler = new EmitirLoteNfseCommandHandler(ctx, new FakeTenantProvider("tenant-1"), new FakeCurrentUser("user-1"), new FakeNfseService { Sucesso = true });

            var result = await handler.Handle(NfseValida(), CancellationToken.None);

            Assert.True(result.Sucesso);
            var nota = await ctx.NotasServicoEletronicas.FirstAsync();
            Assert.Equal("Autorizada", nota.Status);
            Assert.Equal("123", nota.NumeroNfse);
        }

        [Fact]
        public async Task EmitirNfse_ComProvedorFalha_PersisteRejeitadaERetornaErro()
        {
            var ctx = CriarContexto(nameof(EmitirNfse_ComProvedorFalha_PersisteRejeitadaERetornaErro));
            var handler = new EmitirLoteNfseCommandHandler(ctx, new FakeTenantProvider("tenant-1"), new FakeCurrentUser("user-1"), new FakeNfseService { Sucesso = false });

            var result = await handler.Handle(NfseValida(), CancellationToken.None);

            Assert.False(result.Sucesso);
            var nota = await ctx.NotasServicoEletronicas.FirstAsync();
            Assert.Equal("Rejeitada", nota.Status);
        }

        [Fact]
        public void ValidarNfse_ValorServicoZero_Falha()
        {
            var validator = new EmitirLoteNfseCommandValidator();
            var cmd = NfseValida() with { Servico = new NfseServicoCmd("1.01", 0m) };
            var result = validator.Validate(cmd);
            Assert.False(result.IsValid);
        }

        // =========================== CT-e (F8) ===========================

        private static EmitirCteCommand CteValido() => new(
            Serie: 1, Numero: 1, RemetenteDocumento: "12345678000199", DestinatarioDocumento: "98765432000188",
            ValorTotal: 500m, ValorReceber: 500m);

        [Fact]
        public async Task EmitirCte_Sucesso_PersisteAutorizado()
        {
            var ctx = CriarContexto(nameof(EmitirCte_Sucesso_PersisteAutorizado));
            var handler = new EmitirCteCommandHandler(ctx, new FakeTenantProvider("tenant-1"), new FakeCurrentUser("user-1"), new FakeCteService { Sucesso = true });

            var result = await handler.Handle(CteValido(), CancellationToken.None);

            Assert.True(result.Sucesso);
            var cte = await ctx.ConhecimentosTransporteEletronicos.FirstAsync();
            Assert.Equal("Autorizado", cte.Status);
        }

        [Fact]
        public async Task EmitirCte_Falha_PersisteRejeitado()
        {
            var ctx = CriarContexto(nameof(EmitirCte_Falha_PersisteRejeitado));
            var handler = new EmitirCteCommandHandler(ctx, new FakeTenantProvider("tenant-1"), new FakeCurrentUser("user-1"), new FakeCteService { Sucesso = false });

            var result = await handler.Handle(CteValido(), CancellationToken.None);

            Assert.False(result.Sucesso);
            var cte = await ctx.ConhecimentosTransporteEletronicos.FirstAsync();
            Assert.Equal("Rejeitado", cte.Status);
        }

        [Fact]
        public async Task CancelarCte_Autorizado_MudaParaCancelado()
        {
            var ctx = CriarContexto(nameof(CancelarCte_Autorizado_MudaParaCancelado));
            var svc = new FakeCteService { Sucesso = true };
            var cte = new ConhecimentoTransporteEletronico(1, 1, 2, 0, 1, "12345678000199", "98765432000188", 500m, 500m, "tenant-1", "user-1");
            cte.Autorizar(svc.Chave, "P1", 100, "<x/>", "<y/>");
            ctx.ConhecimentosTransporteEletronicos.Add(cte);
            await ctx.SaveChangesAsync();

            var handler = new CancelarCteCommandHandler(ctx, new FakeCurrentUser("user-1"), svc);
            var result = await handler.Handle(new CancelarCteCommand(svc.Chave, "Justificativa valida de cancelamento CT-e"), CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal("Cancelado", (await ctx.ConhecimentosTransporteEletronicos.FirstAsync()).Status);
        }

        [Fact]
        public async Task CancelarCte_ChaveInexistente_Falha()
        {
            var ctx = CriarContexto(nameof(CancelarCte_ChaveInexistente_Falha));
            var handler = new CancelarCteCommandHandler(ctx, new FakeCurrentUser("user-1"), new FakeCteService());
            var result = await handler.Handle(new CancelarCteCommand(new string('9', 44), "Justificativa valida de cancelamento CT-e"), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        // =========================== MDF-e (F8) ===========================

        private static EmitirMdfeCommand MdfeValido() => new(
            Serie: 1, Numero: 1, UfInicio: "SP", UfFim: "RJ", QuantidadeCarregados: 2, ValorCarga: 1000m);

        [Fact]
        public async Task EmitirMdfe_Sucesso_PersisteAutorizado()
        {
            var ctx = CriarContexto(nameof(EmitirMdfe_Sucesso_PersisteAutorizado));
            var handler = new EmitirMdfeCommandHandler(ctx, new FakeTenantProvider("tenant-1"), new FakeCurrentUser("user-1"), new FakeMdfeService { Sucesso = true });

            var result = await handler.Handle(MdfeValido(), CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal("Autorizado", (await ctx.ManifestosEletronicosDocumentosFiscais.FirstAsync()).Status);
        }

        [Fact]
        public async Task EncerrarMdfe_Autorizado_MudaEstado()
        {
            var ctx = CriarContexto(nameof(EncerrarMdfe_Autorizado_MudaEstado));
            var svc = new FakeMdfeService { Sucesso = true };
            var mdfe = new ManifestoEletronicoDocumentosFiscais(1, 1, 2, 1, 1, "SP", "RJ", 2, 1000m, "tenant-1", "user-1");
            mdfe.Autorizar(svc.Chave, "P1", 100, "<x/>", "<y/>");
            ctx.ManifestosEletronicosDocumentosFiscais.Add(mdfe);
            await ctx.SaveChangesAsync();

            var handler = new EncerrarMdfeCommandHandler(ctx, new FakeCurrentUser("user-1"), svc);
            var result = await handler.Handle(new EncerrarMdfeCommand(svc.Chave, "3550308"), CancellationToken.None);

            Assert.True(result.Sucesso);
        }

        [Fact]
        public async Task EncerrarMdfe_NaoAutorizado_Falha()
        {
            var ctx = CriarContexto(nameof(EncerrarMdfe_NaoAutorizado_Falha));
            var mdfe = new ManifestoEletronicoDocumentosFiscais(1, 1, 2, 1, 1, "SP", "RJ", 2, 1000m, "tenant-1", "user-1");
            mdfe.Autorizar("35260612345678000199580010000000011000000016", "P1", 100, "<x/>", "<y/>");
            // Depois rejeitar não faz sentido; usa uma chave diferente para simular status não autorizado.
            var outra = new ManifestoEletronicoDocumentosFiscais(1, 2, 2, 1, 1, "SP", "RJ", 2, 1000m, "tenant-1", "user-1");
            outra.Rejeitar(400, "rej", "<y/>");
            ctx.ManifestosEletronicosDocumentosFiscais.AddRange(mdfe, outra);
            await ctx.SaveChangesAsync();

            var handler = new EncerrarMdfeCommandHandler(ctx, new FakeCurrentUser("user-1"), new FakeMdfeService());
            var result = await handler.Handle(new EncerrarMdfeCommand(outra.ChaveAcesso ?? "x", "3550308"), CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        // =========================== Loaders (F4-1) ===========================

        [Fact]
        public async Task LoaderNcm_JsonValido_ImportaSomente8DigitosEIdempotente()
        {
            var ctx = CriarContexto(nameof(LoaderNcm_JsonValido_ImportaSomente8DigitosEIdempotente));
            var handler = new AtualizarTabelaNcmCommandHandler(ctx, new FakeCurrentUser("user-1"));
            var json = """
            { "Nomenclaturas": [
              { "Codigo": "8471.60.52", "Descricao": "Teclado", "Data_Inicio": "01/01/2022", "Data_Fim": "31/12/2099" },
              { "Codigo": "8471", "Descricao": "Posicao (nao 8 digitos)", "Data_Inicio": "01/01/2022" }
            ]}
            """;

            var r1 = await handler.Handle(new AtualizarTabelaNcmCommand(Texto(json), "tab-ncm.json"), CancellationToken.None);
            Assert.True(r1.Sucesso);
            Assert.Equal(1, await ctx.Ncms.CountAsync());
            Assert.Equal("84716052", (await ctx.Ncms.FirstAsync()).CodigoNcm);

            // Reimportar o mesmo arquivo não duplica.
            var r2 = await handler.Handle(new AtualizarTabelaNcmCommand(Texto(json), "tab-ncm.json"), CancellationToken.None);
            Assert.True(r2.Sucesso);
            Assert.Equal(1, await ctx.Ncms.CountAsync());
        }

        [Fact]
        public async Task LoaderNcm_JsonInvalido_Falha()
        {
            var ctx = CriarContexto(nameof(LoaderNcm_JsonInvalido_Falha));
            var handler = new AtualizarTabelaNcmCommandHandler(ctx, new FakeCurrentUser("user-1"));
            var r = await handler.Handle(new AtualizarTabelaNcmCommand(Texto("{ nao é json"), "x.json"), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task LoaderCfopPadrao_Csv_ImportaCodigosValidos()
        {
            var ctx = CriarContexto(nameof(LoaderCfopPadrao_Csv_ImportaCodigosValidos));
            var handler = new AtualizarTabelaCfopPadraoCommandHandler(ctx, new FakeCurrentUser("user-1"));
            var csv = "codigo;descricao;natureza;dataInicio;dataFim;correlacao;integra;nfe;nfce;simples;devolucao\n" +
                      "5102;Venda de mercadoria;Venda;01/01/2020;;;1;1;1;;\n" + // sem incidenciaSimples -> default
                      "999;linha invalida (fora de faixa);x;;;;;;;;\n";

            var r = await handler.Handle(new AtualizarTabelaCfopPadraoCommand(Texto(csv), "cfop.csv"), CancellationToken.None);

            Assert.True(r.Sucesso);
            Assert.Equal(1, await ctx.CfopPadroes.CountAsync());
            Assert.Equal(5102, (await ctx.CfopPadroes.FirstAsync()).CfopCodigo);
        }

        [Fact]
        public async Task LoaderCodigoServicoSefaz_Csv_Importa()
        {
            var ctx = CriarContexto(nameof(LoaderCodigoServicoSefaz_Csv_Importa));
            var handler = new AtualizarTabelaCodigoServicoSefazCommandHandler(ctx, new FakeCurrentUser("user-1"));
            var csv = "codigo;descricao\n01010;Servico de teste\n01020;Outro servico\n";

            var r = await handler.Handle(new AtualizarTabelaCodigoServicoSefazCommand(Texto(csv), "serv.csv"), CancellationToken.None);

            Assert.True(r.Sucesso);
            Assert.Equal(2, await ctx.CodigosServicosSefaz.CountAsync());
        }

        [Fact]
        public async Task LoaderFcp_Csv_InsereEAtualizaPorUf()
        {
            var ctx = CriarContexto(nameof(LoaderFcp_Csv_InsereEAtualizaPorUf));
            var handler = new AtualizarTabelaFcpAliquotaUfCommandHandler(ctx, new FakeCurrentUser("user-1"));

            await handler.Handle(new AtualizarTabelaFcpAliquotaUfCommand(Texto("uf;aliquota;obs\nSP;2;inicial\n"), "fcp.csv"), CancellationToken.None);
            Assert.Equal(1, await ctx.FcpAliquotasUf.CountAsync());
            Assert.Equal(2m, (await ctx.FcpAliquotasUf.FirstAsync()).ValorAliquota);

            // Recarregar atualiza (não duplica) a mesma UF.
            await handler.Handle(new AtualizarTabelaFcpAliquotaUfCommand(Texto("uf;aliquota;obs\nSP;3;novo\n"), "fcp.csv"), CancellationToken.None);
            Assert.Equal(1, await ctx.FcpAliquotasUf.CountAsync());
            Assert.Equal(3m, (await ctx.FcpAliquotasUf.FirstAsync()).ValorAliquota);
        }

        [Fact]
        public async Task LoaderIbpt_Csv_UfPeloNomeDoArquivo_Importa()
        {
            var ctx = CriarContexto(nameof(LoaderIbpt_Csv_UfPeloNomeDoArquivo_Importa));
            var handler = new AtualizarTabelaIbptCommandHandler(ctx, new FakeCurrentUser("user-1"));
            // codigo;ex;tipo;descricao;nacfed;impfed;estadual;municipal;vigini;vigfim;chave;versao
            var csv = "84716052;0;0;Teclado;12.5;15.0;18.0;5.0;01/01/2025;31/12/2025;ABC;25.1.A\n";

            var r = await handler.Handle(new AtualizarTabelaIbptCommand(Texto(csv), "TabelaIBPTaxSP25.1.A.csv"), CancellationToken.None);

            Assert.True(r.Sucesso);
            var ibpt = await ctx.Ibpts.FirstAsync();
            Assert.Equal("84716052", ibpt.Codigo);
            Assert.Equal(EEstado.SP, ibpt.Uf);
            Assert.Equal(18.0m, ibpt.AliquotaEstadual);
        }

        // =========================== Resolução CST/cClassTrib (F4-2) ===========================

        private static async Task<Guid> SeedClassificacaoAsync(ContextFiscal ctx, string cst, string cClassTrib, string ncm, bool nfe = true, bool nfce = true)
        {
            var cstEnt = new CstIbsCbs(cst, $"Descricao {cst}", DateTime.UtcNow.Date, null, "tenant-1", "user-1");
            var classe = cstEnt.AdicionarClasseTributaria(cClassTrib, "classe", DateTime.UtcNow.Date, null, nfe, nfce, false, false, false, false, "user-1");
            classe.AdicionarAnexo(1, ncm, DateTime.UtcNow.Date, null, "user-1");
            ctx.CstsIbsCbs.Add(cstEnt);
            await ctx.SaveChangesAsync();
            return cstEnt.Id;
        }

        [Fact]
        public async Task ObterPorCst_ModeloInvalido_Falha()
        {
            var ctx = CriarContexto(nameof(ObterPorCst_ModeloInvalido_Falha));
            var handler = new ObterCstIbsCbsPorCstQueryHandler(ctx);
            var r = await handler.Handle(new ObterCstIbsCbsPorCstQuery("000", 99), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task ObterPorCst_Existente_RetornaCst()
        {
            var ctx = CriarContexto(nameof(ObterPorCst_Existente_RetornaCst));
            await SeedClassificacaoAsync(ctx, "000", "000001", "84716052");
            var handler = new ObterCstIbsCbsPorCstQueryHandler(ctx);
            var r = await handler.Handle(new ObterCstIbsCbsPorCstQuery("000", 55), CancellationToken.None);
            Assert.True(r.Sucesso);
        }

        [Fact]
        public async Task ObterPorNcmLst_SemClassificacao_RetornaFallbackGenerico()
        {
            var ctx = CriarContexto(nameof(ObterPorNcmLst_SemClassificacao_RetornaFallbackGenerico));
            // Só existe o CST "000" genérico, sem anexo para o NCM consultado.
            var cst = new CstIbsCbs("000", "Generico", DateTime.UtcNow.Date, null, "tenant-1", "user-1");
            ctx.CstsIbsCbs.Add(cst);
            await ctx.SaveChangesAsync();

            var handler = new ObterClassificacoesPorNcmQueryHandler(ctx);
            var r = await handler.Handle(new ObterClassificacoesPorNcmQuery("99999999", 55), CancellationToken.None);

            Assert.True(r.Sucesso);
            var lista = Assert.IsAssignableFrom<System.Collections.IEnumerable>(r.Dados);
            Assert.Single(lista.Cast<object>());
        }

        [Fact]
        public async Task ObterNcmsClassificados_ListaMista_ResolveClassificadosEFallback()
        {
            var ctx = CriarContexto(nameof(ObterNcmsClassificados_ListaMista_ResolveClassificadosEFallback));
            await SeedClassificacaoAsync(ctx, "200", "200010", "84716052");
            var handler = new ObterNcmsClassificadosQueryHandler(ctx);

            var r = await handler.Handle(new ObterNcmsClassificadosQuery(new[] { "84716052", "99999999" }, 55), CancellationToken.None);

            Assert.True(r.Sucesso);
            var itens = ((IEnumerable<NcmClassificadoDto>)r.Dados!).ToList();
            Assert.Equal(2, itens.Count);
            Assert.Contains(itens, i => i.Ncm == "84716052" && i.Cst == "200" && i.CClassTrib == "200010");
            Assert.Contains(itens, i => i.Ncm == "99999999" && i.Cst == "000" && i.CClassTrib == "000001");
        }

        [Fact]
        public async Task ObterNcmsClassificados_ListaVazia_Falha()
        {
            var ctx = CriarContexto(nameof(ObterNcmsClassificados_ListaVazia_Falha));
            var handler = new ObterNcmsClassificadosQueryHandler(ctx);
            var r = await handler.Handle(new ObterNcmsClassificadosQuery(Array.Empty<string>(), 55), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        // =========================== DELETEs (F4-3) ===========================

        [Fact]
        public async Task DeletarCfop_Existente_SoftDelete()
        {
            var ctx = CriarContexto(nameof(DeletarCfop_Existente_SoftDelete));
            var cfop = new Cfop(
                cfopCodigo: 5102, descricao: "Venda", naturezaOperacao: "Venda", cfopCorrelacao: "5102",
                integraFaturamento: true, indicadorNfe: true, indicadorComunicacao: false, indicadorTransporte: false,
                indicadorDevolucao: false, indicadorRetorno: false, indicadorAnulacao: false, indicadorRemessa: false,
                indicadorCombustivel: false, indicadorTransferencia: false, indicadorNfce: false, indicadorCiap: false,
                indicadorUsoConsumo: false, indicadorUsoSemOperacao: false, indicadorSt: false, indicadorMei: false,
                incidenciaSimples: Epros.Shared.Domain.Enums.EIncidenciaSimples.RevendaMercadorias, cfopDevolucao: null,
                tenantId: "tenant-1", criadoPor: "user-1");
            ctx.Cfops.Add(cfop);
            await ctx.SaveChangesAsync();

            var handler = new DeletarCfopCommandHandler(ctx, new FakeCurrentUser("user-1"));
            var r = await handler.Handle(new DeletarCfopCommand(cfop.Id), CancellationToken.None);

            Assert.True(r.Sucesso);
            Assert.Equal(0, await ctx.Cfops.CountAsync(c => c.DeletadoEm == null));
        }

        [Fact]
        public async Task DeletarCfop_Inexistente_Falha()
        {
            var ctx = CriarContexto(nameof(DeletarCfop_Inexistente_Falha));
            var handler = new DeletarCfopCommandHandler(ctx, new FakeCurrentUser("user-1"));
            var r = await handler.Handle(new DeletarCfopCommand(Guid.NewGuid()), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task DeletarTipoOperacaoFiscal_Existente_SoftDelete()
        {
            var ctx = CriarContexto(nameof(DeletarTipoOperacaoFiscal_Existente_SoftDelete));
            var tipo = new TipoOperacaoFiscal(Guid.NewGuid(), null, null, "Operacao", false,
                EFinalidadeEmissao.fnNormal, ETipoAtendimento.pcNao, EModalidadeFrete.mfContaEmitenteOumfContaRemetente, ETipoMovimento.Saida, "tenant-1", "user-1");
            ctx.TiposOperacoesFiscais.Add(tipo);
            await ctx.SaveChangesAsync();

            var handler = new DeletarTipoOperacaoFiscalCommandHandler(ctx, new FakeCurrentUser("user-1"));
            var r = await handler.Handle(new DeletarTipoOperacaoFiscalCommand(tipo.Id), CancellationToken.None);

            Assert.True(r.Sucesso);
            Assert.Equal(0, await ctx.TiposOperacoesFiscais.CountAsync(t => t.DeletadoEm == null));
        }

        [Fact]
        public async Task DeletarCodigoBeneficioFiscal_Existente_SoftDelete()
        {
            var ctx = CriarContexto(nameof(DeletarCodigoBeneficioFiscal_Existente_SoftDelete));
            var cbenef = new CodigoBeneficioFiscal("SP830001", EEstado.SP, "Beneficio", "tenant-1", "user-1");
            ctx.CodigosBeneficiosFiscais.Add(cbenef);
            await ctx.SaveChangesAsync();

            var handler = new DeletarCodigoBeneficioFiscalCommandHandler(ctx, new FakeCurrentUser("user-1"));
            var r = await handler.Handle(new DeletarCodigoBeneficioFiscalCommand(cbenef.Id), CancellationToken.None);

            Assert.True(r.Sucesso);
            Assert.Equal(0, await ctx.CodigosBeneficiosFiscais.CountAsync(c => c.DeletadoEm == null));
        }
    }
}
