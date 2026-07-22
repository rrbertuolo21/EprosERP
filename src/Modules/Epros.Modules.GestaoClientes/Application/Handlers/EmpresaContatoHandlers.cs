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
    // ---------- EmpresaContato ----------
    /// <summary>Handler de aplicação (AdicionarEmpresaContatoCommandHandler).</summary>
    public class AdicionarEmpresaContatoCommandHandler : ICommandHandler<AdicionarEmpresaContatoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarEmpresaContatoCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarEmpresaContatoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == request.EmpresaId && e.TenantId == tenantId, cancellationToken);
            if (empresa == null)
                return CommandResult.Falha(new[] { "Empresa não encontrada." }, "Erro");

            empresa.AdicionaContato(request.Nome, request.Email, request.Tipo, request.Numero, userId);
            if (!empresa.IsValid)
                return CommandResult.Falha(empresa.Notifications.Select(n => n.Message).Distinct(), "Falha na validação do contato");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Contato adicionado com sucesso!");
        }
    }

    /// <summary>Atualiza Empresa Contato.</summary>
    public class AtualizarEmpresaContatoCommandHandler : ICommandHandler<AtualizarEmpresaContatoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarEmpresaContatoCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarEmpresaContatoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var empresa = await _context.Empresas.Include(e => e.Contatos).FirstOrDefaultAsync(e => e.Id == request.EmpresaId && e.TenantId == tenantId, cancellationToken);
            if (empresa == null)
                return CommandResult.Falha(new[] { "Empresa não encontrada." }, "Erro");

            empresa.AlterarContato(request.ContatoId, request.Nome, request.Email, request.Tipo, request.Numero, userId);
            if (!empresa.IsValid)
                return CommandResult.Falha(empresa.Notifications.Select(n => n.Message).Distinct(), "Falha na validação do contato");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Contato atualizado com sucesso!");
        }
    }

    /// <summary>Exclui Empresa Contato.</summary>
    public class DeletarEmpresaContatoCommandHandler : ICommandHandler<DeletarEmpresaContatoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarEmpresaContatoCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarEmpresaContatoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var empresa = await _context.Empresas.Include(e => e.Contatos).FirstOrDefaultAsync(e => e.Id == request.EmpresaId && e.TenantId == tenantId, cancellationToken);
            if (empresa == null)
                return CommandResult.Falha(new[] { "Empresa não encontrada." }, "Erro");

            empresa.DeletarContato(request.ContatoId, userId);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Contato removido com sucesso!");
        }
    }

    // ---------- IeSt ----------
    /// <summary>Handler de aplicação (AdicionarIeStCommandHandler).</summary>
    public class AdicionarIeStCommandHandler : ICommandHandler<AdicionarIeStCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarIeStCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarIeStCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == request.EmpresaId && e.TenantId == tenantId, cancellationToken);
            if (empresa == null)
                return CommandResult.Falha(new[] { "Empresa não encontrada." }, "Erro");

            empresa.AdicionaIeSt(request.Uf, request.Ie, userId);
            if (!empresa.IsValid)
                return CommandResult.Falha(empresa.Notifications.Select(n => n.Message).Distinct(), "Falha na validação do IE ST");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("IE ST adicionado com sucesso!");
        }
    }

    /// <summary>Atualiza Ie St.</summary>
    public class AtualizarIeStCommandHandler : ICommandHandler<AtualizarIeStCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarIeStCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarIeStCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var empresa = await _context.Empresas.Include(e => e.IeSts).FirstOrDefaultAsync(e => e.Id == request.EmpresaId && e.TenantId == tenantId, cancellationToken);
            if (empresa == null)
                return CommandResult.Falha(new[] { "Empresa não encontrada." }, "Erro");

            empresa.AlterarIeSt(request.IeStId, request.Uf, request.Ie, userId);
            if (!empresa.IsValid)
                return CommandResult.Falha(empresa.Notifications.Select(n => n.Message).Distinct(), "Falha na validação do IE ST");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("IE ST atualizado com sucesso!");
        }
    }

    /// <summary>Exclui Ie St.</summary>
    public class DeletarIeStCommandHandler : ICommandHandler<DeletarIeStCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarIeStCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarIeStCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var empresa = await _context.Empresas.Include(e => e.IeSts).FirstOrDefaultAsync(e => e.Id == request.EmpresaId && e.TenantId == tenantId, cancellationToken);
            if (empresa == null)
                return CommandResult.Falha(new[] { "Empresa não encontrada." }, "Erro");

            empresa.DeletarIeSt(request.IeStId, userId);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("IE ST removido com sucesso!");
        }
    }

    // ---------- Queries ----------
    /// <summary>Lista Empresa Contatos.</summary>
    public class ListarEmpresaContatosQueryHandler : IQueryHandler<ListarEmpresaContatosQuery, System.Collections.Generic.List<EmpresaContatoDto>>
    {
        private readonly ContextGestaoClientes _context;

        public ListarEmpresaContatosQueryHandler(ContextGestaoClientes context) => _context = context;

        public async Task<System.Collections.Generic.List<EmpresaContatoDto>> Handle(ListarEmpresaContatosQuery request, CancellationToken cancellationToken)
        {
            return await _context.EmpresasContatos.AsNoTracking()
                .Where(c => c.EmpresaId == request.EmpresaId)
                .OrderBy(c => c.Nome)
                .Select(c => new EmpresaContatoDto
                {
                    Id = c.Id,
                    EmpresaId = c.EmpresaId,
                    Nome = c.Nome,
                    Email = c.Email,
                    TipoTelefone = c.TipoTelefone,
                    Telefone = c.Telefone
                })
                .ToListAsync(cancellationToken);
        }
    }

    /// <summary>Lista Ie Sts.</summary>
    public class ListarIeStsQueryHandler : IQueryHandler<ListarIeStsQuery, System.Collections.Generic.List<IeStDto>>
    {
        private readonly ContextGestaoClientes _context;

        public ListarIeStsQueryHandler(ContextGestaoClientes context) => _context = context;

        public async Task<System.Collections.Generic.List<IeStDto>> Handle(ListarIeStsQuery request, CancellationToken cancellationToken)
        {
            return await _context.IeSts.AsNoTracking()
                .Where(i => i.EmpresaId == request.EmpresaId)
                .OrderBy(i => i.Uf)
                .Select(i => new IeStDto
                {
                    Id = i.Id,
                    EmpresaId = i.EmpresaId,
                    Uf = i.Uf,
                    Ie = i.Ie
                })
                .ToListAsync(cancellationToken);
        }
    }
}
