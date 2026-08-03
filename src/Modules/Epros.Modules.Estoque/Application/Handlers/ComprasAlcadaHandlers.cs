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
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Handlers da alçada de aprovação de compras (CD3 / EF SOURCING §5.8, §6.2). Montam a instância de
    /// aprovação a partir das regras vigentes, gravam em transação única (tenant pelo filtro global) e
    /// publicam eventos de workflow via Outbox (AprovacaoSolicitada / AprovacaoConcluida).
    /// </summary>
    public class CriarComprasAlcadaRegraCommandHandler : ICommandHandler<CriarComprasAlcadaRegraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarComprasAlcadaRegraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarComprasAlcadaRegraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var regra = new ComprasAlcadaRegra(request.Nivel, request.ValorMinimo, request.ValorMaximo,
                request.CompradorId, request.CategoriaCompra, request.AprovadorId, request.PapelAprovador,
                request.Ativo, tenantId, usuario);
            if (!regra.IsValid)
                return CommandResult.Falha(regra.Notifications.Select(n => n.Message), "Regra de alçada inválida.");

            _context.ComprasAlcadaRegras.Add(regra);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Regra de alçada criada com sucesso!", new { regra.Id });
        }
    }

    public class AtualizarComprasAlcadaRegraCommandHandler : ICommandHandler<AtualizarComprasAlcadaRegraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarComprasAlcadaRegraCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarComprasAlcadaRegraCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var regra = await _context.ComprasAlcadaRegras.FirstOrDefaultAsync(r => r.Id == request.Id && r.DeletadoEm == null, cancellationToken);
            if (regra == null)
                return CommandResult.Falha("Regra de alçada não encontrada.");

            regra.Alterar(request.Nivel, request.ValorMinimo, request.ValorMaximo, request.CompradorId,
                request.CategoriaCompra, request.AprovadorId, request.PapelAprovador, request.Ativo, usuario);
            if (!regra.IsValid)
                return CommandResult.Falha(regra.Notifications.Select(n => n.Message), "Regra de alçada inválida.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Regra de alçada atualizada com sucesso!");
        }
    }

    public class ExcluirComprasAlcadaRegraCommandHandler : ICommandHandler<ExcluirComprasAlcadaRegraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public ExcluirComprasAlcadaRegraCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExcluirComprasAlcadaRegraCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var regra = await _context.ComprasAlcadaRegras.FirstOrDefaultAsync(r => r.Id == request.Id && r.DeletadoEm == null, cancellationToken);
            if (regra == null)
                return CommandResult.Falha("Regra de alçada não encontrada.");

            regra.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Regra de alçada excluída com sucesso!");
        }
    }

    public class SolicitarAprovacaoCompraCommandHandler : ICommandHandler<SolicitarAprovacaoCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public SolicitarAprovacaoCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SolicitarAprovacaoCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var pedido = new ComprasPedidoAprovacao(request.OrigemTipo, request.OrigemId, request.ValorTotal,
                request.CompradorId, request.CategoriaCompra, tenantId, usuario);
            if (!pedido.IsValid)
                return CommandResult.Falha(pedido.Notifications.Select(n => n.Message), "Dados da aprovação inválidos.");

            // §6.2 passo 2: determina os níveis EXIGIDOS por valor + comprador + categoria. Alçada é
            // cumulativa (CD3): o pedido escala por todos os níveis cujo piso foi ultrapassado.
            var regras = await _context.ComprasAlcadaRegras.AsNoTracking()
                .Where(r => r.DeletadoEm == null && r.Ativo)
                .ToListAsync(cancellationToken);

            var aplicaveis = regras
                .Where(r => r.ExigidaEmEscala(request.ValorTotal, request.CompradorId, request.CategoriaCompra))
                .OrderBy(r => r.Nivel)
                .ToList();

            _context.ComprasPedidosAprovacao.Add(pedido);
            foreach (var r in aplicaveis)
            {
                var nivel = new ComprasPedidoAprovacaoNivel(pedido.Id, r.Nivel, r.AprovadorId, r.PapelAprovador,
                    r.ValorMinimo, r.ValorMaximo, tenantId, usuario);
                pedido.AdicionarNivel(nivel);
                _context.ComprasPedidosAprovacaoNiveis.Add(nivel);
            }
            pedido.FinalizarMontagem(usuario);

            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Workflow.AprovacaoSolicitada,
                JsonSerializer.Serialize(new { pedido.Id, pedido.OrigemTipo, pedido.OrigemId, pedido.ValorTotal, niveis = pedido.QuantidadeNiveis, pedido.Status, usuario })));

            // Sem nível aplicável, já nasce aprovado (§6.2): publica conclusão para desbloquear a origem.
            if (pedido.FoiAprovado())
                _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Workflow.AprovacaoConcluida,
                    JsonSerializer.Serialize(new { pedido.Id, pedido.OrigemTipo, pedido.OrigemId, aprovado = true, semAlcada = true })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Solicitação de aprovação registrada.", new { pedido.Id, pedido.Status, pedido.NivelAtual, pedido.QuantidadeNiveis });
        }
    }

    public class AprovarNivelAprovacaoCompraCommandHandler : ICommandHandler<AprovarNivelAprovacaoCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AprovarNivelAprovacaoCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AprovarNivelAprovacaoCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var pedido = await _context.ComprasPedidosAprovacao.Include(p => p.Niveis)
                .FirstOrDefaultAsync(p => p.Id == request.PedidoAprovacaoId && p.DeletadoEm == null, cancellationToken);
            if (pedido == null)
                return CommandResult.Falha("Pedido de aprovação não encontrado.");

            if (!pedido.Aprovar(usuario, request.Justificativa))
                return CommandResult.Falha(pedido.Notifications.Select(n => n.Message), "Não foi possível aprovar o nível.");

            if (pedido.FoiAprovado())
                _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Workflow.AprovacaoConcluida,
                    JsonSerializer.Serialize(new { pedido.Id, pedido.OrigemTipo, pedido.OrigemId, aprovado = true, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok(pedido.FoiAprovado() ? "Pedido aprovado em todos os níveis!" : "Nível aprovado; aguardando próximo nível.",
                new { pedido.Id, pedido.Status, pedido.NivelAtual });
        }
    }

    public class ReprovarNivelAprovacaoCompraCommandHandler : ICommandHandler<ReprovarNivelAprovacaoCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ReprovarNivelAprovacaoCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ReprovarNivelAprovacaoCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var pedido = await _context.ComprasPedidosAprovacao.Include(p => p.Niveis)
                .FirstOrDefaultAsync(p => p.Id == request.PedidoAprovacaoId && p.DeletadoEm == null, cancellationToken);
            if (pedido == null)
                return CommandResult.Falha("Pedido de aprovação não encontrado.");

            if (!pedido.Reprovar(usuario, request.Justificativa))
                return CommandResult.Falha(pedido.Notifications.Select(n => n.Message), "Não foi possível reprovar o nível.");

            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Workflow.AprovacaoConcluida,
                JsonSerializer.Serialize(new { pedido.Id, pedido.OrigemTipo, pedido.OrigemId, aprovado = false, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Pedido reprovado — bloqueado (SRC-008).", new { pedido.Id, pedido.Status });
        }
    }

    public class CancelarAprovacaoCompraCommandHandler : ICommandHandler<CancelarAprovacaoCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public CancelarAprovacaoCompraCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CancelarAprovacaoCompraCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var pedido = await _context.ComprasPedidosAprovacao.Include(p => p.Niveis)
                .FirstOrDefaultAsync(p => p.Id == request.PedidoAprovacaoId && p.DeletadoEm == null, cancellationToken);
            if (pedido == null)
                return CommandResult.Falha("Pedido de aprovação não encontrado.");

            if (!pedido.Cancelar(usuario))
                return CommandResult.Falha(pedido.Notifications.Select(n => n.Message), "Não foi possível cancelar.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Pedido de aprovação cancelado.", new { pedido.Id, pedido.Status });
        }
    }
}
