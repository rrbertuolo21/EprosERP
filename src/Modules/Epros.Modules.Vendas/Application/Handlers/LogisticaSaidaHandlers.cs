using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Handlers
{
    public class CriarExpedicaoCommandHandler : ICommandHandler<CriarExpedicaoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarExpedicaoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarExpedicaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var expedicao = new Expedicao(request.EmpresaId, request.PedidoId, request.DocumentoFiscalId, request.RomaneioId, request.DataExpedicao, request.Observacoes, null, tenantId, usuario);
            if (!expedicao.IsValid) return CommandResult.Falha(expedicao.Notifications.Select(n => n.Message), "Dados da expedição inválidos.");
            _context.Expedicoes.Add(expedicao);
            _context.ExpedicaoHistoricos.Add(new ExpedicaoHistorico(expedicao.Id, System.Guid.Empty, "Criacao", null, expedicao.Status.ToString(), null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Expedição criada com sucesso!", new { expedicao.Id, Status = expedicao.Status.ToString() });
        }
    }

    public class DefinirLocalEntregaCommandHandler : ICommandHandler<DefinirLocalEntregaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DefinirLocalEntregaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DefinirLocalEntregaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var expedicao = await _context.Expedicoes.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == request.ExpedicaoId, cancellationToken);
            if (expedicao == null) return CommandResult.Falha("Expedição não encontrada.");
            var local = new ExpedicaoLocalEntrega(request.ExpedicaoId, expedicao.DocumentoFiscalId, request.CpfCnpj, request.Logradouro, request.Numero, request.Complemento, request.Bairro, request.CodigoMunicipio, request.NomeMunicipio, request.Uf, tenantId, usuario);
            if (!local.IsValid) return CommandResult.Falha(local.Notifications.Select(n => n.Message), "Dados do local de entrega inválidos.");
            _context.ExpedicaoLocaisEntrega.Add(local);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Local de entrega definido.", new { local.Id });
        }
    }

    public class RegistrarEntregaItemCommandHandler : ICommandHandler<RegistrarEntregaItemCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarEntregaItemCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarEntregaItemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var expedicao = await _context.Expedicoes.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == request.ExpedicaoId, cancellationToken);
            if (expedicao == null) return CommandResult.Falha("Expedição não encontrada.");
            var item = new ExpedicaoItemEntrega(request.ExpedicaoId, request.PedidoItemId, request.ProdutoId, request.QuantidadeVendida, request.QuantidadeEntregue, request.UsuarioEntregaId, tenantId, usuario);
            if (!item.IsValid) return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Quantidade entregue inválida.");
            _context.ExpedicaoItensEntrega.Add(item);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Entrega registrada.", new { item.Id, item.SaldoEntrega });
        }
    }

    public class ConfirmarExpedicaoCommandHandler : ICommandHandler<ConfirmarExpedicaoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConfirmarExpedicaoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConfirmarExpedicaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var expedicao = await _context.Expedicoes.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == request.ExpedicaoId, cancellationToken);
            if (expedicao == null) return CommandResult.Falha("Expedição não encontrada.");
            var anterior = expedicao.Status.ToString();
            expedicao.Confirmar(usuario);
            if (!expedicao.IsValid) return CommandResult.Falha(expedicao.Notifications.Select(n => n.Message), "Não foi possível confirmar a expedição.");
            _context.ExpedicaoHistoricos.Add(new ExpedicaoHistorico(expedicao.Id, System.Guid.Empty, "Confirmacao", anterior, expedicao.Status.ToString(), null, null, tenantId, usuario));

            // NF-04 / T-04: a Logística NÃO escreve saldo direto. Ao confirmar a expedição, publica o
            // evento de saída via Outbox; o MOTOR ÚNICO do Estoque consome e faz a baixa (converte a
            // reserva em saída), idempotente por expedicao+item. Aqui só o registro documental + evento.
            var itensEntrega = await _context.ExpedicaoItensEntrega
                .Where(i => i.TenantId == tenantId && i.ExpedicaoId == expedicao.Id)
                .Select(i => new { i.Id, i.ProdutoId, i.QuantidadeEntregue })
                .ToListAsync(cancellationToken);
            var payload = JsonSerializer.Serialize(new
            {
                ExpedicaoId = expedicao.Id,
                expedicao.PedidoId,
                expedicao.DocumentoFiscalId,
                Itens = itensEntrega
            });
            // Convenção de eventos do módulo (ven.*), idempotência por expedicao+item no consumidor (motor único).
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "ven.ExpedicaoConfirmada", payload));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Expedição confirmada.", new { expedicao.Id, Status = expedicao.Status.ToString() });
        }
    }

    public class FaturarExpedicaoCommandHandler : ICommandHandler<FaturarExpedicaoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public FaturarExpedicaoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(FaturarExpedicaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var expedicao = await _context.Expedicoes.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == request.ExpedicaoId, cancellationToken);
            if (expedicao == null) return CommandResult.Falha("Expedição não encontrada.");
            var anterior = expedicao.Status.ToString();
            // FISCAL: emissão do documento é do módulo Fiscal (IHerculesFiscalService); aqui só o vínculo.
            expedicao.Faturar(request.DocumentoFiscalId, usuario);
            _context.ExpedicaoHistoricos.Add(new ExpedicaoHistorico(expedicao.Id, System.Guid.Empty, "Faturamento", anterior, expedicao.Status.ToString(), null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Expedição faturada.", new { expedicao.Id, Status = expedicao.Status.ToString() });
        }
    }

    public class CancelarExpedicaoCommandHandler : ICommandHandler<CancelarExpedicaoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CancelarExpedicaoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CancelarExpedicaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var expedicao = await _context.Expedicoes.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == request.ExpedicaoId, cancellationToken);
            if (expedicao == null) return CommandResult.Falha("Expedição não encontrada.");
            var anterior = expedicao.Status.ToString();
            expedicao.Cancelar(usuario);
            _context.ExpedicaoHistoricos.Add(new ExpedicaoHistorico(expedicao.Id, System.Guid.Empty, "Cancelamento", anterior, expedicao.Status.ToString(), null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Expedição cancelada.", new { expedicao.Id, Status = expedicao.Status.ToString() });
        }
    }
}
