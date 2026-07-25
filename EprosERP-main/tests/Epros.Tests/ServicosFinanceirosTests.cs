using System;
using System.Linq;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Domain.Enums;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes de domínio FIN-SF (Serviços Financeiros — cobrança/boleto/remessa/cobrança por e-mail).
    /// Cobrem regras RSF-004/005/006/013/014/017/018/019/023/030-036.
    /// </summary>
    public class ServicosFinanceirosTests
    {
        private const string TenantId = "tenant-sf-001";
        private const string UserId = "user-sf-001";

        [Fact]
        public void GrupoRecorrencia_Invalido_QuandoMesesForaDoDominio()
        {
            var grupo = new GrupoRecorrencia("Mensal", 5, 10, 100m, TenantId, UserId); // 5 não permitido
            Assert.False(grupo.IsValid);
        }

        [Fact]
        public void GrupoRecorrencia_Invalido_QuandoDiaVencimentoForaDeFaixa()
        {
            var grupo = new GrupoRecorrencia("Mensal", 12, 32, 100m, TenantId, UserId);
            Assert.False(grupo.IsValid);
        }

        [Fact]
        public void GrupoRecorrencia_Valido_ComIntervaloEDiaCorretos()
        {
            var grupo = new GrupoRecorrencia("Anual", 12, 15, 250m, TenantId, UserId);
            Assert.True(grupo.IsValid);
        }

        [Fact]
        public void Fatura_Nasce_Pendente()
        {
            var f = NovaFatura();
            Assert.True(f.IsValid);
            Assert.Equal(ESituacaoFaturaCobranca.Pendente, f.Situacao); // RSF-004
            Assert.True(f.ElegivelRemessa);
            Assert.True(f.ElegivelBoleto);
        }

        [Fact]
        public void Fatura_AtualizarVencimento_MarcaVencida()
        {
            var f = new FaturaCobranca(Guid.NewGuid(), null, "REF", null, DateTime.UtcNow.AddDays(-10),
                DateTime.UtcNow.AddDays(-5), 200m, null, ETipoFaturaCobranca.Avulsa, TenantId, UserId);
            f.AtualizarVencimento(DateTime.UtcNow, UserId);
            Assert.Equal(ESituacaoFaturaCobranca.Vencida, f.Situacao); // RSF-005
        }

        [Fact]
        public void Fatura_Baixada_NaoRetornaEBloqueiaNovaBaixa()
        {
            var f = NovaFatura();
            f.Baixar(DateTime.UtcNow, 200m, UserId);
            Assert.Equal(ESituacaoFaturaCobranca.Baixada, f.Situacao);
            Assert.False(f.ElegivelBoleto); // RSF-023
            Assert.False(f.ElegivelRemessa); // RSF-013

            f.AtualizarVencimento(DateTime.UtcNow.AddDays(10), UserId); // rotina não reverte
            Assert.Equal(ESituacaoFaturaCobranca.Baixada, f.Situacao); // RSF-006
        }

        [Fact]
        public void Fatura_MarcarRemetida_TornaInelegivelParaNovaRemessa()
        {
            var f = NovaFatura();
            f.MarcarRemetida(UserId); // RSF-014
            Assert.True(f.Remetida);
            Assert.False(f.ElegivelRemessa);
        }

        [Fact]
        public void Sacado_Bloqueado_ExpoeFlag()
        {
            var s = new Sacado(null, null, "Cliente X", "123", null, null, null, null, null, null, null, null, null, null, "x@y.com", null, 0m, TenantId, UserId);
            Assert.True(s.IsValid);
            s.Bloquear(UserId);
            Assert.True(s.Bloqueado); // RSF-019 (checagem no handler)
        }

        [Fact]
        public void ContaEmissora_GeraNossoNumeroSequencial()
        {
            var c = new ContaEmissora(Guid.NewGuid(), null, "Banco", "1", "0001", "0", "123", "4", "DM", 100,
                "01", "conv", "contr", "SR", 0, "CNAB240", null, null, false, TenantId, UserId);
            Assert.Equal(101, c.GerarProximoNossoNumero(UserId));
            Assert.Equal(102, c.GerarProximoNossoNumero(UserId));
        }

        [Fact]
        public void Remessa_AdicionarBoleto_Idempotente()
        {
            var r = new Remessa("REM-001.txt", DateTime.UtcNow, 0, ELayoutCnab.Cnab240, Guid.NewGuid(), TenantId, UserId);
            var boletoId = Guid.NewGuid();
            r.AdicionarBoleto(boletoId, Guid.NewGuid(), 100m, UserId);
            r.AdicionarBoleto(boletoId, Guid.NewGuid(), 100m, UserId); // duplicado ignorado
            Assert.Single(r.Boletos);
            Assert.Equal(1, r.QuantidadeTitulos);
            Assert.Equal(100m, r.ValorTotal);
        }

        [Fact]
        public void CobrancaEmail_CicloDeStatus()
        {
            var c = new CobrancaEmail(null, "Cliente", 300m, "07/2026", null, null, null, null, "c@x.com", TenantId, UserId);
            Assert.Equal(EStatusCobrancaEmail.Encubada, c.Status); // RSF-030
            c.EnviarPrimeiraCobranca(UserId);
            Assert.Equal(EStatusCobrancaEmail.EmAndamento, c.Status); // RSF-031
            c.Recobrar(UserId);
            Assert.Equal(EStatusCobrancaEmail.Recobrado, c.Status); // RSF-032
            c.ConfirmarPagamento("comprovante.png", UserId);
            Assert.Equal(EStatusCobrancaEmail.AguardandoValidacao, c.Status); // RSF-034
            c.ValidarPagamento(UserId);
            Assert.Equal(EStatusCobrancaEmail.Finalizada, c.Status); // RSF-035
        }

        [Fact]
        public void CobrancaEmail_Confirmar_ExigeComprovante()
        {
            var c = new CobrancaEmail(null, "Cliente", 300m, null, null, null, null, null, "c@x.com", TenantId, UserId);
            c.ConfirmarPagamento("", UserId);
            Assert.False(c.IsValid); // RSF-036
        }

        private static FaturaCobranca NovaFatura()
            => new FaturaCobranca(Guid.NewGuid(), null, "REF-1", "DOC-1", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 200m, "a@b.com", ETipoFaturaCobranca.Avulsa, TenantId, UserId);
    }
}
