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
    // ===== Commands =====

    /// <summary>Cria Grupo de Plano.</summary>
    public class CriarGrupoPlanoCommandHandler : ICommandHandler<CriarGrupoPlanoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarGrupoPlanoCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarGrupoPlanoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var grupo = new GrupoPlano(request.Descricao, tenantId, criadoPor);

            if (!grupo.IsValid)
            {
                return CommandResult.Falha(grupo.Notifications.Select(n => n.Message), "Falha na validação do grupo de plano");
            }

            _context.GrupoPlanos.Add(grupo);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Grupo de plano criado com sucesso!", new { GrupoPlanoId = grupo.Id });
        }
    }

    /// <summary>Atualiza Grupo de Plano. Ativo=false inativa via soft-delete (não há coluna Ativo dedicada).</summary>
    public class AtualizarGrupoPlanoCommandHandler : ICommandHandler<AtualizarGrupoPlanoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarGrupoPlanoCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarGrupoPlanoCommand request, CancellationToken cancellationToken)
        {
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var grupo = await _context.GrupoPlanos.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
            if (grupo == null)
            {
                return CommandResult.Falha(new[] { "Grupo de plano não encontrado." }, "Erro");
            }

            grupo.Atualizar(request.Descricao, alteradoPor);

            if (!grupo.IsValid)
            {
                return CommandResult.Falha(grupo.Notifications.Select(n => n.Message), "Falha na validação do grupo de plano");
            }

            // GrupoPlano não possui coluna Ativo; inativação é mapeada para soft-delete.
            if (!request.Ativo)
            {
                grupo.Deletar(alteradoPor);
            }

            _context.GrupoPlanos.Update(grupo);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Grupo de plano atualizado com sucesso!", new { GrupoPlanoId = grupo.Id });
        }
    }

    /// <summary>Exclui (soft-delete) Grupo de Plano.</summary>
    public class ExcluirGrupoPlanoCommandHandler : ICommandHandler<ExcluirGrupoPlanoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public ExcluirGrupoPlanoCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExcluirGrupoPlanoCommand request, CancellationToken cancellationToken)
        {
            var deletadoPor = _currentUser.GetUserId() ?? "system";

            var grupo = await _context.GrupoPlanos.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
            if (grupo == null)
            {
                return CommandResult.Falha(new[] { "Grupo de plano não encontrado." }, "Erro");
            }

            grupo.Deletar(deletadoPor);
            _context.GrupoPlanos.Update(grupo);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Grupo de plano excluído com sucesso!");
        }
    }

    // ===== Queries =====

    /// <summary>Lista Grupos de Plano (paginado).</summary>
    public class ListarGruposPlanoQueryHandler : IQueryHandler<ListarGruposPlanoQuery, PagedQueryResult<GrupoPlanoDto>>
    {
        private readonly ContextGestaoClientes _context;

        public ListarGruposPlanoQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<PagedQueryResult<GrupoPlanoDto>> Handle(ListarGruposPlanoQuery request, CancellationToken cancellationToken)
        {
            var query = _context.GrupoPlanos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(g => g.Descricao.Contains(request.Search));
            }

            var totalRegistros = await query.CountAsync(cancellationToken);
            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)request.TamanhoPagina);

            var items = await query
                .OrderBy(g => g.Descricao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(g => new GrupoPlanoDto
                {
                    Id = g.Id,
                    Descricao = g.Descricao,
                    Ativo = g.DeletadoEm == null,
                    CriadoEm = g.CriadoEm
                })
                .ToListAsync(cancellationToken);

            return new PagedQueryResult<GrupoPlanoDto>(items, totalRegistros, totalPaginas);
        }
    }

    /// <summary>Obtém Grupo de Plano por Id.</summary>
    public class ObterGrupoPlanoPorIdQueryHandler : IQueryHandler<ObterGrupoPlanoPorIdQuery, GrupoPlanoDto>
    {
        private readonly ContextGestaoClientes _context;

        public ObterGrupoPlanoPorIdQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<GrupoPlanoDto> Handle(ObterGrupoPlanoPorIdQuery request, CancellationToken cancellationToken)
        {
            var g = await _context.GrupoPlanos.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (g == null) return null!;

            return new GrupoPlanoDto
            {
                Id = g.Id,
                Descricao = g.Descricao,
                Ativo = g.DeletadoEm == null,
                CriadoEm = g.CriadoEm
            };
        }
    }
}
