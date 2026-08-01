using DFe.Classes.Entidades;

namespace Epros.ERP.DfeCalculos.Models.Vendas.VendaItemProdutosEspecificos
{
    public class VendaItemProdutoCombustivel : VendaItemProdutoEspecifico
    {
        public VendaItemProdutoCombustivel() { }
        public VendaItemProdutoCombustivel(string codigoAnp, string descricaoAnp, decimal? pMixGN, decimal? pGLP, decimal? pGNn, decimal? pGNi, decimal? vPart, string cODIF, decimal? qTemp, string uFCons, decimal? pBio)
        {
            CodigoAnp = codigoAnp;
            DescricaoAnp = descricaoAnp;
            PMixGN = pMixGN;
            PGLP = pGLP;
            PGNn = pGNn;
            PGNi = pGNi;
            VPart = vPart;
            CODIF = cODIF;
            QTemp = qTemp;
            UFCons = uFCons;
            PBio = pBio;
        }

        public string CodigoAnp { get; set; } = null!;
        public string DescricaoAnp { get; set; } = null!;
        public decimal? PMixGN { get; set; }
        public decimal? PGLP { get; set; }
        public decimal? PGNn { get; set; }
        public decimal? PGNi { get; set; }
        public decimal? VPart { get; set; }
        public string CODIF { get; set; } = null!;
        public decimal? QTemp { get; set; }
        public string UFCons { get; set; } = null!;
        public decimal? PBio { get; set; }


        //public CIDE CIDE { get; set; }

        //public encerrante encerrante { get; set; }

        public List<VendaItemProdutoCombustivelOrigemCombustivel> Origens { get; set; } = [];

        public void AdicionarOrigemCombustivel(int indImport, Estado cUFOrig, decimal pOrig)
        {
            Origens.Add(new VendaItemProdutoCombustivelOrigemCombustivel
            (
                indImport,
                cUFOrig,
                pOrig
            ));
        }
    }
}
