using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    public class CriarMoedaCommandHandler : ICommandHandler<CriarMoedaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        public CriarMoedaCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CriarMoedaCommand request, CancellationToken cancellationToken)
        {
            var validator = new CriarMoedaCommandValidator();
            var vr = await validator.ValidateAsync(request, cancellationToken);
            if (!vr.IsValid) return CommandResult.Falha(vr.Errors.Select(e => e.ErrorMessage));

            var iso = request.CodigoISO.ToUpperInvariant();
            var existe = await _context.Moedas.IgnoreQueryFilters().AnyAsync(m => m.CodigoISO == iso && m.DeletadoEm == null, cancellationToken);
            if (existe) return CommandResult.Falha(new[] { "Já existe uma moeda com este código ISO." });

            var moeda = new Moeda(request.CodigoISO, request.Simbolo, request.CasasDecimais, _currentUser.GetUserId() ?? "system", request.Nome);
            if (!moeda.IsValid) return CommandResult.Falha(moeda.Notifications.Select(n => n.Message));
            _context.Moedas.Add(moeda);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Moeda criada com sucesso!", new { moeda.Id });
        }
    }

    public class AtualizarMoedaCommandHandler : ICommandHandler<AtualizarMoedaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        public AtualizarMoedaCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarMoedaCommand request, CancellationToken cancellationToken)
        {
            var moeda = await _context.Moedas.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == request.Id && m.DeletadoEm == null, cancellationToken);
            if (moeda == null) return CommandResult.Falha(new[] { "Moeda não encontrada." });
            moeda.Atualizar(request.CodigoISO, request.Simbolo, request.CasasDecimais, _currentUser.GetUserId() ?? "system", request.Nome);
            if (!moeda.IsValid) return CommandResult.Falha(moeda.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Moeda atualizada com sucesso!");
        }
    }

    public class ExcluirMoedaCommandHandler : ICommandHandler<ExcluirMoedaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        public ExcluirMoedaCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(ExcluirMoedaCommand request, CancellationToken cancellationToken)
        {
            var moeda = await _context.Moedas.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == request.Id && m.DeletadoEm == null, cancellationToken);
            if (moeda == null) return CommandResult.Falha(new[] { "Moeda não encontrada." });
            moeda.Deletar(_currentUser.GetUserId() ?? "system");
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Moeda excluída com sucesso!");
        }
    }

    public class ListarMoedasCatalogoQueryHandler : IQueryHandler<ListarMoedasCatalogoQuery, List<MoedaDto>>
    {
        private readonly ContextGestaoClientes _context;
        public ListarMoedasCatalogoQueryHandler(ContextGestaoClientes context) { _context = context; }

        public async Task<List<MoedaDto>> Handle(ListarMoedasCatalogoQuery request, CancellationToken cancellationToken)
        {
            return await _context.Moedas.IgnoreQueryFilters()
                .Where(m => m.DeletadoEm == null)
                .OrderBy(m => m.CodigoISO)
                .Select(m => new MoedaDto { Id = m.Id, CodigoISO = m.CodigoISO, Nome = m.Nome, Simbolo = m.Simbolo, CasasDecimais = m.CasasDecimais })
                .ToListAsync(cancellationToken);
        }
    }
}
