using DFe.Classes.Entidades;

namespace Epros.ERP.DfeCalculos.Models.Vendas.VendaItemProdutosEspecificos
{
    public class VendaItemProdutoCombustivelOrigemCombustivel
    {
        public VendaItemProdutoCombustivelOrigemCombustivel() { }
        public VendaItemProdutoCombustivelOrigemCombustivel(int indImport, Estado cUFOrig, decimal pOrig)
        {
            IndImport = indImport;
            CUFOrig = cUFOrig;
            POrig = pOrig;
        }

        public int IndImport { get; set; }
        public Estado CUFOrig { get; set; }
        public decimal POrig { get; set; }
    }
}