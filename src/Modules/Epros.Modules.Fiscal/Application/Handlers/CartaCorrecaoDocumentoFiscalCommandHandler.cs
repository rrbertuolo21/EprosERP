using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    public class CartaCorrecaoDocumentoFiscalCommandHandler : ICommandHandler<CartaCorrecaoDocumentoFiscalCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IHerculesFiscalService _fiscalService;

        public CartaCorrecaoDocumentoFiscalCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IHerculesFiscalService fiscalService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _fiscalService = fiscalService;
        }

        public async Task<CommandResult> Handle(CartaCorrecaoDocumentoFiscalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var documento = await _context.DocumentosFiscais
                .FirstOrDefaultAsync(d => d.Id == request.DocumentoFiscalId, cancellationToken);

            if (documento == null)
                return CommandResult.Falha("Documento fiscal não localizado.");

            if (documento.Status != "Autorizado")
                return CommandResult.Falha($"Somente documentos 'Autorizado' aceitam carta de correção. Status atual: {documento.Status}");

            // Sequência do evento = nº de CC-e já registradas para o documento + 1.
            var ccesAnteriores = await _context.EventosDocumentosFiscais
                .CountAsync(e => e.DocumentoFiscalId == documento.Id && e.TipoEvento == "CartaCorrecao", cancellationToken);
            var sequencia = ccesAnteriores + 1;

            var retorno = await _fiscalService.CartaCorrecaoAsync(documento, request.TextoCorrecao, sequencia);

            if (!retorno.Sucesso)
                return CommandResult.Falha(new[] { retorno.Motivo }, $"Falha ao registrar a carta de correção na SEFAZ (Código: {retorno.StatusSefaz}).");

            var evento = new EventoDocumentoFiscal(
                documento.Id,
                "CartaCorrecao",
                retorno.StatusSefaz,
                retorno.Motivo,
                retorno.Protocolo,
                sequencia,
                request.TextoCorrecao,
                retorno.XmlRetorno,
                tenantId,
                usuario);

            _context.EventosDocumentosFiscais.Add(evento);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Carta de correção registrada com sucesso!", new
            {
                DocumentoFiscalId = documento.Id,
                SequenciaEvento = sequencia,
                StatusSefaz = retorno.StatusSefaz
            });
        }
    }
}
