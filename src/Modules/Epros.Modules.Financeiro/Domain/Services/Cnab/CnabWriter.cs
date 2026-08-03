using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epros.Modules.Financeiro.Domain.Enums;

namespace Epros.Modules.Financeiro.Domain.Services.Cnab
{
    /// <summary>
    /// Gerador de arquivos de REMESSA bancária CNAB (FIN-SF §7.5). Implementa a ESTRUTURA universal do
    /// formato — sequência de registros header → detalhes → trailer, largura fixa, sequenciais e
    /// totalizações — para os layouts CNAB 400 e CNAB 240 (FEBRABAN). As POSIÇÕES/campos exatos de cada
    /// campo variam por banco (001, 033, 104, 237, 341, 748, 756…) e são convenção bancária homologada
    /// (// valida-contador por banco/layout); aqui adota-se um layout de REFERÊNCIA consistente
    /// (writer e reader simétricos), suficiente para exercitar e testar a estrutura round-trip.
    /// </summary>
    public static class CnabWriter
    {
        public const string CRLF = "\r\n";

        // ----- Posições de referência do detalhe CNAB 400 (1-based) -----
        internal const int D400_NOSSONUMERO_INI = 63, D400_NOSSONUMERO_LEN = 17;
        internal const int D400_OCORRENCIA_INI = 109, D400_OCORRENCIA_LEN = 2;
        internal const int D400_NUMDOC_INI = 111, D400_NUMDOC_LEN = 10;
        internal const int D400_VENCIMENTO_INI = 121; // ddMMyy (6)
        internal const int D400_VALOR_INI = 127, D400_VALOR_LEN = 13;
        internal const int D400_SACADO_DOC_INI = 220, D400_SACADO_DOC_LEN = 14;
        internal const int D400_SACADO_NOME_INI = 235, D400_SACADO_NOME_LEN = 40;
        internal const int D400_SEQ_INI = 395, D400_SEQ_LEN = 6;

        /// <summary>
        /// Gera o conteúdo de uma remessa CNAB 400: 1 header (tipo 0) + N detalhes (tipo 1) + 1 trailer
        /// (tipo 9), cada linha com 400 posições, sequencial 1..M no final de cada registro.
        /// </summary>
        public static string GerarRemessa400(DadosCedenteCnab cedente, IReadOnlyList<TituloRemessaCnab> titulos, int sequencialRemessa, DateTime dataGeracao)
        {
            if (cedente == null) throw new ArgumentNullException(nameof(cedente));
            if (titulos == null || titulos.Count == 0) throw new ArgumentException("Remessa sem títulos.", nameof(titulos));

            var linhas = new List<string>(titulos.Count + 2);
            var seq = 1;

            // Header (tipo 0)
            var header = new StringBuilder(new string(' ', 400));
            Set(header, 1, "0");
            Set(header, 2, "1");                                   // remessa
            Set(header, 3, CnabCampo.Alfa("REMESSA", 7));
            Set(header, 11, CnabCampo.Alfa("01COBRANCA", 15));
            Set(header, 27, CnabCampo.NumStr(cedente.Agencia, 5));
            Set(header, 32, CnabCampo.NumStr(cedente.Conta, 12));
            Set(header, 47, CnabCampo.Alfa(cedente.NomeCedente, 30));
            Set(header, 77, CnabCampo.NumStr(cedente.CodigoBanco, 3));
            Set(header, 80, CnabCampo.Alfa(cedente.NomeBanco, 15));
            Set(header, 95, CnabCampo.Data(dataGeracao));
            Set(header, 101, CnabCampo.Num(sequencialRemessa, 7));
            Set(header, D400_SEQ_INI, CnabCampo.Num(seq++, D400_SEQ_LEN));
            linhas.Add(Fixar(header.ToString(), 400));

            // Detalhes (tipo 1)
            decimal total = 0m;
            foreach (var t in titulos)
            {
                var d = new StringBuilder(new string(' ', 400));
                Set(d, 1, "1");
                Set(d, 21, CnabCampo.Alfa(cedente.Carteira, 4));
                Set(d, D400_NOSSONUMERO_INI, CnabCampo.Num(t.NossoNumero, D400_NOSSONUMERO_LEN));
                Set(d, D400_OCORRENCIA_INI, CnabCampo.Alfa(t.CodigoOcorrencia, D400_OCORRENCIA_LEN));
                Set(d, D400_NUMDOC_INI, CnabCampo.Alfa(t.NumeroDocumento, D400_NUMDOC_LEN));
                Set(d, D400_VENCIMENTO_INI, CnabCampo.Data(t.DataVencimento));
                Set(d, D400_VALOR_INI, CnabCampo.Valor(t.Valor, D400_VALOR_LEN));
                Set(d, D400_SACADO_DOC_INI, CnabCampo.NumStr(t.SacadoDocumento, D400_SACADO_DOC_LEN));
                Set(d, D400_SACADO_NOME_INI, CnabCampo.Alfa(t.SacadoNome, D400_SACADO_NOME_LEN));
                Set(d, D400_SEQ_INI, CnabCampo.Num(seq++, D400_SEQ_LEN));
                linhas.Add(Fixar(d.ToString(), 400));
                total += t.Valor;
            }

            // Trailer (tipo 9)
            var trailer = new StringBuilder(new string(' ', 400));
            Set(trailer, 1, "9");
            Set(trailer, 2, CnabCampo.Num(titulos.Count, 6));      // qtd de títulos
            Set(trailer, 8, CnabCampo.Valor(total, 15));           // valor total
            Set(trailer, D400_SEQ_INI, CnabCampo.Num(seq++, D400_SEQ_LEN));
            linhas.Add(Fixar(trailer.ToString(), 400));

            return string.Join(CRLF, linhas) + CRLF;
        }

        /// <summary>
        /// Gera o conteúdo de uma remessa CNAB 240: header de arquivo (0) + header de lote (1) +
        /// segmentos P e Q por título + trailer de lote (5) + trailer de arquivo (9). Linhas de 240.
        /// </summary>
        public static string GerarRemessa240(DadosCedenteCnab cedente, IReadOnlyList<TituloRemessaCnab> titulos, int sequencialRemessa, DateTime dataGeracao)
        {
            if (cedente == null) throw new ArgumentNullException(nameof(cedente));
            if (titulos == null || titulos.Count == 0) throw new ArgumentException("Remessa sem títulos.", nameof(titulos));

            var linhas = new List<string>();

            // Header de arquivo (tipo 0)
            var ha = new StringBuilder(new string(' ', 240));
            Set(ha, 1, CnabCampo.NumStr(cedente.CodigoBanco, 3));
            Set(ha, 4, "0000");
            Set(ha, 8, "0");                                       // registro header arquivo
            Set(ha, 33, CnabCampo.Alfa(cedente.NomeCedente, 30));
            Set(ha, 103, CnabCampo.Alfa(cedente.NomeBanco, 30));
            Set(ha, 158, CnabCampo.Num(sequencialRemessa, 6));
            Set(ha, 144, CnabCampo.Data(dataGeracao, anoCompleto: true));
            linhas.Add(Fixar(ha.ToString(), 240));

            // Header de lote (tipo 1)
            var hl = new StringBuilder(new string(' ', 240));
            Set(hl, 1, CnabCampo.NumStr(cedente.CodigoBanco, 3));
            Set(hl, 4, "0001");
            Set(hl, 8, "1");                                       // registro header lote
            Set(hl, 9, "R");                                       // remessa
            Set(hl, 10, "01");                                     // cobrança
            Set(hl, 34, CnabCampo.NumStr(cedente.Agencia, 5));
            Set(hl, 40, CnabCampo.NumStr(cedente.Conta, 12));
            Set(hl, 74, CnabCampo.Alfa(cedente.NomeCedente, 30));
            linhas.Add(Fixar(hl.ToString(), 240));

            var seqLote = 1;
            decimal total = 0m;
            foreach (var t in titulos)
            {
                // Segmento P (dados do título)
                var p = new StringBuilder(new string(' ', 240));
                Set(p, 1, CnabCampo.NumStr(cedente.CodigoBanco, 3));
                Set(p, 4, "0001");
                Set(p, 8, "3");                                    // registro detalhe
                Set(p, 9, CnabCampo.Num(seqLote++, 5));
                Set(p, 14, "P");
                Set(p, 38, CnabCampo.Num(t.NossoNumero, 20));
                Set(p, 63, CnabCampo.Alfa(t.NumeroDocumento, 15));
                Set(p, 78, CnabCampo.Data(t.DataVencimento, anoCompleto: true));
                Set(p, 86, CnabCampo.Valor(t.Valor, 15));
                linhas.Add(Fixar(p.ToString(), 240));

                // Segmento Q (dados do sacado)
                var q = new StringBuilder(new string(' ', 240));
                Set(q, 1, CnabCampo.NumStr(cedente.CodigoBanco, 3));
                Set(q, 4, "0001");
                Set(q, 8, "3");
                Set(q, 9, CnabCampo.Num(seqLote++, 5));
                Set(q, 14, "Q");
                Set(q, 20, CnabCampo.NumStr(t.SacadoDocumento, 14));
                Set(q, 34, CnabCampo.Alfa(t.SacadoNome, 40));
                Set(q, 74, CnabCampo.Alfa(t.SacadoEndereco, 40));
                Set(q, 133, CnabCampo.Alfa(t.SacadoBairro, 15));
                Set(q, 148, CnabCampo.NumStr(t.SacadoCep, 8));
                Set(q, 156, CnabCampo.Alfa(t.SacadoCidade, 15));
                Set(q, 171, CnabCampo.Alfa(t.SacadoUF, 2));
                linhas.Add(Fixar(q.ToString(), 240));
                total += t.Valor;
            }

            // Trailer de lote (tipo 5)
            var tl = new StringBuilder(new string(' ', 240));
            Set(tl, 1, CnabCampo.NumStr(cedente.CodigoBanco, 3));
            Set(tl, 4, "0001");
            Set(tl, 8, "5");
            Set(tl, 18, CnabCampo.Num(linhas.Count + 1, 6));       // qtd registros do lote
            Set(tl, 24, CnabCampo.Valor(total, 17));
            linhas.Add(Fixar(tl.ToString(), 240));

            // Trailer de arquivo (tipo 9)
            var ta = new StringBuilder(new string(' ', 240));
            Set(ta, 1, CnabCampo.NumStr(cedente.CodigoBanco, 3));
            Set(ta, 4, "9999");
            Set(ta, 8, "9");
            Set(ta, 18, "000001");                                 // qtd de lotes
            Set(ta, 24, CnabCampo.Num(linhas.Count + 1, 6));       // qtd de registros do arquivo
            linhas.Add(Fixar(ta.ToString(), 240));

            return string.Join(CRLF, linhas) + CRLF;
        }

        /// <summary>
        /// Lê de volta os títulos de uma remessa CNAB 400 gerada por <see cref="GerarRemessa400"/>
        /// (auditoria / round-trip). Recupera nosso número, número do documento, valor e vencimento.
        /// </summary>
        public static IReadOnlyList<TituloLidoCnab> LerRemessa400(string conteudo)
        {
            var resultado = new List<TituloLidoCnab>();
            foreach (var linha in SepararLinhas(conteudo))
            {
                if (linha.Length == 0 || linha[0] != '1') continue; // só detalhes
                var nosso = CnabCampo.LerNum(linha, D400_NOSSONUMERO_INI, D400_NOSSONUMERO_LEN);
                var numdoc = CnabCampo.Ler(linha, D400_NUMDOC_INI, D400_NUMDOC_LEN).Trim();
                var valor = CnabCampo.LerValor(linha, D400_VALOR_INI, D400_VALOR_LEN);
                var venc = CnabCampo.LerDataDdMMyy(linha, D400_VENCIMENTO_INI) ?? DateTime.MinValue;
                resultado.Add(new TituloLidoCnab(nosso, numdoc, valor, venc));
            }
            return resultado;
        }

        internal static IEnumerable<string> SepararLinhas(string conteudo)
            => (conteudo ?? string.Empty)
                .Replace("\r\n", "\n").Replace("\r", "\n")
                .Split('\n')
                .Where(l => l.Length > 0);

        private static void Set(StringBuilder sb, int inicio1based, string valor)
        {
            var idx = inicio1based - 1;
            for (var i = 0; i < valor.Length && idx + i < sb.Length; i++)
                sb[idx + i] = valor[i];
        }

        private static string Fixar(string linha, int tamanho)
            => linha.Length == tamanho ? linha
             : linha.Length > tamanho ? linha.Substring(0, tamanho)
             : linha.PadRight(tamanho, ' ');
    }
}
