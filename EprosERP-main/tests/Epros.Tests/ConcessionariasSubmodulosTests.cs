using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.DMS.Application.Commands;
using Epros.Modules.DMS.Application.Handlers;
using Epros.Modules.DMS.Domain.Entities;
using Epros.Modules.DMS.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes das regras-chave dos submódulos de CONCESSIONÁRIAS (CON-CRM, CON-VEN, CON-SRV,
    /// CON-MNT, CON-PES, CON-GAR, CON-DEV, CON-FIN) que estendem o módulo Epros.Modules.DMS.
    /// </summary>
    public class ConcessionariasSubmodulosTests
    {
        private static ContextDMS NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextDMS>()
                .UseInMemoryDatabase(db)
                .Options;
            return new ContextDMS(options, new ConcTenantProvider("tenant-1"), new ConcCurrentUser("user-1"));
        }

        // ---------- CON-VEN ----------

        [Fact(DisplayName = "CON-VEN | EstoqueVeiculo | Cria unidade com VIN de 17 caracteres")]
        public async Task Deve_Criar_Estoque_Veiculo()
        {
            using var context = NovoContexto("db_con_ven_estoque");
            var handler = new CriarEstoqueVeiculoCommandHandler(context, new ConcTenantProvider("tenant-1"), new ConcCurrentUser("user-1"));
            var command = new CriarEstoqueVeiculoCommand(Guid.NewGuid(), "9BWZZZ372HP123456", Guid.NewGuid(), 82000m, 95000m, DateTime.UtcNow);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);
            var unidade = await context.EstoqueVeiculos.SingleAsync();
            Assert.Equal("Livre", unidade.Status);
            Assert.Equal("9BWZZZ372HP123456", unidade.ChassiVin);
        }

        [Fact(DisplayName = "CON-VEN | EstoqueVeiculo | VIN inválido é rejeitado")]
        public void Deve_Rejeitar_Vin_Invalido()
        {
            var unidade = new EstoqueVeiculo(Guid.NewGuid(), "VINCURTO", Guid.NewGuid(), null, null, null, "tenant-1", "user-1");
            Assert.False(unidade.IsValid);
        }

        [Fact(DisplayName = "CON-VEN | EstoqueVeiculo | Só unidade livre pode ser reservada")]
        public void Nao_Deve_Reservar_Unidade_Nao_Livre()
        {
            var unidade = new EstoqueVeiculo(Guid.NewGuid(), "9BWZZZ372HP123456", Guid.NewGuid(), null, null, null, "tenant-1", "user-1");
            unidade.Reservar("user-1");
            unidade.Reservar("user-1"); // segunda reserva inválida

            Assert.False(unidade.IsValid);
            Assert.Equal("Reservado", unidade.Status);
        }

        [Fact(DisplayName = "CON-VEN | PropostaVenda | ValorFinal = Valor - Desconto e aceite só de emitida")]
        public void Proposta_Calcula_Valor_Final_E_Controla_Aceite()
        {
            var proposta = new PropostaVenda(Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow.AddDays(5), 100000m, 8000m, "tenant-1", "user-1");
            Assert.True(proposta.IsValid);
            Assert.Equal(92000m, proposta.ValorFinal);

            proposta.Aceitar("user-1");
            Assert.Equal("Aceita", proposta.Status);

            proposta.Aceitar("user-1"); // já aceita -> inválido
            Assert.False(proposta.IsValid);
        }

        // ---------- CON-CRM ----------

        [Fact(DisplayName = "CON-CRM | TestDrive | Fim anterior ao início é rejeitado")]
        public void TestDrive_Fim_Antes_Do_Inicio_Invalido()
        {
            var inicio = DateTime.UtcNow;
            var td = new TestDrive(Guid.NewGuid(), Guid.NewGuid(), inicio, inicio.AddHours(-1), "tenant-1", "user-1");
            Assert.False(td.IsValid);
        }

        [Fact(DisplayName = "CON-CRM | Oportunidade | Conversão só ocorre uma vez")]
        public void Oportunidade_Converte_Uma_Vez()
        {
            var op = new OportunidadeConcessionaria(Guid.NewGuid(), null, null, "tenant-1", "user-1");
            op.Converter(Guid.NewGuid(), "user-1");
            Assert.Equal("Convertida", op.Etapa);

            op.Converter(Guid.NewGuid(), "user-1"); // segunda conversão bloqueada
            Assert.False(op.IsValid);
        }

        // ---------- CON-GAR ----------

        [Fact(DisplayName = "CON-GAR | PlanoGarantia | Duração e tipo de duração validados")]
        public void PlanoGarantia_Valida_Duracao()
        {
            var invalido = new PlanoGarantia("PL-01", "Fábrica", null, 0, "Semanas", "tenant-1", "user-1");
            Assert.False(invalido.IsValid);

            var valido = new PlanoGarantia("PL-02", "Fábrica", null, 36, "Meses", "tenant-1", "user-1");
            Assert.True(valido.IsValid);
        }

        [Fact(DisplayName = "CON-GAR | SolicitacaoGarantia | Julgamento só de solicitação aberta")]
        public void SolicitacaoGarantia_Julga_Uma_Vez()
        {
            var s = new SolicitacaoGarantia(Guid.NewGuid(), "PROTO-1", DateTime.UtcNow, 12000m, "Ruído no motor", "Cliente relata barulho", null, "tenant-1", "user-1");
            s.Aprovar("user-1");
            Assert.Equal("Aprovada", s.Status);

            s.Rejeitar("user-1"); // já julgada -> inválido
            Assert.False(s.IsValid);
        }

        // ---------- CON-MNT ----------

        [Fact(DisplayName = "CON-MNT | OrdemServicoManutencao | Pessoa é obrigatória (RN-MNT-001)")]
        public void OsManutencao_Exige_Pessoa()
        {
            var os = new OrdemServicoManutencao(Guid.Empty, null, Guid.NewGuid(), "9BWZZZ372HP123456", "ABC1D23", 55000m, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, null, "tenant-1", "user-1");
            Assert.False(os.IsValid);
        }

        // ---------- CON-PES ----------

        [Fact(DisplayName = "CON-PES | DemandaPeca | Quantidade deve ser maior que zero")]
        public void DemandaPeca_Exige_Quantidade_Positiva()
        {
            var d = new DemandaPeca(Guid.NewGuid(), Guid.NewGuid(), "OrdemServico", Guid.NewGuid(), Guid.NewGuid(), 0m, DateTime.UtcNow.AddDays(3), "tenant-1", "user-1");
            Assert.False(d.IsValid);
        }

        // ---------- CON-FIN ----------

        [Fact(DisplayName = "CON-FIN | SimulacaoFin | Preço do veículo deve ser positivo")]
        public void SimulacaoFin_Exige_Preco_Positivo()
        {
            var s = new SimulacaoFin(Guid.NewGuid(), "idem-1", 0m, 10000m, 90000m, 48, "Meses", null, null, "tenant-1", "user-1");
            Assert.False(s.IsValid);
        }

        [Fact(DisplayName = "CON-FIN | ContratoFin | Cria contrato e liquida")]
        public async Task ContratoFin_Cria_E_Liquida()
        {
            using var context = NovoContexto("db_con_fin_contrato");
            var handler = new CriarContratoFinCommandHandler(context, new ConcTenantProvider("tenant-1"), new ConcCurrentUser("user-1"));
            var command = new CriarContratoFinCommand(null, Guid.NewGuid(), "CT-2026-001", null);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);
            var contrato = await context.ContratosFin.SingleAsync();
            Assert.Equal("Ativo", contrato.Status);
        }

        // ---------- helpers ----------

        private sealed class ConcTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public ConcTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
        }

        private sealed class ConcCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public ConcCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "test_user";
            public string? GetUserEmail() => "test@epros.com.br";
        }
    }
}
