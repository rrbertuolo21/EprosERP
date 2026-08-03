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
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// EST-INV — Inventário Físico e Contagem Cíclica. Cria documento, registra contagens, aprova
    /// (calculando a acurácia agregada, D14) e aplica o ajuste por diferença via motor único (D1/D3),
    /// preservando a trilha/kardex. Eventos via Outbox (catálogo central).
    /// </summary>
    public class CriarInventarioCommandHandler : ICommandHandler<CriarInventarioCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarInventarioCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarInventarioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (request.Itens == null || request.Itens.Count == 0)
                return CommandResult.Falha("O inventário deve possuir ao menos um item [INV-001].");

            var inventario = new Inventario(request.EmpresaId, request.DataContagem, request.TipoInventario, request.Observacao, tenantId, usuario);
            if (!inventario.IsValid)
                return CommandResult.Falha(inventario.Notifications.Select(n => n.Message), "Dados do inventário são inválidos.");

            _context.Inventarios.Add(inventario);

            foreach (var input in request.Itens)
            {
                // INV-005: fotografa o saldo do sistema quando não informado (posição por empresa+produto do kardex).
                var qtdSistema = input.QuantidadeSistema ?? await _context.EstoqueProdutos
                    .Where(e => e.EmpresaId == request.EmpresaId && e.ProdutoId == input.ProdutoId)
                    .Select(e => (decimal?)e.QuantidadeSaldoEstoque)
                    .FirstOrDefaultAsync(cancellationToken) ?? 0m;

                var item = new InventarioItem(inventario.Id, input.ProdutoId, qtdSistema, input.LocalId, input.Lote, tenantId, usuario);
                if (!item.IsValid)
                    return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item de inventário inválido.");
                _context.InventarioItens.Add(item);
            }

            _context.HistoricosEstoque.Add(new HistoricoEstoque("Inventario", inventario.Id, "inventario_criado", null, ESituacaoInventario.Rascunho.ToString(), request.Observacao, usuario, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.InventarioCriado,
                JsonSerializer.Serialize(new { inventario.Id, inventario.EmpresaId, inventario.DataContagem, itens = request.Itens.Count, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Inventário criado com sucesso!", new { inventario.Id });
        }
    }

    public class IniciarContagemInventarioCommandHandler : ICommandHandler<IniciarContagemInventarioCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public IniciarContagemInventarioCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(IniciarContagemInventarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var inventario = await _context.Inventarios.FirstOrDefaultAsync(i => i.Id == request.Id && i.DeletadoEm == null, cancellationToken);
            if (inventario == null) return CommandResult.Falha("Inventário não encontrado.");

            try { inventario.IniciarContagem(usuario); }
            catch (InvalidOperationException ex) { return CommandResult.Falha(ex.Message); }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Inventário em contagem.", new { inventario.Id, inventario.Situacao });
        }
    }

    public class RegistrarContagemInventarioItemCommandHandler : ICommandHandler<RegistrarContagemInventarioItemCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarContagemInventarioItemCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarContagemInventarioItemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var inventario = await _context.Inventarios.FirstOrDefaultAsync(i => i.Id == request.InventarioId && i.DeletadoEm == null, cancellationToken);
            if (inventario == null) return CommandResult.Falha("Inventário não encontrado.");
            if (!inventario.PodeContar())
                return CommandResult.Falha("A contagem só é permitida em inventário em rascunho/contagem.");

            var item = await _context.InventarioItens.FirstOrDefaultAsync(it => it.Id == request.ItemId && it.InventarioId == request.InventarioId && it.DeletadoEm == null, cancellationToken);
            if (item == null) return CommandResult.Falha("Item de inventário não encontrado.");

            item.RegistrarContagem(request.Contagem01, request.Contagem02, request.Contagem03, request.QuantidadeContada, request.Fechar, usuario);

            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.InventarioItemContado,
                JsonSerializer.Serialize(new { inventario.Id, item.ProdutoId, item.QuantidadeSistema, item.QuantidadeContada })));
            if (item.Divergencia.HasValue)
                _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.InventarioDivergenciaCalculada,
                    JsonSerializer.Serialize(new { inventario.Id, item.ProdutoId, item.Divergencia })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Contagem registrada.", new { item.Id, item.QuantidadeContada, item.Divergencia });
        }
    }

    public class EnviarConferenciaInventarioCommandHandler : ICommandHandler<EnviarConferenciaInventarioCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public EnviarConferenciaInventarioCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(EnviarConferenciaInventarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var inventario = await _context.Inventarios.FirstOrDefaultAsync(i => i.Id == request.Id && i.DeletadoEm == null, cancellationToken);
            if (inventario == null) return CommandResult.Falha("Inventário não encontrado.");

            try { inventario.EnviarParaConferencia(usuario); }
            catch (InvalidOperationException ex) { return CommandResult.Falha(ex.Message); }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Inventário em conferência.", new { inventario.Id, inventario.Situacao });
        }
    }

    public class AprovarInventarioCommandHandler : ICommandHandler<AprovarInventarioCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AprovarInventarioCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AprovarInventarioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var inventario = await _context.Inventarios.FirstOrDefaultAsync(i => i.Id == request.Id && i.DeletadoEm == null, cancellationToken);
            if (inventario == null) return CommandResult.Falha("Inventário não encontrado.");

            var itens = await _context.InventarioItens.Where(it => it.InventarioId == inventario.Id && it.DeletadoEm == null).ToListAsync(cancellationToken);
            var contados = itens.Where(it => it.QuantidadeContada.HasValue).ToList();
            if (contados.Count == 0)
                return CommandResult.Falha("Não há itens contados para aprovar.");

            // [DECIDIDO D14] Acurácia = itens sem divergência / total de itens contados (⚠️ valida-contador).
            var semDivergencia = contados.Count(it => it.SemDivergencia());
            var acuracidade = Math.Round((decimal)semDivergencia / contados.Count, 4);

            try { inventario.Aprovar(acuracidade, usuario); }
            catch (InvalidOperationException ex) { return CommandResult.Falha(ex.Message); }

            _context.HistoricosEstoque.Add(new HistoricoEstoque("Inventario", inventario.Id, "inventario_aprovado", ESituacaoInventario.EmConferencia.ToString(), ESituacaoInventario.Aprovado.ToString(), null, usuario, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.InventarioAprovado,
                JsonSerializer.Serialize(new { inventario.Id, acuracidade, aprovadoPor = usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Inventário aprovado.", new { inventario.Id, acuracidade });
        }
    }

    public class AplicarAjusteInventarioCommandHandler : ICommandHandler<AplicarAjusteInventarioCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AplicarAjusteInventarioCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AplicarAjusteInventarioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var inventario = await _context.Inventarios.FirstOrDefaultAsync(i => i.Id == request.Id && i.DeletadoEm == null, cancellationToken);
            if (inventario == null) return CommandResult.Falha("Inventário não encontrado.");
            if (inventario.Situacao != ESituacaoInventario.Aprovado)
                return CommandResult.Falha("Inventário deve estar aprovado antes de ajustar estoque [CA-006/INV-015].");

            var itens = await _context.InventarioItens
                .Where(it => it.InventarioId == inventario.Id && it.DeletadoEm == null && it.Divergencia != null && it.Divergencia != 0m)
                .ToListAsync(cancellationToken);

            var motor = new MotorMovimentacaoEstoque(_context, tenantId, usuario);
            int ajustes = 0;

            foreach (var item in itens)
            {
                var diferenca = item.Divergencia!.Value; // [D3] contado − sistema

                // Valorização do ajuste = custo médio vigente do produto (kardex). Entrada sobre saldo zero
                // assume o custo desta entrada (D13, tratado no motor); aqui usamos a média corrente.
                var custoMedio = await _context.EstoqueProdutos
                    .Where(e => e.EmpresaId == inventario.EmpresaId && e.ProdutoId == item.ProdutoId)
                    .Select(e => (decimal?)e.ValorCustoMedio)
                    .FirstOrDefaultAsync(cancellationToken) ?? 0m;
                var custeioPadrao = await _context.EstoqueProdutos
                    .Where(e => e.EmpresaId == inventario.EmpresaId && e.ProdutoId == item.ProdutoId)
                    .Select(e => (ETipoCusteioEstoque?)e.TipoCusteioEstoque)
                    .FirstOrDefaultAsync(cancellationToken) ?? ETipoCusteioEstoque.CustoMedio;

                var fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.AjusteEstoque, tenantId, usuario, referenciaExterna: $"Inventário {inventario.Id} item {item.Id}");
                _context.FatosGeradoresEstoque.Add(fato);

                ResultadoMovimentacao resultado = diferenca > 0m
                    ? await motor.AplicarEntradaAsync(inventario.EmpresaId, item.ProdutoId, ETipoEstoque.Geral, diferenca, custoMedio, fato.Id, item.LocalId, item.Lote, null, custeioPadrao, cancellationToken)
                    : await motor.AplicarSaidaAsync(inventario.EmpresaId, item.ProdutoId, Math.Abs(diferenca), fato.Id, item.LocalId, cancellationToken);

                if (!resultado.Sucesso)
                    return CommandResult.Falha(resultado.Erro ?? "Falha ao aplicar ajuste do inventário.");

                _context.InventarioMovimentoAjustes.Add(new InventarioMovimentoAjuste(inventario.Id, item.Id, item.ProdutoId, fato.Id, diferenca, tenantId, usuario));
                _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.InventarioAjusteGerado,
                    JsonSerializer.Serialize(new { inventario.Id, item.ProdutoId, diferenca, fatoId = fato.Id })));
                ajustes++;
            }

            inventario.MarcarAjustado(usuario);
            _context.HistoricosEstoque.Add(new HistoricoEstoque("Inventario", inventario.Id, "inventario_ajustado", ESituacaoInventario.Aprovado.ToString(), ESituacaoInventario.Ajustado.ToString(), $"{ajustes} ajuste(s)", usuario, tenantId, usuario));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok($"Inventário ajustado ({ajustes} item(ns) divergente(s)).", new { inventario.Id, ajustes });
        }
    }

    public class CancelarInventarioCommandHandler : ICommandHandler<CancelarInventarioCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public CancelarInventarioCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CancelarInventarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            if (string.IsNullOrWhiteSpace(request.Motivo))
                return CommandResult.Falha("O motivo do cancelamento é obrigatório.");

            var inventario = await _context.Inventarios.FirstOrDefaultAsync(i => i.Id == request.Id && i.DeletadoEm == null, cancellationToken);
            if (inventario == null) return CommandResult.Falha("Inventário não encontrado.");

            try { inventario.Cancelar(request.Motivo, usuario); }
            catch (InvalidOperationException ex) { return CommandResult.Falha(ex.Message); }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Inventário cancelado.", new { inventario.Id });
        }
    }

    public class CriarReajusteEstoqueCommandHandler : ICommandHandler<CriarReajusteEstoqueCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarReajusteEstoqueCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarReajusteEstoqueCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (request.Itens == null || request.Itens.Count == 0)
                return CommandResult.Falha("O reajuste deve possuir ao menos um item [INV-011].");

            var reajuste = new InventarioReajuste(request.ColaboradorId, request.DataReajuste, request.Porcentagem, request.TipoReajuste, tenantId, usuario);
            if (!reajuste.IsValid)
                return CommandResult.Falha(reajuste.Notifications.Select(n => n.Message), "Dados do reajuste são inválidos.");
            _context.InventarioReajustes.Add(reajuste);

            foreach (var input in request.Itens)
            {
                var item = new InventarioReajusteItem(reajuste.Id, input.ProdutoId, input.ValorOriginal, input.ValorReajuste, tenantId, usuario);
                if (!item.IsValid)
                    return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item de reajuste inválido.");
                _context.InventarioReajusteItens.Add(item);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Reajuste registrado.", new { reajuste.Id });
        }
    }
}
