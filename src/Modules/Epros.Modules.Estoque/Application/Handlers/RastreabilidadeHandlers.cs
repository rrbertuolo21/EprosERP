using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// EST-RLT — Rastreabilidade de Lote e Serialização. Cria lotes/seriais, bloqueia/desbloqueia e conduz
    /// recall. [D10] controle configurável por produto; [D11] FEFO SUGERE (na query), não trava aqui.
    /// Auditoria reutiliza HistoricoEstoque (não duplica rlt_historico); eventos via Outbox (catálogo).
    /// </summary>
    public class CriarLoteCommandHandler : ICommandHandler<CriarLoteCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarLoteCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarLoteCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var lote = new LoteEstoque(request.EmpresaId, request.ProdutoId, request.CodigoLote, request.QuantidadeRecebida,
                request.LocalId, request.FichaEntradaId, request.DataFabricacao, request.DataValidade, request.Origem, request.Observacao, tenantId, usuario);
            if (!lote.IsValid)
                return CommandResult.Falha(lote.Notifications.Select(n => n.Message), "Dados do lote são inválidos.");

            _context.LotesEstoque.Add(lote);
            _context.HistoricosEstoque.Add(new HistoricoEstoque("LoteEstoque", lote.Id, "lote_criado", null, EStatusLoteRastreabilidade.Ativo.ToString(), request.Observacao, usuario, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.LoteCriado,
                JsonSerializer.Serialize(new { lote.Id, lote.ProdutoId, lote.CodigoLote, lote.DataValidade, lote.QuantidadeRecebida })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Lote criado com sucesso!", new { lote.Id });
        }
    }

    public class RegistrarNumeroSerialCommandHandler : ICommandHandler<RegistrarNumeroSerialCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarNumeroSerialCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarNumeroSerialCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // RLT-005: serial não pode duplicar no escopo produto/tenant.
            var duplicado = await _context.NumerosSeriais.AsNoTracking()
                .AnyAsync(s => s.ProdutoId == request.ProdutoId && s.Numero == request.Numero && s.DeletadoEm == null, cancellationToken);
            if (duplicado)
                return CommandResult.Falha("Número serial já existe para o produto [RLT-005].");

            var serial = new NumeroSerial(request.ProdutoId, request.Numero, request.LoteId, request.FichaEntradaId, tenantId, usuario);
            if (!serial.IsValid)
                return CommandResult.Falha(serial.Notifications.Select(n => n.Message), "Dados do serial são inválidos.");

            _context.NumerosSeriais.Add(serial);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.SerialRegistrado,
                JsonSerializer.Serialize(new { serial.Id, serial.ProdutoId, serial.Numero, serial.LoteId })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Serial registrado.", new { serial.Id });
        }
    }

    public class BloquearLoteCommandHandler : ICommandHandler<BloquearLoteCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public BloquearLoteCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(BloquearLoteCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (string.IsNullOrWhiteSpace(request.Motivo))
                return CommandResult.Falha("O motivo do bloqueio é obrigatório [RLT-012].");

            var lote = await _context.LotesEstoque.FirstOrDefaultAsync(l => l.Id == request.LoteId && l.DeletadoEm == null, cancellationToken);
            if (lote == null) return CommandResult.Falha("Lote não encontrado.");

            var bloqueio = new BloqueioLote(lote.Id, request.TipoBloqueio, request.Motivo, tenantId, usuario);
            if (!bloqueio.IsValid)
                return CommandResult.Falha(bloqueio.Notifications.Select(n => n.Message), "Dados do bloqueio são inválidos.");

            try { lote.Bloquear(request.Quantidade, usuario); }
            catch (InvalidOperationException ex) { return CommandResult.Falha(ex.Message); }

            _context.BloqueiosLote.Add(bloqueio);
            _context.HistoricosEstoque.Add(new HistoricoEstoque("LoteEstoque", lote.Id, "lote_bloqueado", null, EStatusLoteRastreabilidade.Bloqueado.ToString(), request.Motivo, usuario, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.LoteBloqueado,
                JsonSerializer.Serialize(new { lote.Id, request.TipoBloqueio, request.Motivo, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Lote bloqueado.", new { lote.Id, bloqueioId = bloqueio.Id });
        }
    }

    public class DesbloquearLoteCommandHandler : ICommandHandler<DesbloquearLoteCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DesbloquearLoteCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DesbloquearLoteCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var lote = await _context.LotesEstoque.FirstOrDefaultAsync(l => l.Id == request.LoteId && l.DeletadoEm == null, cancellationToken);
            if (lote == null) return CommandResult.Falha("Lote não encontrado.");

            var bloqueiosAtivos = await _context.BloqueiosLote
                .Where(b => b.LoteId == lote.Id && b.DataDesbloqueio == null && b.DeletadoEm == null)
                .ToListAsync(cancellationToken);
            foreach (var b in bloqueiosAtivos)
                b.RegistrarDesbloqueio(request.MotivoDesbloqueio, usuario);

            lote.Desbloquear(usuario);
            _context.HistoricosEstoque.Add(new HistoricoEstoque("LoteEstoque", lote.Id, "lote_desbloqueado", EStatusLoteRastreabilidade.Bloqueado.ToString(), lote.Status.ToString(), request.MotivoDesbloqueio, usuario, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.LoteDesbloqueado,
                JsonSerializer.Serialize(new { lote.Id, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Lote desbloqueado.", new { lote.Id, lote.Status });
        }
    }

    public class AbrirRecallLoteCommandHandler : ICommandHandler<AbrirRecallLoteCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AbrirRecallLoteCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AbrirRecallLoteCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (string.IsNullOrWhiteSpace(request.Motivo))
                return CommandResult.Falha("O motivo do recall é obrigatório [RLT-011].");

            var lote = await _context.LotesEstoque.FirstOrDefaultAsync(l => l.Id == request.LoteId && l.DeletadoEm == null, cancellationToken);
            if (lote == null) return CommandResult.Falha("Lote não encontrado.");

            var recall = new RecallLote(lote.Id, request.CodigoRecall, request.Motivo, tenantId, usuario);
            if (!recall.IsValid)
                return CommandResult.Falha(recall.Notifications.Select(n => n.Message), "Dados do recall são inválidos.");
            _context.RecallsLote.Add(recall);

            // RLT-008: lote em recall deve ser bloqueado para consumo.
            if (lote.Status == EStatusLoteRastreabilidade.Ativo)
            {
                lote.Bloquear(null, usuario);
                _context.BloqueiosLote.Add(new BloqueioLote(lote.Id, ETipoBloqueioLote.Recall, $"Recall: {request.Motivo}", tenantId, usuario));
            }

            _context.HistoricosEstoque.Add(new HistoricoEstoque("LoteEstoque", lote.Id, "recall_aberto", null, EStatusRecallLote.Aberto.ToString(), request.Motivo, usuario, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.RecallAberto,
                JsonSerializer.Serialize(new { recallId = recall.Id, loteId = lote.Id, lote.ProdutoId, lote.CodigoLote, request.Motivo })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Recall aberto e lote bloqueado.", new { recallId = recall.Id, lote.Id });
        }
    }

    public class ConcluirRecallLoteCommandHandler : ICommandHandler<ConcluirRecallLoteCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConcluirRecallLoteCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConcluirRecallLoteCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var recall = await _context.RecallsLote.FirstOrDefaultAsync(r => r.Id == request.RecallId && r.DeletadoEm == null, cancellationToken);
            if (recall == null) return CommandResult.Falha("Recall não encontrado.");

            recall.Concluir(usuario);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.RecallEncerrado,
                JsonSerializer.Serialize(new { recall.Id, recall.LoteId, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Recall concluído.", new { recall.Id });
        }
    }
}
