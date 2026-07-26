using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Handlers
{
    public class CriarCrmTicketStatusCommandHandler : ICommandHandler<CriarCrmTicketStatusCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCrmTicketStatusCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCrmTicketStatusCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var status = new CrmTicketStatus(request.Titulo, request.Cor, request.Ordem, request.UsoClienteRespondeu, request.UsoEquipeRespondeu, request.PadraoSistema, tenantId, usuario);
            if (!status.IsValid) return CommandResult.Falha(status.Notifications.Select(n => n.Message), "Dados do status inválidos.");
            _context.CrmTicketStatus.Add(status);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Status de ticket criado.", new { status.Id });
        }
    }

    public class CriarCrmTicketCommandHandler : ICommandHandler<CriarCrmTicketCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCrmTicketCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCrmTicketCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var ticket = new CrmTicket(
                null, request.Titulo, request.Descricao, request.StatusId, request.Prioridade, request.CategoriaId,
                request.ClienteId, request.ProjetoId, request.Origem, request.TipoUsuarioAbertura,
                request.UsuarioAberturaId, request.ContatoAberturaId, tenantId, usuario);
            if (!ticket.IsValid) return CommandResult.Falha(ticket.Notifications.Select(n => n.Message), "Dados do ticket inválidos.");
            _context.CrmTickets.Add(ticket);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Ticket aberto com sucesso!", new { ticket.Id });
        }
    }

    public class ResponderCrmTicketCommandHandler : ICommandHandler<ResponderCrmTicketCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ResponderCrmTicketCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ResponderCrmTicketCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ticket = await _context.CrmTickets.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == request.TicketId, cancellationToken);
            if (ticket == null) return CommandResult.Falha("Ticket não encontrado.");

            var resposta = new CrmTicketResposta(
                request.TicketId, request.Texto, request.Tipo, request.Origem,
                request.RemetenteTipo, request.RemetenteId, request.AnexosJson, tenantId, usuario);
            if (!resposta.IsValid) return CommandResult.Falha(resposta.Notifications.Select(n => n.Message), "Resposta inválida.");

            _context.CrmTicketRespostas.Add(resposta);
            // Atualiza a timeline do ticket (última movimentação).
            ticket.MudarStatus(ticket.StatusId, usuario);
            await _context.SaveChangesAsync(cancellationToken);

            // CRM-063: nota interna não dispara comunicação externa.
            return CommandResult.Ok(resposta.EhNotaInterna() ? "Nota interna registrada." : "Resposta registrada.", new { RespostaId = resposta.Id, TicketId = ticket.Id });
        }
    }
}
