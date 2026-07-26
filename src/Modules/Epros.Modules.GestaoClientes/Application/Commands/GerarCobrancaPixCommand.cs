using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    /// <summary>
    /// Gera (ou regenera) a cobrança PIX de uma fatura no gateway ativo, persistindo o
    /// PagamentoFatura com PaymentId + QR + ticket. Resolução da config: por tenant, senão global.
    /// </summary>
    public record GerarCobrancaPixCommand(Guid FaturaId) : ICommand;
}
