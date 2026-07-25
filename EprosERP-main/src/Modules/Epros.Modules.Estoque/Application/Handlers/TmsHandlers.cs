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
    /// Handlers do submódulo Transporte e Frete TMS (EST-TMS). Montam o agregado CompraTransporte e
    /// aplicam TMS-001 (ao menos transportadora, veículo ou volume). Auditoria via HistoricoEstoque
    /// (TMS-023) e evento `est.tms.001.alterado` via Outbox (§12).
    /// </summary>
    public class CriarTmsTransporteCompraCommandHandler : ICommandHandler<CriarTmsTransporteCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarTmsTransporteCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarTmsTransporteCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (request.CompraId == Guid.Empty)
                return CommandResult.Falha("A compra é obrigatória para o transporte (TMS-020).");

            var transporte = new CompraTransporte(request.CompraId, tenantId, usuario);

            if (request.Transportadora != null)
            {
                var t = request.Transportadora;
                transporte.IncluirTransportadora(t.PessoaId, t.Cnpj, t.Cpf, t.RazaoSocial, t.InscricaoEstadual, t.EnderecoCompleto, t.NomeMunicipio, t.Uf, usuario);
            }

            if (request.Veiculo != null)
            {
                var v = request.Veiculo;
                transporte.IncluirVeiculo(v.VeiculoId, v.Placa, v.Uf, v.Rntrc, usuario);
            }

            if (request.Reboques != null)
                foreach (var r in request.Reboques)
                    transporte.IncluirReboque(r.VeiculoId, r.Placa, r.Uf, r.Rntrc, usuario);

            if (request.Volumes != null)
                foreach (var vol in request.Volumes)
                    transporte.IncluirVolume(vol.QuantidadeVolumes, vol.Especie, vol.NumeroVolumes, vol.PesoLiquido, vol.PesoBruto, vol.Marca, usuario);

            // TMS-001 e limites de campos (validação intra-agregado).
            transporte.Validar();
            if (!transporte.IsValid)
                return CommandResult.Falha(transporte.Notifications.Select(n => n.Message), "Dados de transporte da compra são inválidos.");

            _context.CompraTransportes.Add(transporte);

            _context.HistoricosEstoque.Add(new HistoricoEstoque("CompraTransporte", transporte.Id, "transporte_criado", null, "Rascunho", null, usuario, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "est.tms.001.alterado",
                JsonSerializer.Serialize(new { transporte.Id, transporte.CompraId, evento = "criado", usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Transporte da compra registrado com sucesso!", new { transporte.Id });
        }
    }

    public class ConfirmarTmsTransporteCompraCommandHandler : ICommandHandler<ConfirmarTmsTransporteCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConfirmarTmsTransporteCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConfirmarTmsTransporteCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var transporte = await _context.CompraTransportes.FirstOrDefaultAsync(t => t.Id == request.Id && t.DeletadoEm == null, cancellationToken);
            if (transporte == null)
                return CommandResult.Falha("Transporte da compra não encontrado.");

            var anterior = transporte.Situacao.ToString();
            transporte.Confirmar(usuario);

            _context.HistoricosEstoque.Add(new HistoricoEstoque("CompraTransporte", transporte.Id, "transporte_confirmado", anterior, "Confirmado", null, usuario, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "est.tms.001.alterado",
                JsonSerializer.Serialize(new { transporte.Id, evento = "confirmado", usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Transporte da compra confirmado com sucesso!");
        }
    }

    public class CancelarTmsTransporteCompraCommandHandler : ICommandHandler<CancelarTmsTransporteCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CancelarTmsTransporteCompraCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CancelarTmsTransporteCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var transporte = await _context.CompraTransportes.FirstOrDefaultAsync(t => t.Id == request.Id && t.DeletadoEm == null, cancellationToken);
            if (transporte == null)
                return CommandResult.Falha("Transporte da compra não encontrado.");

            var anterior = transporte.Situacao.ToString();
            transporte.Cancelar(usuario);

            _context.HistoricosEstoque.Add(new HistoricoEstoque("CompraTransporte", transporte.Id, "transporte_cancelado", anterior, "Cancelado", null, usuario, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "est.tms.001.alterado",
                JsonSerializer.Serialize(new { transporte.Id, evento = "cancelado", usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Transporte da compra cancelado com sucesso!");
        }
    }
}
