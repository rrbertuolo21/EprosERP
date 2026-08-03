using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Domain.Services.Cnab;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// FIN-SF — CNAB remessa/retorno. Testa a ESTRUTURA universal do formato (registros header/detalhe/
    /// trailer, largura fixa 240/400, sequenciais, totalização, detecção de layout e round-trip
    /// gerar→ler). As POSIÇÕES/campos exatos por banco são convenção bancária (valida-contador); aqui
    /// exercita-se o layout de referência simétrico writer↔reader.
    /// </summary>
    public class CnabTests
    {
        private static readonly DadosCedenteCnab Cedente = new(
            CodigoBanco: "341", NomeBanco: "ITAU", NomeCedente: "EMPRESA TESTE LTDA", DocumentoCedente: "12345678000199",
            Agencia: "1234", DigitoAgencia: "5", Conta: "56789", DigitoConta: "0", Carteira: "109", Convenio: "123456");

        private static List<TituloRemessaCnab> Titulos() => new()
        {
            new TituloRemessaCnab(1001, "DOC001", 150.75m, new DateTime(2026, 9, 10), new DateTime(2026, 8, 1),
                "CLIENTE UM", "11122233344", "RUA A, 100", "CENTRO", "SAO PAULO", "SP", "01000000"),
            new TituloRemessaCnab(1002, "DOC002", 2300.00m, new DateTime(2026, 9, 20), new DateTime(2026, 8, 1),
                "CLIENTE DOIS", "99988877766", "RUA B, 200", "JARDINS", "CAMPINAS", "SP", "13000000"),
        };

        [Fact(DisplayName = "CNAB400 | remessa = header + N detalhes + trailer, cada linha com 400 posições")]
        public void Remessa400_Estrutura()
        {
            var conteudo = CnabWriter.GerarRemessa400(Cedente, Titulos(), 1, new DateTime(2026, 8, 1));
            var linhas = conteudo.Replace("\r\n", "\n").Split('\n').Where(l => l.Length > 0).ToList();

            Assert.Equal(4, linhas.Count);              // 1 header + 2 detalhes + 1 trailer
            Assert.All(linhas, l => Assert.Equal(400, l.Length));
            Assert.Equal('0', linhas[0][0]);            // header tipo 0
            Assert.Equal('1', linhas[1][0]);            // detalhe tipo 1
            Assert.Equal('9', linhas[^1][0]);           // trailer tipo 9
        }

        [Fact(DisplayName = "CNAB400 | round-trip: ler a remessa recupera nosso número, valor e vencimento")]
        public void Remessa400_RoundTrip()
        {
            var titulos = Titulos();
            var conteudo = CnabWriter.GerarRemessa400(Cedente, titulos, 1, new DateTime(2026, 8, 1));
            var lidos = CnabWriter.LerRemessa400(conteudo);

            Assert.Equal(titulos.Count, lidos.Count);
            for (var i = 0; i < titulos.Count; i++)
            {
                Assert.Equal(titulos[i].NossoNumero, lidos[i].NossoNumero);
                Assert.Equal(titulos[i].Valor, lidos[i].Valor);
                Assert.Equal(titulos[i].DataVencimento.Date, lidos[i].DataVencimento.Date);
            }
        }

        [Fact(DisplayName = "CNAB240 | remessa tem header arquivo, header lote, P+Q por título e trailers, linhas de 240")]
        public void Remessa240_Estrutura()
        {
            var conteudo = CnabWriter.GerarRemessa240(Cedente, Titulos(), 1, new DateTime(2026, 8, 1));
            var linhas = conteudo.Replace("\r\n", "\n").Split('\n').Where(l => l.Length > 0).ToList();

            // header arquivo + header lote + 2*(P+Q) + trailer lote + trailer arquivo = 8
            Assert.Equal(8, linhas.Count);
            Assert.All(linhas, l => Assert.Equal(240, l.Length));
            Assert.Equal("P", linhas[2].Substring(13, 1)); // segmento P do 1º título
            Assert.Equal("Q", linhas[3].Substring(13, 1)); // segmento Q do 1º título
        }

        [Fact(DisplayName = "CNAB | detecção de layout por largura (RSF-009/010)")]
        public void Detectar_Layout()
        {
            var r400 = CnabWriter.GerarRemessa400(Cedente, Titulos(), 1, new DateTime(2026, 8, 1));
            var r240 = CnabWriter.GerarRemessa240(Cedente, Titulos(), 1, new DateTime(2026, 8, 1));
            Assert.Equal(ELayoutCnab.Cnab400, CnabRetornoParser.DetectarLayout(r400));
            Assert.Equal(ELayoutCnab.Cnab240, CnabRetornoParser.DetectarLayout(r240));
        }

        [Fact(DisplayName = "CNAB | arquivo vazio/ilegível é rejeitado (RSF-008)")]
        public void Retorno_Vazio_Rejeitado()
        {
            Assert.Throws<ArgumentException>(() => CnabRetornoParser.DetectarLayout(""));
            Assert.Throws<ArgumentException>(() => CnabRetornoParser.DetectarLayout("linha curta\r\n"));
        }

        [Fact(DisplayName = "CNAB400 | retorno round-trip: simular liquidação e parsear recupera baixa")]
        public void Retorno400_RoundTrip()
        {
            var ocorrencias = new[]
            {
                new OcorrenciaRetornoCnab(1001, "DOC001", 150.75m, 150.75m, 2.50m, new DateTime(2026, 9, 11), "06", true),
                new OcorrenciaRetornoCnab(1002, "DOC002", 2300.00m, 2300.00m, 2.50m, new DateTime(2026, 9, 21), "06", true),
            };
            var arquivo = CnabRetornoParser.SimularRetorno400(ocorrencias);
            var parsed = CnabRetornoParser.ParsearRetorno(arquivo);

            Assert.Equal(ELayoutCnab.Cnab400, parsed.Layout);
            Assert.Equal(2, parsed.Ocorrencias.Count);
            Assert.All(parsed.Ocorrencias, o => Assert.True(o.Liquidado));
            Assert.Equal(1001, parsed.Ocorrencias[0].NossoNumero);
            Assert.Equal(150.75m, parsed.Ocorrencias[0].ValorPago);
            Assert.Equal(2300.00m, parsed.Ocorrencias[1].ValorPago);
        }

        [Fact(DisplayName = "CNAB240 | retorno round-trip: segmentos T/U recuperam nosso número e valor pago")]
        public void Retorno240_RoundTrip()
        {
            var ocorrencias = new[]
            {
                new OcorrenciaRetornoCnab(2001, "DOC010", 500m, 500m, 1.90m, null, "06", true),
            };
            var arquivo = CnabRetornoParser.SimularRetorno240(ocorrencias);
            var parsed = CnabRetornoParser.ParsearRetorno(arquivo);

            Assert.Equal(ELayoutCnab.Cnab240, parsed.Layout);
            Assert.Single(parsed.Ocorrencias);
            Assert.Equal(2001, parsed.Ocorrencias[0].NossoNumero);
            Assert.Equal(500m, parsed.Ocorrencias[0].ValorPago);
            Assert.True(parsed.Ocorrencias[0].Liquidado);
        }

        [Fact(DisplayName = "CNAB | ocorrência não-liquidação não marca baixa")]
        public void Retorno_NaoLiquidacao()
        {
            var ocorrencias = new[]
            {
                new OcorrenciaRetornoCnab(3001, "DOC020", 100m, 0m, 0m, null, "02", false), // 02 = confirmação de entrada
            };
            var arquivo = CnabRetornoParser.SimularRetorno400(ocorrencias);
            var parsed = CnabRetornoParser.ParsearRetorno(arquivo);
            Assert.False(parsed.Ocorrencias[0].Liquidado);
        }
    }
}
