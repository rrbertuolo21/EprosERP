using System;

namespace Epros.Shared.Domain.Exceptions
{
    public class OperacaoBloqueadaModoDemoException : Exception
    {
        public OperacaoBloqueadaModoDemoException()
            : base("Mutações de dados não são permitidas no modo de demonstração (Demo).")
        {
        }

        public OperacaoBloqueadaModoDemoException(string message)
            : base(message)
        {
        }

        public OperacaoBloqueadaModoDemoException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
