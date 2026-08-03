using System;
using System.Globalization;

namespace Epros.Modules.Agricultor.Domain.Services
{
    /// <summary>
    /// Formatação byte-a-byte do arquivo LCDPR (Manual 1.3 cap. 2; AGR-D17/D20). Regras congeladas:
    /// numérico com decimais IMPLÍCITAS (10.000,00 → "1000000"); percentual implícito (50% → "05000");
    /// data ddmmaaaa; período mmaaaa; delimitador '|' ao fim de cada campo, exceto o último; '|' proibido
    /// no conteúdo (bloqueante). Isto é ESTRUTURA/formatação — não classificação fiscal.
    /// </summary>
    public static class FormatadorLcdpr
    {
        public const string CodigoVersaoLeiaute = "0013"; // leiaute 1.3 (ADE COPES 1/2020)

        /// <summary>Monta uma linha de registro: campos unidos por '|' (sem pipe no início nem no fim).</summary>
        public static string MontarLinha(params string?[] campos)
        {
            var partes = new string[campos.Length];
            for (int i = 0; i < campos.Length; i++)
            {
                var c = campos[i] ?? string.Empty;
                if (c.Contains('|'))
                    throw new InvalidOperationException(
                        "Caractere '|' proibido no conteúdo de um campo LCDPR (bloqueante). [FormatadorLcdpr]");
                partes[i] = c;
            }
            return string.Join("|", partes);
        }

        /// <summary>Valor monetário N,19,2 com decimais implícitas e SEM sinal (10.000,00 → "1000000").</summary>
        public static string ValorImplicito(decimal valor)
        {
            var abs = Math.Abs(Math.Round(valor, 2, MidpointRounding.AwayFromZero));
            var centavos = (long)(abs * 100m);
            return centavos.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Percentual N,5,2 implícito com zero à esquerda (50% → "05000"); 5 dígitos.</summary>
        public static string PercentualImplicito(decimal percentual)
        {
            var centesimos = (long)Math.Round(Math.Abs(percentual) * 100m, 0, MidpointRounding.AwayFromZero);
            return centesimos.ToString(CultureInfo.InvariantCulture).PadLeft(5, '0');
        }

        /// <summary>Data no formato ddmmaaaa.</summary>
        public static string Data(DateTime data) => data.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

        /// <summary>Período no formato mmaaaa.</summary>
        public static string Periodo(int ano, int mes) =>
            mes.ToString("00", CultureInfo.InvariantCulture) + ano.ToString("0000", CultureInfo.InvariantCulture);

        /// <summary>Código de imóvel/conta com 3 dígitos (000/999 são domínios especiais do Manual).</summary>
        public static string Codigo3(int cod) => cod.ToString("000", CultureInfo.InvariantCulture);

        public static string Natureza(bool positivo) => positivo ? "P" : "N";
    }
}
