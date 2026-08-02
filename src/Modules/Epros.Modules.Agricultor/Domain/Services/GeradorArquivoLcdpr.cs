using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Epros.Modules.Agricultor.Domain.Entities;
using Epros.Modules.Agricultor.Domain.Enums;

namespace Epros.Modules.Agricultor.Domain.Services
{
    /// <summary>Linha derivada do Q200 (resumo mensal) — não persistida, calculada a partir do Q100.</summary>
    public sealed record ResumoMensalLcdpr(int Ano, int Mes, decimal VlEntrada, decimal VlSaida, decimal SldFinCumulativo);

    /// <summary>Resultado da geração: conteúdo do arquivo, contagem real de linhas, hash e Q200 derivado.</summary>
    public sealed record ArquivoLcdprGerado(string Conteudo, int QtdLinhas, string HashSha256, IReadOnlyList<ResumoMensalLcdpr> ResumoMensal)
    {
        /// <summary>AGR-D14: convenção de nome do arquivo (o Manual 1.3 não define). // valida-contador no go-live.</summary>
        public static string NomeArquivo(string cpf, int ano, DateTime? timestamp = null)
            => timestamp.HasValue
                ? $"LCDPR_{cpf}_{ano}_{timestamp.Value:yyyyMMddHHmmss}.txt"
                : $"LCDPR_{cpf}_{ano}.txt";
    }

    /// <summary>
    /// AGR-D20 — Gerador determinístico do arquivo LCDPR (leiaute 1.3). Ordem dos registros:
    /// 0000 → 0010 → 0030 → 0040 (+0045 logo após cada 0040) → 0050 → Q100 → Q200 (derivado) → 9999 (RN47).
    /// SLD_FIN do Q100 é saldo corrido (saldo inicial = 0 a cada ano; RN40); Q200.SLD_FIN é cumulativo
    /// (RN41); 9999.QTD_LIN = nº real de linhas contando repetidos (RN46). Formatação = FormatadorLcdpr.
    /// A ordenação dos campos de cada registro segue a skill fiscal (leiaute campo-a-campo, §leiaute).
    /// </summary>
    public class GeradorArquivoLcdprService
    {
        private const string CRLF = "\r\n";

        public ArquivoLcdprGerado Gerar(LcdprEscrituracao esc)
        {
            var f = FormatadorLcdpr.CodigoVersaoLeiaute; // "0013"
            var linhas = new List<string>();

            // ---- 0000: abertura/identificação ----
            linhas.Add(FormatadorLcdpr.MontarLinha(
                "0000", "LCDPR", f,
                esc.Cpf, esc.Nome,
                esc.IndSituacaoInicioPeriodo.ToString(),
                esc.SituacaoEspecial.ToString(),
                string.Empty, // DT_SIT_ESP (opcional)
                FormatadorLcdpr.Data(esc.DtIni),
                FormatadorLcdpr.Data(esc.DtFin)));

            // ---- 0010: parâmetro de tributação ----
            linhas.Add(FormatadorLcdpr.MontarLinha("0010", ((int)esc.FormaApuracao).ToString()));

            // ---- 0030: dados cadastrais ----
            var dc = esc.DadosCadastrais;
            linhas.Add(FormatadorLcdpr.MontarLinha(
                "0030", dc?.Endereco, string.Empty, string.Empty, string.Empty,
                dc?.Uf, dc?.CodMunicipio, dc?.Cep, string.Empty, dc?.Email));

            // ---- 0040 (+0045) : imóveis ----
            foreach (var imovel in esc.Imoveis.OrderBy(i => i.CodImovel))
            {
                linhas.Add(FormatadorLcdpr.MontarLinha(
                    "0040", FormatadorLcdpr.Codigo3(imovel.CodImovel), "BR", "BRL",
                    imovel.CadItrCafir, imovel.Caepf, string.Empty, // INSCR_ESTADUAL (opc)
                    imovel.NomeImovel, string.Empty, imovel.Uf, imovel.CodMunicipio, string.Empty,
                    ((int)imovel.TipoExploracao).ToString(),
                    FormatadorLcdpr.PercentualImplicito(imovel.Participacao)));

                foreach (var t in imovel.Terceiros)
                {
                    linhas.Add(FormatadorLcdpr.MontarLinha(
                        "0045", FormatadorLcdpr.Codigo3(t.CodImovel), t.TipoContraparte.ToString(),
                        t.IdContraparte, t.NomeContraparte, FormatadorLcdpr.PercentualImplicito(t.PercContraparte)));
                }
            }

            // ---- 0050: contas ----
            foreach (var conta in esc.Contas.OrderBy(c => c.CodConta))
            {
                linhas.Add(FormatadorLcdpr.MontarLinha(
                    "0050", FormatadorLcdpr.Codigo3(conta.CodConta), "BR",
                    conta.Banco?.ToString(), string.Empty,
                    conta.Agencia?.ToString(), conta.NumConta));
            }

            // ---- Q100: lançamentos (SLD_FIN corrido; saldo inicial = 0) ----
            decimal saldo = 0m;
            var lancamentosOrdenados = esc.Lancamentos.OrderBy(l => l.Data).ThenBy(l => l.CriadoEm).ToList();
            foreach (var l in lancamentosOrdenados)
            {
                saldo += l.VlEntrada - l.VlSaida;
                linhas.Add(FormatadorLcdpr.MontarLinha(
                    "Q100", FormatadorLcdpr.Data(l.Data),
                    FormatadorLcdpr.Codigo3(l.CodImovel), FormatadorLcdpr.Codigo3(l.CodConta),
                    l.NumDoc, ((int)l.TipoDoc).ToString(), l.Historico, l.IdPartic,
                    ((int)l.TipoLanc).ToString(),
                    FormatadorLcdpr.ValorImplicito(l.VlEntrada),
                    FormatadorLcdpr.ValorImplicito(l.VlSaida),
                    FormatadorLcdpr.ValorImplicito(saldo),
                    FormatadorLcdpr.Natureza(saldo >= 0)));
            }

            // ---- Q200: resumo mensal DERIVADO do Q100 (SLD_FIN cumulativo) ----
            var resumo = DerivarResumoMensal(lancamentosOrdenados);
            foreach (var r in resumo)
            {
                linhas.Add(FormatadorLcdpr.MontarLinha(
                    "Q200", FormatadorLcdpr.Periodo(r.Ano, r.Mes),
                    FormatadorLcdpr.ValorImplicito(r.VlEntrada),
                    FormatadorLcdpr.ValorImplicito(r.VlSaida),
                    FormatadorLcdpr.ValorImplicito(r.SldFinCumulativo),
                    FormatadorLcdpr.Natureza(r.SldFinCumulativo >= 0)));
            }

            // ---- 9999: encerramento (QTD_LIN = nº real de linhas, incluindo o próprio 9999) ----
            int qtdLin = linhas.Count + 1;
            linhas.Add(FormatadorLcdpr.MontarLinha(
                "9999", esc.IdentificacaoNome, esc.IdentificacaoCpfCnpj,
                string.Empty, string.Empty, string.Empty, qtdLin.ToString()));

            var conteudo = string.Join(CRLF, linhas) + CRLF;
            var hash = Sha256(conteudo);
            return new ArquivoLcdprGerado(conteudo, linhas.Count, hash, resumo);
        }

        /// <summary>Deriva o Q200 do Q100: soma por mês + saldo cumulativo (RN41).</summary>
        public IReadOnlyList<ResumoMensalLcdpr> DerivarResumoMensal(IEnumerable<LcdprLancamento> lancamentos)
        {
            var porMes = lancamentos
                .GroupBy(l => new { l.Data.Year, l.Data.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Entrada = g.Sum(x => x.VlEntrada),
                    Saida = g.Sum(x => x.VlSaida)
                })
                .ToList();

            var resultado = new List<ResumoMensalLcdpr>();
            decimal cumulativo = 0m;
            foreach (var m in porMes)
            {
                cumulativo += m.Entrada - m.Saida;
                resultado.Add(new ResumoMensalLcdpr(m.Year, m.Month, m.Entrada, m.Saida, cumulativo));
            }
            return resultado;
        }

        private static string Sha256(string conteudo)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(conteudo));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
