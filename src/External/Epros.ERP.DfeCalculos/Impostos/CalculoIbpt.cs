using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;

namespace Epros.ERP.DfeCalculos.Impostos
{
    public class CalculoIbpt
    {
        public static RetornoIbptCalculadoDto Calcular(string ncm, decimal valorBase, string origem, decimal aliqNacionalFederal, decimal aliqImportadoFederal, decimal aliqEstadual, decimal aliqMunicipal, string versao)
        {
            decimal somaBase = decimal.Zero;
            decimal somaAliquota = decimal.Zero;
            VendaItemIbpt ibptCalculado;

            if (origem == "0")
            {
                ibptCalculado = new VendaItemIbpt((valorBase * (aliqNacionalFederal / 100)).Arredonda2(),
                    decimal.Zero,
                    (valorBase * (aliqEstadual / 100)).Arredonda2(),
                    (valorBase * (aliqMunicipal / 100)).Arredonda2(),
                    versao);

                somaBase = valorBase * ((aliqNacionalFederal + aliqEstadual + aliqMunicipal) / 100);
                somaAliquota = aliqNacionalFederal + aliqEstadual + aliqMunicipal;
            }
            else
            {
                ibptCalculado = new VendaItemIbpt(decimal.Zero, (valorBase * (aliqImportadoFederal / 100)).Arredonda2(), decimal.Zero, decimal.Zero, versao);
                somaBase = valorBase * ((aliqImportadoFederal) / 100);
                somaAliquota = aliqImportadoFederal;
            }

            return new RetornoIbptCalculadoDto(ncm, valorBase, somaAliquota.Arredonda2(), somaBase.Arredonda2());
        }
        public record RetornoIbptCalculadoDto(string Ncm, decimal ValorBase, decimal Aliquota, decimal ValorImposto);
    }
}
