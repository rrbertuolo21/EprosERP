using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    // =============================== CT-e ===============================

    /// <summary>
    /// Emite um CT-e. Persiste o documento como rascunho, transmite via <see cref="ICteFiscalService"/>
    /// e atualiza o estado conforme o retorno. Fiel ao legado <c>CteController.Emitir</c> (que era mock).
    /// </summary>
    public class EmitirCteCommandHandler : ICommandHandler<EmitirCteCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly ICteFiscalService _cteService;

        public EmitirCteCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser, ICteFiscalService cteService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _cteService = cteService;
        }

        public async Task<CommandResult> Handle(EmitirCteCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var cte = new ConhecimentoTransporteEletronico(
                request.Serie,
                request.Numero,
                request.Ambiente,
                request.TipoCte,
                request.Modal,
                request.RemetenteDocumento,
                request.DestinatarioDocumento,
                request.ValorTotal,
                request.ValorReceber,
                tenantId,
                usuario);

            if (!cte.IsValid)
                return CommandResult.Falha(cte.Notifications.Select(n => n.Message));

            var retorno = await _cteService.EmitirAsync(cte, cancellationToken);

            if (retorno.Sucesso)
                cte.Autorizar(retorno.ChaveAcesso, retorno.Protocolo, retorno.StatusSefaz, retorno.XmlEnvio, retorno.XmlRetorno);
            else
                cte.Rejeitar(retorno.StatusSefaz, retorno.Motivo, retorno.XmlRetorno);

            _context.ConhecimentosTransporteEletronicos.Add(cte);
            await _context.SaveChangesAsync(cancellationToken);

            if (!retorno.Sucesso)
                return CommandResult.Falha(new[] { retorno.Motivo }, $"Falha ao emitir o CT-e (Código: {retorno.StatusSefaz}).");

            return CommandResult.Ok("CT-e emitido com sucesso!", new { cte.Id, cte.ChaveAcesso, cte.Protocolo, cte.Status });
        }
    }

    /// <summary>Cancela um CT-e autorizado. Fiel ao legado <c>CteController.Cancelar</c>.</summary>
    public class CancelarCteCommandHandler : ICommandHandler<CancelarCteCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;
        private readonly ICteFiscalService _cteService;

        public CancelarCteCommandHandler(ContextFiscal context, ICurrentUser currentUser, ICteFiscalService cteService)
        {
            _context = context;
            _currentUser = currentUser;
            _cteService = cteService;
        }

        public async Task<CommandResult> Handle(CancelarCteCommand request, CancellationToken cancellationToken)
        {
            var cte = await _context.ConhecimentosTransporteEletronicos
                .FirstOrDefaultAsync(c => c.ChaveAcesso == request.Chave, cancellationToken);

            if (cte == null)
                return CommandResult.Falha("CT-e não localizado com a chave informada.");

            if (cte.Status != "Autorizado")
                return CommandResult.Falha($"Apenas CT-e 'Autorizado' pode ser cancelado. Status atual: {cte.Status}");

            var retorno = await _cteService.CancelarAsync(cte, request.Justificativa, cancellationToken);

            if (!retorno.Sucesso)
                return CommandResult.Falha(new[] { retorno.Motivo }, $"Falha ao cancelar o CT-e (Código: {retorno.StatusSefaz}).");

            var usuario = _currentUser.GetUserId() ?? "system";
            cte.Cancelar(request.Justificativa, retorno.XmlRetorno, usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("CT-e cancelado com sucesso.", new { retorno.StatusSefaz, retorno.Motivo });
        }
    }

    // =============================== MDF-e ===============================

    /// <summary>
    /// Emite um MDF-e. Persiste como rascunho, transmite via <see cref="IMdfeFiscalService"/> e atualiza
    /// o estado conforme o retorno. Fiel ao legado <c>MdfeController.Emitir</c> (que era mock).
    /// </summary>
    public class EmitirMdfeCommandHandler : ICommandHandler<EmitirMdfeCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IMdfeFiscalService _mdfeService;

        public EmitirMdfeCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser, IMdfeFiscalService mdfeService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _mdfeService = mdfeService;
        }

        public async Task<CommandResult> Handle(EmitirMdfeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var mdfe = new ManifestoEletronicoDocumentosFiscais(
                request.Serie,
                request.Numero,
                request.Ambiente,
                request.Modal,
                request.TipoEmitente,
                request.UfInicio,
                request.UfFim,
                request.QuantidadeCarregados,
                request.ValorCarga,
                tenantId,
                usuario);

            if (!mdfe.IsValid)
                return CommandResult.Falha(mdfe.Notifications.Select(n => n.Message));

            var retorno = await _mdfeService.EmitirAsync(mdfe, cancellationToken);

            if (retorno.Sucesso)
                mdfe.Autorizar(retorno.ChaveAcesso, retorno.Protocolo, retorno.StatusSefaz, retorno.XmlEnvio, retorno.XmlRetorno);
            else
                mdfe.Rejeitar(retorno.StatusSefaz, retorno.Motivo, retorno.XmlRetorno);

            _context.ManifestosEletronicosDocumentosFiscais.Add(mdfe);
            await _context.SaveChangesAsync(cancellationToken);

            if (!retorno.Sucesso)
                return CommandResult.Falha(new[] { retorno.Motivo }, $"Falha ao emitir o MDF-e (Código: {retorno.StatusSefaz}).");

            return CommandResult.Ok("MDF-e emitido com sucesso!", new { mdfe.Id, mdfe.ChaveAcesso, mdfe.Protocolo, mdfe.Status });
        }
    }

    /// <summary>Encerra um MDF-e autorizado. Fiel ao legado <c>MdfeController.Encerrar</c>.</summary>
    public class EncerrarMdfeCommandHandler : ICommandHandler<EncerrarMdfeCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;
        private readonly IMdfeFiscalService _mdfeService;

        public EncerrarMdfeCommandHandler(ContextFiscal context, ICurrentUser currentUser, IMdfeFiscalService mdfeService)
        {
            _context = context;
            _currentUser = currentUser;
            _mdfeService = mdfeService;
        }

        public async Task<CommandResult> Handle(EncerrarMdfeCommand request, CancellationToken cancellationToken)
        {
            var mdfe = await _context.ManifestosEletronicosDocumentosFiscais
                .FirstOrDefaultAsync(m => m.ChaveAcesso == request.Chave, cancellationToken);

            if (mdfe == null)
                return CommandResult.Falha("MDF-e não localizado com a chave informada.");

            if (mdfe.Status != "Autorizado")
                return CommandResult.Falha($"Apenas MDF-e 'Autorizado' pode ser encerrado. Status atual: {mdfe.Status}");

            var retorno = await _mdfeService.EncerrarAsync(mdfe, request.CodigoMunicipio, cancellationToken);

            if (!retorno.Sucesso)
                return CommandResult.Falha(new[] { retorno.Motivo }, $"Falha ao encerrar o MDF-e (Código: {retorno.StatusSefaz}).");

            var usuario = _currentUser.GetUserId() ?? "system";
            mdfe.Encerrar(request.CodigoMunicipio, retorno.Protocolo, retorno.XmlRetorno, usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("MDF-e encerrado com sucesso.", new { retorno.StatusSefaz, retorno.Motivo });
        }
    }
}
