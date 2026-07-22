using System;
using System.Collections.Generic;
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
    /// <summary>Cria Perfil Acesso.</summary>
    public class CriarPerfilAcessoCommandHandler : ICommandHandler<CriarPerfilAcessoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPerfilAcessoCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPerfilAcessoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.PerfisAcessos
                .AnyAsync(p => p.Descricao == request.Descricao && p.TenantId == tenantId, cancellationToken);
            if (duplicado)
                return CommandResult.Falha(new[] { "Já existe um perfil de acesso com esta descrição." }, "Erro de validação");

            var perfil = new PerfilAcesso(request.Descricao, tenantId, userId);
            if (!perfil.IsValid)
                return CommandResult.Falha(perfil.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio do Perfil de Acesso não foram atendidas.");

            if (request.Acessos != null && request.Acessos.Count > 0)
            {
                var novosAcessos = request.Acessos
                    .Select(a => new PerfilAcessoMenu(perfil.Id, a.MenuId, a.MenuItemNivel1Id, a.MenuItemNivel2Id, a.Ver, a.Editar, a.Excluir, tenantId, userId))
                    .ToList();
                perfil.SincronizarAcessos(novosAcessos, userId);
                if (!perfil.IsValid)
                    return CommandResult.Falha(perfil.Notifications.Select(n => n.Message).Distinct(), "Acessos inválidos.");
            }

            _context.PerfisAcessos.Add(perfil);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Perfil de acesso criado com sucesso!", new { PerfilAcessoId = perfil.Id });
        }
    }

    /// <summary>Atualiza Perfil Acesso.</summary>
    public class AtualizarPerfilAcessoCommandHandler : ICommandHandler<AtualizarPerfilAcessoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarPerfilAcessoCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarPerfilAcessoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var perfil = await _context.PerfisAcessos
                .Include(p => p.Acessos)
                .FirstOrDefaultAsync(p => p.Id == request.Id && p.TenantId == tenantId, cancellationToken);

            if (perfil == null)
                return CommandResult.Falha(new[] { "Perfil de acesso não encontrado." });

            var duplicado = await _context.PerfisAcessos
                .AnyAsync(p => p.Descricao == request.Descricao && p.TenantId == tenantId && p.Id != request.Id, cancellationToken);
            if (duplicado)
                return CommandResult.Falha(new[] { "Já existe outro perfil de acesso com esta descrição." }, "Erro de validação");

            perfil.Atualizar(request.Descricao, userId);
            if (!perfil.IsValid)
                return CommandResult.Falha(perfil.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio do Perfil de Acesso não foram atendidas.");

            var novosAcessos = (request.Acessos ?? new List<PerfilAcessoMenuInput>())
                .Select(a => new PerfilAcessoMenu(perfil.Id, a.MenuId, a.MenuItemNivel1Id, a.MenuItemNivel2Id, a.Ver, a.Editar, a.Excluir, tenantId, userId))
                .ToList();
            perfil.SincronizarAcessos(novosAcessos, userId);
            if (!perfil.IsValid)
                return CommandResult.Falha(perfil.Notifications.Select(n => n.Message).Distinct(), "Acessos inválidos.");

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Perfil de acesso atualizado com sucesso!", new { PerfilAcessoId = perfil.Id });
        }
    }

    /// <summary>Exclui Perfil Acesso.</summary>
    public class DeletarPerfilAcessoCommandHandler : ICommandHandler<DeletarPerfilAcessoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarPerfilAcessoCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarPerfilAcessoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var perfil = await _context.PerfisAcessos
                .FirstOrDefaultAsync(p => p.Id == request.Id && p.TenantId == tenantId, cancellationToken);

            if (perfil == null)
                return CommandResult.Falha(new[] { "Perfil de acesso não encontrado." });

            // Soft delete (padrão para entidades SaaS)
            perfil.Deletar(userId);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Perfil de acesso removido com sucesso!");
        }
    }

    /// <summary>Lista Perfis Acesso.</summary>
    public class ListarPerfisAcessoQueryHandler : IQueryHandler<ListarPerfisAcessoQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarPerfisAcessoQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarPerfisAcessoQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.PerfisAcessos
                .Where(p => p.TenantId == tenantId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(p => p.Descricao.Contains(request.Localizar));

            if (request.Ativo.HasValue)
                query = query.Where(p => p.Ativo == request.Ativo.Value);

            var pagina = request.Pagina < 1 ? 1 : request.Pagina;
            var tamanho = request.TamanhoPagina < 1 ? 20 : request.TamanhoPagina;

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(p => p.Descricao)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .Select(p => new PerfilAcessoDto
                {
                    Id = p.Id,
                    Descricao = p.Descricao,
                    Ativo = p.Ativo
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Perfis de acesso listados com sucesso.", new { Total = total, Pagina = pagina, Itens = itens });
        }
    }

    /// <summary>Obtém Perfil Acesso Por Id.</summary>
    public class ObterPerfilAcessoPorIdQueryHandler : IQueryHandler<ObterPerfilAcessoPorIdQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterPerfilAcessoPorIdQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterPerfilAcessoPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var perfil = await _context.PerfisAcessos
                .Include(p => p.Acessos)
                .FirstOrDefaultAsync(p => p.Id == request.Id && p.TenantId == tenantId, cancellationToken);

            if (perfil == null)
                return CommandResult.Falha(new[] { "Perfil de acesso não encontrado." });

            var dto = new PerfilAcessoDto
            {
                Id = perfil.Id,
                Descricao = perfil.Descricao,
                Ativo = perfil.Ativo,
                Acessos = perfil.Acessos.Select(a => new PerfilAcessoMenuDto
                {
                    Id = a.Id,
                    MenuId = a.MenuId,
                    MenuItemNivel1Id = a.MenuItemNivel1Id,
                    MenuItemNivel2Id = a.MenuItemNivel2Id,
                    Ver = a.Ver,
                    Editar = a.Editar,
                    Excluir = a.Excluir
                }).ToList()
            };

            return CommandResult.Ok("Perfil de acesso obtido com sucesso.", dto);
        }
    }
}
