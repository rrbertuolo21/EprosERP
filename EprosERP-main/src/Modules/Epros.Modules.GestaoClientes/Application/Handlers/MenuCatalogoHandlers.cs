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
    /// <summary>Cria Menu principal (catálogo).</summary>
    public class CriarMenuCommandHandler : ICommandHandler<CriarMenuCommand>
    {
        private readonly ContextGestaoClientes _context;
        public CriarMenuCommandHandler(ContextGestaoClientes context) { _context = context; }

        public async Task<CommandResult> Handle(CriarMenuCommand request, CancellationToken cancellationToken)
        {
            var menu = new Menu(request.Descricao, request.Icon, request.To, request.Ordem, request.Modulo);
            if (!menu.IsValid)
                return CommandResult.Falha(menu.Notifications.Select(n => n.Message).Distinct(), "Menu inválido.");

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Menu criado com sucesso!", new { menu.Id });
        }
    }

    /// <summary>Atualiza Menu principal (catálogo).</summary>
    public class AtualizarMenuCommandHandler : ICommandHandler<AtualizarMenuCommand>
    {
        private readonly ContextGestaoClientes _context;
        public AtualizarMenuCommandHandler(ContextGestaoClientes context) { _context = context; }

        public async Task<CommandResult> Handle(AtualizarMenuCommand request, CancellationToken cancellationToken)
        {
            var menu = await _context.Menus.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (menu == null)
                return CommandResult.Falha(new[] { "Menu não encontrado." });

            menu.Alterar(request.Descricao, request.Icon, request.To, request.Ordem, request.Modulo);
            if (!menu.IsValid)
                return CommandResult.Falha(menu.Notifications.Select(n => n.Message).Distinct(), "Menu inválido.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Menu atualizado com sucesso!", new { menu.Id });
        }
    }

    /// <summary>Exclui Menu principal (catálogo estrutural, cascata para itens).</summary>
    public class DeletarMenuCommandHandler : ICommandHandler<DeletarMenuCommand>
    {
        private readonly ContextGestaoClientes _context;
        public DeletarMenuCommandHandler(ContextGestaoClientes context) { _context = context; }

        public async Task<CommandResult> Handle(DeletarMenuCommand request, CancellationToken cancellationToken)
        {
            var menu = await _context.Menus.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (menu == null)
                return CommandResult.Falha(new[] { "Menu não encontrado." });

            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Menu removido com sucesso!");
        }
    }

    /// <summary>Cria item de menu nível 1.</summary>
    public class CriarMenuItemNivel1CommandHandler : ICommandHandler<CriarMenuItemNivel1Command>
    {
        private readonly ContextGestaoClientes _context;
        public CriarMenuItemNivel1CommandHandler(ContextGestaoClientes context) { _context = context; }

        public async Task<CommandResult> Handle(CriarMenuItemNivel1Command request, CancellationToken cancellationToken)
        {
            var menuExiste = await _context.Menus.AnyAsync(m => m.Id == request.MenuId, cancellationToken);
            if (!menuExiste)
                return CommandResult.Falha(new[] { "Menu principal não encontrado." });

            var item = new MenuItemNivel1(request.MenuId, request.Descricao, request.Icon, request.To, request.Ordem, request.Modulo);
            if (!item.IsValid)
                return CommandResult.Falha(item.Notifications.Select(n => n.Message).Distinct(), "Item de nível 1 inválido.");

            _context.MenusItensNivel1.Add(item);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Item de menu (nível 1) criado com sucesso!", new { item.Id });
        }
    }

    /// <summary>Atualiza item de menu nível 1.</summary>
    public class AtualizarMenuItemNivel1CommandHandler : ICommandHandler<AtualizarMenuItemNivel1Command>
    {
        private readonly ContextGestaoClientes _context;
        public AtualizarMenuItemNivel1CommandHandler(ContextGestaoClientes context) { _context = context; }

        public async Task<CommandResult> Handle(AtualizarMenuItemNivel1Command request, CancellationToken cancellationToken)
        {
            var item = await _context.MenusItensNivel1.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (item == null)
                return CommandResult.Falha(new[] { "Item de nível 1 não encontrado." });

            item.Alterar(request.Descricao, request.Icon, request.To, request.Ordem, request.Modulo);
            if (!item.IsValid)
                return CommandResult.Falha(item.Notifications.Select(n => n.Message).Distinct(), "Item de nível 1 inválido.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Item de menu (nível 1) atualizado com sucesso!", new { item.Id });
        }
    }

    /// <summary>Exclui item de menu nível 1 (cascata para nível 2).</summary>
    public class DeletarMenuItemNivel1CommandHandler : ICommandHandler<DeletarMenuItemNivel1Command>
    {
        private readonly ContextGestaoClientes _context;
        public DeletarMenuItemNivel1CommandHandler(ContextGestaoClientes context) { _context = context; }

        public async Task<CommandResult> Handle(DeletarMenuItemNivel1Command request, CancellationToken cancellationToken)
        {
            var item = await _context.MenusItensNivel1.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (item == null)
                return CommandResult.Falha(new[] { "Item de nível 1 não encontrado." });

            _context.MenusItensNivel1.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Item de menu (nível 1) removido com sucesso!");
        }
    }

    /// <summary>Cria item de menu nível 2.</summary>
    public class CriarMenuItemNivel2CommandHandler : ICommandHandler<CriarMenuItemNivel2Command>
    {
        private readonly ContextGestaoClientes _context;
        public CriarMenuItemNivel2CommandHandler(ContextGestaoClientes context) { _context = context; }

        public async Task<CommandResult> Handle(CriarMenuItemNivel2Command request, CancellationToken cancellationToken)
        {
            var itemN1Existe = await _context.MenusItensNivel1.AnyAsync(m => m.Id == request.MenuItemNivel1Id, cancellationToken);
            if (!itemN1Existe)
                return CommandResult.Falha(new[] { "Item de nível 1 não encontrado." });

            var item = new MenuItemNivel2(request.MenuItemNivel1Id, request.Descricao, request.Icon, request.To, request.Ordem, request.Modulo);
            if (!item.IsValid)
                return CommandResult.Falha(item.Notifications.Select(n => n.Message).Distinct(), "Item de nível 2 inválido.");

            _context.MenusItensNivel2.Add(item);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Item de menu (nível 2) criado com sucesso!", new { item.Id });
        }
    }

    /// <summary>Atualiza item de menu nível 2.</summary>
    public class AtualizarMenuItemNivel2CommandHandler : ICommandHandler<AtualizarMenuItemNivel2Command>
    {
        private readonly ContextGestaoClientes _context;
        public AtualizarMenuItemNivel2CommandHandler(ContextGestaoClientes context) { _context = context; }

        public async Task<CommandResult> Handle(AtualizarMenuItemNivel2Command request, CancellationToken cancellationToken)
        {
            var item = await _context.MenusItensNivel2.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (item == null)
                return CommandResult.Falha(new[] { "Item de nível 2 não encontrado." });

            item.Alterar(request.Descricao, request.Icon, request.To, request.Ordem, request.Modulo);
            if (!item.IsValid)
                return CommandResult.Falha(item.Notifications.Select(n => n.Message).Distinct(), "Item de nível 2 inválido.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Item de menu (nível 2) atualizado com sucesso!", new { item.Id });
        }
    }

    /// <summary>Exclui item de menu nível 2.</summary>
    public class DeletarMenuItemNivel2CommandHandler : ICommandHandler<DeletarMenuItemNivel2Command>
    {
        private readonly ContextGestaoClientes _context;
        public DeletarMenuItemNivel2CommandHandler(ContextGestaoClientes context) { _context = context; }

        public async Task<CommandResult> Handle(DeletarMenuItemNivel2Command request, CancellationToken cancellationToken)
        {
            var item = await _context.MenusItensNivel2.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            if (item == null)
                return CommandResult.Falha(new[] { "Item de nível 2 não encontrado." });

            _context.MenusItensNivel2.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Item de menu (nível 2) removido com sucesso!");
        }
    }
}
