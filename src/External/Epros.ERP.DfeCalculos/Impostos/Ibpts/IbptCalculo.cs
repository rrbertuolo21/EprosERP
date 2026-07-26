namespace Epros.ERP.DfeCalculos.Impostos.Ibpts
{
    public class IbptCalculo
    {
        public static string GerarObservacaoIbptValores(decimal valorNacionalFederal, decimal valorImportadoFederal, decimal valorEstadual, decimal valorMunicipal)
        {
            /*
                                           ATENÇÃO

                Achei necessário esse comentário pois demorei um pouco até descrobrir
                que não pode ser utilizado (;) ponto e virgula (\n) quebra de linha e 
                nem ( ) espaço no final do texto.              
            */

            string ObservacaoIbpt = "";
            var soma = valorNacionalFederal + valorImportadoFederal + valorEstadual + valorMunicipal;

            if (soma > decimal.Zero)
            {
                ObservacaoIbpt = $@"Tributos Totais Incidentes (Lei Federal 12.741/2012) (Total {soma:N2})";

                if (valorNacionalFederal > decimal.Zero || valorImportadoFederal > decimal.Zero)
                    ObservacaoIbpt += $" ({valorNacionalFederal + valorImportadoFederal:N2} Federal)";

                if (valorEstadual > decimal.Zero)
                    ObservacaoIbpt += $" ({valorEstadual:N2} Estadual)";

                if (valorMunicipal > decimal.Zero)
                    ObservacaoIbpt += $" ({valorMunicipal:N2} Municipal)";
            }

            return ObservacaoIbpt;
        }
    }
}
