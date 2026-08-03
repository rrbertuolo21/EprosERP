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
    /// Handlers do submódulo Devolução de Compra (CD4 / EF DEVOLUCAO_DE_COMPRA). Gravam em transação única
    /// (tenant pelo filtro global do ContextBase), validam a referência à compra de origem (DEV-001) e a
    /// quantidade devolvida contra a comprada, considerando devoluções anteriores não canceladas (DEV-002).
    /// Rascunho não gera efeito (DEV-003). Confirmação publica, via Outbox: saída de estoque (motor único
    /// D1, DEV-004) + estorno financeiro idempotente por compra (DEV-006). Cancelamento de devolução
    /// confirmada compensa (TEC-C6). CFOP/sentido = valida-contador (NF-06), parametrizado.
    /// </summary>
    internal static class DevolucaoCompraFabrica
    {
        /// <summary>
        /// Monta os itens validando quantidade contra a compra de origem (DEV-002): a soma devolvida por
        /// item (devoluções não canceladas anteriores + a atual) não pode exceder a quantidade comprada.
        /// Retorna null com a falha preenchida em <paramref name="erro"/> quando inválido.
        /// </summary>
        public static bool ValidarQuantidades(
            Compra compra,
            System.Collections.Generic.IEnumerable<DevolucaoCompraItemInput> itens,
            System.Collections.Generic.IReadOnlyCollection<DevolucaoCompraItem> jaDevolvidos,
            out string? erro)
        {
            erro = null;
            foreach (var input in itens)
            {
                if (input.CompraItemOrigemId is null) continue; // sem vínculo: não há como checar DEV-002
                var itemCompra = compra.Itens.FirstOrDefault(ci => ci.Id == input.CompraItemOrigemId.Value);
                if (itemCompra == null)
                {
                    erro = $"Item de origem {input.CompraItemOrigemId} não pertence à compra informada [DEV-002] [Origem: DevolucaoCompra]";
                    return false;
                }
                var anterior = jaDevolvidos.Where(d => d.CompraItemOrigemId == input.CompraItemOrigemId.Value).Sum(d => d.Quantidade);
                if (anterior + input.Quantidade > itemCompra.Quantidade)
                {
                    erro = $"Quantidade devolvida ({anterior + input.Quantidade}) excede a comprada ({itemCompra.Quantidade}) para o item {input.CompraItemOrigemId} [DEV-002] [Origem: DevolucaoCompra]";
                    return false;
                }
            }
            return true;
        }
    }

    public class CriarDevolucaoCompraCommandHandler : ICommandHandler<CriarDevolucaoCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarDevolucaoCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarDevolucaoCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (request.Itens == null || request.Itens.Count == 0)
                return CommandResult.Falha("A devolução deve conter ao menos um item.");

            // DEV-001: a compra de origem deve existir (mesmo tenant, filtro global).
            var compra = await _context.Compras.Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Id == request.CompraOrigemId, cancellationToken);
            if (compra == null)
                return CommandResult.Falha("Compra de origem não encontrada [DEV-001].");

            // DEV-002: soma acumulada com devoluções anteriores não canceladas.
            var jaDevolvidos = await _context.DevolucaoCompraItens.AsNoTracking()
                .Where(di => di.Devolucao!.CompraOrigemId == request.CompraOrigemId
                          && di.Devolucao.Status != EStatusDevolucaoCompra.Cancelada)
                .ToListAsync(cancellationToken);

            if (!DevolucaoCompraFabrica.ValidarQuantidades(compra, request.Itens, jaDevolvidos, out var erroQtd))
                return CommandResult.Falha(erroQtd!, "Quantidade de devolução inválida.");

            var devolucao = new DevolucaoCompra(request.CompraOrigemId, request.FornecedorId, request.Numero,
                request.DataDevolucao, request.Tipo, request.Motivo, request.Cfop, tenantId, usuario);
            if (!devolucao.IsValid)
                return CommandResult.Falha(devolucao.Notifications.Select(n => n.Message), "Dados da devolução inválidos.");

            _context.DevolucoesCompra.Add(devolucao);
            foreach (var input in request.Itens)
            {
                var item = new DevolucaoCompraItem(devolucao.Id, input.CompraItemOrigemId, input.ProdutoId,
                    input.Quantidade, input.ValorUnitario, tenantId, usuario);
                if (!item.IsValid)
                    return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item de devolução inválido.");
                devolucao.AdicionarItem(item);
                _context.DevolucaoCompraItens.Add(item);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Devolução de compra criada em rascunho.", new { devolucao.Id, devolucao.Status, devolucao.Total });
        }
    }

    public class AtualizarDevolucaoCompraCommandHandler : ICommandHandler<AtualizarDevolucaoCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarDevolucaoCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarDevolucaoCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var devolucao = await _context.DevolucoesCompra.Include(d => d.Itens)
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
            if (devolucao == null)
                return CommandResult.Falha("Devolução não encontrada.");

            if (request.Itens == null || request.Itens.Count == 0)
                return CommandResult.Falha("A devolução deve conter ao menos um item.");

            if (!devolucao.Alterar(request.DataDevolucao, request.Tipo, request.Motivo, request.Cfop, usuario))
                return CommandResult.Falha(devolucao.Notifications.Select(n => n.Message), "Não foi possível alterar a devolução.");

            var compra = await _context.Compras.Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Id == devolucao.CompraOrigemId, cancellationToken);
            if (compra == null)
                return CommandResult.Falha("Compra de origem não encontrada [DEV-001].");

            // Ao reescrever os itens, desconsidera os desta própria devolução da soma acumulada (DEV-002).
            var jaDevolvidos = await _context.DevolucaoCompraItens.AsNoTracking()
                .Where(di => di.Devolucao!.CompraOrigemId == devolucao.CompraOrigemId
                          && di.Devolucao.Id != devolucao.Id
                          && di.Devolucao.Status != EStatusDevolucaoCompra.Cancelada)
                .ToListAsync(cancellationToken);
            if (!DevolucaoCompraFabrica.ValidarQuantidades(compra, request.Itens, jaDevolvidos, out var erroQtd))
                return CommandResult.Falha(erroQtd!, "Quantidade de devolução inválida.");

            // Substitui os itens (rascunho): remove os atuais e adiciona os novos.
            _context.DevolucaoCompraItens.RemoveRange(devolucao.Itens);
            devolucao.Itens.Clear();
            foreach (var input in request.Itens)
            {
                var item = new DevolucaoCompraItem(devolucao.Id, input.CompraItemOrigemId, input.ProdutoId,
                    input.Quantidade, input.ValorUnitario, tenantId, usuario);
                if (!item.IsValid)
                    return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item de devolução inválido.");
                devolucao.AdicionarItem(item);
                _context.DevolucaoCompraItens.Add(item);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Devolução atualizada.", new { devolucao.Id, devolucao.Total });
        }
    }

    public class ConfirmarDevolucaoCompraCommandHandler : ICommandHandler<ConfirmarDevolucaoCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConfirmarDevolucaoCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConfirmarDevolucaoCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var devolucao = await _context.DevolucoesCompra.Include(d => d.Itens)
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
            if (devolucao == null)
                return CommandResult.Falha("Devolução não encontrada.");

            if (!devolucao.Confirmar(usuario))
                return CommandResult.Falha(devolucao.Notifications.Select(n => n.Message), "Não foi possível confirmar a devolução.");

            // DEV-004: saída de estoque por evento (motor único D1). Sentido/CFOP = valida-contador (NF-06).
            var itensPayload = devolucao.Itens.Select(i => new { i.ProdutoId, i.CompraItemOrigemId, i.Quantidade, i.ValorUnitario, i.ValorTotal }).ToList();
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Compras.DevolucaoCompraConfirmada,
                JsonSerializer.Serialize(new { devolucao.Id, devolucao.CompraOrigemId, devolucao.FornecedorId, devolucao.Numero, devolucao.Total, usuario })));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Compras.DevolucaoCompraSaidaEstoque,
                JsonSerializer.Serialize(new { devolucaoId = devolucao.Id, devolucao.CompraOrigemId, sentido = "saida", devolucao.Cfop, itens = itensPayload })));
            // DEV-006: estorno financeiro idempotente por devolução (fato gerador único). Reduz o passivo,
            // não apaga o original (compensação TEC-C6). Consumidor dedupe por devolucaoId.
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Compras.DevolucaoCompraEstornoFinanceiro,
                JsonSerializer.Serialize(new { devolucaoId = devolucao.Id, devolucao.CompraOrigemId, devolucao.FornecedorId, valor = devolucao.Total, idempotencia = $"dev-compra:{devolucao.Id}" })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Devolução confirmada — saída de estoque e estorno financeiro publicados.", new { devolucao.Id, devolucao.Status });
        }
    }

    public class CancelarDevolucaoCompraCommandHandler : ICommandHandler<CancelarDevolucaoCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CancelarDevolucaoCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CancelarDevolucaoCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var devolucao = await _context.DevolucoesCompra.Include(d => d.Itens)
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
            if (devolucao == null)
                return CommandResult.Falha("Devolução não encontrada.");

            var eraConfirmada = devolucao.EstaConfirmada();
            if (!devolucao.Cancelar(usuario))
                return CommandResult.Falha(devolucao.Notifications.Select(n => n.Message), "Não foi possível cancelar a devolução.");

            // Só compensa se havia efeito (devolução confirmada). Rascunho cancelado não gerou efeito (DEV-003).
            if (eraConfirmada)
                _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Compras.DevolucaoCompraCancelada,
                    JsonSerializer.Serialize(new { devolucaoId = devolucao.Id, devolucao.CompraOrigemId, compensacao = true, idempotencia = $"dev-compra-cancel:{devolucao.Id}", usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok(eraConfirmada ? "Devolução cancelada — efeitos compensados." : "Devolução (rascunho) cancelada.", new { devolucao.Id, devolucao.Status });
        }
    }

    public class ExcluirDevolucaoCompraCommandHandler : ICommandHandler<ExcluirDevolucaoCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public ExcluirDevolucaoCompraCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExcluirDevolucaoCompraCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var devolucao = await _context.DevolucoesCompra
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
            if (devolucao == null)
                return CommandResult.Falha("Devolução não encontrada.");

            if (devolucao.EstaConfirmada())
                return CommandResult.Falha("Devolução confirmada não pode ser excluída; cancele-a para compensar os efeitos [DEV-014].");

            devolucao.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Devolução excluída.");
        }
    }
}
