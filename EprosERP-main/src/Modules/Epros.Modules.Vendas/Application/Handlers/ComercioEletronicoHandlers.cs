using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Handlers
{
    public class SalvarEcoConfiguracaoLojaCommandHandler : ICommandHandler<SalvarEcoConfiguracaoLojaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public SalvarEcoConfiguracaoLojaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SalvarEcoConfiguracaoLojaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var config = new EcoConfiguracaoLoja(
                request.Nome, request.Rua, request.Numero, request.Bairro, request.Cidade, request.Uf, request.Cep,
                request.Telefone, request.Email, null, null, null, request.FreteGratisValor, request.PagamentoPublicKey,
                request.PagamentoAccessToken, null, null, null, null, null, null, null, request.TokenLoja ?? string.Empty,
                null, null, null, null, null, null, tenantId, usuario);
            if (!config.IsValid) return CommandResult.Falha(config.Notifications.Select(n => n.Message), "Dados da configuração inválidos.");
            _context.EcoConfiguracoesLoja.Add(config);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Configuração da loja salva.", new { config.Id });
        }
    }

    public class CriarEcoClienteCommandHandler : ICommandHandler<CriarEcoClienteCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarEcoClienteCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarEcoClienteCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            // §8.2 (UK email por tenant).
            var duplicado = await _context.EcoClientes.AnyAsync(c => c.TenantId == tenantId && c.Email == request.Email, cancellationToken);
            if (duplicado) return CommandResult.Falha("Já existe um cliente com este e-mail.");
            var cliente = new EcoCliente(request.Nome, request.Sobrenome, request.Cpf, request.InscricaoEstadual, request.Email, request.Telefone, request.SenhaHash, tenantId, usuario);
            if (!cliente.IsValid) return CommandResult.Falha(cliente.Notifications.Select(n => n.Message), "Dados do cliente inválidos.");
            _context.EcoClientes.Add(cliente);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Cliente criado.", new { cliente.Id });
        }
    }

    public class CriarEcoCupomCommandHandler : ICommandHandler<CriarEcoCupomCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarEcoCupomCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarEcoCupomCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var cupom = new EcoCupom(request.Codigo, request.Valor, request.Tipo, tenantId, usuario);
            if (!cupom.IsValid) return CommandResult.Falha(cupom.Notifications.Select(n => n.Message), "Dados do cupom inválidos.");
            _context.EcoCupons.Add(cupom);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Cupom criado.", new { cupom.Id });
        }
    }

    public class CriarEcoPedidoCommandHandler : ICommandHandler<CriarEcoPedidoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarEcoPedidoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarEcoPedidoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // ECO-039: cliente inativo não deve operar checkout.
            var cliente = await _context.EcoClientes.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == request.ClienteId, cancellationToken);
            if (cliente == null) return CommandResult.Falha("Cliente não encontrado.");
            if (!cliente.Status) return CommandResult.Falha("Cliente inativo não pode operar o checkout.");
            if (request.Itens == null || request.Itens.Count == 0) return CommandResult.Falha("O pedido deve possuir itens.");

            decimal valorItens = request.Itens.Sum(i => i.Quantidade * i.ValorUnitario);
            decimal desconto = 0m;

            // ECO-027..ECO-031: valida o cupom quando informado.
            if (!string.IsNullOrWhiteSpace(request.CupomDesconto))
            {
                var cupom = await _context.EcoCupons.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Codigo == request.CupomDesconto, cancellationToken);
                if (cupom == null) return CommandResult.Falha("Cupom inexistente."); // ECO-027
                if (!cupom.Status) return CommandResult.Falha("Cupom inativo.");
                // ECO-028: cupom já usado pelo mesmo cliente no mesmo código.
                var jaUsado = await _context.EcoPedidos.AnyAsync(p => p.TenantId == tenantId && p.ClienteId == request.ClienteId && p.CupomDesconto == request.CupomDesconto, cancellationToken);
                if (jaUsado) return CommandResult.Falha("Cupom já utilizado por este cliente.");
                desconto = cupom.CalcularDesconto(valorItens); // ECO-029/ECO-030
            }

            var pedido = new EcoPedido(request.ClienteId, request.EnderecoId, valorItens, request.ValorFrete, desconto, request.TipoFrete, request.CupomDesconto, request.Observacao, tenantId, usuario);
            if (!pedido.IsValid) return CommandResult.Falha(pedido.Notifications.Select(n => n.Message), "Dados do pedido inválidos.");
            _context.EcoPedidos.Add(pedido);

            foreach (var i in request.Itens)
            {
                var item = new EcoPedidoItem(pedido.Id, i.ProdutoId, i.Quantidade, i.VariacaoId, i.ValorUnitario, tenantId, usuario);
                if (!item.IsValid) return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item de pedido inválido.");
                _context.EcoPedidoItens.Add(item);
            }

            _context.EcoHistoricos.Add(new EcoHistorico("Pedido", pedido.Id, "Criacao", null, null, null, request.ClienteId, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Pedido criado.", new { pedido.Id, pedido.ValorTotal, StatusPagamento = pedido.StatusPagamentoCodigo.ToString() });
        }
    }

    public class IniciarPagamentoPedidoCommandHandler : ICommandHandler<IniciarPagamentoPedidoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public IniciarPagamentoPedidoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(IniciarPagamentoPedidoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var pedido = await _context.EcoPedidos.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.PedidoId, cancellationToken);
            if (pedido == null) return CommandResult.Falha("Pedido não encontrado.");

            switch (request.Forma)
            {
                case EEcoFormaPagamento.Cartao:
                    pedido.IniciarPagamentoCartao(request.TransacaoId, usuario); break; // ECO-021
                case EEcoFormaPagamento.Pix:
                    pedido.IniciarPagamentoPix(request.TransacaoId, request.QrCode, request.QrCodeBase64, usuario); break; // ECO-022
                case EEcoFormaPagamento.Boleto:
                    pedido.IniciarPagamentoBoleto(request.TransacaoId, request.LinkBoleto, usuario); break; // ECO-023
            }
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Pagamento iniciado.", new { pedido.Id, StatusPagamento = pedido.StatusPagamentoCodigo.ToString() });
        }
    }

    public class ConfirmarPagamentoPedidoCommandHandler : ICommandHandler<ConfirmarPagamentoPedidoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConfirmarPagamentoPedidoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConfirmarPagamentoPedidoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var pedido = await _context.EcoPedidos.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.PedidoId, cancellationToken);
            if (pedido == null) return CommandResult.Falha("Pedido não encontrado.");
            // ECO-024: só confirma com confirmação do provedor.
            pedido.ConfirmarPagamento(request.StatusPagamento, request.StatusDetalhe, usuario);
            _context.EcoHistoricos.Add(new EcoHistorico("Pagamento", pedido.Id, "Confirmado", null, null, null, pedido.ClienteId, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Pagamento confirmado.", new { pedido.Id, StatusPagamento = pedido.StatusPagamentoCodigo.ToString() });
        }
    }

    public class ConverterEcoPedidoEmVendaCommandHandler : ICommandHandler<ConverterEcoPedidoEmVendaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConverterEcoPedidoEmVendaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConverterEcoPedidoEmVendaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var pedido = await _context.EcoPedidos.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.PedidoId, cancellationToken);
            if (pedido == null) return CommandResult.Falha("Pedido não encontrado.");
            if (pedido.StatusPagamentoCodigo != EEcoStatusPagamento.PagamentoConfirmado)
                return CommandResult.Falha("Só é possível converter pedido com pagamento confirmado.");
            if (pedido.VendaId.HasValue) return CommandResult.Falha("Pedido já convertido em venda.");

            // ECO-035/ECO-037: a Venda/emissão fiscal (NF-e) é gerada no fluxo de Venda + módulo Fiscal.
            // Aqui publicamos o evento de integração (Outbox) preservando o vínculo do pedido e-commerce.
            // [FATO fiscal: e-commerce = venda não presencial → indPres=2 (internet). MOC 7.0 Anexo I B25b.]
            var payload = JsonSerializer.Serialize(new { PedidoId = pedido.Id, pedido.ClienteId, pedido.ValorTotal, FormaPagamento = pedido.FormaPagamento?.ToString() });
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "PedidoEcommerceParaVenda", payload));
            _context.EcoHistoricos.Add(new EcoHistorico("Venda", pedido.Id, "SolicitacaoConversao", null, payload, null, pedido.ClienteId, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Conversão em venda solicitada.", new { pedido.Id });
        }
    }

    public class AtualizarRastreioPedidoCommandHandler : ICommandHandler<AtualizarRastreioPedidoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarRastreioPedidoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarRastreioPedidoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var pedido = await _context.EcoPedidos.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.PedidoId, cancellationToken);
            if (pedido == null) return CommandResult.Falha("Pedido não encontrado.");
            pedido.AtualizarRastreio(request.CodigoRastreio, usuario); // ECO-041
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Rastreio atualizado.", new { pedido.Id, pedido.CodigoRastreio });
        }
    }
}
