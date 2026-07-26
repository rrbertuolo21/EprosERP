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
    /// Handlers do submódulo Gestão de Contratos de Compra (EST-GCC). Gravam em transação única, respeitam
    /// tenant (filtro global do ContextBase) e publicam eventos via Outbox (§13). GCC-008: consumo recalcula
    /// saldo no item. GCC-009: aprovação/aditivo com impacto financeiro integram workflow externo (pendência).
    /// </summary>
    public class CriarGccContratoCompraCommandHandler : ICommandHandler<CriarGccContratoCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarGccContratoCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarGccContratoCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var contrato = new GccContratoCompra(request.FornecedorId, request.NumeroContrato, request.VigenciaInicio, request.VigenciaFim, request.ValorTotal, request.Observacao, tenantId, usuario);
            if (!contrato.IsValid)
                return CommandResult.Falha(contrato.Notifications.Select(n => n.Message), "Dados do contrato são inválidos.");

            _context.GccContratosCompra.Add(contrato);

            if (request.Itens != null)
            {
                foreach (var input in request.Itens)
                {
                    var item = new GccContratoCompraItem(contrato.Id, input.ProdutoId, input.PrecoUnitario, input.QuantidadeComprometida, tenantId, usuario);
                    if (!item.IsValid)
                        return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item de contrato inválido.");
                    _context.GccContratosCompraItens.Add(item);
                }
            }

            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "estoque.gcc.contrato_criado",
                JsonSerializer.Serialize(new { contrato.Id, contrato.FornecedorId, contrato.VigenciaInicio, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Contrato de compra criado com sucesso!", new { contrato.Id });
        }
    }

    public class AtualizarGccContratoCompraCommandHandler : ICommandHandler<AtualizarGccContratoCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarGccContratoCompraCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarGccContratoCompraCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var contrato = await _context.GccContratosCompra.FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletadoEm == null, cancellationToken);
            if (contrato == null)
                return CommandResult.Falha("Contrato de compra não encontrado.");

            contrato.Alterar(request.FornecedorId, request.NumeroContrato, request.VigenciaInicio, request.VigenciaFim, request.ValorTotal, request.Observacao, usuario);
            if (!contrato.IsValid)
                return CommandResult.Falha(contrato.Notifications.Select(n => n.Message), "Dados do contrato são inválidos.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Contrato de compra atualizado com sucesso!");
        }
    }

    public class EnviarGccContratoParaAprovacaoCommandHandler : ICommandHandler<EnviarGccContratoParaAprovacaoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public EnviarGccContratoParaAprovacaoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(EnviarGccContratoParaAprovacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var contrato = await _context.GccContratosCompra.FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletadoEm == null, cancellationToken);
            if (contrato == null)
                return CommandResult.Falha("Contrato de compra não encontrado.");

            contrato.EnviarParaAprovacao(usuario);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "estoque.gcc.contrato_enviado_aprovacao",
                JsonSerializer.Serialize(new { contrato.Id, contrato.ValorTotal, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Contrato enviado para aprovação!");
        }
    }

    public class AprovarGccContratoCompraCommandHandler : ICommandHandler<AprovarGccContratoCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AprovarGccContratoCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AprovarGccContratoCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var contrato = await _context.GccContratosCompra.FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletadoEm == null, cancellationToken);
            if (contrato == null)
                return CommandResult.Falha("Contrato de compra não encontrado.");

            contrato.Aprovar(usuario);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "estoque.gcc.contrato_aprovado",
                JsonSerializer.Serialize(new { contrato.Id, contrato.FornecedorId, contrato.VigenciaInicio, contrato.ValorTotal })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Contrato aprovado com sucesso!");
        }
    }

    public class RegistrarGccConsumoContratoCommandHandler : ICommandHandler<RegistrarGccConsumoContratoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarGccConsumoContratoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarGccConsumoContratoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var contrato = await _context.GccContratosCompra.FirstOrDefaultAsync(c => c.Id == request.ContratoCompraId && c.DeletadoEm == null, cancellationToken);
            if (contrato == null)
                return CommandResult.Falha("Contrato de compra não encontrado.");

            // Regra §18: apenas contrato aprovado e vigente pode ser consumido.
            if (!contrato.PodeConsumir())
                return CommandResult.Falha("Apenas contrato aprovado pode ser consumido [GCC-007].");

            var item = await _context.GccContratosCompraItens.FirstOrDefaultAsync(i => i.Id == request.ContratoCompraItemId && i.ContratoCompraId == request.ContratoCompraId && i.DeletadoEm == null, cancellationToken);
            if (item == null)
                return CommandResult.Falha("Item contratual não encontrado.");

            // CA-006: consumo não pode exceder saldo (regra de tolerância pendente — ver pendências).
            if (request.QuantidadeConsumida > item.SaldoQuantidade)
                return CommandResult.Falha("Consumo excede o saldo contratual disponível [CA-006].");

            item.RegistrarConsumo(request.QuantidadeConsumida, request.ValorConsumido, usuario);

            var consumo = new GccConsumoContrato(request.ContratoCompraId, request.ContratoCompraItemId, request.CompraId, request.QuantidadeConsumida, request.ValorConsumido, tenantId, usuario);
            if (!consumo.IsValid)
                return CommandResult.Falha(consumo.Notifications.Select(n => n.Message), "Dados de consumo inválidos.");

            _context.GccConsumosContrato.Add(consumo);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "estoque.gcc.consumo_registrado",
                JsonSerializer.Serialize(new { contratoId = contrato.Id, itemId = item.Id, request.CompraId, request.QuantidadeConsumida, request.ValorConsumido })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Consumo contratual registrado com sucesso!", new { consumo.Id, saldoQuantidade = item.SaldoQuantidade, saldoValor = item.SaldoValor });
        }
    }
}
