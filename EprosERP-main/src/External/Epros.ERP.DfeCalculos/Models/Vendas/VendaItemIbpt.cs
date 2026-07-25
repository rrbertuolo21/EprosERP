namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaItemIbpt
    {
        public VendaItemIbpt(decimal aliqFederal, decimal aliqImportado, decimal aliqEstadual, decimal aliqMunicipal, string versao)
        {
            AliqFederal = aliqFederal;
            AliqImportado = aliqImportado;
            AliqEstadual = aliqEstadual;
            AliqMunicipal = aliqMunicipal;
            Versao = versao;
        }

        public decimal AliqFederal { get; private set; }
        public decimal AliqImportado { get; private set; }
        public decimal AliqEstadual { get; private set; }
        public decimal AliqMunicipal { get; private set; }
        public string Versao { get; private set; }
    }
}
