using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Shared.Application.Models;

namespace Epros.Modules.GestaoClientes.Application.Interfaces
{
    /// <summary>
    /// 1.08A — Ponto de extensão (método-agnóstico) para cobrança recorrente com cartão-on-file.
    ///
    /// Na PASSADA A o cartão-on-file NÃO existe (não há tokenização/armazenamento de cartão): a
    /// implementação padrão (<see cref="CobrancaRecorrenteGatewayNoop"/>) é um no-op que apenas loga e
    /// devolve "não suportado", de modo que o motor de fim-de-trial recai no caminho PIX (real, que agora
    /// concilia). Cartão/boleto entram na PASSADA B, quando esta interface passa a ter uma implementação
    /// concreta (Mercado Pago Customers/Cards ou similar). Mantém o motor de cobrança agnóstico ao meio.
    /// </summary>
    public interface ICobrancaRecorrenteGateway
    {
        /// <summary>
        /// Indica se há um instrumento de cartão-on-file disponível para o cliente (sempre false na passada A).
        /// </summary>
        Task<bool> PossuiCartaoOnFileAsync(Guid clienteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cobra recorrentemente a fatura no cartão-on-file do cliente. Passada A: no-op (deferido p/ B).
        /// </summary>
        Task<CommandResult> CobrarCartaoRecorrenteAsync(Fatura fatura, Guid clienteId, CancellationToken cancellationToken = default);
    }
}
