using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Domain.Entities.Faturamento;
using Epros.Modules.Projetos.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Projetos.Application.Handlers
{
    public class CriarFaturamentoProjetoCommandHandler : ICommandHandler<CriarFaturamentoProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarFaturamentoProjetoCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarFaturamentoProjetoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // RN-FAT-003: codigo unico por tenant.
            var codigoDuplicado = await _context.Faturamentos.AnyAsync(f => f.Codigo == request.Codigo, cancellationToken);
            if (codigoDuplicado)
                return CommandResult.Falha("Ja existe faturamento com este codigo para o tenant.");

            var faturamento = new FaturamentoProjeto(
                request.Codigo,
                request.Descricao,
                request.ProjetoId,
                request.ResponsavelId,
                request.ClienteId,
                request.ModalidadeFaturamento,
                request.Moeda,
                request.DataVencimento,
                tenantId,
                usuario);

            if (!faturamento.IsValid)
                return CommandResult.Falha(faturamento.Notifications.Select(n => n.Message));

            _context.Faturamentos.Add(faturamento);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Faturamento criado com sucesso!", new { faturamento.Id });
        }
    }

    public class AdicionarItemFaturamentoCommandHandler : ICommandHandler<AdicionarItemFaturamentoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public AdicionarItemFaturamentoCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarItemFaturamentoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var faturamento = await _context.Faturamentos
                .Include(f => f.Itens)
                .FirstOrDefaultAsync(f => f.Id == request.FaturamentoProjetoId, cancellationToken);

            if (faturamento == null)
                return CommandResult.Falha("Faturamento nao encontrado.");

            faturamento.AdicionarItem(
                request.Sequencia,
                request.Quantidade,
                request.Observacao,
                request.TipoItem,
                request.ValorUnitario,
                request.ValorTotal,
                request.OrigemTipo,
                request.OrigemId,
                usuario);

            if (!faturamento.IsValid)
                return CommandResult.Falha(faturamento.Notifications.Select(n => n.Message));

            var novoItem = faturamento.Itens.Last();
            _context.Entry(novoItem).State = EntityState.Added;
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Item de faturamento adicionado com sucesso!", new { ItemId = novoItem.Id, faturamento.ValorTotal });
        }
    }

    public class SubmeterFaturamentoProjetoCommandHandler : ICommandHandler<SubmeterFaturamentoProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public SubmeterFaturamentoProjetoCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SubmeterFaturamentoProjetoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var faturamento = await _context.Faturamentos
                .Include(f => f.Itens)
                .FirstOrDefaultAsync(f => f.Id == request.FaturamentoProjetoId, cancellationToken);

            if (faturamento == null)
                return CommandResult.Falha("Faturamento nao encontrado.");

            faturamento.Submeter(usuario);
            if (!faturamento.IsValid)
                return CommandResult.Falha(faturamento.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Faturamento submetido para aprovacao.", new { faturamento.Id });
        }
    }

    public class RejeitarFaturamentoProjetoCommandHandler : ICommandHandler<RejeitarFaturamentoProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public RejeitarFaturamentoProjetoCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RejeitarFaturamentoProjetoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var faturamento = await _context.Faturamentos.FirstOrDefaultAsync(f => f.Id == request.FaturamentoProjetoId, cancellationToken);
            if (faturamento == null)
                return CommandResult.Falha("Faturamento nao encontrado.");

            faturamento.Rejeitar(request.Motivo, usuario);
            if (!faturamento.IsValid)
                return CommandResult.Falha(faturamento.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Faturamento rejeitado e retornado a Rascunho.", new { faturamento.Id });
        }
    }

    /// <summary>
    /// RN-FAT-006/008/009: aprova o faturamento (Ativo) e enfileira evento ProjetoFaturado no Outbox.
    /// O titulo de Contas a Receber e criado no modulo Financeiro (consumidor do evento).
    /// </summary>
    public class AprovarFaturamentoProjetoCommandHandler : ICommandHandler<AprovarFaturamentoProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AprovarFaturamentoProjetoCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AprovarFaturamentoProjetoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var faturamento = await _context.Faturamentos.FirstOrDefaultAsync(f => f.Id == request.FaturamentoProjetoId, cancellationToken);
            if (faturamento == null)
                return CommandResult.Falha("Faturamento nao encontrado.");

            faturamento.Aprovar(usuario);
            if (!faturamento.IsValid)
                return CommandResult.Falha(faturamento.Notifications.Select(n => n.Message));

            if (faturamento.PodePublicarEventoFinanceiro())
            {
                var projeto = await _context.Projetos.FirstOrDefaultAsync(p => p.Id == faturamento.ProjetoId, cancellationToken);
                var payload = new
                {
                    ProjetoId = faturamento.ProjetoId,
                    NomeProjeto = projeto?.Nome ?? faturamento.Descricao,
                    ClienteId = faturamento.ClienteId ?? projeto?.ClienteId ?? System.Guid.Empty,
                    Milestone = faturamento.ValorTotal,
                    ValorFaturamento = faturamento.ValorTotal
                };

                var outbox = new OutboxMessage(tenantId, "ProjetoFaturado", JsonSerializer.Serialize(payload));
                _context.OutboxMessages.Add(outbox);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Faturamento aprovado com sucesso!", new { faturamento.Id, faturamento.ValorTotal });
        }
    }
}
