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
    /// <summary>Cria Funcionalidade (catálogo global, IGlobalEntity).</summary>
    public class CriarFuncionalidadeCommandHandler : ICommandHandler<CriarFuncionalidadeCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        public CriarFuncionalidadeCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser) { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CriarFuncionalidadeCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var duplicado = await _context.Funcionalidades.AnyAsync(f => f.Title == request.Title, cancellationToken);
            if (duplicado)
                return CommandResult.Falha(new[] { "Já existe uma funcionalidade com este título." }, "Erro de validação");

            var func = new Funcionalidade(request.Title, request.Description, userId);
            if (!func.IsValid)
                return CommandResult.Falha(func.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio da Funcionalidade não foram atendidas.");

            _context.Funcionalidades.Add(func);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Funcionalidade criada com sucesso!", new { func.Id });
        }
    }

    /// <summary>Atualiza Funcionalidade.</summary>
    public class AtualizarFuncionalidadeCommandHandler : ICommandHandler<AtualizarFuncionalidadeCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        public AtualizarFuncionalidadeCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser) { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarFuncionalidadeCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var func = await _context.Funcionalidades.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
            if (func == null)
                return CommandResult.Falha(new[] { "Funcionalidade não encontrada." });

            var duplicado = await _context.Funcionalidades.AnyAsync(f => f.Title == request.Title && f.Id != request.Id, cancellationToken);
            if (duplicado)
                return CommandResult.Falha(new[] { "Já existe outra funcionalidade com este título." }, "Erro de validação");

            func.Alterar(request.Title, request.Description, userId);
            if (!func.IsValid)
                return CommandResult.Falha(func.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio da Funcionalidade não foram atendidas.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Funcionalidade atualizada com sucesso!", new { func.Id });
        }
    }

    /// <summary>Exclui (soft) Funcionalidade.</summary>
    public class DeletarFuncionalidadeCommandHandler : ICommandHandler<DeletarFuncionalidadeCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        public DeletarFuncionalidadeCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser) { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DeletarFuncionalidadeCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var func = await _context.Funcionalidades.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
            if (func == null)
                return CommandResult.Falha(new[] { "Funcionalidade não encontrada." });

            func.Deletar(userId);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Funcionalidade removida com sucesso!");
        }
    }

    /// <summary>Cria Add-on (catálogo modular, IGlobalEntity).</summary>
    public class CriarAddOnCommandHandler : ICommandHandler<CriarAddOnCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        public CriarAddOnCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser) { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CriarAddOnCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var duplicado = await _context.AddOns.AnyAsync(a => a.NomeModulo == request.NomeModulo, cancellationToken);
            if (duplicado)
                return CommandResult.Falha(new[] { "Já existe um add-on com este nome de módulo." }, "Erro de validação");

            var addon = new AddOn(request.NomeModulo, request.Alias, request.PrecoMensal, request.PrecoAnual, request.Midia, request.Habilitado, request.Admin, request.ParentAddOnId, userId);
            if (!addon.IsValid)
                return CommandResult.Falha(addon.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio do Add-on não foram atendidas.");

            _context.AddOns.Add(addon);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Add-on criado com sucesso!", new { addon.Id });
        }
    }

    /// <summary>Atualiza Add-on (preço/mídia/dependência). REG-023: invalidar cache é responsabilidade do consumidor.</summary>
    public class AtualizarAddOnCommandHandler : ICommandHandler<AtualizarAddOnCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        public AtualizarAddOnCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser) { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarAddOnCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var addon = await _context.AddOns.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
            if (addon == null)
                return CommandResult.Falha(new[] { "Add-on não encontrado." });

            addon.Alterar(request.Alias, request.PrecoMensal, request.PrecoAnual, request.Midia, request.ParentAddOnId, userId);
            if (!addon.IsValid)
                return CommandResult.Falha(addon.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio do Add-on não foram atendidas.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Add-on atualizado com sucesso!", new { addon.Id });
        }
    }

    /// <summary>Habilita Add-on. REG-024: dependência parent deve estar habilitada.</summary>
    public class HabilitarAddOnCommandHandler : ICommandHandler<HabilitarAddOnCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        public HabilitarAddOnCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser) { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(HabilitarAddOnCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var addon = await _context.AddOns.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
            if (addon == null)
                return CommandResult.Falha(new[] { "Add-on não encontrado." });

            if (addon.ParentAddOnId.HasValue)
            {
                var parentHabilitado = await _context.AddOns.AnyAsync(a => a.Id == addon.ParentAddOnId.Value && a.Habilitado, cancellationToken);
                if (!parentHabilitado)
                    return CommandResult.Falha(new[] { "O módulo dependente (parent) precisa estar habilitado antes." });
            }

            addon.Habilitar(userId);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Add-on habilitado com sucesso!");
        }
    }

    /// <summary>Desabilita Add-on. REG-024: bloqueia se houver filhos habilitados.</summary>
    public class DesabilitarAddOnCommandHandler : ICommandHandler<DesabilitarAddOnCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        public DesabilitarAddOnCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser) { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DesabilitarAddOnCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var addon = await _context.AddOns.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
            if (addon == null)
                return CommandResult.Falha(new[] { "Add-on não encontrado." });

            var temFilhosHabilitados = await _context.AddOns.AnyAsync(a => a.ParentAddOnId == addon.Id && a.Habilitado, cancellationToken);
            if (temFilhosHabilitados)
                return CommandResult.Falha(new[] { "Existem módulos dependentes (child) habilitados; desabilite-os antes." });

            addon.Desabilitar(userId);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Add-on desabilitado com sucesso!");
        }
    }

    /// <summary>Ativa módulo avulso para um usuário/contexto. REG-017: evita duplicidade.</summary>
    public class AtivarModuloUsuarioCommandHandler : ICommandHandler<AtivarModuloUsuarioCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        public AtivarModuloUsuarioCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtivarModuloUsuarioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var jaAtivo = await _context.ModulosAtivosUsuario.AnyAsync(
                m => m.TenantId == tenantId && m.UsuarioId == request.UsuarioId && m.Modulo == request.Modulo, cancellationToken);
            if (jaAtivo)
                return CommandResult.Falha(new[] { "Módulo já ativo para este usuário." }, "Erro de validação");

            var modulo = new ModuloAtivoUsuario(request.UsuarioId, request.Modulo, EOrigemModuloAtivo.Avulso, tenantId, userId);
            if (!modulo.IsValid)
                return CommandResult.Falha(modulo.Notifications.Select(n => n.Message).Distinct(), "Invariantes de domínio do Módulo Ativo não foram atendidas.");

            _context.ModulosAtivosUsuario.Add(modulo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Módulo ativado para o usuário com sucesso!", new { modulo.Id });
        }
    }
}
