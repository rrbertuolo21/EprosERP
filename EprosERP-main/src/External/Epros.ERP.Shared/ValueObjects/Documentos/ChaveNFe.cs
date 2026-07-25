using Epros.ERP.Shared.Enums;

namespace Epros.ERP.Shared.ValueObjects.Documentos
{
    public class ChaveNFe
    {
        protected ChaveNFe() { }
        public ChaveNFe(string chave)
        {
            Chave = (chave ?? "").Trim();
            Validar();
        }

        public string Chave { get; private set; } = null!;
        public Documento Documento { get { return ObterDocumento(); } }
        public EEstado UF { get { return ObterUF(); } }
        public DateTime MesAnoEmissao { get { return ObterMesAnoEmissao(); } }
        public EModeloDocumento Modelo { get { return ObterModelo(); } }
        public string Serie { get { return ObterSerie(); } }
        public string Numero { get { return ObterNumero(); } }
        public ETipoOperacaoNfe TipoOperacaoNfe { get { return ObterTipoOperacaoNfe(); } }
        public bool Valido { get; private set; }
        public void Validar() => Valido = Chave.Length == 44;

        private Documento ObterDocumento()
        {
            Validar();
            if (Valido)
                return new Documento(Chave.Substring(6, 14));
            return Documento;
        }

        private string ObterNumero()
        {
            Validar();
            if (Valido)
                return Chave.Substring(25, 9);
            return Numero;
        }

        private EEstado ObterUF()
        {
            Validar();
            if (Valido)
            {
                Enum.TryParse(Chave.Substring(0, 2), out EEstado uf);
                return uf;
            }
            return UF;
        }

        private DateTime ObterMesAnoEmissao()
        {
            Validar();
            if (Valido)
                return new DateTime(int.Parse(Chave.Substring(2, 2)), int.Parse(Chave.Substring(4, 2)), 1);
            return MesAnoEmissao;
        }

        private EModeloDocumento ObterModelo()
        {
            Validar();
            if (Valido)
            {
                Enum.TryParse(Chave.Substring(20, 2), out EModeloDocumento modelo);
                return modelo;
            }
            return Modelo;
        }

        private string ObterSerie()
        {
            Validar();
            if (Valido)
                return Chave.Substring(22, 3);
            return Serie;
        }

        private ETipoOperacaoNfe ObterTipoOperacaoNfe()
        {
            Validar();
            if (Valido)
            {
                Enum.TryParse(Chave.Substring(34, 1), out ETipoOperacaoNfe tipoOperacaoNfe);
                return tipoOperacaoNfe;
            }
            return TipoOperacaoNfe;
        }
    }
}
