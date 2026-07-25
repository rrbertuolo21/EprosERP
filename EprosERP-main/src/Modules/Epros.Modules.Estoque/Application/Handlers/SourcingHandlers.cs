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
    /// Handlers do bloco de sourcing (EST-SC-001 / COM-GC-001): tipos, requisição, cotação e pedido.
    /// Todas as operações gravam em transação única e respeitam tenant (filtro global do ContextBase).
    /// </summary>
    public class CriarScTipoRequisicaoCommandHandler : ICommandHandler<CriarScTipoRequisicaoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarScTipoRequisicaoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarScTipoRequisicaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var tipo = new ScTipoRequisicao(request.Descricao, tenantId, usuario);
            if (!tipo.IsValid)
                return CommandResult.Falha(tipo.Notifications.Select(n => n.Message), "Dados do tipo de requisição são inválidos.");

            _context.ScTiposRequisicao.Add(tipo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Tipo de requisição criado com sucesso!", new { tipo.Id });
        }
    }

    public class CriarScTipoPedidoCommandHandler : ICommandHandler<CriarScTipoPedidoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarScTipoPedidoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarScTipoPedidoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var tipo = new ScTipoPedido(request.Descricao, tenantId, usuario);
            if (!tipo.IsValid)
                return CommandResult.Falha(tipo.Notifications.Select(n => n.Message), "Dados do tipo de pedido são inválidos.");

            _context.ScTiposPedido.Add(tipo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Tipo de pedido criado com sucesso!", new { tipo.Id });
        }
    }

    public class CriarScRequisicaoCommandHandler : ICommandHandler<CriarScRequisicaoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarScRequisicaoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarScRequisicaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var requisicao = new ScRequisicao(request.TipoRequisicaoId, request.ColaboradorId, request.DataRequisicao, tenantId, usuario);
            if (!requisicao.IsValid)
                return CommandResult.Falha(requisicao.Notifications.Select(n => n.Message), "Dados da requisição são inválidos.");

            _context.ScRequisicoes.Add(requisicao);

            if (request.Itens != null)
            {
                foreach (var input in request.Itens)
                {
                    var item = new ScRequisicaoItem(requisicao.Id, input.ProdutoId, input.Quantidade, input.QuantidadeCotada, input.ItemCotado, tenantId, usuario);
                    if (!item.IsValid)
                        return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item de requisição inválido.");
                    _context.ScRequisicaoItens.Add(item);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Requisição de compra criada com sucesso!", new { requisicao.Id });
        }
    }

    public class ExcluirScRequisicaoCommandHandler : ICommandHandler<ExcluirScRequisicaoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public ExcluirScRequisicaoCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExcluirScRequisicaoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var requisicao = await _context.ScRequisicoes
                .Include(r => r.Itens)
                .FirstOrDefaultAsync(r => r.Id == request.Id && r.DeletadoEm == null, cancellationToken);
            if (requisicao == null)
                return CommandResult.Falha("Requisição não encontrada.");

            // SC-056: exclusão de requisição exclui seus itens.
            foreach (var item in requisicao.Itens)
                item.Deletar(usuario);
            requisicao.Deletar(usuario);

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Requisição excluída com sucesso.", new { requisicao.Id });
        }
    }

    public class CriarScCotacaoCommandHandler : ICommandHandler<CriarScCotacaoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarScCotacaoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarScCotacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var cotacao = new ScCotacao(request.DataCotacao, request.Descricao, request.Situacao, tenantId, usuario);
            if (!cotacao.IsValid)
                return CommandResult.Falha(cotacao.Notifications.Select(n => n.Message), "Dados da cotação são inválidos.");

            _context.ScCotacoes.Add(cotacao);

            if (request.Fornecedores != null)
            {
                foreach (var f in request.Fornecedores)
                {
                    var fornecedor = new ScCotacaoFornecedor(cotacao.Id, f.FornecedorId, f.PrazoEntrega, f.CondicoesPagamento, f.Subtotal, f.Desconto, f.Total, tenantId, usuario);
                    if (!fornecedor.IsValid)
                        return CommandResult.Falha(fornecedor.Notifications.Select(n => n.Message), "Fornecedor de cotação inválido.");
                    _context.ScCotacaoFornecedores.Add(fornecedor);
                }
            }

            if (request.Itens != null)
            {
                foreach (var i in request.Itens)
                {
                    var item = new ScCotacaoItem(cotacao.Id, i.CotacaoFornecedorRef, i.ProdutoId, i.Quantidade, i.ValorUnitario, i.ValorDesconto, i.ValorTotal, tenantId, usuario);
                    if (!item.IsValid)
                        return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item de cotação inválido.");
                    _context.ScCotacaoItens.Add(item);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Cotação criada com sucesso!", new { cotacao.Id });
        }
    }

    public class CriarScPedidoCompraCommandHandler : ICommandHandler<CriarScPedidoCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarScPedidoCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarScPedidoCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // SC-060: pedido possui itens vinculados.
            if (request.Itens == null || request.Itens.Count == 0)
                return CommandResult.Falha("O pedido de compra deve possuir ao menos um item.");

            var pedido = new ScPedidoCompra(
                request.TipoPedidoId, request.FornecedorId, request.CotacaoId,
                request.DataPedido, request.DataPrevistaEntrega, request.DataPrevisaoPagamento,
                request.LocalEntrega, request.LocalCobranca, request.Contato, request.FormaPagamento,
                request.QuantidadeParcelas, request.DiasPrimeiroVencimento, request.DiasIntervalo,
                tenantId, usuario);
            if (!pedido.IsValid)
                return CommandResult.Falha(pedido.Notifications.Select(n => n.Message), "Dados do pedido são inválidos.");

            _context.ScPedidosCompra.Add(pedido);

            foreach (var input in request.Itens)
            {
                var item = new ScPedidoCompraItem(
                    pedido.Id, input.ProdutoId, input.Quantidade, input.ValorUnitario,
                    input.ValorDesconto, input.ValorFrete, input.ValorSeguro, input.ValorOutrasDespesas,
                    input.ValorIpi, input.ValorIcms, input.ValorTotal, tenantId, usuario);
                if (!item.IsValid)
                    return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item do pedido inválido.");
                _context.ScPedidoCompraItens.Add(item);
            }

            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "est.sc.pedido_compra_criado",
                JsonSerializer.Serialize(new { pedido.Id, pedido.FornecedorId, pedido.CotacaoId, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Pedido de compra criado com sucesso!", new { pedido.Id });
        }
    }
}
