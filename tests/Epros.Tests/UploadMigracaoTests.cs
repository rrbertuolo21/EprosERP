using System;
using System.Linq;
using System.Text;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Domain.Entities.Upload;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Modules.Aplicativo.Infrastructure.Services;
using Xunit;

namespace Epros.Tests
{
    public class UploadMigracaoTests
    {
        private const string TenantId = "tenant-upl-001";
        private const string UserId = "user-upl-001";

        [Fact(DisplayName = "UplExecucaoImportacao | Todas as linhas com sucesso deve resultar Passed")]
        public void Importacao_TudoSucesso_DevePassed()
        {
            var e = new UplExecucaoImportacao(Guid.NewGuid(), "IMP-1", "clients", null, null, "clientes.csv", TenantId, UserId);
            e.Finalizar(totalLinhas: 3, linhasSucesso: 3, linhasIgnoradas: 0, quantidadeErros: 0, UserId);
            Assert.Equal(EUplResultadoImportacao.Passed, e.Resultado);
            Assert.Equal(EUplStatusImportacao.Finalizada, e.Status);
        }

        [Fact(DisplayName = "UplExecucaoImportacao | Sucesso parcial deve resultar Partial")]
        public void Importacao_Parcial_DevePartial()
        {
            var e = new UplExecucaoImportacao(Guid.NewGuid(), "IMP-2", "clients", null, null, "clientes.csv", TenantId, UserId);
            e.Finalizar(totalLinhas: 3, linhasSucesso: 2, linhasIgnoradas: 0, quantidadeErros: 1, UserId);
            Assert.Equal(EUplResultadoImportacao.Partial, e.Resultado);
            Assert.Equal(EUplStatusImportacao.Parcial, e.Status);
        }

        [Fact(DisplayName = "UplExecucaoImportacao | Nenhum sucesso deve resultar Failed")]
        public void Importacao_SemSucesso_DeveFailed()
        {
            var e = new UplExecucaoImportacao(Guid.NewGuid(), "IMP-3", "clients", null, null, "clientes.csv", TenantId, UserId);
            e.Finalizar(totalLinhas: 3, linhasSucesso: 0, linhasIgnoradas: 0, quantidadeErros: 3, UserId);
            Assert.Equal(EUplResultadoImportacao.Failed, e.Resultado);
            Assert.Equal(EUplStatusImportacao.Erro, e.Status);
        }

        [Fact(DisplayName = "UplExecucaoImportacao | Import ref vazio deve ser inválida")]
        public void Importacao_ImportRefVazio_DeveSerInvalida()
        {
            var e = new UplExecucaoImportacao(Guid.NewGuid(), "", "clients", null, null, "clientes.csv", TenantId, UserId);
            Assert.False(e.IsValid);
        }

        [Fact(DisplayName = "UplImportacaoLinha | Linha de dados deve iniciar em 2")]
        public void ImportacaoLinha_Linha1_DeveSerInvalida()
        {
            var l = new UplImportacaoLinha(Guid.NewGuid(), 1, EUplStatusLinha.Importada, "clients", null, null, TenantId, UserId);
            Assert.False(l.IsValid);
        }

        [Fact(DisplayName = "LeitorTabular | CSV com cabeçalho deve ler dados a partir da linha 2")]
        public void LeitorCsv_DeveLerDados()
        {
            var csv = "nome,email\nAcme,acme@x.com\nBeta,beta@x.com";
            var bytes = Encoding.UTF8.GetBytes(csv);
            var linhas = LeitorTabularService.Ler(bytes, "csv");
            Assert.Equal(2, linhas.Count);
            Assert.Equal(2, linhas[0].NumeroLinha);
            Assert.Equal("Acme", linhas[0].Valores["nome"]);
            Assert.Equal("beta@x.com", linhas[1].Valores["email"]);
        }

        [Fact(DisplayName = "LeitorTabular | CSV com aspas e vírgula interna deve preservar campo")]
        public void LeitorCsv_CampoComAspas_DevePreservar()
        {
            var csv = "nome,obs\n\"Acme, Ltda\",teste";
            var bytes = Encoding.UTF8.GetBytes(csv);
            var linhas = LeitorTabularService.Ler(bytes, "csv");
            Assert.Single(linhas);
            Assert.Equal("Acme, Ltda", linhas[0].Valores["nome"]);
        }

        [Fact(DisplayName = "LeitorTabular | Extensão não suportada deve lançar")]
        public void LeitorTabular_ExtensaoInvalida_DeveLancar()
        {
            Assert.Throws<NotSupportedException>(() => LeitorTabularService.Ler(new byte[] { 1, 2, 3 }, "pdf"));
        }

        [Fact(DisplayName = "Armazenamento | Nome iniciado por ponto deve ser saneado")]
        public void Armazenamento_NomeOculto_DeveSanear()
        {
            var svc = new ArmazenamentoLocalArquivoService();
            var nome = svc.SanitizarNome(".htaccess");
            Assert.False(nome.StartsWith("."));
        }

        [Fact(DisplayName = "Armazenamento | Mesmo conteúdo deve gerar mesmo hash (dedup)")]
        public void Armazenamento_MesmoConteudo_MesmoHash()
        {
            var svc = new ArmazenamentoLocalArquivoService();
            var a = svc.CalcularHash(Encoding.UTF8.GetBytes("conteudo"));
            var b = svc.CalcularHash(Encoding.UTF8.GetBytes("conteudo"));
            Assert.Equal(a, b);
        }

        [Fact(DisplayName = "SolicitacaoUpgradeVersao | Aprovar pelo solicitante deve falhar (segregação)")]
        public void Upgrade_AprovarPeloSolicitante_DeveFalhar()
        {
            var s = new SolicitacaoUpgradeVersao("1.0", "1.1", "correções", "admin-a", true, "system", "admin-a");
            s.Aprovar("admin-a", null, "admin-a");
            Assert.False(s.IsValid);
        }

        [Fact(DisplayName = "SolicitacaoUpgradeVersao | Aprovar por outro admin deve ir para Aprovado")]
        public void Upgrade_AprovarPorOutro_DeveAprovar()
        {
            var s = new SolicitacaoUpgradeVersao("1.0", "1.1", "correções", "admin-a", true, "system", "admin-a");
            s.Aprovar("admin-b", "ok", "admin-b");
            Assert.True(s.IsValid);
            Assert.Equal(EStatusUpgradeVersao.Aprovado, s.Status);
        }

        [Fact(DisplayName = "SolicitacaoUpgradeVersao | Executar sem aprovação deve falhar")]
        public void Upgrade_ExecutarSemAprovacao_DeveFalhar()
        {
            var s = new SolicitacaoUpgradeVersao("1.0", "1.1", "correções", "admin-a", true, "system", "admin-a");
            s.IniciarExecucao("admin-b");
            Assert.False(s.IsValid);
        }
    }
}
