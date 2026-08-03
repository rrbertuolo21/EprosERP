using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    /// <summary>
    /// 1.08B — Gera (ou regenera) o BOLETO de uma fatura no gateway ativo, persistindo o PagamentoFatura
    /// (payment id + linha digitável + código de barras + URL do PDF). A conciliação do pagamento acontece
    /// pelo MESMO webhook unificado do PIX (o MP concilia boleto por webhook).
    /// </summary>
    public record GerarBoletoCommand(Guid FaturaId) : ICommand;
}
