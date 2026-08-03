using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Interfaces;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Shared.Application.Models;
using Microsoft.Extensions.Logging;

namespace Epros.Modules.GestaoClientes.Infrastructure.Gateways
{
    /// <summary>
    /// 1.08A — Implementação no-op de <see cref="ICobrancaRecorrenteGateway"/>. Cartão-on-file é
    /// deferido para a PASSADA B; aqui apenas registramos a intenção e devolvemos "não suportado" para
    /// que o motor recaia no PIX. NÃO cobra nada e nunca reporta cartão disponível.
    /// </summary>
    public class CobrancaRecorrenteGatewayNoop : ICobrancaRecorrenteGateway
    {
        private readonly ILogger<CobrancaRecorrenteGatewayNoop>? _logger;

        public CobrancaRecorrenteGatewayNoop(ILogger<CobrancaRecorrenteGatewayNoop>? logger = null)
        {
            _logger = logger;
        }

        public Task<bool> PossuiCartaoOnFileAsync(Guid clienteId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<CommandResult> CobrarCartaoRecorrenteAsync(Fatura fatura, Guid clienteId, CancellationToken cancellationToken = default)
        {
            _logger?.LogInformation(
                "[CobrancaRecorrente] Cartão-on-file não implementado (passada A). Fatura {FaturaId}, cliente {ClienteId} seguirá pelo caminho PIX.",
                fatura.Id, clienteId);
            return Task.FromResult(CommandResult.Falha(
                "Cobrança por cartão-on-file não disponível nesta versão (deferido para a passada B).",
                "Cartão-on-file indisponível"));
        }
    }
}
