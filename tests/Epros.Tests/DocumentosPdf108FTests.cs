using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Epros.API.Controllers;
using Epros.API.Security;
using Epros.Modules.GestaoClientes.Application.Documentos;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Infrastructure.Documentos;
using Epros.Shared.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// 1.08F — Geração de PDF de fatura/recibo (QuestPDF real) e exposição do link do boleto (URL do gateway).
    /// Estratégia: PDF real; conteúdo textual validado pela fonte única <see cref="DocumentoFinanceiroConteudo"/>.
    /// </summary>
    public class DocumentosPdf108FTests
    {
        private static ContextGestaoClientes CreateContext(string db, string tenant, string user)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>().UseInMemoryDatabase(db).Options;
            return new ContextGestaoClientes(options, new TestTenantProvider(tenant), new TestCurrentUser(user));
        }

        private sealed class TestTenantProvider : ITenantProvider
        {
            private readonly string _t; public TestTenantProvider(string t) => _t = t; public string GetTenantId() => _t;
        }
        private sealed class TestCurrentUser : ICurrentUser
        {
            private readonly string _u; public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u; public string? GetUserName() => "U"; public string? GetUserEmail() => "u@epros.com";
        }

        private static bool EhPdf(byte[] bytes)
            => bytes.Length > 4 && Encoding.ASCII.GetString(bytes, 0, 5) == "%PDF-";

        // ===== Renderer (unidade) =====

        [Fact]
        public void Renderer_Fatura_Deve_Gerar_Pdf_Com_Nome_Contendo_Numero()
        {
            var dto = new FaturaDetalhadaDto
            {
                Id = Guid.NewGuid(),
                ClienteRazaoSocial = "Cliente Teste",
                Valor = 199.90m,
                DataVencimento = new DateTime(2026, 8, 10),
                Status = "Pendente",
                Numero = "FAT-2026-0001",
                Itens = { new FaturaItemDto { Descricao = "Plano Pro", Valor = 199.90m } }
            };
            var renderer = new QuestPdfDocumentoFinanceiroRenderer();

            var doc = renderer.RenderFatura(dto);

            Assert.Equal("application/pdf", doc.ContentType);
            Assert.True(EhPdf(doc.Conteudo), "conteúdo deve ser um PDF (%PDF-)");
            Assert.True(doc.Conteudo.Length > 500);
            Assert.Contains("FAT-2026-0001", doc.NomeArquivo);
        }

        [Fact]
        public void Renderer_Recibo_Deve_Gerar_Pdf()
        {
            var dto = new ReciboPagamentoDto
            {
                Id = Guid.NewGuid(),
                Numero = "REC-20260801-ABCD1234",
                FaturaId = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                Valor = 199.90m,
                DataPagamento = DateTime.UtcNow,
                MeioPagamento = "PIX",
                PagadorNome = "Cliente Teste",
                PagadorDocumento = "00.000.000/0001-00"
            };
            var doc = new QuestPdfDocumentoFinanceiroRenderer().RenderRecibo(dto);

            Assert.Equal("application/pdf", doc.ContentType);
            Assert.True(EhPdf(doc.Conteudo));
            Assert.Contains("REC-20260801-ABCD1234", doc.NomeArquivo);
        }

        // ===== Conteúdo (mapeamento DTO → documento: nº/valor presentes) =====

        [Fact]
        public void Conteudo_Fatura_Deve_Conter_Numero_E_Valor()
        {
            var dto = new FaturaDetalhadaDto { Id = Guid.NewGuid(), Valor = 199.90m, Numero = "FAT-2026-0001", Status = "Pendente", DataVencimento = DateTime.UtcNow };
            var texto = string.Join(" | ", DocumentoFinanceiroConteudo.LinhasFatura(dto).Select(l => $"{l.Rotulo}:{l.Valor}"));
            Assert.Contains("FAT-2026-0001", texto);
            Assert.Contains(DocumentoFinanceiroConteudo.Moeda(199.90m), texto);
        }

        [Fact]
        public void Conteudo_Recibo_Deve_Conter_Numero_Pagador_Valor_Meio()
        {
            var dto = new ReciboPagamentoDto { Numero = "REC-1", Valor = 50m, MeioPagamento = "PIX", PagadorNome = "Fulano", DataPagamento = DateTime.UtcNow, FaturaId = Guid.NewGuid() };
            var texto = string.Join(" | ", DocumentoFinanceiroConteudo.LinhasRecibo(dto).Select(l => $"{l.Rotulo}:{l.Valor}"));
            Assert.Contains("REC-1", texto);
            Assert.Contains("Fulano", texto);
            Assert.Contains("PIX", texto);
            Assert.Contains(DocumentoFinanceiroConteudo.Moeda(50m), texto);
        }

        // ===== Handlers (query → documento) =====

        [Fact]
        public async Task ObterFaturaPdf_Deve_Devolver_Pdf_Da_Fatura()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-fatpdf"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var plano = new Plano("Pago", 100m, tenant, user); ctx.Planos.Add(plano);
            var cliente = new Cliente("Cliente PDF", "00.000.000/0001-00", "c@epros.com", plano.Id, tenant, user);
            ctx.Clientes.Add(cliente);
            var fatura = new Fatura(cliente.Id, 250m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Faturas.Add(fatura);
            await ctx.SaveChangesAsync();

            var handler = new ObterFaturaPdfQueryHandler(ctx, new QuestPdfDocumentoFinanceiroRenderer());
            var doc = await handler.Handle(new ObterFaturaPdfQuery(fatura.Id), CancellationToken.None);

            Assert.NotNull(doc);
            Assert.Equal("application/pdf", doc!.ContentType);
            Assert.True(EhPdf(doc.Conteudo));
        }

        [Fact]
        public async Task ObterFaturaPdf_Fatura_Inexistente_Deve_Retornar_Null()
        {
            var db = Guid.NewGuid().ToString();
            using var ctx = CreateContext(db, "t-x", "u");
            var handler = new ObterFaturaPdfQueryHandler(ctx, new QuestPdfDocumentoFinanceiroRenderer());
            var doc = await handler.Handle(new ObterFaturaPdfQuery(Guid.NewGuid()), CancellationToken.None);
            Assert.Null(doc);
        }

        [Fact]
        public async Task ObterReciboPdf_Deve_Devolver_Pdf_Do_Recibo()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-recpdf"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var plano = new Plano("Pago", 100m, tenant, user); ctx.Planos.Add(plano);
            var cliente = new Cliente("Cliente Rec", "00.000.000/0001-00", "c@epros.com", plano.Id, tenant, user);
            ctx.Clientes.Add(cliente);
            var fatura = new Fatura(cliente.Id, 100m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Faturas.Add(fatura);
            fatura.BaixarManual(DateTime.UtcNow, user);
            var recibo = ReciboPagamento.Emitir(fatura, null, 100m, "PIX", cliente.RazaoSocial, cliente.Cnpj, user);
            ctx.RecibosPagamento.Add(recibo);
            await ctx.SaveChangesAsync();

            var handler = new ObterReciboPdfQueryHandler(ctx, new QuestPdfDocumentoFinanceiroRenderer());
            var doc = await handler.Handle(new ObterReciboPdfQuery(fatura.Id), CancellationToken.None);

            Assert.NotNull(doc);
            Assert.Equal("application/pdf", doc!.ContentType);
            Assert.True(EhPdf(doc.Conteudo));
            Assert.Contains(recibo.Numero, doc.NomeArquivo);
        }

        [Fact]
        public async Task ObterReciboPdf_Sem_Recibo_Deve_Retornar_Null()
        {
            var db = Guid.NewGuid().ToString();
            using var ctx = CreateContext(db, "t-y", "u");
            var handler = new ObterReciboPdfQueryHandler(ctx, new QuestPdfDocumentoFinanceiroRenderer());
            Assert.Null(await handler.Handle(new ObterReciboPdfQuery(Guid.NewGuid()), CancellationToken.None));
        }

        // ===== Boleto: expõe a URL do gateway (não gera boleto) =====

        [Fact]
        public async Task ObterBoletoLink_Deve_Expor_Url_Do_Gateway()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-bolpdf"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var fatura = new Fatura(Guid.NewGuid(), 150m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Faturas.Add(fatura);
            var pag = new PagamentoFatura(fatura.Id, "Boleto", PagamentoFaturaStatus.Pending, 150m, null, null, false, null, tenant, user);
            pag.RegistrarCobrancaBoleto("bol-1", "00190500954", "0339912345", "https://mp/boleto.pdf", DateTime.UtcNow.AddDays(5), user);
            ctx.PagamentosFaturas.Add(pag);
            await ctx.SaveChangesAsync();

            var link = await new ObterBoletoLinkQueryHandler(ctx).Handle(new ObterBoletoLinkQuery(fatura.Id), CancellationToken.None);

            Assert.NotNull(link);
            Assert.Equal("https://mp/boleto.pdf", link!.UrlBoleto);
            Assert.Equal("00190500954", link.LinhaDigitavel);
        }

        [Fact]
        public async Task ObterBoletoLink_Sem_Boleto_Deve_Retornar_Null()
        {
            var db = Guid.NewGuid().ToString();
            using var ctx = CreateContext(db, "t-z", "u");
            var link = await new ObterBoletoLinkQueryHandler(ctx).Handle(new ObterBoletoLinkQuery(Guid.NewGuid()), CancellationToken.None);
            Assert.Null(link);
        }

        // ===== Segurança: endpoints de documento sob a mesma guarda da controller (SuperAdmin/Suporte Comercial) =====

        [Fact]
        public void Endpoints_De_Documento_Existem_E_Sao_HttpGet()
        {
            var t = typeof(FaturasController);
            foreach (var nome in new[] { "ObterFaturaPdf", "ObterReciboPdf", "ObterBoletoPdf" })
            {
                var m = t.GetMethod(nome);
                Assert.NotNull(m);
                Assert.NotEmpty(m!.GetCustomAttributes<Microsoft.AspNetCore.Mvc.HttpGetAttribute>());
            }
        }

        [Fact]
        public void Controller_De_Faturas_Exige_Abac_Nos_Endpoints_De_Documento()
        {
            // Defesa em profundidade: os endpoints herdam o AbacAuthorize da controller (SuperAdmin + Suporte Comercial).
            var abac = typeof(FaturasController).GetCustomAttributes<AbacAuthorizeAttribute>().ToList();
            Assert.NotEmpty(abac);
        }
    }
}
