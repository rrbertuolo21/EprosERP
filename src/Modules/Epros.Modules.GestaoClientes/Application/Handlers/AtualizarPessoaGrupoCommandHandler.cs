using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Atualiza Pessoa Grupo.</summary>
    public class AtualizarPessoaGrupoCommandHandler : ICommandHandler<AtualizarPessoaGrupoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarPessoaGrupoCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarPessoaGrupoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var grupo = await _context.PessoaGrupos.FirstOrDefaultAsync(g => g.Id == request.Id && g.TenantId == tenantId, cancellationToken);
            if (grupo == null)
            {
                return CommandResult.Falha(new[] { "Grupo não encontrado" }, "Erro de validação");
            }

            // RN-PEM-093: Descrição já cadastrada
            var existeDescricao = await _context.PessoaGrupos.AnyAsync(g => g.Descricao == request.Descricao && g.Id != grupo.Id && g.TenantId == tenantId, cancellationToken);
            if (existeDescricao)
            {
                return CommandResult.Falha(new[] { "Descrição já cadastrada" }, "Erro de validação");
            }

            grupo.Atualizar(request.Descricao, alteradoPor);

            if (!grupo.IsValid)
            {
                var erros = grupo.Notifications.Select(n => n.Message).Distinct();
                return CommandResult.Falha(erros, "Invariantes de domínio do Grupo de Pessoa não foram atendidas.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Grupo de Pessoa atualizado com sucesso!", new { PessoaGrupoId = grupo.Id });
        }
    }
}
