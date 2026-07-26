using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Services;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    /// <summary>
    /// Sondagem online do status do serviço SEFAZ. Reusa o motor legado (Hercules) via
    /// <see cref="IHerculesFiscalService.VerificarStatusServicoAsync"/>. Não persiste nada.
    /// </summary>
    public class VerificarStatusServicoCommandHandler : ICommandHandler<VerificarStatusServicoCommand>
    {
        private readonly IHerculesFiscalService _fiscalService;

        public VerificarStatusServicoCommandHandler(IHerculesFiscalService fiscalService)
            => _fiscalService = fiscalService;

        public async Task<CommandResult> Handle(VerificarStatusServicoCommand request, CancellationToken cancellationToken)
        {
            var retorno = await _fiscalService.VerificarStatusServicoAsync(new ConsultaStatusServicoRequest
            {
                Documento = request.Documento,
                Modelo = request.ModeloDocumento == 65 ? "65" : "55",
                Ambiente = request.Ambiente
            });

            if (!retorno.Sucesso)
                return CommandResult.Falha(new[] { retorno.Motivo }, $"Serviço SEFAZ indisponível (Código: {retorno.StatusSefaz}).");

            return CommandResult.Ok("Serviço SEFAZ em operação.", new
            {
                retorno.StatusSefaz,
                retorno.Motivo
            });
        }
    }

    /// <summary>
    /// Consulta a situação de uma NF-e/NFC-e por chave. Reusa o motor legado (Hercules) via
    /// <see cref="IHerculesFiscalService.ConsultarProtocoloAsync"/>. Não persiste nada.
    /// </summary>
    public class ConsultarProtocoloCommandHandler : ICommandHandler<ConsultarProtocoloCommand>
    {
        private readonly IHerculesFiscalService _fiscalService;

        public ConsultarProtocoloCommandHandler(IHerculesFiscalService fiscalService)
            => _fiscalService = fiscalService;

        public async Task<CommandResult> Handle(ConsultarProtocoloCommand request, CancellationToken cancellationToken)
        {
            var retorno = await _fiscalService.ConsultarProtocoloAsync(new ConsultaProtocoloRequest
            {
                Chave = request.Chave,
                Modelo = request.ModeloDocumento == 65 ? "65" : "55",
                Ambiente = request.Ambiente
            });

            if (!retorno.Sucesso)
                return CommandResult.Falha(new[] { retorno.Motivo }, $"Consulta de protocolo sem autorização (Código: {retorno.StatusSefaz}).");

            return CommandResult.Ok("Consulta de protocolo realizada com sucesso.", new
            {
                retorno.StatusSefaz,
                retorno.Motivo,
                retorno.XmlRetorno
            });
        }
    }
}
