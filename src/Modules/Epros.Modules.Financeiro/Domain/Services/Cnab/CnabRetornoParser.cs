using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epros.Modules.Financeiro.Domain.Enums;

namespace Epros.Modules.Financeiro.Domain.Services.Cnab
{
    /// <summary>
    /// Parser de RETORNO bancário CNAB (FIN-SF §7.6). Identifica o layout pela largura da linha
    /// (240/242 → CNAB 240; 400/402 → CNAB 400 — RSF-009/010), carrega as ocorrências e devolve os
    /// dados de baixa (nosso número, valor pago, ocorrência). A tabela de códigos de ocorrência e as
    /// posições exatas variam por banco/layout (// valida-contador); adota-se um layout de REFERÊNCIA
    /// simétrico ao writer, e expõe-se um SIMULADOR de retorno para homologação e testes round-trip.
    /// </summary>
    public static class CnabRetornoParser
    {
        // ----- Posições de referência do detalhe de retorno CNAB 400 (1-based) -----
        internal const int R400_NOSSONUMERO_INI = 63, R400_NOSSONUMERO_LEN = 17;
        internal const int R400_OCORRENCIA_INI = 109, R400_OCORRENCIA_LEN = 2;
        internal const int R400_DATAOCORR_INI = 111; // ddMMyy
        internal const int R400_NUMDOC_INI = 117, R400_NUMDOC_LEN = 10;
        internal const int R400_VALORTITULO_INI = 153, R400_VALORTITULO_LEN = 13;
        internal const int R400_VALORTARIFA_INI = 176, R400_VALORTARIFA_LEN = 13;
        internal const int R400_VALORPAGO_INI = 254, R400_VALORPAGO_LEN = 13;

        // ----- Posições de referência dos segmentos T/U de retorno CNAB 240 (1-based) -----
        internal const int R240_SEGMENTO_INI = 14; // 'T' ou 'U'
        internal const int R240_OCORRENCIA_INI = 16, R240_OCORRENCIA_LEN = 2;
        internal const int R240_T_NOSSONUMERO_INI = 38, R240_T_NOSSONUMERO_LEN = 20;
        internal const int R240_T_NUMDOC_INI = 59, R240_T_NUMDOC_LEN = 15;
        internal const int R240_T_VALORTITULO_INI = 82, R240_T_VALORTITULO_LEN = 15;
        internal const int R240_T_VALORTARIFA_INI = 199, R240_T_VALORTARIFA_LEN = 15;
        internal const int R240_U_VALORPAGO_INI = 78, R240_U_VALORPAGO_LEN = 15;

        /// <summary>Códigos de ocorrência tratados como LIQUIDAÇÃO/baixa efetiva. // valida-contador (tabela por banco).</summary>
        private static readonly HashSet<string> OcorrenciasLiquidacao = new(StringComparer.Ordinal)
        { "06", "08", "09", "10", "15", "17" };

        /// <summary>Detecta o layout pela largura da linha (RSF-009/010). Lança se não reconhecido (RSF-008).</summary>
        public static ELayoutCnab DetectarLayout(string conteudo)
        {
            var linha = CnabWriter.SepararLinhas(conteudo).FirstOrDefault();
            if (linha == null) throw new ArgumentException("Arquivo de retorno vazio.", nameof(conteudo)); // RSF-008
            var len = linha.TrimEnd('\r', '\n').Length;
            if (len is >= 240 and <= 242) return ELayoutCnab.Cnab240;
            if (len is >= 400 and <= 402) return ELayoutCnab.Cnab400;
            throw new ArgumentException($"Layout de retorno CNAB não reconhecido (largura {len}).", nameof(conteudo)); // RSF-008
        }

        /// <summary>Processa o retorno detectando o layout automaticamente.</summary>
        public static RetornoCnab ParsearRetorno(string conteudo)
        {
            var layout = DetectarLayout(conteudo);
            return layout == ELayoutCnab.Cnab400 ? Parsear400(conteudo) : Parsear240(conteudo);
        }

        private static RetornoCnab Parsear400(string conteudo)
        {
            var ocorrencias = new List<OcorrenciaRetornoCnab>();
            foreach (var linha in CnabWriter.SepararLinhas(conteudo))
            {
                if (linha.Length == 0 || linha[0] != '1') continue; // detalhe tipo 1
                var cod = CnabCampo.Ler(linha, R400_OCORRENCIA_INI, R400_OCORRENCIA_LEN).Trim();
                ocorrencias.Add(new OcorrenciaRetornoCnab(
                    NossoNumero: CnabCampo.LerNum(linha, R400_NOSSONUMERO_INI, R400_NOSSONUMERO_LEN),
                    NumeroDocumento: CnabCampo.Ler(linha, R400_NUMDOC_INI, R400_NUMDOC_LEN).Trim(),
                    ValorTitulo: CnabCampo.LerValor(linha, R400_VALORTITULO_INI, R400_VALORTITULO_LEN),
                    ValorPago: CnabCampo.LerValor(linha, R400_VALORPAGO_INI, R400_VALORPAGO_LEN),
                    ValorTarifa: CnabCampo.LerValor(linha, R400_VALORTARIFA_INI, R400_VALORTARIFA_LEN),
                    DataOcorrencia: CnabCampo.LerDataDdMMyy(linha, R400_DATAOCORR_INI),
                    CodigoOcorrencia: cod,
                    Liquidado: OcorrenciasLiquidacao.Contains(cod)));
            }
            return new RetornoCnab(ELayoutCnab.Cnab400, ocorrencias);
        }

        private static RetornoCnab Parsear240(string conteudo)
        {
            var ocorrencias = new List<OcorrenciaRetornoCnab>();
            OcorrenciaRetornoCnab? pendenteT = null;

            foreach (var linha in CnabWriter.SepararLinhas(conteudo))
            {
                if (linha.Length < R240_SEGMENTO_INI) continue;
                if (CnabCampo.Ler(linha, 8, 1) != "3") continue; // registro detalhe
                var seg = CnabCampo.Ler(linha, R240_SEGMENTO_INI, 1);

                if (seg == "T")
                {
                    // fecha um T anterior sem U (sem pagamento)
                    if (pendenteT != null) ocorrencias.Add(pendenteT);
                    var cod = CnabCampo.Ler(linha, R240_OCORRENCIA_INI, R240_OCORRENCIA_LEN).Trim();
                    pendenteT = new OcorrenciaRetornoCnab(
                        NossoNumero: CnabCampo.LerNum(linha, R240_T_NOSSONUMERO_INI, R240_T_NOSSONUMERO_LEN),
                        NumeroDocumento: CnabCampo.Ler(linha, R240_T_NUMDOC_INI, R240_T_NUMDOC_LEN).Trim(),
                        ValorTitulo: CnabCampo.LerValor(linha, R240_T_VALORTITULO_INI, R240_T_VALORTITULO_LEN),
                        ValorPago: 0m,
                        ValorTarifa: CnabCampo.LerValor(linha, R240_T_VALORTARIFA_INI, R240_T_VALORTARIFA_LEN),
                        DataOcorrencia: null,
                        CodigoOcorrencia: cod,
                        Liquidado: OcorrenciasLiquidacao.Contains(cod));
                }
                else if (seg == "U" && pendenteT != null)
                {
                    var valorPago = CnabCampo.LerValor(linha, R240_U_VALORPAGO_INI, R240_U_VALORPAGO_LEN);
                    ocorrencias.Add(pendenteT with { ValorPago = valorPago });
                    pendenteT = null;
                }
            }
            if (pendenteT != null) ocorrencias.Add(pendenteT);
            return new RetornoCnab(ELayoutCnab.Cnab240, ocorrencias);
        }

        // ================= Simuladores (homologação / testes round-trip) =================

        /// <summary>
        /// Simula um arquivo de RETORNO CNAB 400 para as ocorrências dadas (ex.: liquidação, ocorrência
        /// "06"). Documenta exatamente o layout de referência que <see cref="ParsearRetorno"/> lê e
        /// serve de fixture de round-trip. Não substitui a homologação bancária (// valida-contador).
        /// </summary>
        public static string SimularRetorno400(IEnumerable<OcorrenciaRetornoCnab> ocorrencias)
        {
            var linhas = new List<string> { new string('0', 1).PadRight(400, ' ') }; // header tipo 0
            var seq = 1;
            foreach (var o in ocorrencias)
            {
                var d = new StringBuilder(new string(' ', 400));
                Set(d, 1, "1");
                Set(d, R400_NOSSONUMERO_INI, CnabCampo.Num(o.NossoNumero, R400_NOSSONUMERO_LEN));
                Set(d, R400_OCORRENCIA_INI, CnabCampo.Alfa(o.CodigoOcorrencia, R400_OCORRENCIA_LEN));
                if (o.DataOcorrencia.HasValue) Set(d, R400_DATAOCORR_INI, CnabCampo.Data(o.DataOcorrencia.Value));
                Set(d, R400_NUMDOC_INI, CnabCampo.Alfa(o.NumeroDocumento, R400_NUMDOC_LEN));
                Set(d, R400_VALORTITULO_INI, CnabCampo.Valor(o.ValorTitulo, R400_VALORTITULO_LEN));
                Set(d, R400_VALORTARIFA_INI, CnabCampo.Valor(o.ValorTarifa, R400_VALORTARIFA_LEN));
                Set(d, R400_VALORPAGO_INI, CnabCampo.Valor(o.ValorPago, R400_VALORPAGO_LEN));
                Set(d, 395, CnabCampo.Num(++seq, 6));
                linhas.Add(d.ToString().Substring(0, 400));
            }
            linhas.Add(new string('9', 1).PadRight(400, ' ')); // trailer tipo 9
            return string.Join(CnabWriter.CRLF, linhas) + CnabWriter.CRLF;
        }

        /// <summary>Simula um retorno CNAB 240 (pares de segmento T/U) para round-trip/homologação.</summary>
        public static string SimularRetorno240(IEnumerable<OcorrenciaRetornoCnab> ocorrencias)
        {
            var linhas = new List<string> { CabecalhoArquivo240() };
            foreach (var o in ocorrencias)
            {
                var t = new StringBuilder(new string(' ', 240));
                Set(t, 8, "3");
                Set(t, R240_SEGMENTO_INI, "T");
                Set(t, R240_OCORRENCIA_INI, CnabCampo.Alfa(o.CodigoOcorrencia, R240_OCORRENCIA_LEN));
                Set(t, R240_T_NOSSONUMERO_INI, CnabCampo.Num(o.NossoNumero, R240_T_NOSSONUMERO_LEN));
                Set(t, R240_T_NUMDOC_INI, CnabCampo.Alfa(o.NumeroDocumento, R240_T_NUMDOC_LEN));
                Set(t, R240_T_VALORTITULO_INI, CnabCampo.Valor(o.ValorTitulo, R240_T_VALORTITULO_LEN));
                Set(t, R240_T_VALORTARIFA_INI, CnabCampo.Valor(o.ValorTarifa, R240_T_VALORTARIFA_LEN));
                linhas.Add(t.ToString().Substring(0, 240));

                var u = new StringBuilder(new string(' ', 240));
                Set(u, 8, "3");
                Set(u, R240_SEGMENTO_INI, "U");
                Set(u, R240_U_VALORPAGO_INI, CnabCampo.Valor(o.ValorPago, R240_U_VALORPAGO_LEN));
                linhas.Add(u.ToString().Substring(0, 240));
            }
            var tr = new StringBuilder(new string(' ', 240));
            Set(tr, 8, "9");
            linhas.Add(tr.ToString().Substring(0, 240));
            return string.Join(CnabWriter.CRLF, linhas) + CnabWriter.CRLF;
        }

        private static string CabecalhoArquivo240()
        {
            var h = new StringBuilder(new string(' ', 240));
            Set(h, 8, "0");
            return h.ToString().Substring(0, 240);
        }

        private static void Set(StringBuilder sb, int inicio1based, string valor)
        {
            var idx = inicio1based - 1;
            for (var i = 0; i < valor.Length && idx + i < sb.Length; i++)
                sb[idx + i] = valor[i];
        }
    }
}
