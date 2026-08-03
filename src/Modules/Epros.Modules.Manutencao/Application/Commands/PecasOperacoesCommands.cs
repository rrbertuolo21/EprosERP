using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Manutencao.Application.Commands
{
    // ===================================================================================
    // MAN-PEC — D22/D23: reservar / baixar / devolver peca com idempotencia por operacao.
    // A baixa REAL de estoque acontece pelo MOTOR UNICO (Kardex D1) via evento de outbox
    // "OrdemManutencaoConcluida" ja consumido pelo modulo Estoque — nao recria saldo aqui.
    // ===================================================================================

    internal static class PecaOperacaoHelper
    {
        public const string PrefixoOperacao = "op:";
        public static string ChaveOperacao(Guid operacaoId) => PrefixoOperacao + operacaoId.ToString("N");
    }

    // ===== Reservar peca (RN-PEC: ciclo reservar -> consumir) =====
    public record ReservarPecaCommand(Guid ItemId, decimal Quantidade, Guid? LocalEstoqueId, Guid? OperacaoId) : ICommand;

    public class ReservarPecaCommandValidator : AbstractValidator<ReservarPecaCommand>
    {
        public ReservarPecaCommandValidator()
        {
            RuleFor(c => c.ItemId).NotEmpty();
            RuleFor(c => c.Quantidade).GreaterThan(0);
        }
    }

    public class ReservarPecaCommandHandler : ICommandHandler<ReservarPecaCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ReservarPecaCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ReservarPecaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var item = await _context.ItensPecaReposicao.FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);
            if (item == null) return CommandResult.Falha("Item de peca nao encontrado.");
            if (request.Quantidade <= 0) return CommandResult.Falha("A quantidade a reservar deve ser maior que zero.");

            var usuarioGuid = Guid.TryParse(usuario, out var g) ? g : Guid.Empty;
            var reserva = new ReservaPeca(item.Id, item.ProdutoId, request.Quantidade, usuarioGuid, item.GradeId, request.LocalEstoqueId, tenantId, usuario);
            if (!reserva.IsValid) return CommandResult.Falha(reserva.Notifications.Select(n => n.Message));
            reserva.Reservar(usuario);
            item.MarcarReservado(usuario);

            _context.ReservasPeca.Add(reserva);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Peca reservada.", new { reserva.Id, Status = reserva.StatusReserva.ToString() });
        }
    }

    // ===== Baixar peca (consumo) — delta RN-PEC-005, idempotente, move estoque via evento =====
    public record BaixarPecaCommand(Guid ItemId, decimal Quantidade, Guid? OperacaoId, Guid? EquipamentoId) : ICommand;

    public class BaixarPecaCommandValidator : AbstractValidator<BaixarPecaCommand>
    {
        public BaixarPecaCommandValidator()
        {
            RuleFor(c => c.ItemId).NotEmpty();
            RuleFor(c => c.Quantidade).GreaterThan(0);
        }
    }

    public class BaixarPecaCommandHandler : ICommandHandler<BaixarPecaCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public BaixarPecaCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(BaixarPecaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var operacaoId = request.OperacaoId ?? Guid.NewGuid();
            var chave = PecaOperacaoHelper.ChaveOperacao(operacaoId);

            var item = await _context.ItensPecaReposicao.FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);
            if (item == null) return CommandResult.Falha("Item de peca nao encontrado.");

            // Idempotencia: mesma operacao ja processada nao repete baixa (spec integracao secao 9).
            var jaProcessado = await _context.MovimentosPeca.AnyAsync(m =>
                m.ItemId == item.Id && m.TipoMovimento == ETipoMovimentoPeca.Baixa && m.InformacaoMovimento == chave, cancellationToken);
            if (jaProcessado)
                return CommandResult.Ok("Baixa ja processada para esta operacao (idempotente).", new { item.Id, OperacaoId = operacaoId });

            // RN-PEC-005: baixa pelo delta (solicitada - entregue), sem rebaixar o ja entregue.
            var saldo = item.SaldoAConsumir();
            if (saldo <= 0m)
                return CommandResult.Falha("Nao ha saldo a consumir neste item (ja entregue integralmente).");
            var quantidade = Math.Min(request.Quantidade, saldo);

            item.RegistrarEntrega(quantidade, usuario);
            if (!item.IsValid) return CommandResult.Falha(item.Notifications.Select(n => n.Message));

            var movimento = new MovimentoPeca(item.Id, item.ProdutoId, ETipoMovimentoPeca.Baixa, quantidade, chave, item.GradeId, tenantId, usuario);
            if (!movimento.IsValid) return CommandResult.Falha(movimento.Notifications.Select(n => n.Message));
            _context.MovimentosPeca.Add(movimento);

            // Move estoque de verdade pelo MOTOR UNICO (Kardex): evento consumido pelo modulo Estoque.
            // A OrdemManutencaoId do evento e a OPERACAO (chave de idempotencia no Estoque: "Baixa OS {op}").
            var payload = new
            {
                OrdemManutencaoId = operacaoId,
                EquipamentoId = request.EquipamentoId ?? item.OrdemServicoId ?? Guid.Empty,
                Pecas = new[] { new { ProdutoId = item.ProdutoId, Quantidade = quantidade } },
                TenantId = tenantId
            };
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "OrdemManutencaoConcluida", JsonSerializer.Serialize(payload)));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Baixa de peca registrada; estoque sera movimentado pelo motor unico.",
                new { item.Id, MovimentoId = movimento.Id, OperacaoId = operacaoId, Quantidade = quantidade, item.QuantidadeEntregue });
        }
    }

    // ===== Devolver peca (movimento inverso correlacionado) — RN-PEC-007/008 =====
    public record DevolverPecaCommand(Guid ItemId, decimal Quantidade, Guid? OperacaoId) : ICommand;

    public class DevolverPecaCommandValidator : AbstractValidator<DevolverPecaCommand>
    {
        public DevolverPecaCommandValidator()
        {
            RuleFor(c => c.ItemId).NotEmpty();
            RuleFor(c => c.Quantidade).GreaterThan(0);
        }
    }

    public class DevolverPecaCommandHandler : ICommandHandler<DevolverPecaCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DevolverPecaCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DevolverPecaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var operacaoId = request.OperacaoId ?? Guid.NewGuid();
            var chave = PecaOperacaoHelper.ChaveOperacao(operacaoId);

            var item = await _context.ItensPecaReposicao.FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);
            if (item == null) return CommandResult.Falha("Item de peca nao encontrado.");

            var jaProcessado = await _context.MovimentosPeca.AnyAsync(m =>
                m.ItemId == item.Id && m.TipoMovimento == ETipoMovimentoPeca.Devolucao && m.InformacaoMovimento == chave, cancellationToken);
            if (jaProcessado)
                return CommandResult.Ok("Devolucao ja processada para esta operacao (idempotente).", new { item.Id, OperacaoId = operacaoId });

            item.RegistrarDevolucao(request.Quantidade, usuario);
            if (!item.IsValid) return CommandResult.Falha(item.Notifications.Select(n => n.Message));

            var movimento = new MovimentoPeca(item.Id, item.ProdutoId, ETipoMovimentoPeca.Devolucao, request.Quantidade, chave, item.GradeId, tenantId, usuario);
            if (!movimento.IsValid) return CommandResult.Falha(movimento.Notifications.Select(n => n.Message));
            _context.MovimentosPeca.Add(movimento);

            // T5 — estorno SIMETRICO a baixa: devolucao devolve a peca ao estoque real pelo MOTOR UNICO
            // (entrada compensatoria), via evento idempotente pela mesma chave de operacao.
            var payload = new
            {
                OrdemManutencaoId = operacaoId,
                EquipamentoId = item.OrdemServicoId ?? Guid.Empty,
                Pecas = new[] { new { ProdutoId = item.ProdutoId, Quantidade = request.Quantidade } },
                TenantId = tenantId
            };
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Operacoes.DevolucaoPecaManutencao, JsonSerializer.Serialize(payload)));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Devolucao de peca registrada; estoque sera estornado pelo motor unico.",
                new { item.Id, MovimentoId = movimento.Id, OperacaoId = operacaoId, item.QuantidadeEntregue, Status = item.StatusItem.ToString() });
        }
    }
}
