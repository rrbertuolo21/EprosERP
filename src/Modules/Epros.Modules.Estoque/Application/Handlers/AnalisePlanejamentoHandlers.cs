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
    /// EST-APE — Análise e Planejamento de Estoque. A posição é a própria EstoqueProduto (motor único, D1);
    /// este submódulo parametriza mínimo/máximo/custeio (APE-012), reprocessa alertas de reposição/excesso
    /// (APE-010/011 — gatilho de requisição de compra via evento) e mantém o ciclo de alertas.
    /// </summary>
    public class ParametrizarPosicaoEstoqueCommandHandler : ICommandHandler<ParametrizarPosicaoEstoqueCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ParametrizarPosicaoEstoqueCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ParametrizarPosicaoEstoqueCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var pos = await _context.EstoqueProdutos.FirstOrDefaultAsync(e => e.Id == request.EstoqueProdutoId, cancellationToken);
            if (pos == null) return CommandResult.Falha("Posição de estoque não encontrada.");

            if (request.QuantidadeEstoqueMaximo > 0m && request.QuantidadeEstoqueMinimo > request.QuantidadeEstoqueMaximo)
                return CommandResult.Falha("O estoque mínimo não pode ser maior que o máximo.");

            var antes = $"min={pos.QuantidadeEstoqueMinimo};max={pos.QuantidadeEstoqueMaximo};custeio={pos.TipoCusteioEstoque}";
            // APE-012: NÃO toca saldo/valor/reserva — usa o método de dados autorizados.
            pos.AlterarDadosAutorizados(pos.EmpresaId, pos.ProdutoId, request.QuantidadeEstoqueMinimo, request.QuantidadeEstoqueMaximo, request.TipoCusteioEstoque, usuario);
            var depois = $"min={pos.QuantidadeEstoqueMinimo};max={pos.QuantidadeEstoqueMaximo};custeio={pos.TipoCusteioEstoque}";

            // APE-016 / CA-009: auditoria antes/depois.
            _context.HistoricosEstoque.Add(new HistoricoEstoque("EstoqueProduto", pos.Id, "parametros_alterados", antes, depois, request.Justificativa, usuario, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.AnaliseParametrosAlterados,
                JsonSerializer.Serialize(new { pos.Id, pos.EmpresaId, pos.ProdutoId, antes, depois, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Parâmetros de estoque atualizados.", new { pos.Id });
        }
    }

    public class ReprocessarAlertasEstoqueCommandHandler : ICommandHandler<ReprocessarAlertasEstoqueCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ReprocessarAlertasEstoqueCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ReprocessarAlertasEstoqueCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var posicoesQuery = _context.EstoqueProdutos.AsQueryable();
            if (request.EmpresaId.HasValue)
                posicoesQuery = posicoesQuery.Where(p => p.EmpresaId == request.EmpresaId.Value);
            var posicoes = await posicoesQuery.ToListAsync(cancellationToken);

            // Alertas abertos existentes (para não duplicar).
            var abertos = await _context.AlertasEstoque
                .Where(a => a.StatusAlerta == EStatusAlertaEstoque.Aberto && a.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            int gerados = 0, resolvidos = 0;

            foreach (var pos in posicoes)
            {
                bool temMin = pos.QuantidadeEstoqueMinimo > 0m;
                bool temMax = pos.QuantidadeEstoqueMaximo > 0m;

                // APE-010: reposição quando saldo ≤ mínimo.
                var precisaReposicao = temMin && pos.QuantidadeSaldoEstoque <= pos.QuantidadeEstoqueMinimo;
                // APE-011: excesso quando saldo > máximo.
                var excedeMaximo = temMax && pos.QuantidadeSaldoEstoque > pos.QuantidadeEstoqueMaximo;

                gerados += ProcessarTipo(pos, abertos, precisaReposicao, ETipoAlertaEstoque.Reposicao, pos.QuantidadeEstoqueMinimo, tenantId, usuario,
                    CatalogoEventosIntegracao.Estoque.AnaliseAlertaReposicao, ref resolvidos);
                gerados += ProcessarTipo(pos, abertos, excedeMaximo, ETipoAlertaEstoque.ExcessoMaximo, pos.QuantidadeEstoqueMaximo, tenantId, usuario,
                    CatalogoEventosIntegracao.Estoque.AnaliseExcessoMaximo, ref resolvidos);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok($"Alertas reprocessados: {gerados} gerado(s), {resolvidos} resolvido(s).", new { gerados, resolvidos });
        }

        private int ProcessarTipo(EstoqueProduto pos, System.Collections.Generic.List<AlertaEstoque> abertos, bool condicao,
            ETipoAlertaEstoque tipo, decimal referencia, string tenantId, string usuario, string evento, ref int resolvidos)
        {
            var existente = abertos.FirstOrDefault(a => a.PosicaoEstoqueId == pos.Id && a.TipoAlerta == tipo);
            if (condicao)
            {
                if (existente != null) return 0; // já há alerta aberto
                var alerta = new AlertaEstoque(pos.Id, pos.EmpresaId, pos.ProdutoId, tipo, referencia, pos.QuantidadeSaldoEstoque, tenantId, usuario);
                _context.AlertasEstoque.Add(alerta);
                _context.OutboxMessages.Add(new OutboxMessage(tenantId, evento,
                    JsonSerializer.Serialize(new { pos.EmpresaId, pos.ProdutoId, saldo = pos.QuantidadeSaldoEstoque, referencia, disponivel = pos.QuantidadeSaldoEstoque - pos.QuantidadeEstoqueReservado })));
                return 1;
            }
            // Condição não mais válida → resolve o alerta aberto (auto-resolução).
            if (existente != null)
            {
                existente.Resolver(usuario);
                resolvidos++;
            }
            return 0;
        }
    }

    public class ResolverAlertaEstoqueCommandHandler : ICommandHandler<ResolverAlertaEstoqueCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public ResolverAlertaEstoqueCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ResolverAlertaEstoqueCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var alerta = await _context.AlertasEstoque.FirstOrDefaultAsync(a => a.Id == request.AlertaId && a.DeletadoEm == null, cancellationToken);
            if (alerta == null) return CommandResult.Falha("Alerta não encontrado.");

            if (request.Ignorar) alerta.Ignorar(usuario);
            else alerta.Resolver(usuario);

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Alerta atualizado.", new { alerta.Id, alerta.StatusAlerta });
        }
    }
}
