using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Application.Commands;
using Epros.Modules.Qualidade.Application.Handlers;
using Epros.Modules.Qualidade.Domain.Entities;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes das regras-chave dos submodulos portados de Qualidade:
    /// QLD-NCR, QLD-INS, QLD-ACR, QLD-ADM e QLD-ATR.
    /// </summary>
    public class QualidadeSubmodulosTests
    {
        private static ContextQualidade NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextQualidade>()
                .UseInMemoryDatabase(db)
                .Options;
            return new ContextQualidade(options, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
        }

        // ================= QLD-NCR =================
        [Fact]
        public async Task Ncr_Deve_Criar_Registro_Valido_Em_Rascunho()
        {
            using var ctx = NovoContexto(nameof(Ncr_Deve_Criar_Registro_Valido_Em_Rascunho));
            var handler = new CriarNcrCommandHandler(ctx, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            var cmd = new CriarNcrCommand("NCR-001", "Lote fora de especificacao", "Peca com trinca",
                ENcrOrigem.Inspecao, ENcrPrioridade.Alta, Guid.NewGuid(), "Critica", DateTime.UtcNow);

            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.Sucesso);
            var ncr = await ctx.NcrRegistros.FirstAsync();
            Assert.Equal(EStatusRegistroQualidade.Rascunho, ncr.StatusRegistro);
            Assert.Equal(ENcrEtapa.Rascunho, ncr.EtapaNcr);
        }

        [Fact]
        public async Task Ncr_Deve_Bloquear_Codigo_Duplicado_Por_Tenant()
        {
            using var ctx = NovoContexto(nameof(Ncr_Deve_Bloquear_Codigo_Duplicado_Por_Tenant));
            var handler = new CriarNcrCommandHandler(ctx, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            var cmd = new CriarNcrCommand("NCR-DUP", "T", "D", ENcrOrigem.Producao, ENcrPrioridade.Media, Guid.NewGuid(), null, null);

            var primeiro = await handler.Handle(cmd, CancellationToken.None);
            var segundo = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(primeiro.Sucesso);
            Assert.False(segundo.Sucesso);
            Assert.True(segundo.Block);
        }

        [Fact]
        public void Ncr_Encerrar_Sem_Conclusao_Deve_Invalidar()
        {
            var ncr = new NcrRegistro("NCR-002", "T", "D", ENcrOrigem.Auditoria, ENcrPrioridade.Baixa,
                Guid.NewGuid(), null, null, "tenant-1", "user-1");
            ncr.Encerrar("", "user-1");
            Assert.False(ncr.IsValid);
            Assert.NotEqual(EStatusRegistroQualidade.Encerrado, ncr.StatusRegistro);
        }

        [Fact]
        public void Ncr_Cancelar_Exige_Motivo()
        {
            var ncr = new NcrRegistro("NCR-003", "T", "D", ENcrOrigem.Estoque, ENcrPrioridade.Urgente,
                Guid.NewGuid(), null, null, "tenant-1", "user-1");
            ncr.Cancelar("   ", "user-1");
            Assert.False(ncr.IsValid);
        }

        [Fact]
        public void NcrAcaoCapa_Concluir_Com_Evidencia_Obrigatoria_Sem_Resultado_Deve_Falhar()
        {
            var acao = new NcrAcaoCapa(Guid.NewGuid(), ENcrTipoAcao.Corretiva, "Trocar fornecedor",
                Guid.NewGuid(), DateTime.UtcNow.AddDays(5), evidenciaObrigatoria: true, "tenant-1", "user-1");
            acao.Concluir("", "user-1");
            Assert.False(acao.IsValid);
            Assert.NotEqual(ENcrStatusAcao.Concluida, acao.Status);
        }

        // ================= QLD-INS =================
        [Fact]
        public void Plano_Nao_Ativa_Sem_Caracteristica()
        {
            var plano = new PlanoInspecao("PL-001", "Plano recebimento", EContextoPlano.Recebimento,
                Guid.NewGuid(), null, null, null, DateTime.UtcNow, "tenant-1", "user-1");
            plano.Ativar("user-1");
            Assert.False(plano.IsValid);
            Assert.NotEqual(EStatusRegistroQualidade.Ativo, plano.Status);
        }

        [Fact]
        public void Plano_Ativa_Com_Caracteristica()
        {
            var plano = new PlanoInspecao("PL-002", "Plano", EContextoPlano.Produto,
                Guid.NewGuid(), Guid.NewGuid(), null, null, DateTime.UtcNow, "tenant-1", "user-1");
            var carac = new CaracteristicaPlano(plano.Id, 1, "Diametro", ETipoCaracteristica.Dimensional,
                ETipoDadoCaracteristica.Decimal, true, null, Guid.NewGuid(), "10", 9m, 11m, null, "tenant-1", "user-1");
            plano.AdicionarCaracteristica(carac);
            plano.Ativar("user-1");
            Assert.True(plano.IsValid);
            Assert.Equal(EStatusRegistroQualidade.Ativo, plano.Status);
        }

        [Fact]
        public void Caracteristica_Limite_Inferior_Maior_Que_Superior_Deve_Invalidar()
        {
            var carac = new CaracteristicaPlano(Guid.NewGuid(), 1, "Peso", ETipoCaracteristica.Dimensional,
                ETipoDadoCaracteristica.Decimal, true, null, null, null, 20m, 10m, null, "tenant-1", "user-1");
            Assert.False(carac.IsValid);
        }

        [Fact]
        public void ResultadoInspecao_Reprovado_Sem_Conclusao_Deve_Invalidar()
        {
            var res = new ResultadoInspecao(Guid.NewGuid(), EResultadoInspecaoConsolidado.Reprovado,
                10, 3, gerarAcr: true, gerarNcr: true, Guid.NewGuid(), null, null, "tenant-1", "user-1");
            Assert.False(res.IsValid);
        }

        // ================= QLD-ACR =================
        [Fact]
        public void AcrResultado_Rejeitado_Sem_Motivo_Deve_Invalidar()
        {
            var res = new AcrResultado(Guid.NewGuid(), EResultadoAcr.Rejeitado, motivoId: null, null,
                null, null, null, false, "tenant-1", "user-1");
            Assert.False(res.IsValid);
        }

        [Fact]
        public void AcrResultado_Aceito_Sem_Motivo_Eh_Valido()
        {
            var res = new AcrResultado(Guid.NewGuid(), EResultadoAcr.Aceito, motivoId: null, null,
                null, null, null, false, "tenant-1", "user-1");
            Assert.True(res.IsValid);
        }

        [Fact]
        public void AcrItem_Quantidade_Analisada_Deve_Ser_Maior_Que_Zero()
        {
            var item = new AcrItem(Guid.NewGuid(), 1, quantidadeAnalisada: 0m, "UN", false,
                null, null, null, null, null, null, null, null, null, "tenant-1", "user-1");
            Assert.False(item.IsValid);
        }

        [Fact]
        public async Task Acr_Submeter_Sem_Item_Deve_Falhar()
        {
            using var ctx = NovoContexto(nameof(Acr_Submeter_Sem_Item_Deve_Falhar));
            var analise = new AcrAnalise("ACR-001", "Recebimento fornecedor", ETipoAnaliseAcr.Recebimento,
                Guid.NewGuid(), null, null, "tenant-1", "user-1");
            analise.Submeter("user-1");
            Assert.False(analise.IsValid);
            Assert.NotEqual(EStatusRegistroQualidade.EmAnalise, analise.Status);
        }

        // ================= QLD-ADM =================
        [Fact]
        public void Adm_Maquina_De_Estados_Submeter_E_Aprovar()
        {
            var reg = new AdmQualidade("ADM-001", "SGQ Matriz", Guid.NewGuid(), "tenant-1", "user-1");
            reg.Submeter("user-1");
            Assert.Equal(EStatusRegistroQualidade.EmAnalise, reg.Status);
            reg.Aprovar("user-1");
            Assert.Equal(EStatusRegistroQualidade.Ativo, reg.Status);
        }

        [Fact]
        public void Adm_Transicao_Invalida_Deve_Bloquear()
        {
            var reg = new AdmQualidade("ADM-002", "SGQ", Guid.NewGuid(), "tenant-1", "user-1");
            // Rascunho -> Aprovar (invalido, precisa submeter antes)
            reg.Aprovar("user-1");
            Assert.False(reg.IsValid);
            Assert.Equal(EStatusRegistroQualidade.Rascunho, reg.Status);
        }

        // ================= QLD-ATR =================
        [Fact]
        public void Atributo_Qualidade_Sem_TipoCaracteristica_Deve_Invalidar()
        {
            var attr = new AtrAtributo("ATR-001", "espessura", "Espessura", ETipoAtributo.Qualidade,
                ETipoDadoAtributo.Decimal, EEscopoAtributo.Produto, true, true, tipoCaracteristica: null,
                false, 1, null, "tenant-1", "user-1");
            Assert.False(attr.IsValid);
        }

        [Fact]
        public void Atributo_Qualidade_Com_TipoCaracteristica_Eh_Valido()
        {
            var attr = new AtrAtributo("ATR-002", "espessura", "Espessura", ETipoAtributo.Qualidade,
                ETipoDadoAtributo.Decimal, EEscopoAtributo.Produto, true, true,
                ETipoCaracteristica.Dimensional, false, 1, null, "tenant-1", "user-1");
            Assert.True(attr.IsValid);
        }

        [Fact]
        public void Especificacao_Limite_Inferior_Maior_Que_Superior_Deve_Invalidar()
        {
            var esp = new AtrEspecificacao(Guid.NewGuid(), "1", DateTime.UtcNow, null, null, "5", 30m, 10m,
                null, null, "mm", null, null, "tenant-1", "user-1");
            Assert.False(esp.IsValid);
        }

        [Fact]
        public void Especificacao_Com_Limite_Sem_Unidade_Deve_Invalidar()
        {
            var esp = new AtrEspecificacao(Guid.NewGuid(), "1", DateTime.UtcNow, null, null, null, 10m, 20m,
                null, null, unidade: null, null, null, "tenant-1", "user-1");
            Assert.False(esp.IsValid);
        }

        [Fact]
        public void Valor_Texto_Acima_De_255_Deve_Invalidar()
        {
            var texto = new string('x', 256);
            var valor = new AtrValor(Guid.NewGuid(), "Produto", Guid.NewGuid(), texto, null, null, null, null,
                null, "tenant-1", "user-1");
            Assert.False(valor.IsValid);
        }

        [Fact]
        public async Task Atributo_Deve_Bloquear_NomeInterno_Duplicado_No_Escopo()
        {
            using var ctx = NovoContexto(nameof(Atributo_Deve_Bloquear_NomeInterno_Duplicado_No_Escopo));
            var handler = new CriarAtributoCommandHandler(ctx, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            var cmd1 = new CriarAtributoCommand("ATR-A", "cor", "Cor", ETipoAtributo.Comercial,
                ETipoDadoAtributo.Texto, EEscopoAtributo.Produto, true, false, null, false, 1, null);
            var cmd2 = new CriarAtributoCommand("ATR-B", "cor", "Cor 2", ETipoAtributo.Comercial,
                ETipoDadoAtributo.Texto, EEscopoAtributo.Produto, true, false, null, false, 2, null);

            var r1 = await handler.Handle(cmd1, CancellationToken.None);
            var r2 = await handler.Handle(cmd2, CancellationToken.None);

            Assert.True(r1.Sucesso);
            Assert.False(r2.Sucesso);
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
