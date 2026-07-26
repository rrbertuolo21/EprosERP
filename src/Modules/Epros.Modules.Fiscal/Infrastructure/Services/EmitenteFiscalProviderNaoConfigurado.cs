using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;

namespace Epros.Modules.Fiscal.Infrastructure.Services
{
    /// <summary>
    /// Implementação padrão (fallback) de <see cref="IEmitenteFiscalProvider"/> para ambientes que
    /// ainda NÃO têm a fonte real de emitente/certificado (config da empresa/tenant, EmpresaCertificado).
    /// Sempre retorna <c>null</c>, fazendo o <see cref="MotorLegadoFiscalService"/> degradar de forma
    /// controlada (rejeita com motivo claro), NUNCA gerando chave/protocolo simulado.
    ///
    /// TODO: substituir por um provider real que leia a empresa emitente + certificado A1
    /// (UF, CRT, série, CSC, bytes+senha do certificado) da configuração do tenant/EmpresaCertificado.
    /// </summary>
    public class EmitenteFiscalProviderNaoConfigurado : IEmitenteFiscalProvider
    {
        public ContextoTransmissaoFiscal? ObterContexto(DocumentoFiscal documento) => null;

        public ContextoTransmissaoFiscal? ObterContextoPorDocumento(string documentoEmitente, string modelo, int ambiente) => null;
    }
}
