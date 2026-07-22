using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Cria Papel (RBAC).</summary>
    public class CriarPapelCommandHandler : ICommandHandler<CriarPapelCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPapelCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPapelCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.Papeis.AnyAsync(p => p.Name == request.Name && p.TenantId == tenantId, cancellationToken);
            if (duplicado)
                return CommandResult.Falha(new[] { "Já existe um papel com este nome." }, "Erro de validação");

            var papel = new Papel(request.Name, request.Label, request.GuardName, request.Editable, request.RoleSystem,
                request.RoleType, request.RoleHomepage, request.Modules, tenantId, userId);
            if (!papel.IsValid)
                return CommandResult.Falha(papel.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio do Papel não foram atendidas.");

            if (request.CapacidadeIds != null && request.CapacidadeIds.Count > 0)
            {
                var caps = request.CapacidadeIds.Distinct().Select(cid => new PapelCapacidade(papel.Id, cid, tenantId, userId)).ToList();
                papel.DefinirCapacidades(caps, userId);
                if (!papel.IsValid)
                    return CommandResult.Falha(papel.Notifications.Select(n => n.Message).Distinct(), "Capacidades inválidas.");
            }

            _context.Papeis.Add(papel);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Papel criado com sucesso!", new { papel.Id });
        }
    }

    /// <summary>Atualiza Papel (RBAC).</summary>
    public class AtualizarPapelCommandHandler : ICommandHandler<AtualizarPapelCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarPapelCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarPapelCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var papel = await _context.Papeis.Include(p => p.Capacidades)
                .FirstOrDefaultAsync(p => p.Id == request.Id && p.TenantId == tenantId, cancellationToken);
            if (papel == null)
                return CommandResult.Falha(new[] { "Papel não encontrado." });

            var duplicado = await _context.Papeis.AnyAsync(p => p.Name == request.Name && p.TenantId == tenantId && p.Id != request.Id, cancellationToken);
            if (duplicado)
                return CommandResult.Falha(new[] { "Já existe outro papel com este nome." }, "Erro de validação");

            papel.Alterar(request.Name, request.Label, request.GuardName, request.Editable, request.RoleType, request.RoleHomepage, request.Modules, userId);
            if (!papel.IsValid)
                return CommandResult.Falha(papel.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio do Papel não foram atendidas.");

            var caps = (request.CapacidadeIds ?? new List<Guid>()).Distinct().Select(cid => new PapelCapacidade(papel.Id, cid, tenantId, userId)).ToList();
            papel.DefinirCapacidades(caps, userId);
            if (!papel.IsValid)
                return CommandResult.Falha(papel.Notifications.Select(n => n.Message).Distinct(), "Capacidades inválidas.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Papel atualizado com sucesso!", new { papel.Id });
        }
    }

    /// <summary>Exclui (soft) Papel. REG-043: papel de sistema não pode ser excluído.</summary>
    public class DeletarPapelCommandHandler : ICommandHandler<DeletarPapelCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarPapelCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarPapelCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var papel = await _context.Papeis.FirstOrDefaultAsync(p => p.Id == request.Id && p.TenantId == tenantId, cancellationToken);
            if (papel == null)
                return CommandResult.Falha(new[] { "Papel não encontrado." });
            if (papel.RoleSystem)
                return CommandResult.Falha(new[] { "Papel de sistema protegido não pode ser excluído." });

            papel.Deletar(userId);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Papel removido com sucesso!");
        }
    }

    /// <summary>Cria Capacidade (RBAC).</summary>
    public class CriarCapacidadeCommandHandler : ICommandHandler<CriarCapacidadeCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCapacidadeCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCapacidadeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.Capacidades.AnyAsync(c => c.Name == request.Name && c.TenantId == tenantId, cancellationToken);
            if (duplicado)
                return CommandResult.Falha(new[] { "Já existe uma capacidade com este nome." }, "Erro de validação");

            var capacidade = new Capacidade(request.Name, request.Label, request.Module, request.AddOn, request.PermissionKey, tenantId, userId);
            if (!capacidade.IsValid)
                return CommandResult.Falha(capacidade.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio da Capacidade não foram atendidas.");

            _context.Capacidades.Add(capacidade);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Capacidade criada com sucesso!", new { capacidade.Id });
        }
    }

    /// <summary>Atualiza Capacidade (RBAC).</summary>
    public class AtualizarCapacidadeCommandHandler : ICommandHandler<AtualizarCapacidadeCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarCapacidadeCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarCapacidadeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var capacidade = await _context.Capacidades.FirstOrDefaultAsync(c => c.Id == request.Id && c.TenantId == tenantId, cancellationToken);
            if (capacidade == null)
                return CommandResult.Falha(new[] { "Capacidade não encontrada." });

            var duplicado = await _context.Capacidades.AnyAsync(c => c.Name == request.Name && c.TenantId == tenantId && c.Id != request.Id, cancellationToken);
            if (duplicado)
                return CommandResult.Falha(new[] { "Já existe outra capacidade com este nome." }, "Erro de validação");

            capacidade.Alterar(request.Name, request.Label, request.Module, request.AddOn, request.PermissionKey, userId);
            if (!capacidade.IsValid)
                return CommandResult.Falha(capacidade.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio da Capacidade não foram atendidas.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Capacidade atualizada com sucesso!", new { capacidade.Id });
        }
    }

    /// <summary>Exclui (soft) Capacidade.</summary>
    public class DeletarCapacidadeCommandHandler : ICommandHandler<DeletarCapacidadeCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarCapacidadeCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarCapacidadeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var capacidade = await _context.Capacidades.FirstOrDefaultAsync(c => c.Id == request.Id && c.TenantId == tenantId, cancellationToken);
            if (capacidade == null)
                return CommandResult.Falha(new[] { "Capacidade não encontrada." });

            capacidade.Deletar(userId);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Capacidade removida com sucesso!");
        }
    }

    /// <summary>Atribui papel a usuário (RBAC direto).</summary>
    public class AtribuirPapelUsuarioCommandHandler : ICommandHandler<AtribuirPapelUsuarioCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtribuirPapelUsuarioCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtribuirPapelUsuarioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var papelExiste = await _context.Papeis.AnyAsync(p => p.Id == request.PapelId && p.TenantId == tenantId, cancellationToken);
            if (!papelExiste)
                return CommandResult.Falha(new[] { "Papel não encontrado para o inquilino atual." });

            var jaAtribuido = await _context.UsuariosPapeis.AnyAsync(up => up.UsuarioId == request.UsuarioId && up.PapelId == request.PapelId && up.TenantId == tenantId, cancellationToken);
            if (jaAtribuido)
                return CommandResult.Falha(new[] { "Papel já atribuído a este usuário." }, "Erro de validação");

            var vinculo = new UsuarioPapel(request.UsuarioId, request.PapelId, request.ModelType, tenantId, userId);
            if (!vinculo.IsValid)
                return CommandResult.Falha(vinculo.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio do vínculo UsuarioPapel não foram atendidas.");

            _context.UsuariosPapeis.Add(vinculo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Papel atribuído ao usuário com sucesso!", new { vinculo.Id });
        }
    }

    /// <summary>Remove papel de usuário (RBAC direto).</summary>
    public class RemoverPapelUsuarioCommandHandler : ICommandHandler<RemoverPapelUsuarioCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RemoverPapelUsuarioCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RemoverPapelUsuarioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var vinculo = await _context.UsuariosPapeis.FirstOrDefaultAsync(up => up.Id == request.Id && up.TenantId == tenantId, cancellationToken);
            if (vinculo == null)
                return CommandResult.Falha(new[] { "Vínculo usuário-papel não encontrado." });

            vinculo.Deletar(userId);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Papel removido do usuário com sucesso!");
        }
    }

    /// <summary>Cria nível de usuário (quotas).</summary>
    public class CriarNivelUsuarioCommandHandler : ICommandHandler<CriarNivelUsuarioCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarNivelUsuarioCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarNivelUsuarioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.NiveisUsuario.AnyAsync(n => n.LevelId == request.LevelId && n.TenantId == tenantId, cancellationToken);
            if (duplicado)
                return CommandResult.Falha(new[] { "Já existe um nível de usuário com este LevelId." }, "Erro de validação");

            var nivel = new NivelUsuario(request.LevelId, request.Label, request.CanUpload, request.WaitBetweenDownloads, request.DownloadSpeed,
                request.MaxStorageBytes, request.ShowSiteAdverts, request.ShowUpgradeScreen, request.DaysToKeepInactiveFiles, request.ConcurrentUploads,
                request.ConcurrentDownloads, request.DownloadsPer24Hours, request.MaxDownloadFilesizeAllowed, request.MaxRemoteDownloadUrls,
                request.MaxUploadSize, request.LevelType, request.OnUpgradePage, tenantId, userId);
            if (!nivel.IsValid)
                return CommandResult.Falha(nivel.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio do Nível de Usuário não foram atendidas.");

            _context.NiveisUsuario.Add(nivel);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Nível de usuário criado com sucesso!", new { nivel.Id });
        }
    }

    /// <summary>Adiciona preço a um nível de usuário.</summary>
    public class AdicionarPrecoNivelUsuarioCommandHandler : ICommandHandler<AdicionarPrecoNivelUsuarioCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarPrecoNivelUsuarioCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarPrecoNivelUsuarioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var nivelExiste = await _context.NiveisUsuario.AnyAsync(n => n.Id == request.NivelUsuarioId && n.TenantId == tenantId, cancellationToken);
            if (!nivelExiste)
                return CommandResult.Falha(new[] { "Nível de usuário não encontrado para o inquilino atual." });

            var preco = new PrecoNivelUsuario(request.NivelUsuarioId, request.PricingLabel, request.PackagePricingType, request.Period,
                request.DownloadAllowance, request.Price, tenantId, userId);
            if (!preco.IsValid)
                return CommandResult.Falha(preco.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio do Preço de Nível não foram atendidas.");

            _context.PrecosNivelUsuario.Add(preco);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Preço de nível de usuário adicionado com sucesso!", new { preco.Id });
        }
    }
}
