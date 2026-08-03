using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Imobiliaria.Application.Commands;
using Epros.Modules.Imobiliaria.Application.Queries;
using Epros.Modules.Imobiliaria.Domain.Entities;
using Epros.Modules.Imobiliaria.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Imobiliaria.Application.Handlers
{
    public class CriarContratoServicoCommandHandler : ICommandHandler<CriarContratoServicoCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarContratoServicoCommandHandler(ContextImobiliaria context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarContratoServicoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var contrato = new ContratoServico(
                request.ProprietarioId,
                request.ImovelId,
                request.Descricao,
                request.VigenciaInicio,
                request.VigenciaFim,
                request.Remuneracao,
                tenantId,
                usuario);

            if (!contrato.IsValid)
                return CommandResult.Falha(contrato.Notifications.Select(n => n.Message));

            _context.ContratosServico.Add(contrato);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Contrato de servico criado com sucesso!", new { ContratoId = contrato.Id });
        }
    }

    public class ExcluirContratoServicoCommandHandler : ICommandHandler<ExcluirContratoServicoCommand>
    {
        private readonly ContextImobiliaria _context;

        public ExcluirContratoServicoCommandHandler(ContextImobiliaria context) => _context = context;

        public async Task<CommandResult> Handle(ExcluirContratoServicoCommand request, CancellationToken cancellationToken)
        {
            var contrato = await _context.ContratosServico
                .FirstOrDefaultAsync(c => c.Id == request.ContratoId, cancellationToken);
            if (contrato is null)
                return CommandResult.Falha("Contrato de servico nao encontrado."); // RN-005

            _context.ContratosServico.Remove(contrato);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Contrato de servico excluido com sucesso!");
        }
    }

    public class AlterarContratoServicoCommandHandler : ICommandHandler<AlterarContratoServicoCommand>
    {
        private readonly ContextImobiliaria _context;
        private readonly ICurrentUser _currentUser;

        public AlterarContratoServicoCommandHandler(ContextImobiliaria context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AlterarContratoServicoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var contrato = await _context.ContratosServico
                .FirstOrDefaultAsync(c => c.Id == request.ContratoId, cancellationToken);
            if (contrato is null)
                return CommandResult.Falha("Contrato de servico nao encontrado.");

            contrato.AtualizarDados(request.Descricao, request.VigenciaInicio, request.VigenciaFim, request.Remuneracao, usuario);
            if (!contrato.IsValid)
                return CommandResult.Falha(contrato.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Contrato de servico alterado com sucesso!", new { ContratoId = contrato.Id });
        }
    }

    public class ListarContratosServicoQueryHandler : IQueryHandler<ListarContratosServicoQuery, CommandResult>
    {
        private readonly ContextImobiliaria _context;

        public ListarContratosServicoQueryHandler(ContextImobiliaria context) => _context = context;

        public async Task<CommandResult> Handle(ListarContratosServicoQuery request, CancellationToken cancellationToken)
        {
            var contratos = await _context.ContratosServico
                .AsNoTracking()
                .OrderByDescending(c => c.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Contratos de servico listados com sucesso!", contratos);
        }
    }
}
