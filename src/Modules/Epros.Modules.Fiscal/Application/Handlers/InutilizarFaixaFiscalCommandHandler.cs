using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    /// <summary>
    /// Executa a inutilização de faixa de numeração na SEFAZ via <see cref="IHerculesFiscalService"/>.
    /// Ao homologar (cStat 102), persiste o evento como <see cref="InutilizacaoFiscal"/> para o histórico
    /// (consultável em <c>GET api/v1/inutilizacao-dfe</c>).
    /// </summary>
    public class InutilizarFaixaFiscalCommandHandler : ICommandHandler<InutilizarFaixaFiscalCommand>
    {
        private readonly IHerculesFiscalService _fiscalService;
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public InutilizarFaixaFiscalCommandHandler(
            IHerculesFiscalService fiscalService,
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _fiscalService = fiscalService;
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(InutilizarFaixaFiscalCommand request, CancellationToken cancellationToken)
        {
            var ano = request.Ano > 0 ? request.Ano : DateTime.UtcNow.Year % 100;

            var fiscalRequest = new InutilizacaoFiscalRequest
            {
                Modelo = request.ModeloDocumento == 65 ? "65" : "55",
                Ambiente = request.Ambiente,
                Serie = request.Serie,
                NumeroInicial = request.NrNfInicial,
                NumeroFinal = request.NrNfFinal,
                Ano = ano,
                Justificativa = request.Justificativa
            };

            var retorno = await _fiscalService.InutilizarAsync(fiscalRequest);

            // 102 = Inutilização de número homologada.
            if (retorno.Sucesso && retorno.StatusSefaz == 102)
            {
                var tenantId = _tenantProvider.GetTenantId();
                var criadoPor = _currentUser.GetUserId() ?? "system";

                var registro = new InutilizacaoFiscal(
                    request.ModeloDocumento,
                    request.Serie,
                    request.NrNfInicial,
                    request.NrNfFinal,
                    ano,
                    request.Ambiente,
                    request.Justificativa,
                    retorno.StatusSefaz,
                    retorno.Motivo,
                    retorno.Protocolo,
                    tenantId,
                    criadoPor);

                if (registro.IsValid)
                {
                    _context.InutilizacoesFiscais.Add(registro);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                return CommandResult.Ok("Faixa de numeração inutilizada com sucesso na SEFAZ.", new
                {
                    retorno.StatusSefaz,
                    retorno.Motivo,
                    retorno.Protocolo
                });
            }

            return CommandResult.Falha(
                new[] { retorno.Motivo },
                $"Falha ao inutilizar a faixa na SEFAZ: {retorno.Motivo} (Código: {retorno.StatusSefaz})");
        }
    }
}
