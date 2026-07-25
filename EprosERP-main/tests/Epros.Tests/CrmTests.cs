using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Handlers;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>Testes do submódulo CRM (VEN-CRM). Fonte: EF_7_VENDAS_CRM_V1.</summary>
    public class CrmTests
    {
        private const string TenantId = "tenant-crm-001";
        private const string UserId = "user-crm-001";

        // ---------- Lead ----------

        [Fact(DisplayName = "CrmLead | Sem status informado inicia como Novo (CRM-006)")]
        public void Lead_SemStatus_IniciaNovo()
        {
            var lead = new CrmLead(null, null, "João", null, null, null, null, null, null, null, null, null, Guid.NewGuid(), null, null, null, TenantId, UserId);
            Assert.True(lead.IsValid);
            Assert.Equal(ECrmLeadStatus.Novo, lead.Status);
            Assert.False(lead.Convertido);
        }

        [Fact(DisplayName = "CrmLead | Sem nome é inválido (CRM-005)")]
        public void Lead_SemNome_Invalido()
        {
            var lead = new CrmLead(null, null, "", null, null, null, null, null, null, null, null, null, Guid.NewGuid(), null, null, null, TenantId, UserId);
            Assert.False(lead.IsValid);
        }

        [Fact(DisplayName = "CrmLead | Converter marca convertido e grava vínculos (CRM-009)")]
        public void Lead_Converter_MarcaConvertido()
        {
            var lead = new CrmLead(null, null, "João", null, null, null, null, null, null, null, null, null, Guid.NewGuid(), null, null, null, TenantId, UserId);
            var clienteId = Guid.NewGuid();
            var opId = Guid.NewGuid();
            lead.Converter(clienteId, null, opId, UserId);

            Assert.True(lead.Convertido);
            Assert.Equal(ECrmLeadStatus.Convertido, lead.Status);
            Assert.Equal(clienteId, lead.ClienteId);
            Assert.Equal(opId, lead.OportunidadeId);
        }

        [Fact(DisplayName = "Converter lead cria oportunidade e impede reconversão (CRM-011/CRM-014)")]
        public async Task ConverterLeadHandler_CriaOportunidade_EImpedeReconversao()
        {
            var context = CreateContext(nameof(ConverterLeadHandler_CriaOportunidade_EImpedeReconversao));
            var tp = new TestTenantProvider(TenantId);
            var cu = new TestCurrentUser(UserId);

            var pipelineId = Guid.NewGuid();
            var etapaId = Guid.NewGuid();
            var criar = await new CriarCrmLeadCommandHandler(context, tp, cu).Handle(
                new CriarCrmLeadCommand(pipelineId, etapaId, "Maria", null, null, "maria@x.com", null, "Interesse", 5000m, null, null, null, Guid.NewGuid(), null, null, null),
                CancellationToken.None);
            Assert.True(criar.Sucesso);

            var lead = await context.CrmLeads.FirstAsync();
            var conv = await new ConverterCrmLeadCommandHandler(context, tp, cu).Handle(
                new ConverterCrmLeadCommand(lead.Id, Guid.NewGuid(), null, true, "Deal Maria", 5000m), CancellationToken.None);
            Assert.True(conv.Sucesso);
            Assert.Equal(1, await context.CrmOportunidades.CountAsync());

            // Segunda conversão deve falhar (CRM-011).
            var conv2 = await new ConverterCrmLeadCommandHandler(context, tp, cu).Handle(
                new ConverterCrmLeadCommand(lead.Id, Guid.NewGuid(), null, true, null, null), CancellationToken.None);
            Assert.False(conv2.Sucesso);
        }

        // ---------- Oportunidade ----------

        [Fact(DisplayName = "CrmOportunidade | Sem pipeline/etapa é inválida (CRM-025)")]
        public void Oportunidade_SemPipelineEtapa_Invalida()
        {
            var op = new CrmOportunidade(Guid.Empty, Guid.Empty, "Deal", 1000m, null, null, null, null, null, Guid.NewGuid(), TenantId, UserId);
            Assert.False(op.IsValid);
        }

        [Fact(DisplayName = "CrmOportunidade | Perder grava motivo e status Perdida (CRM-026)")]
        public void Oportunidade_Perder_GravaMotivo()
        {
            var op = new CrmOportunidade(Guid.NewGuid(), Guid.NewGuid(), "Deal", 1000m, null, null, null, null, null, Guid.NewGuid(), TenantId, UserId);
            op.Perder("Preço", UserId);
            Assert.Equal(ECrmOportunidadeStatus.Perdida, op.Status);
            Assert.Equal("Preço", op.MotivoPerda);
        }

        // ---------- Campanha ----------

        [Fact(DisplayName = "CrmCampanha | Data inicial após final é inválida (CRM-039)")]
        public void Campanha_DataInicialAposFinal_Invalida()
        {
            var c = new CrmCampanha("Promo", "Email", "Ativa", new DateTime(2026, 5, 10), new DateTime(2026, 5, 1), null, null, null, null, null, null, null, null, null, TenantId, UserId);
            Assert.False(c.IsValid);
        }

        [Fact(DisplayName = "CrmCampanha | Frequência é limpa quando tipo não é Newsletter (CRM-042)")]
        public void Campanha_FrequenciaLimpa_QuandoNaoNewsletter()
        {
            var c = new CrmCampanha("Promo", "Email", "Ativa", null, new DateTime(2026, 5, 10), "Diaria", null, null, null, null, null, null, null, null, TenantId, UserId);
            Assert.True(c.IsValid);
            Assert.Null(c.Frequencia);

            var nl = new CrmCampanha("News", "Newsletter", "Ativa", null, new DateTime(2026, 5, 10), "Semanal", null, null, null, null, null, null, null, null, TenantId, UserId);
            Assert.Equal("Semanal", nl.Frequencia);
        }

        // ---------- Ticket ----------

        [Fact(DisplayName = "CrmTicketResposta | Nota interna não comunica externamente (CRM-063)")]
        public void TicketResposta_NotaInterna_NaoComunicaExternamente()
        {
            var nota = new CrmTicketResposta(Guid.NewGuid(), "interno", ECrmTipoRespostaTicket.NotaInterna, null, null, null, null, TenantId, UserId);
            var resposta = new CrmTicketResposta(Guid.NewGuid(), "cliente", ECrmTipoRespostaTicket.Resposta, ECrmOrigemAtendimento.Web, null, null, null, TenantId, UserId);
            Assert.True(nota.EhNotaInterna());
            Assert.False(resposta.EhNotaInterna());
        }

        [Fact(DisplayName = "CrmTicket | Sem título é inválido (CRM-060)")]
        public void Ticket_SemTitulo_Invalido()
        {
            var t = new CrmTicket(null, "", null, Guid.NewGuid(), null, null, null, null, null, null, null, null, TenantId, UserId);
            Assert.False(t.IsValid);
        }

        // ---------- Setup / tenant ----------

        [Fact(DisplayName = "CrmPipeline criado grava tenant e autor (CRM-002)")]
        public async Task CriarPipeline_GravaTenantEAutor()
        {
            var context = CreateContext(nameof(CriarPipeline_GravaTenantEAutor));
            var result = await new CriarCrmPipelineCommandHandler(context, new TestTenantProvider(TenantId), new TestCurrentUser(UserId))
                .Handle(new CriarCrmPipelineCommand("Funil Vendas", Guid.NewGuid()), CancellationToken.None);
            Assert.True(result.Sucesso);
            var p = await context.CrmPipelines.FirstAsync();
            Assert.Equal(TenantId, p.TenantId);
            Assert.Equal(UserId, p.CriadoPor);
        }

        // ---------- Helpers ----------

        private static ContextVendas CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextVendas>().UseInMemoryDatabase(dbName).Options;
            return new ContextVendas(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _t;
            public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _u;
            public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
