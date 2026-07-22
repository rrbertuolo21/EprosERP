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
    /// <summary>Cria Pessoa Grupo.</summary>
    public class CriarPessoaGrupoCommandHandler : ICommandHandler<CriarPessoaGrupoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPessoaGrupoCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPessoaGrupoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            // RN-PEM-093: Descrição já cadastrada
            var existeDescricao = await _context.PessoaGrupos.AnyAsync(g => g.Descricao == request.Descricao && g.TenantId == tenantId, cancellationToken);
            if (existeDescricao)
            {
                return CommandResult.Falha(new[] { "Descrição já cadastrada" }, "Erro de validação");
            }

            var grupo = new PessoaGrupo(request.Descricao, tenantId, criadoPor);

            if (!grupo.IsValid)
            {
                var erros = grupo.Notifications.Select(n => n.Message).Distinct();
                return CommandResult.Falha(erros, "Invariantes de domínio do Grupo de Pessoa não foram atendidas.");
            }

            _context.PessoaGrupos.Add(grupo);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Grupo de Pessoa cadastrado com sucesso!", new { PessoaGrupoId = grupo.Id });
        }
    }
}
