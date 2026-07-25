using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class CriarPerfilAcessoCommandHandler : ICommandHandler<CriarPerfilAcessoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPerfilAcessoCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPerfilAcessoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            // Validar descrição duplicada
            var descricaoLower = request.Descricao.Trim().ToLower();
            var duplicado = await _context.PerfisAcessos
                .AnyAsync(p => p.TenantId == tenantId && 
                               p.Descricao.ToLower() == descricaoLower && 
                               p.DeletadoEm == null, cancellationToken);

            if (duplicado)
            {
                return CommandResult.Falha(new[] { "Já existe um perfil de acesso cadastrado com esta descrição." });
            }

            var perfil = new PerfilAcesso(request.Descricao, tenantId, userId);
            if (!perfil.IsValid)
            {
                return CommandResult.Falha(perfil.Notifications.Select(n => n.Message));
            }

            var acessos = request.Acessos.Select(a => new PerfilAcessoMenu(
                perfil.Id,
                a.MenuId,
                a.MenuItemNivel1Id,
                a.MenuItemNivel2Id,
                a.Ver,
                a.Editar,
                a.Excluir,
                tenantId,
                userId
            )).ToList();

            perfil.SincronizarAcessos(acessos, userId);
            if (!perfil.IsValid)
            {
                return CommandResult.Falha(perfil.Notifications.Select(n => n.Message));
            }

            _context.PerfisAcessos.Add(perfil);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Perfil de acesso criado com sucesso!", new { PerfilAcessoId = perfil.Id });
        }
    }

    public class AtualizarPerfilAcessoCommandHandler : ICommandHandler<AtualizarPerfilAcessoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IPermissaoCacheManager _cacheManager;

        public AtualizarPerfilAcessoCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IPermissaoCacheManager cacheManager)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _cacheManager = cacheManager;
        }

        public async Task<CommandResult> Handle(AtualizarPerfilAcessoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var perfil = await _context.PerfisAcessos
                .Include(p => p.Acessos)
                .FirstOrDefaultAsync(p => p.Id == request.Id && p.TenantId == tenantId && p.DeletadoEm == null, cancellationToken);

            if (perfil == null)
            {
                return CommandResult.Falha(new[] { "Perfil de acesso não encontrado." });
            }

            // Validar descrição duplicada
            var descricaoLower = request.Descricao.Trim().ToLower();
            var duplicado = await _context.PerfisAcessos
                .AnyAsync(p => p.TenantId == tenantId && 
                               p.Id != request.Id && 
                               p.Descricao.ToLower() == descricaoLower && 
                               p.DeletadoEm == null, cancellationToken);

            if (duplicado)
            {
                return CommandResult.Falha(new[] { "Já existe um outro perfil de acesso cadastrado com esta descrição." });
            }

            perfil.Atualizar(request.Descricao, userId);
            if (!perfil.IsValid)
            {
                return CommandResult.Falha(perfil.Notifications.Select(n => n.Message));
            }

            // Remover acessos antigos explicitamente do Contexto para evitar órfãos
            var acessosAntigos = perfil.Acessos.ToList();
            if (acessosAntigos.Any())
            {
                _context.PerfisAcessosMenus.RemoveRange(acessosAntigos);
            }

            var novosAcessos = request.Acessos.Select(a => new PerfilAcessoMenu(
                perfil.Id,
                a.MenuId,
                a.MenuItemNivel1Id,
                a.MenuItemNivel2Id,
                a.Ver,
                a.Editar,
                a.Excluir,
                tenantId,
                userId
            )).ToList();

            perfil.SincronizarAcessos(novosAcessos, userId);
            if (!perfil.IsValid)
            {
                return CommandResult.Falha(perfil.Notifications.Select(n => n.Message));
            }

            foreach (var acesso in novosAcessos)
            {
                _context.Entry(acesso).State = EntityState.Added;
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Invalida o cache
            _cacheManager.InvalidarCachePerfil(tenantId, perfil.Id);

            return CommandResult.Ok("Perfil de acesso updated com sucesso!");
        }
    }

    public class DeletarPerfilAcessoCommandHandler : ICommandHandler<DeletarPerfilAcessoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IPermissaoCacheManager _cacheManager;

        public DeletarPerfilAcessoCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IPermissaoCacheManager cacheManager)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _cacheManager = cacheManager;
        }

        public async Task<CommandResult> Handle(DeletarPerfilAcessoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var perfil = await _context.PerfisAcessos
                .Include(p => p.Acessos)
                .FirstOrDefaultAsync(p => p.Id == request.Id && p.TenantId == tenantId && p.DeletadoEm == null, cancellationToken);

            if (perfil == null)
            {
                return CommandResult.Falha(new[] { "Perfil de acesso não encontrado." });
            }

            // Soft delete no perfil
            perfil.Deletar(userId);

            // Soft delete nos acessos relacionados
            foreach (var acesso in perfil.Acessos)
            {
                acesso.Deletar(userId);
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Invalida o cache
            _cacheManager.InvalidarCachePerfil(tenantId, perfil.Id);

            return CommandResult.Ok("Perfil de acesso excluído logicamente com sucesso!");
        }
    }
}
