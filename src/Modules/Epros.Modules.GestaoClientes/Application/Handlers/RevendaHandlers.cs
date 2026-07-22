using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    // Commands Handlers
    /// <summary>Cria Revenda.</summary>
    public class CriarRevendaCommandHandler : ICommandHandler<CriarRevendaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarRevendaCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarRevendaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var revenda = new Revenda(request.Nome, request.PercentualComissao, tenantId, criadoPor);

            if (!revenda.IsValid)
            {
                return CommandResult.Falha(revenda.Notifications.Select(n => n.Message), "Falha na validação da revenda");
            }

            _context.Revendas.Add(revenda);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Revenda criada com sucesso!", new { RevendaId = revenda.Id });
        }
    }

    /// <summary>Atualiza Revenda.</summary>
    public class AtualizarRevendaCommandHandler : ICommandHandler<AtualizarRevendaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarRevendaCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarRevendaCommand request, CancellationToken cancellationToken)
        {
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var revenda = await _context.Revendas.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
            if (revenda == null)
            {
                return CommandResult.Falha(new[] { "Revenda não encontrada." }, "Erro");
            }

            revenda.Alterar(request.Nome, request.PercentualComissao, request.Ativo, alteradoPor);

            if (!revenda.IsValid)
            {
                return CommandResult.Falha(revenda.Notifications.Select(n => n.Message), "Falha na validação da revenda");
            }

            _context.Revendas.Update(revenda);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Revenda atualizada com sucesso!", new { RevendaId = revenda.Id });
        }
    }

    /// <summary>Exclui Revenda.</summary>
    public class ExcluirRevendaCommandHandler : ICommandHandler<ExcluirRevendaCommand>
    {
        private readonly ContextGestaoClientes _context;

        public ExcluirRevendaCommandHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ExcluirRevendaCommand request, CancellationToken cancellationToken)
        {
            var revenda = await _context.Revendas.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
            if (revenda == null)
            {
                return CommandResult.Falha(new[] { "Revenda não encontrada." }, "Erro");
            }

            _context.Revendas.Remove(revenda);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Revenda excluída com sucesso!");
        }
    }

    // Queries Handlers
    /// <summary>Lista Revendas.</summary>
    public class ListarRevendasQueryHandler : IQueryHandler<ListarRevendasQuery, PagedQueryResult<RevendaDto>>
    {
        private readonly ContextGestaoClientes _context;

        public ListarRevendasQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<PagedQueryResult<RevendaDto>> Handle(ListarRevendasQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Revendas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(r => r.Nome.Contains(request.Search));
            }

            var totalRegistros = await query.CountAsync(cancellationToken);
            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)request.TamanhoPagina);

            var items = await query
                .OrderBy(r => r.Nome)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(r => new RevendaDto
                {
                    Id = r.Id,
                    Nome = r.Nome,
                    PercentualComissao = r.PercentualComissao,
                    Ativo = r.Ativo,
                    CriadoEm = r.CriadoEm
                })
                .ToListAsync(cancellationToken);

            return new PagedQueryResult<RevendaDto>(items, totalRegistros, totalPaginas);
        }
    }

    /// <summary>Obtém Revenda Por Id.</summary>
    public class ObterRevendaPorIdQueryHandler : IQueryHandler<ObterRevendaPorIdQuery, RevendaDto>
    {
        private readonly ContextGestaoClientes _context;

        public ObterRevendaPorIdQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<RevendaDto> Handle(ObterRevendaPorIdQuery request, CancellationToken cancellationToken)
        {
            var r = await _context.Revendas.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (r == null) return null!;

            return new RevendaDto
            {
                Id = r.Id,
                Nome = r.Nome,
                PercentualComissao = r.PercentualComissao,
                Ativo = r.Ativo,
                CriadoEm = r.CriadoEm
            };
        }
    }
}
