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
    /// <summary>Cria Vendedor.</summary>
    public class CriarVendedorCommandHandler : ICommandHandler<CriarVendedorCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarVendedorCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarVendedorCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var vendedor = new Vendedor(
                request.RevendaId,
                request.Nome,
                request.Email,
                request.Telefone,
                request.PercentualComissao,
                tenantId,
                criadoPor
            );

            if (!vendedor.IsValid)
            {
                return CommandResult.Falha(vendedor.Notifications.Select(n => n.Message), "Falha na validação do vendedor");
            }

            _context.Vendedores.Add(vendedor);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Vendedor criado com sucesso!", new { VendedorId = vendedor.Id });
        }
    }

    /// <summary>Atualiza Vendedor.</summary>
    public class AtualizarVendedorCommandHandler : ICommandHandler<AtualizarVendedorCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarVendedorCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarVendedorCommand request, CancellationToken cancellationToken)
        {
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);
            if (vendedor == null)
            {
                return CommandResult.Falha(new[] { "Vendedor não encontrado." }, "Erro");
            }

            vendedor.Alterar(
                request.Nome,
                request.Email,
                request.Telefone,
                request.PercentualComissao,
                request.RevendaId,
                request.Ativo,
                alteradoPor
            );

            if (!vendedor.IsValid)
            {
                return CommandResult.Falha(vendedor.Notifications.Select(n => n.Message), "Falha na validação do vendedor");
            }

            _context.Vendedores.Update(vendedor);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Vendedor atualizado com sucesso!", new { VendedorId = vendedor.Id });
        }
    }

    /// <summary>Exclui Vendedor.</summary>
    public class ExcluirVendedorCommandHandler : ICommandHandler<ExcluirVendedorCommand>
    {
        private readonly ContextGestaoClientes _context;

        public ExcluirVendedorCommandHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ExcluirVendedorCommand request, CancellationToken cancellationToken)
        {
            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);
            if (vendedor == null)
            {
                return CommandResult.Falha(new[] { "Vendedor não encontrado." }, "Erro");
            }

            _context.Vendedores.Remove(vendedor);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Vendedor excluído com sucesso!");
        }
    }

    // Queries Handlers
    /// <summary>Lista Vendedores.</summary>
    public class ListarVendedoresQueryHandler : IQueryHandler<ListarVendedoresQuery, PagedQueryResult<VendedorDto>>
    {
        private readonly ContextGestaoClientes _context;

        public ListarVendedoresQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<PagedQueryResult<VendedorDto>> Handle(ListarVendedoresQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Vendedores.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(v => v.Nome.Contains(request.Search) || v.Email.Contains(request.Search));
            }

            var totalRegistros = await query.CountAsync(cancellationToken);
            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)request.TamanhoPagina);

            var items = await query
                .OrderBy(v => v.Nome)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(v => new
                {
                    v.Id,
                    v.RevendaId,
                    v.Nome,
                    v.Email,
                    v.Telefone,
                    v.PercentualComissao,
                    v.Ativo,
                    v.CriadoEm
                })
                .ToListAsync(cancellationToken);

            // Buscar nomes das revendas associadas
            var revendaIds = items.Where(x => x.RevendaId.HasValue).Select(x => x.RevendaId!.Value).Distinct().ToList();
            var revendas = await _context.Revendas
                .Where(r => revendaIds.Contains(r.Id))
                .Select(r => new { r.Id, r.Nome })
                .ToDictionaryAsync(r => r.Id, r => r.Nome, cancellationToken);

            var dtos = items.Select(v => new VendedorDto
            {
                Id = v.Id,
                RevendaId = v.RevendaId,
                RevendaNome = v.RevendaId.HasValue && revendas.TryGetValue(v.RevendaId.Value, out var nome) ? nome : null,
                Nome = v.Nome,
                Email = v.Email,
                Telefone = v.Telefone,
                PercentualComissao = v.PercentualComissao,
                Ativo = v.Ativo,
                CriadoEm = v.CriadoEm
            });

            return new PagedQueryResult<VendedorDto>(dtos, totalRegistros, totalPaginas);
        }
    }

    /// <summary>Obtém Vendedor Por Id.</summary>
    public class ObterVendedorPorIdQueryHandler : IQueryHandler<ObterVendedorPorIdQuery, VendedorDto>
    {
        private readonly ContextGestaoClientes _context;

        public ObterVendedorPorIdQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<VendedorDto> Handle(ObterVendedorPorIdQuery request, CancellationToken cancellationToken)
        {
            var v = await _context.Vendedores.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (v == null) return null!;

            string? revendaNome = null;
            if (v.RevendaId.HasValue)
            {
                var r = await _context.Revendas.FirstOrDefaultAsync(x => x.Id == v.RevendaId.Value, cancellationToken);
                revendaNome = r?.Nome;
            }

            return new VendedorDto
            {
                Id = v.Id,
                RevendaId = v.RevendaId,
                RevendaNome = revendaNome,
                Nome = v.Nome,
                Email = v.Email,
                Telefone = v.Telefone,
                PercentualComissao = v.PercentualComissao,
                Ativo = v.Ativo,
                CriadoEm = v.CriadoEm
            };
        }
    }
}
